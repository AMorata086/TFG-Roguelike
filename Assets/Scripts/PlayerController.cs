using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    // Player Stats
    [Header("Player Stats")]
    public float MovementSpeed = 3000f;
    public int MaxHealthPoints = 10;
    private int CurrentHealthPoints = 10;
    public int Damage = 2;
    private bool canMove = false;
    [SerializeField] private float dodgeStrength = 750f;
    private float invulnerabilityTime = 0.25f;
    private float lastTimeGotHurt = 0f;

    // variables for player movement
    private Vector2 movementDirection;
    private Vector2 mousePosition;

    public Rigidbody2D Rb;

    public GameObject Hand;

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

    public Animator animator;

    public InputActions playerControls;

    // Cooldown controls
    [SerializeField] private float dodgeCooldown = 2f;
    private float lastDodgeTime = 0f;
    [SerializeField] private float shootingCooldown = 0.5f;
    private float lastShotTime = 0f;

    [SerializeField] private InGameUIScript interfaceScript;
    [SerializeField] private SoundEffectManager soundEffectManager;

    [SerializeField] private PlayerSpritesReferences playerSpritesReferences;

    private void Dodge(InputAction.CallbackContext context)
    {
        if ((Time.time - lastDodgeTime) < dodgeCooldown)
        {
            return;
        }
        Rb.AddForce(Rb.velocity.normalized * dodgeStrength * Time.fixedDeltaTime, ForceMode2D.Impulse);
        lastDodgeTime = Time.time;
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
        bulletGameObjectPlayerBulletScript.Damage = Damage;
        bulletGameObjectPlayerBulletScript.ChangeBulletSpriteAndMaterial(bulletTag);
        bullet.GetComponent<Rigidbody2D>().AddForce(ShootingPoint.transform.right * BulletSpeed * Time.deltaTime, ForceMode2D.Impulse);
    }

    [ClientRpc]
    private void SpawnShootingAudioAndVisualCuesClientRpc()
    {
        ParticleSystem.Instantiate(MuzzleFlash, ShootingPoint.transform.position, ShootingPoint.transform.rotation);
        soundEffectManager.PlaySound(soundEffectManager.SFXRefs.PlayerShot, ShootingPoint.transform.position);
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
        CurrentHealthPoints -= damageReceived;
        //interfaceScript.updateHealthBar(CurrentHealthPoints, MaxHealthPoints);
        damageVFX.CallDamageEffect();
        Debug.Log(gameObject.tag + " current HP = " + CurrentHealthPoints);
    }

    public void Heal(int healthRestored)
    {
        // Control the health points not overflowing the Max Health value
        if ((((CurrentHealthPoints + healthRestored) % MaxHealthPoints) == (CurrentHealthPoints + healthRestored)) ||
            (((CurrentHealthPoints + healthRestored) % MaxHealthPoints) == 0))
        {
            CurrentHealthPoints += healthRestored;
        } 
        else
        {
            int mod = (CurrentHealthPoints + healthRestored) % MaxHealthPoints;
            CurrentHealthPoints += healthRestored - mod;
        }
        interfaceScript.updateHealthBar(CurrentHealthPoints, MaxHealthPoints);
    }

    private void PerformDeath()
    {
        if (CurrentHealthPoints <= 0)
        {
            ParticleSystem.Instantiate(deathVFX, gameObject.transform.position, gameObject.transform.rotation);
            Destroy(gameObject);
        }
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

    private void Awake()
    {
        playerControls = new InputActions();
        // Subscribe to the actions that are button type
        // playerControls.Player.Attack.performed += Shoot;
        playerControls.Player.Dodge.performed += Dodge;
    }

    private void OnEnable()
    {
        //playerControls.Player.Enable();
    }

    private void OnDisable()
    {
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
            PlayerCamera.transform.gameObject.SetActive(true);
            playerControls.Player.Enable();
        }
        base.OnNetworkSpawn();
    }

    // Start is called before the first frame update
    void Start()
    {
        damageVFX = GetComponent<DamageEffect>();
        soundEffectManager = GameObject.Find("SoundManager").GetComponent<SoundEffectManager>();
        if(!IsOwner)
        {
            return;
        }
        interfaceScript = GameObject.Find("User Interface").GetComponent<InGameUIScript>();
        CurrentHealthPoints = MaxHealthPoints;
        interfaceScript.updateHealthBar(CurrentHealthPoints, MaxHealthPoints);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!IsOwner)
        {
            return;
        }
        /*
        if (!canMove)
        {
            return;
        }
        */
        mousePosition = PlayerCamera.ScreenToWorldPoint(playerControls.Player.MousePosition.ReadValue<Vector2>());
        AnimatePlayer();
        DisplaceCamera();
        AimWeapon();
        //PerformDeath();
    }

    // FixedUpdate is called at a fixed rate, used for physics
    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }
        /*
        if (!canMove)
        {
            return;
        }
        */
        movementDirection = playerControls.Player.Move.ReadValue<Vector2>().normalized;
        // rb.velocity = movementDirection * movementSpeed * Time.deltaTime;
        Vector2 force = movementDirection * MovementSpeed * Time.deltaTime;
        Rb.AddForce(force);
        // Debug.Log(rb.velocity);

        // Shoot when the player is holding or pressing the attack button
        if (playerControls.Player.Attack.ReadValue<float>() == 1)
        {
            Shoot();
        }
    }
}
