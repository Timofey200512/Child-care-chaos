using System;
using System.Collections;
using UnityEngine;

public class WaterGunClickBehavior : Item, IUsable
{
    private SpriteRenderer waterGunSpriteRenderer;
    [SerializeField] private GameObject squirtImage;

    protected override void Awake()
    {        
        base.Awake();
        waterGunSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void OnPickedUp()
    {
        PlayerHolder.instance.Hold(this, waterGunSpriteRenderer);
    }

    protected override void OnDropped(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    public void Use(BoxOverlapDetector overlapBoxDetector)
    {
        StartCoroutine(Squirt(overlapBoxDetector));
    }
    
    private IEnumerator Squirt(BoxOverlapDetector overlapBoxDetector)
    {
        squirtImage.SetActive(true);
        overlapBoxDetector.OnComponentDetected<KidPatience>(kidPatience => kidPatience.TakeDamage());
        yield return new WaitForSeconds(1);
        squirtImage.SetActive(false);
    }
}
