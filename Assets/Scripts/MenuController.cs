using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    private struct Option
    {
        public string label;
        public string detail;
        public int world;
        public int stage;
    }

    private readonly Option[] options =
    {
        new Option { label = "NUEVA PARTIDA", detail = "Mundo 1-1 · aventura completa", world = 1, stage = 1 },
        new Option { label = "NIVEL SUBTERRANEO", detail = "Mundo 1-2 · caverna", world = 1, stage = 2 },
        new Option { label = "NIVEL ACUATICO", detail = "Mundo 1-3 · fondo del mar", world = 1, stage = 3 },
        new Option { label = "CASTILLO DE BOWSER", detail = "Mundo 1-4 · jefe final", world = 1, stage = 4 },
    };

    private Text[] labels;
    private Text[] details;
    private Text title;
    private Text subtitle;
    private Text hint;
    private int selected;
    private float time;
    private bool starting;
    private float openedAt;

    private void Start()
    {
        openedAt = Time.unscaledTime;
        Debug.Log("Menu principal iniciado");

        BuildScene();
        BuildInterface();
        Refresh();
    }

    private void Update()
    {
        if (starting) {
            return;
        }

        // evita leer pulsaciones residuales del arranque
        if (Time.unscaledTime - openedAt < 0.35f) {
            return;
        }

        if (Time.unscaledTime - openedAt < 3f && Input.anyKeyDown) {
            Debug.Log($"Input: '{Input.inputString}' up={Input.GetKeyDown(KeyCode.UpArrow)} down={Input.GetKeyDown(KeyCode.DownArrow)} enter={Input.GetKeyDown(KeyCode.Return)} space={Input.GetKeyDown(KeyCode.Space)} w={Input.GetKeyDown(KeyCode.W)} s={Input.GetKeyDown(KeyCode.S)}");
        }

        time += Time.deltaTime;

        if (title != null) {
            title.rectTransform.anchoredPosition = new Vector2(0f, 300f + Mathf.Sin(time * 1.6f) * 8f);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) {
            Move(-1);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) {
            Move(1);
        }

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Space)) {
            Play();
        }
    }

    private void Move(int direction)
    {
        selected = (selected + direction + options.Length) % options.Length;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.MenuMove);
        }

        Refresh();
    }

    private void Play()
    {
        starting = true;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.Select);
        }

        if (GameManager.Instance != null) {
            GameManager.Instance.StartAt(options[selected].world, options[selected].stage);
        } else {
            SceneManager.LoadScene($"{options[selected].world}-{options[selected].stage}");
        }
    }

    private void Refresh()
    {
        for (int i = 0; i < options.Length; i++)
        {
            bool active = i == selected;

            labels[i].text = (active ? "▶  " : "    ") + options[i].label;
            labels[i].color = active ? new Color(1f, 0.86f, 0.25f) : new Color(0.86f, 0.87f, 0.92f);
            details[i].text = options[i].detail;
            details[i].color = active ? new Color(1f, 1f, 1f, 0.92f) : new Color(1f, 1f, 1f, 0.35f);
        }
    }

    /* Decoracion del menu: suelo, colinas, arbustos y nubes. */
    private void BuildScene()
    {
        Camera camera = Camera.main;

        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 7f;
            camera.transform.position = new Vector3(0f, 6.5f, -10f);
            camera.clearFlags = CameraClearFlags.SolidColor;
        }

        camera.backgroundColor = LevelCatalog.BackgroundColor(GameTheme.Menu);

        GameObject scenery = new GameObject("Scenery");

        for (int x = -13; x <= 13; x++) {
            InstantiatePrefab("Ground", new Vector3(x + 0.5f, 0.5f, 0f), scenery.transform);
            InstantiatePrefab("Ground", new Vector3(x + 0.5f, -0.5f, 0f), scenery.transform);
        }

        InstantiatePrefab("Hill1", new Vector3(-8f, 1f, 0f), scenery.transform);
        InstantiatePrefab("Hill2", new Vector3(7.5f, 1f, 0f), scenery.transform);
        InstantiatePrefab("Bush1", new Vector3(-3.5f, 1f, 0f), scenery.transform);
        InstantiatePrefab("Bush2", new Vector3(2.5f, 1f, 0f), scenery.transform);
        InstantiatePrefab("Bush3", new Vector3(10.5f, 1f, 0f), scenery.transform);
        InstantiatePrefab("Cloud1", new Vector3(-6f, 11f, 0f), scenery.transform);
        InstantiatePrefab("Cloud2", new Vector3(3f, 12f, 0f), scenery.transform);
        InstantiatePrefab("Cloud3", new Vector3(9f, 10.5f, 0f), scenery.transform);
        InstantiatePrefab("Castle", new Vector3(-10.5f, 1f, 0f), scenery.transform);
    }

    private static void InstantiatePrefab(string name, Vector3 position, Transform parent)
    {
        GameObject prefab = Resources.Load<GameObject>($"Prefabs/{name}");

        if (prefab == null) {
            return;
        }

        GameObject instance = Instantiate(prefab, position, Quaternion.identity, parent);
        instance.name = name;

        // la decoracion no debe tener colisiones en el menu
        foreach (Collider2D collider in instance.GetComponentsInChildren<Collider2D>()) {
            Destroy(collider);
        }
    }

    private void BuildInterface()
    {
        Font font = LoadFont();

        GameObject canvasObject = new GameObject("Menu Canvas", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject shadeObject = new GameObject("Shade", typeof(RectTransform));
        shadeObject.transform.SetParent(canvasObject.transform, false);
        Image shade = shadeObject.AddComponent<Image>();
        shade.color = new Color(0.02f, 0.03f, 0.08f, 0.55f);
        shade.raycastTarget = false;
        RectTransform shadeRect = shade.rectTransform;
        shadeRect.anchorMin = Vector2.zero;
        shadeRect.anchorMax = Vector2.one;
        shadeRect.offsetMin = Vector2.zero;
        shadeRect.offsetMax = Vector2.zero;

        title = MakeText(canvasObject.transform, "Title", "SUPER MARIO BROS.", 108, new Color(1f, 0.86f, 0.22f), font, TextAnchor.MiddleCenter);
        title.rectTransform.anchoredPosition = new Vector2(0f, 300f);

        subtitle = MakeText(canvasObject.transform, "Subtitle", "EDICION ALEJANDRO ACHO  ·  UNITY", 30, new Color(0.92f, 0.95f, 1f), font, TextAnchor.MiddleCenter);
        subtitle.rectTransform.anchoredPosition = new Vector2(0f, 218f);

        labels = new Text[options.Length];
        details = new Text[options.Length];
        float startY = 90f;

        for (int i = 0; i < options.Length; i++)
        {
            labels[i] = MakeText(canvasObject.transform, $"Option{i}", options[i].label, 44, Color.white, font, TextAnchor.MiddleCenter);
            labels[i].rectTransform.anchoredPosition = new Vector2(-120f, startY - i * 96f);
            labels[i].alignment = TextAnchor.MiddleLeft;
            labels[i].rectTransform.sizeDelta = new Vector2(760f, 56f);

            details[i] = MakeText(canvasObject.transform, $"Detail{i}", options[i].detail, 24, Color.white, font, TextAnchor.MiddleCenter);
            details[i].rectTransform.anchoredPosition = new Vector2(360f, startY - i * 96f + 2f);
            details[i].rectTransform.sizeDelta = new Vector2(360f, 40f);
            details[i].alignment = TextAnchor.MiddleLeft;
        }

        hint = MakeText(canvasObject.transform, "Hint",
            "FLECHAS / W-S  ELEGIR        ENTER / ESPACIO  JUGAR        M  SILENCIAR",
            26, new Color(0.85f, 0.9f, 1f), font, TextAnchor.MiddleCenter);
        hint.rectTransform.anchoredPosition = new Vector2(0f, -330f);

        Text footer = MakeText(canvasObject.transform, "Footer",
            "Cualquier opcion inicia el juego", 24, new Color(1f, 1f, 1f, 0.55f), font, TextAnchor.MiddleCenter);
        footer.rectTransform.anchoredPosition = new Vector2(0f, -376f);
    }

    private Text MakeText(Transform parent, string name, string content, int fontSize, Color color, Font font, TextAnchor anchor)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = anchor;
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Shadow shadow = textObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
        shadow.effectDistance = new Vector2(4f, -4f);

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1400f, fontSize * 1.6f);

        return text;
    }

    private static Font LoadFont()
    {
        Font font = null;

        try {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        } catch {
            font = null;
        }

        if (font == null)
        {
            try {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            } catch {
                font = null;
            }
        }

        if (font == null) {
            font = Font.CreateDynamicFontFromOSFont("Arial", 32);
        }

        return font;
    }

}
