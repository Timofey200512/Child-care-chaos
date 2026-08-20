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
    public void OnHighlighted()
    {
        print("changing color to highlighted");
        _spriteRenderer.color = overlapColor;
    }
    
    public void OnDeHighlighted()
    {
        print("dehighlighted");
        _spriteRenderer.color = defaultColor;
    }
}
