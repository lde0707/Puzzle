using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Puzzle.Gravity;
using UnityEngine;

namespace Puzzle.Editor
{
    public static class WebLoadingChecks
    {
        static void Require(bool value,string message){if(!value)throw new Exception(message);}
        static string Hash(string path){using var sha=SHA256.Create();return Convert.ToBase64String(sha.ComputeHash(File.ReadAllBytes(path)));}
        public static void ValidateAndBuild()
        {
            PackedBrandBuild.Restore();
            var originals=Directory.GetFiles("Assets/Resources/GravityPuzzle/Brand","*.png").ToDictionary(p=>p,Hash);
            using(new PackedBrandBuild())
            {
                foreach(var file in originals.Keys)
                {
                    string name=Path.GetFileNameWithoutExtension(file),path="GravityPuzzle/Brand/"+name;
                    Require(!File.Exists(file),"raw PNG excluded while packing");
                    Require(Hash(Path.ChangeExtension(file,null)+"-packed.bytes")==originals[file],"lossless source bytes preserved");
                    var texture=BrandArt.Load(path);
                    Require(texture&&texture.width>0&&texture.height>0,"packed PNG decoded: "+name);
                    Require(ReferenceEquals(texture,BrandArt.Load(path)),"decoded texture cached");
                }
                DrawPresentationChecks.Run();
            }
            foreach(var file in originals.Keys)Require(File.Exists(file)&&Hash(file)==originals[file],"original restored unchanged");
            MochiClearChecks.Run();
            GravityPrototypeChecks.Run();
            Debug.Log("PASS web loading: original PNG checksums retained; lossless packed art decoded once; single/ten draw, clear, save and campaign checks passed.");
            IphoneTestSetup.BuildGame();
        }
    }
}
