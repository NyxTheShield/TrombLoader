using System.IO;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MyFirstPlugin;

[HarmonyPatch(typeof(LoadController))]
[HarmonyPatch("LoadGameplayAsync")]
public class LoadControllerPatch
{
    //This hook was used to originally load the ogg file in a async fashion, but for some reason the clip was getting GC'd on scene transition
    //Might be useful for the future
    static bool Prefix(LoadController __instance)
    {
        Debug.Log("Loading Gameplay Scene!");
        
        
        var customTrackReference = GlobalVariables.data_trackrefs[GlobalVariables.chosen_track_index];


        if (Globals.IsCustomTrack(customTrackReference))
        {
            Debug.Log("Loading a custom chart" );
            Plugin.instance.LoadGameplayScene();
        }
        else
        {
            Debug.Log("Loading a base game chart" );
            Plugin.instance.LoadGameplayScene();
        }


        return false;
    }
}