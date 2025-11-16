using UnityEngine;

namespace milan.Core
{
    public struct PlayerStateData
    {
        public float Timestamp;
        public Vector3 Position;
        public Lane CurrentLane;
        public bool IsJumping;
        public bool IsGrounded;
        public float JumpProgress;
        public int CollectibleID;
        public int ObstacleHitID;

        public PlayerStateData(float timestamp, Vector3 position, Lane lane, bool isJumping,
            bool isGrounded, float jumpProgress, int collectibleID = -1, int obstacleHitID = -1)
        {
            Timestamp = timestamp;
            Position = position;
            CurrentLane = lane;
            IsJumping = isJumping;
            IsGrounded = isGrounded;
            JumpProgress = jumpProgress;
            CollectibleID = collectibleID;
            ObstacleHitID = obstacleHitID;
        }
    }
}
