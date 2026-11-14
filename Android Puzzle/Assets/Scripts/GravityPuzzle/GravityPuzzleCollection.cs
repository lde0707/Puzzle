using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp
    {
        private PuzzleCollection collection;
        private readonly SkinDecoration[] homeSkins=new SkinDecoration[3];
        private readonly SkinDecoration[] drawSkins=new SkinDecoration[3];
        private readonly RectTransform[] drawFriends=new RectTransform[3];
        private SkinDecoration[] gallerySkins=new SkinDecoration[0];
        private Text drawCaption,drawNotice,skinConcept;
        private int earnedStars,previewSkin;
        private float collectionTime;
        private DrawStageArt collectionArt;
        private bool showCatalogue=true;
        private PuzzleTap[] galleryTiles;
        private Text[] galleryStatuses;
        private int[] galleryIds;
        private static string StarText(int count)=>new string('★',count)+new string('☆',3-count);
        private void RecordClear()
        {
            activity.For(session.Level.room).clears++;
            completed.Add(session.Level.room);
            earnedStars=collection.Award(session.Level.room,session.Moves,session.Level.route.Length,session.UsedUndo,usedHint||!runAudit);
            // Persist the award immediately; only its visible counter waits for the flight.
            pendingWalletStars=earnedStars;RefreshWallet();
            Save();
        }
        private void OpenCollection(string notice=null,bool preserveSelection=false)
        {
            session.SetPause(PauseReason.Menu,true);
            var card=OpenDialog("skins",750);
            MochiDrawFrame(card);
            collectionTime=0;if(!preserveSelection)previewSkin=collection.selected;
            float width=card.sizeDelta.x;
            DialogText(card,"말랑 옷장",698,38,28);
            DialogText(card,"별을 모아, 우리만의 새로운 모습",671,24,13,PuzzleUI.Muted);
            DialogText(card,"★  "+collection.wallet+"   ·   수집 "+collection.owned.Length+" / "+PuzzleCollection.Names.Length,638,30,17);
            var machine=PuzzleUI.Panel("Starlight capsule",card,new Color(.85f,.76f,.97f,.45f));PuzzleUI.Box(machine.rectTransform,22,449,width-44,184);
            var inner=PuzzleUI.Panel("Capsule window",machine.transform,new Color(1,.97f,.99f,.68f));PuzzleUI.Fill(inner.rectTransform,4);
            var art=PuzzleUI.Rect("Wardrobe candy stage",machine.transform);PuzzleUI.Box(art,7,45,width-58,104);
            collectionArt=art.gameObject.AddComponent<DrawStageArt>();collectionArt.preview=true;collectionArt.raycastTarget=false;
            skinConcept=PuzzleUI.Label("Skin concept",machine.transform,SkinDecoration.Concepts[previewSkin],12,PuzzleUI.Muted);PuzzleUI.Box(skinConcept.rectTransform,8,151,width-60,28);
            for(int i=0;i<3;i++)
            {
                drawFriends[i]=SkinDecoration.Portrait(machine.transform,previewSkin,i,out drawSkins[i]);
                PuzzleUI.Box(drawFriends[i],(width-44)/2-122+i*83,62+(i==1?7:0),78,78);
            }
            drawCaption=PuzzleUI.Label("Preview skin name",machine.transform,PuzzleCollection.Names[previewSkin]+" · 착용 중",17,PuzzleUI.Ink);
            drawCaption.fontStyle=FontStyle.Bold;PuzzleUI.Box(drawCaption.rectTransform,8,12,width-60,32);
            var draw=MakeButton(card,"Draw skin","",null,()=>Active&&modal=="skins"&&collection.wallet>=PuzzleCollection.DrawCost&&collection.Remaining>0,DrawSkin,new Color(1,.86f,.61f));
            float drawWidth=(width-58)/2;
            PuzzleUI.Box((RectTransform)draw.transform,24,387,drawWidth,53);
            var drawLabel=PuzzleUI.Label("Draw cost",draw.transform,collection.Remaining==0?"모두 모았어요!":"1개 뽑기\n별 3개",14,PuzzleUI.Ink);
            drawLabel.fontStyle=FontStyle.Bold;PuzzleUI.Fill(drawLabel.rectTransform,3);
            var batch=MakeButton(card,"Draw ten skins","10개 연속 뽑기\n별 30개",null,()=>Active&&modal=="skins"&&collection.wallet>=PuzzleCollection.BatchDrawCost&&collection.Remaining>=PuzzleCollection.BatchDrawCount,DrawTenSkins,new Color(.85f,.77f,1));
            PuzzleUI.Box((RectTransform)batch.transform,width/2+5,387,drawWidth,53);
            foreach(var label in batch.GetComponentsInChildren<Text>())label.fontSize=14;
            string rule=collection.Remaining==0?"모든 친구를 만났어요!":collection.Remaining<10?"새 친구가 "+collection.Remaining+"종 남았어요 · 1개씩 뽑아 주세요":collection.wallet<30?"10연속은 별 "+(30-collection.wallet)+"개를 더 모으면 열려요":"캡슐 한 번에, 새 친구 열 명을 만나요";
            drawNotice=PuzzleUI.Label("Draw rules",card,notice??"중복 없이 · 미보유 스킨마다 같은 확률\n"+rule,11,PuzzleUI.Muted);PuzzleUI.Box(drawNotice.rectTransform,20,347,width-40,36);
            var heading=PuzzleUI.Label("Owned heading",card,showCatalogue?"스킨 도감 · "+PuzzleCollection.Names.Length+"종":"보유 스킨 · 선택하기",13,PuzzleUI.Ink,TextAnchor.MiddleLeft);heading.fontStyle=FontStyle.Bold;PuzzleUI.Box(heading.rectTransform,24,316,width-145,26);
            var toggle=MakeButton(card,"Collection filter",showCatalogue?"보유만 보기":PuzzleCollection.Names.Length+"종 보기",null,()=>Active&&modal=="skins",()=>{showCatalogue=!showCatalogue;OpenCollection(null,true);},PuzzleUI.Mint);PuzzleUI.Box((RectTransform)toggle.transform,width-116,316,92,27);
            var viewport=PuzzleUI.Panel("Owned skins viewport",card,new Color(1,1,1,0),false);viewport.raycastTarget=true;PuzzleUI.Box(viewport.rectTransform,22,80,width-44,228);viewport.gameObject.AddComponent<RectMask2D>();
            var ids=showCatalogue?Enumerable.Range(0,PuzzleCollection.Names.Length).ToArray():collection.owned;
            const int columns=3;const float gap=8,tileHeight=128;
            float tileWidth=(width-44-(columns-1)*gap)/columns,contentHeight=Mathf.Max(228,Mathf.CeilToInt(ids.Length/(float)columns)*(tileHeight+gap));
            var content=PuzzleUI.Rect("Skins to scroll",viewport.transform);content.anchorMin=new Vector2(0,1);content.anchorMax=Vector2.one;content.pivot=new Vector2(.5f,1);content.sizeDelta=new Vector2(0,contentHeight);content.anchoredPosition=Vector2.zero;
            var scroll=viewport.gameObject.AddComponent<PuzzleLevelScroll>();scroll.viewport=viewport.rectTransform;scroll.content=content;scroll.horizontal=false;scroll.vertical=true;scroll.movementType=ScrollRect.MovementType.Clamped;scroll.inertia=!reduceMotion;scroll.scrollSensitivity=32;
            gallerySkins=new SkinDecoration[ids.Length];galleryTiles=new PuzzleTap[ids.Length];galleryStatuses=new Text[ids.Length];galleryIds=ids;
            for(int j=0;j<ids.Length;j++)
            {
                int id=ids[j];bool owned=collection.owned.Contains(id),equipped=collection.selected==id;
                var tile=MakeButton(content,"Skin "+id,"",null,()=>Active&&modal=="skins"&&owned,()=>SelectSkin(id),new Color(.94f,.92f,.96f));
                galleryTiles[j]=tile;
                PuzzleUI.Box((RectTransform)tile.transform,(j%columns)*(tileWidth+gap),contentHeight-(j/columns+1)*(tileHeight+gap),tileWidth,tileHeight);
                var portrait=SkinDecoration.Portrait(tile.transform,id,0,out gallerySkins[j]);PuzzleUI.Box(portrait,tileWidth/2-43,37,86,86);
                gallerySkins[j].silhouette=!owned;
                if(owned)
                {
                    var name=PuzzleUI.Label("Skin name",tile.transform,PuzzleCollection.Names[id],13,PuzzleUI.Ink);PuzzleUI.Box(name.rectTransform,2,19,tileWidth-4,22);
                    galleryStatuses[j]=PuzzleUI.Label("Skin ownership",tile.transform,"",11,PuzzleUI.Muted);PuzzleUI.Box(galleryStatuses[j].rectTransform,2,2,tileWidth-4,18);
                }
                else
                {
                    var mystery=PuzzleUI.Label("Mystery skin",tile.transform,"?",34,PuzzleUI.Cream);mystery.fontStyle=FontStyle.Bold;PuzzleUI.Box(mystery.rectTransform,0,53,tileWidth,46);
                }
            }
            scroll.verticalNormalizedPosition=1;
            DialogText(card,"위아래로 드래그 · 선택 후 저장하면 착용돼요",55,21,10,PuzzleUI.Muted);
            var back=MakeButton(card,"Exit wardrobe","나가기",null,()=>Active&&modal=="skins",CloseCollection,PuzzleUI.Mint);PuzzleUI.Box((RectTransform)back.transform,24,10,(width-58)/2,44);
            var save=MakeButton(card,"Save outfit","저장하기",null,()=>Active&&modal=="skins"&&previewSkin!=collection.selected,SaveSkinSelection,PuzzleUI.Accent);PuzzleUI.Box((RectTransform)save.transform,width/2+5,10,(width-58)/2,44);
            RefreshSkinSelection();
            LayoutDialog();
        }
        private void DrawSkin()
        {
            if(!collection.TryDraw(count=>Random.Range(0,count),Save,out int skin))
            {drawNotice.text="저장하지 못해 별을 사용하지 않았어요.\n저장 공간을 확인한 뒤 다시 시도해 주세요.";return;}
            OpenSkinDraw(skin);
        }
        private void DrawTenSkins()
        {
            if(!Active||modal!="skins"||collection.wallet<PuzzleCollection.BatchDrawCost||collection.Remaining<PuzzleCollection.BatchDrawCount)return;
            if(!collection.TryDrawMany(PuzzleCollection.BatchDrawCount,count=>Random.Range(0,count),Save,out var skins))
            {drawNotice.text="저장하지 못해 별을 사용하지 않았어요.\n저장 공간을 확인한 뒤 다시 시도해 주세요.";return;}
            OpenSkinBatch(skins);
        }
        private void SelectSkin(int id)
        {
            if(!collection.owned.Contains(id))return;
            previewSkin=id;RefreshSkinSelection();Tone(.95f);
            drawNotice.text=id==collection.selected?"지금 착용 중인 스킨이에요.":"아직 저장 전이에요. 저장하기를 눌러 착용해 주세요.\n저장하지 않고 나가면 이전 스킨을 유지해요.";
        }
        private void RefreshSkinSelection()
        {
            skinConcept.text=SkinDecoration.Concepts[previewSkin];
            foreach(var skin in drawSkins){skin.skin=previewSkin;skin.SetVerticesDirty();}
            drawCaption.text=PuzzleCollection.Names[previewSkin]+(previewSkin==collection.selected?" · 착용 중":" · 선택 중");
            for(int j=0;j<galleryIds.Length;j++)
            {
                int id=galleryIds[j];bool owned=collection.owned.Contains(id);
                galleryTiles[j].normal=id==previewSkin?new Color(1,.87f,.71f):owned?new Color(.90f,.85f,.97f):new Color(.94f,.92f,.96f);
                if(galleryStatuses[j])galleryStatuses[j].text=id==collection.selected?"착용 중":id==previewSkin?"선택 중":"보유";
            }
        }
        private void SaveSkinSelection()
        {
            if(!collection.TryEquip(previewSkin,Save)){drawNotice.text="저장하지 못했어요. 다시 저장하기를 눌러 주세요.\n이전에 착용하던 스킨은 그대로 유지돼요.";return;}
            board.ApplySkin(previewSkin);RefreshHomeSkin();RefreshSkinSelection();Tone(1.1f);
            drawNotice.text="저장 완료! 선택한 스킨을 착용했어요.\n나가기를 눌러 게임을 시작해 보세요.";
        }
        private void RefreshHomeSkin()
        {foreach(var skin in homeSkins)if(skin){skin.skin=collection.selected;skin.SetVerticesDirty();}}
        private void CloseCollection(){OpenHome();}
        private void TickCollection(float delta)
        {
            if(modal=="skin-draw"){TickSkinDraw(delta);return;}
            if(modal!="skins")return;
            if(!reduceMotion)collectionTime+=delta;
            if(collectionArt)collectionArt.Pose(collectionTime,reduceMotion);
            for(int i=0;i<3;i++)if(drawSkins[i])
            {
                drawSkins[i].Pose(collectionTime,reduceMotion);
                float wave=reduceMotion?0:Mathf.Sin(collectionTime*1.6f+i*.7f);
                // Scale around the center rather than the lower-left corner.
                var root=drawFriends[i];root.pivot=new Vector2(.5f,.5f);
                root.anchoredPosition=new Vector2((root.parent as RectTransform).sizeDelta.x/2-83+i*83,101+(i==1?7:0)+wave*3);
                root.localScale=new Vector3(1-wave*.012f,1+wave*.012f,1);
            }
            foreach(var skin in gallerySkins)if(skin)skin.Pose(collectionTime,reduceMotion||skin.silhouette);
        }
    }
}
