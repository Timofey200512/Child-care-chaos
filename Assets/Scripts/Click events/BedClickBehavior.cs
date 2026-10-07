using System;
using UnityEngine;

public class BedClickBehavior : MonoBehaviour, IActionButtonClickable
{
    [SerializeField] private Vector3 kidSleepingPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnActionButtonPressed()
    {
        if (PlayerHolder.instance.HoldsItemOfTag("Kid"))
        {
            Item kid = PlayerHolder.instance.Drop();
            kid.transform.SetParent(transform.root);
            kid.transform.position = kidSleepingPosition;
            kid.transform.eulerAngles = new Vector3(kid.transform.eulerAngles.x, kid.transform.eulerAngles.y, 90f);
            kid.GetComponent<KidClickBehavior>().PutToSleep();
        }
    }
}
