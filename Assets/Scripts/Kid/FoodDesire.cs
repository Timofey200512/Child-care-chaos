using System.Collections;
using UnityEngine;

public class FoodDesire : KidDesire
{
    private readonly KidDesiresManager _kidDesiresManager;
    private bool _satisfied;
    public FoodDesire(KidDesiresManager kidDesiresManager, Sprite sprite, KidClickBehavior kidClickBehavior)
    {
        _kidDesiresManager = kidDesiresManager;
        image = sprite;
        kidClickBehavior.OnKidPressed = UpdateSatisfiedStatus;
    }

    private void UpdateSatisfiedStatus()
    {
        if (!PlayerHolder.instance.HoldsItemOfTag("Food")) return;
        
        PlayerHolder.instance.DeleteItem();
        _satisfied = true;
    }
    
    public override bool Satisfied()
    {
        return _satisfied;
    }

    public override void Reset()
    {
        _satisfied = false;
    }

    public override IEnumerator NotSatisfiedAction() => _kidDesiresManager.Cry();
}