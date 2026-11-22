using System.Collections.Generic;
using BrainrotRunner.Core;
using BrainrotRunner.Player;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BrainrotRunner.World
{
    public sealed class ChunkSpawner : MonoBehaviour
    {
        [SerializeField] private GameTuning tuning;
        [SerializeField] private TrackPalette palette;
        [SerializeField] private Transform player;
        [Tooltip("Скільки метрів дороги тримати позаду гравця, щоб задній фон не пропадав.")]
        [SerializeField] private float rearBufferDistance = 35f;

        private readonly List<TrackChunk> _chunks = new List<TrackChunk>();
        private System.Random _random;
        private int _oldestIndex;
        private float _distanceGenerated;
        private int _spawnedChunkCounter;
        private int _lockedBiomeIndex = 0;
        private Camera _mainCamera;

        public IReadOnlyList<TrackChunk> Chunks => _chunks;
        public int ChunkCount => _chunks.Count;
        public int CurrentLockedBiome => _lockedBiomeIndex;

        private void Awake()
        {
            _mainCamera = Camera.main;

            if (player == null)
            {
                PlayerMotor motor = FindFirstObjectByType<PlayerMotor>();
                if (motor != null)
                    player = motor.transform;
            }

            int count = Mathf.Max(6, tuning.chunksAhead + 2);

            for (int i = 0; i < count; i++)
            {
                GameObject chunkObject = new GameObject("Chunk" + i);
                chunkObject.transform.SetParent(transform, false);

                TrackChunk chunk = chunkObject.AddComponent<TrackChunk>();
                chunk.Build(tuning, palette);
                _chunks.Add(chunk);
            }
        }

        private void Start()
        {
            GameManager game = GameManager.Instance;
            if (game != null)
                game.RunStarted += ResetTrack;

            ResetTrack();
        }

        private void OnDestroy()
        {
            GameManager game = GameManager.Instance;
            if (game != null)
                game.RunStarted -= ResetTrack;
        }

        private void Update()
        {
            if (player == null)
            {
                PlayerMotor motor = FindFirstObjectByType<PlayerMotor>();
                if (motor == null)
                    return;
                player = motor.transform;
            }

            HandleDebugBiomeKeys();
            UpdateSkyColor();

            TrackChunk oldest = _chunks[_oldestIndex];

            if (player.position.z < oldest.EndZ + rearBufferDistance)
                return;

            int biomeIndex = GetClampedBiomeIndex(_lockedBiomeIndex);

            oldest.MoveTo(_distanceGenerated);
            oldest.Populate(_random, true, biomeIndex, false, biomeIndex);

            _distanceGenerated += oldest.Length;
            _spawnedChunkCounter++;
            _oldestIndex = (_oldestIndex + 1) % _chunks.Count;
        }

        private void HandleDebugBiomeKeys()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || !palette.HasBiomes)
                return;

            if (kb.digit1Key.wasPressedThisFrame) SetBiomeImmediate(0);
            else if (kb.digit2Key.wasPressedThisFrame && palette.biomes.Count > 1) SetBiomeImmediate(1);
            else if (kb.digit3Key.wasPressedThisFrame && palette.biomes.Count > 2) SetBiomeImmediate(2);
        }

        /// <summary>
        /// Фіксує 100% інфраструктури тільки для обраного біому і миттєво перебудовує всі чанки на трасі.
        /// </summary>
        public void SetBiomeImmediate(int biomeIndex)
        {
            _lockedBiomeIndex = GetClampedBiomeIndex(biomeIndex);

            if (_random == null)
            {
                _random = tuning.obstacleSeed == 0
                    ? new System.Random()
                    : new System.Random(tuning.obstacleSeed);
            }

            for (int i = 0; i < _chunks.Count; i++)
            {
                TrackChunk chunk = _chunks[i];
                bool allowObstacles = chunk.StartZ >= tuning.safeStartDistance;
                chunk.Populate(_random, allowObstacles, _lockedBiomeIndex, false, _lockedBiomeIndex);
            }

            UpdateSkyColorImmediate();
        }

        private int GetClampedBiomeIndex(int desiredIndex)
        {
            if (!palette.HasBiomes || palette.biomes.Count == 0)
                return 0;

            return Mathf.Clamp(desiredIndex, 0, palette.biomes.Count - 1);
        }

        private void UpdateSkyColorImmediate()
        {
            if (_mainCamera == null || !palette.HasBiomes)
                return;

            int activeBiome = GetClampedBiomeIndex(_lockedBiomeIndex);
            Color targetSky = palette.biomes[activeBiome].skyColor;
            _mainCamera.backgroundColor = targetSky;
            RenderSettings.fogColor = targetSky;
        }

        private void UpdateSkyColor()
        {
            if (_mainCamera == null || !palette.HasBiomes)
                return;

            int activeBiome = GetClampedBiomeIndex(_lockedBiomeIndex);
            Color targetSky = palette.biomes[activeBiome].skyColor;
            _mainCamera.backgroundColor = Color.Lerp(_mainCamera.backgroundColor, targetSky, Time.deltaTime * 3f);
            RenderSettings.fogColor = Color.Lerp(RenderSettings.fogColor, targetSky, Time.deltaTime * 3f);
        }

        public void ResetTrack()
        {
            _random = tuning.obstacleSeed == 0
                ? new System.Random()
                : new System.Random(tuning.obstacleSeed);

            _oldestIndex = 0;
            _distanceGenerated = 0f;
            _spawnedChunkCounter = 0;

            int biomeIndex = GetClampedBiomeIndex(_lockedBiomeIndex);

            for (int i = 0; i < _chunks.Count; i++)
            {
                TrackChunk chunk = _chunks[i];
                chunk.MoveTo(_distanceGenerated);

                bool allowObstacles = _distanceGenerated >= tuning.safeStartDistance;
                chunk.Populate(_random, allowObstacles, biomeIndex, false, biomeIndex);

                _distanceGenerated += chunk.Length;
                _spawnedChunkCounter++;
            }

            UpdateSkyColorImmediate();
        }
    }
}