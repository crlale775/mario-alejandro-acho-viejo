using UnityEngine;

/* Construye el nivel indicado al cargar la escena. */
public class LevelDirector : MonoBehaviour
{
    public string levelId = "1-2";

    private LevelBuilder builder;

    private void Awake()
    {
        float cameraHeight = levelId == "1-4" ? LevelCastle.CameraHeight : LevelUnderground.CameraHeight;

        Camera camera = CreateCamera(cameraHeight);
        builder = new LevelBuilder($"Nivel {levelId}");

        switch (levelId)
        {
            case "1-3":
                LevelWater.Build(builder);
                break;

            case "1-4":
                LevelCastle.Build(builder);
                break;

            default:
                LevelUnderground.Build(builder);
                break;
        }

        SetupCamera(camera, cameraHeight);

        if (LevelCatalog.IsWater(levelId) && builder.playerMovement != null) {
            builder.playerMovement.SetSwimming(true);
        }
    }

    private Camera CreateCamera(float height)
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();

            if (cameraObject.GetComponent<AudioListener>() == null) {
                cameraObject.AddComponent<AudioListener>();
            }
        }

        camera.orthographic = true;
        camera.orthographicSize = 7f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.transform.position = new Vector3(13f, height, -10f);

        return camera;
    }

    private void SetupCamera(Camera camera, float height)
    {
        LevelCamera levelCamera = camera.GetComponent<LevelCamera>();

        if (levelCamera == null) {
            levelCamera = camera.gameObject.AddComponent<LevelCamera>();
        }

        levelCamera.target = builder.player;
        levelCamera.levelStart = 0f;
        levelCamera.levelEnd = builder.length;
        levelCamera.fixedHeight = height;
        levelCamera.SnapToTarget();

        BridgeCollapse bridge = FindFirstObjectByType<BridgeCollapse>();

        if (bridge != null) {
            bridge.levelCamera = levelCamera;
        }
    }

}
