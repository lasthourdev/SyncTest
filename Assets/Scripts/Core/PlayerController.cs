using iPAHeartBeat.Core.Dependency;
using UnityEngine;

namespace milan.Core
{
    public class PlayerController : MonoBehaviour
    {
        GameManager gameManager;
        public PlayerSide _playerSide;
        [SerializeField] private float _laneDistance = 2.5f;

        private Rigidbody _rigidbody;
        private int _currentLane = 1;
        private float _targetX;
        private float _sideOffset;

        private bool _isJumping;
        private float _jumpTimer;
        private float _yVelocity;

        private bool _isGhost;

        public PlayerSide Side => _playerSide;

        private ParticleSystem onColliedCollectible;
        private ParticleSystem onColliedObstacle;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            _isGhost = (_playerSide == PlayerSide.Left);

            if (_isGhost)
            {
                GetComponent<Collider>().isTrigger = true;
            }
            else
            {
                _rigidbody.isKinematic = false;
                Transform collectiblePE = transform.Find("collectiblePE");
                if (collectiblePE != null)
                    onColliedCollectible = collectiblePE.GetComponent<ParticleSystem>();
                Transform obstaclePE = transform.Find("obstaclePE");
                if (obstaclePE != null)
                    onColliedObstacle = obstaclePE.GetComponent<ParticleSystem>();
            }

            _sideOffset = _playerSide == PlayerSide.Right ? GameConstants.RIGHT_SIDE_OFFSET : GameConstants.LEFT_SIDE_OFFSET;
            _targetX = GetLaneX(_currentLane);
        }

        private void Start()
        {
            gameManager ??= DependencyResolver.Resolve<GameManager>();

            if (!_isGhost && SwipeDetector.Instance != null)
            {
                SwipeDetector.Instance.OnSwipeUp += Jump;
                SwipeDetector.Instance.OnSwipeLeft += MoveLeft;
                SwipeDetector.Instance.OnSwipeRight += MoveRight;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnGameStart += ResetPlayer;
            GameEvents.OnGameOver += StopPlayer;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= ResetPlayer;
            GameEvents.OnGameOver -= StopPlayer;

            if (!_isGhost && SwipeDetector.Instance != null)
            {
                SwipeDetector.Instance.OnSwipeUp -= Jump;
                SwipeDetector.Instance.OnSwipeLeft -= MoveLeft;
                SwipeDetector.Instance.OnSwipeRight -= MoveRight;
            }
        }

        private void FixedUpdate()
        {
            if (gameManager.CurrentState != GameState.Playing) return;

            if (_isGhost)
                UpdateGhost();
            else
                UpdatePlayer();
        }

        private void UpdatePlayer()
        {
            ProcessKeyboardInput();
            MoveForward();
            UpdateLanePosition();
            UpdateJump();
            ApplyGravity();
            BroadcastState();
        }

        private void UpdateGhost()
        {
            MoveForward();
            UpdateLanePosition();
            UpdateJump();
            ApplyGravity();
        }

        private void ProcessKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                MoveLeft();
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                MoveRight();
            else if (Input.GetKeyDown(KeyCode.Space))
                Jump();
        }

        private void MoveForward()
        {
            float speed = _isJumping ? GameConstants.BASE_MOVE_SPEED * GameConstants.JUMP_FORWARD_BOOST : GameConstants.BASE_MOVE_SPEED;
            Vector3 currentPos = _rigidbody.position;
            Vector3 targetPos = currentPos + new Vector3(0, 0, speed * Time.fixedDeltaTime);
            _rigidbody.MovePosition(targetPos);
        }

        private void UpdateLanePosition()
        {
            float currentX = _rigidbody.position.x;
            if (Mathf.Abs(currentX - _targetX) > 0.01f)
            {
                float newX = Mathf.MoveTowards(currentX, _targetX, GameConstants.LANE_SWITCH_SPEED * Time.fixedDeltaTime);
                Vector3 pos = _rigidbody.position;
                pos.x = newX;
                _rigidbody.MovePosition(pos);
            }
        }

        private void UpdateJump()
        {
            if (_isJumping)
            {
                _jumpTimer += Time.fixedDeltaTime;
                float progress = _jumpTimer / GameConstants.JUMP_DURATION;
                if (progress >= 1f)
                {
                    _isJumping = false;
                    _yVelocity = 0f;
                    GameEvents.TriggerPlayerLanded(_playerSide);
                }
                else
                {
                    float height = Mathf.Sin(progress * Mathf.PI) * GameConstants.JUMP_HEIGHT;
                    float targetY = 1f + height;
                    _yVelocity = (targetY - _rigidbody.position.y) / Time.fixedDeltaTime;
                }
            }
        }

        private void ApplyGravity()
        {
            if (!_isJumping)
            {
                if (_rigidbody.position.y > 1f)
                {
                    _yVelocity += Physics.gravity.y * GameConstants.GRAVITY_MULTIPLIER * Time.fixedDeltaTime;
                }
                else
                {
                    _yVelocity = 0f;
                    Vector3 pos = _rigidbody.position;
                    pos.y = 1f;
                    _rigidbody.MovePosition(pos);
                }
            }
            Vector3 velocity = _rigidbody.velocity;
            velocity.y = _yVelocity;
            _rigidbody.velocity = velocity;
        }

        private void MoveLeft()
        {
            if (_currentLane > 0)
            {
                _currentLane--;
                _targetX = GetLaneX(_currentLane);
            }
        }

        private void MoveRight()
        {
            if (_currentLane < 2)
            {
                _currentLane++;
                _targetX = GetLaneX(_currentLane);
            }
        }

        private void Jump()
        {
            if (!_isJumping)
            {
                _isJumping = true;
                _jumpTimer = 0f;
                GameEvents.TriggerPlayerJump(_playerSide);
            }
        }

        private float GetLaneX(int lane)
        {
            return _sideOffset + (lane - 1) * _laneDistance;
        }

        private void BroadcastState()
        {
            PlayerStateData state = new PlayerStateData(
                Time.time,
                _rigidbody.position,
                (Lane)_currentLane,
                _isJumping,
                !_isJumping,
                _isJumping ? (_jumpTimer / GameConstants.JUMP_DURATION) : 0f
            );
            gameManager.syncManager.BroadcastPlayerState(state);
        }

        public void SyncFromNetwork(PlayerStateData state)
        {
            if (_isGhost)
            {
                int newLane = (int)state.CurrentLane;
                if (newLane != _currentLane)
                {
                    _currentLane = newLane;
                    _targetX = GetLaneX(_currentLane);
                }

                if (state.IsJumping && !_isJumping)
                {
                    _isJumping = true;
                    _jumpTimer = state.JumpProgress * GameConstants.JUMP_DURATION;
                }
                else if (!state.IsJumping && _isJumping)
                {
                    _isJumping = false;
                }
            }
        }

        public void NotifyOrbCollected(int orbID)
        {
            if (_isGhost) return;

            PlayerStateData state = new PlayerStateData(
                Time.time,
                _rigidbody.position,
                (Lane)_currentLane,
                _isJumping,
                !_isJumping,
                _isJumping ? (_jumpTimer / GameConstants.JUMP_DURATION) : 0f,
                orbID
            );
            gameManager.syncManager.BroadcastPlayerState(state);
        }

        public void NotifyObstacleHit(int obstacleID)
        {
            if (_isGhost) return;

            PlayerStateData state = new PlayerStateData(
                Time.time,
                _rigidbody.position,
                (Lane)_currentLane,
                _isJumping,
                !_isJumping,
                _isJumping ? (_jumpTimer / GameConstants.JUMP_DURATION) : 0f,
                -1,
                obstacleID
            );
            gameManager.syncManager.BroadcastPlayerState(state);
        }

        private void ResetPlayer()
        {
            _currentLane = 1;
            _targetX = GetLaneX(1);
            _isJumping = false;
            _jumpTimer = 0f;
            _yVelocity = 0f;

            _rigidbody.velocity = Vector3.zero;
            _rigidbody.position = new Vector3(_targetX, 1f, 0f);
        }

        private void StopPlayer()
        {
            _yVelocity = 0f;
            _rigidbody.velocity = Vector3.zero;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_isGhost) return;

            if (collision.gameObject.TryGetComponent<Collectible>(out var collectible))
            {
                collectible.OnPlayerCollect(this);
                if (onColliedCollectible != null && !onColliedCollectible.isPlaying)
                {
                    onColliedCollectible.Play();
                }
            }
            else if (collision.gameObject.TryGetComponent<Obstacle>(out var obstacle))
            {
                obstacle.OnPlayerHit(this);
                if (onColliedObstacle != null && !onColliedObstacle.isPlaying)
                {
                    onColliedObstacle.Play();
                }
            }
        }
    }
}
