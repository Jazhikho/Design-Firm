#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace SewItGoes.Editor
{
    /// <summary>
    /// Headless CI / local builds for itch.io: WebGL and Windows 64-bit player from EditorBuildSettings scenes.
    /// Invoke with Unity batchmode: <c>-executeMethod SewItGoes.Editor.ItchBuildCommand.BuildWebGLAndWindowsForItch</c>.
    /// </summary>
    public static class ItchBuildCommand
    {
        private const string WebGlOutputFolderName = "WebGL";
        private const string WindowsOutputFolderName = "Windows64";

        /// <summary>
        /// Editor menu entry: builds WebGL and Windows 64 to <c>Builds/</c> without quitting the editor.
        /// </summary>
        [MenuItem("Sew It Goes/Build for itch.io (WebGL + Windows64)")]
        public static void BuildForItchMenu()
        {
            ExecuteBuilds();
        }

        /// <summary>
        /// Batch-mode entry point: builds both targets then exits the editor with status 0 or 1.
        /// </summary>
        public static void BuildWebGLAndWindowsForItch()
        {
            if (!ExecuteBuilds())
            {
                if (UnityEngine.Application.isBatchMode)
                {
                    EditorApplication.Exit(1);
                }

                return;
            }

            if (UnityEngine.Application.isBatchMode)
            {
                EditorApplication.Exit(0);
            }
        }

        /// <summary>
        /// Collects enabled scene paths from <see cref="EditorBuildSettings"/> for player builds.
        /// </summary>
        /// <returns>Scene paths in build order.</returns>
        private static string[] GetEnabledScenes()
        {
            List<string> paths = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    paths.Add(scene.path);
                }
            }

            if (paths.Count == 0)
            {
                throw new InvalidOperationException("ItchBuildCommand: No enabled scenes in EditorBuildSettings.");
            }

            return paths.ToArray();
        }

        /// <summary>
        /// Builds WebGL to <c>Builds/WebGL</c> and Windows 64-bit to <c>Builds/Windows64</c> (product .exe inside that folder).
        /// </summary>
        /// <returns>True when both player builds succeeded.</returns>
        private static bool ExecuteBuilds()
        {
            try
            {
                string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
                string buildsRoot = Path.Combine(projectRoot, "Builds");
                string webOut = Path.Combine(buildsRoot, WebGlOutputFolderName);
                string winFolder = Path.Combine(buildsRoot, WindowsOutputFolderName);
                Directory.CreateDirectory(webOut);
                Directory.CreateDirectory(winFolder);

                string[] scenes = GetEnabledScenes();

                BuildPlayerOptions webOpts = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = webOut,
                    target = BuildTarget.WebGL,
                    options = BuildOptions.None
                };
                BuildReport webReport = BuildPipeline.BuildPlayer(webOpts);
                if (webReport.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException("ItchBuildCommand: WebGL build failed with result " + webReport.summary.result + ".");
                }

                string exeName = PlayerSettings.productName + ".exe";
                string winExePath = Path.Combine(winFolder, exeName);
                BuildPlayerOptions winOpts = new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = winExePath,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None
                };
                BuildReport winReport = BuildPipeline.BuildPlayer(winOpts);
                if (winReport.summary.result != BuildResult.Succeeded)
                {
                    throw new InvalidOperationException("ItchBuildCommand: Windows build failed with result " + winReport.summary.result + ".");
                }

                UnityEngine.Debug.Log("ItchBuildCommand: WebGL output: " + webOut);
                UnityEngine.Debug.Log("ItchBuildCommand: Windows output folder: " + winFolder);
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("ItchBuildCommand: " + ex.Message + "\n" + ex.StackTrace);
                return false;
            }
        }
    }
}
#endif
