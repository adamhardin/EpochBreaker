using UnityEditor;
using UnityEngine;

public static class BuildScript
{
    public static void PerformBuild()
    {
        string buildPath = "build/WebGL/WebGL";

        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Bootstrap.unity" },
            locationPathName = buildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"WebGL build succeeded: {buildPath} ({report.summary.totalSize} bytes)");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"WebGL build failed: {report.summary.result}");
            EditorApplication.Exit(1);
        }
    }
}
