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

        static void Postfix(BGController __instance)
        {

            var trackReference = GlobalVariables.data_trackrefs[GlobalVariables.chosen_track_index];
            var songPath = Globals.GetCustomSongsPath() + trackReference;
            var spritePath = songPath + "/bg.png";
            Plugin.LogDebug("Trying to load custom background!!");
            //string trackReference = GlobalVariables.data_trackrefs[GlobalVariables.chosen_track_index];
            if (Globals.IsCustomTrack(trackReference))
            {
                __instance.DisableBackground();
                Plugin.LogDebug("Disabled Backgrounds...");
                Plugin.LogDebug($"Trying to load Custom sprite from path: {spritePath}");
                var sprite = ImageHelper.LoadSpriteFromFile(spritePath);
                __instance.SetBasicBackground(sprite);
            }
        }
    }
}