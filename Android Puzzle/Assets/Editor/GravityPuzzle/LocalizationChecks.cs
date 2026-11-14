using System;
using System.Linq;
using Puzzle.Gravity;
using UnityEditor;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class LocalizationChecks
    {
        public static void Run()
        {
            var catalog=JsonUtility.FromJson<PuzzleLanguage.Catalog>(Resources.Load<TextAsset>("GravityPuzzle/localization").text);
            if(catalog.rows.Length<470)throw new Exception("Translation catalog is incomplete");
            if(catalog.rows.Select(x=>x.ko).Distinct().Count()!=catalog.rows.Length)throw new Exception("Duplicate localization keys");
            foreach(string code in PuzzleLanguage.Codes)
            {
                PuzzleLanguage.Set(code,false);
                foreach(var row in catalog.rows)
                {
                    string expected=code=="en"?row.en:code=="zh"?row.zh:code=="ja"?row.ja:row.ko;
                    if(PuzzleLanguage.Translate(row.ko)!=expected)throw new Exception("Translation mismatch: "+code+" "+row.ko);
                }
                if(!PuzzleLanguage.CurrentFont)throw new Exception("Missing font: "+code);
            }
            PuzzleLanguage.Set("bad-code",false);
            if(PuzzleLanguage.Code!="en")throw new Exception("Unknown languages must fall back to English");
            if(PuzzleLanguage.Translate("기본 중력 · 17단계")!="Gravity · Stage 17")throw new Exception("Dynamic stage label");
            if(PuzzleLanguage.Translate("기본 중력 · 1~32단계")!="Gravity · Stages 1–32")throw new Exception("Dynamic stage range");
            var go=new GameObject("Localization test");
            try
            {
                var label=go.AddComponent<PuzzleLocalizedText>();label.text="설정";
                if(label.text!="Settings")throw new Exception("Default English label");
                PuzzleLanguage.Set("zh",false);if(label.text!="设置")throw new Exception("Live Chinese label");
                PuzzleLanguage.Set("ja",false);if(label.text!="設定")throw new Exception("Live Japanese label");
                PuzzleLanguage.Set("ko",false);if(label.text!="설정")throw new Exception("Live Korean label");
                label.text="이동  12";PuzzleLanguage.Set("en",false);
                if(label.text!="Moves  12")throw new Exception("Live dynamic counter");
            }
            finally { UnityEngine.Object.DestroyImmediate(go);PuzzleLanguage.Set("en",false); }
            Debug.Log("LOCALIZATION CHECKS PASS: 4 languages, catalog, dynamic counters, live switching, English fallback.");
        }
        public static void Build(){Run();IphoneTestSetup.BuildGame();}
    }
}
