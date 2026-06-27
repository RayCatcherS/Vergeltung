#if UNITY_EDITOR
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Script di build per la piattaforma Web (WebGL/WebGPU).
/// Usa le scene già configurate in File > Build Settings.
/// Richiamabile da batchmode:
///   Unity.exe -quit -batchmode -projectPath "&lt;repo&gt;" -executeMethod WebBuildScript.BuildWeb -logFile "&lt;log&gt;"
/// </summary>
public static class WebBuildScript {

    private const string OutputPath = "Builds/Web";
    private const string CompatDefine = "URP_COMPATIBILITY_MODE";

    /// <summary>
    /// Aggiunge il define URP_COMPATIBILITY_MODE ai Scripting Define Symbols del target Web.
    /// Va eseguito in un lancio Unity SEPARATO rispetto alla build: il define richiede
    /// la ricompilazione del package URP, che avviene solo al lancio successivo.
    /// </summary>
    [MenuItem("Build/Setup Web Compatibility Define")]
    public static void AddCompatibilityDefine() {

        var nbt = NamedBuildTarget.WebGL;
        string defines = PlayerSettings.GetScriptingDefineSymbols(nbt);

        if (defines.Split(';').Contains(CompatDefine)) {
            Debug.Log($"WEB SETUP: '{CompatDefine}' già presente nei define Web ({defines}).");
        } else {
            defines = string.IsNullOrEmpty(defines) ? CompatDefine : defines + ";" + CompatDefine;
            PlayerSettings.SetScriptingDefineSymbols(nbt, defines);
            AssetDatabase.SaveAssets();
            Debug.Log($"WEB SETUP: aggiunto '{CompatDefine}'. Define Web ora: {defines}");
        }

        EditorApplication.Exit(0);
    }

    /// <summary>
    /// XInputDotNet è una libreria nativa solo-Windows (vibrazione pad). Le sue DLL x86/x86_64
    /// erano marcate "Any Platform" → su WebGL collidono (stesso file di destinazione) e comunque
    /// non possono girare nel browser. Qui le limitiamo alla rispettiva piattaforma Windows + Editor,
    /// escludendo WebGL. Va eseguito in un lancio SEPARATO rispetto alla build (fa un reimport).
    /// </summary>
    [MenuItem("Build/Fix XInput Plugins For Web")]
    public static void FixXInputPlugins() {
        // x86 (32 bit): SOLO Standalone Windows 32-bit. NON Editor (l'Editor è 64-bit), NON WebGL.
        FixNativePlugin("Assets/XInputDotNet/Plugins/x86/XInputInterface.dll", win64: false, editor: false);
        // x86_64: Standalone Windows 64-bit + Editor (x86_64). NON WebGL.
        FixNativePlugin("Assets/XInputDotNet/Plugins/x86_64/XInputInterface.dll", win64: true, editor: true);
        AssetDatabase.Refresh();
        EditorApplication.Exit(0);
    }

    private static void FixNativePlugin(string path, bool win64, bool editor) {
        var imp = AssetImporter.GetAtPath(path) as PluginImporter;
        if (imp == null) {
            Debug.LogError($"WEB SETUP: PluginImporter non trovato per '{path}'.");
            return;
        }
        imp.SetCompatibleWithAnyPlatform(false);
        imp.SetCompatibleWithEditor(editor);
        if (editor) {
            imp.SetEditorData("CPU", "x86_64");
            imp.SetEditorData("OS", "Windows");
        }
        imp.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows, !win64);
        imp.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64, win64);
        imp.SetCompatibleWithPlatform(BuildTarget.WebGL, false);
        imp.SaveAndReimport();
        Debug.Log($"WEB SETUP: plugin '{path}' -> Win{(win64 ? "64" : "32")}, Editor={editor}, WebGL=escluso");
    }

    [MenuItem("Build/Build Web")]
    public static void BuildWeb() {

        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length == 0) {
            Debug.LogError("WEB BUILD: nessuna scena abilitata in Build Settings.");
            EditorApplication.Exit(1);
            return;
        }

        Debug.Log($"WEB BUILD: avvio con {scenes.Length} scene -> {string.Join(", ", scenes)}");

        var options = new BuildPlayerOptions {
            scenes = scenes,
            locationPathName = OutputPath,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded) {
            Debug.Log($"WEB BUILD SUCCEEDED: {summary.totalSize} bytes, {summary.totalTime}, output '{OutputPath}'");
            EditorApplication.Exit(0);
        } else {
            Debug.LogError($"WEB BUILD FAILED: result={summary.result}, errors={summary.totalErrors}");
            EditorApplication.Exit(2);
        }
    }
}
#endif
