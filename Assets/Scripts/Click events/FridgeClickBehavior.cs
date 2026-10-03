using UnityEngine;

public class FridgeClickBehavior : MonoBehaviour, IActionButtonClickable
{
    [SerializeField] private GameObject food;

    public void OnActionButtonPressed()
    {
        var foodItem = Instantiate(food);
        FoodClickBehavior foodClickBehavior = foodItem.GetComponentInChildren<FoodClickBehavior>();
        //SpriteRenderer foodSpriteRenderer = foodClickBehavior1.GetComponent<SpriteRenderer>();
        //PlayerHolder.instance.Hold(foodClickBehavior1.gameObject, foodSpriteRenderer);
        foodClickBehavior.OnActionButtonPressed();
    }
}

public interface IActionButtonClickable
{
    public void OnActionButtonPressed();
}
