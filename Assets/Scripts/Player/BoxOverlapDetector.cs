using System;
using System.Linq;
using UnityEngine;

public class BoxOverlapDetector : MonoBehaviour
{
    private PlayerMovement _playerMovement;
    [SerializeField] private Vector3 overlapBoxOffset;
    [SerializeField] private Vector3 overlapBoxSize = Vector3.one;
    private bool boxOverlapped;
    private Collider collider;
    public event Action<Collider> OnOverlap;
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
        Collider[] colliders = Physics.OverlapBox(
            transform.position + GetFacingOffset(),
            overlapBoxSize / 2,
            Quaternion.identity);

        collider = FindClosestCollider(colliders);
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
        return new Vector3(Mathf.Abs(overlapBoxOffset.x) * _playerMovement.GetFacingDirection(), overlapBoxOffset.y, overlapBoxOffset.z);
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
            collider.GetComponent<ILeftClickable>().OnLeftButtonClicked();
        }
    }
    
    private void OnActionMousePressed()
    {
        if (boxOverlapped)
        {
            collider.GetComponent<IActionClickable>()?.OnActionButtonPressed();
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
