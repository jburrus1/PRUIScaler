using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.IL2CPP.Utils.Collections;
using Il2CppSystem.Text;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PRUIScaler
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static Plugin Instance;
        public override void Load()
        {
            Instance = this;
            // Plugin startup logic
            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            UIScaler.Setup();
        }

    }
}
