using UnityEngine;

/* Garantiza que los administradores persistentes existan aunque se juegue
   una escena directamente desde el editor. */
public static class GameBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        if (GameManager.Instance == null) {
            new GameObject("Game Manager").AddComponent<GameManager>();
        }

        if (AudioManager.Instance == null) {
            new GameObject("Audio Manager").AddComponent<AudioManager>();
        }

        if (HUD.Instance == null) {
            new GameObject("HUD").AddComponent<HUD>();
        }
    }
}
