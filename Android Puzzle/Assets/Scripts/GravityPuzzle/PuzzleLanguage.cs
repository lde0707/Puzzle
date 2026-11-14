using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

namespace Puzzle.Gravity
{
    public static class PuzzleLanguage
    {
        [Serializable] public sealed class Entry { public string ko,en,zh,ja; }
        [Serializable] public sealed class Catalog { public Entry[] rows; }
        public static readonly string[] Codes={"en","ko","zh","ja"};
        public static readonly string[] Names={"English","한국어","简体中文","日本語"};
        public static string Code { get; private set; }="en";
        public static event Action Changed;
        private const string Preference="degul.language.v1";
        private static Dictionary<string,Entry> entries;
        private static Regex fragments;
        private static readonly Dictionary<string,Font> fonts=new Dictionary<string,Font>();
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void DegulSetLanguage(string code);
#endif
        public static void Initialize() { Load(); Set(CrazyPortal.Enabled?(CrazyPortal.Read(Preference)??"en"):PlayerPrefs.GetString(Preference,"en"),false); }
        private static void Load()
        {
            if(entries!=null)return;
            var catalog=JsonUtility.FromJson<Catalog>(Resources.Load<TextAsset>("GravityPuzzle/localization").text);
            entries=catalog.rows.ToDictionary(x=>x.ko);
            fragments=new Regex(string.Join("|",entries.Keys.OrderByDescending(x=>x.Length).Select(Regex.Escape)));
        }
        public static void Set(string code,bool persist=true)
        {
            Load();Code=Codes.Contains(code)?code:"en";
            if(persist)
            {
                if(CrazyPortal.Enabled){try{CrazyPortal.Write(Preference,Code);}catch(Exception e){Debug.LogWarning(e.Message);}}
                else {PlayerPrefs.SetString(Preference,Code);PlayerPrefs.Save();}
            }
            Changed?.Invoke();
#if UNITY_WEBGL && !UNITY_EDITOR
            DegulSetLanguage(Code);
#endif
        }
        public static string Translate(string value)
        {
            if(string.IsNullOrEmpty(value)||Code=="ko")return value;
            Load();
            string Pick(Entry e)=>Code=="zh"?e.zh:Code=="ja"?e.ja:e.en;
            if(entries.TryGetValue(value,out var row))return Pick(row);
            if(Code=="en")
            {
                value=Regex.Replace(value,@"(\d+)[~–-](\d+)단계",m=>"Stages "+m.Groups[1].Value+"–"+m.Groups[2].Value);
                value=Regex.Replace(value,@"(\d+)단계",m=>"Stage "+m.Groups[1].Value);
            }
            else if(Code=="zh")value=Regex.Replace(value,@"(\d+)단계",m=>"第"+m.Groups[1].Value+"关");
            else if(Code=="ja")value=Regex.Replace(value,@"(\d+)단계",m=>"ステージ"+m.Groups[1].Value);
            return fragments.Replace(value,m=>Pick(entries[m.Value]));
        }
        public static Font CurrentFont
        {
            get
            {
                string path=Code=="zh"?"DegulCJK-SC":Code=="ja"?"DegulCJK-JP":"NanumGothic-Regular";
                if(!fonts.TryGetValue(path,out var font))fonts[path]=font=Resources.Load<Font>("GravityPuzzle/"+path);
                return font?font:PuzzleUI.Font;
            }
        }
    }
    // Keep the source string so changing language also updates dynamic HUD labels.
    public sealed class PuzzleLocalizedText : Text
    {
        private string source="";
        private int lastRequestedSize=-1;
        public override string text { get=>base.text; set { source=value??"";Refresh(); } }
        protected override void OnEnable(){base.OnEnable();PuzzleLanguage.Changed+=Refresh;Refresh();}
        protected override void OnDisable(){PuzzleLanguage.Changed-=Refresh;base.OnDisable();}
        public void Refresh(){font=PuzzleLanguage.CurrentFont;base.text=PuzzleLanguage.Translate(source);}
        private void LateUpdate()
        {
            // Callers adjust Text.fontSize for large-text mode and hero buttons.
            if(fontSize==lastRequestedSize)return;
            lastRequestedSize=fontSize;
            if(resizeTextForBestFit){resizeTextMaxSize=fontSize;resizeTextMinSize=Mathf.Min(resizeTextMinSize,fontSize);}
        }
    }
}
