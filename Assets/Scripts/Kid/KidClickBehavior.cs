using UnityEngine;
using UnityEngine.AI;

public class KidClickBehavior : MonoBehaviour, ILeftClickable, IDropable
{
    private KidNavigation _kidNavigation;
    private NavMeshAgent _agent;

    private void Start()
    {
        _kidNavigation = GetComponent<KidNavigation>();
        _agent = GetComponent<NavMeshAgent>();
    }

    public void OnLeftButtonClicked()
    {
        _agent.enabled = false;
        PlayerHolder.instance.Hold(gameObject);
    }

    public void OnDrop(Vector2 newPosition)
    {
        _agent.Warp(newPosition);
        _agent.enabled = true;
        StartCoroutine(_kidNavigation.Wander());
    }
}