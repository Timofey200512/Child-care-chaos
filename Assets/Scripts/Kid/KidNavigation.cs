using System;
using System.Collections;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class KidNavigation : MonoBehaviour
{
    private NavMeshAgent _agent;
    [SerializeField] private NavMeshSurface surface;
    [SerializeField] private float idlWaitTimeSeconds = 3f;
    
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;

        StartCoroutine(Wander());
    }

    public IEnumerator Wander()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0f, idlWaitTimeSeconds));

            Vector3 randomPosition = GetRandomPointOnNavMesh();
            _agent.SetDestination(randomPosition);

            while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance)
                yield return null;
        }
    }

    private Vector3 GetRandomPointOnNavMesh()
    {
        Bounds bounds = new Bounds(
            surface.transform.TransformPoint(surface.center),
            Vector3.Scale(surface.size, surface.transform.lossyScale)
        );

        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                0f
            );

            if (NavMesh.SamplePosition(
                    randomPoint,
                    out NavMeshHit hit,
                    0.5f,
                    NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return bounds.center;
    }
}