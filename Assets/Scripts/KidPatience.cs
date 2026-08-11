using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class KidPatience : MonoBehaviour
{
    [SerializeField] private float patienceSeconds = 5;
    private float _changeTimeSeconds;
    [SerializeField] private Sprite happyFace;
    [SerializeField] private Sprite neutralFace;
    [SerializeField] private Sprite sadFace;
    private SpriteRenderer _spriteRenderer;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        const int emotionsAmount = 3;
        _changeTimeSeconds = patienceSeconds / emotionsAmount;
    }
    
    public IEnumerator StartWaiting()
    {
        _spriteRenderer.sprite = happyFace;
        var nextSprite = neutralFace;
        while (_spriteRenderer.sprite != sadFace)
        {
            yield return new WaitForSeconds(_changeTimeSeconds);
            _spriteRenderer.sprite = nextSprite;
            nextSprite = sadFace;
        }
        yield return new WaitForSeconds(_changeTimeSeconds);
        print("Kid entered naughty mode");
    }
    
    private void Update()
    {
        
    }
}
