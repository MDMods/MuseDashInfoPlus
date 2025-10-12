using System.Collections;

namespace MDIP.Core.Utilities;

public static class CoroutineUtils
{
    public static IEnumerator Run(Action action, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    public static IEnumerator Run<T>(T instance, Action<T> action, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke(instance);
    }
}