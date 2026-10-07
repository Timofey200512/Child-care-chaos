using System;
using UnityEngine;
using UnityEngine.AI;

public class KidClickBehavior : Item
{
    private KidNavigation _kidNavigation;
    private NavMeshAgent _agent;
    private SpriteRenderer kidSpriteRenderer;
    public Action OnKidPressed;
    public event Action OnPutToSleep;
    private KidDesiresManager kidDesiresManager;
    private KidPatience _kidPatience;

    private void Awake()
    {
        base.Awake();
        _kidPatience = GetComponent<KidPatience>();
        kidDesiresManager = GetComponent<KidDesiresManager>();
        kidSpriteRenderer = GetComponent<SpriteRenderer>();
        _kidNavigation = GetComponent<KidNavigation>();
        _agent = GetComponent<NavMeshAgent>();
    }

    public override void OnActionButtonPressed()
    {
        if (PlayerHolder.instance.GotItem && !_kidPatience.IsCrying)
        {
            OnKidPressed?.Invoke();
        }
        else if (!PlayerHolder.instance.GotItem)
        {
            SetPickedUp(true);
            OnPickedUp();
        }
    }
    
    public void PutToSleep()
    {
        _kidNavigation.StopWalking();
        OnPutToSleep?.Invoke();
    }

    protected override void OnPickedUp()
    {
        _kidNavigation.StopWalking();
        PlayerHolder.instance.Hold(this, kidSpriteRenderer);
    }

    protected override void OnDropped(Vector3 newPosition)
    {
        _agent.Warp(newPosition);
        _kidNavigation.KeepWalking();
    }
}
