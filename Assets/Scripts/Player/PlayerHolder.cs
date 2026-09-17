using System;
using UnityEngine;
using UnityEngine.AI;

public class PlayerHolder : MonoBehaviour
{
    public static PlayerHolder instance;
    private SpriteRenderer _playerSpriteRenderer;
    private SpriteRenderer holdingItemSpriteRenderer;
    [SerializeField] private Sprite holdingPlayerSprite;
    [SerializeField] private Sprite defaultPlayerSprite;
    [SerializeField] private GameObject holdingItem;   
    private GameObject originalItem;

    private bool gotItem;
    public bool GotItem => gotItem;

    public GameObject Item => originalItem;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        holdingItemSpriteRenderer = holdingItem.GetComponent<SpriteRenderer>();
        _playerSpriteRenderer = GetComponent<SpriteRenderer>();
        InputGetter.instance.OnLeftMousePressed += UseHoldingItem;
    }

    public void Hold(GameObject item, Sprite sprite)
    {
         item.gameObject.SetActive(false);
         holdingItem.SetActive(true);
         _playerSpriteRenderer.sprite = holdingPlayerSprite;
         holdingItemSpriteRenderer.sprite = sprite;
         originalItem = item;
         gotItem = true;
    }

    public void Drop()
    {
        holdingItem.SetActive(false);
        _playerSpriteRenderer.sprite = defaultPlayerSprite;
        originalItem.gameObject.SetActive(true);
        var newPosition = holdingItem.transform.position;
        print("new position: " + newPosition);
        Item.GetComponentInChildren<IDropable>().OnDrop(newPosition);
        gotItem = false;
    }

    public void DeleteItem()
    {
        Destroy(originalItem);
        _playerSpriteRenderer.sprite = defaultPlayerSprite;
        holdingItem.SetActive(false);
        gotItem = false;
    }

    private void UseHoldingItem()
    {
        if (!gotItem) return;
        if (originalItem.TryGetComponent<IUsable>(out var usable))
        {
            holdingItem.SetActive(false);          
            usable.Use();
        }
    }
}

public interface IUsable
{
    void Use();
}
