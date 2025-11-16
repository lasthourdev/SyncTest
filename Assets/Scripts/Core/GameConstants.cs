using System;
using UnityEngine;

namespace milan.Core
{
    public static class GameConstants
    {
        internal const float BASE_MOVE_SPEED = 10;

        internal const float JUMP_HEIGHT = 4f;
        internal const float JUMP_DURATION = 0.6f;
        internal const float JUMP_FORWARD_BOOST = 1f;
        internal const float GRAVITY_MULTIPLIER = 2.5f;

        internal const float LANE_DISTANCE = 2.5f;
        internal const float LANE_SWITCH_SPEED = 12f;

        internal const float RIGHT_SIDE_OFFSET = 3.75f;
        internal const float LEFT_SIDE_OFFSET = -3.75f;

        internal const int PLAYER_LIVES = 5;
        internal const float SECTION_DISTANCE = 25f;
        internal const float DESPAWN_DISTANCE = -10f;
        internal const float MOVING_OBSTACLE_SPEED = 5f;
    }

    public enum Lane
    {
        Left = 0,
        Center = 1,
        Right = 2
    }


    public enum GameState
    {
        MainMenu,
        Playing,
        GameOver,
    }

    public enum PlayerSide
    {
        Right,
        Left
    }


}
