using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp
    {
        private RectTransform home,homeContent;
        private RectTransform homeLogo;
        private Vector2 homeLogoOrigin;
        private readonly RectTransform[] titleGlints=new RectTransform[3];
        private readonly CanvasGroup[] titleGlintOpacity=new CanvasGroup[3];
        private readonly RectTransform[] homeFriends=new RectTransform[3];
        private readonly Vector2[] homeFriendOrigins=new Vector2[3];
        private float homeTime;

        private void OpenHome()
        {
            CloseDialog();DismissHome();CancelTouches();
            session.SetPause(PauseReason.Menu,true);
            session.SetPause(PauseReason.Levels,false);
            modal="home";homeTime=0;
            var background=PuzzleUI.Panel("Start screen",safe,PuzzleUI.Cream,false);
            background.raycastTarget=true;home=background.rectTransform;PuzzleUI.Fill(home);
            OrbitBackdrop.Add(home);
            homeContent=PuzzleUI.Rect("Start composition",home);
            homeContent.anchorMin=homeContent.anchorMax=new Vector2(.5f,.5f);
            homeContent.pivot=new Vector2(.5f,.5f);homeContent.sizeDelta=new Vector2(390,760);
            var settings=MakeButton(homeContent,"설정","설정",null,()=>Active&&modal=="home",OpenHomeSettings,new Color(.93f,.90f,.97f));
            PuzzleUI.Box((RectTransform)settings.transform,302,718,64,38);

            var outer=PuzzleUI.Glyph(homeContent,GlyphKind.Ring,new Color(.78f,.71f,.90f,.28f));
            PuzzleUI.Box(outer.rectTransform,-68,278,526,372);
            var inner=PuzzleUI.Glyph(homeContent,GlyphKind.Ring,new Color(.78f,.71f,.90f,.20f));
            PuzzleUI.Box(inner.rectTransform,40,324,310,192);
            HomeText("작은 우주에 모인 우리",691,28,14,PuzzleUI.Muted);
            var signature=PuzzleUI.Panel("Signature badge",homeContent,new Color(.92f,.86f,.98f));PuzzleUI.Box(signature.rectTransform,109,297,172,27);
            var signatureText=PuzzleUI.Label("Signature badge text",signature.transform,"손끝에서 만나는 말랑한 우주",11,PuzzleUI.Ink);PuzzleUI.Fill(signatureText.rectTransform);
            var logo=PuzzleUI.Rect("말랑궤도 title logo",homeContent).gameObject.AddComponent<RawImage>();
            logo.texture=brand.texture;logo.raycastTarget=false;
            PuzzleUI.Box(logo.rectTransform,20,572,350,350f*logo.texture.height/logo.texture.width);
            homeLogo=logo.rectTransform;
            homeLogo.pivot=new Vector2(.5f,.5f);
            homeLogoOrigin=new Vector2(195,572+homeLogo.sizeDelta.y/2);
            homeLogo.anchoredPosition=homeLogoOrigin;
            // Light the three glints already painted into the wordmark, in its own coordinates.
            Vector2[] glintAnchors={new Vector2(.038f,.53f),new Vector2(.513f,.875f),new Vector2(.974f,.51f)};
            for(int i=0;i<titleGlints.Length;i++)
            {
                var glint=PuzzleUI.Rect("Title soft glint "+i,homeLogo);
                glint.anchorMin=glint.anchorMax=glintAnchors[i];glint.pivot=new Vector2(.5f,.5f);
                glint.anchoredPosition=Vector2.zero;glint.sizeDelta=Vector2.one*(i==1?22:25);
                glint.gameObject.AddComponent<TitleGlint>().raycastTarget=false;
                titleGlints[i]=glint;
                titleGlintOpacity[i]=glint.gameObject.AddComponent<CanvasGroup>();titleGlintOpacity[i].blocksRaycasts=false;
            }
            HomeText("중력을 돌려, 우리 모두 제자리로",535,30,16,PuzzleUI.Ink);

            float[] xs={40,135,259},ys={363,389,354},sizes={98,121,90};
            for(int i=0;i<3;i++)
            {
                var landing=PuzzleUI.Glyph(homeContent,GlyphKind.Ring,new Color(PuzzleUI.Candy[i].r,PuzzleUI.Candy[i].g,PuzzleUI.Candy[i].b,.38f));
                PuzzleUI.Box(landing.rectTransform,xs[i]-8,ys[i]-25,sizes[i]+16,24);
                var friend=SkinDecoration.Portrait(homeContent,collection.selected,i,out homeSkins[i]);
                PuzzleUI.Box(friend,xs[i],ys[i],sizes[i],sizes[i]);
                homeFriends[i]=friend;homeFriendOrigins[i]=friend.anchoredPosition;
                var star=PuzzleUI.Glyph(homeContent,GlyphKind.Star,PuzzleUI.Candy[i]);
                PuzzleUI.Box(star.rectTransform,43+i*145,472+(i==1?21:-7),12,12);
            }
            
            HomeButton("게임 시작",211,StartFromHome,PuzzleUI.Accent);
            HomeButton("단계 선택",138,OpenLevels,PuzzleUI.Mint);
            HomeButton("스킨 뽑기  ·  ★ "+collection.wallet,65,()=>OpenCollection(),new Color(1,.89f,.67f));
            int next=session.Solved?Mathf.Min(levelIndex+1,levels.Length-1):levelIndex;
            HomeText((next+1)+"단계"+(session.Moves>0&&!session.Solved?"에서 이어서 출발해요":"로 출발해요"),25,25,12,PuzzleUI.Muted);
            LayoutHome();TickHome(0);Save();
        }

        private void HomeText(string value,float y,float height,int size,Color color)
        {
            var text=PuzzleUI.Label(value,homeContent,value,size,color);
            PuzzleUI.Box(text.rectTransform,20,y,350,height);
        }
        private void OpenHomeSettings()
        {
            session.SetPause(PauseReason.Menu,true);
            var card=OpenDialog("home-settings",390);
            DialogText(card,"설정",318,42,27);
            DialogText(card,"나에게 편한 속도로, 말랑하게",278,28,14,PuzzleUI.Muted);
            DialogButton(card,"효과음  "+(sound?"켜짐":"꺼짐"),212,()=>{ToggleSound();OpenHomeSettings();},new Color(.98f,.88f,.82f));
            DialogButton(card,"빠른 진행  "+(reduceMotion?"켜짐":"꺼짐"),150,()=>{ToggleFastPlay();OpenHomeSettings();});
            DialogText(card,"빠른 진행을 켜면 이동·전환 연출을 생략해요.\n장식 움직임도 멈추고, 설정은 자동 저장돼요.",88,48,13,PuzzleUI.Muted);
            DialogButton(card,"돌아가기",24,CloseHomeSettings,new Color(.93f,.90f,.97f));
            LayoutDialog();
        }
        private void CloseHomeSettings()
        {
            CloseDialog();modal="home";
        }
        private void ToggleSound()
        {
            sound=!sound;
            if(!sound&&audioSource)audioSource.Stop();
            Save();if(sound)Tone(1);
        }
        private void ToggleFastPlay()
        {
            reduceMotion=!reduceMotion;PuzzleUI.ReduceMotion=reduceMotion;Save();
        }
        private void HomeButton(string text,float y,UnityEngine.Events.UnityAction action,Color color)
        {
            var button=MakeButton(homeContent,text,text,null,()=>Active&&modal=="home",()=>{Tone(1);action();},color);
            PuzzleUI.Box((RectTransform)button.transform,50,y,290,60);
            foreach(var label in button.GetComponentsInChildren<Text>())label.fontSize=largeText?23:21;
        }
        private void LayoutHome()
        {
            if(!homeContent)return;
            homeContent.localScale=Vector3.one*Mathf.Min(safe.rect.width/390f,safe.rect.height/760f);
        }
        private void TickHome(float delta)
        {
            if(!home)return;
            if(!reduceMotion)homeTime+=Mathf.Min(delta,.05f);
            if(homeLogo)
            {
                float breath=reduceMotion?0:Mathf.Sin(homeTime*1.3f);
                homeLogo.localScale=new Vector3(1+breath*.009f,1-breath*.014f,1);
                homeLogo.anchoredPosition=homeLogoOrigin+new Vector2(0,reduceMotion?0:Mathf.Sin(homeTime*.85f)*1.5f);
                homeLogo.localRotation=Quaternion.Euler(0,0,reduceMotion?0:Mathf.Sin(homeTime*.72f)*.3f);
                for(int i=0;i<titleGlints.Length;i++)
                {
                    float glow=reduceMotion?0:Mathf.Pow(.5f+.5f*Mathf.Sin(homeTime*(1.05f+i*.13f)+i*2.1f),2);
                    titleGlintOpacity[i].alpha=reduceMotion?0:.12f+glow*.58f;
                    titleGlints[i].localScale=Vector3.one*(.86f+glow*.20f);
                }
            }
            for(int i=0;i<homeFriends.Length;i++)
            {
                if(!homeFriends[i])continue;
                float wave=reduceMotion?0:Mathf.Sin(homeTime*1.25f+i*1.8f);
                homeFriends[i].anchoredPosition=homeFriendOrigins[i]+new Vector2(0,wave*5);
                homeFriends[i].localScale=new Vector3(1-wave*.012f,1+wave*.012f,1);
                if(homeSkins[i])homeSkins[i].Pose(homeTime,reduceMotion);
            }
        }
        private void DismissHome()
        {
            if(home){home.gameObject.SetActive(false);Destroy(home.gameObject);}
            home=null;homeContent=null;homeLogo=null;
            for(int i=0;i<homeFriends.Length;i++)homeFriends[i]=null;
        }
        private void StartFromHome()
        {
            DismissHome();CloseDialog();
            session.SetPause(PauseReason.Menu,false);session.SetPause(PauseReason.Levels,false);
            if(session.Solved)LoadLevel(Mathf.Min(levelIndex+1,levels.Length-1));
            else PresentLevel();
        }
        private void SelectLevel(int index)
        {
            if(home)
            {
                DismissHome();CloseDialog();
                session.SetPause(PauseReason.Menu,false);session.SetPause(PauseReason.Levels,false);
                if(index==levelIndex&&!session.Solved)PresentLevel();else LoadLevel(index);
            }
            else if(index==levelIndex)CloseLevels();else LoadLevel(index);
        }
    }
}
