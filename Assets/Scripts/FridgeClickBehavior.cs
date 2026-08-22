using System;
using UnityEngine;

public class FridgeClickBehavior : MonoBehaviour, ILeftClickable
{
    [SerializeField] private GameObject food;
    public void OnLeftButtonClicked()
    {
        PlayerHolder.instance.Hold(Instantiate(food));
    }
    
}