using System;
using System.Collections;
using UnityEngine;

public class Spanker : MonoBehaviour
{
    [SerializeField] private Sprite playerPreparesToAttackSprite;
    [SerializeField] private Sprite playerAttackedSprite;
    [SerializeField] private float attackSpriteStepSeconds = 0.15f;

    private BoxOverlapDetector _overlapDetector;
    private PlayerMovement _playerMovement;
    private SpriteRenderer _spriteRenderer;
    private bool _isAttacking;

    private void Awake()
    {
        _overlapDetector = GetComponent<BoxOverlapDetector>();
        _playerMovement = GetComponent<PlayerMovement>();
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
        // show attack sprites
        _isAttacking = true;
        _playerMovement.enabled = false;
        Sprite defaultSprite = _spriteRenderer.sprite;

        _spriteRenderer.sprite = playerPreparesToAttackSprite;
        yield return new WaitForSeconds(attackSpriteStepSeconds);

        _spriteRenderer.sprite = playerAttackedSprite;
        yield return new WaitForSeconds(attackSpriteStepSeconds);
        
        // use overlap detector 
        Collider[] overlappedColliders = _overlapDetector.DetectOverlaps();
        
        // deal damage on it if overlaped
        foreach (Collider overlappedCollider in overlappedColliders)
        {
            KidPatience kidPatience = overlappedCollider.GetComponentInParent<KidPatience>();
            if (!kidPatience)
            {
                continue;
            }

            kidPatience.TakeDamage();
            break;
        }

        _spriteRenderer.sprite = defaultSprite;
        _playerMovement.enabled = true;
        _isAttacking = false;
        if (callback != null)
        {
            callback();
        }
    }
}
