using BrainrotRunner.Core;
using BrainrotRunner.World;
using UnityEngine;

namespace BrainrotRunner.Player
{
    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerCollisionProbe : MonoBehaviour
    {
        [SerializeField] private float halfWidth = 0.3f;
        [SerializeField] private float halfDepth = 0.25f;
        [Tooltip("Винос перевірки вперед (0 = точно по корпусу гравця).")]
        [SerializeField] private float forwardKillLead = 0f;

        private PlayerMotor _motor;
        private ChunkSpawner _spawner;
        private Rigidbody _rb;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _spawner = FindFirstObjectByType<ChunkSpawner>();

            _rb = GetComponent<Rigidbody>();
            if (_rb == null)
                _rb = gameObject.AddComponent<Rigidbody>();

            _rb.isKinematic = true;
            _rb.useGravity = false;
        }

        private void LateUpdate()
        {
            if (_motor == null || _motor.State == PlayerState.Dead)
                return;

            GameManager game = GameManager.Instance;
            if (game != null && game.State != GameState.Running)
                return;

            if (_spawner == null)
            {
                _spawner = FindFirstObjectByType<ChunkSpawner>();
                if (_spawner == null)
                    return;
            }

            Bounds playerBounds = BuildPlayerBounds();
            Bounds coinPickupBounds = playerBounds;
            coinPickupBounds.Expand(new Vector3(0.55f, 0.45f, 0.45f));

            foreach (TrackChunk chunk in _spawner.Chunks)
            {
                if (transform.position.z < chunk.StartZ - 3f || transform.position.z > chunk.EndZ + 3f)
                    continue;

                foreach (Coin coin in chunk.Coins)
                {
                    if (!coin.gameObject.activeInHierarchy)
                        continue;

                    if (coinPickupBounds.Intersects(coin.WorldBounds))
                        coin.Collect();
                }

                foreach (Obstacle obs in chunk.Obstacles)
                {
                    if (!obs.gameObject.activeInHierarchy)
                        continue;

                    if (IsSafePass(obs, playerBounds))
                        continue;

                    KillPlayer(obs.Type);
                    return;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_motor == null || _motor.State == PlayerState.Dead)
                return;

            Coin coin = other.GetComponent<Coin>();
            if (coin != null)
            {
                coin.Collect();
                return;
            }

            BiomePortal portal = other.GetComponent<BiomePortal>();
            if (portal != null)
            {
                portal.TriggerPortal();
                return;
            }

            // Перешкоди (Obstacle) обробляються виключно в LateUpdate через IsSafePass,
            // щоб фізичний CapsuleCollider з Center Y=0 не вбивав гравця в повітрі!
        }

        private Bounds BuildPlayerBounds()
        {
            // Будуємо чесний хідбокс строго від підошви ніг (transform.position.y) вгору
            float feetY = transform.position.y;
            float height = Mathf.Max(0.5f, _motor.CurrentHeight);

            Vector3 center = new Vector3(
                transform.position.x,
                feetY + height * 0.5f,
                transform.position.z + forwardKillLead);

            Vector3 size = new Vector3(halfWidth * 2f, height * 0.9f, halfDepth * 2f);
            return new Bounds(center, size);
        }

        private bool IsSafePass(Obstacle obstacle, Bounds playerBounds)
        {
            // 1. Низький бар'єр: якщо гравець у стрибку — завжди безпечно
            if (obstacle.Type == ObstacleType.JumpBlock && !_motor.IsGrounded)
                return true;

            // 2. Ворота / верхня балка: якщо гравець у підкаті — завжди безпечно
            if (obstacle.Type == ObstacleType.SlideBar && _motor.IsSliding)
                return true;

            Bounds obsBounds = obstacle.WorldBounds;

            // 3. Машини (LaneBlock): якщо ноги гравця (transform.position.y) піднялися вище 1.15 м
            // або гравець летить у верхній фазі стрибка — машина чисто перестрибується!
            if (obstacle.Type == ObstacleType.LaneBlock && !_motor.IsGrounded)
            {
                if (transform.position.y >= 1.15f)
                    return true;
            }

            return !playerBounds.Intersects(obsBounds);
        }

        private void KillPlayer(ObstacleType hitType)
        {
            _motor.Kill();

            string reason = hitType switch
            {
                ObstacleType.LaneBlock => "Врізався у перешкоду",
                ObstacleType.JumpBlock => "Не перестрибнув бар'єр",
                ObstacleType.SlideBar => "Не зробив підкат під балкою",
                _ => "Зіткнення"
            };

            if (GameManager.Instance != null)
                GameManager.Instance.ReportPlayerHit(reason, _motor.DistanceTravelled);
        }
    }
}