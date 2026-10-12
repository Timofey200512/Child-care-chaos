using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class SleepDesire : KidDesire
{
    private readonly KidClickBehavior kidClickBehavior;
    private NavMeshAgent agent;
    private readonly SpriteRenderer spriteRenderer;
    private readonly Sprite _sleepingKidSprite;
    private readonly Sprite defaultSprite;
    private readonly float minSleepTime;
    private readonly float maxSleepTime;
    private bool _satisfied;
    private IEnumerator aftermath;
    public event Action OnSleep;
    public event Action OnAwake;
    private Transform parent;

    public SleepDesire(KidPatience kidPatience, SpriteRenderer spriteRenderer,
        Sprite sleepingKidSprite, float minSleepTime, float maxSleepTime, Sprite sleepingDesire)
    {
        parent = kidPatience.transform.root;
        agent = kidPatience.gameObject.GetComponent<NavMeshAgent>();
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
        OnSleep();
        yield return CancellableCoroutine.Wait(Random.Range(minSleepTime, maxSleepTime));
        spriteRenderer.sprite = defaultSprite;
        agent.enabled = true;
        if (!PlayerHolder.instance.HoldsItemOfTag("Kid"))
        {
            Debug.Log("wake up");
            kidClickBehavior.transform.SetParent(parent);
            agent.Warp(kidClickBehavior.transform.position);
            kidClickBehavior.GetComponent<KidNavigation>().KeepWalking();
        }
        OnAwake();
    }

    public override void SetSatisfied()
    {
        _satisfied = true;
        // flip because if satisfied that means kid is sleeping on bed which means sprite needs to be flipped
        spriteRenderer.flipX = true;
        agent.enabled = false;
        aftermath = Sleep();
    }
}
