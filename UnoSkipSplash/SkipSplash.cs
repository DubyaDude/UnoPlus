using HarmonyLib;
using MelonLoader;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UNO;

namespace UnoSkipSplash
{
    public class SkipSplash : MelonMod
    {
        private static HarmonyMethod GetPatch(string name)
        {
            return new HarmonyMethod(typeof(SkipSplash).GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic));
        }

        private static HarmonyLib.Harmony harmonyInstanceStatic;
        private static MelonLogger.Instance loggerInstanceStatic;

        public override void OnApplicationStart()
        {
            loggerInstanceStatic = LoggerInstance;
            harmonyInstanceStatic = HarmonyInstance;
            harmonyInstanceStatic.Patch(typeof(SplashScreen).GetMethod("begin", BindingFlags.Instance | BindingFlags.NonPublic), GetPatch(nameof(BeginPrefix)));

            harmonyInstanceStatic.Patch(typeof(TitleScreenManager).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic), GetPatch(nameof(TitlePrefix)), GetPatch(nameof(TitlePostfix)));
            harmonyInstanceStatic.Patch(typeof(Input).GetProperty("anyKey").GetGetMethod(), postfix: GetPatch(nameof(AnyKeyPostfix)));
        }


        private static bool isTitleUpdate = false;
        private static bool TitlePrefix()
        {
            loggerInstanceStatic.Msg("TitlePrefix");
            isTitleUpdate = true;
            return true;
        }

        private static void AnyKeyPostfix(ref bool __result)
        {
            loggerInstanceStatic.Msg("AnyKeyPostfix");
            if(isTitleUpdate)
                __result = true;
        }

        private static void TitlePostfix()
        {
            loggerInstanceStatic.Msg("TitlePostfix");
            isTitleUpdate = false;
        }



        private static bool BeginPrefix()
        {
            MelonCoroutines.Start(StartScene());
            return false;
        }

        public static IEnumerator StartScene()
        {
            yield return new WaitForSeconds(0.05f);
            SceneManager.LoadScene("TitleScreen", LoadSceneMode.Single);
            yield break;
        }
    }
}
