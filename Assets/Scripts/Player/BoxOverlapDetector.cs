using System;
using System.Linq;
using UnityEngine;

public class BoxOverlapDetector : MonoBehaviour
{
    private PlayerMovement _playerMovement;
    [SerializeField] private Vector3 overlapBoxOffset;
    [SerializeField] private Vector3 overlapBoxSize = Vector3.one;
    private bool boxOverlapped;
    private Collider overlapedCollider;
    public event Action<Collider> OnOverlap;
    public event Action OnNoOverlap;
    [SerializeField] private LayerMask detectableLayers;
    private void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        InputGetter.instance.OnLeftMousePressed += OnLeftMousePressed;
        InputGetter.instance.OnRightMousePressed += OnRightMousePressed;
        InputGetter.instance.OnActionMousePressed += OnActionMousePressed;
    }
    
    Collider[] colliders;

    // Update is called once per frame
    void Update()
    {
        colliders = DetectOverlaps();

        Collider newCollider = FindClosestCollider(colliders);
        if (newCollider == overlapedCollider)
        {
            return;
        }

        if (overlapedCollider)
        {
            OnNoOverlap?.Invoke();
        }

        overlapedCollider = newCollider;
        boxOverlapped = overlapedCollider;

        if (overlapedCollider)
        {
            OnOverlap?.Invoke(overlapedCollider);
        }
    }

    private Collider FindClosestCollider(Collider[] colliders)
    {
        Collider closestCollider = null;
        float closestDistance = float.MaxValue;
        foreach (var overlapCollider in colliders)
        {
            var distance = Vector3.Distance(overlapCollider.transform.position, transform.position);
            if (distance < closestDistance)
            {
                closestCollider = overlapCollider;
                closestDistance = distance;
            }
        }
        return closestCollider;
    }

    private Vector3 GetFacingOffset()
    {
        return new Vector3(Mathf.Abs(overlapBoxOffset.x) * _playerMovement.GetFacingDirection(), 
            overlapBoxOffset.y, overlapBoxOffset.z);
    }

    private Collider[] DetectOverlaps()
    {
        return Physics.OverlapBox(
        transform.position + GetFacingOffset(),
        overlapBoxSize / 2,
            Quaternion.identity,
        detectableLayers);
    }
    
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position + GetFacingOffset(), overlapBoxSize);
        }
    }
    
    private void OnLeftMousePressed()
    {
        if (boxOverlapped && !PlayerHolder.instance.GotItem)
        {
            overlapedCollider.GetComponent<ILeftClickable>()?.OnLeftButtonClicked();
        }
    }
    
    private void OnActionMousePressed()
    {
        if (boxOverlapped)
        {
            overlapedCollider.GetComponent<IActionButtonClickable>()?.OnActionButtonPressed();
        }
    }
    
    private void OnRightMousePressed()
    {
        if (PlayerHolder.instance.GotItem)
        {
            PlayerHolder.instance.Drop();
        }
    }

    public void OnComponentDetected<TComponent>(Action<TComponent> callback)
    {
        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent(out TComponent component))
            {
                callback(component);
                return;
            }
        }   
    }
}
