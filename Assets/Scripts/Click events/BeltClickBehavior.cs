using System;
using UnityEngine;

public class BeltClickBehavior : MonoBehaviour, IActionClickable, IDropable, IUsable
{
    [SerializeField] private Sprite beltSprite;
    
    public void OnActionButtonPressed()
    {
        transform.parent.gameObject.SetActive(false);
        PlayerHolder.instance.Hold(gameObject, beltSprite);
    }

    public void OnDrop(Vector3 newPosition)
    {
        transform.parent.gameObject.SetActive(true);
        transform.position = newPosition;
    }

    public void Use(Spanker spanker, Action callback = null)
    {
        print("spanking"); 
        // spanking method
        spanker.Spank(callback);
    }
}
