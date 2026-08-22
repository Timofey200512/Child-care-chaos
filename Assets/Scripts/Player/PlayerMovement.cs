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

        // flip sprite direction on input change
        if (horizontal > 0f)
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (horizontal < 0f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }
    }

    void FixedUpdate()
    {
        rigBody.MovePosition(rigBody.position + input * (speed * Time.fixedDeltaTime));
    }

    public int GetFacingDirection()
    {
        return (int)transform.localScale.x;
    }
}
