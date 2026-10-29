using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

/* Compila el ejecutable de Windows. Se puede usar desde el menu "Mario"
   o en lote con -executeMethod BuildScript.BuildWindows */
public static class BuildScript
{
    private const string OutputFolder = "Build/Windows";
    private const string ExecutableName = "Mario Acho.exe";

    [MenuItem("Mario/Compilar para Windows")]
    public static void BuildWindows()
    {
        string[] scenes =
        {
            "Assets/Scenes/Menu.unity",
            "Assets/Scenes/1-1.unity",
            "Assets/Scenes/1-2.unity",
            "Assets/Scenes/1-3.unity",
            "Assets/Scenes/1-4.unity",
            "Assets/Scenes/GameOver.unity",
            "Assets/Scenes/Victory.unity",
        };

        string absoluteFolder = Path.Combine(Directory.GetCurrentDirectory(), OutputFolder);
        Directory.CreateDirectory(absoluteFolder);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = Path.Combine(absoluteFolder, ExecutableName),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        Debug.Log($"Resultado del build: {summary.result} | tamano: {summary.totalSize} bytes | errores: {summary.totalErrors}");

        if (summary.result != BuildResult.Succeeded) {
            throw new Exception($"El build fallo con resultado {summary.result}");
        }
    }

}
