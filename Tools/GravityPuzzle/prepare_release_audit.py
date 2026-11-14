from pathlib import Path
import json, re

root = Path(__file__).resolve().parents[2]
editor = root / 'Android Puzzle/Assets/Editor/GravityPuzzle'
target = root / 'Android Puzzle/Assets/Editor/ReleaseAudit'
target.mkdir(exist_ok=True)
coupled = ['AmbienceChecks','CommunityChecks','DrawingUndoChecks','ProgressTransferChecks','LifetimeStarsChecks','StageDetailChecks']
for name in coupled:
    source = (editor / (name+'.cs')).read_text(encoding='utf-8-sig')
    source = source.replace('class '+name, 'class Audit'+name)
    source = re.sub(r'\b(?:IphoneTestSetup\.BuildGame|ClearFinaleChecks\.ValidateAndBuild|CommunityChecks\.ValidateAndBuild|GravityPrototypeChecks\.Run|HintChecks\.Run)\(\);', '', source)
    (target / ('Audit'+name+'.cs')).write_text(source,encoding='utf-8')
direct = ['GravityPrototypeChecks','HintChecks','BatchDrawChecks','ClearFinaleChecks','WalletFlightChecks','DrawPresentationChecks','MochiClearChecks','MochiEffectChecks','SkinEffectChecks','StageDetailPresentationChecks']
calls = '\n'.join(f'            Check("{n}", {n}.Run);' for n in direct)
calls += '\n'+'\n'.join(f'            Check("{n}", Audit{n}.ValidateAndBuild);' for n in coupled)
source = '''using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Puzzle.Editor {
public static class ReleaseAuditRunner {
 [Serializable] public class Result { public string name; public bool passed; public string error; public double seconds; }
 [Serializable] public class Report { public string date; public List<Result> results=new List<Result>(); }
 static Report report=new Report();
 static void Check(string name,Action action) { var start=DateTime.UtcNow;var result=new Result{name=name};try {action();result.passed=true;}catch(Exception ex){result.error=ex.ToString();Debug.LogError("RELEASE AUDIT FAIL "+name+": "+ex);}result.seconds=(DateTime.UtcNow-start).TotalSeconds;report.results.Add(result);Debug.Log("RELEASE AUDIT "+name+" "+result.passed);File.WriteAllText("Logs/release-audit-results.json",JsonUtility.ToJson(report,true)); }
 public static void Run() { Directory.CreateDirectory("Logs"); report.date=DateTime.UtcNow.ToString("o");
CALLS
  if(report.results.Exists(x=>!x.passed))throw new Exception("Release audit has failures; see Logs/release-audit-results.json");
  Debug.Log("RELEASE AUDIT ALL "+report.results.Count+" SUITES PASSED");
 }
}}
'''.replace('CALLS',calls)
(target/'ReleaseAuditRunner.cs').write_text(source,encoding='utf-8')
backup={str(p.relative_to(root)):p.read_text(encoding='utf-8') for p in (root/'Android Puzzle/Assets/Resources/GravityPuzzle/Brand').glob('*.meta')}
(root/'Android Puzzle/Logs/release-audit-importer-backup.json').write_text(json.dumps(backup),encoding='utf-8')
print('Prepared isolated build-free wrappers for 16 suites; importer settings backed up.')
