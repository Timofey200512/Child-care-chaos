using UnityEngine;
using UnityEngine.AI;

public class KidClickBehavior : MonoBehaviour, ILeftClickable, IDropable, IActionClickable
{
    private KidNavigation _kidNavigation;
    private NavMeshAgent _agent;
    private Sprite kidSprite;
    private KidPatience kidPatience;

    private void Start()
    {
        kidPatience = GetComponent<KidPatience>();
        kidSprite = GetComponent<SpriteRenderer>().sprite;
        _kidNavigation = GetComponent<KidNavigation>();
        _agent = GetComponent<NavMeshAgent>();
    }

    public void OnLeftButtonClicked()
    {
        _agent.enabled = false;
        PlayerHolder.instance.Hold(gameObject, kidSprite);
    }

    public void OnActionButtonPressed()
    {
        if (PlayerHolder.instance.Item.CompareTag("Food"))
        {
            kidPatience.SatisfyDesire();
            PlayerHolder.instance.DeleteItem();
        }
    }

    public void OnDrop(Vector3 newPosition)
    {
        _agent.Warp(newPosition);
        _agent.enabled = true;
        StartCoroutine(_kidNavigation.Wander());
    }
}