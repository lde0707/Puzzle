using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp
    {
#if UNITY_EDITOR
        // Presentation-only preview; never redirects editor saves to the SDK.
        public bool PreviewQuickPlay;
        private bool QuickPlay=>CrazyPortal.Enabled||PreviewQuickPlay;
#else
        private bool QuickPlay=>CrazyPortal.Enabled;
#endif
        private void LayoutQuickPlay()
        {
            var l=PuzzlePlayLayout.For(safe.rect.width,safe.rect.height);
            float h=safe.rect.height,x=l.left,w=l.width;
            levelIcon.gameObject.SetActive(false);levelButton.gameObject.SetActive(false);
            lifetimeMoves.gameObject.SetActive(false);resetButton.gameObject.SetActive(false);
            centerLabel.gameObject.SetActive(false);ruleButton.gameObject.SetActive(false);
            arrivals.gameObject.SetActive(false);gravity.gameObject.SetActive(false);
            keyLegend.gameObject.SetActive(false);coachTitle.gameObject.SetActive(false);
            coachSkip.gameObject.SetActive(false);controlsPaper.gameObject.SetActive(false);
            coachPaper.gameObject.SetActive(true);coachPaper.color=Color.clear;
            foreach(var shadow in coachPaper.GetComponents<Shadow>())shadow.enabled=false;
            drawingTools.gameObject.SetActive(true);
            float cx;
            if(l.wide)
            {
                float sx=l.controlsX,cw=l.controlsWidth;cx=sx+cw/2;
                PuzzleUI.Box(title.rectTransform,x+12,h-58,l.boardWidth-24,44);
                PuzzleUI.Box(subtitle.rectTransform,x+12,h-82,l.boardWidth-24,20);
                PuzzleUI.Box(walletBadge,sx+130,h-58,96,32);
                PuzzleUI.Box((RectTransform)pauseButton.transform,sx+244,h-64,44,44);
                float bottom=100,top=h-100;
                board.Layout(l.boardWidth-24,top-bottom,x+l.boardWidth/2,(top+bottom)/2);
                PuzzleUI.Box(stats.rectTransform,sx,h-136,cw,34);
                PuzzleUI.Box(coachPaper.rectTransform,sx,h-206,cw,56);
                PuzzleUI.Box(hint.rectTransform,sx,h-247,cw,32);
                float y=Mathf.Max(160,h/2-125);
                QuickDirections(cx,y,60,72);
                QuickUtility(undoButton,sx,y-72,140);
                QuickUtility(hintButton,sx+160,y-72,140);
                PuzzleUI.Box(drawingTools,x+(l.boardWidth-328)/2,36,328,38);
                drawingTools.localScale=Vector3.one;
                PuzzleUI.Box(saveLabel.rectTransform,x,8,w,20);
            }
            else
            {
                cx=x+w/2;
                PuzzleUI.Box(title.rectTransform,x+18,h-60,w-100,46);
                PuzzleUI.Box(subtitle.rectTransform,x+18,h-83,w-140,20);
                PuzzleUI.Box((RectTransform)pauseButton.transform,x+w-62,h-60,44,44);
                PuzzleUI.Box(walletBadge,x+w-114,h-100,96,30);
                PuzzleUI.Box(stats.rectTransform,x+18,h-124,115,30);
                float top=h-144,bottom=310;
                board.Layout(Mathf.Min(safe.rect.width-36,680),Mathf.Max(80,top-bottom),cx,(top+bottom)/2);
                float toolScale=Mathf.Min(1,(w-24)/328);
                PuzzleUI.Box(drawingTools,x+(w-328*toolScale)/2,246,328,38);
                drawingTools.localScale=Vector3.one*toolScale;
                PuzzleUI.Box(coachPaper.rectTransform,x+18,191,w-36,46);
                PuzzleUI.Box(hint.rectTransform,x+18,168,w-36,22);
                QuickDirections(cx,16,46,50);
                float utility=Mathf.Min(100,w/2-62);
                QuickUtility(undoButton,x+18,18,utility);
                QuickUtility(hintButton,x+w-18-utility,18,utility);
                PuzzleUI.Box(saveLabel.rectTransform,x+12,0,w-24,16);
            }
            PuzzleUI.Box(moves.rectTransform,10,0,stats.rectTransform.sizeDelta.x-20,stats.rectTransform.sizeDelta.y);
            PuzzleUI.Fill(coachText.rectTransform,2);coachText.fontSize=largeText?15:13;
            coachText.alignment=TextAnchor.MiddleCenter;
            subtitle.text=PuzzleCampaign.StageLabel(levels,levelIndex);
        }
        private void QuickDirections(float cx,float y,float side,float gap)
        {
            PuzzleUI.Box((RectTransform)arrows[0].transform,cx-side/2,y,side,side);
            PuzzleUI.Box((RectTransform)arrows[1].transform,cx-gap-side/2,y+gap,side,side);
            PuzzleUI.Box((RectTransform)arrows[3].transform,cx+gap-side/2,y+gap,side,side);
            PuzzleUI.Box((RectTransform)arrows[2].transform,cx-side/2,y+gap*2,side,side);
        }
        private static void QuickUtility(PuzzleTap button,float x,float y,float width)
        {PuzzleUI.Box((RectTransform)button.transform,x,y,width,44);}
        private string QuickMechanicTip()
        {
            if(session.Level.HasBlinkBlocks)return "방향을 누르면 길이 열리고 닫혀요.";
            if(session.Level.HasResizers)return "변환대에 멈춰 크기를 바꾸세요. 목적지의 색과 크기를 맞춰요.";
            if(session.Level.HasPortals)return "빛나는 면으로 들어가요. 포탈을 누르면 출구가 보여요.";
            return "방향 하나로 모두 데굴! 같은 색 목적지에 도착하세요.";
        }
        private void OpenQuickPause()
        {
            session.SetPause(PauseReason.Menu,true);
            var card=OpenDialog("pause",330);
            DialogText(card,"잠깐, 쉬어가요",274,36,24);
            DialogButton(card,"계속하기",210,Resume,PuzzleUI.Accent);
            float half=(card.sizeDelta.x-58)/2;
            void Small(string text,int col,float y,System.Action action)
            {
                var b=MakeButton(card,text,text,null,()=>Active,action,PuzzleUI.Mint);
                PuzzleUI.Box((RectTransform)b.transform,24+col*(half+10),y,half,48);
            }
            Small("단계 선택",0,148,()=>{session.SetPause(PauseReason.Menu,false);OpenLevels();});
            Small("초기화",1,148,AskReset);
            Small("스킨 뽑기",0,86,()=>OpenCollection());
            Small("설정",1,86,()=>{settingsFromHome=false;ShowSettings(0);});
            if(session.Level.HasPortals||session.Level.HasResizers||session.Level.HasBlinkBlocks)
                DialogButton(card,"사용법",24,OpenMechanicHelp);
            LayoutDialog();
        }
        private void CompleteQuick()
        {
            var card=OpenDialog("solved",350);
            DialogText(card,"우리, 사뿐히 착륙!",290,36,24);
            int rating=PuzzleCollection.Rating(session.Moves,session.Level.route.Length,session.UsedUndo);
            completionStars=new CompletionStars(card,rating,reduceMotion,i=>PlayRewardNote(i*2),PlayRewardFinale);
            var row=(RectTransform)completionStars.Star(0).parent;
            row.anchoredPosition=new Vector2(card.sizeDelta.x/2,231);
            row.localScale*=.82f;
            DialogText(card,session.Moves+"회 이동 · "+(earnedStars>0?"별 +"+earnedStars:"최고 별 기록 유지"),145,25,14,PuzzleUI.Muted);
            int next=PuzzleCampaign.NextInTheme(levels,levelIndex);
            DialogButton(card,next>=0?"다음 단계":"테마 선택",82,()=>{if(next>=0)LoadLevel(next);else OpenLevels();},PuzzleUI.Accent);
            DialogButton(card,"한번 더 데굴",22,()=>LoadLevel(levelIndex));LayoutDialog();
            celebration=new PuzzleCelebration(dialog);celebration.Play(reduceMotion||!celebrationPending);celebrationPending=false;
            walletBadge.SetAsLastSibling();RefreshWallet();
            if(pendingWalletStars>0&&!reduceMotion)
                walletFlight=new WalletStarFlight(dialog,completionStars,walletIcon,rating,pendingWalletStars,ReceiveWalletStar);
            else {SettleWallet();completionStars.FinishWhenReady();}
        }
    }
}
