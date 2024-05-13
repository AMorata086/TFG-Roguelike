using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

public class PlayerController : NetworkBehaviour
{
    // Player Stats
    [Header("Player Stats")]
    public string PlayerName;
    [SerializeField] private float movementSpeed = 3000f;
    [SerializeField] private int maxHealthPoints = 10;
    private int currentHealthPoints = 10;
    [SerializeField] private int damage = 2;
    public NetworkVariable<bool> CanMove = new NetworkVariable<bool>(false);
    [SerializeField] private float dodgeStrength = 750f;
    private float invulnerabilityTime = 0.25f;
    private float lastTimeGotHurt = 0f;

    // variables for player movement
    private Vector2 movementDirection;
    private Vector2 mousePosition;

    public Rigidbody2D Rb;

    public GameObject Hand;
    [SerializeField] private GameObject selfLight;
    [SerializeField] private GameObject hitbox;

    public GameObject ShootingPoint;
    public GameObject BulletPrefab;
    private string bulletTag = "";
    public float BulletSpeed = 1000f;
    public ParticleSystem MuzzleFlash;

    [SerializeField] private ParticleSystem deathVFX;
    private DamageEffect damageVFX;

    public Camera PlayerCamera;
    // fields for camera movement relative to the player position
    private Vector3 cameraTargetPosition = new Vector3();
    public float cameraThreshold;

    [SerializeField] private Animator animator;

    private InputActions playerControls;

    // Cooldown controls
    [SerializeField] private float dodgeCooldown = 2f;
    private float lastDodgeTime = 0f;
    [SerializeField] private float shootingCooldown = 0.5f;
    private float lastShotTime = 0f;

    [SerializeField] private InGameUIScript interfaceScript;
    [SerializeField] private GameObject pauseMenu;

    [SerializeField] private PlayerSpritesReferences playerSpritesReferences;

    [SerializeField] private List<Vector3> spawnPoints;

    private void SetPlayerReady(InputAction.CallbackContext context)
    {
        SynchronizeCanMoveServerRpc();
        playerControls.Player.Dodge.performed -= SetPlayerReady;
        playerControls.Player.Dodge.performed += Dodge;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SynchronizeCanMoveServerRpc()
    {
        CanMove.Value = true;
    }

    private void Dodge(InputAction.CallbackContext context)
    {
        if ((Time.time - lastDodgeTime) < dodgeCooldown || !CanMove.Value || !IsOwner)
        {
            return;
        }
        Rb.AddForce(Rb.velocity.normalized * dodgeStrength * Time.fixedDeltaTime, ForceMode2D.Impulse);
        lastDodgeTime = Time.time;
    }

    private void TogglePauseMenu(InputAction.CallbackContext context)
    {
        PauseMenu pauseMenuScript = pauseMenu.GetComponent<PauseMenu>();
        if(pauseMenuScript.IsPaused)
        {
            pauseMenuScript.ResumeGame();
        }
        else
        {
            pauseMenuScript.OpenPauseMenu();
        }
        
    }

    private void Shoot()
    {
        if ((Time.time - lastShotTime) < shootingCooldown)
        {
            return;
        }
        SpawnBulletServerRpc(bulletTag);
        lastShotTime = Time.time;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnBulletServerRpc(string bulletOwnerTag)
    {
        //ParticleSystem MuzzleFlashGameObject = ParticleSystem.Instantiate(MuzzleFlash, ShootingPoint.transform.position, ShootingPoint.transform.rotation);
        //NetworkObject MuzzleFlashGameObjectNetorkObject = MuzzleFlashGameObject.GetComponent<NetworkObject>();
        //MuzzleFlashGameObjectNetorkObject.Spawn(true);
        SpawnShootingAudioAndVisualCuesClientRpc();
        GameObject bullet = Instantiate(BulletPrefab, ShootingPoint.transform.position, ShootingPoint.transform.rotation);
        NetworkObject bulletNetworkObject = bullet.GetComponent<NetworkObject>();
        bulletNetworkObject.Spawn(true);
        PlayerBulletScript bulletGameObjectPlayerBulletScript = bullet.GetComponent<PlayerBulletScript>();
        bulletGameObjectPlayerBulletScript.Damage = damage;
        bulletGameObjectPlayerBulletScript.ChangeBulletSpriteAndMaterial(bulletTag);
        bullet.GetComponent<Rigidbody2D>().AddForce(ShootingPoint.transform.right * BulletSpeed * Time.deltaTime, ForceMode2D.Impulse);
    }

    [ClientRpc]
    private void SpawnShootingAudioAndVisualCuesClientRpc()
    {
        ParticleSystem.Instantiate(MuzzleFlash, ShootingPoint.transform.position, ShootingPoint.transform.rotation);
        SoundEffectManager.Instance.PlaySound(SoundEffectManager.Instance.SFXRefs.PlayerShot, ShootingPoint.transform.position);
    }

    public void GetHurt(int damageReceived)
    {
        if(!IsServer)
        {
            return;
        }

        if((Time.time - lastTimeGotHurt) < invulnerabilityTime)
        {
            return;
        }
        GetHurtClientRpc(damageReceived);
        lastTimeGotHurt = Time.time;
        
    }

    [ClientRpc]
    private void GetHurtClientRpc(int damageReceived)
    {   
        currentHealthPoints -= damageReceived;
        damageVFX.CallDamageEffect();
        SoundEffectManager.Instance.PlaySound(SoundEffectManager.Instance.SFXRefs.PlayerHurt, gameObject.transform.position);
        Debug.Log(gameObject.tag + " current HP = " + currentHealthPoints);
        CallUpdateHealthBar();
        if (currentHealthPoints <= 0)
        {
            PerformDeath();
        }
    }

    public void Heal(int healthRestored)
    {
        int healthPointsAfterHealing = 0;
        // Control the health points not overflowing the Max Health value
        if ((((currentHealthPoints + healthRestored) % maxHealthPoints) == (currentHealthPoints + healthRestored)) ||
            (((currentHealthPoints + healthRestored) % maxHealthPoints) == 0))
        {
            healthPointsAfterHealing = currentHealthPoints + healthRestored;
        } 
        else
        {
            int mod = (currentHealthPoints + healthRestored) % maxHealthPoints;
            healthPointsAfterHealing = currentHealthPoints + healthRestored - mod;
        }
        HealClientRpc(healthPointsAfterHealing);
        CallUpdateHealthBar();
    }

    [ClientRpc]
    private void HealClientRpc(int healthPointsAfterHealing)
    {
        currentHealthPoints = healthPointsAfterHealing;
    }

    private void PerformDeath()
    {
        if (currentHealthPoints <= 0)
        {
            PerformDeathServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PerformDeathServerRpc()
    {
        GameManager.Instance.SetGameFinishedState(true);
    }

    void AnimatePlayer()
    {
        // change the animation according to the direction the player is moving
        animator.SetFloat("Speed", movementDirection.sqrMagnitude);
    }

    void AimWeapon()
    {
        Vector2 aimPivotPosition = Rb.position;
        aimPivotPosition.y += 0.5f;
        Vector2 aimDirection = mousePosition - aimPivotPosition;
        float aimAngle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
        Hand.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, aimAngle));
        if (aimAngle < 90f && aimAngle > -90f)
        {
            animator.SetBool("looking_left", false);
            Hand.transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (aimAngle > 90f || aimAngle < -90f)
        {
            animator.SetBool("looking_left", true);
            Hand.transform.localScale = new Vector3(1f, -1f, 1f);
        }
    }

    void DisplaceCamera()
    {
        cameraTargetPosition = (Rb.position + mousePosition) / 2f;
        cameraTargetPosition.x = Mathf.Clamp(cameraTargetPosition.x, -cameraThreshold + Rb.position.x, cameraThreshold + Rb.position.x);
        cameraTargetPosition.y = Mathf.Clamp(cameraTargetPosition.y, -cameraThreshold + Rb.position.y, cameraThreshold + Rb.position.y);
        cameraTargetPosition.z = -9f;

        PlayerCamera.transform.position = cameraTargetPosition;
    }

    private void OnEnable()
    {
        //playerControls.Player.Enable();
    }

    private void OnDisable()
    {
        if(!IsOwner)
        {
            return;
        }

        playerControls.Player.Disable();
    }

    public override void OnNetworkSpawn()
    {
        if ((IsServer && IsOwner) || (!IsServer && !IsOwner))
        {
            gameObject.tag = "Player_1";
            bulletTag = "Player_1_Bullet";
            animator.runtimeAnimatorController = playerSpritesReferences.Player1Animator;
            Hand.GetComponentInChildren<SpriteRenderer>().sprite = playerSpritesReferences.Player1GunSprite;
        }
        else if ((IsServer && !IsOwner) || (!IsServer && IsOwner))
        {
            gameObject.tag = "Player_2";
            bulletTag = "Player_2_Bullet";
            animator.runtimeAnimatorController = playerSpritesReferences.Player2Animator;
            Hand.GetComponentInChildren<SpriteRenderer>().sprite = playerSpritesReferences.Player2GunSprite;
        }

        if (IsOwner)
        {
            playerControls = new InputActions();
            transform.position = spawnPoints[(int)OwnerClientId];
            
            PlayerCamera.transform.gameObject.SetActive(true);
            
            playerControls.Player.Dodge.performed += SetPlayerReady;
            playerControls.Player.Pause.performed += TogglePauseMenu;
            GameManager.Instance.OnPlayerDeath += GameManager_OnPlayerDeath;
            GameManager.Instance.OnReachedGoal += GameManager_OnReachedGoal;
            playerControls.Player.Enable();
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }

        base.OnNetworkSpawn();
    }

    private void GameManager_OnReachedGoal(object sender, EventArgs e)
    {
        SetPlayerDeathStateServerRpc();
    }

    private void GameManager_OnPlayerDeath(object sender, EventArgs e)
    {
        SetPlayerDeathStateServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerDeathStateServerRpc()
    {
        CanMove.Value = false;
        SetPlayerDeathStateClientRpc();
    }

    [ClientRpc]
    private void SetPlayerDeathStateClientRpc()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = false;
        Hand.GetComponentInChildren<SpriteRenderer>().enabled = false;
        selfLight.SetActive(false);
        hitbox.SetActive(false);
        gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        if (IsServer)
        {
            CanMove.Value = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        damageVFX = GetComponent<DamageEffect>();
        pauseMenu = GameObject.Find("PauseMenuOverlay");
        interfaceScript = GameObject.Find("User Interface").GetComponent<InGameUIScript>();
        if (!IsOwner)
        {
            return;
        }
        currentHealthPoints = maxHealthPoints;
        CallInitializeUserInterfaceServerRpc();
        CallUpdateHealthBar();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CallInitializeUserInterfaceServerRpc()
    {
        CallInitializeUserInterfaceClientRpc();
    }

    [ClientRpc]
    private void CallInitializeUserInterfaceClientRpc()
    {
        Debug.Log("Calling Client RPC for UI initialization...");
        interfaceScript.InitializeUserInterface();
    }

    private void CallUpdateHealthBar()
    {
        CallUpdateHealthBarClientRpc();
    }

    [ClientRpc]
    private void CallUpdateHealthBarClientRpc()
    {
        if(IsOwner)
        {
            interfaceScript.updateHealthBar(currentHealthPoints, maxHealthPoints);
        } 
        else
        {
            interfaceScript.updateOtherPlayerHealthBar(currentHealthPoints, maxHealthPoints);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner)
        {
            return;
        }

        if (!CanMove.Value)
        {
            return;
        }
        
        mousePosition = PlayerCamera.ScreenToWorldPoint(playerControls.Player.MousePosition.ReadValue<Vector2>());
        AnimatePlayer();
        DisplaceCamera();
        AimWeapon();
    }

    // FixedUpdate is called at a fixed rate, used for physics
    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        
        if (!CanMove.Value)
        {
            return;
        }
        
        movementDirection = playerControls.Player.Move.ReadValue<Vector2>().normalized;
        // rb.velocity = movementDirection * movementSpeed * Time.deltaTime;
        Vector2 force = movementDirection * movementSpeed * Time.deltaTime;
        Rb.AddForce(force);
        // Debug.Log(rb.velocity);

        // Shoot when the player is holding or pressing the attack button
        if (playerControls.Player.Attack.ReadValue<float>() == 1)
        {
            Shoot();
        }
    }
}
