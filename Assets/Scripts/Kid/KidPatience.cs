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
    
    [SerializeField] private GameObject desireBubble;
    [SerializeField] private SpriteRenderer desireSpriteRenderer;
    [SerializeField] private Sprite desire;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _changeTimeSeconds = patienceSeconds / 3f;
    }

    private bool _stopRequested;

    public IEnumerator StartWaiting()
    {
        _stopRequested = false;
        print("Starting waiting");
        desireBubble.SetActive(true);
        desireSpriteRenderer.sprite = desire;
        
        _spriteRenderer.sprite = happyFace;

        float elapsedSeconds = 0f;
        int emotionStage = 0;

        while (elapsedSeconds < patienceSeconds && !_stopRequested)
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
        FinishWaiting();
    }

    public void SatisfyDesire()
    {
        print("Satisfied Desire");
        _stopRequested = true;
        FinishWaiting();
    }

    private void FinishWaiting()
    {
        desireBubble.SetActive(false);
        _spriteRenderer.sprite = happyFace;
    }
}
