using System;
using UnityEngine;

namespace milan.Core
{
    public static class GameEvents
    {

        public static Action OnGameStart;
        public static Action OnGameOver;
        public static Action OnGameRestart;
        public static Action OnMainMenu;


        public static Action<Vector3> OnOrbCollected;
        public static Action<Vector3> OnObstacleHit;
        public static Action<int> OnScoreChanged;
        public static Action<int> OnLifeChanged;


        public static Action<PlayerSide> OnPlayerJump;
        public static Action<PlayerSide> OnPlayerLanded;


        public static void TriggerGameStart() => OnGameStart?.Invoke();
        public static void TriggerGameOver() => OnGameOver?.Invoke();

        public static void TriggerOrbCollected(Vector3 position) => OnOrbCollected?.Invoke(position);
        public static void TriggerObstacleHit(Vector3 position) => OnObstacleHit?.Invoke(position);  // Add this
        public static void TriggerScoreChanged(int score) => OnScoreChanged?.Invoke(score);
        public static void TriggerLifeChanged(int lives) => OnLifeChanged?.Invoke(lives);

        public static void TriggerPlayerJump(PlayerSide playerSide) => OnPlayerJump?.Invoke(playerSide);
        public static void TriggerPlayerLanded(PlayerSide playerSide) => OnPlayerLanded?.Invoke(playerSide);
    }
}
