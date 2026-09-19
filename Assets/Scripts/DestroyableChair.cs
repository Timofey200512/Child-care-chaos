using UnityEngine;

public class DestroyableChair : MonoBehaviour
{
    [SerializeField] private Sprite destroyedChair;
    [SerializeField] private float breakingVelocity = 5f;

    private SpriteRenderer _spriteRenderer;
    private bool _isBroken;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        HouseHealth.IncreaseChairAmount();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_isBroken)
            return;

        if (collision.relativeVelocity.magnitude >= breakingVelocity)
        {
            BreakChair();
        }
    }

    private void BreakChair()
    {
        _isBroken = true;
        HouseHealth.DecreaseChairAmount();

        _spriteRenderer.sprite = destroyedChair;
        gameObject.tag = "BrokenChair";
    }
}
