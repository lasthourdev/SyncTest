using iPAHeartBeat.Core.Dependency;
using UnityEngine;

namespace milan.Core
{
    public class Collectible : MonoBehaviour
    {
        GameManager gameManager;
        [SerializeField] private float _rotationSpeed = 90f;
        [SerializeField] private float _bobHeight = 0.3f;
        [SerializeField] private float _bobSpeed = 2f;

        private int _instanceID;
        private bool _collected;
        private float _startY;

        private void OnEnable()
        {
            _instanceID = 0;
            _collected = false;
            _startY = transform.position.y;
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

            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);

            float newY = _startY + Mathf.Sin(Time.time * _bobSpeed) * _bobHeight;
            Vector3 pos = transform.position;
            pos.y = newY;
            transform.position = pos;
        }

        public void OnPlayerCollect(PlayerController player)
        {
            if (_collected) return;
         
            _collected = true;
            GameEvents.TriggerOrbCollected(transform.position);
            player.NotifyOrbCollected(_instanceID);

            if (player._playerSide == PlayerSide.Left) return;
            gameManager.AddScore();
            gameObject.SetActive(false);
        }

        private void ReturnToPool()
        {
            _collected = false;
            PoolManager.Instance.ReturnOrb(this);
        }

        public void SetInstanceID(int id)
        {
            _instanceID = id;
        }

        public int GetInstanceID()
        {
            return _instanceID;
        }

        public bool IsRightSide()
        {
            return gameObject.layer == LayerMask.NameToLayer("RightObjects");
        }
    }
}
