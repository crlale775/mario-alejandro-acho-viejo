using UnityEngine;

public enum GameTheme
{
    Menu,
    Overworld,
    Underground,
    Water,
    Castle,
    GameOver,
    Victory,
}

/* Configuracion central de cada escena: tema, tiempo y colores. */
public static class LevelCatalog
{
    public static bool IsLevel(string sceneName)
    {
        return IsLevelName(sceneName);
    }

    private static bool IsLevelName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName)) {
            return false;
        }

        string[] parts = sceneName.Split('-');

        if (parts.Length != 2) {
            return false;
        }

        return int.TryParse(parts[0], out int _) && int.TryParse(parts[1], out int _);
    }

    public static void ParseScene(string sceneName, out int world, out int stage)
    {
        world = 1;
        stage = 1;

        if (!IsLevelName(sceneName)) {
            return;
        }

        string[] parts = sceneName.Split('-');
        int.TryParse(parts[0], out world);
        int.TryParse(parts[1], out stage);
    }

    public static int WorldFor(string sceneName)
    {
        ParseScene(sceneName, out int world, out int _);
        return world;
    }

    public static int StageFor(string sceneName)
    {
        ParseScene(sceneName, out int _, out int stage);
        return stage;
    }

    public static float TimeFor(string sceneName)
    {
        switch (sceneName)
        {
            case "1-1": return 400f;
            case "1-2": return 350f;
            case "1-3": return 350f;
            case "1-4": return 400f;
            default: return 0f;
        }
    }

    public static GameTheme ThemeFor(string sceneName)
    {
        switch (sceneName)
        {
            case "Menu": return GameTheme.Menu;
            case "GameOver": return GameTheme.GameOver;
            case "Victory": return GameTheme.Victory;
            case "1-1": return GameTheme.Overworld;
            case "1-2": return GameTheme.Underground;
            case "1-3": return GameTheme.Water;
            case "1-4": return GameTheme.Castle;
            default: return GameTheme.Overworld;
        }
    }

    public static Color BackgroundColor(GameTheme theme)
    {
        switch (theme)
        {
            case GameTheme.Menu: return new Color(0.04f, 0.05f, 0.12f);
            case GameTheme.Overworld: return new Color(0.36f, 0.58f, 0.99f);
            case GameTheme.Underground: return new Color(0.03f, 0.05f, 0.10f);
            case GameTheme.Water: return new Color(0.08f, 0.28f, 0.55f);
            case GameTheme.Castle: return new Color(0.07f, 0.03f, 0.04f);
            case GameTheme.GameOver: return new Color(0.03f, 0.03f, 0.05f);
            case GameTheme.Victory: return new Color(0.04f, 0.06f, 0.14f);
            default: return Color.black;
        }
    }

    /* Tinte que se dibuja sobre el juego (agua, lava, penumbra del castillo). */
    public static Color OverlayColor(GameTheme theme)
    {
        switch (theme)
        {
            case GameTheme.Water: return new Color(0.10f, 0.45f, 0.90f, 0.22f);
            case GameTheme.Castle: return new Color(0.55f, 0.10f, 0.02f, 0.12f);
            case GameTheme.Underground: return new Color(0.02f, 0.04f, 0.12f, 0.10f);
            default: return new Color(0f, 0f, 0f, 0f);
        }
    }

    public static bool IsWater(string sceneName)
    {
        return sceneName == "1-3";
    }

    public static string NameFor(string sceneName)
    {
        switch (sceneName)
        {
            case "1-1": return "LLANURA DE HONGOS";
            case "1-2": return "CAVERNA SUBTERRANEA  -  pulsa S en la tuberia final";
            case "1-3": return "FONDO DEL MAR  -  manten SALTO para nadar";
            case "1-4": return "CASTILLO DE BOWSER  -  llega al hacha";
            default: return string.Empty;
        }
    }

}
