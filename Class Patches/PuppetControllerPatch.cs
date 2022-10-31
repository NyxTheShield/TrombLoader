using System.IO;
using HarmonyLib;
using TrombLoader.Data;
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

    [HarmonyPatch(typeof(HumanPuppetController))]
    [HarmonyPatch("startPuppetBob")]
    public class HumanPuppetControllerPuppetBobPatch
    {
        static bool Prefix(HumanPuppetController __instance)
        {
            return !__instance.just_testing;
        }
    }

    [HarmonyPatch(typeof(HumanPuppetController))]
    [HarmonyPatch("testMovement")]
    public class HumanPuppetControllerTestMovementPatch
    {
        static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(HumanPuppetController))]
    [HarmonyPatch("Start")]
    public class HumanPuppetControllerStartPatch
    {
        static void Prefix(HumanPuppetController __instance)
        {
            __instance.just_testing = true;
        }

        static void Postfix(HumanPuppetController __instance)
        {
            __instance.just_testing = false;

            Tromboner customTromboner = null;
            foreach (var tromboner in Globals.Tromboners)
            {
                if (tromboner != null && tromboner.controller.Equals(__instance))
                {
                    customTromboner = tromboner;
                }
            }

            int movementType = 0;
            if(customTromboner == null)
            {
                movementType = GlobalVariables.chosen_vibe;
            }
            else
            {
                if(customTromboner.placeholder.MovementType == TrombonerMovementType.DoNotOverride)
                {
                    movementType = GlobalVariables.chosen_vibe;
                }
                else
                {
                    movementType = (int)customTromboner.placeholder.MovementType;
                }
            }

            LeanTween.value(movementType == 0 ? 10f : -38f, -48f, 7f).setLoopPingPong().setEaseInOutQuart().setOnUpdate(delegate (float val)
            {
                __instance.p_parent.transform.localEulerAngles = new Vector3(0f, val, 0f);
            });

            __instance.estudious = movementType == 1;
        }
    }
}