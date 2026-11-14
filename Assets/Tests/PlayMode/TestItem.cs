using UnityEngine;

public class TestItem : Item
{
    protected override void OnPickedUp()
    {
        PlayerHolder.instance.Hold(this, GetComponent<SpriteRenderer>());
    }

    protected override void OnDropped(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void PickUp()
    {
        OnActionButtonPressed();
    }
}
