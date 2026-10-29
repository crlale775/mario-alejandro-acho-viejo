using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int world { get; private set; } = 1;
    public int stage { get; private set; } = 1;
    public int lives { get; private set; } = 3;
    public int coins { get; private set; }
    public int score { get; private set; }

    public float timeRemaining { get; private set; }
    public bool timerRunning { get; private set; }
    public bool playing { get; private set; }

    private void Awake()
    {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

        // si se juega una escena de nivel directamente, aplica su configuracion
        Scene scene = SceneManager.GetActiveScene();

        if (LevelCatalog.IsLevel(scene.name)) {
            HandleSceneLoaded(scene, LoadSceneMode.Single);
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Escena cargada: {scene.name}");

        playing = LevelCatalog.IsLevel(scene.name);

        LevelCatalog.ParseScene(scene.name, out int sceneWorld, out int sceneStage);

        if (playing)
        {
            world = sceneWorld;
            stage = sceneStage;
            timeRemaining = LevelCatalog.TimeFor(scene.name);
            timerRunning = true;
        }
        else
        {
            timerRunning = false;
        }

        GameTheme theme = LevelCatalog.ThemeFor(scene.name);

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlayTheme(theme);
        }

        if (HUD.Instance != null) {
            HUD.Instance.Show(playing);
            HUD.Instance.SetOverlay(LevelCatalog.OverlayColor(theme));

            if (playing) {
                HUD.Instance.ShowLevelBanner($"MUNDO {world}-{stage}", LevelCatalog.NameFor(scene.name));
            }
        }

        Camera mainCamera = Camera.main;

        if (mainCamera != null) {
            mainCamera.backgroundColor = LevelCatalog.BackgroundColor(theme);
        }
    }

    private void Update()
    {
        if (playing && Input.GetKeyDown(KeyCode.Escape)) {
            BackToMenu();
            return;
        }

        if (!timerRunning || !playing) {
            return;
        }

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            timerRunning = false;
            TimeUp();
        }
    }

    public void TimeUp()
    {
        Player player = FindFirstObjectByType<Player>();

        if (player != null && !player.dead) {
            player.Kill();
        } else {
            ResetLevel(2f);
        }
    }

    public void NewGame()
    {
        StartAt(1, 1);
    }

    public void StartAt(int levelWorld, int levelStage)
    {
        lives = 3;
        coins = 0;
        score = 0;
        timeRemaining = 0f;
        LoadLevel(levelWorld, levelStage);
    }

    public void LoadLevel(int levelWorld, int levelStage)
    {
        world = levelWorld;
        stage = levelStage;

        SceneManager.LoadScene($"{levelWorld}-{levelStage}");
    }

    public void NextLevel()
    {
        int nextStage = stage + 1;

        if (nextStage > 4) {
            Victory();
            return;
        }

        LoadLevel(world, nextStage);
    }

    /* Secuencia de fin de nivel: bonifica el tiempo restante y avanza. */
    public void CompleteLevel()
    {
        if (!playing) {
            return;
        }

        timerRunning = false;

        int bonus = Mathf.RoundToInt(timeRemaining) * 10;
        AddScore(bonus);

        if (HUD.Instance != null) {
            HUD.Instance.ShowBonus(bonus);
        }

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.LevelClear);
        }

        CancelInvoke(nameof(FinishLevel));
        Invoke(nameof(FinishLevel), 2.2f);
    }

    private void FinishLevel()
    {
        NextLevel();
    }

    public void GameOver()
    {
        timerRunning = false;
        playing = false;
        lives = 0;

        SceneManager.LoadScene("GameOver");
    }

    public void Victory()
    {
        timerRunning = false;
        playing = false;

        SceneManager.LoadScene("Victory");
    }

    public void ResetLevel(float delay)
    {
        timerRunning = false;

        CancelInvoke(nameof(ResetLevel));
        Invoke(nameof(ResetLevel), delay);
    }

    public void ResetLevel()
    {
        lives--;

        if (lives > 0) {
            LoadLevel(world, stage);
        } else {
            GameOver();
        }
    }

    public void AddCoin()
    {
        coins++;
        AddScore(200);

        if (coins >= 100)
        {
            coins -= 100;
            AddLife();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void AddLife()
    {
        lives++;

        if (AudioManager.Instance != null) {
            AudioManager.Instance.PlaySfx(Sfx.OneUp);
        }
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

}
