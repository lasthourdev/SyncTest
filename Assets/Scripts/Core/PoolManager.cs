using System.Collections.Generic;
using UnityEngine;

namespace milan.Core
{
    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [SerializeField] private Obstacle obstaclePrefab;
        [SerializeField] private Collectible orbPrefab;

        private Queue<Obstacle> rightObstaclePool = new Queue<Obstacle>();
        private Queue<Obstacle> leftObstaclePool = new Queue<Obstacle>();

        private Queue<Collectible> rightOrbPool = new Queue<Collectible>();
        private Queue<Collectible> leftOrbPool = new Queue<Collectible>();

        private Dictionary<int, Obstacle> activeRightObstacles = new Dictionary<int, Obstacle>();
        private Dictionary<int, Obstacle> activeLeftObstacles = new Dictionary<int, Obstacle>();

        private Dictionary<int, Collectible> activeRightOrbs = new Dictionary<int, Collectible>();
        private Dictionary<int, Collectible> activeLeftOrbs = new Dictionary<int, Collectible>();

        private int nextObjectID = 1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            WarmPool();
        }

        private void WarmPool()
        {
            int initialCount = 100;

            for (int i = 0; i < initialCount; i++)
            {
                CreateObstacle(true);
                CreateObstacle(false);
                CreateOrb(true);
                CreateOrb(false);
            }
        }

        private Obstacle CreateObstacle(bool isRight)
        {
            Obstacle obs = Instantiate(obstaclePrefab, transform);
            obs.gameObject.SetActive(false);
            obs.SetInstanceID(0);

            if (isRight)
                rightObstaclePool.Enqueue(obs);
            else
                leftObstaclePool.Enqueue(obs);

            return obs;
        }

        private Collectible CreateOrb(bool isRight)
        {
            Collectible orb = Instantiate(orbPrefab, transform);
            orb.gameObject.SetActive(false);
            orb.SetInstanceID(0);

            if (isRight)
                rightOrbPool.Enqueue(orb);
            else
                leftOrbPool.Enqueue(orb);

            return orb;
        }

        public Obstacle GetObstacle(bool isRight)
        {
            Obstacle obs = null;
            if (isRight)
            {
                if (rightObstaclePool.Count > 0)
                    obs = rightObstaclePool.Dequeue();
                else
                    obs = CreateObstacle(true);

                obs.SetInstanceID(nextObjectID++);
                activeRightObstacles[obs.GetInstanceID()] = obs;
            }
            else
            {
                if (leftObstaclePool.Count > 0)
                    obs = leftObstaclePool.Dequeue();
                else
                    obs = CreateObstacle(false);

                obs.SetInstanceID(nextObjectID++);
                activeLeftObstacles[obs.GetInstanceID()] = obs;
            }

            obs.gameObject.SetActive(true);
            return obs;
        }

        public Collectible GetOrb(bool isRight)
        {
            Collectible orb = null;
            if (isRight)
            {
                if (rightOrbPool.Count > 0)
                    orb = rightOrbPool.Dequeue();
                else
                    orb = CreateOrb(true);

                orb.SetInstanceID(nextObjectID++);
                activeRightOrbs[orb.GetInstanceID()] = orb;
            }
            else
            {
                if (leftOrbPool.Count > 0)
                    orb = leftOrbPool.Dequeue();
                else
                    orb = CreateOrb(false);

                orb.SetInstanceID(nextObjectID++);
                activeLeftOrbs[orb.GetInstanceID()] = orb;
            }

            orb.gameObject.SetActive(true);
            return orb;
        }

        public void ReturnObstacle(Obstacle obs)
        {
            if (obs == null)
                return;

            int id = obs.GetInstanceID();

            if (obs.IsRightSide())
            {
                if (activeRightObstacles.ContainsKey(id))
                    activeRightObstacles.Remove(id);

                obs.gameObject.SetActive(false);
                rightObstaclePool.Enqueue(obs);
            }
            else
            {
                if (activeLeftObstacles.ContainsKey(id))
                    activeLeftObstacles.Remove(id);

                obs.gameObject.SetActive(false);
                leftObstaclePool.Enqueue(obs);
            }
        }

        public void ReturnOrb(Collectible orb)
        {
            if (orb == null)
                return;

            int id = orb.GetInstanceID();

            if (orb.IsRightSide())
            {
                if (activeRightOrbs.ContainsKey(id))
                    activeRightOrbs.Remove(id);

                orb.gameObject.SetActive(false);
                rightOrbPool.Enqueue(orb);
            }
            else
            {
                if (activeLeftOrbs.ContainsKey(id))
                    activeLeftOrbs.Remove(id);

                orb.gameObject.SetActive(false);
                leftOrbPool.Enqueue(orb);
            }
        }

        public void ReturnAllObjects()
        {
            foreach (var kvp in activeRightObstacles)
                ReturnObstacle(kvp.Value);
            foreach (var kvp in activeLeftObstacles)
                ReturnObstacle(kvp.Value);
            foreach (var kvp in activeRightOrbs)
                ReturnOrb(kvp.Value);
            foreach (var kvp in activeLeftOrbs)
                ReturnOrb(kvp.Value);

            activeRightObstacles.Clear();
            activeLeftObstacles.Clear();
            activeRightOrbs.Clear();
            activeLeftOrbs.Clear();
        }
    }
}
