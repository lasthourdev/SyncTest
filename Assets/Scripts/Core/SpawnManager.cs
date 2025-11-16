using iPAHeartBeat.Core.Dependency;
using UnityEngine;

namespace milan.Core
{
    public class SpawnManager : MonoBehaviour
    {
        GameManager gameManager;
        public static SpawnManager Instance { get; private set; }

        [SerializeField] private Transform _playerReference;

        private float _nextSpawnZ = 20f;
        private System.Random _random;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _random = new System.Random();
        }

        void Start()
        {
            gameManager ??= DependencyResolver.Resolve<GameManager>();
        }

        private void OnEnable()
        {
            GameEvents.OnGameStart += HandleGameStart;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= HandleGameStart;
        }

        private void Update()
        {
            if (gameManager.CurrentState != GameState.Playing) return;
            if (_playerReference == null) return;

            CheckAndSpawn();
        }

        private void CheckAndSpawn()
        {
            float playerZ = _playerReference.position.z;

            while (playerZ + 50f > _nextSpawnZ)
            {
                SpawnPattern(_nextSpawnZ);
                _nextSpawnZ += GameConstants.SECTION_DISTANCE;
            }
        }

        private void SpawnPattern(float spawnZ)
        {
            int patternType = _random.Next(0, 4);

            switch (patternType)
            {
                case 0:
                    SpawnType1_RandomMix(spawnZ);
                    break;
                case 1:
                    SpawnType2_AllObstacles(spawnZ);
                    break;
                case 2:
                    SpawnType3_MovingObstacle(spawnZ);
                    break;
                case 3:
                    SpawnType4_SideObstaclesWithAir(spawnZ);
                    break;
            }
        }

        private void SpawnType1_RandomMix(float spawnZ)
        {
            for (int laneIndex = 0; laneIndex < 3; laneIndex++)
            {
                Lane lane = (Lane)laneIndex;
                int randomValue = _random.Next(0, 100);

                if (randomValue < 66)
                {
                    SpawnObstacle(lane, spawnZ, false);
                }
                else
                {
                    SpawnOrb(lane, spawnZ, 0.5f, ObjectType.Orb);
                }
            }
        }

        private void SpawnType2_AllObstacles(float spawnZ)
        {
            for (int laneIndex = 0; laneIndex < 3; laneIndex++)
            {
                Lane lane = (Lane)laneIndex;
                SpawnObstacle(lane, spawnZ, false);
            }
        }

        private void SpawnType3_MovingObstacle(float spawnZ)
        {
            Lane startLane = (Lane)_random.Next(0, 3);
            SpawnObstacle(startLane, spawnZ, true);
        }

        private void SpawnType4_SideObstaclesWithAir(float spawnZ)
        {
            Lane middleLane = (Lane)_random.Next(0, 3);

            for (int laneIndex = 0; laneIndex < 3; laneIndex++)
            {
                Lane lane = (Lane)laneIndex;

                if (lane == middleLane)
                {
                    SpawnObstacle(lane, spawnZ, false);
                    SpawnOrb(lane, spawnZ + 2f, 2.5f, ObjectType.AirOrb);
                }
                else
                {
                    SpawnObstacle(lane, spawnZ, false);
                    SpawnObstacle(lane, spawnZ + 5f, false);
                }
            }
        }
        private void SpawnObstacle(Lane lane, float zPos, bool isMoving)
        {
            Obstacle rightObs = PoolManager.Instance.GetObstacle(true);
            rightObs.gameObject.SetActive(true);

            Vector3 position = GetWorldPosition(GameConstants.RIGHT_SIDE_OFFSET, lane, zPos, 0.5f);
            rightObs.transform.position = position;

            SetLayer(rightObs.gameObject, LayerMask.NameToLayer("RightObjects"));

            if (isMoving)
                rightObs.SetAsMoving(GameConstants.RIGHT_SIDE_OFFSET);
            else
                rightObs.SetAsStatic();

            ObjectSpawnData spawnData = new ObjectSpawnData(
                rightObs.GetInstanceID(),
                ObjectType.Obstacle,
                position,
                lane,
                isMoving
            );
            gameManager.syncManager.BroadcastObjectSpawn(spawnData);
        }

        private void SpawnOrb(Lane lane, float zPos, float yPos, ObjectType orbType)
        {
            Collectible rightOrb = PoolManager.Instance.GetOrb(true);
            rightOrb.gameObject.SetActive(true);

            Vector3 position = GetWorldPosition(GameConstants.RIGHT_SIDE_OFFSET, lane, zPos, yPos);
            rightOrb.transform.position = position;

            SetLayer(rightOrb.gameObject, LayerMask.NameToLayer("RightObjects"));

            ObjectSpawnData spawnData = new ObjectSpawnData(
                rightOrb.GetInstanceID(),
                orbType,
                position,
                lane,
                false
            );
            gameManager.syncManager.BroadcastObjectSpawn(spawnData);
        }

        private void SetLayer(GameObject obj, int layer)
        {
            if (obj == null)
                return;

            obj.layer = layer;

            foreach (Transform child in obj.transform)
            {
                if (child == null)
                    continue;
                SetLayer(child.gameObject, layer);
            }
        }

        private Vector3 GetWorldPosition(float sideOffset, Lane lane, float zPos, float yPos)
        {
            float xOffset = sideOffset + ((int)lane - 1) * GameConstants.LANE_DISTANCE;
            return new Vector3(xOffset, yPos, zPos);
        }

        private void HandleGameStart()
        {
            _nextSpawnZ = 20f;
            PoolManager.Instance.ReturnAllObjects();
            gameManager.syncManager.ClearSyncData();
        }
    }
}
