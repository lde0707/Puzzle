using System;
using System.Collections.Generic;
using System.Linq;

namespace Puzzle.Gravity
{
    [Serializable] public sealed class StageStars
    {
        public string id;
        public int stars, bestMoves;
    }
    [Serializable] public sealed class PuzzleCollection
    {
        public const int DrawCost=3;
        // Append only: saved ownership and equipped skins use these stable indices.
        public static readonly string[] Names={"순정 말랑","새싹 산책","냥냥 궤도","달토끼","포근 곰","작은 왕관","별사탕","우주 탐험","몽실 구름","행운 클로버","선물 리본","리듬 친구","바다 여행","꼬마 마법사","봄날 꽃","두근 하트","꿀벌 소풍","겨울 눈꽃","작은 행성","잘 자요",
            "숲속 여우","대나무 판다","연못 개굴","빙하 펭귄","꼬마 공룡","구름 용","느긋 거북","밤송이 고슴","아홀로틀","문어 산책",
            "버터 토스트","카라멜 푸딩","와플 소풍","딸기 케이크","수박 한입","도넛 파티","오후의 찻잔","찰칵 카메라","체크 탐정","우편 배달부"};
        public int wallet, selected;
        public int[] owned={0};
        public StageStars[] records=new StageStars[0];

        // A missing collection is a pre-stars save. Its historical move count is unknown.
        public static PuzzleCollection Restore(PuzzleCollection saved,IEnumerable<string> completed,IEnumerable<string> validLevels)
        {
            var valid=new HashSet<string>(validLevels);
            if(saved==null)
            {
                var ids=(completed??Array.Empty<string>()).Where(valid.Contains).Distinct().ToArray();
                return new PuzzleCollection{wallet=ids.Length,records=ids.Select(id=>new StageStars{id=id,stars=1}).ToArray()};
            }
            saved.owned=(saved.owned??Array.Empty<int>()).Append(0).Where(i=>i>=0&&i<Names.Length).Distinct().OrderBy(i=>i).ToArray();
            if(!saved.owned.Contains(saved.selected))saved.selected=0;
            saved.wallet=Math.Max(0,saved.wallet);
            saved.records=(saved.records??Array.Empty<StageStars>()).Where(r=>r!=null&&r.id!=null&&valid.Contains(r.id))
                .GroupBy(r=>r.id).Select(g=>new StageStars{id=g.Key,stars=Math.Max(1,Math.Min(3,g.Max(r=>r.stars))),bestMoves=g.Where(r=>r.bestMoves>0).Select(r=>r.bestMoves).DefaultIfEmpty(0).Min()}).ToArray();
            // Unity can materialize a missing inline serializable object as an empty one.
            // Reconcile by stable stage ID, so this also repairs early empty-collection saves.
            foreach(string id in (completed??Array.Empty<string>()).Where(valid.Contains).Distinct())
                if(saved.Stars(id)==0){saved.records=saved.records.Append(new StageStars{id=id,stars=1}).ToArray();saved.wallet++;}
            return saved;
        }
        public static int Rating(int moves,int optimum,bool usedUndo)=>1+(moves==optimum?1:0)+(usedUndo?0:1);
        public int Stars(string id)=>records.FirstOrDefault(r=>r.id==id)?.stars??0;
        public int Award(string id,int moves,int optimum,bool usedUndo)
        {
            int stars=Rating(moves,optimum,usedUndo);
            var record=records.FirstOrDefault(r=>r.id==id);
            if(record==null){record=new StageStars{id=id};records=records.Append(record).ToArray();}
            int added=Math.Max(0,stars-record.stars);
            wallet+=added;record.stars=Math.Max(record.stars,stars);
            if(record.bestMoves==0||moves<record.bestMoves)record.bestMoves=moves;
            return added;
        }
        public bool TryEquip(int skin,Func<bool> persist)
        {
            if(!owned.Contains(skin))return false;
            int previous=selected;selected=skin;bool saved=false;
            try{saved=persist();}finally{if(!saved)selected=previous;}
            return saved;
        }
        public int Remaining=>Names.Length-owned.Length;
        // The picker receives the remaining count, so every unowned skin has equal odds.
        // Persist the spend and unlock together, before starting the cosmetic reveal.
        public bool TryDraw(Func<int,int> pick,Func<bool> persist,out int skin)
        {
            skin=-1;
            if(wallet<DrawCost||Remaining==0)return false;
            var candidates=Enumerable.Range(0,Names.Length).Except(owned).ToArray();
            int index=pick(candidates.Length);
            if(index<0||index>=candidates.Length)return false;
            int chosen=candidates[index];var previousOwned=owned;
            owned=owned.Append(chosen).OrderBy(i=>i).ToArray();wallet-=DrawCost;
            bool saved=false;
            try{saved=persist();}finally{if(!saved){owned=previousOwned;wallet+=DrawCost;}}
            if(!saved)return false;
            skin=chosen;return true;
        }
    }
}
