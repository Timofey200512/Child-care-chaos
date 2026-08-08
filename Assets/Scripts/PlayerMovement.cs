using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float speed;
    private Rigidbody2D rigBody;
    private Vector2 input;

    private void Awake()
    {
        rigBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        input = new Vector2(horizontal, vertical).normalized;
    }

    void FixedUpdate()
    {
        rigBody.MovePosition(rigBody.position + input * (speed * Time.fixedDeltaTime));
    }
}
