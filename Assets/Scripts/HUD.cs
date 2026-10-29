using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public static HUD Instance { get; private set; }

    private Canvas canvas;
    private Image overlay;
    private Text scoreText;
    private Text coinsText;
    private Text worldText;
    private Text timeText;
    private Text livesText;
    private Text bannerText;
    private Text bonusText;

    private Coroutine bannerRoutine;
    private Coroutine bonusRoutine;

    private static readonly Color HUD_WHITE = new Color(1f, 1f, 1f, 1f);
    private static readonly Color HUD_RED = new Color(1f, 0.30f, 0.24f, 1f);
    private static readonly Color HUD_YELLOW = new Color(1f, 0.86f, 0.30f, 1f);

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Build();
    }

    private void Build()
    {
        Font font = LoadFont();

        GameObject canvasObject = new GameObject("HUD Canvas", typeof(RectTransform));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // tinte del nivel (agua, lava) por detras de los textos
        GameObject overlayObject = new GameObject("Level Tint", typeof(RectTransform));
        overlayObject.transform.SetParent(canvasObject.transform, false);
        overlay = overlayObject.AddComponent<Image>();
        overlay.raycastTarget = false;
        overlay.color = new Color(0f, 0f, 0f, 0f);
        RectTransform overlayRect = overlay.rectTransform;
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;

        scoreText = MakeText(canvasObject.transform, "Score", "PUNTAJE 000000", TextAnchor.UpperLeft, new Vector2(0f, 1f), 420f, 32, HUD_WHITE, font);
        coinsText = MakeText(canvasObject.transform, "Coins", "MONEDAS x00", TextAnchor.UpperLeft, new Vector2(0f, 1f), 420f, 28, HUD_WHITE, font);
        worldText = MakeText(canvasObject.transform, "World", "MUNDO 1-1", TextAnchor.UpperCenter, new Vector2(0.5f, 1f), 420f, 32, HUD_WHITE, font);
        timeText = MakeText(canvasObject.transform, "Time", "TIEMPO 300", TextAnchor.UpperCenter, new Vector2(0.5f, 1f), 420f, 28, HUD_WHITE, font);
        livesText = MakeText(canvasObject.transform, "Lives", "VIDAS x3", TextAnchor.UpperRight, new Vector2(1f, 1f), 320f, 32, HUD_WHITE, font);

        bonusText = MakeText(canvasObject.transform, "Bonus", "", TextAnchor.UpperCenter, new Vector2(0.5f, 0.5f), 600f, 56, HUD_YELLOW, font);
        bonusText.gameObject.SetActive(false);

        bannerText = MakeText(canvasObject.transform, "Banner", "MUNDO 1-1", TextAnchor.MiddleCenter, new Vector2(0.5f, 0.5f), 1400f, 64, HUD_WHITE, font);
        bannerText.gameObject.SetActive(false);

        canvas.enabled = false;
    }

    private Text MakeText(Transform parent, string name, string content, TextAnchor anchor, Vector2 pivot,
        float width, int fontSize, Color color, Font font)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        text.fontStyle = FontStyle.Bold;

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(3f, -3f);

        RectTransform rect = text.rectTransform;

        if (anchor == TextAnchor.UpperLeft)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(48f, name == "Score" ? -34f : -86f);
        }
        else if (anchor == TextAnchor.UpperRight)
        {
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-48f, -34f);
        }
        else if (anchor == TextAnchor.UpperCenter)
        {
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, name == "World" ? -34f : -86f);
        }
        else
        {
            rect.anchorMin = pivot;
            rect.anchorMax = pivot;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
        }

        rect.sizeDelta = new Vector2(width, fontSize * 1.6f);

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

    public void Show(bool visible)
    {
        if (canvas == null) {
            return;
        }

        canvas.enabled = visible;

        if (!visible)
        {
            if (bannerRoutine != null) {
                StopCoroutine(bannerRoutine);
            }

            if (bannerText != null) {
                bannerText.gameObject.SetActive(false);
            }
        }
    }

    public void SetOverlay(Color color)
    {
        if (overlay != null) {
            overlay.color = color;
        }
    }

    public void ShowLevelBanner(string title, string subtitle)
    {
        if (bannerText == null) {
            return;
        }

        if (bannerRoutine != null) {
            StopCoroutine(bannerRoutine);
        }

        bannerRoutine = StartCoroutine(BannerRoutine(string.IsNullOrEmpty(subtitle) ? title : $"{title}\n<size=34>{subtitle}</size>"));
    }

    private IEnumerator BannerRoutine(string text)
    {
        bannerText.text = text;
        bannerText.gameObject.SetActive(true);

        Color color = bannerText.color;
        float elapsed = 0f;

        while (elapsed < 2.4f)
        {
            elapsed += Time.unscaledDeltaTime;
            color.a = elapsed < 1.8f ? 1f : Mathf.Clamp01(1f - (elapsed - 1.8f) / 0.6f);
            bannerText.color = color;
            yield return null;
        }

        color.a = 0f;
        bannerText.color = color;
        bannerText.gameObject.SetActive(false);
        bannerRoutine = null;
    }

    public void ShowBonus(int bonus)
    {
        if (bonusText == null) {
            return;
        }

        if (bonusRoutine != null) {
            StopCoroutine(bonusRoutine);
        }

        bonusRoutine = StartCoroutine(BonusRoutine(bonus));
    }

    private IEnumerator BonusRoutine(int bonus)
    {
        bonusText.text = $"¡NIVEL SUPERADO!\n<size=40>BONO DE TIEMPO +{bonus}</size>";
        bonusText.gameObject.SetActive(true);

        float elapsed = 0f;

        while (elapsed < 2.6f)
        {
            elapsed += Time.unscaledDeltaTime;
            Color color = HUD_YELLOW;
            color.a = elapsed < 2f ? 1f : Mathf.Clamp01(1f - (elapsed - 2f) / 0.6f);
            bonusText.color = color;
            yield return null;
        }

        bonusText.gameObject.SetActive(false);
        bonusRoutine = null;
    }

    private bool loggedMetrics;
    private bool capturedShot;

    private void Update()
    {
        if (!loggedMetrics && Time.unscaledTime > 1f)
        {
            loggedMetrics = true;
            Camera main = Camera.main;
            RectTransform rect = canvas.GetComponent<RectTransform>();
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            Debug.Log($"PANTALLA {Screen.width}x{Screen.height} dpi={Screen.dpi} escala={canvas.scaleFactor} scaler={(scaler != null ? scaler.scaleFactor : -1f)} rect={rect.rect.width}x{rect.rect.height} camara={(main != null ? main.pixelWidth + "x" + main.pixelHeight : "n/d")} res={Screen.currentResolution.width}x{Screen.currentResolution.height}");
        }

        if (!capturedShot && Time.unscaledTime > 2f)
        {
            capturedShot = true;
            ScreenCapture.CaptureScreenshot("C:/Users/ALEJANDRO/Desktop/tarea uwuw/shot-ingame.png");
        }

        if (canvas == null || !canvas.enabled) {
            return;
        }

        GameManager game = GameManager.Instance;

        if (game == null) {
            return;
        }

        scoreText.text = $"PUNTAJE {game.score:000000}";
        coinsText.text = $"MONEDAS x{game.coins:00}";
        worldText.text = $"MUNDO {game.world}-{game.stage}";
        livesText.text = $"VIDAS x{game.lives}";

        int seconds = Mathf.CeilToInt(Mathf.Max(0f, game.timeRemaining));
        timeText.text = $"TIEMPO {seconds:000}";
        bool urgent = seconds <= 100 && Mathf.Repeat(Time.time, 0.5f) < 0.25f;
        timeText.color = urgent ? HUD_RED : HUD_WHITE;
    }

}
