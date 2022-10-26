using TrombLoader.Helpers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TrombLoader.Class_Patches
{
    // size of 100 is hard coded in this method
    [HarmonyPatch(typeof(SaveSlotController), nameof(SaveSlotController.checkScores))]
    public class SaveSlotControllerPatch
    {
        static bool Prefix()
        {
            try
            {
                Plugin.LogDebug("checking scores (trombloder)...");

                var oldTrackRefs = new HashSet<string>(GlobalVariables.localsave.data_trackscores.Select(i => i[0]));
                var newTrackRefs = new HashSet<string>(GlobalVariables.data_trackrefs);
                var extraTrackRefs = newTrackRefs.Except(oldTrackRefs).ToList();

                Plugin.LogDebug(extraTrackRefs.Count + " tracks to add");
                if (extraTrackRefs.Count > 0)
                {
                    var newScores = extraTrackRefs.Select(trackRef => new string[] { trackRef, "-", "0", "0", "0", "0", "0" }).ToList();
                    var combinedScores = new string[GlobalVariables.localsave.data_trackscores.Length + newScores.Count][];
                    GlobalVariables.localsave.data_trackscores.CopyTo(combinedScores, 0);
                    newScores.CopyTo(combinedScores, GlobalVariables.localsave.data_trackscores.Length);

                    GlobalVariables.localsave.data_trackscores = combinedScores;
                    SaverLoader.updateSavedGame();
                }
                return false;
            }
            catch (Exception ex)
            {
                Plugin.LogError(ex.ToString());
                Plugin.LogError("Disabling save creation/deletion out of an abundance of caution...");
                Globals.SaveCreationEnabled = false;

                return false;
            }
        }
    }

    [HarmonyPatch(typeof(SaveSlotController), nameof(SaveSlotController.clickNewSave))]
    public class SaveSlotControllerNewSavePatch
    {
        static bool Prefix()
        {
            return Globals.SaveCreationEnabled;
        }
    }

    [HarmonyPatch(typeof(SaveSlotController), nameof(SaveSlotController.clickDeleteSave))]
    public class SaveSlotControllerDeleteSavePatch
    {
        static bool Prefix()
        {
            return Globals.SaveCreationEnabled;
        }
    }
}
