using BrainrotRunner.Core;
using UnityEngine;

namespace BrainrotRunner.Progression
{
    public sealed class CoinSystem : MonoBehaviour
    {
        public static CoinSystem Instance { get; private set; }

        public int Collected { get; private set; }

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
                game.RunStarted += Reset;
        }

        private void OnDestroy()
        {
            GameManager game = GameManager.Instance;
            if (game != null)
                game.RunStarted -= Reset;

            if (Instance == this)
                Instance = null;
        }

        public void Add() => Collected++;

        private void Reset() => Collected = 0;
    }
}
