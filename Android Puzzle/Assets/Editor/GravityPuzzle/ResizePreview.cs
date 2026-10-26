using System.IO;
using System.Collections.Generic;
using System.Reflection;
using Puzzle.Gravity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Editor
{
    public static class ResizePreview
    {
        public static void BuildAndRender() { GravityPrototypeSetup.BuildWeb();Render(); }
        [MenuItem("Puzzle/Prototype/Render Resize Boards")]
        public static void Render()
        {
            PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
            var maps=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/resize_levels").text).layouts;
            Capture(maps[0],33,false,"Logs/resize-33-board.png");
            Capture(maps[0],33,true,"Logs/resize-33-grown.png");
            Capture(maps[15],48,false,"Logs/resize-48-board.png");
            Debug.Log("Resize board renders saved (offscreen rendering, not browser interaction).");
        }
        private static void Capture(PuzzleDefinition map,int stage,bool grow,string path)
        {
            var cameraObject=new GameObject("Preview camera",typeof(Camera));var cam=cameraObject.GetComponent<Camera>();
            var canvasObject=new GameObject("Preview canvas",typeof(Canvas));
            var rt=new RenderTexture(780,1688,24);var old=RenderTexture.active;Texture2D texture=null;
            var meshes=new List<Mesh>();
            try
            {
                cam.orthographic=true;cam.orthographicSize=422;cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=PuzzleUI.Cream;cam.targetTexture=rt;
                var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=cam;canvas.planeDistance=1;
                var scaler=canvasObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(390,844);scaler.matchWidthOrHeight=1;
                var root=PuzzleUI.Rect("Board",canvas.transform);var view=new BoardView(root);view.Build(map);
                var session=new PuzzleSession(map);
                if(grow)foreach(int d in map.route){session.Command(d);bool changed=session.Plan.HasResize;session.Tick(100);if(changed)break;}
                var title=PuzzleUI.Label("Title",canvas.transform,stage+" / 48   "+map.displayName,22,PuzzleUI.Ink);PuzzleUI.Box(title.rectTransform,20,756,350,50);
                var legend=PuzzleUI.Label("Legend",canvas.transform,"+ 확대 2×2     − 축소 1×1",15,PuzzleUI.Muted);PuzzleUI.Box(legend.rectTransform,20,709,350,40);
                view.Layout(340,510,195,444);view.Render(session,0,true);
                var note=PuzzleUI.Label("Note",canvas.transform,"틀의 왼쪽 아래에 맞춰 멈춰요.\n목적지의 색과 크기를 맞추면 완료!",15,PuzzleUI.Ink);PuzzleUI.Box(note.rectTransform,20,104,350,70);
                var caption=PuzzleUI.Label("Caption",canvas.transform,"보드 렌더 검증 · "+(grow?"확대 후":"시작 상태"),11,PuzzleUI.Muted);PuzzleUI.Box(caption.rectTransform,20,50,350,30);
                Canvas.ForceUpdateCanvases();
                // Render the same procedural meshes without requiring a play-mode lifecycle.
                var populate=typeof(PuzzleGlyph).GetMethod("OnPopulateMesh",BindingFlags.Instance|BindingFlags.NonPublic,null,new[]{typeof(VertexHelper)},null);
                foreach(var glyph in canvasObject.GetComponentsInChildren<PuzzleGlyph>())
                {
                    using(var vertices=new VertexHelper())
                    {
                        populate.Invoke(glyph,new object[]{vertices});var mesh=new Mesh();vertices.FillMesh(mesh);meshes.Add(mesh);
                        var renderer=glyph.GetComponent<CanvasRenderer>();if(!renderer)renderer=glyph.gameObject.AddComponent<CanvasRenderer>();
                        renderer.cull=false;renderer.materialCount=1;renderer.SetMaterial(Graphic.defaultGraphicMaterial,0);renderer.SetTexture(Texture2D.whiteTexture);renderer.SetColor(Color.white);renderer.SetMesh(mesh);
                    }
                }
                cam.Render();RenderTexture.active=rt;
                texture=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());
            }
            finally
            {
                RenderTexture.active=old;cam.targetTexture=null;Object.DestroyImmediate(canvasObject);Object.DestroyImmediate(cameraObject);foreach(var mesh in meshes)Object.DestroyImmediate(mesh);if(texture)Object.DestroyImmediate(texture);rt.Release();Object.DestroyImmediate(rt);
            }
        }
    }
}
