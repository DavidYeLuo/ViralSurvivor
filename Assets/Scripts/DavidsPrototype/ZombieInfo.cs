using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DavidsPrototype
{
    struct ZombieInfo
    {
        public int activeZombies;
        public int maxZombies;
        public float baseMovementSpeed;
        public float spawnYOffset;
        public List<GameObject> gameObjects;
        public List<Vector3> wishDirections;
        public List<float> health;
        public List<float> bonusSpeed;
        public ZombieInfo(int activeZombies, int maxZombies)
        {
            spawnYOffset = 0.5f; // NOTE: this fixes the bug where the zombie is stuck between the ground
            baseMovementSpeed = 1.0f;
            this.activeZombies = activeZombies;
            this.maxZombies = maxZombies;

            gameObjects = new List<GameObject>(maxZombies);
            wishDirections = new List<Vector3>(maxZombies);
            health = new List<float>(maxZombies);
            bonusSpeed = new List<float>(maxZombies);

            for (int i = 0; i < maxZombies; i++)
            {
                wishDirections.Add(Vector3.zero);
                bonusSpeed.Add(0.0f);
            }
        }
    }
}
