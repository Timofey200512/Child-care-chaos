using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KidDesiresManager : MonoBehaviour
{
    [SerializeField] private float minTimeBetweenWaits = 3f;
    [SerializeField] private float maxTimeBetweenWaits = 8f;
    private KidPatience _kidPatience;
    private KidDesire[] _desires;
    [SerializeField] private Sprite foodDesire;
    [SerializeField] private Sprite sleepingSprite;
    [SerializeField, Min(0f)] private float minSleepTime = 3f;
    [SerializeField, Min(0f)] private float maxSleepTime = 8f;
    private KidClickBehavior _kidClickBehavior;
    private KidNavigation kidNavigation;
    [SerializeField] private Sprite sleepingDesire;
    private LayerMask originalLayerMask;

    private void Awake()
    {
        _kidClickBehavior = GetComponent<KidClickBehavior>();
        _kidPatience = GetComponent<KidPatience>();
    }

    private void Start()
    {
        originalLayerMask = gameObject.layer;
        SleepDesire sleepDesire = new SleepDesire(_kidPatience, GetComponent<SpriteRenderer>(),
            sleepingSprite, minSleepTime, maxSleepTime, sleepingDesire);
        sleepDesire.OnSleep += DisableOverlap;
        _desires = new KidDesire[]
        {
            new FoodDesire(_kidPatience, foodDesire, _kidClickBehavior),
            sleepDesire
        };
        StartCoroutine(RunDesires());
    }

    private IEnumerator RunDesires()
    {
        while (true)
        {
            // пока не чего не происходит и пока wait for seconds возврощается ребенок гуляет из за wander функции которая
            // вызвана в kid navigation в начале
            yield return new WaitForSeconds(Random.Range(minTimeBetweenWaits, maxTimeBetweenWaits));
            var  desire = ChooseRandomDesire();
            yield return _kidPatience.StartWaiting(desire);
            yield return desire.ActivateAftermath();
            EnableOverlap();
            print("exited waiting");
            //yield return kidNavigation.Wander(3);
            //yield return kidNavigation.Wander(wanderTimeSeconds); лучше новерное так
            
        }
    }

    private KidDesire ChooseRandomDesire()
    {
        var desire = _desires[Random.Range(0, _desires.Length)];
        desire.Reset();
        return desire;
    }

    private void EnableOverlap()
    {
        gameObject.layer = originalLayerMask;
    }

    private void DisableOverlap()
    {
        gameObject.layer = LayerMask.NameToLayer("Ignore overlap");
    }
    
}
