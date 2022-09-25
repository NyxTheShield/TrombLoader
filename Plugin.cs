using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;


namespace TrombLoader
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public AudioClip currentClip;
        
        private void Awake()
        {
            instance = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            var harmony = new Harmony(PluginInfo.PLUGIN_GUID);
            harmony.PatchAll();

#if DEBUG
            Logger.LogInfo("NYX: Fixing Latency!!!");
            AudioConfiguration configuration = AudioSettings.GetConfiguration();
            configuration.dspBufferSize = 256;
            AudioSettings.Reset(configuration);
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
                    Debug.Log("Loaded File:" + path);
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
                Debug.Log("Loaded File:" + path);
                callback?.Invoke();
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                yield return DownloadHandlerAudioClip.GetContent(www);
                
            }
        }
          
        public IEnumerator GetSpriteFromPath(string path)
        {   
            path = "file:\\\\" + Path.GetFullPath(path);
            Debug.Log($"Web Request Texture: {path}");
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
                Debug.Log($"Got Texture!! null?:{texture == null}");
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
    }
}
