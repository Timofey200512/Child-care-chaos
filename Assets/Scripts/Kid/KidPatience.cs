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
    private Coroutine wanderingCoroutine; 
    public IEnumerator StartWaiting(KidDesire desire)
    {
        //kidNavigation.KeepWalking();
        wanderingCoroutine = StartCoroutine(kidNavigation.Wander());
        //wanderingCoroutine.Resume();
        desireBubble.SetActive(true);
        desireSpriteRenderer.sprite = desire.Image;

        _spriteRenderer.sprite = happyFace;

        float elapsedSeconds = 0f;
        int emotionStage = 0;

        while (elapsedSeconds < patienceSeconds)
        {
            if (desire.IsSatisfied())
            {
                //desire.SetSatisfied();
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

        cryCoroutine = desire.SetUnsatisfied();
        FinishWaiting();
    }

    private void FinishWaiting()
    {
        desireBubble.SetActive(false);
        _spriteRenderer.sprite = happyFace;
        //kidNavigation.StopWalking();
        StopCoroutine(wanderingCoroutine);
    }

    private GameObject chair;
    private Rigidbody chairRigidBody;
    [SerializeField] private Sprite cryFace;
    [SerializeField] private float throwingForce = 1;
    [SerializeField] private float damageRedSeconds = 0.25f;
    [SerializeField] private float minThrowAngle = 20f;
    [SerializeField, Min(0f)] private float chairGrabDistance = 1f;
    public IEnumerator Cry()
    {
        kidNavigation.KeepWalking();
        IsCrying = true;
        while (true)
        {
            // find chair
            chair = GetRandomChair();
            if (chair)
            {
                // set cry sprite
                _spriteRenderer.sprite = cryFace;
                desireBubble.SetActive(false);
                // Keep approaching this chair until it is close enough to grab.
                while (chair && (chair.transform.position - transform.position).sqrMagnitude >
                       chairGrabDistance * chairGrabDistance)
                {
                    yield return kidNavigation.GoTowards(chair.transform.position);
                    // Allow a frame before retrying if movement ended without arrival.
                    yield return null;
                }

                if (!chair)
                {
                    continue;
                }

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
    public void TakeDamage()
    {
        if (cryCoroutine != null)
        {
            _changeColorHighlight.enabled = false;
            FlashDamageRed();
            _currentDamage++;
            if (_currentDamage == totalHealth)
            {
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