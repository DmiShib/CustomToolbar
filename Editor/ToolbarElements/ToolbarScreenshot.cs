namespace CustomToolbar.Editor.ToolbarElements
{
    using System;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;
    using Object = UnityEngine.Object;


    internal sealed class ToolbarScreenshot : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/Screenshot";
        private const string SCREENSHOT_FOLDER_PATH = "Screenshots";

        public static ToolbarScreenshot Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Screenshot";
        protected override string Tooltip => "Screenshot options";


        [MainToolbarElement(ID, defaultDockPosition = MainToolbarDockPosition.Right)]
        public static MainToolbarElement Register()
        {
            return Instance.GetOrCreateElement();
        }

        protected override MainToolbarElement CreateElement()
        {
            var icon = EditorGUIUtility.IconContent("d_FrameCapture").image as Texture2D;
            var content = new MainToolbarContent(icon, Tooltip);

            return new MainToolbarDropdown(content, ShowScreenshotMenu);
        }

        private void ShowScreenshotMenu(Rect rect)
        {
            var menu = new GenericMenu();

            // --- Game View Options ---
            menu.AddItem(new GUIContent("Capture Game View/Current Resolution"), false, () => CaptureGameView(1));
            menu.AddSeparator("Capture Game View/");
            menu.AddItem(new GUIContent("Capture Game View/HD (1920x1080)"), false, () => CaptureGameViewAtResolution(1920, 1080));
            menu.AddItem(new GUIContent("Capture Game View/4K (3840x2160)"), false, () => CaptureGameViewAtResolution(3840, 2160));

            // --- Scene View Options ---
            if (SceneView.lastActiveSceneView != null)
            {
                menu.AddItem(new GUIContent("Capture Scene View/Opaque Background"), false, () => CaptureSceneView(false));
                menu.AddItem(new GUIContent("Capture Scene View/Transparent Background"), false, () => CaptureSceneView(true));
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Capture Scene View/Opaque (No Scene View active)"));
                menu.AddDisabledItem(new GUIContent("Capture Scene View/Transparent (No Scene View active)"));
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Open Screenshots Folder"), false, OpenScreenshotsFolder);

            menu.DropDown(rect);
        }

        private static void OpenScreenshotsFolder()
        {
            EnsureFolderExists();
            Application.OpenURL(Path.GetFullPath(SCREENSHOT_FOLDER_PATH));
        }

        private static void CaptureGameView(int resolutionMultiplier)
        {
            EnsureFolderExists();
            var fullPath = GetUniqueScreenshotPath("GameView");
            ScreenCapture.CaptureScreenshot(fullPath, resolutionMultiplier);
            EditorApplication.delayCall += () => LogScreenshot(fullPath);
        }

        private static void CaptureGameViewAtResolution(int width, int height)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("[CustomToolbar] Cannot capture Game View: no main camera.");
                return;
            }

            EnsureFolderExists();
            var fullPath = GetUniqueScreenshotPath($"GameView_{width}x{height}");

            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var screenShot = new Texture2D(width, height, TextureFormat.ARGB32, false);

            var prevTargetTexture = mainCamera.targetTexture;
            mainCamera.targetTexture = rt;
            mainCamera.Render();
            mainCamera.targetTexture = prevTargetTexture;

            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();
            RenderTexture.active = null;

            File.WriteAllBytes(fullPath, screenShot.EncodeToPNG());

            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(screenShot);
            LogScreenshot(fullPath);
        }

        private static void CaptureSceneView(bool withTransparency)
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            var prevLighting = sceneView.sceneLighting;
            var tempCamGo = new GameObject("ScreenshotCamera") { hideFlags = HideFlags.HideAndDontSave };
            var tempCam = tempCamGo.AddComponent<Camera>();
            tempCam.CopyFrom(sceneView.camera);

            var rt = new RenderTexture(sceneView.camera.pixelWidth, sceneView.camera.pixelHeight, 24, RenderTextureFormat.ARGB32);
            var tex2D = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);

            tempCam.targetTexture = rt;
            if (withTransparency)
            {
                tempCam.clearFlags = CameraClearFlags.SolidColor;
                tempCam.backgroundColor = new Color(0, 0, 0, 0);
                sceneView.sceneLighting = false;
            }
            else
            {
                tempCam.clearFlags = sceneView.camera.clearFlags;
                tempCam.backgroundColor = sceneView.camera.backgroundColor;
            }

            tempCam.Render();
            RenderTexture.active = rt;
            tex2D.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex2D.Apply();
            RenderTexture.active = null;

            sceneView.sceneLighting = prevLighting;
            Object.DestroyImmediate(tempCamGo);

            EnsureFolderExists();
            var fileName = withTransparency ? "SceneView_Transparent" : "SceneView_Opaque";
            var fullPath = GetUniqueScreenshotPath(fileName);

            File.WriteAllBytes(fullPath, tex2D.EncodeToPNG());

            Object.DestroyImmediate(tex2D);
            Object.DestroyImmediate(rt);
            LogScreenshot(fullPath);
        }

        private static void EnsureFolderExists()
        {
            if (!Directory.Exists(SCREENSHOT_FOLDER_PATH))
                Directory.CreateDirectory(SCREENSHOT_FOLDER_PATH);
        }

        private static string GetUniqueScreenshotPath(string prefix)
        {
            return Path.Combine(SCREENSHOT_FOLDER_PATH, $"{prefix}_{DateTimeOffset.Now:yyyy-MM-dd_HH-mm-ss}.png");
        }

        private static void LogScreenshot(string path)
        {
            AssetDatabase.Refresh();
            Debug.Log($"Screenshot saved: <a href=\"{path}\">{path}</a>", AssetDatabase.LoadAssetAtPath<Object>(path));
        }
    }
}