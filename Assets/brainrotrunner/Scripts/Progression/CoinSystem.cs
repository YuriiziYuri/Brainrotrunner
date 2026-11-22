using System;
using BrainrotRunner.Core;
using UnityEngine;

namespace BrainrotRunner.Progression
{
    public sealed class CoinSystem : MonoBehaviour
    {
        private static CoinSystem _instance;

        public static CoinSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<CoinSystem>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("CoinSystem");
                        _instance = go.AddComponent<CoinSystem>();
                    }
                }
                return _instance;
            }
        }

        public event Action<int> Changed;
        public int Collected { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(this);
                return;
            }

            _instance = this;
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

            if (_instance == this)
                _instance = null;
        }

        public void Add(int amount = 1)
        {
            Collected += amount;
            Changed?.Invoke(Collected);
        }

        private void Reset()
        {
            Collected = 0;
            Changed?.Invoke(Collected);
        }
    }
}