using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        // รับปุ่ม WASD
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed)
                input.y += 1;

            if (Keyboard.current.sKey.isPressed)
                input.y -= 1;

            if (Keyboard.current.aKey.isPressed)
                input.x -= 1;

            if (Keyboard.current.dKey.isPressed)
                input.x += 1;
        }

        // ทิศทางการเดิน
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        // วิ่งด้วย Shift
        float speed = moveSpeed;

        if (Keyboard.current != null && Keyboard.current.leftShiftKey.isPressed)
        {
            speed = runSpeed;
        }

        controller.Move(move.normalized * speed * Time.deltaTime);

        // กระโดด
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;

            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}