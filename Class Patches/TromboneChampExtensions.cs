using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace TrombLoader.Class_Patches
{
    public static class TromboneChampExtensions
    {
        public static void DisableBackground(this BGController bgController, bool enable = false)
        {
            DisableLayer(bgController.bgplane.gameObject, enable);
            DisableLayer(bgController.bgplane,enable);
            DisableLayer(bgController.bgimg1,enable);
            DisableLayer(bgController.bgimg2,enable);
            DisableLayer(bgController.img1fade,enable);
            DisableLayer(bgController.fg1,enable);
            DisableLayer(bgController.fg2,enable);
            DisableLayer(bgController.fg3,enable);
            DisableLayer(bgController.fg4,enable);
            DisableLayer(bgController.fg5,enable);
            DisableLayer(bgController.fg6,enable);
            DisableLayer(bgController.fg7,enable);
            DisableLayer(bgController.fg8,enable);
        }
        
        //Sets a bg with just a bg and a fg
        public static void SetBasicBackground(this BGController bgController, Sprite bg)
        {
            try
            {
                bgController.bgplane.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = bg;
                
                DisableLayer(bgController.bgplane, true);
                
            }
            catch
            {
                Plugin.LogError("Failed to set up custom BG");
            }

        }

        public static async void PlayVideoDelayed(this VideoPlayer videoPlayer, int delay = 2500)
        {
            await Task.Delay(delay);

            videoPlayer?.Play();
        }

        public static void SetVideoBackground(this BGController bgController, string url)
        {
            DisableLayer(bgController.bgplane, true);

            var planeObject = bgController.bgplane.transform.GetChild(0);
            var videoPlayer = planeObject.GetComponent<VideoPlayer>() ?? planeObject.gameObject.AddComponent<VideoPlayer>();

            planeObject.GetComponent<SpriteRenderer>().color = Color.black;
            videoPlayer.url = url;
            videoPlayer.isLooping = true;
            videoPlayer.playOnAwake = false;
            videoPlayer.skipOnDrop = true;
            videoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
            videoPlayer.targetCamera = bgController.transform.parent.GetChild(1).GetComponent<Camera>();

            videoPlayer.enabled = true;

            videoPlayer.Pause();
            videoPlayer.PlayVideoDelayed();
        }

        public static void DisableLayer(GameObject obj, bool enable = false)
        {
            if (obj != null) obj.SetActive(enable);
        }
        
        public static void DisableLayer(Image obj, bool enable = false)
        {
            if (obj != null) obj.gameObject.SetActive(enable);
        }
    }        
}

