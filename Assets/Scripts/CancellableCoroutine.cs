using System.Collections;
using UnityEngine;

public class CancellableCoroutine
{
    private MonoBehaviour _runner;
    private Coroutine _coroutine;

    private bool IsRunning { get; set; }

    public CancellableCoroutine(MonoBehaviour runner, IEnumerator routine)
    {
        _runner = runner;
        _coroutine = _runner.StartCoroutine(Run(routine));
    }

    private IEnumerator Run(IEnumerator routine)
    {
        IsRunning = true;

        yield return routine;

        IsRunning = false;
        _coroutine = null;
    }

    public void Cancel()
    {
        if (_coroutine == null) return;
        _runner.StopCoroutine(_coroutine);

        _coroutine = null;
        IsRunning = false;
    }

    public IEnumerator Wait()
    {
        while (IsRunning)
            yield return null;
    }
}