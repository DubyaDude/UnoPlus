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

        public override void OnApplicationStart()
        {
            harmonyInstanceStatic = HarmonyInstance;
            harmonyInstanceStatic.Patch(typeof(SplashScreen).GetMethod("begin", BindingFlags.Instance | BindingFlags.NonPublic), GetPatch(nameof(BeginPrefix)));
            harmonyInstanceStatic.Patch(typeof(TitleScreenManager).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic), GetPatch(nameof(TitlePrefix)), GetPatch(nameof(TitlePostfix)));
        }

        private static bool TitlePrefix()
        {
            harmonyInstanceStatic.Patch(typeof(Input).GetProperty("anyKey").GetGetMethod(), postfix: GetPatch(nameof(AnyKeyPostfix)));
            return true;
        }

        private static void AnyKeyPostfix(ref bool __result)
        {
            __result = true;
        }

        private static void TitlePostfix()
        {
            harmonyInstanceStatic.Unpatch(typeof(TitleScreenManager).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic), HarmonyPatchType.Prefix);
            harmonyInstanceStatic.Unpatch(typeof(TitleScreenManager).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic), HarmonyPatchType.Postfix);
            harmonyInstanceStatic.Unpatch(typeof(Input).GetProperty("anyKey").GetGetMethod(), HarmonyPatchType.Postfix);
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
