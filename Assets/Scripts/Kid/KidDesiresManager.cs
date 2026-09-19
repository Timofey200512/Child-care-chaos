using System.Collections;
using NavMeshPlus.Components;
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
    private SpriteRenderer _spriteRenderer;
    [SerializeField] private NavMeshSurface surface;
    private Color _defaultColor;
    private KidNavigation kidNavigation;
    private CancellableCoroutine cryCoroutine;
    private void Awake()
    {
        _kidClickBehavior = GetComponent<KidClickBehavior>();
        kidNavigation = GetComponent<KidNavigation>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        print("sprite renderer: " + _spriteRenderer);
        _defaultColor = _spriteRenderer.color;
        _changeColorHighlight = GetComponent<ChangeColorHighlight>();
        _kidPatience = GetComponent<KidPatience>();
    }

    private void Start()
    {
        _desires = new[] { new FoodDesire(this, foodDesire, _kidClickBehavior) };
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
            //yield return kidNavigation.Wander(wanderTimeSeconds); лучше новерное так
            
        }
    }

    private KidDesire ChooseRandomDesire()
    {
        var desire = _desires[Random.Range(0, _desires.Length)];
        desire.Reset();
        return desire;
    }
    
    private GameObject chair;
    private Rigidbody chairRigidBody;
    [SerializeField] private Sprite cryFace;
    [SerializeField] private GameObject desireBubble;
    [SerializeField] private float throwingForce = 1;
    [SerializeField] private float damageRedSeconds = 0.25f;
    [SerializeField] private float minThrowAngle = 20f;
    public IEnumerator Cry()
    {
        while (true)
        {
            // find chair
            chair = GameObject.FindWithTag("Chair");
            if (chair)
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
            }
            yield return kidNavigation.Wander(2);
        }
    }
    
    [ContextMenu("Take damage test")]
    public void TakeDamage() // работает 
    {
        if (cryCoroutine != null)
        {
            _changeColorHighlight.enabled = false;
            FlashDamageRed();
            _amountOfSpanks++;
            print("amount of spanks: " + _amountOfSpanks);
            if (_amountOfSpanks == spanksAmountToCalmDown)
            {
                StopCrying();
                _amountOfSpanks = 0;
            }
        }
    }

    [SerializeField] private Sprite happyFace;
    private void StopCrying()
    {            
        if (cryCoroutine != null)
        {
            _spriteRenderer.sprite = happyFace;
            print("Cry coroutine: " + cryCoroutine);
            cryCoroutine.Cancel(); 
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
    private ChangeColorHighlight _changeColorHighlight;
    [SerializeField] private int spanksAmountToCalmDown = 3;
    
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
