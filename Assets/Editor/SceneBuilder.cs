using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/* Genera las escenas del juego y configura el proyecto.
   Se puede ejecutar desde el menu "Mario" o en lote con
   -executeMethod SceneBuilder.BuildAll */
public static class SceneBuilder
{
    private const string ScenesFolder = "Assets/Scenes";

    [MenuItem("Mario/Reconstruir escenas y ajustes")]
    public static void BuildAll()
    {
        CreateLevelScene("1-2");
        CreateLevelScene("1-3");
        CreateLevelScene("1-4");
        CreateMenuScene();
        CreateEndScene("GameOver");
        CreateEndScene("Victory");

        ConfigureBuildSettings();
        ConfigurePlayerSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Escenas y ajustes del proyecto generados correctamente.");
    }

    private static void CreateLevelScene(string levelId)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject directorObject = new GameObject("Level Director");
        LevelDirector director = directorObject.AddComponent<LevelDirector>();
        director.levelId = levelId;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/{levelId}.unity");
    }

    private static void CreateMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject menuObject = new GameObject("Menu");
        menuObject.AddComponent<MenuController>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/Menu.unity");
    }

    private static void CreateEndScene(string sceneName)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject endObject = new GameObject("End Screen");
        endObject.AddComponent<EndScreenController>();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, $"{ScenesFolder}/{sceneName}.unity");
    }

    private static void ConfigureBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene($"{ScenesFolder}/Menu.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/1-1.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/1-2.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/1-3.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/1-4.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/GameOver.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/Victory.unity", true),
        };
    }

    private static void ConfigurePlayerSettings()
    {
        PlayerSettings.productName = "Mario Acho";
        PlayerSettings.companyName = "Alejandro Acho";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.resizableWindow = true;
        PlayerSettings.runInBackground = true;
        PlayerSettings.colorSpace = ColorSpace.Gamma;
    }

}
