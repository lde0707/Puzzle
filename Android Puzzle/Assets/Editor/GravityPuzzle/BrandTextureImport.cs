using UnityEditor;
using UnityEngine;

namespace Puzzle.Editor
{
    // Keep the small header wordmark sharp and preserve its transparent edges.
    public sealed class BrandTextureImport : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/Resources/GravityPuzzle/Brand/") || !assetPath.EndsWith(".png"))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;
            importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency=true;
            importer.mipmapEnabled=false;
            importer.npotScale=TextureImporterNPOTScale.None;
            importer.maxTextureSize=2048;
            importer.wrapMode=TextureWrapMode.Clamp;
            importer.filterMode=FilterMode.Bilinear;
        }
    }
}
