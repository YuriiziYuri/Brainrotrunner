#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

public class BrainrotUIBuilder : EditorWindow
{
    [MenuItem("Brainrot/Збудувати гарне мобільне UI-меню")]
    public static void BuildMobileUI()
    {
        Canvas oldCanvas = Object.FindFirstObjectByType<Canvas>();
        if (oldCanvas != null) Undo.DestroyObjectImmediate(oldCanvas.gameObject);

        Sprite pillSprite = CreateRoundedCardSprite(112, 112, 34);
        Sprite pillBorderSprite = CreateRoundedBorderSprite(112, 112, 34, 6);
        Sprite coinSprite = CreateCoinSprite(96);
        Sprite keySprite = CreateKeySprite(96);

        GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Undo.RegisterCreatedObjectUndo(canvasObj, "Create Brainrot Arcade Pop UI");
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject esObj = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(esObj, "Create EventSystem");
        }

        // Нова палітра: Street Arcade / Comic Pop
        Color inkOutline   = new Color(0.10f, 0.08f, 0.16f, 1.00f);
        Color creamCard    = new Color(0.99f, 0.96f, 0.88f, 0.96f);
        Color warmAmber    = new Color(1.00f, 0.78f, 0.14f, 1.00f);
        Color amberShadow  = new Color(0.76f, 0.44f, 0.04f, 1.00f);
        Color coralRed     = new Color(0.98f, 0.30f, 0.34f, 1.00f);
        Color coralShadow  = new Color(0.66f, 0.12f, 0.18f, 1.00f);
        Color limeGreen    = new Color(0.24f, 0.86f, 0.36f, 1.00f);
        Color limeShadow   = new Color(0.08f, 0.54f, 0.18f, 1.00f);
        Color skyBlue      = new Color(0.20f, 0.72f, 0.98f, 1.00f);
        Color skyShadow    = new Color(0.08f, 0.42f, 0.72f, 1.00f);
        Color royalPurple  = new Color(0.58f, 0.34f, 0.98f, 1.00f);
        Color purpleShadow = new Color(0.34f, 0.14f, 0.68f, 1.00f);
        Color darkPlum     = new Color(0.16f, 0.12f, 0.26f, 0.94f);

        // ==================== 1. SPLASH INTRO ====================
        GameObject splashPanel = CreatePanel(canvasObj.transform, "SplashIntroPanel", new Color(0, 0, 0, 0), null, false);
        Button splashBtn = CreateStyledButton(splashPanel.transform, "SplashTapButton", "", 10, new Color(0, 0, 0, 0), Color.clear, null);
        SetAnchors(splashBtn.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        GameObject splashLogoObj = CreateArcadeBannerLogo(splashPanel.transform, "SplashLogo", "BRAINROT", "RUNNER", "ODESA STREET RUN", warmAmber, coralRed, limeGreen, inkOutline, pillSprite, pillBorderSprite);
        RectTransform splashLogoRect = splashLogoObj.GetComponent<RectTransform>();
        SetAnchors(splashLogoRect, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -340), new Vector2(900, 400));

        GameObject tapSticker = Create3DCard(splashPanel.transform, "TapSticker", warmAmber, amberShadow, inkOutline, pillSprite, pillBorderSprite, 14f);
        SetAnchors(tapSticker.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 230), new Vector2(760, 130));
        TMP_Text tapToPlayTxt = CreateComicTMP(tapSticker.transform, "TapToPlayText", "ТОРКАЙСЯ, ЩОБ СТАРТУВАТИ!", 40, inkOutline, Color.white, 0.12f);
        SetAnchors(tapToPlayTxt.rectTransform, Vector2.zero, Vector2.one, new Vector2(0, 5), Vector2.zero);

        // ==================== 2. MAIN HUB (ARCADE POP STYLE) ====================
        GameObject mainHubPanel = CreatePanel(canvasObj.transform, "MainHubPanel", new Color(0, 0, 0, 0), null, false);
        Button hubRunBtn = CreateStyledButton(mainHubPanel.transform, "HubTapToRunButton", "", 10, new Color(0, 0, 0, 0), Color.clear, null);
        SetAnchors(hubRunBtn.GetComponent<RectTransform>(), new Vector2(0.15f, 0.28f), new Vector2(0.85f, 0.68f), Vector2.zero, Vector2.zero);

        // 1-й ряд зверху: стрічка валют (Ключі / Монети / Множник)
        GameObject topCurrencyBar = Create3DCard(mainHubPanel.transform, "TopCurrencyBar", creamCard, new Color(0.78f, 0.72f, 0.60f, 1f), inkOutline, pillSprite, pillBorderSprite, 10f);
        SetAnchors(topCurrencyBar.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -80), new Vector2(980, 98));

        CreateIconImage(topCurrencyBar.transform, "HubKeyIcon", keySprite, new Vector2(0f, 0.5f), new Vector2(75, 4), new Vector2(54, 54));
        TMP_Text hubKeysTxt = CreateComicTMP(topCurrencyBar.transform, "HubKeysText", "3", 38, inkOutline, Color.clear, 0f);
        SetAnchors(hubKeysTxt.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(165, 4), new Vector2(130, 60));

        CreateIconImage(topCurrencyBar.transform, "HubCoinIcon", coinSprite, new Vector2(0.5f, 0.5f), new Vector2(-75, 4), new Vector2(54, 54));
        TMP_Text hubCoinsTxt = CreateComicTMP(topCurrencyBar.transform, "HubCoinsText", "0", 38, inkOutline, Color.clear, 0f);
        SetAnchors(hubCoinsTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(50, 4), new Vector2(240, 60));

        GameObject multBadge = CreatePanel(topCurrencyBar.transform, "MultPill", limeGreen, pillSprite, false);
        SetAnchors(multBadge.GetComponent<RectTransform>(), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-105, 4), new Vector2(145, 64));
        CreatePanel(multBadge.transform, "B", inkOutline, pillBorderSprite, false);
        TMP_Text hubMultTxt = CreateComicTMP(multBadge.transform, "MultiplierText", "x30", 34, Color.white, inkOutline, 0.25f);
        SetAnchors(hubMultTxt.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        // 2-й ряд зверху (НАД Рекордом та Ареною): МУЗИКА: ВКЛ та КРАМНИЦЯ КЛЮЧІВ
        Button musicToggleBtn = Create3DButton(mainHubPanel.transform, "HubMusicToggleButton", "МУЗИКА: ВКЛ", 28, creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, inkOutline, pillSprite, pillBorderSprite, 8f);
        SetAnchors(musicToggleBtn.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280, -195), new Vector2(465, 92));
        TMP_Text musicToggleLbl = musicToggleBtn.GetComponentInChildren<TMP_Text>();

        Button shopBtn = Create3DButton(mainHubPanel.transform, "ShopTabButton", "КРАМНИЦЯ КЛЮЧІВ", 28, coralRed, coralShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 8f);
        SetAnchors(shopBtn.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-280, -195), new Vector2(465, 92));

        // 3-й ряд зверху (ПІД Музикою та Крамницею): РЕКОРД та АРЕНА
        GameObject recordBadge = Create3DCard(mainHubPanel.transform, "RecordBadge", warmAmber, amberShadow, inkOutline, pillSprite, pillBorderSprite, 8f);
        SetAnchors(recordBadge.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(280, -310), new Vector2(465, 102));
        TMP_Text recTitle = CreateComicTMP(recordBadge.transform, "RecTitle", "РЕКОРД", 22, inkOutline, Color.clear, 0f);
        SetAnchors(recTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -22), new Vector2(420, 32));
        TMP_Text hubHighScoreTxt = CreateComicTMP(recordBadge.transform, "HubHighScoreText", "0", 40, Color.white, inkOutline, 0.28f);
        SetAnchors(hubHighScoreTxt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 34), new Vector2(420, 48));

        GameObject biomeBadge = Create3DCard(mainHubPanel.transform, "BiomeBadge", skyBlue, skyShadow, inkOutline, pillSprite, pillBorderSprite, 8f);
        SetAnchors(biomeBadge.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-280, -310), new Vector2(465, 102));
        TMP_Text biomeCaption = CreateComicTMP(biomeBadge.transform, "BiomeCap", "АРЕНА", 22, inkOutline, Color.clear, 0f);
        SetAnchors(biomeCaption.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -22), new Vector2(420, 32));
        TMP_Text hubBiomeTxt = CreateComicTMP(biomeBadge.transform, "HubBiomeBadgeText", "БІОМ: МІСТО ОДЕСА", 28, Color.white, inkOutline, 0.28f);
        SetAnchors(hubBiomeTxt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 34), new Vector2(435, 48));

        // Головна об'ємна кнопка СТАРТ
        Button playMainBtn = Create3DButton(mainHubPanel.transform, "CenterPlayButton", "БІГТИ ЗАРАЗ!", 48, limeGreen, limeShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 16f);
        SetAnchors(playMainBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 335), new Vector2(720, 145));
        TMP_Text hubTapTxt = playMainBtn.GetComponentInChildren<TMP_Text>();

        // Нижні 3 об'ємні картки навігації
        Button missionsBtn = Create3DButton(mainHubPanel.transform, "MissionsTabButton", "МІСІЇ", 32, warmAmber, amberShadow, inkOutline, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(missionsBtn.GetComponent<RectTransform>(), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(205, 145), new Vector2(295, 130));

        Button heroesBtn = Create3DButton(mainHubPanel.transform, "HeroesTabButton", "ГЕРОЇ", 34, royalPurple, purpleShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(heroesBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(320, 140));

        Button biomesBtn = Create3DButton(mainHubPanel.transform, "BiomesTabButton", "БІОМИ", 32, skyBlue, skyShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(biomesBtn.GetComponent<RectTransform>(), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-205, 145), new Vector2(295, 130));

        // ==================== 3. BIOMES MENU ====================
        GameObject biomesPanel = CreatePanel(canvasObj.transform, "BiomesMenuPanel", new Color(0.12f, 0.10f, 0.22f, 0.94f), null, true);
        TMP_Text biomesHeader = CreateComicTMP(biomesPanel.transform, "BiomesHeader", "ОБЕРИ ЛОКАЦІЮ", 58, warmAmber, inkOutline, 0.35f);
        SetAnchors(biomesHeader.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -135), new Vector2(900, 90));

        string[] bTitles = { "1. МІСТО (ОДЕСА)", "2. КРИЖАНА ТУНДРА", "3. ДИКА САВАНА" };
        string[] bReqs = { "ВІДКРИТО · НАТИСНИ", "ПОТРІБНО 500,000 ОЧОК", "ПОТРІБНО 1,000,000 ОЧОК" };
        Color[] bColors = { skyBlue, new Color(0.32f, 0.82f, 0.96f, 1f), warmAmber };
        Color[] bShadows = { skyShadow, new Color(0.14f, 0.50f, 0.68f, 1f), amberShadow };

        Button[] biomeBtns = new Button[3];
        Image[] biomeLocks = new Image[3];
        TMP_Text[] biomeLabels = new TMP_Text[3];

        for (int i = 0; i < 3; i++)
        {
            float yPos = -410f - (i * 370f);
            GameObject card = Create3DCard(biomesPanel.transform, $"BiomeCard_{i}", bColors[i], bShadows[i], inkOutline, pillSprite, pillBorderSprite, 14f);
            SetAnchors(card.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, yPos), new Vector2(900, 310));
            biomeBtns[i] = card.AddComponent<Button>();

            TMP_Text bTitle = CreateComicTMP(card.transform, "Title", bTitles[i], 46, Color.white, inkOutline, 0.3f);
            SetAnchors(bTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -76), new Vector2(820, 70));

            TMP_Text bStatus = CreateComicTMP(card.transform, "Status", bReqs[i], 32, i == 0 ? creamCard : inkOutline, i == 0 ? inkOutline : Color.clear, i == 0 ? 0.25f : 0f);
            SetAnchors(bStatus.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 82), new Vector2(820, 60));
            biomeLabels[i] = bStatus;

            GameObject lockOv = CreatePanel(card.transform, "LockOverlay", new Color(0.08f, 0.06f, 0.14f, 0.78f), pillSprite, false);
            biomeLocks[i] = lockOv.GetComponent<Image>();
            TMP_Text lockIconTxt = CreateComicTMP(lockOv.transform, "LockBadge", "ЗАБЛОКОВАНО", 42, coralRed, Color.white, 0.25f);
            SetAnchors(lockIconTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 12), new Vector2(520, 70));
            if (i == 0) lockOv.SetActive(false);
        }

        Button biomesBackBtn = Create3DButton(biomesPanel.transform, "BiomesBackButton", "НАЗАД У МЕНЮ", 36, creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(biomesBackBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(640, 125));

        // ==================== 4. HERO WARDROBE ====================
        GameObject wardrobePanel = CreatePanel(canvasObj.transform, "WardrobePanel", new Color(0, 0, 0, 0), null, false);
        GameObject heroCard = Create3DCard(wardrobePanel.transform, "HeroInfoCard", creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, pillSprite, pillBorderSprite, 14f);
        SetAnchors(heroCard.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -235), new Vector2(880, 275));

        TMP_Text counterText = CreateComicTMP(heroCard.transform, "HeroCounterText", "ГЕРОЙ 1 / 10", 28, royalPurple, Color.clear, 0f);
        SetAnchors(counterText.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -34), new Vector2(720, 38));
        TMP_Text nameText = CreateComicTMP(heroCard.transform, "HeroNameText", "Strawberry Man", 54, inkOutline, Color.clear, 0f);
        SetAnchors(nameText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 20), new Vector2(820, 68));
        TMP_Text subText = CreateComicTMP(heroCard.transform, "HeroSubtitleText", "Класичний бігун · Місто Одеса", 28, new Color(0.32f, 0.30f, 0.42f, 1f), Color.clear, 0f);
        SetAnchors(subText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -32), new Vector2(780, 42));
        TMP_Text heroLockTxt = CreateComicTMP(heroCard.transform, "HeroLockStatusText", "СКІН ВІДКРИТО", 30, limeShadow, Color.clear, 0f);
        SetAnchors(heroLockTxt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 38), new Vector2(800, 44));

        Button prevBtn = Create3DButton(wardrobePanel.transform, "PrevButton", "<", 48, warmAmber, amberShadow, inkOutline, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(prevBtn.GetComponent<RectTransform>(), new Vector2(0f, 0.46f), new Vector2(0f, 0.46f), new Vector2(115, 0), new Vector2(145, 145));
        Button nextBtn = Create3DButton(wardrobePanel.transform, "NextButton", ">", 48, warmAmber, amberShadow, inkOutline, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(nextBtn.GetComponent<RectTransform>(), new Vector2(1f, 0.46f), new Vector2(1f, 0.46f), new Vector2(-115, 0), new Vector2(145, 145));

        Button playFromWardrobeBtn = Create3DButton(wardrobePanel.transform, "PlayWardrobeButton", "БІГТИ!", 44, limeGreen, limeShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 14f);
        SetAnchors(playFromWardrobeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(95, 160), new Vector2(540, 130));
        TMP_Text playWardrobeLbl = playFromWardrobeBtn.GetComponentInChildren<TMP_Text>();

        Button backBtn = Create3DButton(wardrobePanel.transform, "BackButton", "←", 44, creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, inkOutline, pillSprite, pillBorderSprite, 14f);
        SetAnchors(backBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-295, 160), new Vector2(170, 130));

        // ==================== 5. MISSIONS MENU ====================
        GameObject missionsPanel = CreatePanel(canvasObj.transform, "MissionsMenuPanel", new Color(0.12f, 0.10f, 0.22f, 0.94f), null, true);
        TMP_Text mHeader = CreateComicTMP(missionsPanel.transform, "MissionsHeader", "ЩОДЕННІ МІСІЇ", 58, warmAmber, inkOutline, 0.35f);
        SetAnchors(mHeader.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -140), new Vector2(880, 90));

        TMP_Text[] mTitles = new TMP_Text[3];
        Button[] mBtns = new Button[3];
        TMP_Text[] mLbls = new TMP_Text[3];

        for (int i = 0; i < 3; i++)
        {
            float y = -410f - (i * 340f);
            GameObject mCard = Create3DCard(missionsPanel.transform, $"MissionCard_{i}", creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, pillSprite, pillBorderSprite, 12f);
            SetAnchors(mCard.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, y), new Vector2(900, 290));

            mTitles[i] = CreateComicTMP(mCard.transform, "Title", $"Місія {i + 1}", 34, inkOutline, Color.clear, 0f);
            SetAnchors(mTitles[i].rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -68), new Vector2(820, 75));

            mBtns[i] = Create3DButton(mCard.transform, "ClaimBtn", "В ПРОЦЕСІ", 32, limeGreen, limeShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 10f);
            SetAnchors(mBtns[i].GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 78), new Vector2(460, 96));
            mLbls[i] = mBtns[i].GetComponentInChildren<TMP_Text>();
        }

        Button missionsBackBtn = Create3DButton(missionsPanel.transform, "MissionsBackButton", "НАЗАД У МЕНЮ", 36, creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(missionsBackBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(640, 125));

        // ==================== 6. SHOP MENU ====================
        GameObject shopPanel = CreatePanel(canvasObj.transform, "ShopMenuPanel", new Color(0.12f, 0.10f, 0.22f, 0.94f), null, true);
        TMP_Text sHeader = CreateComicTMP(shopPanel.transform, "ShopHeader", "КРАМНИЦЯ КЛЮЧІВ", 58, warmAmber, inkOutline, 0.35f);
        SetAnchors(sHeader.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -140), new Vector2(880, 85));

        TMP_Text sWallet = CreateComicTMP(shopPanel.transform, "ShopWalletText", "БАЛАНС: 0 МОНЕТ", 38, warmAmber, inkOutline, 0.25f);
        SetAnchors(sWallet.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -245), new Vector2(840, 58));

        TMP_Text sKeysOwned = CreateComicTMP(shopPanel.transform, "ShopKeysOwnedText", "У ТЕБЕ ЗАРАЗ: 3 КЛЮЧІВ", 34, skyBlue, inkOutline, 0.25f);
        SetAnchors(sKeysOwned.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -310), new Vector2(840, 52));

        Button buyKey1Btn = Create3DButton(shopPanel.transform, "BuyKey1Btn", "+1 КЛЮЧ  —  500 МОНЕТ", 36, skyBlue, skyShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(buyKey1Btn.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -490), new Vector2(880, 175));

        Button buyKey3Btn = Create3DButton(shopPanel.transform, "BuyKey3Btn", "+3 КЛЮЧІ  —  1,200 МОНЕТ", 36, limeGreen, limeShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(buyKey3Btn.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -705), new Vector2(880, 175));

        Button buyKey10Btn = Create3DButton(shopPanel.transform, "BuyKey10Btn", "+10 КЛЮЧІВ  —  3,500 МОНЕТ", 36, coralRed, coralShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(buyKey10Btn.GetComponent<RectTransform>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -920), new Vector2(880, 175));

        Button shopBackBtn = Create3DButton(shopPanel.transform, "ShopBackButton", "НАЗАД У МЕНЮ", 36, creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(shopBackBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 150), new Vector2(640, 125));

        // ==================== 7. ІГРОВИЙ HUD (STREET POP) ====================
        GameObject hudPanel = CreatePanel(canvasObj.transform, "GameplayHUD", new Color(0, 0, 0, 0), null, false);

        Button pauseBtn = Create3DButton(hudPanel.transform, "PauseButton", "||", 38, warmAmber, amberShadow, inkOutline, inkOutline, pillSprite, pillBorderSprite, 8f);
        SetAnchors(pauseBtn.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(95, -105), new Vector2(110, 110));

        GameObject biomePill = Create3DCard(hudPanel.transform, "BiomePill", creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, pillSprite, pillBorderSprite, 8f);
        SetAnchors(biomePill.GetComponent<RectTransform>(), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(375, -105), new Vector2(410, 110));
        TMP_Text biomeTxt = CreateComicTMP(biomePill.transform, "BiomeStatusText", "LVL 1 · МІСТО ОДЕСА", 24, inkOutline, Color.clear, 0f);
        SetAnchors(biomeTxt.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -32), new Vector2(370, 40));

        GameObject progBg = CreatePanel(biomePill.transform, "ProgressBg", new Color(0.84f, 0.80f, 0.72f, 1f), pillSprite, false);
        SetAnchors(progBg.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 30), new Vector2(340, 24));
        CreatePanel(progBg.transform, "B", inkOutline, pillBorderSprite, false);
        GameObject progFillObj = CreatePanel(progBg.transform, "ProgressFill", limeGreen, pillSprite, false);
        Image progFillImg = progFillObj.GetComponent<Image>();
        progFillImg.type = Image.Type.Filled;
        progFillImg.fillMethod = Image.FillMethod.Horizontal;
        progFillImg.fillAmount = 0f;

        GameObject scorePill = Create3DCard(hudPanel.transform, "ScorePill", darkPlum, inkOutline, inkOutline, pillSprite, pillBorderSprite, 8f);
        SetAnchors(scorePill.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-210, -90), new Vector2(350, 84));
        TMP_Text scoreTxt = CreateComicTMP(scorePill.transform, "ScoreText", "0", 42, Color.white, inkOutline, 0.25f);
        SetAnchors(scoreTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 3), new Vector2(310, 60));

        GameObject coinPill = Create3DCard(hudPanel.transform, "CoinPill", warmAmber, amberShadow, inkOutline, pillSprite, pillBorderSprite, 8f);
        SetAnchors(coinPill.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-210, -186), new Vector2(350, 76));
        CreateIconImage(coinPill.transform, "HudCoinIcon", coinSprite, new Vector2(0f, 0.5f), new Vector2(46, 3), new Vector2(48, 48));
        TMP_Text coinTxt = CreateComicTMP(coinPill.transform, "CoinsText", "0", 36, inkOutline, Color.clear, 0f);
        SetAnchors(coinTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(24, 3), new Vector2(230, 54));

        GameObject keyPill = Create3DCard(hudPanel.transform, "KeyPill", skyBlue, skyShadow, inkOutline, pillSprite, pillBorderSprite, 8f);
        SetAnchors(keyPill.GetComponent<RectTransform>(), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-145, -274), new Vector2(220, 68));
        CreateIconImage(keyPill.transform, "HudKeyIcon", keySprite, new Vector2(0f, 0.5f), new Vector2(42, 3), new Vector2(42, 42));
        TMP_Text keyTxt = CreateComicTMP(keyPill.transform, "KeysText", "3", 32, Color.white, inkOutline, 0.25f);
        SetAnchors(keyTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(22, 3), new Vector2(130, 48));

        // ==================== 8. ПАУЗА / ВОСКРЕСІННЯ / LEVEL UP ====================
        GameObject pausePanel = CreatePanel(canvasObj.transform, "PausePanel", new Color(0.08f, 0.06f, 0.16f, 0.88f), null, true);
        GameObject pauseCard = Create3DCard(pausePanel.transform, "PauseCard", creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, pillSprite, pillBorderSprite, 16f);
        SetAnchors(pauseCard.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840, 960));

        TMP_Text pauseTitle = CreateComicTMP(pauseCard.transform, "PauseTitle", "ПАУЗА", 68, warmAmber, inkOutline, 0.35f);
        SetAnchors(pauseTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -110), new Vector2(700, 90));
        TMP_Text pauseStats = CreateComicTMP(pauseCard.transform, "PauseStatsText", "Очки: 0  ·  Монети: 0", 34, inkOutline, Color.clear, 0f);
        SetAnchors(pauseStats.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -215), new Vector2(740, 60));

        Button resumeBtn = Create3DButton(pauseCard.transform, "ResumeButton", "ПРОДОВЖИТИ", 36, limeGreen, limeShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(resumeBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 110), new Vector2(640, 120));
        Button restartBtn = Create3DButton(pauseCard.transform, "RestartButton", "ЗАНОВО", 36, warmAmber, amberShadow, inkOutline, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(restartBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -35), new Vector2(640, 120));
        Button audioBtn = Create3DButton(pauseCard.transform, "AudioToggleButton", "МУЗИКА: УВІМКНЕНО", 32, skyBlue, skyShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(audioBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -180), new Vector2(640, 110));
        TMP_Text audioLbl = audioBtn.GetComponentInChildren<TMP_Text>();
        Button quitBtn = Create3DButton(pauseCard.transform, "QuitMenuButton", "ВИЙТИ В МЕНЮ", 36, coralRed, coralShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(quitBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 130), new Vector2(640, 120));

        GameObject revivePanel = CreatePanel(canvasObj.transform, "RevivePanel", new Color(0.08f, 0.06f, 0.16f, 0.88f), null, true);
        GameObject reviveCard = Create3DCard(revivePanel.transform, "ReviveCard", creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, pillSprite, pillBorderSprite, 16f);
        SetAnchors(reviveCard.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840, 860));

        TMP_Text revTitle = CreateComicTMP(reviveCard.transform, "ReviveTitle", "ПРОДОВЖИТИ?", 66, warmAmber, inkOutline, 0.35f);
        SetAnchors(revTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -110), new Vector2(720, 90));
        TMP_Text revTimer = CreateComicTMP(reviveCard.transform, "ReviveTimerText", "5", 110, coralRed, inkOutline, 0.35f);
        SetAnchors(revTimer.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(300, 140));
        TMP_Text revKeysTxt = CreateComicTMP(reviveCard.transform, "ReviveKeysCountText", "У тебе є: 3 ключі", 34, inkOutline, Color.clear, 0f);
        SetAnchors(revKeysTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -35), new Vector2(680, 56));

        Button revKeyBtn = Create3DButton(reviveCard.transform, "ReviveKeyButton", "ВОСКРЕСНУТИ (1 КЛЮЧ)", 34, limeGreen, limeShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(revKeyBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 240), new Vector2(680, 125));
        Button declineBtn = Create3DButton(reviveCard.transform, "DeclineReviveButton", "ЗАВЕРШИТИ ЗАБІГ", 32, coralRed, coralShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(declineBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 100), new Vector2(540, 100));

        GameObject levelUpPanel = CreatePanel(canvasObj.transform, "LevelUpPanel", new Color(0.08f, 0.06f, 0.16f, 0.85f), null, true);
        GameObject modalCard = Create3DCard(levelUpPanel.transform, "LevelUpCard", creamCard, new Color(0.75f, 0.70f, 0.60f, 1f), inkOutline, pillSprite, pillBorderSprite, 16f);
        SetAnchors(modalCard.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(840, 830));

        TMP_Text lvlUpTitle = CreateComicTMP(modalCard.transform, "LevelUpTitle", "LEVEL UP!", 68, warmAmber, inkOutline, 0.35f);
        SetAnchors(lvlUpTitle.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -105), new Vector2(720, 85));
        TMP_Text lvlUpSub = CreateComicTMP(modalCard.transform, "LevelUpSubtitle", "Рівень 2 · ТУНДРА", 34, inkOutline, Color.clear, 0f);
        SetAnchors(lvlUpSub.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -180), new Vector2(720, 50));

        GameObject iconBadge = CreatePanel(modalCard.transform, "HeroIconBadge", skyBlue, pillSprite, false);
        SetAnchors(iconBadge.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 88), new Vector2(220, 220));
        CreatePanel(iconBadge.transform, "B", inkOutline, pillBorderSprite, false);

        GameObject iconObj = new GameObject("UnlockedHeroIcon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        iconObj.transform.SetParent(iconBadge.transform, false);
        Image heroIconImg = iconObj.GetComponent<Image>();
        heroIconImg.preserveAspect = true;
        heroIconImg.raycastTarget = false;
        SetAnchors(heroIconImg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(185, 185));

        TMP_Text unlockedNameTxt = CreateComicTMP(modalCard.transform, "UnlockedHeroName", "Banana Man", 44, inkOutline, Color.clear, 0f);
        SetAnchors(unlockedNameTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -52), new Vector2(720, 60));
        TMP_Text unlockedTagTxt = CreateComicTMP(modalCard.transform, "UnlockedHeroTag", "НОВИЙ СКІН ВІДКРИТО!", 28, royalPurple, Color.clear, 0f);
        SetAnchors(unlockedTagTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -104), new Vector2(720, 44));

        Button nextLevelBtn = Create3DButton(modalCard.transform, "LevelUpNextButton", "ДАЛІ (1.0 с)", 36, limeGreen, limeShadow, Color.white, inkOutline, pillSprite, pillBorderSprite, 12f);
        SetAnchors(nextLevelBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 155), new Vector2(640, 115));
        TMP_Text nextLevelBtnLabel = nextLevelBtn.GetComponentInChildren<TMP_Text>();

        TMP_Text bonusTxt = CreateComicTMP(modalCard.transform, "LevelUpBonusText", "+500 монет за рівень!", 28, inkOutline, Color.clear, 0f);
        SetAnchors(bonusTxt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 65), new Vector2(740, 48));

        // ==================== ПІДКЛЮЧЕННЯ ====================
        HeroSelectUI ui = canvasObj.AddComponent<HeroSelectUI>();
        HeroSwitcher switcher = Object.FindFirstObjectByType<HeroSwitcher>();
        if (switcher != null && switcher.heroModels != null)
            ui.heroIcons = GenerateHeroPreviewSprites(switcher.heroModels);

        ui.heroSwitcher = switcher;
        ui.splashPanel = splashPanel;
        ui.mainHubPanel = mainHubPanel;
        ui.biomesPanel = biomesPanel;
        ui.wardrobePanel = wardrobePanel;
        ui.missionsPanel = missionsPanel;
        ui.shopPanel = shopPanel;
        ui.hudPanel = hudPanel;
        ui.pausePanel = pausePanel;
        ui.revivePanel = revivePanel;
        ui.levelUpPanel = levelUpPanel;

        ui.splashLogoRect = splashLogoRect;
        ui.tapToPlayText = tapToPlayTxt;
        ui.hubLogoRect = null;
        ui.hubTapToRunText = hubTapTxt;

        ui.splashFullScreenButton = splashBtn;
        ui.hubTapToRunButton = hubRunBtn;
        ui.centerPlayButton = playMainBtn;
        ui.biomesTabButton = biomesBtn;
        ui.heroesTabButton = heroesBtn;
        ui.missionsTabButton = missionsBtn;
        ui.shopTabButton = shopBtn;
        ui.hubMusicToggleButton = musicToggleBtn;
        ui.hubMusicToggleLabel = musicToggleLbl;
        ui.biomesBackButton = biomesBackBtn;
        ui.wardrobeBackButton = backBtn;
        ui.missionsBackButton = missionsBackBtn;
        ui.shopBackButton = shopBackBtn;

        ui.biomeSelectButtons = biomeBtns;
        ui.biomeLockOverlays = biomeLocks;
        ui.biomeStatusLabels = biomeLabels;

        ui.missionTitleTexts = mTitles;
        ui.missionClaimButtons = mBtns;
        ui.missionClaimLabels = mLbls;

        ui.shopWalletText = sWallet;
        ui.shopKeysOwnedText = sKeysOwned;
        ui.buyKey1Button = buyKey1Btn;
        ui.buyKey3Button = buyKey3Btn;
        ui.buyKey10Button = buyKey10Btn;

        ui.prevButton = prevBtn;
        ui.nextButton = nextBtn;
        ui.playFromWardrobeButton = playFromWardrobeBtn;
        ui.playFromWardrobeLabel = playWardrobeLbl;
        ui.heroCounterText = counterText;
        ui.heroNameText = nameText;
        ui.heroSubtitleText = subText;
        ui.heroLockStatusText = heroLockTxt;

        ui.hubCoinsText = hubCoinsTxt;
        ui.hubKeysText = hubKeysTxt;
        ui.hubHighScoreText = hubHighScoreTxt;
        ui.hubBiomeBadgeText = hubBiomeTxt;

        ui.pauseButton = pauseBtn;
        ui.scoreText = scoreTxt;
        ui.coinsText = coinTxt;
        ui.keysText = keyTxt;
        ui.biomeStatusText = biomeTxt;
        ui.hudProgressFill = progFillImg;

        ui.pauseStatsText = pauseStats;
        ui.resumeButton = resumeBtn;
        ui.restartButton = restartBtn;
        ui.quitToMenuButton = quitBtn;
        ui.toggleAudioButton = audioBtn;
        ui.toggleAudioLabel = audioLbl;

        ui.reviveKeysCountText = revKeysTxt;
        ui.reviveTimerText = revTimer;
        ui.reviveWithKeyButton = revKeyBtn;
        ui.declineReviveButton = declineBtn;

        ui.levelUpSubtitleText = lvlUpSub;
        ui.unlockedHeroIconImage = heroIconImg;
        ui.unlockedHeroNameText = unlockedNameTxt;
        ui.unlockedHeroTagText = unlockedTagTxt;
        ui.levelUpNextButton = nextLevelBtn;
        ui.levelUpButtonText = nextLevelBtnLabel;
        ui.levelUpBonusText = bonusTxt;
        ui.autoCloseSeconds = 1.0f;

        if (switcher != null)
        {
            MonoBehaviour motor = switcher.GetComponent("PlayerMotor") as MonoBehaviour;
            if (motor != null) ui.gameplayScriptsToPause = new MonoBehaviour[] { motor };
        }

        Selection.activeGameObject = canvasObj;
        Debug.Log("[BrainrotUIBuilder] Оновлене UI-меню створено!");
    }

    private static GameObject Create3DCard(Transform parent, string name, Color faceColor, Color shadowColor, Color borderColor, Sprite fillSprite, Sprite borderSprite, float depth)
    {
        GameObject root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(parent, false);

        GameObject shadow = CreatePanel(root.transform, "Shadow", shadowColor, fillSprite, false);
        RectTransform srt = shadow.GetComponent<RectTransform>();
        srt.offsetMin = new Vector2(0, -depth);
        srt.offsetMax = new Vector2(0, -depth);
        CreatePanel(shadow.transform, "ShadowBorder", borderColor, borderSprite, false);

        GameObject face = CreatePanel(root.transform, "Face", faceColor, fillSprite, false);
        CreatePanel(face.transform, "FaceBorder", borderColor, borderSprite, false);

        return root;
    }

    private static Button Create3DButton(Transform parent, string name, string label, int fontSize, Color faceColor, Color shadowColor, Color txtColor, Color borderColor, Sprite fillSprite, Sprite borderSprite, float depth)
    {
        GameObject root = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        root.transform.SetParent(parent, false);
        Image hitImg = root.GetComponent<Image>();
        hitImg.color = new Color(0, 0, 0, 0);
        hitImg.raycastTarget = true;

        GameObject shadow = CreatePanel(root.transform, "Shadow", shadowColor, fillSprite, false);
        RectTransform srt = shadow.GetComponent<RectTransform>();
        srt.offsetMin = new Vector2(0, -depth);
        srt.offsetMax = new Vector2(0, -depth);
        CreatePanel(shadow.transform, "ShadowBorder", borderColor, borderSprite, false);

        GameObject face = CreatePanel(root.transform, "Face", faceColor, fillSprite, false);
        CreatePanel(face.transform, "FaceBorder", borderColor, borderSprite, false);

        TMP_Text txt = CreateComicTMP(face.transform, "Label", label, fontSize, txtColor, borderColor, txtColor == borderColor ? 0f : 0.25f);
        SetAnchors(txt.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

        return root.GetComponent<Button>();
    }

    private static Sprite CreateRoundedCardSprite(int width, int height, int radius)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = Mathf.Max(0, Mathf.Max(radius - x, x - (width - 1 - radius)));
                float dy = Mathf.Max(0, Mathf.Max(radius - y, y - (height - 1 - radius)));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(radius - dist + 0.5f);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    private static Sprite CreateRoundedBorderSprite(int width, int height, int radius, int thickness)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = Mathf.Max(0, Mathf.Max(radius - x, x - (width - 1 - radius)));
                float dy = Mathf.Max(0, Mathf.Max(radius - y, y - (height - 1 - radius)));
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                bool insideOuter = dist <= radius + 0.5f;
                bool outsideInner = x < thickness || x >= width - thickness || y < thickness || y >= height - thickness || dist >= (radius - thickness);
                tex.SetPixel(x, y, (insideOuter && outsideInner) ? Color.white : Color.clear);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
    }

    private static void CreateIconImage(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 pos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.sprite = sprite;
        img.preserveAspect = true;
        img.raycastTarget = false;
        SetAnchors(img.rectTransform, anchor, anchor, pos, size);
    }

    private static Sprite CreateCoinSprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size * 0.5f;
        float radius = size * 0.44f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist > radius) tex.SetPixel(x, y, Color.clear);
                else if (dist > radius - 6f) tex.SetPixel(x, y, new Color(0.10f, 0.08f, 0.16f, 1f));
                else
                {
                    float shine = Mathf.Clamp01(1f - (Vector2.Distance(new Vector2(x, y), new Vector2(center - 10, center + 10)) / radius));
                    tex.SetPixel(x, y, Color.Lerp(new Color(1f, 0.78f, 0.10f, 1f), new Color(1f, 0.96f, 0.65f, 1f), shine * 0.65f));
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private static Sprite CreateKeySprite(int size)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color keyCyan = new Color(0.20f, 0.85f, 1.0f, 1f);
        Color keyBorder = new Color(0.10f, 0.08f, 0.16f, 1f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                tex.SetPixel(x, y, Color.clear);
                float distRing = Vector2.Distance(new Vector2(x, y), new Vector2(size * 0.30f, size * 0.50f));
                if (distRing <= size * 0.23f && distRing >= size * 0.09f)
                    tex.SetPixel(x, y, distRing > size * 0.18f ? keyBorder : keyCyan);
                if (x >= size * 0.48f && x <= size * 0.86f && Mathf.Abs(y - size * 0.50f) <= size * 0.06f)
                    tex.SetPixel(x, y, keyCyan);
                if ((x >= size * 0.68f && x <= size * 0.74f || x >= size * 0.79f && x <= size * 0.85f) &&
                    y >= size * 0.32f && y <= size * 0.46f)
                    tex.SetPixel(x, y, keyCyan);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private static GameObject CreateArcadeBannerLogo(Transform parent, string name, string topWord, string bottomWord, string badgeWord, Color yellow, Color coral, Color green, Color outline, Sprite fillSprite, Sprite borderSprite)
    {
        GameObject root = new GameObject(name, typeof(RectTransform));
        root.transform.SetParent(parent, false);

        TMP_Text line1 = CreateComicTMP(root.transform, "TopWord", topWord, 120, yellow, outline, 0.38f);
        SetAnchors(line1.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0, -68), new Vector2(880, 140));
        line1.enableVertexGradient = true;
        line1.colorGradient = new VertexGradient(Color.white, Color.white, yellow, yellow);

        TMP_Text line2 = CreateComicTMP(root.transform, "BottomWord", bottomWord, 126, coral, outline, 0.38f);
        SetAnchors(line2.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -8), new Vector2(880, 145));
        line2.enableVertexGradient = true;
        line2.colorGradient = new VertexGradient(yellow, yellow, coral, coral);

        GameObject ribbon = CreatePanel(root.transform, "Ribbon", green, fillSprite, false);
        SetAnchors(ribbon.GetComponent<RectTransform>(), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0, 48), new Vector2(620, 66));
        CreatePanel(ribbon.transform, "B", outline, borderSprite, false);

        TMP_Text subBadge = CreateComicTMP(ribbon.transform, "BadgeWord", badgeWord, 34, Color.white, outline, 0.28f);
        SetAnchors(subBadge.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return root;
    }

    private static TMP_Text CreateComicTMP(Transform parent, string name, string content, int size, Color color, Color outlineColor, float outlineWidth)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        if (outlineWidth > 0f && outlineColor.a > 0f)
        {
            tmp.outlineWidth = outlineWidth;
            tmp.outlineColor = outlineColor;
        }
        return tmp;
    }

    private static List<Sprite> GenerateHeroPreviewSprites(List<GameObject> models)
    {
        List<Sprite> sprites = new List<Sprite>();
        string folderPath = "Assets/brainrotunner/UI_Previews";
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        for (int i = 0; i < models.Count; i++)
        {
            GameObject model = models[i];
            if (model == null) { sprites.Add(null); continue; }
            Texture2D preview = AssetPreview.GetAssetPreview(model);
            int attempts = 0;
            while (preview == null && AssetPreview.IsLoadingAssetPreview(model.GetInstanceID()) && attempts < 60)
            {
                System.Threading.Thread.Sleep(30);
                preview = AssetPreview.GetAssetPreview(model);
                attempts++;
            }
            if (preview == null) preview = AssetPreview.GetMiniThumbnail(model);
            if (preview != null)
            {
                Texture2D cleanTex = new Texture2D(preview.width, preview.height, TextureFormat.RGBA32, false);
                Color[] pixels = preview.GetPixels();
                Color bgSample = pixels[0];
                bool hasTransparentBg = bgSample.a < 0.1f;

                for (int p = 0; p < pixels.Length; p++)
                {
                    Color c = pixels[p];
                    if (!hasTransparentBg)
                    {
                        float diff = Mathf.Abs(c.r - bgSample.r) + Mathf.Abs(c.g - bgSample.g) + Mathf.Abs(c.b - bgSample.b);
                        if (diff < 0.06f) c.a = 0f;
                    }
                    pixels[p] = c;
                }
                cleanTex.SetPixels(pixels);
                cleanTex.Apply();

                string filePath = $"{folderPath}/HeroPreview_{i:00}.png";
                File.WriteAllBytes(filePath, cleanTex.EncodeToPNG());
                AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceUpdate);
                TextureImporter importer = AssetImporter.GetAtPath(filePath) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.alphaIsTransparency = true;
                    importer.SaveAndReimport();
                }
                sprites.Add(AssetDatabase.LoadAssetAtPath<Sprite>(filePath));
            }
            else sprites.Add(null);
        }
        AssetDatabase.Refresh();
        return sprites;
    }

    private static GameObject CreatePanel(Transform parent, string name, Color color, Sprite sprite, bool raycast)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        Image img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = raycast;
        if (sprite != null) { img.sprite = sprite; img.type = Image.Type.Sliced; }
        return go;
    }

    private static Button CreateStyledButton(Transform parent, string name, string label, int fontSize, Color bgColor, Color txtColor, Sprite sprite)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        Image img = go.GetComponent<Image>();
        img.color = bgColor;
        img.sprite = sprite;
        img.raycastTarget = true;
        if (sprite != null) img.type = Image.Type.Sliced;
        TMP_Text txt = CreateTMP(go.transform, "Label", label, fontSize, FontStyles.Bold, txtColor);
        SetAnchors(txt.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return go.GetComponent<Button>();
    }

    private static TMP_Text CreateTMP(Transform parent, string name, string content, int size, FontStyles style, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = content;
        tmp.fontSize = size;
        tmp.fontStyle = style;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 anchoredPos, Vector2 sizeDelta)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.anchoredPosition = anchoredPos;
        if (sizeDelta != Vector2.zero) rt.sizeDelta = sizeDelta;
    }
}
#endif