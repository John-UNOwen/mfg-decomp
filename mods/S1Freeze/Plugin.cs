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

        // S3: emulate the cabinet free-play coin option. The guest entry dialog
        // otherwise dies in credit/PASELI/cardmng checks we do not emulate (real
        // cabinet options come from AVS sysfile /coin/free). With free play on,
        // ExecConsume takes the ExecFreePlay path and goes straight into the game.
        [HarmonyPatch(typeof(GameUtility), "IsFreePlay")]
        [HarmonyPostfix]
        public static void ForceFreePlay(ref bool __result)
        {
            __result = true;
        }

        // S3 debug: pin down the ExecTsumo IndexOutOfRange - log the offending
        // command and the client-side table sizes at the moment of the throw.
        [HarmonyPatch(typeof(ExecTsumo), "ProcessCommand")]
        [HarmonyFinalizer]
        public static System.Exception LogTsumoCrash(System.Exception __exception, MFG.Taikyoku.Command.Receive.Tsumo cmd, MahjongPlayFlow mpf)
        {
            Debug.Log($"[S3DIAG] ExecTsumo ex={__exception?.GetType().Name} seq={cmd.SeqNo} pindex={cmd.PlayerIndex} pai={cmd.PaiID} players={mpf.Players.Count} nokori={mpf.m_nokoriHaiNum}");
            return __exception;
        }

        // S2 capture: log exactly what the eAC3 service_list verb returns.
        // This decides whether the provider also fronts the HTTP game API.
        [HarmonyPatch(typeof(TCSNET.TcsXrpcModuleTypes.GetServiceListResponse), "Parse")]
        [HarmonyPostfix]
        public static void LogServiceList(TCSNET.TcsXrpcModuleTypes.GetServiceListResponse __instance)
        {
            Debug.Log($"[S2CAP] service_list service_url={__instance.ServiceUrl} services={__instance.ServiceInfos.Count}");
            foreach (var info in __instance.ServiceInfos)
            {
                var t = info.GetType();
                var name = t.GetProperty("Name", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var url = t.GetProperty("Url", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var mode = t.GetProperty("Mode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Debug.Log($"[S2CAP] service name={name?.GetValue(info)} url={url?.GetValue(info)} mode={mode?.GetValue(info)}");
            }
        }

        // S2 capture: log every time the front base URL is (re)assigned.
        [HarmonyPatch(typeof(SystemConfigGameData), "set_FrontServerUrl")]
        [HarmonyPostfix]
        public static void LogFrontUrl(string value)
        {
            Debug.Log($"[S2CAP] FrontServerUrl set to: {value}");
        }
    }
}
