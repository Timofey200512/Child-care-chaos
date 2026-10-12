using System;
using System.Collections;
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
    private Item originalItem;

    private bool gotItem;
    private BoxOverlapDetector overlapBoxDetector;
    private int originalItemSortingOrder;
    private string originalItemSortingLayerName;
    private SpriteRenderer originalItemSpriteRenderer;
    public bool GotItem => gotItem;
    public Item HeldItem => GotItem ? originalItem : null;

    private void Awake()
    {
        overlapBoxDetector = GetComponent<BoxOverlapDetector>();
        instance = this;
    }

    void Start()
    {
        holdingItemSpriteRenderer = holdingItem.GetComponent<SpriteRenderer>();
        _playerSpriteRenderer = GetComponent<SpriteRenderer>();
        InputGetter.instance.OnLeftMousePressed += UseHoldingItem;
    }

    public void Hold(Item item, SpriteRenderer spriteRenderer)
    {
        originalItemSortingOrder = spriteRenderer.sortingOrder;
        originalItemSortingLayerName = spriteRenderer.sortingLayerName;
        originalItemSpriteRenderer = spriteRenderer;
        spriteRenderer.sortingLayerName = _playerSpriteRenderer.sortingLayerName;
        spriteRenderer.sortingOrder = _playerSpriteRenderer.sortingOrder + 1;
        _playerSpriteRenderer.sprite = holdingPlayerSprite;
        item.transform.SetParent(gameObject.transform);
        item.transform.localScale = new Vector3(Mathf.Abs(item.transform.localScale.x), 
            item.transform.localScale.y, item.transform.localScale.z);
        item.transform.localPosition = holdingItem.transform.localPosition;
        originalItem = item;
        gotItem = true;
    }

    public Item Drop()
    {
        _playerSpriteRenderer.sprite = defaultPlayerSprite;
        originalItemSpriteRenderer.sortingLayerName = originalItemSortingLayerName;
        originalItemSpriteRenderer.sortingOrder = originalItemSortingOrder;
        var newPosition = holdingItem.transform.position;
        originalItem.transform.SetParent(null);
        originalItem.OnDrop(newPosition);
        gotItem = false;
        var item = originalItem;
        originalItem = null;
        return item;
    }

    public void DeleteItem()
    {
        Destroy(originalItem);
        _playerSpriteRenderer.sprite = defaultPlayerSprite;
        holdingItem.SetActive(false);
        gotItem = false;
        originalItem = null;
    }

    private void UseHoldingItem()
    {
        if (!gotItem) return;
        if (originalItem.GetComponentInChildren<IUsable>() is IUsable usable)
        {
            holdingItem.SetActive(false);          
            usable.Use(overlapBoxDetector);
        }
    }

    public bool HoldsItemOfTag(string tag)
    {
        print("original item: " + originalItem);
        print("tag: " + tag);
        return originalItem && originalItem.CompareTag(tag);
    }
}
