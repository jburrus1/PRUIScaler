using System;
using BepInEx;
using BepInEx.IL2CPP;
using Il2CppSystem.Threading;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PRUIil2cpp
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

            Thread.Sleep(1_000);
            MyBehavior.Setup();
        }
        
    }

    public class MyBehavior : MonoBehaviour
    {
        public static MyBehavior Instance;
        public static void Setup()
        {
            ClassInjector.RegisterTypeInIl2Cpp<MyBehavior>();

            GameObject obj = new("MyBehavior");
            DontDestroyOnLoad(obj);

            Instance = obj.AddComponent<MyBehavior>();
        }
        private void Awake()
        {
            Plugin.Instance.Log.LogInfo("Component Added");
        }

        private float interval = 1;
        private float counter = 0f;
        public void Update()
        {
            Plugin.Instance.Log.LogInfo($"Update");
            counter += Time.deltaTime;
            if (counter > interval)
            {
                Plugin.Instance.Log.LogInfo($"Looking");
                counter = 0f;
                try
                {
                    var found = GameObject.Find("menu_base");
                    if (found is null)
                    {
                    
                        Plugin.Instance.Log.LogInfo($"Not Found :(");
                    }
                    else
                    {
                        Plugin.Instance.Log.LogInfo($"Found!!!!");
                    }
                }
                catch (Exception e)
                {
                    Plugin.Instance.Log.LogInfo(e);
                }
            }
        }
    }
}
