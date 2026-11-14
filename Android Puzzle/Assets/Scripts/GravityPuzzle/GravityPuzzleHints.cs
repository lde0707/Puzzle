using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp
    {
        private PuzzleHints hints=new PuzzleHints();
        private HintWallet hintWallet=HintWallet.Restore(null,HintWallet.Now);
        private long hintWalletTick;
        private float nextHintAdTime;
        private string earnedAdHintKey;
        private bool usedHint;
        private bool runAudit;
        private PuzzleHintSearch hintSearch;
        private string requestedHintState,shownHintState;
        private int hintedDirection=-1;
        private float hintAge;
        private PuzzleTap hintButton;
        private Text hintButtonLabel;
        private Text hintRechargeLabel;
        private readonly Image[] directionGlows=new Image[4];
        private PuzzleSnapshot hintSnapshot;
        private PuzzleDefinition hintVersionLevel;
        private string hintKey,hintVersion;
        private string HintVersion {get{if(hintVersionLevel!=session.Level){hintVersionLevel=session.Level;hintVersion=SaveVersion;}return hintVersion;}}
        private string HintState {get{if(hintSnapshot!=session.Stable){hintSnapshot=session.Stable;hintKey=PuzzleHintSearch.Key(session.Level,session.Stable.positions,session.Stable.sizes,session.Stable.moves);}return hintKey;}}
        private bool CanHint()=>Active&&modal==""&&session!=null&&session.Ready&&hintSearch==null;
        private void CreateHintsUI()
        {
            hintButton=MakeButton(safe,"Direction hint","",null,CanHint,RequestDirectionHint,new Color(1,.91f,.65f));
            hintButtonLabel=PuzzleUI.Label("Hint uses",hintButton.transform,"힌트 3/3",14,PuzzleUI.Ink);hintButtonLabel.fontStyle=FontStyle.Bold;PuzzleUI.Fill(hintButtonLabel.rectTransform,3);
            for(int i=0;i<4;i++)
            {
                var glow=PuzzleUI.Panel("Recommended direction glow",arrows[i].transform,new Color(1,.83f,.25f,0));
                PuzzleUI.Fill(glow.rectTransform,-5);glow.transform.SetAsFirstSibling();directionGlows[i]=glow;
                var rim=glow.gameObject.AddComponent<Outline>();rim.effectColor=new Color(1,.91f,.4f,.65f);rim.effectDistance=new Vector2(2,-2);
                glow.gameObject.SetActive(false);
            }
        }
        private void RequestDirectionHint()
        {
            if(!CanHint())return;
            requestedHintState=HintState;
            int known=hints.Known(session.Level.room,HintVersion,requestedHintState);
            if(known>=0){ShowDirectionHint(known);return;}
            hintSearch=new PuzzleHintSearch(session.Level,session.Stable);
            hint.text="별빛이 다음 길을 찾고 있어요…";
        }
        private void ShowDirectionHint(int direction)
        {
            usedHint=true;Save();
            hintedDirection=direction;shownHintState=HintState;hintAge=0;
            hint.text=new[]{"아래","왼쪽","위","오른쪽"}[direction]+"쪽으로 데굴해 볼까요?";Tone(1.25f);
        }
        private void CancelDirectionHint()
        {
            hintSearch=null;hintedDirection=-1;shownHintState=null;
            foreach(var glow in directionGlows)if(glow)glow.gameObject.SetActive(false);
        }
        private void TickDirectionHint(float delta)
        {
            long now=HintWallet.Now;
            if(hintWalletTick!=now){hintWalletTick=now;if(hintWallet.Recharge(now))Save();}
            if(hintRechargeLabel){long left=hintWallet.SecondsUntilNext(now);hintRechargeLabel.text="다음 충전까지 "+(left/60).ToString("00")+":"+(left%60).ToString("00");}
            if(hintButtonLabel)
            {
                int remaining=hintWallet.balance;
                long seconds=hintWallet.SecondsUntilNext(now);
                string timer=remaining<3?"\n"+(seconds/60).ToString("00")+":"+(seconds%60).ToString("00"):"";
                hintButtonLabel.text=hintSearch!=null?"찾는 중…":session.Ready&&hints.Known(session.Level.room,HintVersion,HintState)>=0?"힌트 다시보기":"힌트 "+remaining+"/3"+timer;
                hintButtonLabel.fontSize=largeText?14:13;
            }
            if(hintSearch!=null&&Active&&modal=="")
            {
                if(!session.Ready||HintState!=requestedHintState){CancelDirectionHint();return;}
                hintSearch.Step();
                if(hintSearch.Done)
                {
                    var result=hintSearch;hintSearch=null;
                    if(result.Route==null||result.Route.Length==0)
                        hint.text=result.Exhausted?"이 배치에서는 길이 없어요. 되돌리기를 해보세요.\n힌트 횟수는 사용하지 않았어요.":"다음 길을 확인하지 못했어요.\n힌트 횟수는 사용하지 않았어요.";
                    else if(hintWallet.balance>0||earnedAdHintKey==session.Level.room+"/"+HintState)CommitAccountHint(result.Route[0],earnedAdHintKey==session.Level.room+"/"+HintState);
                    else OfferHintAd(result.Route[0]);
                }
            }
            if(hintedDirection<0)return;
            if(session.Moving||session.Solved||HintState!=shownHintState){CancelDirectionHint();return;}
            if(Active&&modal=="")hintAge+=delta;
            for(int i=0;i<4;i++)
            {
                bool show=i==hintedDirection&&modal=="";directionGlows[i].gameObject.SetActive(show);
                if(show)directionGlows[i].color=new Color(1,.84f,.3f,reduceMotion?.55f:.30f+.28f*(.5f+.5f*Mathf.Sin(hintAge*5)));
            }
        }
        private void CommitAccountHint(int direction,bool adReward)
        {
            if(HintState!=requestedHintState)return;
            bool oldUsed=usedHint;var record=activity.For(session.Level.room);int oldCount=record.hints;
            usedHint=true;record.hints++;
            bool ok=hints.Reveal(session.Level.room,HintVersion,HintState,direction,
                ()=>adReward?Save():hintWallet.Spend(HintWallet.Now,Save),true);
            if(ok){earnedAdHintKey=null;ShowDirectionHint(direction);}
            else{usedHint=oldUsed;record.hints=oldCount;hint.text="힌트를 저장하지 못했어요. 다시 눌러 주세요.";}
        }
        private void OfferHintAd(int direction)
        {
            string state=HintState,room=session.Level.room;
            session.SetPause(PauseReason.Menu,true);
            var card=OpenDialog("hint-offer",380);
            DialogText(card,"힌트 충전을 기다리고 있어요",308,45,21);
            DialogText(card,"힌트는 계정 전체에서 함께 써요.\n1시간마다 1개 · 최대 3개",240,57,14,PuzzleUI.Muted);
            long seconds=hintWallet.SecondsUntilNext(HintWallet.Now);
            hintRechargeLabel=PuzzleUI.Label("Hint recharge",card,"다음 충전까지 "+(seconds/60).ToString("00")+":"+(seconds%60).ToString("00"),14,PuzzleUI.Ink);
            PuzzleUI.Box(hintRechargeLabel.rectTransform,18,202,card.sizeDelta.x-36,28);
            bool available=CrazyPortal.AdsEnabled&&Time.realtimeSinceStartup>=nextHintAdTime;
            if(available)
                DialogButton(card,"광고 보고 힌트 1개 받기",122,()=>BeginPortalReward(success=>{
                    nextHintAdTime=Time.realtimeSinceStartup+60;
                    if(success&&session.Level.room==room&&HintState==state){earnedAdHintKey=room+"/"+state;CommitAccountHint(direction,true);}
                    else hint.text="광고를 완료하지 못했어요. 힌트는 사용하지 않았어요.";
                }),PuzzleUI.Mint);
            else DialogText(card,"지금은 자동 충전을 기다려 주세요.",125,50,14,PuzzleUI.Muted);
            DialogButton(card,"계속 플레이하기",52,Resume,PuzzleUI.Mint);LayoutDialog();
        }
    }
}
