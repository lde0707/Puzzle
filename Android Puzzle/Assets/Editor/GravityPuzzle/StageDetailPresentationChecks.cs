using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Puzzle.Gravity;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Puzzle.Editor
{
    public static class StageDetailPresentationChecks
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Run()
        {
            Export(390,844,32,false);
            Export(320,640,63,true);
            Export(844,390,32,false);
            Debug.Log("PASS stage panel: portrait/narrow/landscape renders, original art aspect, rounded font and button alignment.");
        }
        public static void ValidateAndBuild(){Run();StageDetailChecks.ValidateAndBuild();}
        static void Export(int width,int height,int index,bool perfect)
        {
            PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
            var root=new GameObject("Stage preview",typeof(RectTransform),typeof(Canvas));
            var host=new GameObject("Stage controller");var app=host.AddComponent<GravityPuzzleApp>();app.enabled=false;
            var cameraObject=new GameObject("Stage camera",typeof(Camera));
            var target=new RenderTexture(width*2,height*2,24){antiAliasing=4};var old=RenderTexture.active;
            void Set(string name,object value)=>typeof(GravityPuzzleApp).GetField(name,Flags).SetValue(app,value);
            try
            {
                var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
                var rect=root.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(width,height);
                var cam=cameraObject.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=height/2f;
                cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=PuzzleUI.Cream;cam.targetTexture=target;cam.cullingMask=1<<5;cam.nearClipPlane=.1f;cam.farClipPlane=30;canvas.worldCamera=cam;
                var levels=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/layouts").text).layouts;
                foreach(string name in new[]{"late_levels","resize_levels","blink_levels"})levels=levels.Concat(JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/"+name).text).layouts).ToArray();
                levels=PuzzleCampaign.Order(levels);
                var collection=new PuzzleCollection();var activity=new PuzzleActivity();
                if(perfect){collection.Award(levels[index].room,levels[index].route.Length,levels[index].route.Length,false,false);activity.For(levels[index].room).moves=123456;activity.For(levels[index].room).restarts=1234;}
                Set("safe",rect);Set("levels",levels);Set("session",new PuzzleSession(levels[index]));Set("collection",collection);Set("activity",activity);Set("sound",false);
                typeof(GravityPuzzleApp).GetMethod("OpenStageDetail",Flags).Invoke(app,new object[]{index});
                foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
                Canvas.ForceUpdateCanvases();
                var art=root.GetComponentsInChildren<RawImage>().Single();
                if(Mathf.Abs(art.rectTransform.rect.width/art.rectTransform.rect.height-(float)art.texture.width/art.texture.height)>.001f)throw new Exception("Stage artwork stretched");
                if(!Resources.Load<Font>("GravityPuzzle/Jua-Regular"))throw new Exception("Rounded display font missing");
                cam.Render();RenderTexture.active=target;
                var png=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
                png.ReadPixels(new Rect(0,0,target.width,target.height),0,0);png.Apply();Directory.CreateDirectory("Logs");
                File.WriteAllBytes($"Logs/stage-detail-matte-{width}x{height}.png",png.EncodeToPNG());Object.DestroyImmediate(png);
            }
            finally{RenderTexture.active=old;Object.DestroyImmediate(host);Object.DestroyImmediate(root);Object.DestroyImmediate(cameraObject);target.Release();Object.DestroyImmediate(target);}
        }
    }
}
