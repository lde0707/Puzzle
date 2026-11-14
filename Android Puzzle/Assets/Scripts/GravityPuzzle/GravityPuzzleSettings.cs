using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp
    {
        private bool settingsFromHome;
        private int settingsTab;
        private void ShowSettings(int tab)
        {
            settingsTab=tab;
            session.SetPause(PauseReason.Menu,true);
            var card=OpenDialog("home-settings",560);
            card.GetComponent<Image>().color=new Color(.98f,.96f,.99f);
            var rim=PuzzleUI.Panel("Lilac settings header",card,new Color(.88f,.82f,.95f));
            PuzzleUI.Box(rim.rectTransform,14,446,332,98);
            DialogText(card,"설정",490,38,27);
            DialogText(card,"나에게 편한 속도로, 말랑하게",458,28,13,PuzzleUI.Muted);
            string[] tabs={"플레이","언어","기록"};
            for(int i=0;i<3;i++)
            {
                int selected=i;
                var b=MakeButton(card,"Settings tab "+i,tabs[i],null,()=>Active,()=>ShowSettings(selected),i==tab?PuzzleUI.Accent:new Color(.92f,.88f,.96f));
                PuzzleUI.Box((RectTransform)b.transform,24+i*105,386,102,44);
            }
            if(tab==0)
            {
                SettingsRow(card,"배경음악",318,!musicMuted,()=>{ToggleMusic();ShowSettings(0);});
                SettingsRow(card,"효과음",250,sound,()=>{ToggleSound();ShowSettings(0);});
                SettingsRow(card,"빠른 진행",182,reduceMotion,()=>{ToggleFastPlay();ShowSettings(0);});
                SettingsRow(card,"큰 글씨",114,largeText,()=>{largeText=!largeText;Save();Layout();ShowSettings(0);});
            }
            else if(tab==1)
            {
                DialogText(card,"원하는 언어를 골라 주세요",331,34,17);
                for(int i=0;i<4;i++)
                {
                    string code=PuzzleLanguage.Codes[i];
                    var b=MakeButton(card,"Language "+code,PuzzleLanguage.Names[i],null,()=>Active,()=>{PuzzleLanguage.Set(code);RefreshBrandLanguage();ShowSettings(1);},code==PuzzleLanguage.Code?new Color(.78f,.90f,.86f):new Color(.92f,.87f,.96f));
                    PuzzleUI.Box((RectTransform)b.transform,24+(i%2)*160,244-(i/2)*76,152,62);
                    if(code==PuzzleLanguage.Code){var check=PuzzleUI.Glyph(b.transform,GlyphKind.Check,PuzzleUI.Ink);PuzzleUI.Box(check.rectTransform,128,23,16,16);}
                    // Autonyms must remain readable before choosing their language.
                    foreach(var label in b.GetComponentsInChildren<Text>())label.font=Resources.Load<Font>("GravityPuzzle/"+(code=="ja"?"DegulCJK-JP":code=="zh"?"DegulCJK-SC":"NanumGothic-Regular"));
                }
                DialogText(card,"바로 적용되고 다음에도 기억해요",105,42,13,PuzzleUI.Muted);
            }
            else
            {
                DialogText(card,"소중한 기록을 함께 데려가요",315,42,20);
                DialogText(card,"단계·스킨·별을 파일로 보관하고 이어받아요.",237,65,15,PuzzleUI.Muted);
                DialogButton(card,"기록 옮기기 · 저장 파일",164,OpenBackup,PuzzleUI.Mint);
                DialogText(card,"이 기기에 자동으로 저장돼요",100,42,13,PuzzleUI.Muted);
            }
            DialogButton(card,"돌아가기",30,CloseHomeSettings,new Color(.97f,.84f,.75f));LayoutDialog();
        }
        private void SettingsRow(RectTransform card,string label,float y,bool enabled,System.Action change)
        {
            var surface=PuzzleUI.Panel("Settings row",card,Color.white);PuzzleUI.Box(surface.rectTransform,24,y,312,56);
            var text=PuzzleUI.Label("Setting label",surface.transform,label,18,PuzzleUI.Ink,TextAnchor.MiddleLeft);PuzzleUI.Box(text.rectTransform,15,0,185,56);
            var button=MakeButton(surface.transform,"Toggle "+label,enabled?"켜짐":"꺼짐",null,()=>Active,change,enabled?new Color(.73f,.88f,.82f):new Color(.88f,.86f,.92f));
            PuzzleUI.Box((RectTransform)button.transform,214,8,88,40);
        }
        private void RefreshBrandLanguage()
        {
            if(!homeLogo)return;
            var image=homeLogo.GetComponent<RawImage>();
            image.texture=BrandArt.Load("GravityPuzzle/Brand/degul-degul-wordmark-"+(PuzzleLanguage.Code=="ko"?"ko":"en"));
            var subtitle=homeLogo.Find("Degul Degul English title");if(subtitle)subtitle.gameObject.SetActive(PuzzleLanguage.Code=="ko");
        }
    }
}
