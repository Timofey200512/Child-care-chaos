using UnityEngine;

public class FoodClickBehavior : MonoBehaviour, IDropable
{

    public void OnDrop(Vector2 newPosition)
    {
        transform.position = newPosition;
    }
}
