using System.Collections;
using UnityEngine;

public class KidDesiresManager : MonoBehaviour
{
    [SerializeField] private float minTimeBetweenWaits = 3f;
    [SerializeField] private float maxTimeBetweenWaits = 8f;
    private KidPatience _kidPatience;

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
            yield return new WaitForSeconds(Random.Range(minTimeBetweenWaits, maxTimeBetweenWaits));
            yield return _kidPatience.StartWaiting();
        }
    }
}
