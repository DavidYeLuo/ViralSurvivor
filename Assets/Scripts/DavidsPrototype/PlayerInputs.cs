using UnityEngine;
using System.Collections.Generic;
namespace DavidsPrototype
{
    struct PlayerInfo
    {
        public int activePlayers;
        public int maxPlayers;

        public List<GameObject> gameObjects;
        public List<Vector3> playersWishDirection;
        public List<bool> playersWishToFire;

        public List<Vector3> playerHandOffset;
        public List<Vector3> weaponOffset;
        public List<Vector3> weaponShootOffset;

        // Key Inputs
        public List<KeyCode> lefts;
        public List<KeyCode> rights;
        public List<KeyCode> downs;
        public List<KeyCode> ups;
        public List<KeyCode> fire;

        public float baseMovementSpeed;
        public List<float> bonusSpeed;

        public PlayerInfo(int activePlayers, int maxPlayers)
        {
            this.activePlayers = activePlayers;
            this.maxPlayers = maxPlayers;

            gameObjects = new List<GameObject>(maxPlayers);
            playersWishDirection = new List<Vector3>(maxPlayers);
            playersWishToFire = new List<bool>(maxPlayers);

            playerHandOffset = new List<Vector3>(maxPlayers);
            weaponOffset = new List<Vector3>(maxPlayers);
            weaponShootOffset = new List<Vector3>(maxPlayers);

            lefts = new List<KeyCode>(maxPlayers);
            rights = new List<KeyCode>(maxPlayers);
            downs = new List<KeyCode>(maxPlayers);
            ups = new List<KeyCode>(maxPlayers);
            fire = new List<KeyCode>(maxPlayers);

            baseMovementSpeed = 1.0f;
            bonusSpeed = new List<float>(maxPlayers);

            for (int i = 0; i < maxPlayers; i++)
            {
                playersWishDirection.Add(Vector3.zero);
                playersWishToFire.Add(false);

                playerHandOffset.Add(Vector3.zero);
                weaponOffset.Add(Vector3.zero);
                weaponShootOffset.Add(Vector3.zero);

                lefts.Add(KeyCode.None);
                rights.Add(KeyCode.None);
                downs.Add(KeyCode.None);
                ups.Add(KeyCode.None);
                fire.Add(KeyCode.None);

                bonusSpeed.Add(0.0f);
            }
        }
    }
}
