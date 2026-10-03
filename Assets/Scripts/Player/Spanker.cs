using System;
using System.Collections;
using UnityEngine;

public class Spanker : MonoBehaviour
{
    [SerializeField] private Sprite playerPreparesToAttackSprite;
    [SerializeField] private Sprite playerAttackedSprite;
    [SerializeField] private float attackSpriteStepSeconds = 0.15f;

    private BoxOverlapDetector _overlapDetector;
    private SpriteRenderer _spriteRenderer;
    private bool _isAttacking;

    private void Awake()
    {
        _overlapDetector = GetComponent<BoxOverlapDetector>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Spank(Action callback = null)
    {
        if (_isAttacking)
        {
            return;
        }

        StartCoroutine(SpankRoutine(callback));
    }

    private IEnumerator SpankRoutine(Action callback = null)
    {
        _isAttacking = true;
        Sprite defaultSprite = _spriteRenderer.sprite;

        _spriteRenderer.sprite = playerPreparesToAttackSprite;
        yield return new WaitForSeconds(attackSpriteStepSeconds);

        _spriteRenderer.sprite = playerAttackedSprite;
        yield return new WaitForSeconds(attackSpriteStepSeconds);
        
        _overlapDetector.OnComponentDetected<KidPatience>(kidPatience => kidPatience.TakeDamage());

        _spriteRenderer.sprite = defaultSprite;
        _isAttacking = false;
        if (callback != null)
        {
            callback();
        }
    }
}
