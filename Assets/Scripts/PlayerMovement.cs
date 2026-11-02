using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float sprintMultiplier = 1.7f;
    public float crouchMultiplier = 0.5f;
    public float crouchHeight = 1.0f;
    public float jumpHeight = 1.25f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Vector3 velocity;
    private float standingHeight;
    private Vector3 standingCenter;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        standingHeight = controller.height;
        standingCenter = controller.center;
        crouchHeight = Mathf.Clamp(crouchHeight, controller.radius * 2f, standingHeight);
    }

    private void Update()
    {
        if (controller == null) return;

        bool crouching = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        bool sprinting = !crouching && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift));
        float targetHeight = crouching ? crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, 12f * Time.deltaTime);
        controller.center = standingCenter + Vector3.down * ((standingHeight - controller.height) * 0.5f);

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        if (move.sqrMagnitude > 1f) move.Normalize();

        float speed = moveSpeed * (crouching ? crouchMultiplier : sprinting ? sprintMultiplier : 1f);
        if (controller.isGrounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (!crouching && controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move((move * speed + Vector3.up * velocity.y) * Time.deltaTime);
    }
}