using UnityEngine;
using BrainrotRunner.Player;

public class BrainrotGameplaySystems : MonoBehaviour
{
    [Header("Посилання (підтягуються автоматично)")]
    public HeroSelectUI ui;
    public PlayerMotor motor;
    public HeroSwitcher heroSwitcher;

    [Header("Прогрес місій")]
    public int missionCoinsRun = 0;      // Ціль: 50 монет за забіг
    public int missionJumpsTotal = 0;    // Ціль: 15 стрибків

    private bool wasGroundedLastFrame = true;
    private Animator cachedActiveAnimator;
    private Animation cachedLegacyAnimation;
    private int lastHeroIndex = -1;
    private float runBounceTimer = 0f;

    private void Awake()
    {
        missionJumpsTotal = PlayerPrefs.GetInt("Mission_Jumps", 0);
    }

    private void Start()
    {
        if (ui == null) ui = FindFirstObjectByType<HeroSelectUI>();
        if (heroSwitcher == null) heroSwitcher = FindFirstObjectByType<HeroSwitcher>();
        if (motor == null && heroSwitcher != null) motor = heroSwitcher.GetComponent<PlayerMotor>();
        if (motor == null) motor = FindFirstObjectByType<PlayerMotor>();
    }

    public void ResetForNewRun()
    {
        missionCoinsRun = 0;
        wasGroundedLastFrame = true;
        lastHeroIndex = -1;
        runBounceTimer = 0f;
        RefreshAndForceRunAnimator(true);
    }

    private void Update()
    {
        if (ui == null || motor == null) return;

        bool isRunning = ui.IsGameplayRunning();
        SyncHeroAnimator(isRunning);

        if (!isRunning) return;

        Transform playerTr = motor.transform;

        // Відстежуємо стрибки для місії
        if (wasGroundedLastFrame && !motor.IsGrounded)
        {
            missionJumpsTotal++;
            PlayerPrefs.SetInt("Mission_Jumps", missionJumpsTotal);
        }
        wasGroundedLastFrame = motor.IsGrounded;

        // Збір наявних 3D-монет на трасі
        ProcessCoinsOnTrack(playerTr);
    }

    public void RefreshAndForceRunAnimator(bool isRunning)
    {
        if (heroSwitcher == null) return;

        Animator foundAnim = null;
        Animation foundLegacy = null;

        if (heroSwitcher.heroModels != null &&
            heroSwitcher.activeHeroIndex >= 0 &&
            heroSwitcher.activeHeroIndex < heroSwitcher.heroModels.Count &&
            heroSwitcher.heroModels[heroSwitcher.activeHeroIndex] != null)
        {
            GameObject activeModel = heroSwitcher.heroModels[heroSwitcher.activeHeroIndex];
            if (!activeModel.activeSelf) activeModel.SetActive(true);
            foundAnim = activeModel.GetComponentInChildren<Animator>(true);
            foundLegacy = activeModel.GetComponentInChildren<Animation>(true);
        }

        if (foundAnim == null)
            foundAnim = heroSwitcher.GetComponentInChildren<Animator>(true);
        if (foundLegacy == null)
            foundLegacy = heroSwitcher.GetComponentInChildren<Animation>(true);

        cachedActiveAnimator = foundAnim;
        cachedLegacyAnimation = foundLegacy;

        if (cachedActiveAnimator != null)
        {
            cachedActiveAnimator.applyRootMotion = false;
            cachedActiveAnimator.updateMode = AnimatorUpdateMode.Normal;
            cachedActiveAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            cachedActiveAnimator.speed = 1f;

            // Вмикаємо Animator тільки якщо в нього призначений AnimatorController
            if (cachedActiveAnimator.runtimeAnimatorController != null)
            {
                cachedActiveAnimator.enabled = true;
                if (isRunning)
                {
                    cachedActiveAnimator.Rebind();
                    cachedActiveAnimator.Update(0f);
                }
            }
            else
            {
                cachedActiveAnimator.enabled = false;
            }
        }

        if (cachedLegacyAnimation != null && cachedLegacyAnimation.clip != null)
        {
            cachedLegacyAnimation.enabled = true;
            if (isRunning && !cachedLegacyAnimation.isPlaying)
                cachedLegacyAnimation.Play();
        }
    }

    private void SyncHeroAnimator(bool isRunning)
    {
        if (heroSwitcher != null && heroSwitcher.activeHeroIndex != lastHeroIndex)
        {
            lastHeroIndex = heroSwitcher.activeHeroIndex;
            RefreshAndForceRunAnimator(isRunning);
        }

        // Якщо є Animator із призначеним AnimatorController — безпечно оновлюємо параметри без спаму в консоль
        if (cachedActiveAnimator != null && cachedActiveAnimator.runtimeAnimatorController != null)
        {
            if (!cachedActiveAnimator.enabled)
                cachedActiveAnimator.enabled = true;

            cachedActiveAnimator.speed = 1f;

            if (cachedActiveAnimator.isInitialized)
            {
                foreach (var param in cachedActiveAnimator.parameters)
                {
                    string pName = param.name.ToLower();
                    if (param.type == AnimatorControllerParameterType.Float)
                    {
                        if (pName.Contains("speed") || pName.Contains("velocity") || pName.Contains("move"))
                            cachedActiveAnimator.SetFloat(param.name, isRunning ? 1f : 0f);
                    }
                    else if (param.type == AnimatorControllerParameterType.Bool)
                    {
                        if (pName.Contains("run") || pName.Contains("move") || pName.Contains("play"))
                            cachedActiveAnimator.SetBool(param.name, isRunning);
                        else if (pName.Contains("ground"))
                            cachedActiveAnimator.SetBool(param.name, motor != null ? motor.IsGrounded : true);
                        else if (pName.Contains("jump"))
                            cachedActiveAnimator.SetBool(param.name, isRunning && motor != null && !motor.IsGrounded);
                    }
                }

                if (isRunning && cachedActiveAnimator.layerCount > 0)
                {
                    AnimatorStateInfo st = cachedActiveAnimator.GetCurrentAnimatorStateInfo(0);
                    if (st.IsName("Idle") || st.IsName("Stand") || st.IsName("T-Pose"))
                    {
                        if (cachedActiveAnimator.HasState(0, Animator.StringToHash("Run")))
                            cachedActiveAnimator.Play("Run", 0, 0f);
                        else if (cachedActiveAnimator.HasState(0, Animator.StringToHash("Running")))
                            cachedActiveAnimator.Play("Running", 0, 0f);
                    }
                }
            }
            return;
        }

        // Якщо у 3D-моделі немає AnimatorController, використовуємо Legacy Animation або жвавий процедурний біг моделі
        if (cachedLegacyAnimation != null && cachedLegacyAnimation.clip != null)
        {
            if (isRunning && !cachedLegacyAnimation.isPlaying)
                cachedLegacyAnimation.Play();
            return;
        }

        AnimateModelFallbackRun(isRunning);
    }

    private void AnimateModelFallbackRun(bool isRunning)
    {
        if (heroSwitcher == null || heroSwitcher.heroModels == null) return;
        int idx = heroSwitcher.activeHeroIndex;
        if (idx < 0 || idx >= heroSwitcher.heroModels.Count) return;

        GameObject activeModel = heroSwitcher.heroModels[idx];
        if (activeModel == null) return;

        Transform modelTr = activeModel.transform;
        if (isRunning && motor != null && motor.IsGrounded)
        {
            runBounceTimer += Time.deltaTime * 16f;
            float bobY = Mathf.Abs(Mathf.Sin(runBounceTimer)) * 0.14f;
            float tiltZ = Mathf.Sin(runBounceTimer) * 6f;
            Vector3 lp = modelTr.localPosition;
            lp.y = bobY;
            modelTr.localPosition = lp;
            modelTr.localRotation = Quaternion.Euler(8f, 0f, tiltZ);
        }
        else
        {
            Vector3 lp = modelTr.localPosition;
            lp.y = Mathf.Lerp(lp.y, 0f, Time.deltaTime * 10f);
            modelTr.localPosition = lp;
            modelTr.localRotation = Quaternion.Lerp(modelTr.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        }
    }

    private void ProcessCoinsOnTrack(Transform playerTr)
    {
        Vector3 pPos = playerTr.position + Vector3.up * 1.0f;
        Collider[] cols = Physics.OverlapSphere(pPos, 2.3f);

        foreach (var col in cols)
        {
            if (col == null || !col.gameObject.activeInHierarchy) continue;
            string n = col.gameObject.name.ToLower();
            if (!n.Contains("coin")) continue;

            if (Vector3.Distance(pPos, col.transform.position) < 2.2f)
            {
                col.gameObject.SetActive(false);
                missionCoinsRun++;
                ui.AddCollectedCoin(1);
                BrainrotAudioManager.Instance?.PlayClick();
            }
        }
    }

    public void ApplyBiomeVisualTheme(int biomeIndex)
    {
        Camera cam = Camera.main;
        Color skyColor;
        Color roadColor;

        if (biomeIndex == 1) // Крижана Тундра
        {
            skyColor = new Color(0.68f, 0.88f, 0.98f, 1f);
            roadColor = new Color(0.88f, 0.94f, 0.98f, 1f);
        }
        else if (biomeIndex == 2) // Дика Савана
        {
            skyColor = new Color(0.98f, 0.72f, 0.35f, 1f);
            roadColor = new Color(0.82f, 0.62f, 0.28f, 1f);
        }
        else // Місто (Одеса)
        {
            skyColor = new Color(0.32f, 0.65f, 0.96f, 1f);
            roadColor = new Color(0.24f, 0.26f, 0.30f, 1f);
        }

        if (cam != null)
        {
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = skyColor;
        }

        RenderSettings.fog = true;
        RenderSettings.fogColor = skyColor;

        foreach (var rend in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            if (rend == null) continue;
            string n = rend.gameObject.name.ToLower();
            if (n.Contains("road") || n.Contains("ground") || n.Contains("track") || n.Contains("floor"))
            {
                if (rend.material != null)
                    rend.material.color = roadColor;
            }
        }
    }
}