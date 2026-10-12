using System.Collections;
using NavMeshPlus.Components;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class KidNavigation : MonoBehaviour
{
    private NavMeshAgent _agent;

    [SerializeField] private NavMeshSurface surface;
    [SerializeField] private float idleWaitTimeSeconds = 3f;

    private CancellableCoroutine _currentMovement;
    private CancellableCoroutine _wanderingMovement;
    private bool canWalk = true;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();

        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
    }

    // ---------------- PUBLIC MOVEMENT ----------------

    public IEnumerator Wander()
    {
        return StartMovement(WanderRoutine(float.PositiveInfinity), true);
    }

    public IEnumerator Wander(float seconds)
    {
        return StartMovement(WanderRoutine(seconds), true);
    }

    public IEnumerator GoTowards(Vector3 position)
    {
        return StartMovement(GoTowardsRoutine(position));
    }

    private IEnumerator StartMovement(IEnumerator movement, bool isWandering = false)
    {
        CancelMovement();

        _currentMovement = new CancellableCoroutine(movement);
        if (isWandering)
        {
            _wanderingMovement = _currentMovement;
        }
        if (!canWalk)
        {
            _currentMovement.Pause();
        }
        yield return _currentMovement;
    }

    // ---------------- WANDER ----------------

    private IEnumerator WanderRoutine(float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float waitTime = Random.Range(0f, idleWaitTimeSeconds);

            yield return CancellableCoroutine.Wait(waitTime);

            elapsed += waitTime;

            if (elapsed >= duration)
                yield break;

            if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
            {
                yield return null;
                continue;
            }
            print("seting destination");
            _agent.SetDestination(GetRandomPointOnNavMesh());

            while (_agent.pathPending ||
                   _agent.remainingDistance > _agent.stoppingDistance)
            {
                yield return null;
            }
        }
    }

    private Vector3 destination;
    // ---------------- GO TOWARDS ----------------

    private IEnumerator GoTowardsRoutine(Vector3 newPosition)
    {
        if (!_agent.isActiveAndEnabled || !_agent.isOnNavMesh)
        {
            yield break;
        }

        destination = newPosition;
        if (NavMesh.SamplePosition(
                newPosition,
                out NavMeshHit hit,
                1f,
                NavMesh.AllAreas))
        {
            newPosition = hit.position;
        }

        if (!_agent.SetDestination(newPosition))
        {
            yield break;
        }

        while (_agent.pathPending ||
               _agent.remainingDistance > _agent.stoppingDistance)
        {

            yield return null;
        }

    }

    // ---------------- CONTROL ----------------

    public void StopWandering()
    {
        if (_wanderingMovement == null)
            return;

        _wanderingMovement.Cancel();
        _wanderingMovement = null;
        _currentMovement = null;
        ClearPath();
    }

    public void StopWalking()
    {
        _currentMovement?.Pause();
        canWalk = false;
        print("pausing current movement: " + _currentMovement);
        
        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
            _agent.isStopped = true;
    }

    public void KeepWalking()
    {
        _agent.SetDestination(destination);
        _currentMovement?.Resume();
        canWalk = true;
        
        print("continuing current movement: " + _currentMovement);
        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
            _agent.isStopped = false;
    }

    private void CancelMovement()
    {
        _wanderingMovement = null;
        _currentMovement?.Cancel();
        _currentMovement = null;

        ClearPath();
    }

    private void ClearPath()
    {
        if (_agent.isActiveAndEnabled && _agent.isOnNavMesh)
            _agent.ResetPath();
    }

    // ---------------- NAVMESH ----------------

    private Vector3 GetRandomPointOnNavMesh()
    {
        Bounds bounds = new Bounds(
            surface.transform.TransformPoint(surface.center),
            Vector3.Scale(surface.size, surface.transform.lossyScale));

        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                0f);

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

    private void OnDisable()
    {
        CancelMovement();
    }
}