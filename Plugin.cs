using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BaboonAPI.Hooks;
using System.Linq;
using TrombLoader.CustomTracks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


namespace TrombLoader
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;

        private void Awake()
        {
            Instance = this;
            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            // var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            // harmony.PatchAll();

            Tracks.EVENT.Register(new TrackLoader());
        }

        public IEnumerator GetAudioClipSync(string path, Action callback = null)
        {
            path = "file:\\\\" + Path.GetFullPath(path);
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

        /// <summary>
        /// Dictionary of track scores
        /// </summary>
        /// <returns>Dictionary of key: trackref, value: [ trackref, letter, score1...5 ]</returns>
        public Dictionary<string, string[]> GetTrackScores()
        {
            return GlobalVariables.localsave.data_trackscores
                .Where(i => i != null && i[0] != null)
                .GroupBy(i => i[0])
                .ToDictionary(i => i.Key, i => i.First());
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
