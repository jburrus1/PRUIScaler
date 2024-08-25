using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BepInEx.IL2CPP.Utils.Collections;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace PRUIScaler;


    public class UIScaler : MonoBehaviour
    {
        public const float TEXT_DELAY = 3;

        public static float scaleRatio = 1f;
        public static UIScaler Instance;
        private Dictionary<Scene, Dictionary<Text, Action<Text>>> _textToScale;
        private Dictionary<Scene, Dictionary<Image, Action<Image>>>  _imageToScale;
        public static void Setup()
        {
            scaleRatio = (16f / 9f) * ((float)Screen.height / Screen.width);

            ClassInjector.RegisterTypeInIl2Cpp<UIScaler>();

            GameObject obj = new("MyBehavior");
            DontDestroyOnLoad(obj);

            obj.hideFlags = HideFlags.HideAndDontSave;

            Instance = obj.AddComponent<UIScaler>();
        }
        private void Awake()
        {
            Plugin.Instance.Log.LogInfo("Component Added");

            SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)OnSceneLoaded;
            _textToScale = new Dictionary<Scene, Dictionary<Text, Action<Text>>>();
            _imageToScale = new Dictionary<Scene, Dictionary<Image, Action<Image>>>();
        }

        private void Start()
        {
            foreach (var scene in SceneManager.GetAllScenes().ToList())
            {
                HandleScene(scene);
            }
        }

        private void Update()
        {

            foreach(var scene in _textToScale.Keys.ToList())
            {
                if (!scene.isLoaded)
                {
                    continue;
                }
                foreach (var text in _textToScale[scene].Keys.ToList())
                {
                    try
                    {
                        if (text.text.Length > 0)
                        {
                            _textToScale[scene][text].Invoke(text);
                            _textToScale[scene].Remove(text);
                        }
                    }
                    catch
                    {
                        LogError($"Could not scale text for {scene.name}.{text?.name ?? "null"}");
                    }
                }
                foreach (var image in _imageToScale[scene].Keys.ToList())
                {
                    try
                    {
                        if ((image.activeSprite.name.Length > 0) && image.gameObject.active)
                        {
                            _imageToScale[scene][image].Invoke(image);
                            _imageToScale[scene].Remove(image);
                        }
                    }
                    catch
                    {
                        LogError($"Could not scale image for {scene.name}.{image?.name ?? "null"}");
                    }
                }
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            HandleScene(scene);
        }

        private static void Log(string message)
        {
            // Plugin.Instance.Log.LogInfo(message);
        }private static void LogError(string message)
        {
            Plugin.Instance.Log.LogError(message);
        }

        void HandleScene(Scene scene)
        {
            _textToScale[scene] = new Dictionary<Text, Action<Text>>();
            _imageToScale[scene] = new Dictionary<Image, Action<Image>>();
            Log($"Handling {scene.name} for {scaleRatio}");
            switch (scene.name)
            {
                case "CampMenu":
                    StartCoroutine(InitCampMenu(scene,1).WrapToIl2Cpp());
                    break;
                case "MessageWindow":
                    StartCoroutine(InitMessageWindow(scene, 1).WrapToIl2Cpp());
                    break;
                case "TitleScreen":
                    StartCoroutine(InitTitleScreen(scene, 1).WrapToIl2Cpp());
                    break;
                case "BattleMenu":
                    StartCoroutine(InitBattleMenu(scene, 1).WrapToIl2Cpp());
                    break;
                default:
                    break;
            }
        }


        private IEnumerator InitCampMenu(Scene scene, float textDelay)
        {

            yield return new WaitForSeconds(textDelay);
            // Plugin.Instance.Log.LogInfo("Found scene");
            var rootObjects = scene.GetRootGameObjects().ToList();

            foreach (var child in rootObjects)
            {
                child.transform.localScale = new Vector3(1, scaleRatio, 1);
                ScalerHelper(scene,child.transform);
                Plugin.Instance.Log.LogInfo($"Scaled {child.name}");
            }

            yield return null;
        }

        private IEnumerator InitTitleScreen(Scene scene, float textDelay)
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
                        ScalerHelper(scene,grandchild.transform);
                        Plugin.Instance.Log.LogInfo($"Scaled {grandchild.name}");
                    }
                }
            }

            yield return null;
        }

        private IEnumerator InitMessageWindow(Scene scene, float textDelay)
        {

            yield return new WaitForSeconds(textDelay);
            // Plugin.Instance.Log.LogInfo("Found scene");
            var rootObjects = scene.GetRootGameObjects().ToList();

            foreach (var root in rootObjects)
            {
                for (var i = 0; i < root.transform.childCount; i++)
                {
                    var child = root.transform.GetChild(i);
                    child.transform.localScale = new Vector3(1, scaleRatio, 1);
                    ScalerHelper(scene,child.transform);
                }
            }

            yield return null;
        }

        private IEnumerator InitBattleMenu(Scene scene, float delay)
        {
            yield return new WaitForSeconds(delay);

            var rootGameObjects = scene.GetRootGameObjects();
            foreach (var root in rootGameObjects)
            {
                var relChild = root.transform.Find("Canvas")?.Find("aspect_parent")?.Find("notch_parent")
                    ?.Find("KeyParent")?.Find("menu_parent")?.Find("menu_parent(Clone)");
                if (relChild is null)
                {
                    LogError($"Could not add scale info for {scene.name}.{root.name}");
                    continue;
                }
                for (var i = 0; i < relChild.transform.childCount; i++)
                {
                    var child = relChild.transform.GetChild(i);
                    child.transform.localScale = new Vector3(1, scaleRatio, 1);
                    ScalerHelper(scene,child.transform);
                }

            }
        }

        private void ScalerHelper(Scene scene,Transform child)
        {
            try
            {
                for (var i = 0; i < child.transform.childCount; i++)
                {
                    ScalerHelper(scene, child.transform.GetChild(i));
                }

                var text = child.GetComponent<Text>();
                if (!(text is null))
                {
                    // Log($"Scaling text {text.text}");
                    _textToScale[scene].Add(text, TextScale);
                }

                var image = child.GetComponent<Image>();
                if (!(image is null) && !(image.activeSprite is null))
                {
                    _imageToScale[scene].Add(image, SpriteScale);
                }
            }

            catch
            {
                LogError($"Could not add scale info for {scene.name}.{child.name}");
            }
        }

        private void TextScale(Text text)
        {
            text.transform.localScale = new Vector3(1, 1/scaleRatio, 1);

            if (text.fontSize < 27)
            {
                text.resizeTextMaxSize = (int)(text.fontSize * scaleRatio);
                text.fontSize = (int)(text.fontSize * scaleRatio);
                text.verticalOverflow = VerticalWrapMode.Overflow;
            }
        }


        private void SpriteScale(Image image)
        {
            var spriteTrans = image.transform;
            Log($"Scaling {spriteTrans.name} with sprite {image.activeSprite.name}?");
            if ((!(image.activeSprite.name.Contains("Common") || spriteTrans.name.Contains("Viewport") ||
                   image.activeSprite.name.Contains("Background")) ||
                 spriteTrans.name.Contains("Icon")))
            {
                Log($"Yes");
                spriteTrans.localScale = new Vector3(scaleRatio * spriteTrans.localScale.x, 1, 1);

                for (var i = 0; i < spriteTrans.transform.childCount; i++)
                {
                    var grandchild = spriteTrans.GetChild(i);
                    grandchild.localScale = new Vector3(grandchild.localScale.x / scaleRatio, 1, 1);
                }
            }
            else
            {
                Log($"No");
            }
        }

    }