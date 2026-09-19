using System;
using UnityEngine;
using UnityEngine.AI;

public class KidClickBehavior : MonoBehaviour, IDropable, IActionClickable
{
    private KidNavigation _kidNavigation;
    private NavMeshAgent _agent;
    private Sprite kidSprite;
    public Action OnKidPressed;

    private void Start()
    {
        kidSprite = GetComponent<SpriteRenderer>().sprite;
        _kidNavigation = GetComponent<KidNavigation>();
        _agent = GetComponent<NavMeshAgent>();
    }

    public void OnActionButtonPressed()
    {
        if (PlayerHolder.instance.GotItem)
        {
            OnKidPressed?.Invoke();
        }
        else
        {
            _agent.enabled = false;
            PlayerHolder.instance.Hold(gameObject, kidSprite);
        }
    }

    public void OnDrop(Vector3 newPosition)
    {
        _agent.Warp(newPosition);
        _agent.enabled = true;
        StartCoroutine(_kidNavigation.Wander());
    }
}