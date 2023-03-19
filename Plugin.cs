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
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace TrombLoader
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public ShaderHelper ShaderHelper;
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

            ShaderHelper = new();
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
                callback?.Invoke();
                yield return DownloadHandlerAudioClip.GetContent(www);
            }
        }

        public void LoadGameplayScene()
        {
            SceneManager.LoadSceneAsync("gameplay", LoadSceneMode.Single);
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
