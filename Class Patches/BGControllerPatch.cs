using System.Collections;
using HarmonyLib;
using MyFirstPlugin.Class_Patches;
using MyFirstPlugin.Helpers;
using UnityEngine;

namespace MyFirstPlugin;

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
        Debug.Log("Trying to load custom background!!");
        //string trackReference = GlobalVariables.data_trackrefs[GlobalVariables.chosen_track_index];
        if (Globals.IsCustomTrack(trackReference))
        {
            __instance.DisableBackground();
            Debug.Log("Disabled Backgrounds...");
            Debug.Log($"Trying to load Custom sprite from path: {spritePath}");
            var sprite = ImageHelper.LoadSpriteFromFile(spritePath);
            __instance.SetBasicBackground(sprite);
        }
    }
}