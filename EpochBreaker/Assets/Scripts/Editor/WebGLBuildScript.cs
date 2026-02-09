#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EpochBreaker.Editor
{
    public static class WebGLBuildScript
    {
        /// <summary>
        /// Called by GameCI unity-builder via -executeMethod.
        /// Configures WebGL-specific settings and builds the project.
        /// </summary>
        public static void Build()
        {
            string[] scenes = { "Assets/Scenes/Bootstrap.unity" };

            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.template = "PROJECT:EpochBreaker";

            PlayerSettings.WebGL.initialMemorySize = 512;
            PlayerSettings.WebGL.maximumMemorySize = 1024;
            PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;

            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;

            // Set default resolution to 1920x1080 for sharper rendering
            PlayerSettings.defaultWebScreenWidth = 1920;
            PlayerSettings.defaultWebScreenHeight = 1080;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "build/WebGL/WebGL",
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"WebGL build succeeded: {summary.totalSize / (1024 * 1024)} MB, {summary.totalTime}");
            }
            else
            {
                Debug.LogError($"WebGL build failed: {summary.result}");
                EditorApplication.Exit(1);
            }
        }
    }
}
#endif
