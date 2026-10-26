using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using Puzzle.UI;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp : MonoBehaviour
    {
        private static readonly string[] Names={"작은 첫걸음","모퉁이 산책","둘이서 함께","크고 작은 친구","엇갈린 길","셋의 약속","다시 만난 길","좁은 틈 사이","차례를 기다려","서로 다른 길","한 박자 쉬고","모두의 자리","빙글 돌아서","마지막 모퉁이","가벼운 산책","아주 특별한 날"};
        private PuzzleDefinition[] levels;
        private readonly Dictionary<string,string> levelNames=new Dictionary<string,string>();
        private PuzzleSession session;
        private ProgressSaveService saves;
        private string version;
        private readonly HashSet<string> completed=new HashSet<string>();
        private int levelIndex, epoch, baseLevelCount;
        private readonly HashSet<string> seenLessons=new HashSet<string>();
        private string SaveVersion => ProgressSaveService.LevelVersion(session.Level,version);
        private bool reduceMotion, largeText, sound=true, focused=true, appPaused;
        private RectTransform safe, boardRoot, dialog;
        private BoardView board;
        private RawImage brand;
        private Text title,subtitle,moves,arrivals,hint,saveLabel,centerLabel;
        private Image stats;
        private PuzzleGlyph gravity;
        private PuzzleTap pauseButton,levelButton,undoButton,resetButton,ruleButton;
        private readonly PuzzleTap[] arrows=new PuzzleTap[4];
        private string modal="";
        private bool resetFromPause, solvedPresented;
        private Vector2 lastSize;
        private AudioSource audioSource;
        private AudioClip clickClip,completeClip;
        private AudioSource rewardSource;
        private AudioClip[] rewardClips;
        private AudioClip rewardFinale;
        private float finishDelay=-1,dialogAge;
        private PuzzleCelebration celebration;
        private CompletionStars completionStars;
        private WalletStarFlight walletFlight;
        private RectTransform walletBadge, walletIcon;
        private Text walletLabel;
        private int pendingWalletStars, shownWallet=-1;
        private float walletPulse;
        private bool celebrationPending;
        private Vector3 dialogScale=Vector3.one;
        private int shownMoves=-1,shownDirection=-1,shownArrivals=-1;
        private bool shownMoving;
        private bool Active => focused && !appPaused;

        private void Start()
        {
            PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
            var asset=Resources.Load<TextAsset>("GravityPuzzle/layouts");
            version=ProgressSaveService.Checksum(asset.text);
            levels=JsonUtility.FromJson<LayoutDocument>(asset.text).layouts;
            baseLevelCount=levels.Length;
            for(int i=0;i<baseLevelCount;i++)levelNames[levels[i].room]=i<Names.Length?Names[i]:levels[i].displayName;
            var late=Resources.Load<TextAsset>("GravityPuzzle/late_levels");
            if(late)levels=levels.Concat(JsonUtility.FromJson<LayoutDocument>(late.text).layouts).ToArray();
            var resize=Resources.Load<TextAsset>("GravityPuzzle/resize_levels");
            if(resize)levels=levels.Concat(JsonUtility.FromJson<LayoutDocument>(resize.text).layouts).ToArray();
            var blink=Resources.Load<TextAsset>("GravityPuzzle/blink_levels");
            if(blink)levels=levels.Concat(JsonUtility.FromJson<LayoutDocument>(blink.text).layouts).ToArray();
            foreach(var level in levels) level.Validate();
            levels=PuzzleCampaign.Order(levels);
            saves=new ProgressSaveService(ProgressSaveService.DefaultDirectory);
            var saved=saves.Load();
            if(saved!=null)
            {
                foreach(var id in saved.seenLessons??new string[0])seenLessons.Add(id);
                reduceMotion=saved.reduceMotion;largeText=saved.largeText;sound=saved.sound;
                foreach(var id in saved.completed??new string[0]) if(levels.Any(l=>l.room==id)) completed.Add(id);
                levelIndex=Array.FindIndex(levels,l=>l.room==saved.levelId);
                if(levelIndex<0)levelIndex=saved.levelId!=null&&(saved.levelId.StartsWith("P")||saved.levelId.StartsWith("S"))?baseLevelCount:0;
            }
            PuzzleUI.ReduceMotion=reduceMotion;
            collection=PuzzleCollection.Restore(saved?.collection,completed,levels.Select(l=>l.room));
            CreateUI(); CreateSound();
            board.ApplySkin(collection.selected);
            LoadLevel(levelIndex,saved,false);
            OpenHome();
        }
        private void CreateUI()
        {
            var canvasObject=new GameObject("말랑궤도 Canvas",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform,false);
            canvasObject.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
            var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(390,844);scaler.matchWidthOrHeight=1f;
            var background=PuzzleUI.Panel("Warm paper",canvasObject.transform,PuzzleUI.Cream,false);PuzzleUI.Fill(background.rectTransform);
            OrbitBackdrop.Add(background.transform);
            for(int i=0;i<3;i++)
            {
                var orbit=PuzzleUI.Glyph(canvasObject.transform,GlyphKind.Ring,new Color(.74f,.65f,.90f,.15f));
                orbit.rectTransform.anchorMin=orbit.rectTransform.anchorMax=new Vector2(i==1?1:0,i==2?.15f:.85f);
                orbit.rectTransform.sizeDelta=new Vector2(340+i*120,340+i*120);
            }
            safe=PuzzleUI.Rect("Safe area",canvasObject.transform);PuzzleUI.Fill(safe);safe.gameObject.AddComponent<SafeAreaFitter>();
            if(!FindFirstObjectByType<EventSystem>()) new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
            brand=PuzzleUI.Rect("말랑궤도 Logo",safe).gameObject.AddComponent<RawImage>();
            brand.texture=Resources.Load<Texture2D>("GravityPuzzle/Brand/mallang-orbit-wordmark");
            brand.raycastTarget=false;
            brand.gameObject.SetActive(false);
            title=PuzzleUI.Label("Level title",safe,"",27,PuzzleUI.Ink,TextAnchor.MiddleLeft);title.fontStyle=FontStyle.Bold;
            title.alignment=TextAnchor.UpperLeft;title.resizeTextForBestFit=true;title.resizeTextMinSize=20;
            subtitle=PuzzleUI.Label("Subtitle",safe,"",12,PuzzleUI.Muted,TextAnchor.MiddleLeft);
            stats=PuzzleUI.Panel("Status strip",safe,new Color(.90f,.86f,.96f));
            moves=PuzzleUI.Label("Moves",stats.transform,"",14,PuzzleUI.Ink,TextAnchor.MiddleLeft);
            arrivals=PuzzleUI.Label("Arrivals",stats.transform,"",13,PuzzleUI.Ink);
            gravity=PuzzleUI.Glyph(stats.transform,GlyphKind.Down,PuzzleUI.Ink);
            boardRoot=PuzzleUI.Rect("Board",safe);board=new BoardView(boardRoot);
            hint=PuzzleUI.Label("Hint",safe,"",12,PuzzleUI.Muted);
            board.OnHint=text=>hint.text=text;
            centerLabel=PuzzleUI.Label("Together",safe,"작은 우주\n함께 착륙",10,PuzzleUI.Muted);
            ruleButton=MakeButton(safe,"Portal rules","별문\n규칙",null,()=>Active&&modal=="",OpenMechanicHelp,new Color(.90f,.86f,.96f));
            saveLabel=PuzzleUI.Label("Save status",safe,"이 기기에 자동으로 저장돼요",10,PuzzleUI.Muted);
            pauseButton=MakeButton(safe,"Pause","",GlyphKind.Pause,()=>Active&&modal=="",OpenPause,PuzzleUI.Cream);
            levelButton=MakeButton(safe,"Levels","",GlyphKind.Grid,()=>Active&&modal=="",OpenLevels,PuzzleUI.Cream);
            walletBadge=PuzzleUI.Panel("Owned stars",safe,new Color(1,.92f,.74f)).rectTransform;
            walletIcon=PuzzleUI.Rect("Wallet star",walletBadge);PuzzleUI.Box(walletIcon,7,4,25,25);
            walletIcon.gameObject.AddComponent<PuffyStar>().raycastTarget=false;
            walletLabel=PuzzleUI.Label("Owned star count",walletBadge,"",18,PuzzleUI.Ink,TextAnchor.MiddleRight);walletLabel.fontStyle=FontStyle.Bold;
            PuzzleUI.Box(walletLabel.rectTransform,34,0,57,32);walletLabel.resizeTextForBestFit=true;walletLabel.resizeTextMinSize=12;walletLabel.resizeTextMaxSize=18;
            undoButton=MakeButton(safe,"Undo","되돌리기",null,()=>Active&&modal==""&&session!=null&&session.History.Count>0,Undo,new Color(.90f,.86f,.96f));
            resetButton=MakeButton(safe,"Reset","처음부터",null,()=>Active&&modal=="",AskReset,new Color(1f,.88f,.81f));
            GlyphKind[] kinds={GlyphKind.Down,GlyphKind.Left,GlyphKind.Up,GlyphKind.Right};
            for(int i=0;i<4;i++) { int d=i;arrows[i]=MakeButton(safe,"Direction "+i,"",kinds[i],()=>Active&&modal==""&&session!=null&&session.Ready,()=>Tilt(d),PuzzleUI.Mint); }
        }
        private PuzzleTap MakeButton(Transform parent,string name,string label,GlyphKind? icon,Func<bool> canUse,Action action,Color color)
        {
            var image=PuzzleUI.Panel(name,parent,color);image.raycastTarget=true;
            var depth=image.gameObject.AddComponent<Shadow>();depth.effectColor=new Color(.24f,.17f,.37f,.20f);depth.effectDistance=new Vector2(0,-4);
            var edge=image.gameObject.AddComponent<Outline>();edge.effectColor=new Color(.39f,.25f,.53f,.16f);edge.effectDistance=new Vector2(.65f,-.65f);
            var sheen=PuzzleUI.Panel("Satin top edge",image.transform,new Color(1,1,1,.38f));
            sheen.rectTransform.anchorMin=new Vector2(.10f,1);sheen.rectTransform.anchorMax=new Vector2(.90f,1);sheen.rectTransform.pivot=new Vector2(.5f,1);sheen.rectTransform.sizeDelta=new Vector2(0,1.5f);sheen.rectTransform.anchoredPosition=new Vector2(0,-2.5f);
            var tap=image.gameObject.AddComponent<PuzzleTap>();tap.CanUse=canUse;tap.Epoch=()=>epoch;tap.Activate=action;tap.surface=image;tap.normal=color;
            if(icon.HasValue)
            {
                var glyph=PuzzleUI.Glyph(image.transform,icon.Value,PuzzleUI.Ink);
                if(string.IsNullOrEmpty(label)) { glyph.rectTransform.anchorMin=new Vector2(.25f,.25f);glyph.rectTransform.anchorMax=new Vector2(.75f,.75f); }
                else { glyph.rectTransform.anchorMin=new Vector2(.09f,.24f);glyph.rectTransform.anchorMax=new Vector2(.30f,.76f); }
                glyph.rectTransform.offsetMin=glyph.rectTransform.offsetMax=Vector2.zero;
            }
            if(!string.IsNullOrEmpty(label))
            {
                var text=PuzzleUI.Label("Label",image.transform,label,largeText?16:14,PuzzleUI.Ink);
                text.fontStyle=FontStyle.Bold;PuzzleUI.Fill(text.rectTransform,4);if(icon.HasValue)text.rectTransform.offsetMin=new Vector2(30,4);
            }
            return tap;
        }
        private void Layout()
        {
            Canvas.ForceUpdateCanvases();
            float h=safe.rect.height,w=Mathf.Min(440,safe.rect.width),left=(safe.rect.width-w)/2;
            float f=Mathf.Clamp(h/780,.75f,1);
            PuzzleUI.Box(title.rectTransform,left+24,h-80,w-166,68);
            PuzzleUI.Box(subtitle.rectTransform,left+24,h-119,w-48,24);
            // Horizontal touch targets, with a separate wallet badge underneath.
            PuzzleUI.Box((RectTransform)levelButton.transform,left+w-124,h-52,44,44);
            PuzzleUI.Box((RectTransform)pauseButton.transform,left+w-68,h-52,44,44);
            PuzzleUI.Box(walletBadge,left+w-124,h-96,100,32);
            PuzzleUI.Box(stats.rectTransform,left+24,h-158,w-48,36);
            PuzzleUI.Box(moves.rectTransform,14,0,110,36);
            PuzzleUI.Box(arrivals.rectTransform,115,0,w-235,36);
            PuzzleUI.Box(gravity.rectTransform,w-85,7,22,22);
            float bottom=304*f,top=h-180;
            board.Layout(w-64,Mathf.Max(80,top-bottom),left+w/2,(top+bottom)/2);
            PuzzleUI.Box(hint.rectTransform,left+16,268*f,w-32,28);
            float side=62*f,gap=72*f,cx=left+w/2;
            PuzzleUI.Box((RectTransform)arrows[2].transform,cx-side/2,199*f,side,side);
            PuzzleUI.Box((RectTransform)arrows[1].transform,cx-gap-side/2,129*f,side,side);
            PuzzleUI.Box((RectTransform)arrows[3].transform,cx+gap-side/2,129*f,side,side);
            PuzzleUI.Box((RectTransform)arrows[0].transform,cx-side/2,59*f,side,side);
            PuzzleUI.Box(centerLabel.rectTransform,cx-side/2,129*f,side,side);
            PuzzleUI.Box((RectTransform)ruleButton.transform,cx-side/2,129*f,side,side);
            PuzzleUI.Box((RectTransform)undoButton.transform,left+24,14,116,40);
            PuzzleUI.Box((RectTransform)resetButton.transform,left+w-140,14,116,40);
            PuzzleUI.Box(saveLabel.rectTransform,left+24,0,w-48,12);
            title.fontSize=largeText?29:27;subtitle.fontSize=largeText?14:12;hint.fontSize=largeText?14:12;
            title.resizeTextMaxSize=title.fontSize;
            moves.fontSize=largeText?16:14;arrivals.fontSize=largeText?15:13;
            foreach(var label in undoButton.GetComponentsInChildren<Text>())label.fontSize=largeText?16:14;
            foreach(var label in resetButton.GetComponentsInChildren<Text>())label.fontSize=largeText?16:14;
            lastSize=safe.rect.size;
            LayoutHome();
            if(dialog) LayoutDialog();
        }
        private void Update()
        {
            if(session==null) return;
            if(lastSize!=safe.rect.size) Layout();
            TickHome(Active?Time.unscaledDeltaTime:0);
            TickCollection(Active?Mathf.Min(Time.unscaledDeltaTime,.05f):0);
            if(session.Tick(Time.unscaledDeltaTime,reduceMotion))
            {
                CancelTouches();Save();if(session.Solved)QueueComplete();else Tone(.85f);
            }
            float visualDelta=Active?Mathf.Min(Time.unscaledDeltaTime,.05f):0;
            RefreshWallet();
            walletPulse=Mathf.Max(0,walletPulse-visualDelta);
            walletIcon.localScale=Vector3.one*(1+.22f*Mathf.Sin(Mathf.Clamp01(walletPulse/.3f)*Mathf.PI));
            board.Render(session,visualDelta,reduceMotion);RefreshStatus();
            if(finishDelay>=0 && !session.Paused && Active)
            {
                finishDelay-=visualDelta;
                if(finishDelay<=0){finishDelay=-1;if(session.Solved)Complete();}
            }
            if(dialog && Active)
            {
                dialogAge+=visualDelta;
                var card=(RectTransform)dialog.GetChild(0);
                float t=reduceMotion?1:Mathf.Clamp01(dialogAge/.32f);
                float eased=1-Mathf.Pow(1-t,3);
                card.localScale=dialogScale*(.94f+.06f*eased);
                card.anchoredPosition=new Vector2(0,-16*(1-eased));
                celebration?.Tick(visualDelta,reduceMotion);
                completionStars?.Tick(visualDelta,reduceMotion);
                walletFlight?.Tick(visualDelta,reduceMotion);
            }
            var keyboard=Keyboard.current;
            if(keyboard==null || !Active) return;
            if(keyboard.escapeKey.wasPressedThisFrame)
            {
                if(modal=="home")return;
                if(modal=="skin-draw")BackFromSkinDraw();else if(modal=="skins")CloseCollection();else if(modal=="home-settings")CloseHomeSettings();else if(modal=="pause"||modal=="rules")Resume();else if(modal=="reset")CancelReset();else if(modal=="levels")CloseLevels();else if(modal=="")OpenPause();
                return;
            }
            if(modal!="")return;
            if(keyboard.zKey.wasPressedThisFrame)Undo();
            else if(keyboard.rKey.wasPressedThisFrame)AskReset();
            else if(keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)Tilt(2);
            else if(keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)Tilt(1);
            else if(keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)Tilt(0);
            else if(keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)Tilt(3);
        }
        private void RefreshStatus()
        {
            if(shownMoves==session.Moves && shownDirection==session.Direction && shownMoving==session.Moving && hint.text!="")return;
            int count=Enumerable.Range(0,session.Level.cargo.Length).Count(i=>session.Stable.positions[i].Equals(session.Level.Goal[i])&&session.Stable.sizes[i]==session.Level.GoalSizes[i]);
            if(shownMoves!=session.Moves){shownMoves=session.Moves;moves.text="이동  "+shownMoves.ToString("00");}
            if(shownArrivals!=count){shownArrivals=count;arrivals.text="도착  "+count+" / "+session.Level.cargo.Length;}
            if(shownDirection!=session.Direction)
            {
                shownDirection=session.Direction;gravity.kind=new[]{GlyphKind.Down,GlyphKind.Left,GlyphKind.Up,GlyphKind.Right}[shownDirection];gravity.SetVerticesDirty();
                for(int i=0;i<4;i++)arrows[i].normal=i==shownDirection?PuzzleUI.Accent:PuzzleUI.Mint;
            }
            if(shownMoving!=session.Moving || hint.text=="")
            {
                shownMoving=session.Moving;
                hint.text=session.Solved?"작은 우주에, 모두 사뿐히!":session.Moving?"같은 중력, 함께 데굴!":session.Level.HasPortals?(session.Plan!=null&&session.Plan.HasBlocked?"출구가 막혔어요. 친구를 먼저 비켜 주세요.":"빛나는 입구로 진입 · 누르면 출구가 빛나요"):session.Level.family==0?"위쪽 길을 먼저 찾아볼까요?":session.Level.family==3?"큰 친구는 넓은 길이 필요해요.":"친구와 같은 색 자리로, 모두 함께!";
                if(session.Level.HasBlinkBlocks&&!session.Solved&&!session.Moving)
                    hint.text=session.Level.blinkBlocks.Any(b=>b.phase==1)?((session.Moves&1)==0?"● 살구 닫힘 · ●● 민트 열림 · 다음 턴 교대":"● 살구 열림 · ●● 민트 닫힘 · 다음 턴 교대"):((session.Moves&1)==0?"● 살구 닫힘 · 한 턴 뒤 점선 길이 열려요":"● 살구 열림 · 한 턴 뒤 블록이 돌아와요");
                if(session.Level.HasResizers&&!session.Level.HasBlinkBlocks&&!session.Solved&&!session.Moving)
                    hint.text=session.Plan!=null&&session.Plan.resizeBlocked.Any(b=>b)?"커질 자리가 막혔어요. 친구를 먼저 비켜 주세요.":"변환대에서 멈춰요 · 목적지의 색과 크기를 맞춰요";
            }
        }
        private void LoadLevel(int index,ProgressData saved=null,bool present=true)
        {
            celebrationPending=false;finishDelay=-1;levelIndex=index;solvedPresented=false;earnedStars=0;CloseDialog();session=new PuzzleSession(levels[index]);
            if(saved!=null)
            {
                ProgressSaveService.Restore(saved,SaveVersion,session);solvedPresented=session.Solved;
                // Recover a clear saved just before reward processing, without paying twice.
                if(session.Solved){completed.Add(session.Level.room);earnedStars=collection.Award(session.Level.room,session.Moves,session.Level.route.Length,session.UsedUndo);}
            }
            if(!Active)session.SetPause(PauseReason.Inactive,true);
            CancelTouches();board.Build(session.Level);
            title.text=LevelName(session.Level);
            subtitle.text="ORBIT "+(index+1).ToString("00")+" / "+levels.Length+"   ·   "+(session.Level.HasPortals?session.Level.width+"×"+session.Level.height+" · 별문 "+session.Level.portals.Length+"쌍":session.Level.cargo.Length==1?"첫 번째 작은 우주":"말랑 친구들의 우주 산책");
            if(session.Level.HasResizers)subtitle.text="ORBIT "+(index+1).ToString("00")+" / "+levels.Length+"   ·   크기 변환"+(session.Level.HasPortals?" + 별문":"");
            if(session.Level.HasBlinkBlocks)subtitle.text="ORBIT "+(index+1).ToString("00")+" / "+levels.Length+"   ·   숨바꼭질"+(session.Level.HasResizers?" + 변신":session.Level.HasPortals?" + 별문":"");
            bool special=session.Level.HasBlinkBlocks||session.Level.HasPortals||session.Level.HasResizers;
            ruleButton.gameObject.SetActive(special);centerLabel.gameObject.SetActive(!special);
            foreach(var label in ruleButton.GetComponentsInChildren<Text>())label.text=session.Level.HasBlinkBlocks?"길바꿈\n규칙":session.Level.HasResizers?"변신\n규칙":"별문\n규칙";
            shownMoves=shownDirection=shownArrivals=-1;hint.text="";Layout();board.Render(session);RefreshStatus();Save();
            if(present)PresentLevel();
        }
        private void PresentLevel()
        {
            bool special=session.Level.HasBlinkBlocks||session.Level.HasPortals||session.Level.HasResizers;
            if(session.Solved)Complete();
            else if(special&&seenLessons.Add(session.Level.room)){Save();OpenMechanicHelp();}
        }
        private void Tilt(int direction)
        {
            if(modal!="" || !Active || !session.Command(direction))return;
            board.BeginTilt(session);hint.text="";CancelTouches();Tone(1.1f);
            if(!session.Moving){Save();if(session.Solved)QueueComplete();}
        }
        private void Undo()
        {
            if(!session.Undo())return;
            SettleWallet();
            celebrationPending=false;finishDelay=-1;board.CancelMotion();solvedPresented=false;CancelTouches();Save();Tone(.85f);hint.text="";
        }
        private void CancelTouches(){epoch++;PuzzleTap.CancelAll();}
        private bool Save()
        {
            var data=new ProgressData { dataVersion=SaveVersion,seenLessons=seenLessons.OrderBy(id=>id).ToArray(),levelId=session.Level.room,current=session.Stable.Copy(),history=session.StableHistory(),
                blockIds=Enumerable.Range(0,session.Level.cargo.Length).ToArray(),sizes=session.Level.cargo.Select(c=>c.size).ToArray(),
                completed=completed.OrderBy(id=>id).ToArray(),reduceMotion=reduceMotion,largeText=largeText,sound=sound,collection=collection,usedUndo=session.UsedUndo };
            bool ok=saves.Save(data);
            saveLabel.text=ok?"이 기기에 자동으로 저장돼요":"저장하지 못했어요 · 저장 공간을 확인해 주세요";
            return ok;
        }
        private RectTransform OpenDialog(string type,float height)
        {
            CloseDialog(type!="solved");CancelTouches();modal=type;dialogAge=0;
            var overlay=PuzzleUI.Panel("Modal overlay",safe,new Color(.17f,.10f,.28f,.44f),false);PuzzleUI.Fill(overlay.rectTransform);overlay.raycastTarget=true;dialog=overlay.rectTransform;
            var panel=PuzzleUI.Panel("Dialog card",dialog,PuzzleUI.Cream);
            var border=panel.gameObject.AddComponent<Outline>();border.effectColor=new Color(1,1,1,.7f);border.effectDistance=new Vector2(1.25f,-1.25f);
            var depth=panel.gameObject.AddComponent<Shadow>();depth.effectColor=new Color(.16f,.08f,.27f,.25f);depth.effectDistance=new Vector2(0,-7);
            panel.rectTransform.sizeDelta=new Vector2(Mathf.Min(360,safe.rect.width-32),height);panel.rectTransform.anchorMin=panel.rectTransform.anchorMax=new Vector2(.5f,.5f);panel.rectTransform.pivot=new Vector2(.5f,.5f);
            return panel.rectTransform;
        }
        private void LayoutDialog()
        {
            var card=(RectTransform)dialog.GetChild(0);float max=Mathf.Min(360,safe.rect.width-32);
            dialogScale=Vector3.one*Mathf.Min(1,(safe.rect.height-30)/card.sizeDelta.y,max/card.sizeDelta.x);card.localScale=dialogScale;
        }
        private void CloseDialog(bool settleWallet=true)
        {
            if(rewardSource)rewardSource.Stop();
            if(settleWallet)SettleWallet();
            if(dialog){dialog.gameObject.SetActive(false);Destroy(dialog.gameObject);dialog=null;}
            celebration=null;completionStars=null;walletFlight=null;modal="";CancelTouches();
        }
        private void RefreshWallet()
        {
            if(!walletLabel||collection==null)return;
            int visible=Mathf.Max(0,collection.wallet-pendingWalletStars);
            if(shownWallet==visible)return;
            shownWallet=visible;walletLabel.text=visible.ToString();
        }
        private void SettleWallet(){pendingWalletStars=0;RefreshWallet();}
        private void ReceiveWalletStar()
        {
            if(pendingWalletStars<=0)return;
            int note=Mathf.Clamp(earnedStars-pendingWalletStars,0,2);
            pendingWalletStars--;RefreshWallet();walletPulse=reduceMotion?0:.3f;
            PlayRewardNote(3+note);
            if(pendingWalletStars==0)completionStars?.FinishWhenReady();
        }
        private void DialogText(RectTransform parent,string text,float y,float height,int size,Color? color=null)
        { var label=PuzzleUI.Label("Dialog text",parent,text,size,color??PuzzleUI.Ink);PuzzleUI.Box(label.rectTransform,18,y,parent.sizeDelta.x-36,height); }
        private void DialogButton(RectTransform parent,string text,float y,Action action,Color? color=null)
        {
            var button=MakeButton(parent,text,text,null,()=>Active,action,color??PuzzleUI.Mint);
            PuzzleUI.Box((RectTransform)button.transform,24,y,parent.sizeDelta.x-48,48);
        }
        private void OpenMechanicHelp()
        {
            session.SetPause(PauseReason.Menu,true);
            var card=OpenDialog("rules",444);
            if(session.Level.HasBlinkBlocks)
            {
                DialogText(card,"길도 함께 숨바꼭질",376,42,25);
                DialogText(card,"방향을 한 번 누를 때마다 길이 바뀌어요",326,32,14,PuzzleUI.Muted);
                DialogText(card,"점선은 열린 길 · 도톰한 블록은 벽",276,36,17);
                DialogText(card,"친구들이 모두 멈춘 뒤 열리고 닫혀요.\n제자리인 방향을 눌러도 한 턴이 지나가요.",207,56,14);
                DialogText(card,"● 살구와 ●● 민트는 서로 번갈아 나타나요.\n친구가 있는 칸은 그 턴 동안 열린 채 기다려요.",139,56,13);
                DialogText(card,session.Level.HasResizers?"포탈 → 크기 변환 → 길바꿈 순서예요.":session.Level.HasPortals?"포탈 이동까지 끝나면 길이 바뀌어요.":"블록을 누르면 현재 상태를 볼 수 있어요.",92,32,13,PuzzleUI.Muted);
                DialogButton(card,"알겠어요, 출발!",26,Resume,PuzzleUI.Accent);LayoutDialog();return;
            }
            if(session.Level.HasResizers)
            {
                DialogText(card,"말랑, 크기를 바꿔요",376,42,25);
                DialogText(card,"+ 넓은 틀은 2×2 · − 작은 틀은 1×1",326,32,16,PuzzleUI.Muted);
                DialogText(card,"틀의 왼쪽 아래에 맞춰 멈춰요",275,36,19);
                DialogText(card,"지나가기만 하면 변하지 않아요.\n모두 멈춘 뒤 크기가 바뀌고 그 턴은 끝나요.",207,56,14);
                DialogText(card,"커질 자리에 친구가 있으면 그대로예요.\n목적지는 색과 크기가 모두 맞아야 해요.",139,56,14);
                DialogText(card,session.Level.HasPortals?"작은 포탈은 작은 친구만 · 전송 후 크기 변환":"변환대를 누르면 사용법을 볼 수 있어요.",92,32,13,PuzzleUI.Muted);
                DialogButton(card,"알겠어요, 말랑!",26,Resume,PuzzleUI.Accent);LayoutDialog();return;
            }
            DialogText(card,"반짝, 별문 사용법",376,42,25);
            DialogText(card,session.Level.lesson.Replace("같은 문자","같은 색").Replace("문 앞 화살표를 따라 들어가요.","빛나는 입구를 향해 들어가요."),319,46,14,PuzzleUI.Muted);
            DialogText(card,"같은 빛의 입구끼리 연결돼요",271,34,20);
            DialogText(card,"밝은 입구를 향해 들어가요.\n연결된 입구 바로 앞에 나와요.",209,52,14);
            DialogText(card,"포탈을 누르면 반대편 출구가 크게 빛나요.\n출구에 친구가 있으면 전송되지 않아요.",147,52,14);
            DialogText(card,"벽에 있는 문도 같아요. 전송 후에는 멈춰요.",106,30,14,PuzzleUI.Muted);
            DialogButton(card,"알겠어요, 출발!",36,Resume,PuzzleUI.Accent);LayoutDialog();
        }
        private void OpenPause()
        {
            session.SetPause(PauseReason.Menu,true);
            var card=OpenDialog("pause",518);
            DialogText(card,"잠깐, 쉬어가요",444,42,26);
            DialogText(card,"친구들은 여기서 기다릴게요.",412,28,13,PuzzleUI.Muted);
            DialogButton(card,"계속하기",348,Resume);
            DialogButton(card,"단계 선택",290,()=>{session.SetPause(PauseReason.Menu,false);OpenLevels();},new Color(.90f,.86f,.96f));
            DialogButton(card,"시작 화면",232,OpenHome,new Color(.98f,.88f,.82f));
            DialogButton(card,"효과음  "+(sound?"켜짐":"꺼짐"),174,()=>{ToggleSound();OpenPause();},new Color(.93f,.90f,.97f));
            DialogButton(card,"빠른 진행  "+(reduceMotion?"켜짐":"꺼짐"),116,()=>{ToggleFastPlay();OpenPause();},new Color(.93f,.90f,.97f));
            DialogButton(card,"큰 글씨  "+(largeText?"켜짐":"꺼짐"),58,()=>{largeText=!largeText;Save();Layout();OpenPause();},new Color(.93f,.90f,.97f));
            DialogText(card,"방향키 / WASD · 되돌리기 Z · 처음부터 R",14,26,11,PuzzleUI.Muted);LayoutDialog();
        }
        private void Resume(){session.SetPause(PauseReason.Menu,false);CloseDialog();}
        private void AskReset()
        {
            resetFromPause=modal=="pause";session.SetPause(PauseReason.Reset,true);
            var card=OpenDialog("reset",294);
            DialogText(card,"처음부터 해볼까요?",224,40,24);
            DialogText(card,"이동·되돌리기 기록을 지우고 새로 도전해요.",185,28,13,PuzzleUI.Muted);
            DialogButton(card,"처음부터",110,()=>{celebrationPending=false;finishDelay=-1;board.CancelMotion();solvedPresented=false;session.Reset();session.SetPause(PauseReason.Reset,false);session.SetPause(PauseReason.Menu,false);CloseDialog();Save();hint.text="";});
            DialogButton(card,"이어 하기",48,CancelReset,new Color(.92f,.88f,.97f));LayoutDialog();
        }
        private void CancelReset(){session.SetPause(PauseReason.Reset,false);if(resetFromPause)OpenPause();else CloseDialog();}
        private string LevelName(PuzzleDefinition level)=>levelNames.TryGetValue(level.room,out var name)?name:level.displayName;
        private static string Difficulty(PuzzleDefinition level)
        {
            int moves=level.route.Length;
            if(level.HasBlinkBlocks||level.HasResizers)return moves<=6?"입문":moves<=10?"기본":moves<=14?"도전":"심화";
            if(level.HasPortals)return moves<=6?"입문":moves<=10?"기본":moves<=14?"도전":"심화";
            return moves<=3?"입문":moves<=6?"기본":moves<=9?"도전":"심화";
        }
        private void OpenLevels()
        {
            session.SetPause(PauseReason.Levels,true);
            var card=OpenDialog("levels",668);
            DialogText(card,"한 단계씩, 우주 여행",598,42,25);
            DialogText(card,"위아래로 드래그해 원하는 단계를 골라요",565,26,13,PuzzleUI.Muted);
            const int columns=4;
            const float viewportHeight=450,tileHeight=80,gap=8,sectionHeight=48;
            int chapters=Mathf.CeilToInt(levels.Length/16f);
            float contentHeight=Mathf.CeilToInt(levels.Length/(float)columns)*(tileHeight+gap)+sectionHeight*chapters+8;
            var viewport=PuzzleUI.Panel("Level list viewport",card,new Color(1,1,1,0),false);viewport.raycastTarget=true;
            PuzzleUI.Box(viewport.rectTransform,18,100,card.sizeDelta.x-48,viewportHeight);viewport.gameObject.AddComponent<RectMask2D>();
            var content=PuzzleUI.Rect("All levels in difficulty order",viewport.transform);
            content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(0,contentHeight);content.anchoredPosition=Vector2.zero;
            var scroll=viewport.gameObject.AddComponent<PuzzleLevelScroll>();scroll.viewport=viewport.rectTransform;scroll.content=content;
            scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=!reduceMotion;scroll.decelerationRate=.12f;scroll.scrollSensitivity=34;
            var track=PuzzleUI.Panel("Level scrollbar track",card,new Color(.90f,.86f,.96f));track.raycastTarget=true;PuzzleUI.Box(track.rectTransform,card.sizeDelta.x-23,100,7,viewportHeight);
            var handle=PuzzleUI.Panel("Level scrollbar handle",track.transform,new Color(.61f,.48f,.77f));handle.raycastTarget=true;PuzzleUI.Fill(handle.rectTransform);
            var scrollbar=track.gameObject.AddComponent<Scrollbar>();scrollbar.handleRect=handle.rectTransform;scrollbar.targetGraphic=handle;scrollbar.direction=Scrollbar.Direction.BottomToTop;
            scroll.verticalScrollbar=scrollbar;scroll.verticalScrollbarVisibility=ScrollRect.ScrollbarVisibility.Permanent;
            float width=card.sizeDelta.x-48,cursor=0,tileWidth=(card.sizeDelta.x-52-gap*(columns-1))/columns;
            for(int i=0;i<levels.Length;i++)
            {
                if(i%16==0)
                {
                    string chapter=levels[i].HasBlinkBlocks?"길이 열리는 여행":levels[i].HasResizers?"크기를 바꾸는 여행":levels[i].HasPortals?"포탈 있는 여행":"포탈 없는 여행";
                    var heading=PuzzleUI.Label("Chapter heading",content,(i+1).ToString("00")+"–"+Math.Min(i+16,levels.Length)+"  ·  "+chapter,15,PuzzleUI.Ink,TextAnchor.MiddleLeft);heading.fontStyle=FontStyle.Bold;
                    PuzzleUI.Box(heading.rectTransform,4,contentHeight-cursor-36,width-8,30);cursor+=sectionHeight;
                }
                int index=i;var level=levels[index];bool done=completed.Contains(level.room),current=index==levelIndex;
                int sectionIndex=i%16;
                var tile=MakeButton(content,"Level "+(index+1)+" "+LevelName(level),"",null,()=>Active&&modal=="levels",()=>SelectLevel(index),current?new Color(1,.86f,.76f):done?new Color(.86f,.82f,.96f):new Color(.94f,.91f,.98f));
                PuzzleUI.Box((RectTransform)tile.transform,2+(sectionIndex%columns)*(tileWidth+gap),contentHeight-cursor-tileHeight,tileWidth,tileHeight);
                var number=PuzzleUI.Label("Stage number",tile.transform,(index+1).ToString("00"),largeText?29:27,PuzzleUI.Ink);number.fontStyle=FontStyle.Bold;PuzzleUI.Box(number.rectTransform,2,37,tileWidth-4,36);
                var detail=PuzzleUI.Label("Stage difficulty",tile.transform,Difficulty(level),largeText?14:12,PuzzleUI.Muted);PuzzleUI.Box(detail.rectTransform,2,20,tileWidth-4,20);
                int stars=collection.Stars(level.room);
                var status=PuzzleUI.Label("Stage status",tile.transform,stars>0?StarText(stars):current?"진행 중":"",largeText?12:11,PuzzleUI.Muted);PuzzleUI.Box(status.rectTransform,2,3,tileWidth-4,18);
                if(sectionIndex%columns==columns-1||i==baseLevelCount-1||i==levels.Length-1)cursor+=tileHeight+gap;
            }
            scroll.verticalNormalizedPosition=1;
            DialogText(card,"최소 이동 · 되돌리기 미사용\n둘 다 ★★★  ·  하나 ★★  ·  없음 ★",66,32,11,PuzzleUI.Muted);
            DialogButton(card,"돌아가기",14,CloseLevels);LayoutDialog();
        }
        private void CloseLevels()
        {
            session.SetPause(PauseReason.Levels,false);CloseDialog();
            if(home){modal="home";return;}
            if(session.Solved)Complete();
        }
        private void QueueComplete()
        {
            board.Celebrate();celebrationPending=true;finishDelay=reduceMotion?0:.85f;
            if(!solvedPresented){solvedPresented=true;RecordClear();VictoryTone();}
        }
        private void Complete()
        {
            if(modal=="solved")return;
            finishDelay=-1;
            bool fresh=!solvedPresented;
            if(fresh){solvedPresented=true;RecordClear();VictoryTone();}
            var card=OpenDialog("solved",590);
            float cx=card.sizeDelta.x/2;
            var orbit=PuzzleUI.Glyph(card,GlyphKind.Ring,PuzzleUI.Mint);PuzzleUI.Box(orbit.rectTransform,cx-69,455,138,105);
            for(int i=0;i<3;i++)
            {
                var friend=SkinDecoration.Portrait(card,collection.selected,i,out var accessory);PuzzleUI.Box(friend,cx-70+i*47,475+(i==1?13:0),46,49);
                var sparkle=PuzzleUI.Glyph(card,GlyphKind.Star,PuzzleUI.Candy[i]);PuzzleUI.Box(sparkle.rectTransform,cx-91+i*82,525+(i==1?12:0),15,15);
            }
            int rating=PuzzleCollection.Rating(session.Moves,session.Level.route.Length,session.UsedUndo);
            completionStars=new CompletionStars(card,rating,reduceMotion,PlayRewardNote,PlayRewardFinale);
            DialogText(card,"우리, 사뿐히 착륙!",295,40,26);
            DialogText(card,session.Moves+"회 이동 · "+(earnedStars>0?"별 +"+earnedStars:"최고 별 기록 유지"),265,27,14,PuzzleUI.Muted);
            DialogText(card,"최소 이동 "+session.Level.route.Length+"회: "+(session.Moves==session.Level.route.Length?"달성":"미달")+"\n되돌리기: "+(session.UsedUndo?"사용":"미사용")+"\n최고 평가보다 늘어난 별만 추가로 받아요",201,60,12,PuzzleUI.Muted);
            DialogButton(card,levelIndex<levels.Length-1?"다음 우주로  →":"모든 우주 둘러보기",151,()=>{if(levelIndex<levels.Length-1)LoadLevel(levelIndex+1);else OpenLevels();},PuzzleUI.Accent);
            DialogButton(card,"한 번 더 데굴",91,()=>LoadLevel(levelIndex));
            DialogButton(card,"레벨 목록",31,OpenLevels,new Color(.92f,.88f,.97f));LayoutDialog();
            celebration=new PuzzleCelebration(dialog);celebration.Play(reduceMotion||!celebrationPending);celebrationPending=false;
            walletBadge.SetAsLastSibling();RefreshWallet();
            if(pendingWalletStars>0)
            {
                if(reduceMotion){SettleWallet();completionStars.FinishWhenReady();}
                else walletFlight=new WalletStarFlight(dialog,completionStars,walletIcon,rating,pendingWalletStars,ReceiveWalletStar);
            }
            else completionStars.FinishWhenReady();
        }
        private void OnApplicationFocus(bool value){focused=value;Lifecycle();}
        private void OnApplicationPause(bool value){appPaused=value;Lifecycle();}
        private void Lifecycle()
        {
            if(session==null)return;
            CancelTouches();session.SetPause(PauseReason.Inactive,!Active);
            if(rewardSource){if(Active)rewardSource.UnPause();else rewardSource.Pause();}
            if(!Active && modal=="")OpenPause();
        }
        private void CreateSound()
        {
            audioSource=gameObject.AddComponent<AudioSource>();audioSource.playOnAwake=false;
            rewardSource=gameObject.AddComponent<AudioSource>();rewardSource.playOnAwake=false;
            rewardClips=new AudioClip[ClearRewardAudio.Notes.Length];
            for(int i=0;i<rewardClips.Length;i++)rewardClips[i]=ClearRewardAudio.MakeClip(i);
            rewardFinale=ClearRewardAudio.MakeFinale();
            var samples=new float[4410];for(int i=0;i<samples.Length;i++){float t=(float)i/44100; samples[i]=Mathf.Sin(t*2*Mathf.PI*620)*.09f*(1f-(float)i/samples.Length);}
            clickClip=AudioClip.Create("Soft plink",samples.Length,1,44100,false);clickClip.SetData(samples,0);
            var melody=new float[33075];float[] notes={392f,440f,493.88f,523.25f};
            for(int i=0;i<melody.Length;i++)
            {
                float t=i/44100f;
                for(int n=0;n<notes.Length;n++) { float local=t-n*.12f;if(local>=0&&local<.35f)melody[i]+=Mathf.Sin(local*2*Mathf.PI*notes[n])*.065f*Mathf.Exp(-local*11)*Mathf.Min(1,local*180); }
            }
            completeClip=AudioClip.Create("Jelly landing chime",melody.Length,1,44100,false);completeClip.SetData(melody,0);
        }
        private void Tone(float pitch){if(sound&&Active&&audioSource){audioSource.pitch=pitch;audioSource.PlayOneShot(clickClip);}}
        private void VictoryTone(){if(sound&&Active&&audioSource){audioSource.pitch=1;audioSource.PlayOneShot(completeClip);}}
        private void PlayRewardNote(int index){if(sound&&Active&&rewardSource)rewardSource.PlayOneShot(rewardClips[index]);}
        private void PlayRewardFinale(){if(sound&&Active&&rewardSource)rewardSource.PlayOneShot(rewardFinale);}
        private void OnDestroy()
        {
            if(completeClip)Destroy(completeClip);if(clickClip)Destroy(clickClip);
            if(rewardClips!=null)foreach(var clip in rewardClips)if(clip)Destroy(clip);
            if(rewardFinale)Destroy(rewardFinale);PuzzleTap.CancelAll();
        }
    }
}




