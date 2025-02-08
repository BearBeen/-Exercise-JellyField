#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Diagnostics;

public partial class AssetManager : MonoSingleton<AssetManager>
{
    private const int MAX_LOADTIME = 20;//ms

    private AssetBundleManifest _manifest;
    private Dictionary<string, AssetPoolInstance> _assetPool;
    private Dictionary<string, IAssetLoader> _loadingAssetDic;
    private Queue<IAssetLoader> _loadingAssetQueue;
    private List<string> _unloadThisFrame = new List<string>();
    private HashSet<string> _noPoolAsset;
    private Stopwatch _stopWatch = new Stopwatch();
    #region INIT

    public override void Init()
    {
        base.Init();
        _noPoolAsset = new HashSet<string>();//idead: there are some objects that you dont want to be pool, but be reload every time it got required. like lua script.
        //should consider a solution for reload the pooled object and all its dependency.
        _assetPool = new Dictionary<string, AssetPoolInstance>();
        _loadingAssetDic = new Dictionary<string, IAssetLoader>();
        _loadingAssetQueue = new Queue<IAssetLoader>();

        _manifest = LoadAssetSync<AssetBundleManifest>(Path.GetFileNameWithoutExtension(AssetConst.AssetBundleRootPath));
    }

    #endregion
    #region UNITY EVENT

    private void LateUpdate()
    {
        _stopWatch.Restart();
        CheckAwaitLoadingAsset();
        CheckUnloadUnuseAsset();
    }

    private void CheckAwaitLoadingAsset()
    {
        if (_loadingAssetQueue.Count <= 0 || _stopWatch.ElapsedMilliseconds > MAX_LOADTIME)
        {
            return;
        }

        IAssetLoader topLoading = _loadingAssetQueue.Peek();
        if (!_loadingAssetDic.ContainsKey(topLoading.resPath))
        {
            //asset got a sync load call after another submit a async load call. ignore it. it's already loaded and cached
            _loadingAssetQueue.Dequeue();
            CheckAwaitLoadingAsset();
        }
        else if (topLoading.TryLoad())
        {
            _loadingAssetDic.Remove(topLoading.resPath);
            _loadingAssetQueue.Dequeue();

            AssetPoolInstance assetPoolInstance = new AssetPoolInstance(topLoading is AssetBundleLoader ? (topLoading as AssetBundleLoader).Bundle : null, topLoading.Asset, topLoading.refCount);
            _assetPool.Add(topLoading.resPath, assetPoolInstance);
            CheckAwaitLoadingAsset();
        }
    }

    private void CheckUnloadUnuseAsset()
    {
        _unloadThisFrame.Clear();
        foreach (string key in _assetPool.Keys)
        {
            if (_assetPool[key].refCount == 0)
            {
                _unloadThisFrame.Add(key);
                _assetPool[key].UnLoad();
            }
            else if (_assetPool[key].refCount < 0)
            {
                DebugManager.Instance.LogError($"{key}: ref count is {_assetPool[key].refCount}. This should not happen!");
                _unloadThisFrame.Add(key);
                _assetPool[key].UnLoad();
            }
        }

        for (int i = 1; i < _unloadThisFrame.Count; i++)
        {
            _assetPool.Remove(_unloadThisFrame[i]);
        }
    }
    #endregion
    public T LoadAssetSync<T>(string resPath, bool noPool = false) where T : UnityEngine.Object
    {
        AssetConst.ResolveResPath(ref resPath, out string assetBundlePath, out string builtInAssetPath);
        //load depemdemcies
        if (!string.IsNullOrEmpty(assetBundlePath) && _manifest != null)
        {
            string[] depPaths = _manifest.GetDirectDependencies(resPath);
            for (int i = 0; i < depPaths.Length; i++)
            {
                LoadAssetSync<T>(depPaths[i]);
            }
        }
        //check loaded asset cache
        if (_assetPool.TryGetValue(resPath, out AssetPoolInstance assetPoolInstance))
        {
            return assetPoolInstance.Get() as T;
        }
        //check loading queue
        else if (_loadingAssetDic.TryGetValue(resPath, out var assetAsyncLoadInstance))
        {
            assetAsyncLoadInstance.LoadImmedietly();
            assetPoolInstance = new AssetPoolInstance(
                assetAsyncLoadInstance is AssetBundleLoader ? (assetAsyncLoadInstance as AssetBundleLoader).Bundle : null, 
                assetAsyncLoadInstance.Asset, assetAsyncLoadInstance.refCount);
            _assetPool.Add(assetAsyncLoadInstance.resPath, assetPoolInstance);
            _loadingAssetDic.Remove(resPath);
            return assetPoolInstance.Get() as T;
        }
        //load asset
        else
        {
            //T asset = null;
            if (!string.IsNullOrEmpty(assetBundlePath))
            {
                AssetBundle bundle = AssetBundle.LoadFromFile(assetBundlePath);
                UnityEngine.Object asset = bundle.LoadAllAssets<UnityEngine.Object>()[0];
                assetPoolInstance = new AssetPoolInstance(bundle, asset, 0);
                _assetPool.Add(resPath, assetPoolInstance);
                return assetPoolInstance.Get() as T;
            }

#if UNITY_EDITOR
            if (EditorConfigManager.Instance.IsUseAssetBundle && string.IsNullOrEmpty(builtInAssetPath))
            {
                DebugManager.Instance.LogError($"Can not load asset {resPath} in Editor assetbundle or builtin");
            }
            return AssetDatabase.LoadAssetAtPath<T>(resPath);
#else
            if (!string.IsNullOrEmpty(builtInAssetPath))
            {
                return Resources.Load<T>(builtInAssetPath);
            }
            DebugManager.Instance.LogError($"Can not load asset {resPath} in Editor assetbundle or builtin");
            return null;
#endif
        }
    }

    public void LoadAssetAsync(string resPath, Action<UnityEngine.Object> completedCallback = null)
    {
        AssetConst.ResolveResPath(ref resPath, out string assetBundlePath, out string builtInAssetPath);
        //check loaded asset cache
        if (_assetPool.TryGetValue(resPath, out AssetPoolInstance assetPoolInstance))
        {
            UnityEngine.Object asset = assetPoolInstance.Get();
            completedCallback?.Invoke(asset);
        }
        //check loading queue
        else if (_loadingAssetDic.TryGetValue(resPath, out var assetAsyncLoadInstance))
        {
            assetAsyncLoadInstance.AddCallback(completedCallback);
        }
        //creat new asyn loading instance
        else
        {
            AddAssetLoader(resPath, assetBundlePath, builtInAssetPath, completedCallback);
        }
    }

    public AsyncOperation LoadSceneAsync(string scenePath, bool isBuiltIn = true)
    {
        //cheating, need to do it properly. this will only load the assetbundle and all ref that need for the scene.
#if UNITY_EDITOR
        if (isBuiltIn)
        {
            return EditorSceneManager.LoadSceneAsync(scenePath);
        }
        else
        {
            return EditorSceneManager.LoadSceneAsyncInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Additive));
        }
#else
            return null;
#endif
    }

    //will handle assetbundle caching here
    //just a cheating for lua bundle, need to do it properly later
    public AssetBundle LoadAssetBundle(string assetBundlePath)
    {
        AssetBundle assetBundle = null;
        if (File.Exists(assetBundlePath))
        {
            assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
            if (assetBundle != null)
            {
                OnAssetBundleLoadComplete(assetBundlePath, assetBundle);
            }
        }
        return assetBundle;
    }

    public void UnloadAset(string resPath)
    {
        AssetConst.ResolveResPath(ref resPath, out string assetBundlePath, out string builtInAssetPath);
        if (_assetPool.TryGetValue(resPath, out AssetPoolInstance assetPoolInstance))
        {
            if (assetPoolInstance.isAssetBundle)
            {
                string[] depPaths = _manifest.GetDirectDependencies(resPath);
                for (int i = 0; i < depPaths.Length; i++)
                {
                    UnloadAset(depPaths[i]);
                }
            }
            assetPoolInstance.Release();
        }
        else
        {
            DebugManager.Instance.LogError($"{resPath} hasn't been loaded. Can't unload");
        }
    }

    //will handle assetbundle caching here
    private void OnAssetBundleLoadComplete(string assetBundlePath, AssetBundle assetBundle)
    {

    }
}
