using System.Collections;
using UnityEngine;

public class SleepDesire : KidDesire
{
    private readonly KidPatience _kidPatience;
    private readonly KidClickBehavior kidClickBehavior;
    private readonly KidNavigation kidNavigation;
    private readonly SpriteRenderer spriteRenderer;
    private readonly Sprite _sleepingKidSprite;
    private readonly Sprite defaultSprite;
    private readonly float minSleepTime;
    private readonly float maxSleepTime;
    private bool _satisfied;
    private IEnumerator aftermath;

    public SleepDesire(KidPatience kidPatience, SpriteRenderer spriteRenderer,
        Sprite sleepingKidSprite, float minSleepTime, float maxSleepTime, Sprite sleepingDesire)
    {
        _kidPatience = kidPatience;
        kidNavigation = kidPatience.GetComponent<KidNavigation>();
        this.spriteRenderer = spriteRenderer;
        this._sleepingKidSprite = sleepingKidSprite;
        defaultSprite = spriteRenderer.sprite;
        this.minSleepTime = Mathf.Max(0f, minSleepTime);
        this.maxSleepTime = Mathf.Max(this.minSleepTime, maxSleepTime);
        image = sleepingKidSprite;
        kidClickBehavior = kidPatience.GetComponent<KidClickBehavior>();
        kidClickBehavior.OnPutToSleep += SetSatisfied;
        image = sleepingDesire;
    }

    public override bool IsSatisfied() => _satisfied;

    public override void Reset()
    {
        _satisfied = false;
        aftermath = null;
    }

    public override IEnumerator ActivateAftermath()
    {
        yield return aftermath;
    }

    public override CancellableCoroutine SetUnsatisfied()
    {
        //var crying = new CancellableCoroutine(_kidPatience.Cry());
        var sleeping = new CancellableCoroutine(Sleep());
        aftermath = sleeping;
        return sleeping;
    }

    private IEnumerator Sleep()
    {
        kidNavigation.StopWalking();
        kidClickBehavior.SetShadowActive(false);
        //_kidPatience.StopWaiting();
        spriteRenderer.sprite = _sleepingKidSprite;
        yield return CancellableCoroutine.Wait(Random.Range(minSleepTime, maxSleepTime));
        spriteRenderer.sprite = defaultSprite;
        kidClickBehavior.SetShadowActive(true);
        kidNavigation.KeepWalking();
    }

    public override void SetSatisfied()
    {
        _satisfied = true;
        aftermath = Sleep();
    }
}
