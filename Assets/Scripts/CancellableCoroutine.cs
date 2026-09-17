using System.Collections;

public class CancellableCoroutine
{
    private bool _stopped;
    private readonly IEnumerator _enumerator;

    public CancellableCoroutine(IEnumerator enumerator)
    {
        _enumerator = enumerator;
    }

    public IEnumerator Run()
    {
        while (!_stopped && _enumerator.MoveNext())
        {
            yield return _enumerator.Current;
        }
    }

    public void CancelCoroutine()
    {
        _stopped = true;
    }
}