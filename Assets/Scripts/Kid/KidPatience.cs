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
    
    [SerializeField] private GameObject desireBubble;
    [SerializeField] private SpriteRenderer desireSpriteRenderer;
    [SerializeField] private Sprite desire;
    [SerializeField] private Sprite cryFace;
    private KidNavigation kidNavigation;
    [SerializeField] private float minThrowAngle = 20f;
    [SerializeField] private NavMeshSurface surface;

    private void Awake()
    {
        surface = GameObject.FindAnyObjectByType<NavMeshSurface>();
        kidNavigation = GetComponent<KidNavigation>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
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
        yield return Cry();
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

    private IEnumerator Cry()
    {
        //while (!satisfied)
        //{
            print("Crying Cry");
            // set cry sprite
            _spriteRenderer.sprite = cryFace;
            desireBubble.SetActive(false);
            // find chair
            GameObject chair = GameObject.FindWithTag("Chair");
            // go towards it
            yield return kidNavigation.GoTowards(chair.transform.position);
            // grab it
            chair.transform.parent.SetParent(transform);
            chair.transform.parent.localPosition = new Vector3(0f, 1f, 0f);
            chair.transform.localPosition = Vector3.zero;

            var chairRigidBody = chair.GetComponent<Rigidbody>();
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
     //   }
    }
    
}
