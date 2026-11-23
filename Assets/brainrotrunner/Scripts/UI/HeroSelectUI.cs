using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
using BrainrotRunner.Core;
using BrainrotRunner.Player;
using BrainrotRunner.World;

public class HeroSelectUI : MonoBehaviour
{
    public enum UIState { SplashIntro, MainHub, BiomesMenu, HeroWardrobe, MissionsMenu, ShopMenu, Gameplay }

    [Header("Посилання на систему")]
    public HeroSwitcher heroSwitcher;
    public BrainrotGameplaySystems gameplaySystems;
    public GameObject splashPanel;
    public GameObject mainHubPanel;
    public GameObject biomesPanel;
    public GameObject wardrobePanel;
    public GameObject missionsPanel;
    public GameObject shopPanel;
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject revivePanel;
    public GameObject levelUpPanel;

    [Header("Анімовані об'єкти")]
    public RectTransform splashLogoRect;
    public TMP_Text tapToPlayText;
    public RectTransform hubLogoRect;
    public TMP_Text hubTapToRunText;

    [Header("Кнопки навігації")]
    public Button splashFullScreenButton;
    public Button hubTapToRunButton;
    public Button centerPlayButton;
    public Button biomesTabButton;
    public Button heroesTabButton;
    public Button missionsTabButton;
    public Button shopTabButton;
    public Button hubMusicToggleButton;
    public TMP_Text hubMusicToggleLabel;
    public Button biomesBackButton;
    public Button wardrobeBackButton;
    public Button missionsBackButton;
    public Button shopBackButton;

    [Header("Картки 3 Біомів (Одеса відкрита одразу)")]
    public Button[] biomeSelectButtons = new Button[3];
    public Image[] biomeLockOverlays = new Image[3];
    public TMP_Text[] biomeStatusLabels = new TMP_Text[3];

    [Header("Вибір 10 героїв")]
    public Button prevButton;
    public Button nextButton;
    public Button playFromWardrobeButton;
    public TMP_Text playFromWardrobeLabel;
    public TMP_Text heroCounterText;
    public TMP_Text heroNameText;
    public TMP_Text heroSubtitleText;
    public TMP_Text heroLockStatusText;

    [Header("Місії (3 завдання)")]
    public TMP_Text[] missionTitleTexts = new TMP_Text[3];
    public Button[] missionClaimButtons = new Button[3];
    public TMP_Text[] missionClaimLabels = new TMP_Text[3];

    [Header("Магазин ключів (3 пакети)")]
    public TMP_Text shopWalletText;
    public TMP_Text shopKeysOwnedText;
    public Button buyKey1Button;
    public Button buyKey3Button;
    public Button buyKey10Button;

    [Header("Верхній бар хабу та ігровий HUD")]
    public TMP_Text hubCoinsText;
    public TMP_Text hubKeysText;
    public TMP_Text hubHighScoreText;
    public TMP_Text hubBiomeBadgeText;

    public Button pauseButton;
    public TMP_Text scoreText;
    public TMP_Text coinsText;
    public TMP_Text keysText;
    public TMP_Text biomeStatusText;
    public Image hudProgressFill;

    [Header("Меню Паузи")]
    public TMP_Text pauseStatsText;
    public Button resumeButton;
    public Button restartButton;
    public Button quitToMenuButton;
    public Button toggleAudioButton;
    public TMP_Text toggleAudioLabel;

    [Header("Вікно Воскресіння (Save Me!)")]
    public TMP_Text reviveKeysCountText;
    public TMP_Text reviveTimerText;
    public Button reviveWithKeyButton;
    public Button declineReviveButton;

    [Header("Вікно LEVEL UP / SKIN UNLOCKED")]
    public List<Sprite> heroIcons = new List<Sprite>();
    public TMP_Text levelUpSubtitleText;
    public Image unlockedHeroIconImage;
    public TMP_Text unlockedHeroNameText;
    public TMP_Text unlockedHeroTagText;
    public Button levelUpNextButton;
    public TMP_Text levelUpButtonText;
    public TMP_Text levelUpBonusText;

    [Header("Налаштування прогресії")]
    public float scorePerLevel = 500000f;
    public int levelBonusCoins = 500;
    public float autoCloseSeconds = 1.0f;
    public MonoBehaviour[] gameplayScriptsToPause;

    private UIState currentState = UIState.SplashIntro;
    private float distanceScore = 0f;
    private int bestScore = 0;
    private int currentLevel = 1;
    private int runCoins = 0;
    private int walletCoins = 0;
    private int totalKeys = 3;
    private int activeBiomeIndex = 0;
    private int unlockedHeroCount = 1;
    private float prevLevelScoreTarget = 0f;
    private float nextLevelScoreTarget = 500000f;
    private bool isPaused = false;
    private bool isWaitingRevive = false;
    private float invulnerableTimer = 0f;
    private bool isMusicEnabled = true;
    private readonly bool[] missionClaimed = new bool[3];

    private Coroutine levelUpRoutine;
    private Coroutine reviveRoutine;

    private Camera mainCam;
    private Vector3 defaultCamPos;
    private Quaternion defaultCamRot;
    private bool hasSavedCamTransform = false;

    private BrainrotAudioManager audioManager;
    private ChunkSpawner chunkSpawner;
    private readonly string[] biomeNames = { "МІСТО ОДЕСА", "ТУНДРА", "САВАНА" };
    private readonly int[] biomeUnlockScores = { 0, 500000, 1000000 };

    public bool IsGameplayRunning() => currentState == UIState.Gameplay && !isPaused && !isWaitingRevive;
    public bool IsInvulnerable() => invulnerableTimer > 0f;

    private PlayerMotor GetPlayerMotor()
    {
        if (heroSwitcher != null)
        {
            PlayerMotor m = heroSwitcher.GetComponent<PlayerMotor>();
            if (m != null) return m;
        }
        return FindFirstObjectByType<PlayerMotor>();
    }

    private float GetStepScore()
    {
        return scorePerLevel >= 500000f ? scorePerLevel : 500000f;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        DisableLegacyOverlays();

        // Фіксуємо крок рівнів на 500,000 (1-й перехід: 500,000; 2-й перехід: 1,000,000)
        scorePerLevel = GetStepScore();
        autoCloseSeconds = 1.0f;

        audioManager = FindFirstObjectByType<BrainrotAudioManager>();
        if (audioManager == null)
        {
            GameObject audioObj = new GameObject("BrainrotAudioManager");
            audioManager = audioObj.AddComponent<BrainrotAudioManager>();
        }

        gameplaySystems = FindFirstObjectByType<BrainrotGameplaySystems>();
        if (gameplaySystems == null)
        {
            gameplaySystems = gameObject.AddComponent<BrainrotGameplaySystems>();
        }
        gameplaySystems.ui = this;

        chunkSpawner = FindFirstObjectByType<ChunkSpawner>();

        if (heroSwitcher == null)
            heroSwitcher = FindFirstObjectByType<HeroSwitcher>();

        mainCam = Camera.main;
        if (mainCam != null)
        {
            defaultCamPos = mainCam.transform.position;
            defaultCamRot = mainCam.transform.rotation;
            hasSavedCamTransform = true;
        }

        runCoins = 0;
        distanceScore = 0f;
        walletCoins = PlayerPrefs.GetInt("WalletCoins", 0);
        bestScore = PlayerPrefs.GetInt("BestScore", 0);
        totalKeys = PlayerPrefs.GetInt("TotalKeys", 3);
        unlockedHeroCount = Mathf.Clamp(PlayerPrefs.GetInt("UnlockedHeroes", 1), 1, 10);
        isMusicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;

        for (int i = 0; i < 3; i++)
            missionClaimed[i] = PlayerPrefs.GetInt($"MissionClaimed_{i}", 0) == 1;

        ApplyAudioState();
        BindButtons();

        PlayerMotor motor = GetPlayerMotor();
        if (motor != null) motor.ResetState();
        SetGameplayActive(false);

        if (heroSwitcher != null && heroSwitcher.heroModels.Count > 0)
            heroSwitcher.SwitchHero(0);

        if (pausePanel != null) pausePanel.SetActive(false);
        if (revivePanel != null) revivePanel.SetActive(false);

        ApplyBiomeEverywhere(activeBiomeIndex);
        SwitchState(UIState.SplashIntro);
        if (isMusicEnabled) audioManager.PlayMenuMusic();
    }

    private void ApplyBiomeEverywhere(int biomeIdx)
    {
        gameplaySystems?.ApplyBiomeVisualTheme(biomeIdx);
        if (chunkSpawner == null)
            chunkSpawner = FindFirstObjectByType<ChunkSpawner>();
        if (chunkSpawner != null)
            chunkSpawner.SetBiomeImmediate(biomeIdx);
    }

    private void ApplyAudioState()
    {
        AudioListener.pause = false;
        AudioListener.volume = isMusicEnabled ? 1f : 0f;
        if (audioManager != null)
            audioManager.masterVolume = isMusicEnabled ? 0.5f : 0f;

        if (hubMusicToggleLabel != null)
            hubMusicToggleLabel.text = isMusicEnabled ? "МУЗИКА: ВКЛ" : "МУЗИКА: ВИКЛ";

        if (toggleAudioLabel != null)
            toggleAudioLabel.text = isMusicEnabled ? "МУЗИКА: УВІМКНЕНО" : "МУЗИКА: ВИМКНЕНО";
    }

    public void ToggleMusic()
    {
        isMusicEnabled = !isMusicEnabled;
        PlayerPrefs.SetInt("MusicEnabled", isMusicEnabled ? 1 : 0);
        PlayerPrefs.Save();
        ApplyAudioState();

        if (isMusicEnabled)
        {
            audioManager?.PlayClick();
            if (currentState == UIState.Gameplay) audioManager?.PlayRunMusic();
            else audioManager?.PlayMenuMusic();
        }
    }

    private void BindButtons()
    {
        if (splashFullScreenButton != null)
        {
            splashFullScreenButton.onClick.RemoveAllListeners();
            splashFullScreenButton.onClick.AddListener(() => { audioManager?.PlayClick(); SwitchState(UIState.MainHub); });
        }

        if (hubTapToRunButton != null)
        {
            hubTapToRunButton.onClick.RemoveAllListeners();
            hubTapToRunButton.onClick.AddListener(OnPlayButtonClicked);
        }

        if (centerPlayButton == null && mainHubPanel != null)
        {
            Transform found = mainHubPanel.transform.Find("CenterPlayButton");
            if (found != null) centerPlayButton = found.GetComponent<Button>();
        }

        if (centerPlayButton != null)
        {
            centerPlayButton.onClick.RemoveAllListeners();
            centerPlayButton.onClick.AddListener(OnPlayButtonClicked);
        }

        if (hubMusicToggleButton != null)
        {
            hubMusicToggleButton.onClick.RemoveAllListeners();
            hubMusicToggleButton.onClick.AddListener(ToggleMusic);
        }

        if (biomesTabButton != null)
        {
            biomesTabButton.onClick.RemoveAllListeners();
            biomesTabButton.onClick.AddListener(() => { audioManager?.PlayClick(); SwitchState(UIState.BiomesMenu); });
        }

        if (heroesTabButton != null)
        {
            heroesTabButton.onClick.RemoveAllListeners();
            heroesTabButton.onClick.AddListener(() => { audioManager?.PlayClick(); SwitchState(UIState.HeroWardrobe); });
        }

        if (missionsTabButton != null)
        {
            missionsTabButton.onClick.RemoveAllListeners();
            missionsTabButton.onClick.AddListener(() => { audioManager?.PlayClick(); SwitchState(UIState.MissionsMenu); });
        }

        if (shopTabButton != null)
        {
            shopTabButton.onClick.RemoveAllListeners();
            shopTabButton.onClick.AddListener(() => { audioManager?.PlayClick(); SwitchState(UIState.ShopMenu); });
        }

        if (biomesBackButton != null)
        {
            biomesBackButton.onClick.RemoveAllListeners();
            biomesBackButton.onClick.AddListener(() => { audioManager?.PlayClick(); SwitchState(UIState.MainHub); });
        }

        if (missionsBackButton != null)
        {
            missionsBackButton.onClick.RemoveAllListeners();
            missionsBackButton.onClick.AddListener(() => { audioManager?.PlayClick(); SwitchState(UIState.MainHub); });
        }

        if (shopBackButton != null)
        {
            shopBackButton.onClick.RemoveAllListeners();
            shopBackButton.onClick.AddListener(() => { audioManager?.PlayClick(); SwitchState(UIState.MainHub); });
        }

        if (wardrobeBackButton != null)
        {
            wardrobeBackButton.onClick.RemoveAllListeners();
            wardrobeBackButton.onClick.AddListener(() =>
            {
                audioManager?.PlayClick();
                if (heroSwitcher != null && heroSwitcher.activeHeroIndex >= unlockedHeroCount)
                    heroSwitcher.SwitchHero(0);
                SwitchState(UIState.MainHub);
            });
        }

        for (int i = 0; i < biomeSelectButtons.Length; i++)
        {
            int idx = i;
            if (biomeSelectButtons[idx] != null)
            {
                biomeSelectButtons[idx].onClick.RemoveAllListeners();
                biomeSelectButtons[idx].onClick.AddListener(() => SelectBiome(idx));
            }
        }

        for (int i = 0; i < missionClaimButtons.Length; i++)
        {
            int idx = i;
            if (missionClaimButtons[idx] != null)
            {
                missionClaimButtons[idx].onClick.RemoveAllListeners();
                missionClaimButtons[idx].onClick.AddListener(() => ClaimMissionReward(idx));
            }
        }

        if (buyKey1Button != null)
        {
            buyKey1Button.onClick.RemoveAllListeners();
            buyKey1Button.onClick.AddListener(() => BuyKeysPack(1, 500));
        }

        if (buyKey3Button != null)
        {
            buyKey3Button.onClick.RemoveAllListeners();
            buyKey3Button.onClick.AddListener(() => BuyKeysPack(3, 1200));
        }

        if (buyKey10Button != null)
        {
            buyKey10Button.onClick.RemoveAllListeners();
            buyKey10Button.onClick.AddListener(() => BuyKeysPack(10, 3500));
        }

        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(() => { audioManager?.PlayClick(); heroSwitcher?.PrevHero(); UpdateAllLabels(); });
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => { audioManager?.PlayClick(); heroSwitcher?.NextHero(); UpdateAllLabels(); });
        }

        if (playFromWardrobeButton != null)
        {
            playFromWardrobeButton.onClick.RemoveAllListeners();
            playFromWardrobeButton.onClick.AddListener(() =>
            {
                if (heroSwitcher != null && heroSwitcher.activeHeroIndex >= unlockedHeroCount) return;
                OnPlayButtonClicked();
            });
        }

        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveAllListeners();
            pauseButton.onClick.AddListener(TogglePause);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(ResumeGame);
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveAllListeners();
            restartButton.onClick.AddListener(RestartRunClean);
        }

        if (quitToMenuButton != null)
        {
            quitToMenuButton.onClick.RemoveAllListeners();
            quitToMenuButton.onClick.AddListener(QuitRunToMenu);
        }

        if (toggleAudioButton != null)
        {
            toggleAudioButton.onClick.RemoveAllListeners();
            toggleAudioButton.onClick.AddListener(ToggleMusic);
        }

        if (reviveWithKeyButton != null)
        {
            reviveWithKeyButton.onClick.RemoveAllListeners();
            reviveWithKeyButton.onClick.AddListener(ReviveWithKey);
        }

        if (declineReviveButton != null)
        {
            declineReviveButton.onClick.RemoveAllListeners();
            declineReviveButton.onClick.AddListener(FinishRunAfterDeath);
        }

        if (levelUpNextButton != null)
        {
            levelUpNextButton.onClick.RemoveAllListeners();
            levelUpNextButton.onClick.AddListener(() => { audioManager?.PlayClick(); CloseLevelUpImmediate(); });
        }
    }

    public void OnPlayButtonClicked()
    {
        if (currentState == UIState.Gameplay) return;
        audioManager?.PlayClick();
        StartRun();
    }

    private void BuyKeysPack(int keysAmount, int priceCoins)
    {
        if (walletCoins < priceCoins) return;
        walletCoins -= priceCoins;
        totalKeys += keysAmount;
        PlayerPrefs.SetInt("WalletCoins", walletCoins);
        PlayerPrefs.SetInt("TotalKeys", totalKeys);
        PlayerPrefs.Save();
        audioManager?.PlayLevelUp();
        UpdateAllLabels();
    }

    public void AddCollectedCoin(int amount)
    {
        runCoins += amount;
        walletCoins += amount;
        PlayerPrefs.SetInt("WalletCoins", walletCoins);
        UpdateTopHUD();
    }

    private void ClaimMissionReward(int idx)
    {
        if (missionClaimed[idx]) return;
        missionClaimed[idx] = true;
        PlayerPrefs.SetInt($"MissionClaimed_{idx}", 1);

        if (idx == 0) totalKeys += 2;
        else if (idx == 1) walletCoins += 1000;
        else totalKeys += 3;

        PlayerPrefs.SetInt("TotalKeys", totalKeys);
        PlayerPrefs.SetInt("WalletCoins", walletCoins);
        PlayerPrefs.Save();

        audioManager?.PlayLevelUp();
        UpdateAllLabels();
    }

    private void SelectBiome(int biomeIdx)
    {
        if (bestScore < biomeUnlockScores[biomeIdx]) return;
        audioManager?.PlayClick();
        activeBiomeIndex = biomeIdx;
        ApplyBiomeEverywhere(activeBiomeIndex);
        UpdateAllLabels();
        SwitchState(UIState.MainHub);
    }

    private void SwitchState(UIState newState)
    {
        currentState = newState;

        if (splashPanel != null) splashPanel.SetActive(newState == UIState.SplashIntro);
        if (mainHubPanel != null) mainHubPanel.SetActive(newState == UIState.MainHub);
        if (biomesPanel != null) biomesPanel.SetActive(newState == UIState.BiomesMenu);
        if (wardrobePanel != null) wardrobePanel.SetActive(newState == UIState.HeroWardrobe);
        if (missionsPanel != null) missionsPanel.SetActive(newState == UIState.MissionsMenu);
        if (shopPanel != null) shopPanel.SetActive(newState == UIState.ShopMenu);
        if (hudPanel != null) hudPanel.SetActive(newState == UIState.Gameplay);
        if (levelUpPanel != null && newState != UIState.Gameplay) levelUpPanel.SetActive(false);

        UpdateAllLabels();
    }

    private void Update()
    {
        float t = Time.unscaledTime;

        if (currentState == UIState.SplashIntro)
        {
            if (splashLogoRect != null)
            {
                float scale = 1f + Mathf.Sin(t * 3.2f) * 0.045f;
                splashLogoRect.localScale = new Vector3(scale, scale, 1f);
                splashLogoRect.localRotation = Quaternion.Euler(0f, 0f, 4f + Mathf.Sin(t * 2.1f) * 2.5f);
            }

            if (tapToPlayText != null)
            {
                float alpha = 0.55f + 0.45f * Mathf.Sin(t * 4.5f);
                Color c = tapToPlayText.color;
                c.a = alpha;
                tapToPlayText.color = c;
            }

            AnimateShowcaseCamera(t, true);
            return;
        }

        if (currentState == UIState.MainHub || currentState == UIState.HeroWardrobe ||
            currentState == UIState.BiomesMenu || currentState == UIState.MissionsMenu || currentState == UIState.ShopMenu)
        {
            if (hubLogoRect != null)
            {
                float scale = 1f + Mathf.Sin(t * 2.8f) * 0.03f;
                hubLogoRect.localScale = new Vector3(scale, scale, 1f);
            }

            if (hubTapToRunText != null)
            {
                float pulse = 1f + Mathf.Sin(t * 4.2f) * 0.06f;
                hubTapToRunText.rectTransform.localScale = new Vector3(pulse, pulse, 1f);
            }

            if (currentState == UIState.MainHub && Keyboard.current != null &&
                (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
            {
                OnPlayButtonClicked();
                return;
            }

            AnimateShowcaseCamera(t, false);
            return;
        }

        if (currentState == UIState.Gameplay)
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame)
                    TogglePause();

                if (!isPaused && !isWaitingRevive)
                {
                    if (Keyboard.current.lKey.wasPressedThisFrame) TriggerLevelUp();
                    if (Keyboard.current.kKey.wasPressedThisFrame) OnPlayerDied();
                }
            }

            if (isPaused || isWaitingRevive) return;

            if (invulnerableTimer > 0f)
                invulnerableTimer -= Time.deltaTime;

            // Рахунок йде від 0 і перевіряє пороги 500,000 -> 1,000,000 -> далі по +500,000
            distanceScore += Time.deltaTime * 350f;
            int intScore = Mathf.FloorToInt(distanceScore);
            if (intScore > bestScore)
            {
                bestScore = intScore;
                PlayerPrefs.SetInt("BestScore", bestScore);
            }

            while (distanceScore >= nextLevelScoreTarget)
            {
                prevLevelScoreTarget = nextLevelScoreTarget;
                nextLevelScoreTarget += GetStepScore();
                TriggerLevelUp();
            }

            UpdateTopHUD();
        }
    }

    public void TogglePause()
    {
        if (currentState != UIState.Gameplay || isWaitingRevive) return;
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        audioManager?.PlayClick();

        if (pauseStatsText != null)
            pauseStatsText.text = $"Очки: {Mathf.FloorToInt(distanceScore):N0}  ·  Монети: {runCoins}";

        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        audioManager?.PlayClick();
        if (pausePanel != null) pausePanel.SetActive(false);
    }

    public void RestartRunClean()
    {
        Time.timeScale = 1f;
        isPaused = false;
        isWaitingRevive = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (revivePanel != null) revivePanel.SetActive(false);

        StartRun();
    }

    public void QuitRunToMenu()
    {
        Time.timeScale = 1f;
        isPaused = false;
        isWaitingRevive = false;
        if (pausePanel != null) pausePanel.SetActive(false);
        if (revivePanel != null) revivePanel.SetActive(false);

        PlayerMotor motor = GetPlayerMotor();
        if (motor != null) motor.ResetState();

        if (GameManager.Instance != null)
            GameManager.Instance.Restart();

        SetGameplayActive(false);
        distanceScore = 0f;
        runCoins = 0;
        currentLevel = 1;
        prevLevelScoreTarget = 0f;
        nextLevelScoreTarget = GetStepScore();

        SwitchState(UIState.MainHub);
        if (isMusicEnabled) audioManager?.PlayMenuMusic();
    }

    public void OnPlayerDied()
    {
        if (currentState != UIState.Gameplay || isWaitingRevive) return;
        if (invulnerableTimer > 0f) return;

        audioManager?.PlayGameOver();
        SetGameplayActive(false);

        if (levelUpRoutine != null) { StopCoroutine(levelUpRoutine); levelUpRoutine = null; }
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        if (totalKeys > 0 && revivePanel != null)
        {
            isWaitingRevive = true;
            if (reviveKeysCountText != null)
                reviveKeysCountText.text = $"У тебе є: {totalKeys} ключів";

            if (reviveRoutine != null) StopCoroutine(reviveRoutine);
            reviveRoutine = StartCoroutine(ReviveCountdownRoutine());
        }
        else
        {
            FinishRunAfterDeath();
        }
    }

    private IEnumerator ReviveCountdownRoutine()
    {
        revivePanel.SetActive(true);
        float timer = 5.0f;
        while (timer > 0f)
        {
            if (reviveTimerText != null)
                reviveTimerText.text = $"{Mathf.CeilToInt(timer)}";
            yield return null;
            timer -= Time.unscaledDeltaTime;
        }
        FinishRunAfterDeath();
    }

    public void ReviveWithKey()
    {
        if (!isWaitingRevive || totalKeys <= 0) return;

        if (reviveRoutine != null) { StopCoroutine(reviveRoutine); reviveRoutine = null; }

        totalKeys--;
        PlayerPrefs.SetInt("TotalKeys", totalKeys);
        PlayerPrefs.Save();

        isWaitingRevive = false;
        if (revivePanel != null) revivePanel.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeAfterRevive();

        PlayerMotor motor = GetPlayerMotor();
        if (motor != null) motor.ReviveInPlace();

        ClearObstaclesSafelyWithoutTouchingTrack();
        invulnerableTimer = 3.5f;
        audioManager?.PlayLevelUp();
        SetGameplayActive(true);
        UpdateTopHUD();
    }

    private void ClearObstaclesSafelyWithoutTouchingTrack()
    {
        if (heroSwitcher == null) return;
        Transform playerTr = heroSwitcher.transform;

        GameObject fxObj = new GameObject("ReviveBlastFX");
        fxObj.transform.position = playerTr.position + Vector3.up * 1.2f;
        ParticleSystem ps = fxObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startColor = new Color(1f, 0.85f, 0.1f, 1f);
        main.startSize = 0.45f;
        main.startSpeed = 14f;
        main.startLifetime = 0.6f;
        Destroy(fxObj, 1.2f);

        Collider[] hits = Physics.OverlapSphere(playerTr.position + Vector3.forward * 14f, 28f);
        foreach (var col in hits)
        {
            if (col == null || col.transform.root == playerTr.root) continue;

            string objName = col.gameObject.name.ToLower();
            if (objName.Contains("road") || objName.Contains("ground") || objName.Contains("track") ||
                objName.Contains("floor") || objName.Contains("tile") || objName.Contains("coin"))
                continue;

            if (objName.Contains("obstacle") || objName.Contains("car") || objName.Contains("bus") ||
                objName.Contains("train") || objName.Contains("barrier") || objName.Contains("block") ||
                objName.Contains("cone") || objName.Contains("fence"))
            {
                col.gameObject.SetActive(false);
            }
        }
    }

    public void FinishRunAfterDeath()
    {
        if (reviveRoutine != null) { StopCoroutine(reviveRoutine); reviveRoutine = null; }
        isWaitingRevive = false;
        if (revivePanel != null) revivePanel.SetActive(false);

        QuitRunToMenu();
    }

    public void StartRun()
    {
        distanceScore = 0f;
        runCoins = 0;
        currentLevel = 1;
        prevLevelScoreTarget = 0f;
        nextLevelScoreTarget = GetStepScore(); // 500,000 для переходу на 2-й рівень, 1,000,000 для 3-го
        isPaused = false;
        isWaitingRevive = false;
        invulnerableTimer = 0f;
        Time.timeScale = 1f;

        if (pausePanel != null) pausePanel.SetActive(false);
        if (revivePanel != null) revivePanel.SetActive(false);
        if (levelUpPanel != null) levelUpPanel.SetActive(false);

        SwitchState(UIState.Gameplay);
        SetGameplayActive(true);

        PlayerMotor motor = GetPlayerMotor();
        if (motor != null) motor.ResetState();

        if (GameManager.Instance != null)
            GameManager.Instance.Restart();

        if (mainCam != null && hasSavedCamTransform)
        {
            mainCam.transform.position = defaultCamPos;
            mainCam.transform.rotation = defaultCamRot;
        }

        gameplaySystems?.ResetForNewRun();
        ApplyBiomeEverywhere(activeBiomeIndex);
        UpdateTopHUD();

        if (isMusicEnabled) audioManager?.PlayRunMusic();
    }

    public void TriggerLevelUp()
    {
        currentLevel++;
        AddCollectedCoin(levelBonusCoins);
        audioManager?.PlayLevelUp();

        int newBiomeIdx = (activeBiomeIndex + currentLevel - 1) % biomeNames.Length;
        ApplyBiomeEverywhere(newBiomeIdx);

        if (heroSwitcher != null && heroSwitcher.heroModels.Count > 0)
        {
            unlockedHeroCount = Mathf.Clamp(unlockedHeroCount + 1, 1, heroSwitcher.heroModels.Count);
            PlayerPrefs.SetInt("UnlockedHeroes", unlockedHeroCount);
            PlayerPrefs.Save();

            int nextHeroIdx = unlockedHeroCount - 1;
            heroSwitcher.SwitchHero(nextHeroIdx);

            string biomeName = biomeNames[newBiomeIdx];
            string skinName = heroSwitcher.GetCurrentHeroName();

            if (levelUpSubtitleText != null) levelUpSubtitleText.text = $"Рівень {currentLevel} · {biomeName}";
            if (unlockedHeroNameText != null) unlockedHeroNameText.text = skinName;
            if (unlockedHeroTagText != null) unlockedHeroTagText.text = "новий скін відкрито!";

            if (unlockedHeroIconImage != null)
            {
                if (heroIcons != null && nextHeroIdx >= 0 && nextHeroIdx < heroIcons.Count && heroIcons[nextHeroIdx] != null)
                {
                    unlockedHeroIconImage.sprite = heroIcons[nextHeroIdx];
                    unlockedHeroIconImage.color = Color.white;
                    unlockedHeroIconImage.gameObject.SetActive(true);
                }
                else
                {
                    unlockedHeroIconImage.gameObject.SetActive(false);
                }
            }
        }

        if (levelUpBonusText != null) levelUpBonusText.text = $"+{levelBonusCoins} монет за рівень!";
        UpdateTopHUD();

        if (levelUpRoutine != null) StopCoroutine(levelUpRoutine);
        levelUpRoutine = StartCoroutine(ShowLevelUpPopupRoutine());
    }

    private IEnumerator ShowLevelUpPopupRoutine()
    {
        if (levelUpPanel != null) levelUpPanel.SetActive(true);
        float timer = 1.0f;
        while (timer > 0f)
        {
            if (levelUpButtonText != null) levelUpButtonText.text = $"ДАЛІ ({Mathf.Max(0f, timer):0.0} с)";
            yield return null;
            timer -= Time.deltaTime;
        }
        CloseLevelUpImmediate();
    }

    public void CloseLevelUpImmediate()
    {
        if (levelUpRoutine != null) { StopCoroutine(levelUpRoutine); levelUpRoutine = null; }
        if (levelUpPanel != null) levelUpPanel.SetActive(false);
    }

    private void AnimateShowcaseCamera(float t, bool isSplash)
    {
        if (mainCam == null || heroSwitcher == null) return;
        Transform heroTr = heroSwitcher.transform;
        float orbitAngle = isSplash ? Mathf.Sin(t * 0.9f) * 18f : Mathf.Sin(t * 0.7f) * 10f;
        float dist = isSplash ? 3.6f : 4.2f;
        float height = isSplash ? 1.85f : 2.1f;

        Vector3 offset = Quaternion.Euler(0f, orbitAngle, 0f) * new Vector3(0f, height, -dist);
        Vector3 targetPos = heroTr.position + offset;

        mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, targetPos, Time.unscaledDeltaTime * 4f);
        mainCam.transform.LookAt(heroTr.position + Vector3.up * 1.15f);
    }

    private void UpdateAllLabels()
    {
        if (hubCoinsText != null) hubCoinsText.text = walletCoins.ToString("N0");
        if (hubKeysText != null) hubKeysText.text = totalKeys.ToString();
        if (hubHighScoreText != null) hubHighScoreText.text = bestScore.ToString("N0");
        if (hubBiomeBadgeText != null) hubBiomeBadgeText.text = $"БІОМ: {biomeNames[activeBiomeIndex]}";

        for (int i = 0; i < 3; i++)
        {
            bool unlocked = i == 0 || bestScore >= biomeUnlockScores[i];
            if (biomeLockOverlays != null && i < biomeLockOverlays.Length && biomeLockOverlays[i] != null)
                biomeLockOverlays[i].gameObject.SetActive(!unlocked);
            if (biomeSelectButtons != null && i < biomeSelectButtons.Length && biomeSelectButtons[i] != null)
                biomeSelectButtons[i].interactable = unlocked;
            if (biomeStatusLabels != null && i < biomeStatusLabels.Length && biomeStatusLabels[i] != null)
            {
                if (unlocked)
                    biomeStatusLabels[i].text = activeBiomeIndex == i ? "ОБРАНО ЗАРАЗ" : "ВІДКРИТО · НАТИСНИ";
                else
                    biomeStatusLabels[i].text = $"ПОТРІБНО {biomeUnlockScores[i]:N0} ОЧОК";
            }
        }

        if (heroSwitcher != null)
        {
            int idx = heroSwitcher.activeHeroIndex;
            bool isHeroUnlocked = idx < unlockedHeroCount;
            if (heroCounterText != null) heroCounterText.text = $"ГЕРОЙ {idx + 1} / {heroSwitcher.heroModels.Count}";
            if (heroNameText != null) heroNameText.text = heroSwitcher.GetCurrentHeroName();
            if (heroSubtitleText != null) heroSubtitleText.text = heroSwitcher.GetCurrentHeroSubtitle();
            if (heroLockStatusText != null)
            {
                heroLockStatusText.text = isHeroUnlocked ? "СКІН ВІДКРИТО" : $"ЗАБЛОКОВАНО · ПОТРІБНО {idx * 500000:N0} ОЧОК";
                heroLockStatusText.color = isHeroUnlocked ? new Color(0.2f, 0.95f, 0.4f, 1f) : new Color(1f, 0.35f, 0.35f, 1f);
            }
            if (playFromWardrobeButton != null) playFromWardrobeButton.interactable = isHeroUnlocked;
            if (playFromWardrobeLabel != null) playFromWardrobeLabel.text = isHeroUnlocked ? "БІГТИ!" : "ЗАБЛОКОВАНО";
        }

        if (gameplaySystems != null)
        {
            int[] progress = { gameplaySystems.missionCoinsRun, gameplaySystems.missionJumpsTotal, bestScore };
            int[] targets = { 50, 15, 50000 };
            string[] titles = {
                $"1. Зібрати 50 монет за забіг ({Mathf.Min(progress[0], 50)}/50)",
                $"2. Зробити 15 стрибків ({Mathf.Min(progress[1], 15)}/15)",
                $"3. Набрати 50,000 очок ({Mathf.Min(progress[2], 50000):N0}/50,000)"
            };

            for (int i = 0; i < 3; i++)
            {
                if (missionTitleTexts[i] != null) missionTitleTexts[i].text = titles[i];
                bool done = progress[i] >= targets[i];
                if (missionClaimButtons[i] != null) missionClaimButtons[i].interactable = done && !missionClaimed[i];
                if (missionClaimLabels[i] != null)
                    missionClaimLabels[i].text = missionClaimed[i] ? "ОТРИМАНО" : (done ? "ЗАБРАТИ!" : "В ПРОЦЕСІ");
            }
        }

        if (shopWalletText != null) shopWalletText.text = $"БАЛАНС: {walletCoins:N0} МОНЕТ";
        if (shopKeysOwnedText != null) shopKeysOwnedText.text = $"У ТЕБЕ ЗАРАЗ: {totalKeys} КЛЮЧІВ";
        if (buyKey1Button != null) buyKey1Button.interactable = walletCoins >= 500;
        if (buyKey3Button != null) buyKey3Button.interactable = walletCoins >= 1200;
        if (buyKey10Button != null) buyKey10Button.interactable = walletCoins >= 3500;

        UpdateTopHUD();
    }

    private void UpdateTopHUD()
    {
        if (scoreText != null) scoreText.text = $"{Mathf.FloorToInt(distanceScore):N0}";
        if (coinsText != null) coinsText.text = $"{runCoins}";
        if (keysText != null) keysText.text = $"{totalKeys}";

        if (biomeStatusText != null)
        {
            int curBiome = (activeBiomeIndex + currentLevel - 1) % biomeNames.Length;
            biomeStatusText.text = $"LVL {currentLevel} · {biomeNames[curBiome]}";
        }

        if (hudProgressFill != null)
        {
            float span = Mathf.Max(1f, nextLevelScoreTarget - prevLevelScoreTarget);
            hudProgressFill.fillAmount = Mathf.Clamp01((distanceScore - prevLevelScoreTarget) / span);
        }
    }

    private void SetGameplayActive(bool isActive)
    {
        if (gameplayScriptsToPause != null)
        {
            foreach (var script in gameplayScriptsToPause)
            {
                if (script != null) script.enabled = isActive;
            }
        }

        if (heroSwitcher != null)
        {
            CharacterController cc = heroSwitcher.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = true;

            foreach (var mb in heroSwitcher.GetComponents<MonoBehaviour>())
            {
                if (mb == null || mb == heroSwitcher) continue;
                string n = mb.GetType().Name;
                if (n.Contains("Motor") || n.Contains("Move") || n.Contains("Runner") ||
                    n.Contains("Input") || n.Contains("Anim") || n.Contains("Controller"))
                {
                    mb.enabled = isActive;
                }
            }

            if (isActive && gameplaySystems != null)
            {
                gameplaySystems.RefreshAndForceRunAnimator(true);
            }
        }
    }

    private void DisableLegacyOverlays()
    {
        foreach (var mb in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
        {
            if (mb == null || mb == this) continue;
            string typeName = mb.GetType().Name;
            if (typeName.Contains("Debug") || typeName.Contains("Overlay") ||
                typeName.Contains("HudView") || typeName.Contains("HUD") ||
                typeName.Contains("GameOverView") || typeName.Contains("BiomeProgress") ||
                typeName.Contains("ProgressBar"))
            {
                mb.enabled = false;
            }
        }

        foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (c.gameObject != this.gameObject && c.transform.root != this.transform.root)
                c.gameObject.SetActive(false);
        }
    }
}