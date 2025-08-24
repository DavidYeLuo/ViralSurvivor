using UnityEngine;
using System.Collections.Generic;
namespace DavidsPrototype
{
    struct BalloonInfo
    {
        public List<GameObject> gameObjects;
        public int numOfBalloonSpawnAtOnce;
        public float balloonWaveDuration;
        public float balloonSpawnRange;
        public int activeBalloon;
        public int maxBalloon;
        public float spawnYOffset;
        public float collectionSqrRadius;

        public float nextBalloonWaveSpawnInSeconds;
        public BalloonInfo(int activeBalloon, int maxBalloon)
        {
            gameObjects = new List<GameObject>(maxBalloon);
            this.activeBalloon = activeBalloon;
            this.maxBalloon = maxBalloon;
            balloonWaveDuration = 60.0f;
            balloonSpawnRange = 10.0f;
            nextBalloonWaveSpawnInSeconds = 0.0f;
            numOfBalloonSpawnAtOnce = 2;
            spawnYOffset = 0.5f;
            collectionSqrRadius = 8.0f;
        }
    }
}
