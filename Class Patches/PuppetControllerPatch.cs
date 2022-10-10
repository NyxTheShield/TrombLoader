using System.IO;
using HarmonyLib;
using TrombLoader.Helpers;
using UnityEngine;

namespace TrombLoader.Class_Patches
{
    [HarmonyPatch(typeof(GameController))]
    [HarmonyPatch("startSong")]
    public class GameControllerStartSongPatch
    {
        static void Postfix(GameController __instance)
        {
            foreach(var tromboner in Globals.Tromboners)
            {
                if(tromboner != null) LeanTween.scaleY(tromboner.gameObject, 1f, 0.5f).setEaseOutBounce().setDelay(2f);
            }
        }
    }

    [HarmonyPatch(typeof(GameController))]
    [HarmonyPatch("startSong")]
    public class GameControllerStartDancePatch
    {
        static void Postfix(GameController __instance)
        {
            var tempTempo = __instance.tempo;
            if (tempTempo > 90f) tempTempo *= 0.5f;
            if (__instance.beatspermeasure == 1) tempTempo *= 0.6666667f;

            foreach (var tromboner in Globals.Tromboners)
            {
                if (tromboner != null) tromboner.controller.startPuppetBob(tempTempo);
            }
        }
    }

    [HarmonyPatch(typeof(HumanPuppetController))]
    [HarmonyPatch("doPuppetControl")]
    public class HumanPuppetControllerPuppetControlPatch
    {
        static void Postfix(HumanPuppetController __instance, float vp)
        {
            if (__instance.transform.parent.name == "PlayerModelHolder")
            {
                foreach (var tromboner in Globals.Tromboners)
                {
                    if (tromboner != null)
                    {
                        tromboner.controller.doPuppetControl(vp);
                        tromboner.controller.vibrato = __instance.vibrato;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameController))]
    [HarmonyPatch("setPuppetBreath")]
    public class GameControllerPuppetBreathPatch
    {
        static void Postfix(GameController __instance, bool hasbreath)
        {
            foreach (var tromboner in Globals.Tromboners)
            {
                if (tromboner != null)
                {
                    tromboner.controller.outofbreath = hasbreath;
                    tromboner.controller.applyFaceTex();
                }
            }
        }
    }

    [HarmonyPatch(typeof(GameController))]
    [HarmonyPatch("setPuppetShake")]
    public class GameControllerPuppetShakePatch
    {
        static void Postfix(GameController __instance, bool shake)
        {
            foreach (var tromboner in Globals.Tromboners)
            {
                if (tromboner != null)
                {
                    tromboner.controller.shaking = shake;
                    tromboner.controller.applyFaceTex();
                }
            }
        }
    }
}