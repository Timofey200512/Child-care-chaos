using UnityEngine;

public class BeltClickBehavior : Item, IUsable
{
    private SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    protected override void OnPickedUp()
    {
        transform.parent.gameObject.SetActive(false);
        PlayerHolder.instance.Hold(this, spriteRenderer);
    }

    protected override void OnDropped(Vector3 newPosition)
    {
        transform.parent.gameObject.SetActive(true);
        transform.position = newPosition;
    }

    public void Use(BoxOverlapDetector overlapBoxDetector)
    {
        print("spanking"); 
        // spanking method
        //overlapBoxDetector.Spank(callback);
    }
}
