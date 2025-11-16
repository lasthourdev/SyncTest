using UnityEngine;
using iPAHeartBeat.Core.Dependency;
using milan_UI;

namespace milan.Core
{
    public class GameManager : MonoBehaviour
    {
        internal SyncManager syncManager;
        public GameState _currentState = GameState.MainMenu;
        internal int _currentScore;
        private int _orbsCollected;
        private int _livesRemaining;
        private float _distanceTraveled;
        internal int _bestScore;
        public GameState CurrentState => _currentState;
        public int CurrentScore => _currentScore;


        private void Awake()
        {
            Application.targetFrameRate = 60;
            DependencyResolver.Register<GameManager>(this);
            _bestScore = PlayerPrefs.GetInt("BestScore", 0);
        }

        void Start()
        {
            syncManager ??= DependencyResolver.Resolve<SyncManager>();
            UIManager.Instance.Show<MainMenuPanel>();
        }

        private void OnEnable()
        {
            GameEvents.OnGameStart += HandleGameStart;
            GameEvents.OnGameOver += HandleGameOver;
            GameEvents.OnGameRestart += HandleGameRestart;
            GameEvents.OnMainMenu += HandleMainMenu;
            GameEvents.OnScoreChanged += HandleScoreChanged;
            GameEvents.OnLifeChanged += HandleLifeChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStart -= HandleGameStart;
            GameEvents.OnGameOver -= HandleGameOver;
            GameEvents.OnGameRestart -= HandleGameRestart;
            GameEvents.OnMainMenu -= HandleMainMenu;
            GameEvents.OnScoreChanged -= HandleScoreChanged;
            GameEvents.OnLifeChanged -= HandleLifeChanged;
        }


        private void Update()
        {
            if (_currentState != GameState.Playing) return;
            UpdateDistance();
            UpdateUI();
        }

        private void HandleGameStart()
        {
            _currentState = GameState.Playing;
            _currentScore = 0;
            _orbsCollected = 0;
            _livesRemaining = GameConstants.PLAYER_LIVES;
            _distanceTraveled = 0f;
            UpdateUI();
            GameEvents.TriggerScoreChanged(_currentScore);
            GameEvents.TriggerLifeChanged(_livesRemaining);
        }

        private void HandleGameOver()
        {
            _currentState = GameState.GameOver;
            SaveBestScore();
            UIManager.Instance.Show<GameOverPanel>();
        }

        private void HandleGameRestart()
        {
            _currentState = GameState.MainMenu;
            UIManager.Instance.Show<MainMenuPanel>();
            GameEvents.TriggerGameStart();
        }

        private void HandleMainMenu()
        {
            _currentState = GameState.MainMenu;
            UIManager.Instance.Show<MainMenuPanel>();
        }

        private void HandleScoreChanged(int newScore)
        {
            _currentScore = newScore;
            if (_currentScore > _bestScore)
            {
                _bestScore = _currentScore;
            }
        }

        private void HandleLifeChanged(int newLives)
        {
            _livesRemaining = newLives;
        }

        public void LoseLife()
        {
            // Debug.Log($"Lose: {_livesRemaining}");

            _livesRemaining--;
            GameEvents.TriggerLifeChanged(_livesRemaining);

            if (_livesRemaining <= 0)
            {
                GameEvents.TriggerGameOver();
            }
        }


        private void UpdateDistance()
        {
            _distanceTraveled += GameConstants.BASE_MOVE_SPEED * Time.deltaTime;
        }

        public void AddScore()
        {
            _currentScore += 1;
            GameEvents.TriggerScoreChanged(_currentScore);
        }

        private void SaveBestScore()
        {
            PlayerPrefs.SetInt("BestScore", _bestScore);
            PlayerPrefs.Save();
        }

        private void UpdateUI()
        {
            GamePanel gamePanel = UIManager.Instance.GetActivePanel<GamePanel>();
            if (gamePanel != null)
            {
                gamePanel._currentScoreTMP.text = $"Score: {_currentScore}";
                gamePanel._bestScoreTMP.text = $"Best: {_bestScore}";
                gamePanel._currentHealthTMP.text = $"Lives: {_livesRemaining}";
            }
        }
    }
}
