using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    // Safe-area coordinates. The puzzle, not the directional pad, gets the main space.
    public struct PuzzlePlayLayout
    {
        public bool wide;
        public float left,width,boardWidth,controlsX,controlsWidth;
        public static PuzzlePlayLayout For(float width,float height)
        {
            bool wide=width>=1000&&height>=600;
            float used=wide?Mathf.Min(1260,width-48):Mathf.Min(440,width);
            return new PuzzlePlayLayout{wide=wide,left=(width-used)/2,width=used,
                boardWidth=wide?used-332:used,controlsWidth=wide?300:used,
                controlsX=(width-used)/2+(wide?used-300:0)};
        }
    }

    public sealed partial class GravityPuzzleApp
    {
        private const string CoachKey="intro-gravity-v2";
        private Image controlsPaper,coachPaper;
        private Text coachTitle,coachText,keyLegend;
        private PuzzleTap coachSkip;
        private Image[] coachGlows;
        private bool coaching;
        private float coachAge;
        private string coachRoom;
        private Cell[][] coachPositions;
        private static readonly string[] CoachSteps={"위 버튼을 눌러 보세요. 벽에 닿을 때까지 움직여요.","이번에는 오른쪽! 한 번 누르면 끝까지 데굴.","아래로 데굴! 같은 색 목적지에 도착해요."};

        private void CreateCoachUI()
        {
            controlsPaper=PuzzleUI.Panel("Lilac control paper",safe,new Color(.94f,.91f,.98f));
            controlsPaper.transform.SetAsFirstSibling();
            coachPaper=PuzzleUI.Panel("Play together guide",safe,new Color(1,.94f,.82f));
            coachTitle=PuzzleUI.Label("Guide title",coachPaper.transform,"모두 함께 데굴!",18,PuzzleUI.Ink,TextAnchor.MiddleLeft);
            coachTitle.fontStyle=FontStyle.Bold;
            coachText=PuzzleUI.Label("Guide instruction",coachPaper.transform,"",14,PuzzleUI.Ink,TextAnchor.MiddleLeft);
            coachSkip=MakeButton(coachPaper.transform,"Skip introduction","건너뛰기",null,()=>Active&&modal==""&&coaching,()=>EndCoach(true),new Color(.96f,.89f,.76f));
            keyLegend=PuzzleUI.Label("Keyboard controls",safe,"방향키 / WASD · Z 되돌리기",12,PuzzleUI.Muted);
            coachGlows=new Image[4];
            for(int i=0;i<4;i++)
            {
                coachGlows[i]=PuzzleUI.Panel("Free tutorial direction",arrows[i].transform,new Color(1,.78f,.36f,.6f));
                PuzzleUI.Fill(coachGlows[i].rectTransform,-4);coachGlows[i].transform.SetAsFirstSibling();
                coachGlows[i].gameObject.SetActive(false);
            }
        }
        private void LayoutPresentation()
        {
            if(QuickPlay){LayoutQuickPlay();return;}
            var l=PuzzlePlayLayout.For(safe.rect.width,safe.rect.height);
            float h=safe.rect.height,x=l.left,w=l.width;
            controlsPaper.gameObject.SetActive(l.wide);
            keyLegend.gameObject.SetActive(l.wide);
            if(l.wide)
            {
                float sx=l.controlsX,cw=l.controlsWidth,cx=sx+cw/2;
                PuzzleUI.Box(controlsPaper.rectTransform,sx-14,42,cw+28,h-116);
                PuzzleUI.Box(levelIcon.rectTransform,x+8,h-66,40,40);
                PuzzleUI.Box(title.rectTransform,x+60,h-74,l.boardWidth-80,56);
                PuzzleUI.Box(subtitle.rectTransform,x+12,h-94,l.boardWidth-24,18);
                PuzzleUI.Box((RectTransform)levelButton.transform,sx+140,h-58,44,44);
                PuzzleUI.Box((RectTransform)pauseButton.transform,sx+194,h-58,44,44);
                PuzzleUI.Box(walletBadge,sx+4,h-54,100,32);
                PuzzleUI.Box(stats.rectTransform,sx,h-136,cw,42);
                PuzzleUI.Box(lifetimeMoves.rectTransform,sx,h-164,cw,22);
                PuzzleUI.Box(moves.rectTransform,12,0,104,42);
                PuzzleUI.Box(arrivals.rectTransform,118,0,140,42);
                PuzzleUI.Box(gravity.rectTransform,cw-34,10,22,22);
                float bottom=100,top=h-108;
                board.Layout(l.boardWidth-24,top-bottom,x+l.boardWidth/2,(top+bottom)/2);
                PuzzleUI.Box(coachPaper.rectTransform,sx,h-294,cw,112);
                PuzzleUI.Box(hint.rectTransform,sx,h-332,cw,34);
                float y=Mathf.Max(184,h-536),side=60,gap=70;
                PuzzleUI.Box((RectTransform)arrows[2].transform,cx-side/2,y+140,side,side);
                PuzzleUI.Box((RectTransform)arrows[1].transform,cx-gap-side/2,y+70,side,side);
                PuzzleUI.Box((RectTransform)arrows[3].transform,cx+gap-side/2,y+70,side,side);
                PuzzleUI.Box((RectTransform)arrows[0].transform,cx-side/2,y,side,side);
                PuzzleUI.Box(centerLabel.rectTransform,cx-side/2,y+70,side,side);
                PuzzleUI.Box((RectTransform)ruleButton.transform,cx-side/2,y+70,side,side);
                PuzzleUI.Box((RectTransform)hintButton.transform,sx+34,y-64,cw-68,46);
                PuzzleUI.Box(keyLegend.rectTransform,sx,y-100,cw,26);
                PuzzleUI.Box((RectTransform)undoButton.transform,sx,64,142,46);
                PuzzleUI.Box((RectTransform)resetButton.transform,sx+158,64,142,46);
                PuzzleUI.Box(drawingTools,x+(l.boardWidth-328)/2,36,328,38);
                PuzzleUI.Box(saveLabel.rectTransform,x,16,w,18);
            }
            else
            {
                PuzzleUI.Box(levelIcon.rectTransform,x+16,h-55,32,32);
                PuzzleUI.Box(title.rectTransform,x+54,h-72,w-182,62);
                PuzzleUI.Box(subtitle.rectTransform,x+18,h-96,w-140,20);
                PuzzleUI.Box((RectTransform)levelButton.transform,x+w-114,h-54,44,44);
                PuzzleUI.Box((RectTransform)pauseButton.transform,x+w-62,h-54,44,44);
                PuzzleUI.Box(walletBadge,x+w-114,h-92,96,30);
                PuzzleUI.Box(stats.rectTransform,x+18,h-134,w-36,34);
                PuzzleUI.Box(moves.rectTransform,12,0,100,34);
                PuzzleUI.Box(arrivals.rectTransform,110,0,w-202,34);
                PuzzleUI.Box(gravity.rectTransform,w-70,7,20,20);
                PuzzleUI.Box(lifetimeMoves.rectTransform,x+18,h-154,w-36,18);
                float bottom=coaching?364:278,top=h-164;
                // On a tablet or a narrow desktop window the board can use the
                // spare side space, while touch controls retain their familiar width.
                float boardWidth=Mathf.Min(safe.rect.width-36,680);
                board.Layout(boardWidth,Mathf.Max(80,top-bottom),x+w/2,(top+bottom)/2);
                PuzzleUI.Box(coachPaper.rectTransform,x+18,266,w-36,76);
                PuzzleUI.Box(hint.rectTransform,x+18,190,w-36,20);
                float cx=x+w/2,side=50,gap=58;
                PuzzleUI.Box((RectTransform)arrows[2].transform,cx-side/2,137,side,side);
                PuzzleUI.Box((RectTransform)arrows[1].transform,cx-gap-side/2,81,side,side);
                PuzzleUI.Box((RectTransform)arrows[3].transform,cx+gap-side/2,81,side,side);
                PuzzleUI.Box((RectTransform)arrows[0].transform,cx-side/2,25,side,side);
                PuzzleUI.Box(centerLabel.rectTransform,cx-side/2,81,side,side);
                PuzzleUI.Box((RectTransform)ruleButton.transform,cx-side/2,81,side,side);
                PuzzleUI.Box((RectTransform)hintButton.transform,x+18,140,Mathf.Min(100,w/2-54),44);
                float utility=Mathf.Min(108,w/2-54);
                PuzzleUI.Box((RectTransform)undoButton.transform,x+18,26,utility,44);
                PuzzleUI.Box((RectTransform)resetButton.transform,x+w-18-utility,26,utility,44);
                float toolScale=Mathf.Min(1,(w-24)/328);
                PuzzleUI.Box(drawingTools,x+(w-328*toolScale)/2,214,328,38);
                drawingTools.localScale=Vector3.one*toolScale;
                PuzzleUI.Box(saveLabel.rectTransform,x+18,3,w-36,16);
            }
            if(l.wide)drawingTools.localScale=Vector3.one;
            float cardW=coachPaper.rectTransform.sizeDelta.x,cardH=coachPaper.rectTransform.sizeDelta.y;
            PuzzleUI.Box(coachTitle.rectTransform,12,cardH-31,cardW-106,25);
            PuzzleUI.Box(coachText.rectTransform,12,8,cardW-24,cardH-39);
            PuzzleUI.Box((RectTransform)coachSkip.transform,cardW-88,cardH-34,78,28);
            coachPaper.gameObject.SetActive(l.wide||coaching);
            coachSkip.gameObject.SetActive(coaching);
            drawingTools.gameObject.SetActive(true);
        }
        private void BeginCoach()
        {
            coaching=session.Level.room=="F0"&&!session.Solved&&!seenLessons.Contains(CoachKey);
            if(coaching)
            {
                var state=new PuzzleSession(session.Level);
                coachPositions=new Cell[session.Level.route.Length][];
                for(int i=0;i<coachPositions.Length;i++)
                {coachPositions[i]=state.Stable.positions.ToArray();state.Command(session.Level.route[i]);state.Tick(100,true);}
            }
            coachRoom=session.Level.room;coachAge=0;Layout();
        }
        private void EndCoach(bool remember)
        {
            if(!coaching)return;
            coaching=false;
            if(remember){seenLessons.Add(CoachKey);Save();}
            foreach(var glow in coachGlows)glow.gameObject.SetActive(false);
            Layout();
        }
        private void TickCoach(float delta)
        {
            if(!coachPaper)return;
            if(coaching&&coachRoom!=session.Level.room)EndCoach(false);
            coachAge+=delta;
            int step=-1,direction=-1;
            if(coaching&&!session.Solved)
            {
                for(int i=0;i<session.Level.route.Length;i++)
                {
                    if(coachPositions[i].SequenceEqual(session.Stable.positions)){step=i;direction=session.Level.route[i];break;}
                }
            }
            coachTitle.text=coaching?"첫 여행 · 무료 조작 안내":"모두 함께 데굴!";
            coachText.text=coaching?(step>=0?CoachSteps[Mathf.Min(step,2)]:"다른 길도 괜찮아요. 되돌리기로 이전 위치로 돌아가요."):"방향 하나로 모든 친구가 움직여요.\n각자 같은 색 목적지로 데려다주세요.";
            if(QuickPlay&&!coaching)coachText.text=QuickMechanicTip();
            for(int i=0;i<4;i++)
            {
                bool visible=coaching&&i==direction&&modal==""&&!session.Moving;
                coachGlows[i].gameObject.SetActive(visible);
                if(visible)coachGlows[i].color=new Color(1,.78f,.36f,reduceMotion?.55f:.35f+.2f*Mathf.Sin(coachAge*4));
            }
        }
    }
}
