using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class KidPatience : MonoBehaviour
{
    [SerializeField] private float patienceSeconds = 5f;
    [SerializeField] private Sprite happyFace;
    [SerializeField] private Sprite neutralFace;
    [SerializeField] private Sprite sadFace;

    private SpriteRenderer _spriteRenderer;
    private float _changeTimeSeconds;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _changeTimeSeconds = patienceSeconds / 3f;
    }
    public IEnumerator StartWaiting()
    {
        _spriteRenderer.sprite = happyFace;

        float elapsedSeconds = 0f;
        int emotionStage = 0;

        while (elapsedSeconds < patienceSeconds)
        {
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
        print("desire not satisfied");
    }

    public void SatisfyDesire()
    {
        StopCoroutine(StartWaiting());
        _spriteRenderer.sprite = happyFace;
    }
}
