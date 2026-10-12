using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class SleepDesire : KidDesire
{
    private readonly KidClickBehavior kidClickBehavior;
    private readonly SpriteRenderer spriteRenderer;
    private readonly Sprite _sleepingKidSprite;
    private readonly Sprite defaultSprite;
    private readonly float minSleepTime;
    private readonly float maxSleepTime;
    private bool _satisfied;
    private IEnumerator aftermath;
    public event Action OnSleep;

    public SleepDesire(KidPatience kidPatience, SpriteRenderer spriteRenderer,
        Sprite sleepingKidSprite, float minSleepTime, float maxSleepTime, Sprite sleepingDesire)
    {
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
        var sleeping = new CancellableCoroutine(Sleep());
        aftermath = sleeping;
        return sleeping;
    }

    private IEnumerator Sleep()
    {
        kidClickBehavior.SetShadowActive(false);
        spriteRenderer.sprite = _sleepingKidSprite;
        yield return CancellableCoroutine.Wait(Random.Range(minSleepTime, maxSleepTime));
        spriteRenderer.sprite = defaultSprite;
        OnSleep();
    }

    public override void SetSatisfied()
    {
        _satisfied = true;
        aftermath = Sleep();
    }
}
