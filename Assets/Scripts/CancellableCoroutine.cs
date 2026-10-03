// using System.Collections;
// using UnityEngine;
//
// public class CancellableCoroutine
// {
//     private MonoBehaviour _runner;
//     private Coroutine _coroutine;
//
//     private bool IsRunning { get; set; }
//
//     public CancellableCoroutine(MonoBehaviour runner, IEnumerator routine)
//     {
//         _runner = runner;
//         _coroutine = _runner.StartCoroutine(Run(routine));
//     }
//
//     private IEnumerator Run(IEnumerator routine)
//     {
//         IsRunning = true;
//
//         yield return routine;
//
//         IsRunning = false;
//         _coroutine = null;
//     }
//
//     public void Cancel()
//     {
//         if (_coroutine == null) return;
//         _runner.StopCoroutine(_coroutine);
//
//         _coroutine = null;
//         IsRunning = false;
//     }
//
//     public IEnumerator Wait()
//     {
//         while (IsRunning)
//             yield return null;
//     }
// }
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancellableCoroutine : IEnumerator
{
    private readonly Stack<IEnumerator> _stack = new();

    private bool _paused;
    private bool _cancelled;

    public object Current { get; private set; }

    public CancellableCoroutine(IEnumerator routine)
    {
        _stack.Push(routine);
    }

    public bool MoveNext()
    {
        // 1. Check cancellation BEFORE moving anything
        if (_cancelled)
        {
            _stack.Clear();
            return false;
        }

        // 2. If paused, don't MoveNext() any of our routines
        if (_paused)
        {
            Current = null;
            return true;
        }

        // 3. Run whatever IEnumerator is currently on top
        while (_stack.Count > 0)
        {
            IEnumerator routine = _stack.Peek();

                //Debug.Log("Advancing: " + routine);

            // Advance that IEnumerator
            if (!routine.MoveNext())
            {
                _stack.Pop();
                continue;
            }

            object yielded = routine.Current;

            // Did it yield another IEnumerator?
            if (yielded is IEnumerator nested)
            {
                _stack.Push(nested);
                continue;
            }

            // Something like null was yielded.
            // Give it to Unity.
            Current = yielded;
            return true;
        }

        return false;
    }

    public void Pause()
    {
        _paused = true;
    }

    public void Resume()
    {
        _paused = false;
    }

    public void Cancel()
    {
        _cancelled = true;
    }

    public void Reset()
    {
        throw new NotSupportedException();
    }

    public static IEnumerator Wait(float seconds)
    {
        float remaining = seconds;
        
        while (remaining > 0)
        {
            remaining -= Time.deltaTime;

            // Return control every frame!
            yield return null;
        }
    }
}