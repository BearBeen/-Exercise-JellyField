using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewJellyBox", menuName = "ScriptableObjects/JellyBoxConfig")]
public class JellyBoxConfig : ScriptableObject
{
    public List<JellyConfig> Jellies = new List<JellyConfig>();

    [ContextMenu("Test Jelly Box")]
    private void Test()
    {
        int surface = 0;
        HashSet<int> indices = new HashSet<int>();
        for (int i = 0, length = Jellies.Count; i < length; i++)
        {
            if (!Jelly.TryLayout(Jellies[i].Layout, ref surface))
            {
                DebugManager.Instance.LogError($"{name}: Jellies overflow !!!");
                return;
            }
            if (!indices.Add(Jellies[i].Index))
            {
                DebugManager.Instance.LogError($"{name}: duplicate jelly's index !!!");
                return;
            }
        }
        if (surface != Jelly.FULL_SURFACE && Jellies.Count > 0)
        {
            DebugManager.Instance.LogWarning($"{name}: box has free space. Runtime-created JellyBox will expand those jellys for you, but be aware that this layout may not be as you intend.");
        }
        else
        {
            DebugManager.Instance.Log($"{name}: good box.");
        }
    }
}

[Serializable]
public class JellyConfig
{
    public JellyLayout Layout;
    public int Index;
}
