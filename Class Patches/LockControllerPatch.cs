using HarmonyLib;
using TrombLoader.Helpers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrombLoader.Class_Patches
{
    [HarmonyPatch(typeof(LockController))]
    [HarmonyPatch("loadCharSelect")]
    public class LockControllerPatch
    {
        /*static bool Prefix(LockController __instance)
        {
            GlobalVariables.scene_destination = "einefinal";
            for (int i = 0; i < GlobalVariables.data_trackrefs.Length; i++)
            {
                if (GlobalVariables.data_trackrefs[i] == "einefinal") GlobalVariables.chosen_track_index = i;
            }

            SceneManager.LoadScene("charselect");
            return false;
        }*/
    }
}