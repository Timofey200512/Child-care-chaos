using System;
using UnityEngine;

public class Highlighter : MonoBehaviour
{
    private PlayerMovement _playerMovement;
    [SerializeField] private Vector2 overlapBoxOffset;
    [SerializeField] private Vector2 overlapBoxSize = Vector2.one;
    private IHighlightable _highlightedObject;
    public bool boxOverlapped;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        InputGetter.instance.OnLeftMousePressed += OnLeftMousePressed;
        InputGetter.instance.OnRightMousePressed += OnRightMousePressed;
    }

    private void Update()
    {
        Vector2 facingOffset = GetFacingOffset();
        var collider = Physics2D.OverlapBox(
            (Vector2)transform.position + facingOffset,
            overlapBoxSize,
            0f);

        if (collider?.TryGetComponent<IHighlightable>(out var highlightable) == true)
        {
            highlightable.OnHighlighted();
            boxOverlapped  = true;
            _highlightedObject = highlightable;
        }
        else
        {
            boxOverlapped = false;
            _highlightedObject?.OnDeHighlighted();
        }
    }

    private void OnLeftMousePressed()
    {
        if (boxOverlapped && !PlayerHolder.instance.GotItem)
        {
            _highlightedObject.GiveItemToPlayer();
        }
    }

    private void OnRightMousePressed()
    {
        if (PlayerHolder.instance.GotItem)
        {
            PlayerHolder.instance.Drop();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube((Vector2)transform.position + GetFacingOffset(), overlapBoxSize);
    }

    private Vector2 GetFacingOffset()
    {
        return new Vector2(Mathf.Abs(overlapBoxOffset.x) * _playerMovement.GetFacingDirection(), overlapBoxOffset.y);
    }
}
