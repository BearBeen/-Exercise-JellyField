using System;
using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class AssetManager
{
    private void AddBuiltInAssetLoader(string resPath, string builtInAssetPath, Action<UnityEngine.Object> completedCallback = null)
    {
        BuiltInAssetLoader builtInAssetLoader = new BuiltInAssetLoader(resPath, builtInAssetPath);
        builtInAssetLoader.AddCallback(completedCallback);
        _loadingAssetDic.Add(resPath, builtInAssetLoader);
        _loadingAssetQueue.Enqueue(builtInAssetLoader);
    }

    private void AddAssetBundleAssetLoader(string resPath, string assetBundlePath, Action<UnityEngine.Object> completedCallback = null)
    {
        string[] depPaths = _manifest.GetDirectDependencies(resPath);
        for(int i = 0; i < depPaths.Length; i++)
        {
            LoadAssetAsync(depPaths[i]);
        }
        AssetBundleLoader assetBundleLoader = new AssetBundleLoader(resPath, assetBundlePath);
        assetBundleLoader.AddCallback(completedCallback);
        _loadingAssetDic.Add(resPath, assetBundleLoader);
        _loadingAssetQueue.Enqueue(assetBundleLoader);
    }

#if UNITY_EDITOR
    private void AddEditorAssetLoader(string resPath, Action<UnityEngine.Object> completedCallback = null)
    {
        EditorAssetLoader editorAssetLoader = new EditorAssetLoader(resPath);
        editorAssetLoader.AddCallback(completedCallback);
        _loadingAssetDic.Add(resPath, editorAssetLoader);
        _loadingAssetQueue.Enqueue(editorAssetLoader);
    }
#endif

    private void AddAssetLoader(string resPath, string assetBundlePath, string builtInAssetPath, Action<UnityEngine.Object> completedCallback = null) 
    { 
        if (!string.IsNullOrEmpty(assetBundlePath))
        {
            AddAssetBundleAssetLoader(resPath, assetBundlePath, completedCallback);
            return;
        }

#if UNITY_EDITOR
        if (EditorConfigManager.Instance.IsUseAssetBundle)
        {
            if (!string.IsNullOrEmpty(builtInAssetPath))
            {
                AddBuiltInAssetLoader(resPath, builtInAssetPath, completedCallback);
                return;
            }
            else
            {
                DebugManager.Instance.LogError($"Can not load asset {resPath} in Editor assetbundle or builtin");
            }
        }
        AddEditorAssetLoader(resPath, completedCallback);
#else
        if (!string.IsNullOrEmpty(builtInAssetPath))
        {
            AddBuiltInAssetLoader(resPath, builtInAssetPath, completedCallback);
            return;
        }

        DebugManager.Instance.LogError($"Can not load asset {resPath} in assetbundle or builtin");
#endif
    }
}