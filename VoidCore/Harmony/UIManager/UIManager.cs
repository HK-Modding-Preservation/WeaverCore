﻿using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VoidCore.Hooks;

namespace VoidCore.Harmony
{
    [HarmonyPatch(typeof(UIManager))]
    [HarmonyPatch("Start")]
    internal class UIManagerStart
    {
        static void Postfix(UIManager __instance)
        {
            ModLog.Log("UI START");
            foreach (var hook in UIHook.AvailableHooks)
            {
                __instance.gameObject.AddComponent(hook);
            }
        }
    }
}
