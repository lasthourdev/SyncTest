using UnityEngine;
using System.Collections.Generic;
using iPAHeartBeat.Core.Dependency;

namespace milan.Core
{
    public class SyncManager : MonoBehaviour
    {

        GameManager gameManager;
        [SerializeField] private PlayerController _leftPlayer;

        [SerializeField] private float _syncDelay = 0.02f;
        [SerializeField] private int _bufferCapacity = 30;

        private Queue<PlayerStateData> _playerStateQueue;
        private Queue<ObjectSpawnData> _objectSpawnQueue;
        private Dictionary<int, GameObject> _syncedObjects;
        private float _delayTimer;

        private void Awake()
        {

            DependencyResolver.Register<SyncManager>(this);

            _playerStateQueue = new Queue<PlayerStateData>(_bufferCapacity);
            _objectSpawnQueue = new Queue<ObjectSpawnData>(_bufferCapacity * 3);
            _syncedObjects = new Dictionary<int, GameObject>();
            _delayTimer = 0f;
        }

        void Start()
        {
            gameManager ??= DependencyResolver.Resolve<GameManager>();
        }

        private void Update()
        {
            if (gameManager.CurrentState != GameState.Playing) return;

            ProcessSyncQueues();
        }

        public void BroadcastObjectSpawn(ObjectSpawnData spawnData)
        {
            if (_objectSpawnQueue.Count >= _bufferCapacity * 3)
                _objectSpawnQueue.Dequeue();

            _objectSpawnQueue.Enqueue(spawnData);
        }

        public void BroadcastPlayerState(PlayerStateData state)
        {
            if (_playerStateQueue.Count >= _bufferCapacity)
                _playerStateQueue.Dequeue();

            _playerStateQueue.Enqueue(state);
        }

        public void RegisterSyncedObject(int rightObjectID, GameObject leftObject)
        {
            if (!_syncedObjects.ContainsKey(rightObjectID))
                _syncedObjects.Add(rightObjectID, leftObject);
        }

        public void UnregisterSyncedObject(int rightObjectID)
        {
            if (_syncedObjects.ContainsKey(rightObjectID))
                _syncedObjects.Remove(rightObjectID);
        }

        private void ProcessSyncQueues()
        {
            _delayTimer += Time.deltaTime;

            if (_delayTimer >= _syncDelay)
            {
                _delayTimer = 0f;

                if (_playerStateQueue.Count > 0)
                {
                    PlayerStateData state = _playerStateQueue.Dequeue();
                    ApplyStateToGhost(state);
                }

                while (_objectSpawnQueue.Count > 0)
                {
                    ObjectSpawnData spawnData = _objectSpawnQueue.Dequeue();
                    SpawnObjectOnLeftSide(spawnData);
                }
            }
        }

        private void ApplyStateToGhost(PlayerStateData state)
        {
            if (_leftPlayer == null) return;

            _leftPlayer.SyncFromNetwork(state);

            if (state.CollectibleID >= 0 && _syncedObjects.ContainsKey(state.CollectibleID))
            {
                GameObject syncedOrb = _syncedObjects[state.CollectibleID];
                if (syncedOrb != null && syncedOrb.activeSelf)
                {
                    GameEvents.TriggerOrbCollected(syncedOrb.transform.position);
                    syncedOrb.SetActive(false);
                    UnregisterSyncedObject(state.CollectibleID);
                }
            }

            if (state.ObstacleHitID >= 0 && _syncedObjects.ContainsKey(state.ObstacleHitID))
            {
                GameObject syncedObstacle = _syncedObjects[state.ObstacleHitID];
                if (syncedObstacle != null && syncedObstacle.activeSelf)
                {
                    GameEvents.TriggerObstacleHit(syncedObstacle.transform.position);
                    syncedObstacle.SetActive(false);
                    UnregisterSyncedObject(state.ObstacleHitID);
                }
            }
        }

        private void SpawnObjectOnLeftSide(ObjectSpawnData spawnData)
        {
            GameObject leftObject = null;

            switch (spawnData.Type)
            {
                case ObjectType.Obstacle:
                    Obstacle obs = PoolManager.Instance.GetObstacle(false);
                    obs.gameObject.SetActive(true);
                    obs.transform.position = MirrorPosition(spawnData.Position);
                    obs.gameObject.layer = LayerMask.NameToLayer("LeftObjects");

                    if (spawnData.IsMoving)
                        obs.SetAsMoving(GameConstants.LEFT_SIDE_OFFSET);
                    else
                        obs.SetAsStatic();

                    leftObject = obs.gameObject;
                    break;

                case ObjectType.Orb:
                case ObjectType.AirOrb:
                    Collectible orb = PoolManager.Instance.GetOrb(false);
                    orb.gameObject.SetActive(true);
                    orb.transform.position = MirrorPosition(spawnData.Position);
                    orb.gameObject.layer = LayerMask.NameToLayer("LeftObjects");
                    leftObject = orb.gameObject;
                    break;
            }

            if (leftObject != null)
            {
                RegisterSyncedObject(spawnData.ObjectID, leftObject);
            }
        }

        private Vector3 MirrorPosition(Vector3 rightPos)
        {
            float xOffset = rightPos.x - GameConstants.RIGHT_SIDE_OFFSET;
            float leftX = GameConstants.LEFT_SIDE_OFFSET + xOffset;
            return new Vector3(leftX, rightPos.y, rightPos.z);
        }

        public void ClearSyncData()
        {
            _playerStateQueue.Clear();
            _objectSpawnQueue.Clear();
            _syncedObjects.Clear();
            _delayTimer = 0f;
        }
    }
}
