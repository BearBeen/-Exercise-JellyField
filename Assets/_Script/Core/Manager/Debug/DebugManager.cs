using UnityEngine;

public class DebugManager : Singleton<DebugManager>
{
    public DebugManager()
    {
    }

    public void Log(string str)
    {
#if UNITY_EDITOR
        Debug.Log(str);
#endif
    }

    public void LogWarning(string str)
    {
#if UNITY_EDITOR
        Debug.LogWarning(str);
#endif
    }

    public void LogError(string str)
    {
#if UNITY_EDITOR
        Debug.LogError(str);
#endif
    }
}