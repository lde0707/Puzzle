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
        private DrawStageArt drawArt;
        private RectTransform drawBackdrop;
        private AudioClip[] drawClips;
        private static readonly float[] DrawCueTimes={.28f,.56f,.84f,1.68f,1.9f};
        private RectTransform capsuleRoot,capsuleTop,capsuleBottom,drawStage,drawReward,drawActions,drawSkip;
        private CanvasGroup capsuleFade,rewardFade;
        private Text revealTitle,revealSubtitle,revealMessage,rewardConcept;
        private readonly RectTransform[] gatheringStars=new RectTransform[18],burstStars=new RectTransform[18],rewardFriends=new RectTransform[3];
        private readonly SkinDecoration[] rewardSkins=new SkinDecoration[3];
        private readonly PuzzleGlyph[] revealRings=new PuzzleGlyph[3];

        private static void CenterDraw(RectTransform rect,float x,float y,float w,float h)
        {PuzzleUI.Box(rect,x-w/2,y-h/2,w,h);rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=new Vector2(x,y);}

        private static RectTransform MochiDrawFrame(RectTransform card,string resource="mochi-draw-panel")
        {
            var texture=BrandArt.Load("GravityPuzzle/Brand/"+resource);
            if(!texture)return null;
            card.GetComponent<Image>().color=Color.clear;
            foreach(var shadow in card.GetComponents<Shadow>())shadow.enabled=false;
            var art=PuzzleUI.Rect("Painted marshmallow frame",card).gameObject.AddComponent<AspectCoverImage>();
            PuzzleUI.Fill(art.rectTransform);art.texture=texture;art.raycastTarget=false;
            art.transform.SetAsFirstSibling();return art.rectTransform;
        }

        private void OpenSkinDraw(int skin)
        {
            // The unlock is already saved. Leaving or reloading the reveal cannot lose it.
            drawnBatch=System.Array.Empty<int>();batchGrid=null;batchShown=0;
            drawnSkin=skin;drawTime=0;drawReady=false;drawNotes=0;
            var card=OpenDialog("skin-draw",690);float w=card.sizeDelta.x;
            drawBackdrop=MochiDrawFrame(card);
            var badge=PuzzleUI.Panel("Candy club badge",card,new Color(1,.88f,.76f));PuzzleUI.Box(badge.rectTransform,w/2-70,662,140,20);
            var badgeText=PuzzleUI.Label("Candy club label",badge.transform,"별사탕 캡슐",11,PuzzleUI.Ink);badgeText.fontStyle=FontStyle.Bold;PuzzleUI.Fill(badgeText.rectTransform);
            revealTitle=PuzzleUI.Label("Reveal title",card,"별빛을 모아볼까요?",22,PuzzleUI.Ink);
            revealTitle.fontStyle=FontStyle.Bold;PuzzleUI.Box(revealTitle.rectTransform,30,598,w-60,38);
            revealSubtitle=PuzzleUI.Label("Reveal subtitle",card,"작은 별 셋, 새로운 친구 하나",12,PuzzleUI.Muted);PuzzleUI.Box(revealSubtitle.rectTransform,22,570,w-44,26);
            var stage=PuzzleUI.Panel("Soft starlight stage",card,Color.clear);PuzzleUI.Box(stage.rectTransform,16,252,w-32,315);drawStage=stage.rectTransform;
            var artwork=PuzzleUI.Rect("Candy spotlight and confetti",drawStage);PuzzleUI.Fill(artwork);
            drawArt=artwork.gameObject.AddComponent<DrawStageArt>();drawArt.raycastTarget=false;
            float cx=drawStage.sizeDelta.x/2;
            for(int i=0;i<3;i++)
            {
                revealRings[i]=PuzzleUI.Glyph(drawStage,GlyphKind.Ring,new Color(.73f,.56f,.91f,.25f));
                CenterDraw(revealRings[i].rectTransform,cx,160,172+i*42,172+i*42);
            }
            capsuleRoot=PuzzleUI.Rect("Star capsule",drawStage);CenterDraw(capsuleRoot,cx,165,214,214);
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
            var colors=PuzzleUI.Label("Three colors included",drawReward,"세 가지 색, 모두 함께 왔어요",12,PuzzleUI.Muted);PuzzleUI.Box(colors.rectTransform,8,6,w-48,25);
            for(int i=0;i<18;i++)
            {
                var star=PuzzleUI.Rect("Gathering candy star "+i,drawStage);gatheringStars[i]=star;
                if(i%6==0){var paintedStar=star.gameObject.AddComponent<PuffyStar>();paintedStar.painted=true;paintedStar.raycastTarget=false;}
                else{var sparkle=star.gameObject.AddComponent<PuzzleGlyph>();sparkle.kind=GlyphKind.Star;sparkle.color=new Color(1,.83f,.42f);sparkle.raycastTarget=false;}
                CenterDraw(star,cx,158,i%6==0?34:12,i%6==0?34:12);
                var burst=PuzzleUI.Glyph(drawStage,GlyphKind.Star,PuzzleUI.Candy[i%3]);burstStars[i]=burst.rectTransform;CenterDraw(burst.rectTransform,cx,158,9+i%3*3,9+i%3*3);
            }
            revealMessage=PuzzleUI.Label("Reward status",card,"캡슐 속에 누가 기다릴까요?",23,PuzzleUI.Ink);revealMessage.fontStyle=FontStyle.Bold;PuzzleUI.Box(revealMessage.rectTransform,10,202,w-20,39);
            var concept=PuzzleUI.Label("Reward concept",card,SkinDecoration.Concepts[skin],12,PuzzleUI.Muted);PuzzleUI.Box(concept.rectTransform,16,169,w-32,31);
            // Avoid revealing the name or concept before the capsule opens.
            rewardConcept=concept;concept.gameObject.SetActive(false);concept.transform.SetParent(drawReward,false);PuzzleUI.Box(concept.rectTransform,6,-85,w-44,31);
            drawActions=PuzzleUI.Rect("Reward actions",card);PuzzleUI.Box(drawActions,0,12,w,145);
            var equip=MakeButton(drawActions,"Equip reward","이 친구로 바로 착용",null,()=>Active&&modal=="skin-draw"&&drawReady,EquipDrawnSkin,PuzzleUI.Accent);PuzzleUI.Box((RectTransform)equip.transform,24,76,w-48,52);
            var later=MakeButton(drawActions,"Keep reward","나중에 · 옷장에 보관",null,()=>Active&&modal=="skin-draw"&&drawReady,KeepDrawnSkin,PuzzleUI.Mint);PuzzleUI.Box((RectTransform)later.transform,24,23,w-48,43);
            var saved=PuzzleUI.Label("Reward saved",drawActions,"이미 옷장에 저장했어요 · 남은 별 "+collection.wallet,10,PuzzleUI.Muted);PuzzleUI.Box(saved.rectTransform,10,0,w-20,20);
            var skip=MakeButton(card,"Skip reveal","연출 건너뛰기",null,()=>Active&&modal=="skin-draw"&&!drawReady,FinishSkinDraw,new Color(.92f,.86f,.97f));drawSkip=(RectTransform)skip.transform;PuzzleUI.Box(drawSkip,62,64,w-124,42);
            drawActions.gameObject.SetActive(false);LayoutDialog();
            if(drawClips==null){drawClips=new AudioClip[6];for(int i=0;i<drawClips.Length;i++)drawClips[i]=SkinDrawAudio.Make(i);}
            // Fast play shortens puzzle turns; the capsule reveal keeps its full presentation.
            RenderSkinDraw();
        }
        private RectTransform MakeCapsuleHalf(Transform parent,bool top)
        {
            var rect=PuzzleUI.Rect(top?"Capsule lid":"Capsule bowl",parent);PuzzleUI.Fill(rect);
            var texture=BrandArt.Load("GravityPuzzle/Brand/mochi-capsule");
            if(texture)
            {
                // Sample the same sprite on each half: no texture duplication or baked animation.
                const float seam=.455f;
                var image=PuzzleUI.Rect("Painted mochi shell",rect).gameObject.AddComponent<RawImage>();
                image.texture=texture;image.raycastTarget=false;
                image.rectTransform.anchorMin=new Vector2(0,top?seam:0);
                image.rectTransform.anchorMax=new Vector2(1,top?1:seam);
                image.rectTransform.offsetMin=image.rectTransform.offsetMax=Vector2.zero;
                image.uvRect=new Rect(0,top?seam:0,1,top?1-seam:seam);
                return rect;
            }
            var graphic=rect.gameObject.AddComponent<StarCapsuleHalf>();graphic.top=top;graphic.raycastTarget=false;return rect;
        }
        private void TickSkinDraw(float delta)
        {
            if(delta<=0)return;
            drawTime+=delta;
            if(!drawReady)
            {
                while(drawNotes<DrawCueTimes.Length&&drawTime>=DrawCueTimes[drawNotes]){PlayDrawCue(drawNotes);drawNotes++;}
                TickBatchCues();
                if(drawTime>=DrawEndTime)FinishSkinDraw();
            }
            RenderSkinDraw();
        }
        private void FinishSkinDraw()
        {
            if(drawReady)return;
            drawTime=Mathf.Max(DrawEndTime,drawTime);drawReady=true;CancelTouches();
            drawActions.gameObject.SetActive(true);drawSkip.gameObject.SetActive(false);
            revealTitle.text="새 친구를 만났어요!";revealSubtitle.text="우리의 궤도에 오신 걸 환영해요";
            // Skipping cancels the unfinished flourish instead of stacking sounds.
            if(rewardSource)rewardSource.Stop();PlayDrawCue(5);RenderSkinDraw();
        }
        private void PlayDrawCue(int cue){if(sound&&Active&&rewardSource&&drawClips!=null)rewardSource.PlayOneShot(drawClips[cue]);}
        private void RenderSkinDraw()
        {
            float t=drawTime,cx=drawStage.sizeDelta.x/2;
            drawArt.Pose(t,false);
            if(drawBackdrop)drawBackdrop.localScale=new Vector3(1+Mathf.Sin(t*1.8f)*.004f,1-Mathf.Sin(t*1.8f)*.003f,1);
            float opening=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-1.65f)/.65f));
            float reveal=Mathf.Clamp01((t-1.88f)/.85f);
            capsuleRoot.gameObject.SetActive(t<2.65f);
            capsuleRoot.localEulerAngles=new Vector3(0,0,t>1&&t<1.65f?Mathf.Sin(t*39)*5:0);
            float swell=1+.11f*Mathf.Sin(Mathf.Clamp01((t-.85f)/.8f)*Mathf.PI);
            float squeeze=Mathf.Sin(Mathf.Clamp01((t-1.05f)/.6f)*Mathf.PI)*.16f;
            capsuleRoot.localScale=new Vector3(swell+squeeze,swell-squeeze,1);
            capsuleRoot.anchoredPosition=new Vector2(cx,165+Mathf.Sin(Mathf.Min(t,1.65f)*3)*5);
            capsuleTop.anchoredPosition=new Vector2(-opening*42,opening*87+Mathf.Sin(opening*Mathf.PI)*22);capsuleTop.localEulerAngles=new Vector3(0,0,opening*-32);
            capsuleBottom.anchoredPosition=new Vector2(opening*30,-opening*56);capsuleBottom.localEulerAngles=new Vector3(0,0,opening*23);
            capsuleTop.localScale=new Vector3(1-opening*.12f,1+Mathf.Sin(opening*Mathf.PI)*.08f,1);
            capsuleBottom.localScale=new Vector3(1+Mathf.Sin(opening*Mathf.PI)*.12f,1-opening*.16f,1);
            capsuleFade.alpha=1-Mathf.Clamp01((t-1.85f)/.55f);
            drawReward.gameObject.SetActive(t>=1.88f&&!IsBatchDraw);rewardFade.alpha=Mathf.Clamp01(reveal*4);
            // A damped overshoot makes the reveal feel soft, then settles to a quiet idle.
            float spring=reveal>=1?1:1-Mathf.Pow(1-reveal,3)*Mathf.Cos(reveal*Mathf.PI*2);
            for(int i=0;i<3;i++)
            {
                float wave=Mathf.Sin(t*1.7f+i*.7f)*2;
                rewardFriends[i].anchoredPosition=new Vector2(cx+(i-1)*91,(i==1?177:131)-24*(1-reveal)+wave);
                float bounce=Mathf.Sin(reveal*Mathf.PI*3)*(1-reveal)*.12f;
                rewardFriends[i].localScale=new Vector3((.12f+.88f*spring)*(1-bounce),(.12f+.88f*spring)*(1+bounce),1);rewardSkins[i].Pose(t,false);
            }
            // The concept label is a child of the reward so it shares the reveal fade.
            rewardConcept.gameObject.SetActive(t>=1.88f);
            if(!drawReady){revealTitle.text=t<1.25f?"별빛이 모이고 있어요":"두근두근, 누구일까요?";revealMessage.text=t<1.88f?"캡슐 속에 누가 기다릴까요?":PuzzleCollection.Names[drawnSkin];}
            else revealMessage.text=PuzzleCollection.Names[drawnSkin];
            for(int i=0;i<3;i++)
            {
                float breath=Mathf.Sin(t*1.6f+i)*.035f;
                float ripple=Mathf.Clamp01((t-1.7f-i*.13f)/.8f);
                revealRings[i].rectTransform.localScale=Vector3.one*(1+breath+Mathf.Sin(ripple*Mathf.PI)*.32f);
                revealRings[i].color=new Color(.78f,.60f,.91f,drawReady?.15f:.15f+Mathf.Sin(ripple*Mathf.PI)*.36f);
            }
            for(int i=0;i<18;i++)
            {
                int source=i/6;float p=Mathf.Clamp01((t-source*.22f-(i%6)*.034f)/.75f);
                gatheringStars[i].gameObject.SetActive(t<1.55f&&p<1);
                float angle=source*Mathf.PI*2/3+Mathf.PI/2+p*1.7f;
                float r=132*(1-Mathf.SmoothStep(0,1,p));
                gatheringStars[i].anchoredPosition=new Vector2(cx+Mathf.Cos(angle)*r,158+Mathf.Sin(angle)*r);
                gatheringStars[i].localScale=Vector3.one*(1-p*.65f);gatheringStars[i].localEulerAngles=new Vector3(0,0,p*180);
                float burst=Mathf.Clamp01((t-1.85f)/1.25f);
                burstStars[i].gameObject.SetActive(t>=1.85f&&(t<3.1f||i%3==0));
                float a=i*Mathf.PI*2/18;float radius=drawReady?130:Mathf.Lerp(15,140,1-Mathf.Pow(1-burst,3));
                burstStars[i].anchoredPosition=new Vector2(cx+Mathf.Cos(a)*radius,160+Mathf.Sin(a)*radius);
                float sparkle=.75f+.25f*Mathf.Sin(t*2+i);
                burstStars[i].localScale=Vector3.one*(drawReady?.7f:sparkle);burstStars[i].localEulerAngles=new Vector3(0,0,i*23);
            }
            if(IsBatchDraw)RenderSkinBatch();
        }
        private void EquipDrawnSkin()
        {
            if(!drawReady)return;
            if(!collection.TryEquip(drawnSkin,Save)){revealSubtitle.text="착용을 저장하지 못했어요. 다시 눌러 주세요.";return;}
            board.ApplySkin(drawnSkin);RefreshHomeSkin();Tone(1.1f);
            OpenCollection(PuzzleCollection.Names[drawnSkin]+" 착용 완료!\n이 모습으로 출발해 볼까요?");
        }
        private void KeepDrawnSkin(){OpenCollection((IsBatchDraw?"새 친구 10명을 모두":PuzzleCollection.Names[drawnSkin]+"을")+" 옷장에 보관했어요.\n원할 때 선택하고 저장하면 착용돼요.");}
        private void BackFromSkinDraw(){if(!drawReady)FinishSkinDraw();else KeepDrawnSkin();}
    }
}
