using UnityEngine;
using System.Collections.Generic;
namespace DavidsPrototype
{
    public class GameManager : MonoBehaviour
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
        [SerializeField] private float basicBulletDamage = 3f;

        [SerializeField] private int activePlayers = 1;
        [SerializeField] private int maxPlayers = 2;
        [SerializeField] private float playerBaseSpeed = 4.0f;
        [SerializeField] private int activeZombies = 0;
        [SerializeField] private int maxZombies = 64;
        [SerializeField] private float zombieBaseSpeed = 1.0f;
        [SerializeField] private float zombieBaseHealth = 10.0f;
        [SerializeField] private int activeBasicBullets = 0;
        [SerializeField] private int maxBasicBullets = 512;
        float bulletRadius = 0.5f;
        int enemyLayerMask = 1 << 3;

        [SerializeField] private int numOfZombiesSpawnAtOnce = 10; // n zombies spawns every waveDuration of seconds
        [SerializeField] private float waveDuration = 60.0f; // seconds
        [SerializeField] private float spawnRange = 10.0f;
        private float nextZombieWaveSpawnInSeconds = 0.0f;

        // BTW: these gun attributes probably belong in PlayerInputs.cs (so players can have different basic weapons??)
        [SerializeField] private float gunCycleTime = 0.2f;
        float spread = 0f; // in degrees

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
                zombieInfo.gameObjects[i].GetComponent<ZombieInfoContainer>().index = i;
                zombieInfo.gameObjects[i].SetActive(false);
                zombieInfo.health.Add(zombieBaseHealth);
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

                playerInfo.shotCooldowns[0] = playerInfo.shotCooldowns[0] < 0f ? 0f : playerInfo.shotCooldowns[0] - Time.fixedDeltaTime;
                if (playerInfo.playersWishToFire[i] && playerInfo.shotCooldowns[0] <= 0f)
                {
                    playerInfo.shotCooldowns[0] = gunCycleTime;
                    GameObject bullet = basicBullets[activeBasicBullets];
                    Vector3 offset = playerInfo.playerHandOffset[i] + playerInfo.weaponOffset[i] + playerInfo.weaponShootOffset[i];
                    // BUG: when shooting diagonally, the bullet comes out more toward the size compared to when the player is shooting straight
                    // Janky way to place bullet to face where the player is
                    offset.x *= wishDirection.x;
                    offset.y *= wishDirection.y;
                    offset.z *= wishDirection.z;

                    bullet.transform.rotation = playerInfo.gameObjects[i].transform.rotation;
                    bullet.transform.Rotate(0f, Random.Range(-1f, 1f) * spread, 0f);
                    bullet.transform.position = playerInfo.gameObjects[i].transform.position + offset;
                    bullet.SetActive(true);
                    activeBasicBullets++;
                }
            }

            if (nextZombieWaveSpawnInSeconds < 0.0f)
            {
                SpawnZombiesRandomly();
                nextZombieWaveSpawnInSeconds = waveDuration;
            }
            nextZombieWaveSpawnInSeconds -= Time.fixedDeltaTime;
            for (int i = 0; i < zombieInfo.activeZombies; i++)
            {
                zombieInfo.wishDirections[i] = (playerInfo.gameObjects[0].transform.position - zombieInfo.gameObjects[i].transform.position).normalized;
                zombieInfo.gameObjects[i].transform.position += (zombieInfo.baseMovementSpeed + zombieInfo.bonusSpeed[i]) * Time.fixedDeltaTime * zombieInfo.wishDirections[i].normalized;
            }

            for (int i = 0; i < activeBasicBullets; i++)
            {
                RaycastHit hit;
                Physics.SphereCast(basicBullets[i].transform.position, bulletRadius, basicBullets[i].transform.forward, out hit, basicBulletSpeed * Time.fixedDeltaTime, enemyLayerMask);
                if (hit.collider)
                {
                    int indexOfCollidedZombie = hit.collider.gameObject.GetComponent<ZombieInfoContainer>().index;
                    zombieInfo.health[indexOfCollidedZombie] -= basicBulletDamage;
                    if (zombieInfo.health[indexOfCollidedZombie] <= 0f)
                    {
                        zombieInfo.gameObjects[indexOfCollidedZombie].SetActive(false);
                        zombieInfo.health[indexOfCollidedZombie] = zombieBaseHealth;
                    }
                    basicBullets[i].SetActive(false);
                }
                else
                {
                    basicBullets[i].transform.position += basicBulletSpeed * Time.fixedDeltaTime * basicBullets[i].transform.forward;
                }
            }
        }
        private void SpawnZombiesRandomly()
        {
            for (int i = 0; i < numOfZombiesSpawnAtOnce; i++)
            {
                if (zombieInfo.activeZombies >= zombieInfo.maxZombies)
                    break;
                int index = zombieInfo.activeZombies;
                zombieInfo.activeZombies++; // Update the zombie count
                zombieInfo.bonusSpeed[index] = 0.0f; // Resets its speed
                zombieInfo.health[index] = zombieBaseHealth;

                float angleRad = Random.Range(0.0f, 2 * Mathf.PI);
                float randX = spawnRange * Mathf.Cos(angleRad) + cam.transform.position.x;
                float randY = spawnRange * Mathf.Sin(angleRad) + cam.transform.position.z;
                Vector3 randPosition = new Vector3(randX, 0.0f, randY);
                GameObject currentObject = zombieInfo.gameObjects[index];
                currentObject.transform.position = randPosition;
                currentObject.SetActive(true);
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
