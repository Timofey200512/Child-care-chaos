using System;
using UnityEngine;

public class FridgeClickBehavior : MonoBehaviour, IActionClickable
{
    [SerializeField] private GameObject food;
    [SerializeField] private Sprite foodSprite;

    public void OnActionButtonPressed()
    {
        PlayerHolder.instance.Hold(Instantiate(food), foodSprite);
    }
}