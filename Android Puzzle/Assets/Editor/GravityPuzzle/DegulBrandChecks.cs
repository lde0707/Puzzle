using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Puzzle.Gravity;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Puzzle.Editor
{
    public static class DegulBrandChecks
    {
        const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
        public static void Run()
        {
            Require(PlayerSettings.productName=="데굴데굴","application display name");
            foreach(var target in new[]{NamedBuildTarget.Android,NamedBuildTarget.iOS})
                Require(PlayerSettings.GetApplicationIdentifier(target)=="com.puzzlestudio.androidpuzzle","existing app identity retained");
            Require(ProgressSaveService.StorageDirectory("C:/PuzzleStudio/데굴데굴",true)==ProgressSaveService.StorageDirectory("C:/PuzzleStudio/Android Puzzle",true),"Windows save directory retained after rename");
            Require(ProgressSaveService.StorageDirectory("/idbfs/existing",false)==Path.Combine("/idbfs/existing","MellowGravity"),"existing browser save path retained");
            Export(390,844);Export(320,640);Export(844,390);
            Debug.Log("PASS Degul brand: Korean and English wordmarks, original aspect, title animation/fast mode, menu separation, 3 screen sizes, unchanged app IDs and save paths. No web build or deployment.");
        }
        static void Export(int width,int height)
        {
            PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
            var root=new GameObject("Brand preview",typeof(RectTransform),typeof(Canvas));
            var host=new GameObject("Brand preview controller");var app=host.AddComponent<GravityPuzzleApp>();app.enabled=false;
            var cameraObject=new GameObject("Brand preview camera",typeof(Camera));
            var target=new RenderTexture(width*2,height*2,24){antiAliasing=4};var old=RenderTexture.active;
            string isolated=Path.GetFullPath("Logs/degul-brand-preview-save");
            void Set(string name,object value)=>typeof(GravityPuzzleApp).GetField(name,Flags).SetValue(app,value);
            object Get(string name)=>typeof(GravityPuzzleApp).GetField(name,Flags).GetValue(app);
            void Call(string name,params object[] args)=>typeof(GravityPuzzleApp).GetMethod(name,Flags).Invoke(app,args);
            try
            {
                var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
                var rect=root.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(width,height);
                var cam=cameraObject.GetComponent<Camera>();cam.orthographic=true;cam.orthographicSize=height/2f;
                cam.transform.position=new Vector3(0,0,-10);cam.clearFlags=CameraClearFlags.SolidColor;
                cam.backgroundColor=PuzzleUI.Cream;cam.targetTexture=target;cam.cullingMask=1<<5;cam.nearClipPlane=.1f;cam.farClipPlane=30;canvas.worldCamera=cam;
                var levels=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/layouts").text).layouts;
                Set("safe",rect);Set("levels",levels);Set("session",new PuzzleSession(levels[0]));Set("version","f0dda662");
                Set("collection",new PuzzleCollection());Set("sound",false);Set("saves",new ProgressSaveService(isolated));
                var brand=PuzzleUI.Rect("Test brand",root.transform).gameObject.AddComponent<RawImage>();
                brand.texture=BrandArt.Load("GravityPuzzle/Brand/degul-degul-wordmark-ko");brand.gameObject.SetActive(false);Set("brand",brand);
                var saveLabel=PuzzleUI.Label("Isolated save status",root.transform,"",10,PuzzleUI.Muted);saveLabel.gameObject.SetActive(false);Set("saveLabel",saveLabel);
                Call("OpenHome");
                var home=(RectTransform)Get("home");var logo=(RectTransform)Get("homeLogo");
                var images=logo.GetComponentsInChildren<RawImage>();Require(images.Length==2,"both language images are in the animated logo group");
                foreach(var image in images)
                    Require(image.texture&&Mathf.Abs(image.rectTransform.rect.width/image.rectTransform.rect.height-(float)image.texture.width/image.texture.height)<.001f,"wordmark aspect preserved");
                var composition=(RectTransform)Get("homeContent");
                var titleBounds=RectTransformUtility.CalculateRelativeRectTransformBounds(composition,logo);
                foreach(string button in new[]{"설정","Ranking"})
                {
                    var box=RectTransformUtility.CalculateRelativeRectTransformBounds(composition,composition.Find(button));
                    Require(titleBounds.max.y<box.min.y,"title stays below top menu");
                }
                var english=images.Single(i=>i.name=="Degul Degul English title");
                var tagline=home.GetComponentsInChildren<Text>().Single(t=>t.text=="중력을 돌려, 우리 모두 제자리로");
                var enBounds=RectTransformUtility.CalculateRelativeRectTransformBounds(composition,english.transform);
                var tagBounds=RectTransformUtility.CalculateRelativeRectTransformBounds(composition,tagline.transform);
                Require(enBounds.min.y>tagBounds.max.y,"English title does not overlap tagline");
                var origin=logo.anchoredPosition;for(int i=0;i<30;i++)Call("TickHome",.05f);
                Require(logo.anchoredPosition!=origin&&logo.localScale!=Vector3.one,"soft animation is active");
                Set("reduceMotion",true);Call("TickHome",0f);
                Require(logo.anchoredPosition==origin&&logo.localScale==Vector3.one,"fast mode rests the title without changing proportions");
                foreach(var t in root.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;
                foreach(var g in root.GetComponentsInChildren<Graphic>(true))
                    if(!g.GetComponent<CanvasRenderer>())
                    {var cr=g.gameObject.AddComponent<CanvasRenderer>();typeof(Graphic).GetField("m_CanvasRenderer",Flags)?.SetValue(g,cr);}
                Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=target;
                var png=new Texture2D(target.width,target.height,TextureFormat.RGBA32,false);
                png.ReadPixels(new Rect(0,0,target.width,target.height),0,0);png.Apply();Directory.CreateDirectory("Logs");
                File.WriteAllBytes($"Logs/degul-home-{width}x{height}.png",png.EncodeToPNG());Object.DestroyImmediate(png);
                Require(new ProgressSaveService(isolated).Load()!=null,"isolated preview progress saves under new display name");
            }
            finally{RenderTexture.active=old;Object.DestroyImmediate(host);Object.DestroyImmediate(root);Object.DestroyImmediate(cameraObject);target.Release();Object.DestroyImmediate(target);}
        }
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
