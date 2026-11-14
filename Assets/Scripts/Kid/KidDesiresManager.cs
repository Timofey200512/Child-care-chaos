using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class KidDesiresManager : MonoBehaviour
{
    [SerializeField] private float minTimeBetweenWaits = 3f;
    [SerializeField] private float maxTimeBetweenWaits = 8f;
    private KidPatience _kidPatience;
    private KidDesire[] _desires;
    private KidDesire currentDesire;
    public bool WantsToSleep => currentDesire is SleepDesire && !currentDesire.IsSatisfied();
    [SerializeField] private Sprite foodDesire;
    [SerializeField] private Sprite sleepingSprite;
    [SerializeField, Min(0f)] private float minSleepTime = 3f;
    [SerializeField, Min(0f)] private float maxSleepTime = 8f;
    [SerializeField] private bool alwaysSleepForTesting;
    private KidNavigation kidNavigation;
    [SerializeField] private Sprite sleepingDesire;
    private LayerMask originalLayerMask;
    private IEnumerator aftermath;
    private SleepDesire sleepDesire;
    private CancellableCoroutine runDesires;
    private NavMeshAgent agent;
    private Rigidbody body;
    private Collider kidCollider;
    private Vector3 lastWalkingDirection = Vector3.right;
    private Vector3 lastPosition;
    private Quaternion standingRotation;
    private bool wandering;
    private bool falling;
    private bool landed;
    private bool descending;
    [SerializeField, Min(0f)] private float fallForce = 4f;
    [SerializeField, Min(0f)] private float upwardFallForce = 4f;
    private KidClickBehavior _kidClickBehavior;

    private void Awake()
    {
        _kidClickBehavior = GetComponent<KidClickBehavior>();
        _kidPatience = GetComponent<KidPatience>();
        kidNavigation = GetComponent<KidNavigation>();
        agent = GetComponent<NavMeshAgent>();
        kidCollider = GetComponent<Collider>();
        body = GetComponent<Rigidbody>();
        if (!body) body = gameObject.AddComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        standingRotation = transform.rotation;
        lastPosition = transform.position;
    }
    //-_____________________________ fix direction of falling
    private void Start()
    {
        originalLayerMask = gameObject.layer;
        sleepDesire = new SleepDesire(_kidPatience, GetComponent<SpriteRenderer>(),
            sleepingSprite, minSleepTime, maxSleepTime, sleepingDesire);
        sleepDesire.OnSleep += DisableOverlap;
        sleepDesire.OnSleep += () => Sleeping = true;
        //sleepDesire.OnSleep += SetColliderAsTrigger;
        sleepDesire.OnAwake += EnableOverlap;
        sleepDesire.OnAwake += () => Sleeping = false;
        //sleepDesire.OnAwake += SetColliderAsSolid;
        _desires = new KidDesire[]
        {
            new FoodDesire(_kidPatience, foodDesire, _kidClickBehavior),
            sleepDesire
        };
        
        if (alwaysSleepForTesting)
        {
            kidNavigation.StopWandering();
            sleepDesire.SleepForeverForTesting();
            return;
        }
        
        runDesires = new CancellableCoroutine(RunDesires());
        StartCoroutine(runDesires);
    }

    private IEnumerator RunDesires()
    {
        while (true)
        {
            // пока не чего не происходит и пока wait for seconds возврощается ребенок гуляет из за wander функции которая
            // вызвана в kid navigation в начале
            wandering = true;
            StartCoroutine(kidNavigation.Wander());
            yield return new WaitForSeconds(Random.Range(minTimeBetweenWaits, maxTimeBetweenWaits));
            wandering = false;
            kidNavigation.StopWandering();
            var desire = ChooseRandomDesire();
            currentDesire = desire;
            yield return _kidPatience.StartWaiting(desire);
            currentDesire = null;
            aftermath = desire.ActivateAftermath();
            yield return aftermath;
            //EnableOverlap();
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

    
    private void OnTriggerEnter(Collider other)
    {
        //CheckForLanding(other);   
        
        print("collision");
        var kidDesiresManager = other.gameObject.GetComponentInParent<KidDesiresManager>();
        if (!kidDesiresManager || !kidDesiresManager.CompareTag("Kid") || kidDesiresManager == this ||
            !kidDesiresManager.Sleeping || !wandering || falling)
            return;

        Vector3 direction = lastWalkingDirection;
        if (agent && agent.enabled && agent.isOnNavMesh && agent.velocity.sqrMagnitude > 0.01f)
            direction = agent.velocity.normalized;
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) direction = Vector3.right;
        direction.Normalize();

        // stop the main loop rundesires one
        runDesires?.Pause();
        wandering = false;
        kidNavigation.StopWandering();

        // calculate direction of falling
        // direction has been captured above from current/last movement

        // give impulse in that direction
        // turn side ways either left or right depending on what direction it is falling
        // wake up the sleeping kid
        kidDesiresManager.CancelAftermath();
        StartCoroutine(FallAndCry(direction));
    }

    private void Update()
    {
        Vector3 delta = transform.position - lastPosition;
        delta.y = 0f;
        if (delta.sqrMagnitude > 0.0001f)
            lastWalkingDirection = delta.normalized;
        lastPosition = transform.position;
    }

    private IEnumerator FallAndCry(Vector3 direction)
    {
        falling = false;
        // landed = false;
        // descending = false;
        // kidNavigation.StopWalking();
        agent.enabled = false;
        body.isKinematic = false;
        body.useGravity = true;
        // body.linearVelocity = Vector3.zero;
        // body.angularVelocity = Vector3.zero;

        // turn side ways either left or right depending on what direction it is falling
        Vector3 currentAngles = transform.eulerAngles;
        body.AddForce(Vector3.right * fallForce + Vector3.up * upwardFallForce, ForceMode.Impulse);
        // print("force " + upwardFallForce);
        transform.rotation = Quaternion.Euler(currentAngles.x, currentAngles.y,
            direction.x >= 0f ? -90f : 90f);
        
        yield return new WaitForSeconds(0.1f);
        landed = false;
        //falling = true;
        // //Debug.Break();
        //
        // // wait until the fall hits the ground
        while (!landed)
             yield return null;
        //
        // print("exit!");
        //
         // when fall is complete fix orientation and enter cry state
         // fix orientation
         body.linearVelocity = Vector3.zero;
         body.angularVelocity = Vector3.zero;
         body.useGravity = false;
         body.isKinematic = true;
         transform.rotation = standingRotation;
         if (agent)
         {
             agent.enabled = true;
             agent.Warp(transform.position);
         }
         kidNavigation.KeepWalking();
         falling = false;
        
         // crying
         _kidPatience.StartCrying();
         while (_kidPatience.IsCrying) yield return null;
        
         // come back to the main loop
         runDesires?.Resume();
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckForLanding(collision);
    }

    // private void OnCollisionStay(Collision collision)
    // {
    //     CheckForLanding(collision);
    // }

    private void CheckForLanding(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Walkable tiles"))
        {
            print("landed");
            landed = true;
        }
        // if (!falling || !descending ||
        //     collision.gameObject.layer != LayerMask.NameToLayer("Walkable tiles"))
        //     return;
        //
        // foreach (ContactPoint contact in collision.contacts)
        // {
        //     if (contact.normal.y > 0.5f)
        //     {
        //         landed = true;
        //         return;
        //     }
        // }
    }

    private void FixedUpdate()
    {
        if (falling && body && !body.isKinematic && body.linearVelocity.y < -0.05f)
            descending = true;
    }

    // private void SetColliderAsTrigger()
    // {
    //     if (kidCollider) kidCollider.isTrigger = true;
    // }
    //
    // private void SetColliderAsSolid()
    // {
    //     if (kidCollider) kidCollider.isTrigger = false;
    // }

    private void CancelAftermath()
    {
        sleepDesire?.WakeFromTrip();
    }

    public bool Sleeping { get; private set; }
}
