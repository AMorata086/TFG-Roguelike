using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // variables for player movement
    public float movementSpeed = 5f;
    private Vector2 movementDirection;

    public Rigidbody2D rb;
    
    public InputActions playerControls;
    private InputAction move;

    private void Awake()
    {
        playerControls = new InputActions();
    }

    private void OnEnable()
    {
        move = playerControls.Player.Move;
        move.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        movementDirection = move.ReadValue<Vector2>();
    }

    // FixedUpdate is called at a fixed rate, used for physics
    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementDirection * movementSpeed * Time.fixedDeltaTime);
    }
}
