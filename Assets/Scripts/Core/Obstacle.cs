
using iPAHeartBeat.Core.Dependency;
using UnityEngine;



namespace milan.Core
{
    public class Obstacle : MonoBehaviour
    {
        GameManager gameManager;
        private int _instanceID;
        private bool _hasCollided;

        private bool _isMoving;
        private float _moveDirection = 1f;
        private float _minX;
        private float _maxX;

        private void OnEnable()
        {
            _hasCollided = false;
        }

        void Start()
        {
            gameManager ??= DependencyResolver.Resolve<GameManager>();
        }

        private void Update()
        {
            if (transform.position.z < GameConstants.DESPAWN_DISTANCE)
            {
                ReturnToPool();
                return;
            }

            if (_isMoving)
            {
                UpdateMovement();
            }
        }

        public void SetAsMoving(float sideOffset)
        {
            _isMoving = true;
            _minX = sideOffset - GameConstants.LANE_DISTANCE;
            _maxX = sideOffset + GameConstants.LANE_DISTANCE;
            _moveDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
        }

        public void SetAsStatic()
        {
            _isMoving = false;
        }

        private void UpdateMovement()
        {
            float currentX = transform.position.x;
            currentX += _moveDirection * GameConstants.MOVING_OBSTACLE_SPEED * Time.deltaTime;

            if (currentX <= _minX)
            {
                currentX = _minX;
                _moveDirection = 1f;
            }
            else if (currentX >= _maxX)
            {
                currentX = _maxX;
                _moveDirection = -1f;
            }

            transform.position = new Vector3(currentX, transform.position.y, transform.position.z);
        }

        public void OnPlayerHit(PlayerController player)
        {
            if (_hasCollided) return;

            //Debug.LogError($"HasCollided {player._playerSide}");
            _hasCollided = true;

            GameEvents.TriggerObstacleHit(transform.position);
            player.NotifyObstacleHit(_instanceID);
            gameManager.LoseLife();

            gameObject.SetActive(false);
        }


        private void ReturnToPool()
        {
            _isMoving = false;
            _hasCollided = false;
            gameManager.syncManager.UnregisterSyncedObject(_instanceID);
            PoolManager.Instance.ReturnObstacle(this);
        }

        public void SetInstanceID(int id) { _instanceID = id; }
        public int GetInstanceID() { return _instanceID; }
        public bool IsRightSide() { return gameObject.layer == LayerMask.NameToLayer("RightObjects"); }
    }
}
