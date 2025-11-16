using UnityEngine;

namespace milan.Core
{
    [System.Serializable]
    public struct ObjectSpawnData
    {
        public int ObjectID;
        public ObjectType Type;
        public Vector3 Position;
        public Lane Lane;
        public bool IsMoving;
        public float SpawnTime;

        public ObjectSpawnData(int id, ObjectType type, Vector3 position, Lane lane, bool isMoving)
        {
            ObjectID = id;
            Type = type;
            Position = position;
            Lane = lane;
            IsMoving = isMoving;
            SpawnTime = Time.time;
        }
    }

    public enum ObjectType
    {
        Obstacle,
        Orb,
        AirOrb
    }
}
