using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{

    public CharacterController controller;
    public Transform cam;

    [Header("Movement Settings")]
    public float speed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float smoothTurnTime = 0.2f;

    private float smoothTurnVelocity;
    private Vector3 velocity;
    private bool isGrounded;

    [Header("Ground Check")]
    public Transform groundCheckObject;
    public float groundDistance = 0.4f;
    public LayerMask groundLayer;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();

    }

    void HandleMovement()
    {
        isGrounded = Physics.CheckSphere(groundCheckObject.position, groundDistance, groundLayer);

        // reset vertical velocity when grounded
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // movement input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // move player based on input
        if (direction.magnitude > 0.1f)
        {

            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y; // angle to rotate player by
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref smoothTurnVelocity, smoothTurnTime); // smooth angle to avoid snapping when turning player
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward; // move player where camera is pointing

            controller.Move(speed * Time.deltaTime * moveDirection.normalized);
        }

        // handle jumping
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }
}
