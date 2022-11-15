using System.IO;
using HarmonyLib;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.Video;

namespace TrombLoader.Class_Patches
{
    [HarmonyPatch(typeof(BGController))]
    [HarmonyPatch("setUpBGControllerRefsDelayed")]
    public class BGControllerPatch
    {
        //Patch to load a custom background

        public static string BGEffect = "none";
        static void Postfix(BGController __instance)
        {
            var trackReference = GlobalVariables.chosen_track;

            if (Globals.IsCustomTrack(trackReference))
            {
                var songPath = Globals.ChartFolders[trackReference];
                var spritePath = Path.Combine(songPath, "bg.png");
                var videoPath = Path.Combine(songPath, "bg.mp4");
                var backgroundPath = Path.Combine(songPath, "bg.trombackground"); // comically large file extension

                // Possible backgrounds in order of priority:
                // AssetBundle (.trombackground)
                // Video (.mp4)
                // Sprite (.png)

                Plugin.LogDebug("Trying to load custom background!!");

                if (File.Exists(backgroundPath))
                {
                    // probably change the above to have a proper confirmation it worked at some point

                    foreach (var videoPlayer in __instance.bgplane.transform.parent.parent.GetComponentsInChildren<VideoPlayer>())
                    {
                        if (videoPlayer.playOnAwake && videoPlayer.gameObject.activeInHierarchy)
                        {
                            videoPlayer.enabled = true;
                            videoPlayer.playOnAwake = false;
                            videoPlayer.Pause();
                            videoPlayer.PlayVideoDelayed(2400);
                        }
                    }
                }
                else
                {
                    __instance.DisableBackground();
                    Plugin.LogDebug("Disabled Backgrounds...");

                    if (File.Exists(videoPath))
                    {
                        Plugin.LogDebug($"Trying to load Custom Video from path: {spritePath}");

                        __instance.SetVideoBackground(videoPath);
                    }
                    else
                    {
                        Plugin.LogDebug($"Trying to load Custom sprite from path: {spritePath}");
                        var sprite = ImageHelper.LoadSpriteFromFile(spritePath);
                        __instance.SetBasicBackground(sprite);
                    }
                }

                __instance.doBGEffect(BGEffect);
            }
        }
    }
}