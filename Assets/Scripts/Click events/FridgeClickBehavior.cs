using System;
using UnityEngine;

public class FridgeClickBehavior : MonoBehaviour, ILeftClickable
{
    [SerializeField] private GameObject food;
    private Sprite foodSprite;

    private void Start()
    {
        foodSprite = food.GetComponent<SpriteRenderer>().sprite;
    }

    public void OnLeftButtonClicked()
    {
        PlayerHolder.instance.Hold(Instantiate(food), foodSprite);
    }
    
}