using System.Collections.Generic;
using BrainrotRunner.Core;
using UnityEngine;

namespace BrainrotRunner.World
{

    public sealed class ChunkSpawner : MonoBehaviour
    {
        [SerializeField] private GameTuning tuning;
        [SerializeField] private TrackPalette palette;
        [SerializeField] private Transform player;

        private readonly List<TrackChunk> _chunks = new List<TrackChunk>();
        private System.Random _random;
        private int _oldestIndex;
        private float _distanceGenerated;

        public int ChunkCount => _chunks.Count;

        private void Awake()
        {
            int count = Mathf.Max(2, tuning.chunksAhead + 1);

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
                return;

            TrackChunk oldest = _chunks[_oldestIndex];

            if (player.position.z < oldest.EndZ + tuning.cameraOffset.z - 5f)
                return;

            oldest.MoveTo(_distanceGenerated);
            oldest.Populate(_random, true);

            _distanceGenerated += oldest.Length;
            _oldestIndex = (_oldestIndex + 1) % _chunks.Count;
        }

        private void ResetTrack()
        {
            _random = tuning.obstacleSeed == 0
                ? new System.Random()
                : new System.Random(tuning.obstacleSeed);

            _oldestIndex = 0;
            _distanceGenerated = 0f;

            for (int i = 0; i < _chunks.Count; i++)
            {
                TrackChunk chunk = _chunks[i];
                chunk.MoveTo(_distanceGenerated);

                bool allowObstacles = _distanceGenerated >= tuning.safeStartDistance;
                chunk.Populate(_random, allowObstacles);

                _distanceGenerated += chunk.Length;
            }
        }
    }
}
