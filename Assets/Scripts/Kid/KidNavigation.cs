using System;
using System.Collections;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class KidNavigation : MonoBehaviour
{
    private NavMeshAgent _agent;
    private bool _isGoingToTarget;
    [SerializeField] private NavMeshSurface surface;
    [SerializeField] private float idleWaitTimeSeconds = 3f;
    
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
            yield return new WaitForSeconds(Random.Range(0f, idleWaitTimeSeconds));

            if (_isGoingToTarget)
            {
                continue;
            }

            print("wander");
            Vector3 randomPosition = GetRandomPointOnNavMesh();
            _agent.SetDestination(randomPosition);

            while (_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance)
            {
                if (_isGoingToTarget)
                {
                    print("stop wandering");
                    break;
                }
                print("lalal");

                yield return null;
            }
        }
    }

    public IEnumerator Wander(float wanderTimeSecond)
    {
        float endTime = Time.time + wanderTimeSecond;

        while (Time.time < endTime)
        {
            float waitEndTime = Time.time + Random.Range(0f, idleWaitTimeSeconds);

            while (Time.time < waitEndTime && Time.time < endTime)
            {
                yield return null;
            }

            if (Time.time >= endTime)
            {
                break;
            }

            if (_isGoingToTarget)
            {
                continue;
            }

            Vector3 randomPosition = GetRandomPointOnNavMesh();
            _agent.SetDestination(randomPosition);

            while ((_agent.pathPending || _agent.remainingDistance > _agent.stoppingDistance) && Time.time < endTime)
            {
                if (_isGoingToTarget)
                {
                    break;
                }

                yield return null;
            }
        }

        if (!_isGoingToTarget)
        {
            StopWalking();
        }
    }
    
    private Vector3 GetRandomPointOnNavMesh()
    {
        Bounds bounds = new Bounds(
            surface.transform.TransformPoint(surface.center),
            Vector3.Scale(surface.size, surface.transform.lossyScale));

        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = new Vector3(Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y), 0f);

            if (NavMesh.SamplePosition(randomPoint, 
                    out NavMeshHit hit,
                    0.5f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return bounds.center;
    }

    public IEnumerator GoTowards(Vector3 newPosition)
    {
        Vector3 destination = newPosition;
        if (NavMesh.SamplePosition(newPosition, out NavMeshHit hit, 1f, NavMesh.AllAreas))
        {
            destination = hit.position;
        }

        _isGoingToTarget = true;
        _agent.SetDestination(destination);

        while (_agent.pathPending)
        {
            yield return null;
        }

        while (_agent.isOnNavMesh && _agent.remainingDistance > _agent.stoppingDistance)
        {
            yield return null;
        }

        _isGoingToTarget = false;
    }

    public void StopWalking()
    {
        _agent.ResetPath();
        _isGoingToTarget = false;
    }
}
