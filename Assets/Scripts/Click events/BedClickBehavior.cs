using System;
using UnityEngine;

public class BedClickBehavior : MonoBehaviour, IActionButtonClickable
{
    [SerializeField] private Vector3 kidSleepingPosition;

    public void OnActionButtonPressed()
    {
        var heldKid = PlayerHolder.instance.HeldItem as KidClickBehavior;
        if (heldKid != null && heldKid.CanBePutToSleep)
        {
            Item kid = PlayerHolder.instance.Drop();
            heldKid.PutToSleep();
            kid.transform.SetParent(transform.root);
            kid.transform.localPosition = kidSleepingPosition;
        }
    }
}
