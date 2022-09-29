using System.IO;
using HarmonyLib;
using TrombLoader.Helpers;
using UnityEngine;

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
            var trackReference = GlobalVariables.data_trackrefs[GlobalVariables.chosen_track_index];

            var songPath = Path.Combine(Globals.GetCustomSongsPath(), trackReference);
            var spritePath = Path.Combine(songPath, "bg.png");
            var backgroundPath = Path.Combine(songPath, "bg.trombackground"); // comically large file extension

            Debug.Log("Trying to load custom background!!");
            //string trackReference = GlobalVariables.data_trackrefs[GlobalVariables.chosen_track_index];
            if (Globals.IsCustomTrack(trackReference))
            {
                if (File.Exists(backgroundPath))
                {
                    // do nothing
                    // probably change the above to have a proper confirmation it worked at some point
                    __instance.doBGEffect(BGEffect);
                }
                else
                {
                    __instance.DisableBackground();
                    Debug.Log("Disabled Backgrounds...");
                    Debug.Log($"AssetBundle loading failed. Trying to load Custom sprite from path: {spritePath}");
                    var sprite = ImageHelper.LoadSpriteFromFile(spritePath);
                    __instance.SetBasicBackground(sprite);

                    __instance.doBGEffect(BGEffect);
                }
            }
        }
    }
}