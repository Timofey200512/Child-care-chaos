using System.Collections;
using UnityEngine;

public class KidDesiresManager : MonoBehaviour
{
    [SerializeField] private float minTimeBetweenWaits = 3f;
    [SerializeField] private float maxTimeBetweenWaits = 8f;
    private KidPatience _kidPatience;
    [SerializeField] private float wanderTimeSeconds;

    private void Awake()
    {
        _kidPatience = GetComponent<KidPatience>();
    }

    private void Start()
    {
        StartCoroutine(RunDesires());
    }

    private IEnumerator RunDesires()
    {
        while (true)
        {
            // пока не чего не происходит и пока wait for seconds возврощается ребенок гуляет из за wander функции которая
            // вызвана в kid navigation в начале
            yield return new WaitForSeconds(Random.Range(minTimeBetweenWaits, maxTimeBetweenWaits));
            yield return _kidPatience.StartWaiting();
            print("exited waiting");
            //yield return kidNavigation.Wander(wanderTimeSeconds); лучше новерное так
            
        }
    }
    

}
