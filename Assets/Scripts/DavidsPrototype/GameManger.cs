using UnityEngine;
using System.Collections.Generic;
namespace DavidsPrototype
{
    public class GameManger : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] private GameObject basicBulletPrefab;

        private PlayerInfo playerInfo;
        private ZombieInfo zombieInfo;
        private Camera cam;
        private Vector3 cameraOffset;
        private List<GameObject> basicBullets;

        [SerializeField] private float basicBulletSpeed = 1.0f;

        [SerializeField] private int activePlayers = 1;
        [SerializeField] private int maxPlayers = 2;
        [SerializeField] private float playerBaseSpeed = 4.0f;
        [SerializeField] private int activeZombies = 0;
        [SerializeField] private int maxZombies = 64;
        [SerializeField] private float zombieBaseSpeed = 1.0f;
        [SerializeField] private int activeBasicBullets = 0;
        [SerializeField] private int maxBasicBullets = 512;

        private void Start()
        {
            playerInfo = new PlayerInfo(activePlayers, maxPlayers);
            basicBullets = new List<GameObject>(maxBasicBullets);

            for (int i = 0; i < maxPlayers; i++)
            {
                playerInfo.gameObjects.Add(Instantiate(playerPrefab));
                playerInfo.gameObjects[i].SetActive(false);
            }

            zombieInfo = new ZombieInfo(activeZombies, maxZombies);
            for (int i = 0; i < maxZombies; i++)
            {
                zombieInfo.gameObjects.Add(Instantiate(zombiePrefab));
                zombieInfo.gameObjects[i].SetActive(false);
            }
            for (int i = 0; i < maxBasicBullets; i++)
            {
                basicBullets.Add(Instantiate(basicBulletPrefab));
                basicBullets[i].SetActive(false);
            }
            for (int i = 0; i < activePlayers; i++)
            {
                playerInfo.gameObjects[i].SetActive(true);
            }
            for (int i = 0; i < activeZombies; i++)
            {
                zombieInfo.gameObjects[i].SetActive(true);
            }
            playerInfo.lefts[0] = KeyCode.A;
            playerInfo.rights[0] = KeyCode.D;
            playerInfo.downs[0] = KeyCode.S;
            playerInfo.ups[0] = KeyCode.W;
            playerInfo.fire[0] = KeyCode.J;
            cam = Camera.main;
            cameraOffset = cam.transform.position;

            zombieInfo.baseMovementSpeed = zombieBaseSpeed;
            playerInfo.baseMovementSpeed = playerBaseSpeed;
            playerInfo.weaponOffset[0] = new Vector3(0.0f, 0.0f, 1.0f);
        }

        private void Update()
        {
            ProcessPlayerInput();
        }
        private void LateUpdate()
        {
            Vector3 firstPlayerPosition = playerInfo.gameObjects[0].transform.position;
            cam.transform.position = cameraOffset + firstPlayerPosition;
        }
        private void FixedUpdate()
        {
            for (int i = 0; i < playerInfo.activePlayers; i++)
            {
                Vector3 wishDirection = playerInfo.playersWishDirection[i].normalized;
                // TODO: Change this to rigidbody so player can collide to walls properly (if we're adding walls in the future)
                playerInfo.gameObjects[i].transform.position += (playerInfo.baseMovementSpeed + playerInfo.bonusSpeed[i]) * Time.fixedDeltaTime * wishDirection;
                // This if statement fixes prevents player model to snap back to looking forward when player isn't pressing anything
                if (wishDirection.sqrMagnitude > 0.1f)
                    playerInfo.gameObjects[i].transform.rotation = Quaternion.LookRotation(wishDirection);

                if (playerInfo.playersWishToFire[i])
                {
                    GameObject bullet = basicBullets[activeBasicBullets];
                    Vector3 offset = playerInfo.playerHandOffset[i] + playerInfo.weaponOffset[i] + playerInfo.weaponShootOffset[i];
                    // BUG: when shooting diagonally, the bullet comes out more toward the size compared to when the player is shooting straight
                    // Janky way to place bullet to face where the player is
                    offset.x *= wishDirection.x;
                    offset.y *= wishDirection.y;
                    offset.z *= wishDirection.z;

                    bullet.transform.rotation = playerInfo.gameObjects[i].transform.rotation;
                    bullet.transform.position = playerInfo.gameObjects[i].transform.position + offset;
                    bullet.SetActive(true);
                    activeBasicBullets++;
                }
            }
            for (int i = 0; i < zombieInfo.activeZombies; i++)
            {
                zombieInfo.wishDirections[i] = (playerInfo.gameObjects[0].transform.position - zombieInfo.gameObjects[i].transform.position).normalized;
                zombieInfo.gameObjects[i].transform.position += (zombieInfo.baseMovementSpeed + zombieInfo.bonusSpeed[i]) * Time.fixedDeltaTime * zombieInfo.wishDirections[i].normalized;
            }

            for (int i = 0; i < activeBasicBullets; i++)
            {
                basicBullets[i].transform.position += basicBulletSpeed * Time.fixedDeltaTime * basicBullets[i].transform.forward;
            }
        }
        private void ProcessPlayerInput()
        {
            for (int i = 0; i < activePlayers; i++)
            {
                float horizontal = 0.0f;
                float vertical = 0.0f;
                if (Input.GetKeyDown(playerInfo.lefts[i]))
                {
                    horizontal += -1.0f;
                }
                if (Input.GetKeyUp(playerInfo.lefts[i]))
                {
                    horizontal += 1.0f;
                }
                if (Input.GetKeyDown(playerInfo.rights[i]))
                {
                    horizontal += 1.0f;
                }
                if (Input.GetKeyUp(playerInfo.rights[i]))
                {
                    horizontal += -1.0f;
                }
                if (Input.GetKeyDown(playerInfo.downs[i]))
                {
                    vertical += -1.0f;
                }
                if (Input.GetKeyUp(playerInfo.downs[i]))
                {
                    vertical += 1.0f;
                }
                if (Input.GetKeyDown(playerInfo.ups[i]))
                {
                    vertical += 1.0f;
                }
                if (Input.GetKeyUp(playerInfo.ups[i]))
                {
                    vertical += -1.0f;
                }
                if (Input.GetKeyDown(playerInfo.fire[i]))
                {
                    playerInfo.playersWishToFire[i] = true;
                }
                if (Input.GetKeyUp(playerInfo.fire[i]))
                {
                    playerInfo.playersWishToFire[i] = false;
                }
                playerInfo.playersWishDirection[i] += new Vector3(horizontal, 0.0f, vertical);
            }
        }
    }
}
