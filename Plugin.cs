using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


namespace TrombLoader
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance;
        public AudioClip currentClip;
        
        private void Awake()
        {
            Instance = this;
            LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

#if DEBUG
            
#endif
        }
        
        //Load AudioClip
        public IEnumerator GetAudioClip(string path, Action callback = null)
        {
            path = "file:\\\\" + Path.GetFullPath(path);
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
            ((DownloadHandlerAudioClip)www.downloadHandler).streamAudio = true;
            yield return www.SendWebRequest();
            if (www.isNetworkError)
            {
                    Debug.Log(www.error);
            }
            else 
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                this.currentClip = clip;
                LogDebug("Loaded File:" + path);
                callback?.Invoke();
            }
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
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                yield return DownloadHandlerAudioClip.GetContent(www);
                
            }
        }
          
        public IEnumerator GetSpriteFromPath(string path)
        {   
            path = "file:\\\\" + Path.GetFullPath(path);
            LogDebug($"Web Request Texture: {path}");
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(path);
            yield return www.SendWebRequest();
            
            while (!www.isDone)
                yield return null;
            
            if (www.isNetworkError || www.isHttpError)
            {
                yield return www.error;
            }
            else 
            {
                var texture = ((DownloadHandlerTexture) www.downloadHandler).texture;
                LogDebug($"Got Texture!! null?:{texture == null}");
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
                yield return sprite;
            }
        } 
        
        public IEnumerator Request(string path)
        {
            path = "file:\\\\"+Path.GetFullPath(path);
            var www = new WWW(path);
 
            while (!www.isDone)
                yield return null;

            if (string.IsNullOrEmpty(www.error))
                yield return www.GetAudioClip();
            else
                yield return www.error;
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
