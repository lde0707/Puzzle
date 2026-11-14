using System;
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
            Check("GravityPrototypeChecks", GravityPrototypeChecks.Run);
            Check("HintChecks", HintChecks.Run);
            Check("BatchDrawChecks", BatchDrawChecks.Run);
            Check("ClearFinaleChecks", ClearFinaleChecks.Run);
            Check("WalletFlightChecks", WalletFlightChecks.Run);
            Check("DrawPresentationChecks", DrawPresentationChecks.Run);
            Check("MochiClearChecks", MochiClearChecks.Run);
            Check("MochiEffectChecks", MochiEffectChecks.Run);
            Check("SkinEffectChecks", SkinEffectChecks.Run);
            Check("StageDetailPresentationChecks", StageDetailPresentationChecks.Run);
            Check("AmbienceChecks", AuditAmbienceChecks.ValidateAndBuild);
            Check("CommunityChecks", AuditCommunityChecks.ValidateAndBuild);
            Check("DrawingUndoChecks", AuditDrawingUndoChecks.ValidateAndBuild);
            Check("ProgressTransferChecks", AuditProgressTransferChecks.ValidateAndBuild);
            Check("LifetimeStarsChecks", AuditLifetimeStarsChecks.ValidateAndBuild);
            Check("StageDetailChecks", AuditStageDetailChecks.ValidateAndBuild);
  if(report.results.Exists(x=>!x.passed))throw new Exception("Release audit has failures; see Logs/release-audit-results.json");
  Debug.Log("RELEASE AUDIT ALL "+report.results.Count+" SUITES PASSED");
 }
}}
