using System;
using UnityEditor.UIElements;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ChangeColorHighlight : MonoBehaviour, IHighlightable
{
    [SerializeField] private Color overlapColor = Color.red;
    [SerializeField] private Color defaultColor = Color.white;
    private SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnDisable()
    {
        print("disabling change color: " + name);
        OnDeHighlighted();
    }

    public void OnHighlighted()
    {
        if (enabled)
        {   
            _spriteRenderer.color = overlapColor;
        }
    }
    
    public void OnDeHighlighted()
    {
        _spriteRenderer.color = defaultColor;
    }
}
