using System;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public sealed partial class GravityPuzzleApp
    {
        private const float BatchStart=1.88f, BatchInterval=.20f;
        private int[] drawnBatch=Array.Empty<int>();
        private int batchShown;
        private RectTransform batchGrid;
        private RectTransform[] batchCards;
        private CanvasGroup[] batchFades;
        private SkinDecoration[] batchSkins;
        private PuzzleTap[] batchTiles;
        private bool IsBatchDraw=>drawnBatch.Length>1;
        private float DrawEndTime=>IsBatchDraw?BatchStart+(drawnBatch.Length-1)*BatchInterval+.85f:DrawFinished;

        private void OpenSkinBatch(int[] skins)
        {
            OpenSkinDraw(skins[0]);
            drawnBatch=(int[])skins.Clone();
            batchGrid=PuzzleUI.Rect("Ten new friends",drawStage);PuzzleUI.Fill(batchGrid);
            int count=skins.Length;
            batchCards=new RectTransform[count];batchFades=new CanvasGroup[count];batchSkins=new SkinDecoration[count];batchTiles=new PuzzleTap[count];
            float width=(drawStage.sizeDelta.x-24)/3;
            for(int i=0;i<count;i++)
            {
                int id=skins[i];
                // Animate the wrapper; PuzzleTap owns the child button's press scale.
                var wrapper=PuzzleUI.Rect("Reveal friend "+(i+1),batchGrid);batchCards[i]=wrapper;
                CenterDraw(wrapper,drawStage.sizeDelta.x/2,158,width,70);
                batchFades[i]=wrapper.gameObject.AddComponent<CanvasGroup>();
                var tile=MakeButton(wrapper,"Choose reward "+id,"",null,()=>Active&&modal=="skin-draw"&&drawReady,()=>SelectBatchReward(id),PuzzleUI.Cream);
                batchTiles[i]=tile;PuzzleUI.Fill((RectTransform)tile.transform);
                var portrait=SkinDecoration.Portrait(tile.transform,id,i%3,out batchSkins[i]);CenterDraw(portrait,width/2,43,52,52);batchSkins[i].Mood(true,Vector2.zero);
                var label=PuzzleUI.Label("Reward name",tile.transform,PuzzleCollection.Names[id],10,PuzzleUI.Ink);PuzzleUI.Box(label.rectTransform,1,1,width-2,17);
                for(int variant=0;variant<3;variant++)
                {
                    var dot=PuzzleUI.Panel("Included color "+variant,tile.transform,SkinDecoration.ColorFor(id,variant));PuzzleUI.Box(dot.rectTransform,width-9,57-variant*7,5,5);
                }
            }
            RenderSkinDraw();
        }
        private void TickBatchCues()
        {
            if(!IsBatchDraw)return;
            int visible=Mathf.Clamp(Mathf.FloorToInt((drawTime-BatchStart)/BatchInterval)+1,0,drawnBatch.Length);
            // One light chime per newly appearing card, without stacking after a delayed frame.
            if(visible>batchShown){if(batchShown>0)PlayDrawCue(visible%3);batchShown=visible;}
        }
        private void SelectBatchReward(int id)
        {
            if(!drawReady||Array.IndexOf(drawnBatch,id)<0)return;
            drawnSkin=id;Tone(1.05f);RenderSkinBatch();
        }
        private void RenderSkinBatch()
        {
            float t=drawTime;int shown=0;
            float width=(drawStage.sizeDelta.x-24)/3;
            for(int i=0;i<drawnBatch.Length;i++)
            {
                float progress=Mathf.Clamp01((t-BatchStart-i*BatchInterval)/.48f);
                bool visible=t>=BatchStart+i*BatchInterval;
                batchCards[i].gameObject.SetActive(visible);if(!visible)continue;shown++;
                int row=i/3,rowCount=Mathf.Min(3,drawnBatch.Length-row*3);
                var destination=new Vector2(drawStage.sizeDelta.x/2+(i%3-(rowCount-1)*.5f)*(width+4),274-row*75);
                float ease=1-Mathf.Pow(1-progress,3);
                batchCards[i].anchoredPosition=Vector2.Lerp(new Vector2(drawStage.sizeDelta.x/2,158),destination,ease);
                float pop=1-Mathf.Pow(1-progress,3)*Mathf.Cos(progress*Mathf.PI*2);
                batchCards[i].localScale=Vector3.one*(.18f+.82f*pop);
                batchCards[i].localEulerAngles=new Vector3(0,0,(i%2==0?12:-12)*(1-ease));
                batchFades[i].alpha=Mathf.Clamp01(progress*4);batchFades[i].blocksRaycasts=drawReady;
                batchSkins[i].Pose(t+i*.3f,false);
                batchTiles[i].normal=drawReady&&drawnSkin==drawnBatch[i]?new Color(1,.86f,.58f):PuzzleUI.Cream;
                batchTiles[i].surface.color=batchTiles[i].normal;
            }
            revealTitle.text=drawReady?"새 친구 열 명이 왔어요!":t<BatchStart?"열 명의 친구가 기다려요":"쭈르르, 반가워요!";
            revealSubtitle.text=drawReady?"마음에 드는 친구를 눌러 바로 착용해요":"한 캡슐에서 펼쳐지는 열 가지 설렘";
            revealMessage.text=drawReady?PuzzleCollection.Names[drawnSkin]+" · 선택 중":shown==0?"별빛을 모으는 중…":"새 친구 "+shown+" / "+drawnBatch.Length;
        }
    }
}
