using System.Collections;
using UnityEngine;

public class KidDesiresManager : MonoBehaviour
{
    [SerializeField] private float minTimeBetweenWaits = 3f;
    [SerializeField] private float maxTimeBetweenWaits = 8f;
    private KidPatience _kidPatience;
    [SerializeField] private float wanderTimeSeconds;
    private KidDesire[] _desires;
    [SerializeField] private Sprite foodDesire;
    private KidClickBehavior _kidClickBehavior;
    private KidNavigation kidNavigation;

    private void Awake()
    {
        _kidClickBehavior = GetComponent<KidClickBehavior>();
        _kidPatience = GetComponent<KidPatience>();
    }

    private void Start()
    {
        _desires = new[] { new FoodDesire(_kidPatience, foodDesire, _kidClickBehavior) };
        StartCoroutine(RunDesires());
    }

    private IEnumerator RunDesires()
    {
        while (true)
        {
            // пока не чего не происходит и пока wait for seconds возврощается ребенок гуляет из за wander функции которая
            // вызвана в kid navigation в начале
            yield return new WaitForSeconds(Random.Range(minTimeBetweenWaits, maxTimeBetweenWaits));
            yield return _kidPatience.StartWaiting(ChooseRandomDesire());
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
}
