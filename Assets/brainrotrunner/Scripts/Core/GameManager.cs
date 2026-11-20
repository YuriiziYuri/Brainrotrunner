using System;
using UnityEngine;

namespace BrainrotRunner.Core
{
    public sealed class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameTuning tuning;

        public GameTuning Tuning => tuning;
        public GameState State { get; private set; } = GameState.Boot;
        public float RunTime { get; private set; }

        ///Чому забіг завершився (текст для гравця)
        public string LastFailReason { get; private set; } = string.Empty;

        ///Дистанція останнього забігу, метри
        public float LastRunDistance { get; private set; }

        public event Action RunStarted;
        public event Action RunEnded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start() => StartRun();

        private void Update()
        {
            if (State == GameState.Running)
                RunTime += Time.deltaTime;
        }

        public void StartRun()
        {
            RunTime = 0f;
            LastFailReason = string.Empty;
            LastRunDistance = 0f;
            State = GameState.Running;
            RunStarted?.Invoke();
        }

        ///Викликається детектором зіткнень: забіг завершується
        public void ReportPlayerHit(string reason, float distance)
        {
            if (State != GameState.Running)
                return;

            LastFailReason = reason;
            LastRunDistance = distance;
            State = GameState.GameOver;
            RunEnded?.Invoke();
        }

        ///Повний перезапуск без LoadScene: усі системи скидаються по події
        public void Restart() => StartRun();

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}
