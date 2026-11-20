using BrainrotRunner.Core;
using BrainrotRunner.Player;
using UnityEngine;

namespace BrainrotRunner.Progression
{

    public sealed class ScoreSystem : MonoBehaviour
    {
        public static ScoreSystem Instance { get; private set; }

        [SerializeField] private GameTuning tuning;
        [SerializeField] private PlayerMotor motor;

        public int Score { get; private set; }
        public bool ThresholdReached { get; private set; }

        public float Progress01 => tuning.levelScoreThreshold <= 0
            ? 0f
            : Mathf.Clamp01((float)Score / tuning.levelScoreThreshold);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            GameManager game = GameManager.Instance;
            if (game != null)
                game.RunStarted += ResetScore;
        }

        private void OnDestroy()
        {
            GameManager game = GameManager.Instance;
            if (game != null)
                game.RunStarted -= ResetScore;

            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            GameManager game = GameManager.Instance;
            if (game == null || game.State != GameState.Running)
                return;

            int coins = CoinSystem.Instance != null ? CoinSystem.Instance.Collected : 0;

            Score = Mathf.RoundToInt(motor.DistanceTravelled * tuning.pointsPerMeter)
                    + coins * tuning.pointsPerCoin;

            if (!ThresholdReached && Score >= tuning.levelScoreThreshold)
                ThresholdReached = true;
        }

        private void ResetScore()
        {
            Score = 0;
            ThresholdReached = false;
        }
    }
}
