using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace MfgS1
{
    [BepInPlugin("decomp.mfg.s1freeze", "S1 Freeze+Log", "0.1.0")]
    public class S1FreezePlugin : BaseUnityPlugin
    {
        public void Awake()
        {
            // ResidentFlow.InitializeOnBoot kills Unity logging at :219.
            // Awake may run before or after it; KeepLogOn below covers the after case.
            Debug.unityLogger.logEnabled = true;
            var harmony = new Harmony("decomp.mfg.s1freeze");
            harmony.PatchAll(typeof(S1FreezePatches));
            Logger.LogInfo("[S1Freeze] harmony patches applied");
        }
    }

    [HarmonyPatch]
    public static class S1FreezePatches
    {
        // Freeze the global and per-window countdowns: CountDownTimer.Update
        // is what decrements m_time and raises END (which logs the player out
        // mid-login after ~20s in the S1 mock loop).
        [HarmonyPatch(typeof(CountDownTimer), "Update")]
        [HarmonyPrefix]
        public static bool FreezeTimer()
        {
            return false;
        }

        // DebugFlag.IsActive is stubbed to `return false` in the shipping build.
        // Re-enable only the request/response logging flag so every API response
        // body lands in the Unity log next to the error strings.
        [HarmonyPatch(typeof(DebugFlag), "IsActive")]
        [HarmonyPrefix]
        public static bool LogResponses(DebugFlag.Flag key, ref bool __result)
        {
            if (key == DebugFlag.Flag.EnableRequestAPIResponseLog)
            {
                __result = true;
                return true;
            }
            return false;
        }

        // Re-arm logging after InitializeOnBoot's own `logEnabled = false`,
        // and after the args dump it swallows.
        [HarmonyPatch(typeof(ResidentFlow), "InitializeOnBoot")]
        [HarmonyPostfix]
        public static void KeepLogOn()
        {
            Debug.unityLogger.logEnabled = true;
        }
    }
}
