using System;
using UnityEngine;

public class FoodClickBehavior : Item
{
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void OnPickedUp()
    {
        PlayerHolder.instance.Hold(this, spriteRenderer);
    }

    protected override void OnDropped(Vector3 newPosition)
    {
        transform.parent.gameObject.SetActive(true);
        transform.position = newPosition;
    }

    private void OnDestroy()
    {   
        Destroy(parent.gameObject);
        Destroy(gameObject);
    }
}
