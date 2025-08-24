using UnityEngine;
using System.Collections.Generic;
using TMPro;
namespace DavidsPrototype
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject zombiePrefab;
        [SerializeField] private GameObject basicBulletPrefab;
        [SerializeField] private GameObject basicBalloonPrefab;
        [SerializeField] private GameObject winUI;
        [SerializeField] private GameObject loseUI;
        [SerializeField] private TMP_Text balloonCounter;
        [SerializeField] private TMP_Text playerHealthUI;

        private PlayerInfo playerInfo;
        private ZombieInfo zombieInfo;
        private BalloonInfo balloonInfo;
        private Camera cam;
        private Vector3 cameraOffset;
        private List<GameObject> basicBullets;

        [SerializeField] private float basicBulletSpeed = 1.0f;
        [SerializeField] private float basicBulletDamage = 3f;

        [SerializeField] private int activePlayers = 1;
        [SerializeField] private int maxPlayers = 2;
        [SerializeField] private float playerBaseSpeed = 4.0f;
        [SerializeField] private float playerBaseHealth = 10.0f;
        [SerializeField] private int activeZombies = 0;
        [SerializeField] private int maxZombies = 64;
        [SerializeField] private float zombieBaseSpeed = 1.0f;
        [SerializeField] private float zombieBaseHealth = 10.0f;
        [SerializeField] private float zombieBaseDamage = 2.0f;
        [SerializeField] private int activeBasicBullets = 0;
        [SerializeField] private int maxBasicBullets = 512;
        float bulletRadius = 0.5f;
        int enemyLayerMask = 1 << 3;

        [SerializeField] private int activeBalloons;
        [SerializeField] private int maxBalloons;
        [SerializeField] private int numOfBalloonSpawnAtOnce;
        [SerializeField] private float balloonWaveDuration;
        [SerializeField] private float balloonSpawnRange;
        [SerializeField] private float balloonYOffset;
        [SerializeField] private int winWhenCollectedNBalloon = 2;
        [SerializeField] private float balloonCollectionSqrRadius = 8.0f;

        [SerializeField] private int numOfZombiesSpawnAtOnce = 10; // n zombies spawns every waveDuration of seconds
        [SerializeField] private float waveDuration = 60.0f; // seconds
        [SerializeField] private float spawnRange = 10.0f;
        private float nextZombieWaveSpawnInSeconds = 0.0f;
        float playerMaxDamageCooldown = 1.5f;

        // BTW: these gun attributes probably belong in PlayerInputs.cs (so players can have different basic weapons??)
        [SerializeField] private float gunCycleTime = 0.2f;
        float spread = 0f; // in degrees


        private void Start()
        {
            winUI.SetActive(false);
            loseUI.SetActive(false);
            playerInfo = new PlayerInfo(activePlayers, maxPlayers);
            basicBullets = new List<GameObject>(maxBasicBullets);

            for (int i = 0; i < maxPlayers; i++)
            {
                playerInfo.gameObjects.Add(Instantiate(playerPrefab));
                playerInfo.gameObjects[i].SetActive(false);
                playerInfo.health.Add(playerBaseHealth);
                playerInfo.receiveDamageCooldown.Add(0f);
            }

            zombieInfo = new ZombieInfo(activeZombies, maxZombies);
            for (int i = 0; i < maxZombies; i++)
            {
                zombieInfo.gameObjects.Add(Instantiate(zombiePrefab));
                zombieInfo.gameObjects[i].GetComponent<ZombieInfoContainer>().index = i;
                zombieInfo.gameObjects[i].SetActive(false);
                zombieInfo.health.Add(zombieBaseHealth);
            }
            balloonInfo = new BalloonInfo(activeBalloons, maxBalloons);
            balloonInfo.balloonWaveDuration = balloonWaveDuration;
            balloonInfo.balloonSpawnRange = balloonSpawnRange;
            balloonInfo.numOfBalloonSpawnAtOnce = this.numOfBalloonSpawnAtOnce;
            balloonInfo.spawnYOffset = balloonYOffset;
            balloonInfo.collectionSqrRadius = balloonCollectionSqrRadius;
            for (int i = 0; i < maxBalloons; i++)
            {
                balloonInfo.gameObjects.Add(Instantiate(basicBalloonPrefab));
                balloonInfo.gameObjects[i].SetActive(false);
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

            balloonCounter.text = "Balloons: 0";
            playerHealthUI.text = "Health: " + playerInfo.health[0];
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
                if (!playerInfo.gameObjects[i].activeSelf)
                {
                    continue;
                }
                if (playerInfo.receiveDamageCooldown[i] > 0f)
                    playerInfo.receiveDamageCooldown[i] -= Time.fixedDeltaTime;
                if (playerInfo.receiveDamageCooldown[i] <= 0f)
                {
                    Collider[] playerCollisions = Physics.OverlapSphere(playerInfo.gameObjects[i].transform.position, 0.6f);
                    foreach (Collider c in playerCollisions)
                    {
                        if (c.tag == "Enemy")
                        {
                            playerInfo.health[i] -= zombieBaseDamage;
                            print("player " + (i + 1) + " took " + zombieBaseDamage + " damage");
                            playerHealthUI.text = "Health: " + playerInfo.health[0];

                            if (playerInfo.health[i] <= 0f)
                            {
                                // the player is now dead, we should check if all active players are dead at this point
                                // TODO: elaborate on death handling (e.g. if(everyone is dead){gameover;} )
                                playerInfo.gameObjects[i].SetActive(false);
                                loseUI.SetActive(true);
                            }
                            playerInfo.receiveDamageCooldown[i] = playerMaxDamageCooldown;
                            break;
                        }
                    }
                }

                Vector3 wishDirection = playerInfo.playersWishDirection[i].normalized;
                // TODO: Change this to rigidbody so player can collide to walls properly (if we're adding walls in the future)
                // playerInfo.gameObjects[i].transform.position += (playerInfo.baseMovementSpeed + playerInfo.bonusSpeed[i]) * Time.fixedDeltaTime * wishDirection;
                // ^^^ DONE vvv
                playerInfo.gameObjects[i].GetComponent<Rigidbody>().velocity = (playerInfo.baseMovementSpeed + playerInfo.bonusSpeed[i]) * wishDirection;
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
                // zombieInfo.gameObjects[i].transform.position += (zombieInfo.baseMovementSpeed + zombieInfo.bonusSpeed[i]) * Time.fixedDeltaTime * zombieInfo.wishDirections[i].normalized;
                // ^ v   replaced the movement mode so that zombies collide with each other
                zombieInfo.gameObjects[i].GetComponent<Rigidbody>().velocity = (zombieInfo.baseMovementSpeed + zombieInfo.bonusSpeed[i]) * zombieInfo.wishDirections[i].normalized;
            }
            if (balloonInfo.nextBalloonWaveSpawnInSeconds < 0.0f)
            {
                SpawnBalloonRandomly();
                balloonInfo.nextBalloonWaveSpawnInSeconds = balloonInfo.balloonWaveDuration;
            }
            balloonInfo.nextBalloonWaveSpawnInSeconds -= Time.fixedDeltaTime;

            for (int i = 0; i < activeBasicBullets; i++)
            {
                if (!basicBullets[i].activeSelf)
                    continue;

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

            for (int i = 0; i < balloonInfo.activeBalloon; i++)
            {
                for (int playerIndex = 0; playerIndex < playerInfo.activePlayers; playerIndex++)
                {
                    if (Vector3.SqrMagnitude(playerInfo.gameObjects[playerIndex].transform.position - balloonInfo.gameObjects[i].transform.position) < balloonInfo.collectionSqrRadius)
                    {
                        balloonInfo.gameObjects[i].transform.position = balloonInfo.gameObjects[balloonInfo.activeBalloon - 1].transform.position;
                        balloonInfo.gameObjects[balloonInfo.activeBalloon - 1].SetActive(false);
                        playerInfo.balloonsCollected++;
                        balloonInfo.activeBalloon--;
                        balloonCounter.text = "Balloons: " + playerInfo.balloonsCollected;
                        if (playerInfo.balloonsCollected == winWhenCollectedNBalloon)
                        {
                            // Player wins
                            winUI.SetActive(true);
                        }
                        break;
                    }
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
                // NOTE: spawnYOffset is there to prevent zombie being stuck between the ground
                Vector3 randPosition = new Vector3(randX, zombieInfo.spawnYOffset, randY);
                GameObject currentObject = zombieInfo.gameObjects[index];
                currentObject.transform.position = randPosition;
                currentObject.SetActive(true);
            }
        }
        private void SpawnBalloonRandomly()
        {
            for (int i = 0; i < balloonInfo.numOfBalloonSpawnAtOnce; i++)
            {
                if (balloonInfo.activeBalloon >= balloonInfo.maxBalloon) break;
                int index = balloonInfo.activeBalloon;
                balloonInfo.activeBalloon++;

                float angleRad = Random.Range(0.0f, 2 * Mathf.PI);
                float randX = spawnRange * Mathf.Cos(angleRad) + cam.transform.position.x;
                float randY = spawnRange * Mathf.Sin(angleRad) + cam.transform.position.z;
                // NOTE: spawnYOffset is there to prevent balloon being stuck between the ground
                Vector3 randPosition = new Vector3(randX, balloonInfo.spawnYOffset, randY);
                GameObject currentObject = balloonInfo.gameObjects[index];
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
