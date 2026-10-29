using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndScreenController : MonoBehaviour
{
    private Text title;
    private Text stats;
    private Text prompt;
    private Image flash;
    private float time;
    private bool ready;
    private bool exiting;

    private void Start()
    {
        bool victory = SceneManager.GetActiveScene().name == "Victory";
        BuildInterface(victory);
        StartCoroutine(Intro(victory));
    }

    private void Update()
    {
        time += Time.unscaledDeltaTime;

        if (title != null) {
            float bob = Mathf.Sin(time * 2f) * 6f;
            title.rectTransform.anchoredPosition = new Vector2(0f, 170f + bob);
        }

        if (prompt != null) {
            Color color = prompt.color;
            color.a = ready ? (Mathf.Repeat(time, 1.4f) < 0.7f ? 1f : 0.25f) : 0f;
            prompt.color = color;
        }

        if (ready && !exiting && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            exiting = true;

            if (AudioManager.Instance != null) {
                AudioManager.Instance.PlaySfx(Sfx.Select);
            }

            if (GameManager.Instance != null) {
                GameManager.Instance.BackToMenu();
            } else {
                SceneManager.LoadScene("Menu");
            }
        }
    }

    private IEnumerator Intro(bool victory)
    {
        if (flash != null)
        {
            flash.color = new Color(1f, 1f, 1f, 1f);
            float elapsed = 0f;

            while (elapsed < 0.35f)
            {
                elapsed += Time.unscaledDeltaTime;
                flash.color = new Color(1f, 1f, 1f, 1f - elapsed / 0.35f);
                yield return null;
            }

            flash.color = new Color(1f, 1f, 1f, 0f);
        }

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(victory ? Sfx.LevelClear : Sfx.Death);
        }

        yield return new WaitForSecondsRealtime(0.6f);
        ready = true;
    }

    private void BuildInterface(bool victory)
    {
        Font font = LoadFont();

        GameObject canvasObject = new GameObject("End Canvas", typeof(RectTransform));
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject flashObject = new GameObject("Flash", typeof(RectTransform));
        flashObject.transform.SetParent(canvasObject.transform, false);
        flash = flashObject.AddComponent<Image>();
        flash.color = new Color(1f, 1f, 1f, 0f);
        flash.raycastTarget = false;
        RectTransform flashRect = flash.rectTransform;
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.offsetMin = Vector2.zero;
        flashRect.offsetMax = Vector2.zero;

        Color titleColor = victory ? new Color(1f, 0.86f, 0.22f) : new Color(1f, 0.28f, 0.24f);
        string titleText = victory ? "¡VICTORIA!" : "GAME OVER";

        title = MakeText(canvasObject.transform, "Title", titleText, 120, titleColor, font);
        title.rectTransform.anchoredPosition = new Vector2(0f, 170f);

        GameManager game = GameManager.Instance;
        int finalScore = game != null ? game.score : 0;
        int finalCoins = game != null ? game.coins : 0;
        string worldReached = game != null ? $"{game.world}-{game.stage}" : "1-1";

        string body = victory
            ? "¡Bowser cayo al lava y el reino esta a salvo!"
            : "Te has quedado sin vidas... pero puedes intentarlo otra vez.";

        Text message = MakeText(canvasObject.transform, "Message", body, 34, new Color(0.92f, 0.94f, 1f), font);
        message.rectTransform.anchoredPosition = new Vector2(0f, 60f);

        stats = MakeText(canvasObject.transform, "Stats",
            $"PUNTAJE FINAL  {finalScore:000000}\nMONEDAS  {finalCoins:00}\nMUNDO ALCANZADO  {worldReached}",
            40, Color.white, font);
        stats.rectTransform.anchoredPosition = new Vector2(0f, -60f);

        prompt = MakeText(canvasObject.transform, "Prompt", "PRESIONA ENTER PARA VOLVER AL MENU", 30,
            new Color(1f, 1f, 1f, 0f), font);
        prompt.rectTransform.anchoredPosition = new Vector2(0f, -260f);
    }

    private Text MakeText(Transform parent, string name, string content, int fontSize, Color color, Font font)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        Text text = textObject.AddComponent<Text>();
        text.text = content;
        text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        Outline outline = textObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
        outline.effectDistance = new Vector2(4f, -4f);

        RectTransform rect = text.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(1600f, fontSize * 2.4f);

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
