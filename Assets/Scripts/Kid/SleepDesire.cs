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
    private readonly Rigidbody body;
    private readonly bool originalIsKinematic;
    private readonly bool originalUseGravity;
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
        body = kidPatience.GetComponent<Rigidbody>();
        if (body)
        {
            originalIsKinematic = body.isKinematic;
            originalUseGravity = body.useGravity;
        }
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

    public void SleepForeverForTesting()
    {
        _satisfied = true;
        aftermath = null;
        agent.gameObject.GetComponent<Collider>().isTrigger = true;
        // body.useGravity = false;
        BeginSleepingPhysics();
        spriteRenderer.sprite = _sleepingKidSprite;
        spriteRenderer.flipX = false;
        kidClickBehavior.SetShadowActive(false);
        OnSleep?.Invoke();
    }

    public void WakeFromTrip()
    {
        if (aftermath is CancellableCoroutine cancellable)
            cancellable.Cancel();
        
        spriteRenderer.sprite = defaultSprite;
        spriteRenderer.flipX = false;
        kidClickBehavior.transform.SetParent(parent);
        RestoreAwakePhysics();
        if (agent)
        {
            if (!agent.enabled) agent.enabled = true;
            agent.Warp(kidClickBehavior.transform.position);
        }
        kidClickBehavior.SetShadowActive(true);
        if (agent && agent.isOnNavMesh)
            kidClickBehavior.GetComponent<KidNavigation>().KeepWalking();
        OnAwake?.Invoke();
        aftermath = null;
    }

    private IEnumerator Sleep()
    {
        BeginSleepingPhysics();
        kidClickBehavior.SetShadowActive(false);
        spriteRenderer.sprite = _sleepingKidSprite;
        OnSleep?.Invoke();
        body.useGravity = false;
        agent.gameObject.GetComponent<Collider>().isTrigger = true;
        yield return CancellableCoroutine.Wait(Random.Range(minSleepTime, maxSleepTime));
        agent.gameObject.GetComponent<Collider>().isTrigger = false;
        body.useGravity = true;
        spriteRenderer.sprite = defaultSprite;
        RestoreAwakePhysics();
        agent.enabled = true;
        if (!PlayerHolder.instance.HoldsItemOfTag("Kid"))
        {
            kidClickBehavior.transform.SetParent(parent);
            agent.Warp(kidClickBehavior.transform.position);
            kidClickBehavior.GetComponent<KidNavigation>().KeepWalking();
        }
        OnAwake?.Invoke();
    }

    private void BeginSleepingPhysics()
    {
        if (body)
        {
            body.useGravity = false;
            body.isKinematic = true;
        }

        if (agent && agent.enabled)
            agent.enabled = false;
    }

    private void RestoreAwakePhysics()
    {
        if (!body) return;
        body.isKinematic = originalIsKinematic;
        body.useGravity = originalUseGravity;
    }

    public override void SetSatisfied()
    {
        _satisfied = true;
        // flip because if satisfied that means kid is sleeping on bed which means sprite needs to be flipped
        spriteRenderer.flipX = true;
        agent.enabled = false;
        aftermath = new CancellableCoroutine(Sleep());
    }
}
