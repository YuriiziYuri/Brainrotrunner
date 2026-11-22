using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BrainrotRunner.Player;

public class HeroSwitcher : MonoBehaviour
{
    [Header("Список із 10 моделей героїв")]
    public List<GameObject> heroModels = new List<GameObject>();

    [Header("Імена героїв для UI (10 шт.)")]
    public List<string> heroNames = new List<string>
    {
        "Strawberry Man",
        "Banana Man",
        "Banana Explorer",
        "Strawberry Nomad",
        "Ice Cream Polar",
        "Spicy Chili",
        "Pineapple Diver",
        "Cyber Watermelon",
        "Donut Pilot",
        "Golden Apple"
    };

    [Header("Підписи / біоми героїв (10 шт.)")]
    public List<string> heroSubtitles = new List<string>
    {
        "Класичний бігун · Місто",
        "Тропічний спринтер · Савана",
        "Шукач пригод · Савана",
        "Кочівник пустелі · Савана",
        "Полярник · Тундра",
        "Вогняний перець · Савана",
        "Дайвер · Місто",
        "Кіборг · Кібер-біом",
        "Пілот · Місто",
        "Легендарний скін · Усі біоми"
    };

    [Header("Загальний контролер анімацій")]
    public RuntimeAnimatorController heroAnimator;

    [Header("Поточний активний герой (0–9)")]
    public int activeHeroIndex = 0;

    private const string PREFS_KEY = "SelectedHeroIndex";
    private GameObject currentVisualInstance;
    private Animator rootAnimator;
    private Animator activeModelAnimator;
    private PlayerMotor playerMotor;
    private int defaultRunStateHash = 0;

    private void Awake()
    {
        rootAnimator = GetComponent<Animator>();
        playerMotor = GetComponent<PlayerMotor>();
        activeHeroIndex = PlayerPrefs.GetInt(PREFS_KEY, 0);
    }

    private void Start()
    {
        if (heroModels != null && heroModels.Count > 0)
        {
            activeHeroIndex = Mathf.Clamp(activeHeroIndex, 0, heroModels.Count - 1);
            SwitchHero(activeHeroIndex);
        }
    }

    private void Update()
    {
        if (heroModels == null || heroModels.Count == 0) return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.nKey.wasPressedThisFrame) NextHero();
            else if (Keyboard.current.bKey.wasPressedThisFrame) PrevHero();

            if (Keyboard.current.f1Key.wasPressedThisFrame) SwitchHero(0);
            if (Keyboard.current.f2Key.wasPressedThisFrame) SwitchHero(1);
            if (Keyboard.current.f3Key.wasPressedThisFrame) SwitchHero(2);
            if (Keyboard.current.f4Key.wasPressedThisFrame) SwitchHero(3);
            if (Keyboard.current.f5Key.wasPressedThisFrame) SwitchHero(4);
            if (Keyboard.current.f6Key.wasPressedThisFrame) SwitchHero(5);
            if (Keyboard.current.f7Key.wasPressedThisFrame) SwitchHero(6);
            if (Keyboard.current.f8Key.wasPressedThisFrame) SwitchHero(7);
            if (Keyboard.current.f9Key.wasPressedThisFrame) SwitchHero(8);
            if (Keyboard.current.f10Key.wasPressedThisFrame) SwitchHero(9);
        }

        SyncAnimatorParameters();
        EnsureRunAnimationOnGround();
    }

    public void NextHero()
    {
        if (heroModels == null || heroModels.Count == 0) return;
        SwitchHero((activeHeroIndex + 1) % heroModels.Count);
    }

    public void PrevHero()
    {
        if (heroModels == null || heroModels.Count == 0) return;
        SwitchHero((activeHeroIndex - 1 + heroModels.Count) % heroModels.Count);
    }

    public string GetCurrentHeroName()
    {
        if (heroNames != null && activeHeroIndex >= 0 && activeHeroIndex < heroNames.Count && !string.IsNullOrWhiteSpace(heroNames[activeHeroIndex]))
            return heroNames[activeHeroIndex];

        if (heroModels != null && activeHeroIndex >= 0 && activeHeroIndex < heroModels.Count && heroModels[activeHeroIndex] != null)
            return heroModels[activeHeroIndex].name.Replace("@Running", "");

        return $"Hero #{activeHeroIndex + 1}";
    }

    public string GetCurrentHeroSubtitle()
    {
        if (heroSubtitles != null && activeHeroIndex >= 0 && activeHeroIndex < heroSubtitles.Count)
            return heroSubtitles[activeHeroIndex];

        return "Brainrot Runner";
    }

    private void SyncAnimatorParameters()
    {
        if (rootAnimator == null || activeModelAnimator == null || activeModelAnimator == rootAnimator) return;
        if (rootAnimator.runtimeAnimatorController == null || activeModelAnimator.runtimeAnimatorController == null) return;

        foreach (AnimatorControllerParameter param in rootAnimator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Bool)
                activeModelAnimator.SetBool(param.nameHash, rootAnimator.GetBool(param.nameHash));
            else if (param.type == AnimatorControllerParameterType.Float)
                activeModelAnimator.SetFloat(param.nameHash, rootAnimator.GetFloat(param.nameHash));
            else if (param.type == AnimatorControllerParameterType.Int)
                activeModelAnimator.SetInteger(param.nameHash, rootAnimator.GetInteger(param.nameHash));
            else if (param.type == AnimatorControllerParameterType.Trigger && rootAnimator.GetBool(param.nameHash))
            {
                activeModelAnimator.SetTrigger(param.nameHash);
                rootAnimator.ResetTrigger(param.nameHash);
            }
        }
    }

    // Гарантує, що коли персонаж біжить по землі (після стрибка чи слайду), аніматор не застрягає у Jump/Slide/Idle
    private void EnsureRunAnimationOnGround()
    {
        if (activeModelAnimator == null || activeModelAnimator.runtimeAnimatorController == null) return;
        if (playerMotor == null) return;

        if (playerMotor.State == PlayerState.Running && playerMotor.IsGrounded && !playerMotor.IsSliding)
        {
            AnimatorStateInfo st = activeModelAnimator.GetCurrentAnimatorStateInfo(0);
            bool isStuckInAction = st.IsName("Jump") || st.IsName("Slide") || st.IsName("Fall") ||
                                   (st.normalizedTime >= 0.98f && !st.loop);

            if (isStuckInAction && defaultRunStateHash != 0)
            {
                activeModelAnimator.Play(defaultRunStateHash, 0, 0f);
            }
        }
    }

    public void ForcePlayRunAnimation()
    {
        if (activeModelAnimator == null || activeModelAnimator.runtimeAnimatorController == null) return;
        activeModelAnimator.enabled = true;
        activeModelAnimator.speed = 1f;
        if (defaultRunStateHash != 0)
            activeModelAnimator.Play(defaultRunStateHash, 0, 0f);
    }

    public void SwitchHero(int index)
    {
        if (heroModels == null || index < 0 || index >= heroModels.Count) return;
        if (heroModels[index] == null) return;

        if (currentVisualInstance != null)
        {
            Destroy(currentVisualInstance);
        }
        else
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child.GetComponentInChildren<SkinnedMeshRenderer>() != null ||
                    child.GetComponentInChildren<MeshRenderer>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        activeHeroIndex = index;
        PlayerPrefs.SetInt(PREFS_KEY, activeHeroIndex);
        PlayerPrefs.Save();

        currentVisualInstance = Instantiate(heroModels[index], transform);
        currentVisualInstance.transform.localPosition = Vector3.zero;
        currentVisualInstance.transform.localRotation = Quaternion.identity;
        currentVisualInstance.transform.localScale = Vector3.one;

        activeModelAnimator = currentVisualInstance.GetComponentInChildren<Animator>();
        if (activeModelAnimator == null)
        {
            activeModelAnimator = currentVisualInstance.AddComponent<Animator>();
        }

        activeModelAnimator.enabled = true;
        activeModelAnimator.applyRootMotion = false;
        activeModelAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        // Якщо у префаба немає власного контролера — ставимо загальний heroAnimator
        if (activeModelAnimator.runtimeAnimatorController == null && heroAnimator != null)
        {
            activeModelAnimator.runtimeAnimatorController = heroAnimator;
        }
        else if (heroAnimator != null && index > 0)
        {
            activeModelAnimator.runtimeAnimatorController = heroAnimator;
        }

        if (activeModelAnimator.runtimeAnimatorController != null)
        {
            activeModelAnimator.Rebind();
            activeModelAnimator.Update(0f);
            defaultRunStateHash = activeModelAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        }
        else
        {
            defaultRunStateHash = 0;
        }

        if (rootAnimator != null && activeModelAnimator.avatar != null)
        {
            rootAnimator.avatar = activeModelAnimator.avatar;
        }

        if (playerMotor != null)
        {
            playerMotor.RefreshAnimatorReference();
        }
    }
}