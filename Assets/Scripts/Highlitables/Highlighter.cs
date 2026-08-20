using UnityEngine;

public class Highlighter : MonoBehaviour
{
    private PlayerMovement playerMovement;
    [SerializeField] private Vector2 overlapBoxOffset;
    [SerializeField] private Vector2 overlapBoxSize = Vector2.one;
    private IHighlightable _highlightedObject;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        
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
            _highlightedObject = highlightable;
        }
        else
        {
            _highlightedObject?.OnDeHighlighted();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube((Vector2)transform.position + GetFacingOffset(), overlapBoxSize);
    }

    private Vector2 GetFacingOffset()
    {
        float facingSign = playerMovement != null && playerMovement.IsFacingLeft ? -1f : 1f;
        return new Vector2(Mathf.Abs(overlapBoxOffset.x) * facingSign, overlapBoxOffset.y);
    }
}
