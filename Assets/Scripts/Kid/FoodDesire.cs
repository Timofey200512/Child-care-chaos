using System;
using System.Collections;
using UnityEngine;

public class FoodDesire : KidDesire
{
    private KidPatience _kidPatience;
    private bool _satisfied;
    public FoodDesire(KidPatience kidPatience, Sprite sprite, KidClickBehavior kidClickBehavior)
    {
        _kidPatience = kidPatience;
        image = sprite;
        kidClickBehavior.OnKidPressed = UpdateSatisfiedStatus;
    }

    private void UpdateSatisfiedStatus()
    {
        if (!PlayerHolder.instance.HoldsItemOfTag("Food")) return;
        
        PlayerHolder.instance.DeleteItem();
        _satisfied = true;
        SetSatisfied();
    }
    
    public override bool IsSatisfied()
    {
        return _satisfied;
    }

    public override void Reset()
    {
        _satisfied = false;
    }

    private IEnumerator aftermath;

    public override IEnumerator ActivateAftermath()
    {
        yield return aftermath;
    }

    public override CancellableCoroutine SetUnsatisfied()
    {
        aftermath = new CancellableCoroutine(_kidPatience.Cry());
        return (CancellableCoroutine)aftermath;
    }

    public override void SetSatisfied()
    {
        aftermath = StopWantingFood();
    }

    private IEnumerator StopWantingFood()
    {
        yield break;
    }
}