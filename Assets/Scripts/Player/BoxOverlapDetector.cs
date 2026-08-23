using System;
using UnityEngine;

public class BoxOverlapDetector : MonoBehaviour
{

    private PlayerMovement _playerMovement;
    [SerializeField] private Vector2 overlapBoxOffset;
    [SerializeField] private Vector2 overlapBoxSize = Vector2.one;
    private bool boxOverlapped;
    private Collider2D collider;
    public event Action<Collider2D> OnOverlap;
    public event Action OnOnOverlap;
    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        InputGetter.instance.OnLeftMousePressed += OnLeftMousePressed;
        InputGetter.instance.OnRightMousePressed += OnRightMousePressed;
        InputGetter.instance.OnActionMousePressed += OnActionMousePressed;
    }
    
    // Update is called once per frame
    void Update()
    {
        Vector2 facingOffset = GetFacingOffset();
        collider = Physics2D.OverlapBox(
            (Vector2)transform.position + facingOffset,
            overlapBoxSize,
            0f);
        
        if (collider)
        {
            boxOverlapped = true;
            OnOverlap?.Invoke(collider);
        }
        else
        {
            boxOverlapped = false;
            OnOnOverlap?.Invoke();
        }
    }
    
    private Vector2 GetFacingOffset()
    {
        return new Vector2(Mathf.Abs(overlapBoxOffset.x) * _playerMovement.GetFacingDirection(), overlapBoxOffset.y);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube((Vector2)transform.position + GetFacingOffset(), overlapBoxSize);
    }
    
    private void OnLeftMousePressed()
    {
        if (boxOverlapped && !PlayerHolder.instance.GotItem)
        {
            collider.GetComponent<ILeftClickable>().OnLeftButtonClicked();
        }
    }
    
    private void OnActionMousePressed()
    {
        if (boxOverlapped)
        {
            collider.GetComponent<IActionClickable>().OnActionButtonPressed();
        }
    }
    
    private void OnRightMousePressed()
    {
        if (PlayerHolder.instance.GotItem)
        {
            PlayerHolder.instance.Drop();
        }
    }

}
