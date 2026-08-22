using UnityEngine;
using UnityEngine.AI;

public class KidClickBehavior : MonoBehaviour, ILeftClickable, IDropable
{
    private KidNavigation _kidNavigation;
    private NavMeshAgent _agent;
    private Sprite sprite;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>().sprite;
        _kidNavigation = GetComponent<KidNavigation>();
        _agent = GetComponent<NavMeshAgent>();
    }

    public void OnLeftButtonClicked()
    {
        _agent.enabled = false;
        PlayerHolder.instance.Hold(gameObject, sprite);
    }

    public void OnDrop(Vector2 newPosition)
    {
        _agent.Warp(newPosition);
        _agent.enabled = true;
        StartCoroutine(_kidNavigation.Wander());
    }
}