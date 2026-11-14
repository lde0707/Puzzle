using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Puzzle.Gravity
{
    public static class ProgressTransfer
    {
        public static string Encode(ProgressData data)
        {
            string payload=JsonUtility.ToJson(data);
            return JsonUtility.ToJson(new SaveEnvelope{sequence=1,payload=payload,checksum=ProgressSaveService.Checksum(payload)});
        }
        public static bool Decode(string raw,PuzzleDefinition[] levels,out ProgressData data)
        {
            data=null;
            try
            {
                if(string.IsNullOrEmpty(raw)||raw.Length>1048576)return false;
                var e=JsonUtility.FromJson<SaveEnvelope>(raw);
                if(e==null||e.version!=1||e.payload==null||e.checksum!=ProgressSaveService.Checksum(e.payload))return false;
                var p=JsonUtility.FromJson<ProgressData>(e.payload);
                if(p==null||!levels.Any(l=>l.room==p.levelId)||string.IsNullOrEmpty(p.dataVersion))return false;
                var ids=levels.Select(l=>l.room).ToArray();
                if((p.completed?.Length??0)>ids.Length||(p.completed??Array.Empty<string>()).Any(id=>!ids.Contains(id)))return false;
                if(p.collection!=null)
                {
                    var c=p.collection;
                    if(c.wallet<0||c.wallet>ids.Length*3||(c.owned?.Length??0)>PuzzleCollection.Names.Length||(c.records?.Length??0)>ids.Length)return false;
                    if((c.owned??Array.Empty<int>()).Any(i=>i<0||i>=PuzzleCollection.Names.Length))return false;
                    if((c.records??Array.Empty<StageStars>()).Any(r=>r==null||!ids.Contains(r.id)||r.stars<1||r.stars>3||r.bestMoves<0||r.bestMoves>100000))return false;
                }
                if((p.activity?.stages?.Length??0)>ids.Length)return false;
                foreach(var s in p.activity?.stages??Array.Empty<StageActivity>())
                    if(s==null||!ids.Contains(s.room)||new[]{s.moves,s.restarts,s.undos,s.hints,s.visits,s.clears}.Any(n=>n<0||n>10000000))return false;
                if((p.history?.Length??0)>10000)return false;
                // Invalid or changed board state is safely restarted; collection still transfers.
                var level=levels.First(l=>l.room==p.levelId);var session=new PuzzleSession(level);
                try{if(!ProgressSaveService.Restore(p,ProgressSaveService.LevelVersion(level,"f0dda662"),session)){p.current=null;p.history=null;}}
                catch{p.current=null;p.history=null;}
                data=p;return true;
            }
            catch{return false;}
        }
        public static ProgressData Merge(ProgressData current,ProgressData incoming,string[] valid)
        {
            var a=JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(current));
            var b=JsonUtility.FromJson<ProgressData>(JsonUtility.ToJson(incoming));
            var ac=PuzzleCollection.Restore(a.collection,a.completed,valid);
            var bc=PuzzleCollection.Restore(b.collection,b.completed,valid);
            int sharedStars=ac.records.Sum(r=>Math.Min(r.stars,bc.Stars(r.id)));
            int sharedSkins=ac.owned.Intersect(bc.owned).Count(i=>i!=0);
            // Count shared rewards and shared purchases once, so reimporting cannot refill spent stars.
            int wallet=Math.Max(0,ac.wallet+bc.wallet-sharedStars+sharedSkins*PuzzleCollection.DrawCost);
            bc.records=ac.records.Concat(bc.records).ToArray();bc.owned=ac.owned.Concat(bc.owned).ToArray();
            b.completed=(a.completed??Array.Empty<string>()).Union(b.completed??Array.Empty<string>()).ToArray();
            b.collection=PuzzleCollection.Restore(bc,b.completed,valid);b.collection.wallet=Math.Min(valid.Distinct().Count()*3,wallet);
            b.seenLessons=(a.seenLessons??Array.Empty<string>()).Union(b.seenLessons??Array.Empty<string>()).ToArray();
            b.portalHelpSeen|=a.portalHelpSeen;
            var merged=new PuzzleActivity();
            foreach(var s in (a.activity?.stages??Array.Empty<StageActivity>()).Concat(b.activity?.stages??Array.Empty<StageActivity>()))
            {
                var m=merged.For(s.room);m.moves=Math.Max(m.moves,s.moves);m.restarts=Math.Max(m.restarts,s.restarts);
                m.undos=Math.Max(m.undos,s.undos);m.hints=Math.Max(m.hints,s.hints);m.visits=Math.Max(m.visits,s.visits);m.clears=Math.Max(m.clears,s.clears);
            }
            b.activity=merged;
            b.hints=PuzzleHints.Restore(new PuzzleHints{stages=(a.hints?.stages??Array.Empty<StageHints>()).Concat(b.hints?.stages??Array.Empty<StageHints>()).ToArray()});
            // Imports may add levels/skins, but must not refill the account's consumable hints.
            b.hintWallet=HintWallet.Restore(a.hintWallet,HintWallet.Now);
            return b;
        }
    }
    public sealed partial class GravityPuzzleApp
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void MallangBackupOpen(string target,string json,string previous);
        [DllImport("__Internal")] private static extern void MallangBackupResult(string text);
#endif
        private void OpenBackup()
        {
            if(!Save())return;
            var data=saves.Load();string previous="";
            try{if(CrazyPortal.Enabled)previous=CrazyPortal.Read("degul.before-import.v1")??"";else{string path=Path.Combine(ProgressSaveService.DefaultDirectory,"before-import.json");if(File.Exists(path))previous=File.ReadAllText(path);}}catch{}
#if UNITY_WEBGL && !UNITY_EDITOR
            MallangBackupOpen(gameObject.name,ProgressTransfer.Encode(data),previous);
#else
            string exportPath=Path.Combine(ProgressSaveService.DefaultDirectory,"mallang-backup.json");
            File.WriteAllText(exportPath,ProgressTransfer.Encode(data));
            var card=OpenDialog("home-settings",270);DialogText(card,"저장 파일을 만들었어요",195,40,23);
            DialogText(card,exportPath,85,95,12,PuzzleUI.Muted);DialogButton(card,"돌아가기",24,OpenHomeSettings);LayoutDialog();
#endif
        }
        public void ImportProgressBackup(string raw)
        {
            if(modal!="home-settings")return;
            string result="저장 파일을 확인해 주세요. 현재 기록은 바뀌지 않았어요.";
            if(ProgressTransfer.Decode(raw,levels,out var incoming)&&Save())
            {
                var old=saves.Load();var merged=ProgressTransfer.Merge(old,incoming,levels.Select(l=>l.room).ToArray());
                try
                {
                    if(CrazyPortal.Enabled)CrazyPortal.Write("degul.before-import.v1",ProgressTransfer.Encode(old));
                    else File.WriteAllText(Path.Combine(ProgressSaveService.DefaultDirectory,"before-import.json"),ProgressTransfer.Encode(old));
                    if(!saves.Save(merged))throw new IOException(saves.LastError);
                    DismissHome();completed.Clear();foreach(var id in merged.completed)completed.Add(id);
                    seenLessons.Clear();foreach(var id in merged.seenLessons)seenLessons.Add(id);
                    collection=merged.collection;hints=merged.hints;activity=merged.activity;portalHelpSeen=merged.portalHelpSeen;
                    hintWallet=merged.hintWallet;
                    reduceMotion=merged.reduceMotion;largeText=merged.largeText;sound=merged.sound;musicMuted=merged.musicMuted;
                    PuzzleUI.ReduceMotion=reduceMotion;board.ApplySkin(collection.selected);
                    LoadLevel(Array.FindIndex(levels,l=>l.room==merged.levelId),merged,false);OpenHome();
                    result="이어받았어요! 단계 기록과 스킨을 합쳤고, 같은 별 보상은 한 번만 계산했어요.";
                }
                catch{result="저장하지 못했어요. 저장 공간을 확인하고 다시 시도해 주세요.";}
            }
#if UNITY_WEBGL && !UNITY_EDITOR
            MallangBackupResult(PuzzleLanguage.Translate(result));
#endif
        }
    }
}
