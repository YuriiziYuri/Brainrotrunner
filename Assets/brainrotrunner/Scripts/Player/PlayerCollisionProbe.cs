using BrainrotRunner.Core;
using BrainrotRunner.World;
using UnityEngine;

namespace BrainrotRunner.Player
{

    [RequireComponent(typeof(PlayerMotor))]
    public sealed class PlayerCollisionProbe : MonoBehaviour
    {
        private const int MaxHits = 8;

        [Tooltip("Половина ширини та глибини персонажа. Трохи менше за візуальну капсулу: " +
                 "чесніше для гравця, коли зачіп по краю не вважається зіткненням.")]
        [SerializeField] private float halfWidth = 0.35f;
        [SerializeField] private float halfDepth = 0.35f;

        private PlayerMotor _motor;
        private GameManager _game;
        private readonly Collider[] _hits = new Collider[MaxHits];

        private void Awake() => _motor = GetComponent<PlayerMotor>();

        private void Start() => _game = GameManager.Instance;

        private void Update()
        {
            if (_game == null || _game.State != GameState.Running)
                return;

            float halfHeight = _motor.CurrentHeight * 0.5f;
            Vector3 center = transform.position;
            Vector3 extents = new Vector3(halfWidth, halfHeight, halfDepth);

            CollectCoins(center, extents);

            int count = Physics.OverlapBoxNonAlloc(center, extents, _hits, Quaternion.identity);

            for (int i = 0; i < count; i++)
            {
                Obstacle obstacle = _hits[i].GetComponent<Obstacle>();
                if (obstacle == null)
                    continue;

                _game.ReportPlayerHit(ReasonFor(obstacle.Type), _motor.DistanceTravelled);
                return;
            }
        }

        private void CollectCoins(Vector3 center, Vector3 extents)
        {
            Vector3 coinCenter = new Vector3(center.x, Mathf.Max(center.y, 0.7f), center.z);
            Vector3 coinExtents = extents + new Vector3(0.25f, 0.45f, 0.25f);

            int count = Physics.OverlapBoxNonAlloc(
                coinCenter, coinExtents, _hits, Quaternion.identity, ~0, QueryTriggerInteraction.Collide);

            for (int i = 0; i < count; i++)
            {
                Coin coin = _hits[i].GetComponent<Coin>();
                if (coin == null || coin.Collected)
                    continue;

                coin.Collect();

                if (Progression.CoinSystem.Instance != null)
                    Progression.CoinSystem.Instance.Add();
            }
        }

        private static string ReasonFor(ObstacleType type)
        {
            switch (type)
            {
                case ObstacleType.JumpBlock:
                    return "Не перестрибнув зелений блок";
                case ObstacleType.SlideBar:
                    return "Не підкотився під фіолетову балку";
                default:
                    return "Не обійшов червону стіну";
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (_motor == null)
                return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position,
                new Vector3(halfWidth * 2f, _motor.CurrentHeight, halfDepth * 2f));
        }
    }
}
