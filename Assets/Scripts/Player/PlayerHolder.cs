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
        //item.gameObject.SetActive(false);
        //holdingItem.SetActive(true);
        _playerSpriteRenderer.sprite = holdingPlayerSprite;
        //holdingItemSpriteRenderer.sprite = sprite;
        item.transform.SetParent(gameObject.transform);
        item.transform.localScale = new Vector3(Mathf.Abs(item.transform.localScale.x), 
            item.transform.localScale.y, item.transform.localScale.z);
        item.transform.localPosition = holdingItem.transform.localPosition;
        originalItem = item;
        gotItem = true;
    }

    public void Drop()
    {
        //holdingItem.SetActive(false);
        _playerSpriteRenderer.sprite = defaultPlayerSprite;
        originalItemSpriteRenderer.sortingLayerName = originalItemSortingLayerName;
        originalItemSpriteRenderer.sortingOrder = originalItemSortingOrder;
        //originalItem.gameObject.SetActive(true);
        //originalItem.transform.
        var newPosition = holdingItem.transform.position;
        originalItem.transform.SetParent(null);
        originalItem.OnDrop(newPosition);
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
        if (originalItem.GetComponentInChildren<IUsable>() is IUsable usable)
        {
            holdingItem.SetActive(false);          
            usable.Use(overlapBoxDetector);
        }
    }

    public bool HoldsItemOfTag(string tag)
    {
        return originalItem && originalItem.CompareTag(tag);
    }
}
