using System;
using System.IO;
using UnityEditor;

namespace Puzzle.Editor
{
    // Preserve original PNGs and import settings. Only the Web player contains
    // their lossless PNG bytes, decoded on first use instead of raw RGBA pixels.
    public sealed class PackedBrandBuild : IDisposable
    {
        const string Source="Assets/Resources/GravityPuzzle/Brand";
        const string Stash="Assets/Editor/WebBrandBuildSources";
        public PackedBrandBuild()
        {
            Restore();Directory.CreateDirectory(Stash);AssetDatabase.Refresh();
            try
            {
                foreach(string file in Directory.GetFiles(Source,"*.png"))
                {
                    string name=Path.GetFileName(file),destination=Stash+"/"+name;
                    string error=AssetDatabase.MoveAsset(file.Replace('\\','/'),destination);
                    if(!string.IsNullOrEmpty(error))throw new Exception(error);
                    string packed=Source+"/"+Path.GetFileNameWithoutExtension(name)+"-packed.bytes";
                    File.Copy(destination,packed,true);AssetDatabase.ImportAsset(packed);
                }
                AssetDatabase.SaveAssets();
            }
            catch{Dispose();throw;}
        }
        public static void Restore()
        {
            if(!Directory.Exists(Stash))return;
            foreach(string file in Directory.GetFiles(Stash,"*.png"))
            {
                string name=Path.GetFileName(file),original=Source+"/"+name;
                string error=AssetDatabase.MoveAsset(file.Replace('\\','/'),original);
                if(!string.IsNullOrEmpty(error))throw new Exception("Restore original brand art: "+error);
                AssetDatabase.DeleteAsset(Source+"/"+Path.GetFileNameWithoutExtension(name)+"-packed.bytes");
            }
            AssetDatabase.SaveAssets();
        }
        public void Dispose()=>Restore();
    }
}
