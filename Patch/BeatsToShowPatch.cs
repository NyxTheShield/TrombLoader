using HarmonyLib;

namespace TrombLoader.Patch;

[HarmonyPatch]
public class BeatsToShowPatch
{
    [HarmonyPatch(typeof(GameController), "Start")]
    [HarmonyPrefix]
    static void PatchBeatsToShow(out int ___beatstoshow)
    {
        ___beatstoshow = Plugin.Instance.beatsToShow?.Value ?? 64;
    }
}