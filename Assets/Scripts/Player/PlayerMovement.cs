using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rigBody;
    private Vector2 input;

    private void Awake()
    {
        rigBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        input = new Vector2(horizontal, vertical).normalized;

        if (horizontal > 0f)
        {
            spriteRenderer.flipX = false;
        }
        else if (horizontal < 0f)
        {
            spriteRenderer.flipX = true;
        }
    }

    void FixedUpdate()
    {
        rigBody.MovePosition(rigBody.position + input * (speed * Time.fixedDeltaTime));
    }

    public bool IsFacingLeft => spriteRenderer != null && spriteRenderer.flipX;
}
