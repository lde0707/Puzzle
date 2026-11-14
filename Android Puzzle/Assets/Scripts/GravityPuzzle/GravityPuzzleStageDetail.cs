using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp
    {
        private int detailLevelIndex;
        private float levelListPosition=1;
        private PuzzleLevelScroll levelListScroll;
        private Text stageRank,stageRankBasis;
        private Text[] levelRanks=System.Array.Empty<Text>();
        private Text clearRank,clearRankBasis;
        private Font stageDisplayFont;
        private StageTierCatalog stageTiers=new StageTierCatalog();
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void MallangStageTiers(string target);
#endif
        private void OpenStageDetail(int index)
        {
            if(QuickPlay&&CanPlayLevel(index)){session.SetPause(PauseReason.Levels,false);SelectLevel(index);return;}
            if(levelListScroll)levelListPosition=levelListScroll.verticalNormalizedPosition;
            detailLevelIndex=index;
            var level=levels[index];var record=activity.stages.FirstOrDefault(s=>s.room==level.room);
            var card=OpenDialog("stage-detail",600);
            // Keep the artwork's 971:1619 aspect even on narrow phones; LayoutDialog scales the entire card.
            card.sizeDelta=new Vector2(600f*971f/1619f,600);
            if(!stageDisplayFont)stageDisplayFont=Resources.Load<Font>("GravityPuzzle/Jua-Regular");
            card.GetComponent<Image>().color=Color.clear;
            foreach(var effect in card.GetComponents<Shadow>())effect.enabled=false;
            var art=PuzzleUI.Rect("Generated marshmallow stage panel",card).gameObject.AddComponent<RawImage>();
            PuzzleUI.Fill(art.rectTransform);art.texture=BrandArt.Load("GravityPuzzle/Brand/stage-detail-panel");art.raycastTarget=false;
            DetailText(card,PuzzleCampaign.StageLabel(levels,index),.12f,.085f,.76f,.035f,13,PuzzleUI.Muted);
            DetailText(card,LevelName(level),.12f,.125f,.76f,.06f,26,PuzzleUI.Ink,true);
            DetailText(card,"예상 상위 등급",.23f,.245f,.54f,.035f,14,PuzzleUI.Ink);
            stageRank=DetailText(card,"",.20f,.285f,.60f,.105f,54,PuzzleUI.Ink,true);
            var stars=collection.records.FirstOrDefault(r=>r.id==level.room);
            DetailText(card,stars?.perfect==true?"퍼펙트 클리어":stars==null?"아직 클리어 전이에요":stars.bestMoves>0?"최고 기록  "+stars.bestMoves+"회":"기존 클리어 기록",.22f,.39f,.56f,.04f,15,PuzzleUI.Ink);
            DetailText(card,"초기화 횟수",.08f,.513f,.40f,.035f,13,PuzzleUI.Muted);
            DetailText(card,(record?.restarts??0)+"회",.08f,.548f,.40f,.052f,27,PuzzleUI.Ink,true);
            DetailText(card,"누적 이동 횟수",.52f,.513f,.40f,.035f,13,PuzzleUI.Muted);
            DetailText(card,(record?.moves??0)+"회",.52f,.548f,.40f,.052f,27,PuzzleUI.Ink,true);
            stageRankBasis=DetailText(card,"",.09f,.625f,.82f,.105f,12,PuzzleUI.Muted);
            stageRankBasis.horizontalOverflow=HorizontalWrapMode.Wrap;stageRankBasis.lineSpacing=1.15f;
            bool unlocked=CanPlayLevel(index);
            DetailButton(card,unlocked?"시작하기":"앞 단계를 먼저 완료해 주세요",.744f,.09f,()=>{if(!CanPlayLevel(index))return;session.SetPause(PauseReason.Levels,false);SelectLevel(index);},unlocked);
            DetailButton(card,"돌아가기",.85f,.09f,()=>OpenLevels(true));
            RequestStageRatings();LayoutDialog();
        }
        private Text DetailText(RectTransform card,string value,float x,float top,float width,float height,int size,Color color,bool display=false)
        {
            var label=PuzzleUI.Label("Stage detail "+value,card,value,size,color);
            if(display&&stageDisplayFont&&PuzzleLanguage.Code=="ko")label.font=stageDisplayFont;
            label.fontStyle=FontStyle.Normal;label.resizeTextForBestFit=true;label.resizeTextMinSize=display?18:11;label.resizeTextMaxSize=size;
            label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;
            PuzzleUI.Box(label.rectTransform,card.sizeDelta.x*x,card.sizeDelta.y*(1-top-height),card.sizeDelta.x*width,card.sizeDelta.y*height);
            return label;
        }
        private void DetailButton(RectTransform card,string text,float top,float height,System.Action action,bool enabled=true)
        {
            // The generated artwork supplies the button surface; only text and the hit target are live UI.
            var image=PuzzleUI.Panel(text,card,enabled?Color.clear:new Color(.86f,.85f,.90f,.85f));image.raycastTarget=true;
            PuzzleUI.Box(image.rectTransform,card.sizeDelta.x*.075f,card.sizeDelta.y*(1-top-height),card.sizeDelta.x*.85f,card.sizeDelta.y*height);
            var tap=image.gameObject.AddComponent<PuzzleTap>();tap.CanUse=()=>enabled&&Active&&modal=="stage-detail";tap.Epoch=()=>epoch;tap.Activate=()=>{if(!enabled)return;Tone(1);action();};
            var label=PuzzleUI.Label("Label",image.transform,text,enabled?23:17,enabled?PuzzleUI.Ink:PuzzleUI.Muted);
            if(stageDisplayFont&&PuzzleLanguage.Code=="ko")label.font=stageDisplayFont;
            label.fontStyle=FontStyle.Normal;PuzzleUI.Fill(label.rectTransform);
        }
        private void RefreshStageRating()
        {
            if(modal=="levels")
            {
                for(int i=0;i<levelRanks.Length&&i<levels.Length;i++)
                    if(levelRanks[i])levelRanks[i].text="예상 상위 "+RatingFor(levels[i]);
                return;
            }
            if(modal=="solved"&&clearRank)
            {
                clearRank.text="예상 상위 "+RatingFor(session.Level);
                var saved=collection.records.FirstOrDefault(r=>r.id==session.Level.room);
                clearRankBasis.text=saved?.perfect==true?"이 단계 최고 기록 · 퍼펙트 고정 등급":"이 단계 최고 기록 · "+(StageRating.HasData(TierFor(session.Level))?"참여자 기준":"기본 밸런스 기준");
                return;
            }
            if(modal!="stage-detail"||!stageRank)return;
            var level=levels[detailLevelIndex];var record=collection.records.FirstOrDefault(r=>r.id==level.room);
            var data=TierFor(level);
            stageRank.text=StageRating.Evaluate(record,level.route.Length,data);
            string basis=record?.perfect==true?"퍼펙트 보상 · 0.1% 고정 등급":StageRating.HasData(data)?"참여자 "+data.players+"명의 최고 이동 기록 기준":"기본 밸런스 기준 · 표본 부족/오프라인";
            stageRankBasis.text=basis+"\n실제 순위가 아닌 구간 등급이에요.\n퍼펙트: 최소 "+level.route.Length+"회 · 되돌리기·힌트 미사용";
            if(!CanPlayLevel(detailLevelIndex))
            {
                int first=PuzzleCampaign.FirstUncleared(levels,PuzzleCampaign.Theme(level),HasClearedLevel);
                stageRankBasis.text=PuzzleCampaign.StageLabel(levels,first)+"부터 완료해 주세요.\n앞 단계를 순서대로 깨면 열려요.\n기존 클리어 기록과 별은 유지돼요.";
            }
        }
        private StageTierData TierFor(PuzzleDefinition level)=>stageTiers.rows?.FirstOrDefault(r=>r!=null&&r.room==level.room);
        private string RatingFor(PuzzleDefinition level)=>StageRating.Evaluate(collection.records.FirstOrDefault(r=>r.id==level.room),level.route.Length,TierFor(level));
        private void RequestStageRatings()
        {
            RefreshStageRating();
#if UNITY_WEBGL && !UNITY_EDITOR
            MallangStageTiers(gameObject.name);
#endif
        }
        public void ReceiveStageTiers(string json)
        {
            try{stageTiers=JsonUtility.FromJson<StageTierCatalog>(json)??new StageTierCatalog();RefreshStageRating();}catch{}
        }
    }
}
