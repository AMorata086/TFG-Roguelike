using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Player Stats
    public float MovementSpeed = 3000f;
    public float HealthPoints = 10f;
    public float Damage = 2f;

    // variables for player movement
    private Vector2 movementDirection;
    private Vector2 mousePosition;

    public Rigidbody2D Rb;

    public GameObject Hand;

    public GameObject ShootingPoint;
    public GameObject BulletPrefab;
    public float BulletSpeed = 750f;
    public ParticleSystem MuzzleFlash;

    public Camera playerCamera;
    // fields for camera movement relative to the player position
    private Vector3 cameraTargetPosition = new Vector3();
    public float cameraThreshold;

    public Animator animator;
    
    public InputActions playerControls;

    // Cooldown controls
    public float ShootingCooldown = 0.5f;
    private float lastShotTime = 0;

    private void Awake()
    {
        playerControls = new InputActions();
        // Subscribe to the actions that are button type
        // playerControls.Player.Attack.performed += Shoot;
        playerControls.Player.Dodge.performed += Dodge;
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
    }

    private void Dodge(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    /*
    private void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (Time.time - lastShotTime < shootingCooldown) 
            {
                return;
            }
            GameObject bullet = Instantiate(bulletPrefab, shootingPoint.transform.position, shootingPoint.transform.rotation);
            bullet.GetComponent<Rigidbody2D>().AddForce(shootingPoint.transform.right * bulletSpeed, ForceMode2D.Impulse);
            lastShotTime = Time.time;
        }
    }
    */
    private void Shoot()
    {
        if ((Time.time - lastShotTime) < ShootingCooldown)
        {
            return;
        }
        ParticleSystem muzzleFlashVFX = ParticleSystem.Instantiate(MuzzleFlash, ShootingPoint.transform.position, ShootingPoint.transform.rotation);
        Destroy(muzzleFlashVFX.gameObject, muzzleFlashVFX.main.duration);
        GameObject bullet = Instantiate(BulletPrefab, ShootingPoint.transform.position, ShootingPoint.transform.rotation);
        bullet.GetComponent<Rigidbody2D>().AddForce(ShootingPoint.transform.right * BulletSpeed * Time.deltaTime, ForceMode2D.Impulse);
        lastShotTime = Time.time;
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
        } else if(aimAngle > 90f || aimAngle < -90f)
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

        playerCamera.transform.position = cameraTargetPosition;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        mousePosition = playerCamera.ScreenToWorldPoint(playerControls.Player.MousePosition.ReadValue<Vector2>());
        //mousePosition = Camera.main.ScreenToWorldPoint(playerControls.Player.MousePosition.ReadValue<Vector2>());
        AnimatePlayer();
        DisplaceCamera();
        AimWeapon();
    }

    // FixedUpdate is called at a fixed rate, used for physics
    private void FixedUpdate()
    {
        movementDirection = playerControls.Player.Move.ReadValue<Vector2>().normalized;
        // rb.velocity = movementDirection * movementSpeed * Time.deltaTime;
        Vector2 force = movementDirection * MovementSpeed * Time.deltaTime;
        Rb.AddForce(force);
        // Debug.Log(rb.velocity);

        // Shoot when the player is holding or pressing the attack button
        if(playerControls.Player.Attack.ReadValue<float>() == 1)
        {
            Shoot();
        }
    }
}
