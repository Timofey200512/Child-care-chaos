using System;
using UnityEngine;

public class FoodClickBehavior : MonoBehaviour, ILeftClickable, IDropable
{
    private Sprite sprite;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>().sprite;
    }

    public void OnDrop(Vector2 newPosition)
    {
        transform.position = newPosition;
    }

    public void OnLeftButtonClicked()
    {
        PlayerHolder.instance.Hold(gameObject, sprite);
    }
}
