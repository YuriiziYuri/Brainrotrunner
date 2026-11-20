using UnityEngine;

namespace BrainrotRunner.Core
{
    [CreateAssetMenu(menuName = "Brainrot/Game Tuning", fileName = "GameTuning")]
    public sealed class GameTuning : ScriptableObject
    {
        [Header("Lanes")]
        [Tooltip("Відстань між центрами смуг, юніти.")]
        public float laneWidth = 2.5f;
        [Tooltip("Тривалість переходу між сусідніми смугами, с. <0.1 відчувається як телепорт.")]
        public float laneChangeTime = 0.15f;
        [Tooltip("Дозволити новий свайп, не дочекавшись завершення попереднього переходу.")]
        public bool allowChainedLaneChange = true;

        [Header("Forward speed")]
        public float startSpeed = 8f;
        public float maxSpeed = 16f;
        [Tooltip("Приріст швидкості, юніти/с за секунду забігу. 0 = стала швидкість.")]
        public float speedRamp = 0.09f;

        [Header("Jump / Slide")]
        public float jumpHeight = 1.8f;
        [Tooltip("Час до вершини стрибка, с. Разом із jumpHeight задає гравітацію.")]
        public float timeToApex = 0.32f;
        [Tooltip("Множник гравітації на спуску. >1 робить падіння різкішим і стрибок чіткішим.")]
        public float fallGravityMultiplier = 1.6f;
        public float slideDuration = 0.6f;
        [Tooltip("До якої частки висоти стискається персонаж під час ковзання.")]
        public float slideHeightScale = 0.45f;
        [Tooltip("Час входу у стиснену позу й виходу з неї, с.")]
        public float slideTransitionTime = 0.06f;

        [Header("Swipe")]
        [Tooltip("Мінімальна довжина свайпу у сантиметрах екрана — не в пікселях (NFR-01).")]
        public float swipeThresholdCm = 0.6f;
        [Tooltip("Максимальна тривалість жесту, с. Довше — це драг, не свайп.")]
        public float swipeMaxTime = 0.3f;

        [Header("Track")]
        [Tooltip("Довжина одного чанка траси, юніти.")]
        public float chunkLength = 60f;
        [Tooltip("Скільки чанків тримати попереду персонажа. Разом із chunkLength задає дальність огляду.")]
        public int chunksAhead = 4;
        [Tooltip("Перші метри без перешкод, щоб забіг не починався впритул до стіни.")]
        public float safeStartDistance = 45f;

        [Header("Obstacles")]
        public float rowSpacingMin = 14f;
        public float rowSpacingMax = 22f;
        [Tooltip("Шанс, що в ряду буде заблоковано дві смуги замість однієї.")]
        [Range(0f, 1f)]
        public float doubleBlockChance = 0.35f;
        [Tooltip("Зерно генератора. 0 — випадкове щоразу; інше значення робить трасу відтворюваною.")]
        public int obstacleSeed = 12345;

        [Header("Score")]
        [Tooltip("Очок за метр дистанції.")]
        public float pointsPerMeter = 1f;
        [Tooltip("Очок за монету.")]
        public int pointsPerCoin = 10;
        [Tooltip("Поріг очок для переходу на наступний рівень. Приблизно 18 секунд бігу з ТЗ.")]
        public int levelScoreThreshold = 200;

        [Header("Coins")]
        [Tooltip("Скільки монет ставити ланцюжком у вільній смузі.")]
        public int coinsPerRow = 5;
        [Tooltip("Відстань між монетами в ланцюжку, юніти.")]
        public float coinSpacing = 2f;

        [Header("Camera")]
        public Vector3 cameraOffset = new Vector3(0f, 4f, -6f);
        [Tooltip("Час згладжування позиції камери, с. Більше = плавніше й інертніше.")]
        public float cameraSmoothTime = 0.12f;
        [Tooltip("Наскільки камера дивиться вперед від персонажа, юніти (огляд перешкод, NFR-03).")]
        public float cameraLookAhead = 6f;
        [Tooltip("Окреме, сильніше згладжування по X — прибирає ривок при зміні смуги (NFR-02).")]
        public float cameraHorizontalSmoothTime = 0.18f;
        [Range(0f, 1f)]
        [Tooltip("Наскільки камера повторює вертикальний рух персонажа. 0 — зовсім не стрибає " +
                 "разом із ним, 1 — повторює повністю й ламає композицію кадру (п.7.1).")]
        public float cameraVerticalFollow = 0.25f;

        public float Gravity => -2f * jumpHeight / (timeToApex * timeToApex);
        public float JumpVelocity => 2f * jumpHeight / timeToApex;
    }
}
