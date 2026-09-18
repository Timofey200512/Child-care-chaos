using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Windows;
using NavMeshPlus.Components;
using NavMeshSurface = NavMeshPlus.Components.NavMeshSurface;

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
    private Color _defaultColor;
    private Coroutine damageFlashCoroutine;

    [SerializeField] private GameObject desireBubble;
    [SerializeField] private SpriteRenderer desireSpriteRenderer;
    [SerializeField] private Sprite desire;
    [SerializeField] private Sprite cryFace;
    [SerializeField] private float damageRedSeconds = 0.25f;
    private KidNavigation kidNavigation;
    [SerializeField] private float minThrowAngle = 20f;
    private NavMeshSurface surface;
    private CancellableCoroutine cryCoroutine;

    private void Awake()
    {
        surface = GameObject.FindAnyObjectByType<NavMeshSurface>();
        kidNavigation = GetComponent<KidNavigation>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _defaultColor = _spriteRenderer.color;
        _changeTimeSeconds = patienceSeconds / 3f;
    }
    
    private bool satisfied;
    [SerializeField] private float throwingForce = 1;

    public IEnumerator StartWaiting()
    {
        satisfied = false;
        print("Starting waiting");
        desireBubble.SetActive(true);
        desireSpriteRenderer.sprite = desire;
        
        _spriteRenderer.sprite = happyFace;

        float elapsedSeconds = 0f;
        int emotionStage = 0;

        while (elapsedSeconds < patienceSeconds && !satisfied)
        {
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

        if (satisfied) yield break;
        cryCoroutine = new CancellableCoroutine(this, Cry());
        yield return cryCoroutine.Wait();
    }

    public void SatisfyDesire()
    {
        print("Satisfied Desire");
        satisfied = true;
        FinishWaiting();
    }

    private void FinishWaiting()
    {
        desireBubble.SetActive(false);
        _spriteRenderer.sprite = happyFace;
    }

    private GameObject chair;
    private Rigidbody chairRigidBody;
    private IEnumerator Cry()
    {
        // find chair
        chair = GameObject.FindWithTag("Chair");
        while (!satisfied)
        {
            print("Crying Cry");
            // set cry sprite
            _spriteRenderer.sprite = cryFace;
            desireBubble.SetActive(false);
            // go towards chair
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
            yield return kidNavigation.Wander(2);
        }
    }

    private void StopCrying()
    {            
        if (cryCoroutine != null)
        {
            _spriteRenderer.sprite = happyFace;
            print("Cry coroutine: " + cryCoroutine);
            cryCoroutine.Cancel(); // something here takes a lot of time
            kidNavigation.StopWalking();
            chair.transform.parent.SetParent(null);
            if (chairRigidBody)
            {
                chairRigidBody.isKinematic = false;
            }
            cryCoroutine = null;
        }
    }

    private int _amountOfSpanks;
    [ContextMenu("Take damage test")]
    public void TakeDamage() // работает
    {
        FlashDamageRed();
        _amountOfSpanks++;
        print("amount of spanks: " + _amountOfSpanks);
        if (_amountOfSpanks == 1)
        {
            StopCrying();
            _amountOfSpanks = 0;
        }
    }

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
    }
    
    
}
