using System.Collections.Generic;
using UnityEngine;

namespace Puzzle.Gravity
{
    public static class BrandArt
    {
        private static readonly Dictionary<string,Texture2D> Cache=new Dictionary<string,Texture2D>();
        public static Texture2D Load(string path)
        {
            if(Cache.TryGetValue(path,out var cached)&&cached)return cached;
            var texture=Resources.Load<Texture2D>(path);
            if(!texture)
            {
                var packed=Resources.Load<TextAsset>(path+"-packed");
                if(!packed)return null;
                texture=new Texture2D(2,2,TextureFormat.RGBA32,false);
                if(!texture.LoadImage(packed.bytes,true)){Object.Destroy(texture);Resources.UnloadAsset(packed);return null;}
                texture.name=path.Substring(path.LastIndexOf('/')+1);
                texture.filterMode=FilterMode.Bilinear;texture.wrapMode=TextureWrapMode.Clamp;
                Resources.UnloadAsset(packed);
            }
            Cache[path]=texture;return texture;
        }
    }
}
