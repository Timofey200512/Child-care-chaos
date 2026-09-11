using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    private SpriteRenderer spriteRenderer;
    private Rigidbody rigBody;
    private Vector3 input;

    private void Awake()
    {
        rigBody = GetComponent<Rigidbody>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        input = new Vector3(horizontal, 0, vertical).normalized;

        // flip sprite direction on input change transform is used instead of sprite renderer because
        // children should change their position as well when player is facing different directions
        if (horizontal > 0f)
        {
            float xScale = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
        }
        else if (horizontal < 0f)
        {
            float xScale = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(-xScale, transform.localScale.y, transform.localScale.z);
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
