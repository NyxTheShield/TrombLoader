using HarmonyLib;
using TrombLoader.Helpers;
using UnityEngine;

namespace TrombLoader.Class_Patches
{
    [HarmonyPatch(typeof(LoadController))]
    [HarmonyPatch("LoadGameplayAsync")]
    public class LoadControllerPatch
    {
        //This hook was used to originally load the ogg file in a async fashion, but for some reason the clip was getting GC'd on scene transition
        //Might be useful for the future
        static bool Prefix(LoadController __instance)
        {
            Plugin.LogDebug("Loading Gameplay Scene!");


            var customTrackReference = GlobalVariables.chosen_track;


            if (Globals.IsCustomTrack(customTrackReference))
            {
                Plugin.LogDebug("Loading a custom chart");
                Plugin.Instance.LoadGameplayScene();
            }
            else
            {
                Plugin.LogDebug("Loading a base game chart");
                Plugin.Instance.LoadGameplayScene();
            }

            return false;
        }
    }
}