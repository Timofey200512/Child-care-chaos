using System.Collections;
using UnityEngine;

public abstract class KidDesire
{
    public Sprite Image => image;
    protected Sprite image;

    public abstract bool IsSatisfied();
    public abstract void Reset();

    public abstract IEnumerator ActivateAftermath();

    public abstract CancellableCoroutine SetUnsatisfied();

    public abstract void SetSatisfied();
}