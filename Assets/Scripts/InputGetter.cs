using System;
using UnityEngine;

public class InputGetter : MonoBehaviour
{
    public static InputGetter instance;
    public event Action OnLeftMousePressed;
    public event Action OnRightMousePressed;
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
    }
}
