using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace TrombLoader.Helpers
{
    public class ShaderHelper
    {
        public ShaderHelper()
        {
            if(Application.platform == RuntimePlatform.OSXPlayer)
            {
                LoadBaseGameShaders();

                ShaderCache = LoadShaderBundleFromPath(Plugin.Instance.Info.Location, Application.platform);
            }
            else
            {
                ShaderCache = new();
            }
        }

        // Shader asset caching for metal and opengl
        // This includes some shaders included in the Unity Editor but NOT the base game
        public Dictionary<string, Shader> ShaderCache { get; private set; } = null;

        public Dictionary<string, Shader> BaseGameShaderCache { get; private set; } = new();

        // I cannot describe how much I did not want to do this. Unfortunately, unity has forced my hand.
        // Shader deserialization will sometimes break *despite* the shader already being in the game IF the assetbundle has a reference to the same shader for the wrong platform.
        // Unfortunately, this means you need a reference to every shader **before** loading custom assetbundles.
        // Resources.LoadObjectsOfTypeAll does not get references to unloaded shaders, which includes many necessary and common ones if you're getting references in the menu
        // All of these factors lead to me ripping the shader list from the macOS depot for Trombone Champ v1.093.
        // This list *may* need to be updated if major shader changes are made in base game. Ideally this would be done automatically, but shipping a library like AssetTools.NET is probably too unwieldy.
        public List<string> BaseGameShaderNames = new() { "Custom/WavySpriteLit", "Custom/WavySpriteUnlit", "FX/Flare", "FX/Gem", "GUI/Text Shader", "Hidden/BlitCopy", "Hidden/BlitCopyDepth", "Hidden/BlitCopyWithDepth", "Hidden/BlitToDepth", "Hidden/BlitToDepth/MSAA", "Hidden/Compositing", "Hidden/ConvertTexture", "Hidden/CubeBlend", "Hidden/CubeBlur", "Hidden/CubeCopy", "Hidden/FrameDebuggerRenderTargetDisplay", "Hidden/Internal-Colored", "Hidden/Internal-CombineDepthNormals", "Hidden/Internal-CubemapToEquirect", "Hidden/Internal-DeferredReflections", "Hidden/Internal-DeferredShading", "Hidden/Internal-DepthNormalsTexture", "Hidden/Internal-Flare", "Hidden/Internal-GUIRoundedRect", "Hidden/Internal-GUIRoundedRectWithColorPerBorder", "Hidden/Internal-GUITexture", "Hidden/Internal-GUITextureBlit", "Hidden/Internal-GUITextureClip", "Hidden/Internal-GUITextureClipText", "Hidden/Internal-Halo", "Hidden/Internal-MotionVectors", "Hidden/Internal-ODSWorldTexture", "Hidden/Internal-PrePassLighting", "Hidden/Internal-ScreenSpaceShadows", "Hidden/Internal-StencilWrite", "Hidden/Internal-UIRAtlasBlitCopy", "Hidden/Internal-UIRDefault", "Hidden/InternalClear", "Hidden/InternalErrorShader", "Hidden/Post FX/Ambient Occlusion", "Hidden/Post FX/Blit", "Hidden/Post FX/Bloom", "Hidden/Post FX/Builtin Debug Views", "Hidden/Post FX/Depth Of Field", "Hidden/Post FX/Eye Adaptation", "Hidden/Post FX/Fog", "Hidden/Post FX/FXAA", "Hidden/Post FX/Grain Generator", "Hidden/Post FX/Lut Generator", "Hidden/Post FX/Motion Blur", "Hidden/Post FX/Screen Space Reflection", "Hidden/Post FX/Temporal Anti-aliasing", "hidden/SuperSystems/Wireframe-Global", "hidden/SuperSystems/Wireframe-Shaded-Unlit-Global", "hidden/SuperSystems/Wireframe-Transparent-Culled-Global", "hidden/SuperSystems/Wireframe-Transparent-Global", "Hidden/TextCore/Distance Field SSD", "Hidden/VideoComposite", "Hidden/VideoDecode", "Hidden/VideoDecodeOSX", "Hidden/VR/BlitFromTex2DToTexArraySlice", "Hidden/VR/BlitTexArraySlice", "Legacy Shaders/Diffuse", "Legacy Shaders/Particles/Additive", "Legacy Shaders/Particles/Alpha Blended Premultiply", "Legacy Shaders/Particles/Alpha Blended", "Legacy Shaders/Transparent/VertexLit", "Legacy Shaders/VertexLit", "Mobile/Unlit (Supports Lightmap)", "Particles/Standard Unlit", "Skybox/Procedural", "Spaventacorvi/Glitter/Glitter F - Bumped Specular", "Spaventacorvi/Holographic/Holo D - Specular Textured", "Sprites/Default", "Sprites/Diffuse", "Sprites/Mask", "Standard (Specular setup)", "Standard", "SuperSystems/Wireframe-Transparent-Culled", "TextMeshPro/Bitmap Custom Atlas", "TextMeshPro/Bitmap", "TextMeshPro/Distance Field (Surface)", "TextMeshPro/Distance Field Overlay", "TextMeshPro/Distance Field", "TextMeshPro/Mobile/Bitmap", "TextMeshPro/Mobile/Distance Field (Surface)", "TextMeshPro/Mobile/Distance Field - Masking", "TextMeshPro/Mobile/Distance Field Overlay", "TextMeshPro/Mobile/Distance Field", "TextMeshPro/Sprite", "UI/Default", "Hidden/Post FX/Uber" };

        // The entire list is included for posterity, but Hidden/TextMeshPro shaders don't need to be loaded with current implementation
        public List<string> BaseGameShadersFamiliesToNotLoad = new() { "Hidden", "TextMeshPro" };

        // IMPORTANT: RUN AS SOON AS POSSIBLE, IF CUSTOM SONG ASSETBUNDLES ARE LOADED BEFORE THIS RUNS IT WILL **NOT** WORK
        private void LoadBaseGameShaders()
        {
            int failedShaders = 0;
            foreach (var baseGameShaderName in BaseGameShaderNames)
            {
                if (BaseGameShadersFamiliesToNotLoad.Any(e => baseGameShaderName.ToLower().StartsWith(e.ToLower()))) continue;

                var baseGameShader = Shader.Find(baseGameShaderName);

                if (baseGameShader == null)
                {
                    Plugin.LogInfo($"Failed to cache {baseGameShaderName}");
                    failedShaders++;
                    continue;
                }

                BaseGameShaderCache.Add(baseGameShader.name, baseGameShader);
            }

            Plugin.LogInfo($"{BaseGameShaderCache.Count} Base game shaders loaded. {failedShaders} failed to load.");
        }

        public Dictionary<string, Shader> LoadShaderBundleFromPath(string path, RuntimePlatform? platform = null)
        {
            // more robust "assetbundle platform detection" could be useful in the future
            Dictionary<string, Shader> shaderMap = new();
            List<string> shaderBundleFileExtensions = new() { "*.DONOTDELETE", "*.shaderbundle", "*.shaders" };
            List<string> bundlePaths = new();

            // shader bundle loading is only necessary on mac for now.
            // otherwise, only load if null, which means every shaderbundle in the directory will be loaded.
            if (platform == RuntimePlatform.OSXPlayer)
            {
                foreach(string fileExtension in shaderBundleFileExtensions)
                {
                    var files = Directory.GetFiles(Path.GetDirectoryName(path), fileExtension, SearchOption.TopDirectoryOnly);
                    foreach(var file in files) 
                    {
                        if((file.ToLower().Contains("macos") || file.ToLower().Contains("osx")) && !bundlePaths.Contains(file)) bundlePaths.Add(file);
                    }
                }
            }
            else if (platform == null)
            {
                foreach (string fileExtension in shaderBundleFileExtensions)
                {
                    var files = Directory.GetFiles(Path.GetDirectoryName(path), fileExtension, SearchOption.TopDirectoryOnly);
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
                        if (BaseGameShaderCache.ContainsKey(shader.name)) continue; // unnecessary to cache shaders that already exist in base game, as we can just use those
                        shaderMap.Add(shader.name, shader);
                    }
                }

                bundle.Unload(false);
            }

            return shaderMap;
        }
    }
}
