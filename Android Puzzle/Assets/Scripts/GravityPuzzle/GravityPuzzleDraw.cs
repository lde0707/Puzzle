using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp
    {
        private const float DrawFinished=3.15f;
        private int drawnSkin,drawNotes;
        private float drawTime;
        private bool drawReady;
        private RectTransform capsuleRoot,capsuleTop,capsuleBottom,drawStage,drawReward,drawActions,drawSkip;
        private CanvasGroup capsuleFade,rewardFade;
        private Text revealTitle,revealSubtitle,revealMessage,rewardConcept;
        private readonly RectTransform[] gatheringStars=new RectTransform[18],burstStars=new RectTransform[18],rewardFriends=new RectTransform[3];
        private readonly SkinDecoration[] rewardSkins=new SkinDecoration[3];
        private readonly PuzzleGlyph[] revealRings=new PuzzleGlyph[3];

        private static void CenterDraw(RectTransform rect,float x,float y,float w,float h)
        {PuzzleUI.Box(rect,x-w/2,y-h/2,w,h);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2(x,y);}

        private void OpenSkinDraw(int skin)
        {
            // The unlock is already saved. Leaving or reloading the reveal cannot lose it.
            drawnSkin=skin;drawTime=0;drawReady=false;drawNotes=0;
            var card=OpenDialog("skin-draw",690);float w=card.sizeDelta.x;
            revealTitle=PuzzleUI.Label("Reveal title",card,"별빛을 모아볼까요?",27,PuzzleUI.Ink);
            revealTitle.fontStyle=FontStyle.Bold;PuzzleUI.Box(revealTitle.rectTransform,12,616,w-24,42);
            revealSubtitle=PuzzleUI.Label("Reveal subtitle",card,"작은 별 셋, 새로운 친구 하나",13,PuzzleUI.Muted);PuzzleUI.Box(revealSubtitle.rectTransform,12,584,w-24,28);
            var stage=PuzzleUI.Panel("Soft starlight stage",card,new Color(.94f,.89f,.99f));PuzzleUI.Box(stage.rectTransform,16,252,w-32,315);drawStage=stage.rectTransform;
            float cx=drawStage.sizeDelta.x/2;
            for(int i=0;i<3;i++)
            {
                revealRings[i]=PuzzleUI.Glyph(drawStage,GlyphKind.Ring,new Color(.73f,.56f,.91f,.25f));
                CenterDraw(revealRings[i].rectTransform,cx,160,180+i*48,180+i*48);
            }
            capsuleRoot=PuzzleUI.Rect("Star capsule",drawStage);CenterDraw(capsuleRoot,cx,158,170,170);
            capsuleFade=capsuleRoot.gameObject.AddComponent<CanvasGroup>();capsuleFade.blocksRaycasts=false;
            capsuleBottom=MakeCapsuleHalf(capsuleRoot,false);capsuleTop=MakeCapsuleHalf(capsuleRoot,true);
            drawReward=PuzzleUI.Rect("Three new friends",drawStage);PuzzleUI.Fill(drawReward);
            rewardFade=drawReward.gameObject.AddComponent<CanvasGroup>();rewardFade.blocksRaycasts=false;
            for(int i=0;i<3;i++)
            {
                rewardFriends[i]=SkinDecoration.Portrait(drawReward,skin,i,out rewardSkins[i]);
                CenterDraw(rewardFriends[i],cx+(i-1)*91,i==1?177:131,i==1?164:89,i==1?164:89);
                rewardSkins[i].Mood(true,Vector2.zero);
            }
            rewardFriends[1].SetAsLastSibling();
            var colors=PuzzleUI.Label("Three colors included",drawReward,"세 가지 색, 모두 함께 왔어요",12,PuzzleUI.Muted);PuzzleUI.Box(colors.rectTransform,8,22,w-48,25);
            for(int i=0;i<18;i++)
            {
                var star=PuzzleUI.Glyph(drawStage,GlyphKind.Star,new Color(1,.78f,.31f));gatheringStars[i]=star.rectTransform;
                CenterDraw(star.rectTransform,cx,158,i%6==0?24:10,i%6==0?24:10);
                var burst=PuzzleUI.Glyph(drawStage,GlyphKind.Star,PuzzleUI.Candy[i%3]);burstStars[i]=burst.rectTransform;CenterDraw(burst.rectTransform,cx,158,9+i%3*3,9+i%3*3);
            }
            revealMessage=PuzzleUI.Label("Reward status",card,"캡슐 속에 누가 기다릴까요?",23,PuzzleUI.Ink);revealMessage.fontStyle=FontStyle.Bold;PuzzleUI.Box(revealMessage.rectTransform,10,202,w-20,39);
            var concept=PuzzleUI.Label("Reward concept",card,SkinDecoration.Concepts[skin],12,PuzzleUI.Muted);PuzzleUI.Box(concept.rectTransform,16,169,w-32,31);
            // Avoid revealing the name or concept before the capsule opens.
            rewardConcept=concept;concept.gameObject.SetActive(false);concept.transform.SetParent(drawReward,false);PuzzleUI.Box(concept.rectTransform,6,-85,w-44,31);
            drawActions=PuzzleUI.Rect("Reward actions",card);PuzzleUI.Box(drawActions,0,12,w,145);
            var equip=MakeButton(drawActions,"Equip reward","바로 착용",null,()=>Active&&modal=="skin-draw"&&drawReady,EquipDrawnSkin,PuzzleUI.Accent);PuzzleUI.Box((RectTransform)equip.transform,24,76,w-48,52);
            var later=MakeButton(drawActions,"Keep reward","나중에 · 옷장에 보관",null,()=>Active&&modal=="skin-draw"&&drawReady,KeepDrawnSkin,PuzzleUI.Mint);PuzzleUI.Box((RectTransform)later.transform,24,23,w-48,43);
            var saved=PuzzleUI.Label("Reward saved",drawActions,"이미 옷장에 저장했어요 · 남은 별 "+collection.wallet,10,PuzzleUI.Muted);PuzzleUI.Box(saved.rectTransform,10,0,w-20,20);
            var skip=MakeButton(card,"Skip reveal","연출 건너뛰기",null,()=>Active&&modal=="skin-draw"&&!drawReady,FinishSkinDraw,new Color(.92f,.86f,.97f));drawSkip=(RectTransform)skip.transform;PuzzleUI.Box(drawSkip,62,64,w-124,42);
            drawActions.gameObject.SetActive(false);LayoutDialog();
            if(reduceMotion)FinishSkinDraw();else{Tone(.8f);RenderSkinDraw();}
        }
        private RectTransform MakeCapsuleHalf(Transform parent,bool top)
        {
            var rect=PuzzleUI.Rect(top?"Capsule lid":"Capsule bowl",parent);PuzzleUI.Fill(rect);
            var graphic=rect.gameObject.AddComponent<StarCapsuleHalf>();graphic.top=top;graphic.raycastTarget=false;return rect;
        }
        private void TickSkinDraw(float delta)
        {
            if(delta<=0)return;
            if(!reduceMotion)drawTime+=delta;
            if(!drawReady)
            {
                int notes=Mathf.Min(3,Mathf.FloorToInt(Mathf.Max(0,drawTime-.36f)/.25f));
                if(notes>drawNotes){drawNotes=notes;Tone(.9f+notes*.18f);}
                if(drawTime>=DrawFinished||reduceMotion)FinishSkinDraw();
            }
            RenderSkinDraw();
        }
        private void FinishSkinDraw()
        {
            if(drawReady)return;
            drawTime=Mathf.Max(DrawFinished,drawTime);drawReady=true;CancelTouches();
            drawActions.gameObject.SetActive(true);drawSkip.gameObject.SetActive(false);
            revealTitle.text="새 친구를 만났어요!";revealSubtitle.text="우리의 궤도에 오신 걸 환영해요";
            VictoryTone();RenderSkinDraw();
        }
        private void RenderSkinDraw()
        {
            float t=drawTime,cx=drawStage.sizeDelta.x/2;
            float opening=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-1.65f)/.65f));
            float reveal=Mathf.Clamp01((t-1.88f)/.85f);
            capsuleRoot.gameObject.SetActive(t<2.65f);
            capsuleRoot.localEulerAngles=new Vector3(0,0,t>1&&t<1.65f?Mathf.Sin(t*39)*5:0);
            float swell=1+.08f*Mathf.Sin(Mathf.Clamp01((t-.85f)/.8f)*Mathf.PI);
            capsuleRoot.localScale=Vector3.one*swell;
            capsuleTop.anchoredPosition=new Vector2(-opening*24,opening*85);capsuleTop.localEulerAngles=new Vector3(0,0,opening*-22);
            capsuleBottom.anchoredPosition=new Vector2(opening*16,-opening*65);capsuleBottom.localEulerAngles=new Vector3(0,0,opening*16);
            capsuleFade.alpha=1-Mathf.Clamp01((t-2.05f)/.6f);
            drawReward.gameObject.SetActive(t>=1.88f);rewardFade.alpha=Mathf.Clamp01(reveal*4);
            // A damped overshoot makes the reveal feel soft, then settles to a quiet idle.
            float spring=reveal>=1?1:1-Mathf.Pow(1-reveal,3)*Mathf.Cos(reveal*Mathf.PI*2);
            for(int i=0;i<3;i++)
            {
                float wave=reduceMotion?0:Mathf.Sin(t*1.7f+i*.7f)*2;
                rewardFriends[i].anchoredPosition=new Vector2(cx+(i-1)*91,(i==1?177:131)-24*(1-reveal)+wave);
                rewardFriends[i].localScale=Vector3.one*(.12f+.88f*spring);rewardSkins[i].Pose(t,reduceMotion);
            }
            // The concept label is a child of the reward so it shares the reveal fade.
            rewardConcept.gameObject.SetActive(t>=1.88f);
            if(!drawReady){revealTitle.text=t<1.25f?"별빛이 모이고 있어요":"두근두근, 누구일까요?";revealMessage.text=t<1.88f?"캡슐 속에 누가 기다릴까요?":PuzzleCollection.Names[drawnSkin];}
            else revealMessage.text=PuzzleCollection.Names[drawnSkin];
            for(int i=0;i<3;i++)
            {
                float breath=reduceMotion?0:Mathf.Sin(t*1.6f+i)*.035f;
                revealRings[i].rectTransform.localScale=Vector3.one*(1+breath+opening*.06f);
                revealRings[i].color=new Color(.73f,.56f,.91f,drawReady?.22f:.18f+opening*.15f);
            }
            for(int i=0;i<18;i++)
            {
                int source=i/6;float p=Mathf.Clamp01((t-source*.22f-(i%6)*.034f)/.75f);
                gatheringStars[i].gameObject.SetActive(!reduceMotion&&t<1.55f&&p<1);
                float angle=source*Mathf.PI*2/3+Mathf.PI/2+p*1.7f;
                float r=132*(1-Mathf.SmoothStep(0,1,p));
                gatheringStars[i].anchoredPosition=new Vector2(cx+Mathf.Cos(angle)*r,158+Mathf.Sin(angle)*r);
                gatheringStars[i].localScale=Vector3.one*(1-p*.65f);gatheringStars[i].localEulerAngles=new Vector3(0,0,p*180);
                float burst=Mathf.Clamp01((t-1.85f)/1.25f);
                burstStars[i].gameObject.SetActive(t>=1.85f&&(reduceMotion||t<3.1f||i%3==0));
                float a=i*Mathf.PI*2/18;float radius=reduceMotion||drawReady?130:Mathf.Lerp(15,140,1-Mathf.Pow(1-burst,3));
                burstStars[i].anchoredPosition=new Vector2(cx+Mathf.Cos(a)*radius,160+Mathf.Sin(a)*radius);
                float sparkle=reduceMotion?1:.75f+.25f*Mathf.Sin(t*2+i);
                burstStars[i].localScale=Vector3.one*(drawReady?.7f:sparkle);burstStars[i].localEulerAngles=new Vector3(0,0,i*23);
            }
        }
        private void EquipDrawnSkin()
        {
            if(!drawReady)return;
            if(!collection.TryEquip(drawnSkin,Save)){revealSubtitle.text="착용을 저장하지 못했어요. 다시 눌러 주세요.";return;}
            board.ApplySkin(drawnSkin);RefreshHomeSkin();Tone(1.1f);
            OpenCollection(PuzzleCollection.Names[drawnSkin]+" 착용 완료!\n이 모습으로 출발해 볼까요?");
        }
        private void KeepDrawnSkin(){OpenCollection(PuzzleCollection.Names[drawnSkin]+"을 옷장에 보관했어요.\n원할 때 선택하고 저장하면 착용돼요.");}
        private void BackFromSkinDraw(){if(!drawReady)FinishSkinDraw();else KeepDrawnSkin();}
    }
}
