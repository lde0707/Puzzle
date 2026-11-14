using System.IO;
using Puzzle.Gravity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Editor
{
    public static class SkinConceptPreview
    {
        [MenuItem("Puzzle/Prototype/Validate and Preview Skin Concepts")]
        public static void ValidateAndPreview(){GravityPrototypeChecks.Run();Export();}
        public static void BuildWeb(){GravityPrototypeSetup.PrepareAndValidate();Export();IphoneTestSetup.BuildGame();}
        public static void Export()=>ExportFrame("skin-concepts-contact-sheet.png",0);
        public static void BuildMotionWeb(){ValidateMotionAndPreview();IphoneTestSetup.BuildGame();}
        public static void ValidateExpansionAndPreview()
        {GravityPrototypeChecks.Run();ExportFrame("skin-expansion-idle.png",0,20);ExportFrame("skin-expansion-moving.png",1,20);ExportFrame("skin-expansion-arrival.png",2,20);}
        public static void BuildExpansionWeb(){ExportFrame("skin-expansion-idle.png",0,20);ExportFrame("skin-expansion-arrival.png",2,20);IphoneTestSetup.BuildGame();}
        public static void ValidateMotionAndPreview()
        {GravityPrototypeChecks.Run();ExportFrame("skin-motion-moving.png",1);ExportFrame("skin-motion-arrival.png",2);}
        private static void ExportFrame(string filename,int motion,int first=0)
        {
            const int width=1200,height=1200;
            PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
            var root=new GameObject("Skin contact sheet",typeof(RectTransform),typeof(Canvas));
            var cameraObject=new GameObject("Skin preview camera",typeof(Camera));
            var texture=new RenderTexture(width*2,height*2,24,RenderTextureFormat.ARGB32){antiAliasing=4};
            var previous=RenderTexture.active;
            try
            {
                var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
                var rect=root.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(width,height);
                var camera=cameraObject.GetComponent<Camera>();camera.orthographic=true;camera.orthographicSize=height/2f;camera.transform.position=new Vector3(0,0,-10);camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=PuzzleUI.Cream;camera.targetTexture=texture;camera.cullingMask=1<<5;camera.nearClipPlane=.1f;camera.farClipPlane=30;canvas.worldCamera=camera;
                var title=PuzzleUI.Label("Title",rect,motion==0?(first==20?"데굴데굴 · 새로운 친구 20종":"데굴데굴 · 20가지 작은 친구"):motion==1?"데굴데굴 · 움직이는 친구들":"데굴데굴 · 도착한 친구들",38,PuzzleUI.Ink);title.fontStyle=FontStyle.Bold;PuzzleUI.Box(title.rectTransform,20,1123,1160,60);
                var subtitle=PuzzleUI.Label("Subtitle",rect,"말랑한 네모 몸체 · 20가지 소재와 움직임 · 스킨마다 전용 3색",20,PuzzleUI.Muted);PuzzleUI.Box(subtitle.rectTransform,20,1086,1160,34);
                for(int slot=0;slot<20;slot++)
                {
                    int id=first+slot;
                    var card=PuzzleUI.Panel("Concept "+id,rect,id%2==0?new Color(.93f,.89f,.97f):new Color(.96f,.91f,.88f));
                    PuzzleUI.Box(card.rectTransform,22+(slot%4)*294,20+(4-slot/4)*210,280,198);
                    var name=PuzzleUI.Label("Name",card.transform,(id+1).ToString("00")+"  "+PuzzleCollection.Names[id],20,PuzzleUI.Ink);name.fontStyle=FontStyle.Bold;PuzzleUI.Box(name.rectTransform,6,154,268,32);
                    for(int v=0;v<3;v++){var portrait=SkinDecoration.Portrait(card.transform,id,v,out var graphic);PuzzleUI.Box(portrait,8+v*89,54,86,86);graphic.Motion(motion==1,motion==2?.28f:10,Vector2.right);graphic.Mood(motion==2,motion==1?Vector2.right:Vector2.zero);graphic.Pose(.8f,false);}
                    var concept=PuzzleUI.Label("Concept",card.transform,SkinDecoration.Concepts[id],14,PuzzleUI.Muted);PuzzleUI.Box(concept.rectTransform,10,8,260,32);
                }
                foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
                Canvas.ForceUpdateCanvases();
                camera.Render();RenderTexture.active=texture;
                var png=new Texture2D(width*2,height*2,TextureFormat.RGBA32,false);png.ReadPixels(new Rect(0,0,width*2,height*2),0,0);png.Apply();Directory.CreateDirectory("Logs");File.WriteAllBytes(Path.Combine("Logs",filename),png.EncodeToPNG());Object.DestroyImmediate(png);
                Debug.Log("Skin concept contact sheet exported: Logs/"+filename);
            }
            finally{RenderTexture.active=previous;Object.DestroyImmediate(root);Object.DestroyImmediate(cameraObject);texture.Release();Object.DestroyImmediate(texture);}
        }
    }
}
