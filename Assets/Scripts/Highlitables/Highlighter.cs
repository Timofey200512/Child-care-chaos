using System;
using UnityEngine;

[RequireComponent(typeof(BoxOverlapDetector))]
public class Highlighter : MonoBehaviour
{
    private IHighlightable _highlightedObject;
    private Collider2D collider;
    private BoxOverlapDetector _boxOverlapDetector;

    private void Awake()
    {
        _boxOverlapDetector = GetComponent<BoxOverlapDetector>();
        _boxOverlapDetector.OnOverlap += Highlight;
        _boxOverlapDetector.OnOnOverlap += DeHighlight;
    }

    private void Highlight(Collider collider)
    {
        if (collider.TryGetComponent<IHighlightable>(out var highlightable))
        {
            highlightable.OnHighlighted();
            _highlightedObject = highlightable;
        }
    }

    private void DeHighlight()
    {
        _highlightedObject?.OnDeHighlighted();
    }
}

