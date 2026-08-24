namespace CustomToolbar.Editor.ToolbarElements
{
    using System;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Toolbars;
    using Object = UnityEngine.Object;


    internal sealed class ToolbarScreenshot : BaseToolbarElement
    {
        public const string ID = "CustomToolbar/Screenshot";
        private const string SCREENSHOT_FOLDER_PATH = "Screenshots";
        private const string TRANSPARENT_PREF_KEY = "CustomToolbar.Screenshot.Transparent";

        public static ToolbarScreenshot Instance { get; } = new();
        public override string ElementId => ID;
        protected override string Name => "Screenshot";
        protected override string Tooltip => "Screenshot options";

        /// <summary>Save screenshots with alpha instead of an opaque background.</summary>
        private static bool Transparent
        {
            get => EditorPrefs.GetBool(TRANSPARENT_PREF_KEY, false);
            set => EditorPrefs.SetBool(TRANSPARENT_PREF_KEY, value);
        }


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

            // --- Common Options ---
            menu.AddItem(new GUIContent("Transparent Background"), Transparent, () => Transparent = !Transparent);
            menu.AddSeparator("");

            // --- Game View Options ---
            menu.AddItem(new GUIContent("Capture Game View/Current Resolution"), false, () => CaptureGameView(1));
            menu.AddSeparator("Capture Game View/");
            menu.AddItem(new GUIContent("Capture Game View/HD (1920x1080)"), false, () => CaptureGameViewAtResolution(1920, 1080));
            menu.AddItem(new GUIContent("Capture Game View/4K (3840x2160)"), false, () => CaptureGameViewAtResolution(3840, 2160));

            // --- Scene View Options ---
            if (SceneView.lastActiveSceneView != null)
                menu.AddItem(new GUIContent("Capture Scene View"), false, CaptureSceneView);
            else
                menu.AddDisabledItem(new GUIContent("Capture Scene View (No Scene View active)"));

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
            // ScreenCapture grabs the composited game view, it can never give us alpha.
            if (Transparent)
            {
                var reference = GetGameCameras().FirstOrDefault();
                if (reference == null)
                {
                    Debug.LogWarning("[CustomToolbar] Cannot capture Game View: no active camera.");
                    return;
                }

                CaptureGameViewAtResolution(reference.pixelWidth * resolutionMultiplier, reference.pixelHeight * resolutionMultiplier);
                return;
            }

            EnsureFolderExists();
            var fullPath = GetUniqueScreenshotPath("GameView");
            ScreenCapture.CaptureScreenshot(fullPath, resolutionMultiplier);
            EditorApplication.delayCall += () => LogScreenshot(fullPath);
        }

        private static void CaptureGameViewAtResolution(int width, int height)
        {
            var cameras = GetGameCameras();
            if (cameras.Length == 0)
            {
                Debug.LogWarning("[CustomToolbar] Cannot capture Game View: no active camera.");
                return;
            }

            var suffix = Transparent ? "_Transparent" : string.Empty;
            var fullPath = GetUniqueScreenshotPath($"GameView_{width}x{height}{suffix}");
            RenderToFile(cameras, width, height, Transparent, fullPath);
        }

        private static void CaptureSceneView()
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null) return;

            var prevLighting = sceneView.sceneLighting;
            var tempCamGo = new GameObject("ScreenshotCamera") { hideFlags = HideFlags.HideAndDontSave };
            var tempCam = tempCamGo.AddComponent<Camera>();
            tempCam.CopyFrom(sceneView.camera);

            // Scene lighting adds an ambient wash that shows up in the alpha edges.
            if (Transparent) sceneView.sceneLighting = false;

            var suffix = Transparent ? "_Transparent" : "_Opaque";
            var fullPath = GetUniqueScreenshotPath($"SceneView{suffix}");
            RenderToFile(new[] { tempCam }, sceneView.camera.pixelWidth, sceneView.camera.pixelHeight, Transparent, fullPath);

            sceneView.sceneLighting = prevLighting;
            Object.DestroyImmediate(tempCamGo);
        }

        /// <summary>Active cameras that draw into the game view, in render order.</summary>
        private static Camera[] GetGameCameras()
        {
            return Camera.allCameras
                .Where(cam => cam.targetTexture == null)
                .OrderBy(cam => cam.depth)
                .ToArray();
        }

        /// <summary>Renders the cameras one by one into a shared target and writes it as PNG.</summary>
        private static void RenderToFile(Camera[] cameras, int width, int height, bool transparent, string fullPath)
        {
            var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32);
            var tex2D = new Texture2D(width, height, TextureFormat.ARGB32, false);
            var prevActive = RenderTexture.active;

            try
            {
                // Cameras set to Depth only / Don't Clear never touch the target,
                // so the background has to be prepared before the first render.
                RenderTexture.active = rt;
                GL.Clear(true, true, transparent ? Color.clear : Color.black);
                RenderTexture.active = prevActive;

                for (var i = 0; i < cameras.Length; i++)
                    RenderCamera(cameras[i], rt, transparent, isFirst: i == 0);

                RenderTexture.active = rt;
                tex2D.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                tex2D.Apply();
            }
            finally
            {
                RenderTexture.active = prevActive;
            }

            EnsureFolderExists();
            File.WriteAllBytes(fullPath, tex2D.EncodeToPNG());

            Object.DestroyImmediate(tex2D);
            Object.DestroyImmediate(rt);
            LogScreenshot(fullPath);
        }

        private static void RenderCamera(Camera cam, RenderTexture target, bool transparent, bool isFirst)
        {
            var prevTarget = cam.targetTexture;
            var prevClearFlags = cam.clearFlags;
            var prevBackground = cam.backgroundColor;
            var prevHdr = cam.allowHDR;

            cam.targetTexture = target;

            if (transparent)
            {
                // The HDR resolve blit writes alpha = 1, so an HDR camera never produces a transparent frame.
                cam.allowHDR = false;

                if (isFirst)
                {
                    cam.clearFlags = CameraClearFlags.SolidColor;
                    cam.backgroundColor = new Color(0f, 0f, 0f, 0f);
                }
                else if (cam.clearFlags is CameraClearFlags.Skybox or CameraClearFlags.SolidColor)
                {
                    // Stacked cameras must not overwrite what the previous ones already drew.
                    cam.clearFlags = CameraClearFlags.Depth;
                }
            }
            else
            {
                cam.backgroundColor = new Color(prevBackground.r, prevBackground.g, prevBackground.b, 1f);
            }

            cam.Render();

            cam.targetTexture = prevTarget;
            cam.clearFlags = prevClearFlags;
            cam.backgroundColor = prevBackground;
            cam.allowHDR = prevHdr;
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
