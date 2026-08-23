using System;
using UnityEngine;

public class InputGetter : MonoBehaviour
{
    [SerializeField] private KeyCode ActionKey = KeyCode.F;
    public static InputGetter instance;
    public event Action OnLeftMousePressed;
    public event Action OnRightMousePressed;
    public event Action OnActionMousePressed;
    private void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            OnLeftMousePressed?.Invoke();
        }

        if (Input.GetMouseButton(1))
        {
            OnRightMousePressed?.Invoke();
        }

        if (Input.GetKeyDown(ActionKey))
        {
            OnActionMousePressed?.Invoke();
        }
    }

}
