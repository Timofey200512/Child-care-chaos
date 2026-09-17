using System;
using UnityEngine;

public class Spanker : MonoBehaviour
{
    [SerializeField] private Sprite playerPreparesToAttackSprite;
    [SerializeField] private Sprite playerAttackedSprite;
    private BoxOverlapDetector _overlapDetector;

    private void Awake()
    {
        _overlapDetector = GetComponent<BoxOverlapDetector>();
    }
    
    public void Spank()
    {
        // show attack sprites
        
        // use overlap detector 
        
        // deal damage on it if overlaped
        throw new NotImplementedException();
    }
}