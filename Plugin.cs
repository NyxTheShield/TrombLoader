using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BaboonAPI.Hooks.Initializer;
using BaboonAPI.Hooks.Tracks;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using TrombLoader.CustomTracks;
using TrombLoader.Patch;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace TrombLoader
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public ConfigEntry<int> beatsToShow;
        
        private Harmony _harmony = new(PluginInfo.PLUGIN_GUID);

        private void Awake()
        {
            var customFile = new ConfigFile(Path.Combine(Paths.ConfigPath, "TrombLoader.cfg"), true);
            beatsToShow = customFile.Bind("General", "Note Display Limit", 64, "The maximum amount of notes displayed on screen at once.");

            Instance = this;
            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");

            GameInitializationEvent.Register(Info, TryInitialize);
            TrackRegistrationEvent.EVENT.Register(new TrackLoader());
        }

        private void TryInitialize()
        {
            _harmony.PatchAll();
        }

        public IEnumerator GetAudioClipSync(string path, Action callback = null)
        {
            path = "file://" + Path.GetFullPath(path);
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
            ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;
            yield return www.SendWebRequest();
            while (!www.isDone)
                yield return null;
            
            if (www.isNetworkError || www.isHttpError)
            {
                yield return www.error;
            }
            else 
            {
                LogDebug("Loaded File:" + path);
                callback?.Invoke();
                yield return DownloadHandlerAudioClip.GetContent(www);
            }
        }

        public void LoadGameplayScene()
        {
            SceneManager.LoadSceneAsync("gameplay", LoadSceneMode.Single);
        }

        // Local shader asset caching for non-windows platforms
        // Because assetbundles are built for windows, platforms like MacOS will have invalid shaders when it's a shader that's not already included in the game build.
        // This can be somewhat fixed with a "master list" of default unity shaders, but custom shaders will still have to include something (todo)
        private Dictionary<string, Shader> _shaderAssets = null;
        public Dictionary<string, Shader> ShaderAssets { 
            get {
                if (_shaderAssets != null) return _shaderAssets;

                var bundleName = "";
                if (Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    bundleName = "ShaderBundle_WIN64.DONOTDELETE";
                    // unnecessary on windows for now. filename is just for testing and potential future compatibility.
                }
                if (Application.platform == RuntimePlatform.OSXPlayer)
                {
                    bundleName = "ShaderBundle_OSX.DONOTDELETE";
                }

                var bundlePath = Path.Combine(Path.GetDirectoryName(Instance.Info.Location), bundleName);

                if (bundleName.IsNullOrWhiteSpace() || !File.Exists(bundlePath))
                {
                    _shaderAssets = new();
                    return _shaderAssets;
                }

                // Each child of prefab contains a cube with a unique shader/material
                // This dictionary structuring *May* have unintended consequences if somebody creates a shader with the exact same name as a base unity shader
                // Unfortunately I couldn't find an easy way to get a cross-platform hash/ID, so names will have to suffice 
                var bundle = AssetBundle.LoadFromFile(bundlePath);
                var asset = bundle.LoadAsset("Assets/_Shaders.prefab") as GameObject;
                Dictionary<string, Shader> shaderMap = new();

                foreach (var renderer in asset.GetComponentsInChildren<Renderer>())
                {
                    var shader = renderer?.sharedMaterial?.shader;
                    if (shader != null && !shaderMap.ContainsKey(shader.name))
                    {
                        shaderMap.Add(shader.name, shader);
                    }
                }

                _shaderAssets = shaderMap;
                return _shaderAssets;
            }
        }

        #region logging
        internal static void LogDebug(string message) => Instance.Log(message, LogLevel.Debug);
        internal static void LogInfo(string message) => Instance.Log(message, LogLevel.Info);
        internal static void LogWarning(string message) => Instance.Log(message, LogLevel.Warning);
        internal static void LogError(string message) => Instance.Log(message, LogLevel.Error);
        private void Log(string message, LogLevel logLevel) => Logger.Log(logLevel, message);
        #endregion
    }
}
