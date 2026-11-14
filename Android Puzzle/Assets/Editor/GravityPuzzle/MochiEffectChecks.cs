using System;
using System.Collections;
using System.Reflection;
using Puzzle.Gravity;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace Puzzle.Editor
{
    public static class MochiEffectChecks
    {
        public static void Run()
        {
            foreach(string name in new[]{"mochi-capsule","mochi-draw-panel"})
            {
                string path="Assets/Resources/GravityPuzzle/Brand/"+name+".png";
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);
                importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;
                importer.alphaIsTransparency=true;importer.npotScale=TextureImporterNPOTScale.None;
                importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
                importer.filterMode=FilterMode.Bilinear;importer.wrapMode=TextureWrapMode.Clamp;
                importer.SaveAndReimport();
                Require(Resources.Load<Texture2D>("GravityPuzzle/Brand/"+name)!=null,"painted asset loads");
            }
            var root=PuzzleUI.Rect("Size and origin regression",null);
            try
            {
                var fx=root.gameObject.AddComponent<SkinTravelEffects>();
                IList Particles()=>(IList)typeof(SkinTravelEffects).GetField("particles",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(fx);
                T Field<T>(object p,string key)=>(T)p.GetType().GetField(key).GetValue(p);
                var origin=new Vector2(140,180);
                for(int id=0;id<PuzzleCollection.Names.Length;id++)
                {
                    fx.Clear();fx.Arrive(id,0,origin,40);
                    var small=new ArrayList(Particles());
                    fx.Clear();fx.Arrive(id,0,origin,80);
                    var large=Particles();Require(small.Count==large.Count,"size does not multiply particle load");
                    for(int i=0;i<large.Count;i++)
                    {
                        Require(Field<Vector2>(large[i],"position")==origin,"every arrival particle originates at body centre");
                        Require(Mathf.Approximately(Field<float>(large[i],"size"),Field<float>(small[i],"size")*2),"2x unit has 2x motif size");
                        Require((Field<Vector2>(large[i],"velocity")-Field<Vector2>(small[i],"velocity")*2).sqrMagnitude<.001f,"2x unit has 2x burst radius");
                    }
                    fx.Clear();fx.Trail(0,id,0,origin,80,true,.04f,false);
                    fx.Trail(0,id,0,origin+Vector2.right*20,80,true,.04f,false);
                    Require(fx.ParticleCount>0,"every skin emits a trail");
                    foreach(var p in Particles())Require(Mathf.Approximately(Field<Vector2>(p,"position").y,origin.y),"trail starts on centre line, not side lanes");
                }
            }
            finally{Object.DestroyImmediate(root.gameObject);}
            for(int size=1;size<=2;size++)
            {
                var boardRoot=PuzzleUI.Rect("Rendered size "+size,null);
                try
                {
                    var level=new PuzzleDefinition{room="centre-check",width=8,height=4,walls=new Cell[0],
                        cargo=new[]{new CargoDefinition{x=0,y=1,gx=8-size,gy=1,size=size}},route=new[]{3}};
                    var run=new PuzzleSession(level);var board=new BoardView(boardRoot);
                    board.Build(level);board.Layout(480,240,240,120);board.Render(run);
                    run.Command(3);board.BeginTilt(run);run.Tick(100);board.Render(run,.01f);
                    var fx=boardRoot.GetComponentInChildren<SkinTravelEffects>();
                    var particles=(IList)typeof(SkinTravelEffects).GetField("particles",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(fx);
                    Require(particles.Count>0,"actual board arrival emits for size "+size);
                    var expected=new Vector2((8-size+size*.5f)*60,(1+size*.5f)*60);
                    foreach(var p in particles)
                    {
                        var actual=(Vector2)p.GetType().GetField("position").GetValue(p);
                        Require((actual-expected).sqrMagnitude<.01f,"actual rendered centre for size "+size);
                    }
                    Require(fx.transform.parent.GetSiblingIndex()==boardRoot.childCount-1&&!fx.raycastTarget,"foreground effects remain non-interactive");
                }
                finally{Object.DestroyImmediate(boardRoot.gameObject);}
            }
            SkinEffectChecks.Run();
            DrawPresentationChecks.Run();
            Debug.Log("PASS mochi presentation: forty centred emitters; double-size effects and radii; generated assets; single/ten draw, fast-play animation, pause, skip and inventory checks.");
        }
        public static void ValidateAndBuild(){Run();IphoneTestSetup.BuildGame();}
        static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    }
}
