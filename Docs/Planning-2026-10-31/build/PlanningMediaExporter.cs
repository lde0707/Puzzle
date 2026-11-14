using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Puzzle.Gravity;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;
namespace Puzzle.Editor {
public static class PlanningMediaExporter {
 const BindingFlags F=BindingFlags.Instance|BindingFlags.NonPublic;
 public static void Run(){
  UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene,UnityEditor.SceneManagement.NewSceneMode.Single);
  string output=Path.GetFullPath("../Docs/Planning-2026-10-31/media");Directory.CreateDirectory(output);
  PuzzleUI.Font=Resources.Load<Font>("GravityPuzzle/NanumGothic-Regular");
  var host=new GameObject("Planning isolated controller");var app=host.AddComponent<GravityPuzzleApp>();app.enabled=false;
  void Set(string n,object v)=>typeof(GravityPuzzleApp).GetField(n,F).SetValue(app,v);
  object Get(string n)=>typeof(GravityPuzzleApp).GetField(n,F).GetValue(app);
  void Call(string n,params object[] a){var m=typeof(GravityPuzzleApp).GetMethods(F).Where(x=>x.Name==n&&x.GetParameters().Length>=a.Length).OrderBy(x=>x.GetParameters().Length).First();var ps=m.GetParameters();m.Invoke(app,ps.Select((p,i)=>i<a.Length?a[i]:p.DefaultValue).ToArray());}
  var levels=JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/layouts").text).layouts;
  string[] names={"작은 첫걸음","모퉁이 산책","둘이서 함께","크고 작은 친구","엇갈린 길","셋의 약속","다시 만난 길","좁은 틈 사이","차례를 기다려","서로 다른 길","한 박자 쉬고","모두의 자리","빙글 돌아서","마지막 모퉁이","가벼운 산책","아주 특별한 날"};
  var dict=(System.Collections.Generic.Dictionary<string,string>)Get("levelNames");for(int i=0;i<levels.Length;i++)dict[levels[i].room]=names[i];
  foreach(string n in new[]{"late_levels","resize_levels","blink_levels"})levels=levels.Concat(JsonUtility.FromJson<LayoutDocument>(Resources.Load<TextAsset>("GravityPuzzle/"+n).text).layouts).ToArray();
  levels=PuzzleCampaign.Order(levels);Set("levels",levels);Set("baseLevelCount",16);Set("version","f0dda662");Set("sound",false);
  Set("collection",new PuzzleCollection{wallet=31,owned=Enumerable.Range(0,14).ToArray(),selected=2});
  Set("saves",new ProgressSaveService(Path.Combine(output,"../build/isolated-preview-save")));
  Call("CreateUI");
  var canvas=host.GetComponentInChildren<Canvas>();canvas.GetComponent<CanvasScaler>().enabled=false;canvas.renderMode=RenderMode.WorldSpace;
  var root=(RectTransform)canvas.transform;root.pivot=new Vector2(.5f,.5f);root.sizeDelta=new Vector2(390,844);root.position=Vector3.zero;root.localScale=Vector3.one;root.rotation=Quaternion.identity;
  var safe=(RectTransform)Get("safe");foreach(var b in safe.GetComponents<MonoBehaviour>())b.enabled=false;
  var co=new GameObject("Planning camera",typeof(Camera));var cam=co.GetComponent<Camera>();var rt=new RenderTexture(780,1688,24){antiAliasing=4};
  cam.orthographic=true;cam.orthographicSize=422;cam.transform.position=new Vector3(0,0,-10);cam.targetTexture=rt;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=PuzzleUI.Cream;cam.cullingMask=1<<5;cam.nearClipPlane=.1f;cam.farClipPlane=30;canvas.worldCamera=cam;
  void Capture(string n){foreach(var t in host.GetComponentsInChildren<Transform>(true))t.gameObject.layer=5;Canvas.ForceUpdateCanvases();cam.Render();RenderTexture.active=rt;var tx=new Texture2D(780,1688,TextureFormat.RGBA32,false);tx.ReadPixels(new Rect(0,0,780,1688),0,0);tx.Apply();File.WriteAllBytes(Path.Combine(output,n+".png"),tx.EncodeToPNG());Object.DestroyImmediate(tx);}
  void Level(int i){Call("DismissHome");Call("CloseDialog");try{Call("LoadLevel",i,null,false);}catch(TargetInvocationException e){if(!(e.InnerException is MissingComponentException))throw;foreach(var g in host.GetComponentsInChildren<Graphic>(true)){var cr=g.GetComponent<CanvasRenderer>();if(!cr)cr=g.gameObject.AddComponent<CanvasRenderer>();typeof(Graphic).GetField("m_CanvasRenderer",F)?.SetValue(g,cr);}Call("Layout");((BoardView)Get("board")).Render((PuzzleSession)Get("session"));Call("RefreshStatus");}Call("CancelTouches");}
  try{
   Level(0);Call("OpenHome");Capture("home");Call("OpenHomeSettings");Capture("settings");
   Level(0);Capture("board");Call("OpenPause");Capture("pause");Call("Resume");Call("AskReset");Capture("reset");Call("CancelReset");
   Call("OpenLevels",false);Capture("levels");Level(16);Capture("portal-board");Call("OpenMechanicHelp");Capture("portal-help");
   Level(32);Capture("resize-board");Level(63);Capture("blink-board");
   Level(0);Call("ShowDirectionHint",levels[0].route[0]);Call("TickDirectionHint",.2f);Capture("hint");
   Call("CancelDirectionHint");var drawing=host.GetComponentInChildren<BoardDrawing>();drawing.Editing=true;drawing.ColorIndex=0;drawing.Mark(new Vector2(.18f,.28f),new Vector2(.18f,.76f));drawing.ColorIndex=1;drawing.Mark(new Vector2(.18f,.76f),new Vector2(.79f,.76f));Call("TickDrawing");Capture("drawing");
   Call("OpenCollection",null,false);Capture("wardrobe");
   for(int i=0;i<6;i++)Wave(Path.Combine(output,"draw-cue-"+i+".wav"),SkinDrawAudio.Make(i));
   for(int i=0;i<6;i++)Wave(Path.Combine(output,"clear-note-"+i+".wav"),ClearRewardAudio.MakeClip(i));
   Wave(Path.Combine(output,"clear-finale.wav"),ClearRewardAudio.MakeFinale());
   Debug.Log("PLANNING MEDIA EXPORTED. Isolated save only. No build or deployment.");
  }finally{RenderTexture.active=null;Object.DestroyImmediate(host);Object.DestroyImmediate(co);rt.Release();Object.DestroyImmediate(rt);}
 }
 static void Wave(string p,AudioClip clip){var d=new float[clip.samples*clip.channels];clip.GetData(d,0);using(var w=new BinaryWriter(File.Create(p))){w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(36+d.Length*2);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)clip.channels);w.Write(clip.frequency);w.Write(clip.frequency*clip.channels*2);w.Write((short)(clip.channels*2));w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(d.Length*2);foreach(float x in d)w.Write((short)(Mathf.Clamp(x,-1,1)*32767));}Object.DestroyImmediate(clip);}
}}
