using System;
using UnityEngine;

public abstract class Item : MonoBehaviour, IActionButtonClickable
{
    private Collider collider;
    private Rigidbody rigidBody;
    protected Transform parent;
    [SerializeField] private GameObject shadow;

    protected virtual void Awake()
    {
        collider = GetComponent<Collider>();
        rigidBody = GetComponent<Rigidbody>();
        parent = transform.parent;
    }

    public virtual void OnActionButtonPressed()
    {
        if (PlayerHolder.instance.GotItem) return;
        
        SetPickedUp(true);
        OnPickedUp();
    }

    public void OnDrop(Vector3 newPosition)
    {
        transform.SetParent(parent);
        SetPickedUp(false);
        OnDropped(newPosition);
    }

    protected void SetPickedUp(bool pickedUp)
    {
        collider.enabled = !pickedUp;
        if (rigidBody)
        {
            rigidBody.isKinematic = pickedUp;
        }
        shadow.SetActive(!pickedUp);
    }

    protected abstract void OnPickedUp();

    protected abstract void OnDropped(Vector3 newPosition);
}
