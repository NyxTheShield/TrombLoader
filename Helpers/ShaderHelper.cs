using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace TrombLoader.Helpers
{
    public class ShaderHelper
    {
        public ShaderHelper()
        {
            ShaderCache = LoadShaderBundleFromPath(Plugin.Instance.Info.Location, Application.platform);
        }

        // Local shader asset caching for non-windows platforms
        // Because assetbundles are built for windows, platforms like MacOS will have invalid shaders when it's a shader that's not already included in the game build.
        // This can be somewhat fixed with a "master list" of default unity shaders, but custom shaders will still have to include something (todo)
        public Dictionary<string, Shader> ShaderCache { get; private set; } = null;

        public Dictionary<string, Shader> LoadShaderBundleFromPath(string path, RuntimePlatform? platform = null)
        {
            // more robust "assetbundle platform detection" could be useful in the future
            Dictionary<string, Shader> shaderMap = new();
            List<string> shaderBundleFileExtensions = new() { ".DONOTDELETE", ".shaderbundle", ".shaders" };
            List<string> bundlePaths = new();

            // shader bundle loading is only necessary on mac for now.
            // otherwise, only load if null, which means every shaderbundle in the directory will be loaded.
            if (platform == RuntimePlatform.OSXPlayer)
            {
                foreach(string fileExtension in shaderBundleFileExtensions)
                {
                    var files = Directory.GetFiles(path, fileExtension, SearchOption.TopDirectoryOnly);
                    foreach(var file in files) {
                        if((file.Contains("_MACOS") || file.Contains("_OSX")) && !bundlePaths.Contains(file)) bundlePaths.Add(file);
                    }
                }
            }
            else if (platform == null)
            {
                foreach (string fileExtension in shaderBundleFileExtensions)
                {
                    var files = Directory.GetFiles(path, fileExtension, SearchOption.TopDirectoryOnly);
                    foreach (var file in files)
                    {
                        if (!bundlePaths.Contains(file)) bundlePaths.Add(file);
                    }
                }
            }

            foreach(var bundlePath in bundlePaths)
            {
                if (!File.Exists(bundlePath)) continue;

                // Each child of prefab contains a cube with a unique shader/material
                // This dictionary structuring *May* have unintended consequences if somebody creates a shader with the exact same name as a base unity shader
                // Unfortunately I couldn't find an easy way to get a cross-platform hash/ID, so names will have to suffice 
                var bundle = AssetBundle.LoadFromFile(bundlePath);
                var asset = bundle.LoadAsset("Assets/_Shaders.prefab") as GameObject;

                foreach (var renderer in asset.GetComponentsInChildren<Renderer>())
                {
                    var shader = renderer?.sharedMaterial?.shader;
                    if (shader != null && !shaderMap.ContainsKey(shader.name))
                    {
                        shaderMap.Add(shader.name, shader);
                    }
                }
            }

            return shaderMap;
        }
    }
}
