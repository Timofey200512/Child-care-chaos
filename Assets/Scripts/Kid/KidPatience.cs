using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(KidNavigation))]
public class KidPatience : MonoBehaviour
{
    [SerializeField] private float patienceSeconds = 5f;
    [SerializeField] private Sprite happyFace;
    [SerializeField] private Sprite neutralFace;
    [SerializeField] private Sprite sadFace;

    private SpriteRenderer _spriteRenderer;
    private float _changeTimeSeconds;

    [SerializeField] private GameObject desireBubble;
    [SerializeField] private SpriteRenderer desireSpriteRenderer;
    private CancellableCoroutine cryCoroutine;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _changeTimeSeconds = patienceSeconds / 3f;
    }

    public IEnumerator StartWaiting(KidDesire desire)
    {
        desireBubble.SetActive(true);
        desireSpriteRenderer.sprite = desire.Image;
        
        _spriteRenderer.sprite = happyFace;

        float elapsedSeconds = 0f;
        int emotionStage = 0;

        while (elapsedSeconds < patienceSeconds)
        {
            if (desire.Satisfied())
            {
                FinishWaiting();
                yield break;
            }
            
            elapsedSeconds += Time.deltaTime;

            if (emotionStage == 0 && elapsedSeconds >= _changeTimeSeconds)
            {
                _spriteRenderer.sprite = neutralFace;
                emotionStage = 1;
            }
            else if (emotionStage == 1 && elapsedSeconds >= _changeTimeSeconds * 2f)
            {
                _spriteRenderer.sprite = sadFace;
                emotionStage = 2;
            }

            yield return null;
        }
        
        cryCoroutine = new CancellableCoroutine(this, desire.NotSatisfiedAction());
        yield return cryCoroutine.Wait();
    }

    private void FinishWaiting()
    {
        desireBubble.SetActive(false);
        _spriteRenderer.sprite = happyFace;
    }
}

public abstract class KidDesire
{
    public Sprite Image => image;
    protected Sprite image;

    public abstract bool Satisfied();
    public abstract void Reset();
    public abstract IEnumerator NotSatisfiedAction();
}

public class SleepDesire : KidDesire
{
    public override bool Satisfied()
    {
        throw new System.NotImplementedException();
    }

    public override void Reset()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerator NotSatisfiedAction()
    {
        throw new NotImplementedException();
    }
}

