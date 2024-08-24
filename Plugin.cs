using System;
using System.Collections;
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
            MyBehavior.Setup();
        }

    }

    public class MyBehavior : MonoBehaviour
    {
        public const float TEXT_DELAY = 3;

        public static float scaleRatio = 1f;
        public static MyBehavior Instance;
        public static void Setup()
        {
            scaleRatio = (16f / 9f) * ((float)Screen.height / Screen.width);

            ClassInjector.RegisterTypeInIl2Cpp<MyBehavior>();

            GameObject obj = new("MyBehavior");
            DontDestroyOnLoad(obj);

            obj.hideFlags = HideFlags.HideAndDontSave;

            Instance = obj.AddComponent<MyBehavior>();
        }
        private void Awake()
        {
            Plugin.Instance.Log.LogInfo("Component Added");

            SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
        }

        private void Start()
        {
            foreach (var scene in SceneManager.GetAllScenes().ToList())
            {
                HandleScene(scene);
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HandleScene(scene);
        }

        private static void Log(string message)
        {
            Plugin.Instance.Log.LogInfo(message);
        }

        void HandleScene(Scene scene)
        {
            Log($"Handling {scene.name} for {scaleRatio}");
            switch (scene.name)
            {
                case "CampMenu":
                    StartCoroutine(InitCampMenu(scene,1).WrapToIl2Cpp());
                    break;
                case "TitleScreen":
                    StartCoroutine(InitTitleScreen(scene, 1).WrapToIl2Cpp());
                    break;
                default:
                    break;
            }
        }


        public IEnumerator InitCampMenu(Scene scene, float textDelay)
        {

            yield return new WaitForSeconds(textDelay);
            // Plugin.Instance.Log.LogInfo("Found scene");
            var rootObjects = scene.GetRootGameObjects().ToList();

            foreach (var child in rootObjects)
            {
                child.transform.localScale = new Vector3(1, scaleRatio, 1);
                ScalerHelper(child.transform);
                Plugin.Instance.Log.LogInfo($"Scaled {child.name}");
            }

            yield return null;
        }

        public IEnumerator InitTitleScreen(Scene scene, float textDelay)
        {

            yield return new WaitForSeconds(textDelay);
            // Plugin.Instance.Log.LogInfo("Found scene");
            var rootObjects = scene.GetRootGameObjects().ToList();

            foreach (var root in rootObjects)
            {
                for (var i = 0; i < root.transform.childCount; i++)
                {
                    var child = root.transform.GetChild(i);
                    if (child.name.Equals("sab_canvas"))
                    {
                        var grandchild = child.GetChild(0);
                        grandchild.transform.localScale = new Vector3(1, scaleRatio, 1);
                        ScalerHelper(grandchild.transform);
                        Plugin.Instance.Log.LogInfo($"Scaled {grandchild.name}");
                    }
                }
            }

            yield return null;
        }

        public void ScalerHelper(Transform child)
        {
            for (var i = 0; i < child.transform.childCount; i++)
            {
                ScalerHelper(child.transform.GetChild(i));
            }

            var text = child.GetComponent<Text>();
            if (!(text is null))
            {
                child.localScale = new Vector3(1, 1/scaleRatio, 1);

                if (text.fontSize < 27)
                {
                    text.resizeTextMaxSize = (int)(text.fontSize * scaleRatio);
                    text.fontSize = (int)(text.fontSize * scaleRatio);
                    text.verticalOverflow = VerticalWrapMode.Overflow;
                }
            }

            // var image = child.GetComponent<Image>();
            // if (!(image is null) && !(image.activeSprite is null))
            // {
            //     if(!(image.activeSprite.name.Contains("UI_Default")) || child.name.Contains("Icon"))
            //     {
            //         child.localScale = new Vector3(scaleRatio, 1, 1);
            //     }
            // }
        }

    }
}
