using System;
using UnityEngine;

public class FoodClickBehavior : MonoBehaviour, ILeftClickable, IDropable
{
    private Sprite sprite;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>().sprite;
    }

    public void OnDrop(Vector3 newPosition)
    {
        transform.parent.gameObject.SetActive(true);
        transform.position = newPosition;
    }

    public void OnLeftButtonClicked()
    {
        transform.parent.gameObject.SetActive(false);
        PlayerHolder.instance.Hold(gameObject, sprite);
    }
}
