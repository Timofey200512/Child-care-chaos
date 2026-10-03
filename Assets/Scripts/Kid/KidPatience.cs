using System;
using System.Collections;
using NavMeshPlus.Components;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(KidNavigation))]
public class KidPatience : MonoBehaviour
{
    [SerializeField] private float patienceSeconds = 5f;
    [SerializeField] private Sprite happyFace;
    [SerializeField] private Sprite neutralFace;
    [SerializeField] private Sprite sadFace;

    private SpriteRenderer _spriteRenderer;
    private float _changeTimeSeconds;

    [SerializeField] private GameObject desireBubble;
    [SerializeField] private SpriteRenderer desireSpriteRenderer;
    private CancellableCoroutine cryCoroutine;
    [SerializeField] private NavMeshSurface surface;
    private Color _defaultColor;
    private KidNavigation kidNavigation;
    public bool IsCrying { get; private set; }

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        kidNavigation = GetComponent<KidNavigation>();
        _defaultColor = _spriteRenderer.color;
        _changeColorHighlight = GetComponent<ChangeColorHighlight>();
        _changeTimeSeconds = patienceSeconds / 3f;
    }

    public IEnumerator StartWaiting(KidDesire desire)
    {
        //kidNavigation.KeepWalking();
        StartCoroutine(kidNavigation.Wander());
        desireBubble.SetActive(true);
        desireSpriteRenderer.sprite = desire.Image;

        _spriteRenderer.sprite = happyFace;

        float elapsedSeconds = 0f;
        int emotionStage = 0;

        while (elapsedSeconds < patienceSeconds)
        {
            if (desire.IsSatisfied())
            {
                FinishWaiting();
                yield break;
            }

            elapsedSeconds += Time.deltaTime;

            if (emotionStage == 0 && elapsedSeconds >= _changeTimeSeconds)
            {
                _spriteRenderer.sprite = neutralFace;
                emotionStage = 1;
            }
            else if (emotionStage == 1 && elapsedSeconds >= _changeTimeSeconds * 2f)
            {
                _spriteRenderer.sprite = sadFace;
                emotionStage = 2;
            }

            yield return null;
        }

        cryCoroutine = new CancellableCoroutine(desire.NotSatisfiedAction());
        yield return cryCoroutine;
    }

    private void FinishWaiting()
    {
        desireBubble.SetActive(false);
        _spriteRenderer.sprite = happyFace;
    }

    private GameObject chair;
    private Rigidbody chairRigidBody;
    [SerializeField] private Sprite cryFace;
    [SerializeField] private float throwingForce = 1;
    [SerializeField] private float damageRedSeconds = 0.25f;
    [SerializeField] private float minThrowAngle = 20f;
    public IEnumerator Cry()
    {
        IsCrying = true;
        while (true)
        {
            // find chair
            chair = GetRandomChair();
            if (chair)
            {
                //print("Crying Cry");
                // set cry sprite
                _spriteRenderer.sprite = cryFace;
                desireBubble.SetActive(false);
                // go towards chair
                //kidNavigation.StopWalking();
                yield return kidNavigation.GoTowards(chair.transform.position);
                // grab it
                chair.transform.parent.SetParent(transform);
                chair.transform.parent.localPosition = new Vector3(0f, 1f, 0f);
                chair.transform.localPosition = Vector3.zero;

                chairRigidBody = chair.GetComponent<Rigidbody>();
                chairRigidBody.isKinematic = true;

                yield return new WaitForSeconds(1);
                surface.BuildNavMesh();

                // throw it
                chair.transform.parent.SetParent(null);
                chairRigidBody.isKinematic = false;
                chair.transform.localScale = new Vector3(1f, 1f, 1f);

                var angle = Random.Range(minThrowAngle, 90f);
                var direction = Random.insideUnitCircle.normalized;

                Vector3 throwDirection = new Vector3(
                    direction.x * Mathf.Cos(angle * Mathf.Deg2Rad),
                    Mathf.Sin(angle * Mathf.Deg2Rad),
                    direction.y * Mathf.Cos(angle * Mathf.Deg2Rad)
                );
                chairRigidBody.useGravity = true;
                chairRigidBody.AddForce(throwDirection * throwingForce, ForceMode.Impulse);
                chairRigidBody.AddTorque(throwDirection * throwingForce / 3, ForceMode.Impulse);
            }
            yield return kidNavigation.Wander(2);
        }
    }

    private GameObject GetRandomChair()
    {
        GameObject[] chairs = GameObject.FindGameObjectsWithTag("Chair");
        return chairs.Length > 0 ? chairs[Random.Range(0, chairs.Length)] : null;
    }

    [ContextMenu("Take damage test")]
    public void TakeDamage() // работает
    {
        if (cryCoroutine != null)
        {
            _changeColorHighlight.enabled = false;
            FlashDamageRed();
            _currentDamage++;
            //print("amount of spanks: " + _currentDamage);
            if (_currentDamage == totalHealth)
            {
               // print("stop crying");
                StopCrying();
                _currentDamage = 0;
            }
        }
    }

    private void StopCrying()
    {
        IsCrying = false;
        if (cryCoroutine != null)
        {
            _spriteRenderer.sprite = happyFace;
            cryCoroutine.Cancel();
            //walkingTowardsCoroutine.Cancel();
            kidNavigation.Wander();
            chair.transform.parent.SetParent(null);
            if (chairRigidBody)
            {
                chairRigidBody.isKinematic = false;
            }
            cryCoroutine = null;
        }
    }

    private int _currentDamage;
    private ChangeColorHighlight _changeColorHighlight;
    [SerializeField] private int totalHealth = 3;

    private Coroutine damageFlashCoroutine;
    //private CancellableCoroutine walkingTowardsCoroutine;

    private void FlashDamageRed()
    {
        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
        }

        damageFlashCoroutine = StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        _spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(damageRedSeconds);
        _spriteRenderer.color = _defaultColor;
        damageFlashCoroutine = null;
        _changeColorHighlight.enabled = true;
    }
}

public abstract class KidDesire
{
    public Sprite Image => image;
    protected Sprite image;

    public abstract bool IsSatisfied();
    public abstract void Reset();
    public abstract IEnumerator NotSatisfiedAction();
}

public class SleepDesire : KidDesire
{
    public override bool IsSatisfied()
    {
        throw new System.NotImplementedException();
    }

    public override void Reset()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerator NotSatisfiedAction()
    {
        throw new NotImplementedException();
    }
}
