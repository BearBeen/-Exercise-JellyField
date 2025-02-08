using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;


public class ResourceManager : MonoSingleton<ResourceManager>
{
    private Transform _poolRoot;
    private Dictionary<string, Pool<PoolableGameObject>> _gameobjectPools = new Dictionary<string, Pool<PoolableGameObject>>();

    /// <summary>
    /// To instantly load an asset. Cached in AssetManager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public T GetAssetSync<T>(string resPath) where T : UnityEngine.Object
    {
        return AssetManager.Instance.LoadAssetSync<T>(resPath);
    }

    /// <summary>
    /// To asynchronously load an asset. Cached in AssetManager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resPath"></param>
    /// <param name="completedCallback"></param>
    public void GetAssetAsync<T>(string resPath, Action<T> completedCallback = null) where T : UnityEngine.Object
    {
        AssetManager.Instance.LoadAssetAsync(resPath, (unityObject) => completedCallback(unityObject as T));
    }

    /// <summary>
    /// To asynchronously load a GameObject. Cached in AssetManager. Pooled in ResourceManager
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="resPath"></param>
    /// <param name="completedCallback"></param>
    public void GetGameObjectAsync<T>(string resPath, Action<T> completedCallback = null) where T : PoolableGameObject
    {
        if (_gameobjectPools.TryGetValue(resPath, out Pool<PoolableGameObject> pool))
        {
            completedCallback(pool.Get() as T);
        }
        else
        {
            //problem: 1 gameobject may need many frames to completely load. many async load request can be submit in loading time.
            //resolution: re-check pool existence on load callback.
            AssetManager.Instance.LoadAssetAsync(resPath, (unityObject) =>
            {
                if (_gameobjectPools.TryGetValue(resPath, out Pool<PoolableGameObject> justLoadedPool))
                {
                    completedCallback(justLoadedPool.Get() as T);
                }
                else if (unityObject != null)
                {
                    pool = new Pool<PoolableGameObject>(() =>
                    {
                        Type type = typeof(T);
                        if (type == typeof(PoolableEffect))
                        {
                            return new PoolableEffect(resPath, Object.Instantiate(unityObject) as GameObject);
                        }
                        else if (type == typeof(PoolableGameObject))
                        {
                            return new PoolableGameObject(resPath, Object.Instantiate(unityObject) as GameObject);
                        }
                        else
                        {
                            DebugManager.Instance.LogError($"Un supoprt poolable type {type}. Don't you forget to adding it to GetGameObjectAsync function");
                            return null;
                        }
                    },
                        64);
                    _gameobjectPools.Add(resPath, pool);
                    completedCallback(pool.Get() as T);
                }
                else
                {
                    DebugManager.Instance.LogError($"Load GameObject failed as path: {resPath}");
                    completedCallback(null);
                }
            });
        }
    }

    public void ResetGameObject(PoolableGameObject poolableGameObject)
    {
        poolableGameObject.gameObject.transform.parent = _poolRoot;
    }

    public void RecycleGameObject(PoolableGameObject poolableGameObject)
    {
        if (_gameobjectPools.TryGetValue(poolableGameObject.loadingPath, out Pool<PoolableGameObject> pool))
        {
            pool.Return(poolableGameObject);
        }
    }

    /// <summary>
    /// To asynchronously load a Scene. 
    /// </summary>
    /// <param name="resPath"></param>
    /// <returns></returns>
    public AsyncOperation LoadSceneAsync(string resPath)
    {
        //need to re-construct those mechanic
        return AssetManager.Instance.LoadSceneAsync(resPath);
    }

    #region INIT
    public override void Init()
    {
        base.Init();
        _poolRoot = new GameObject().transform;
        _poolRoot.gameObject.name = "POOL_ROOT";
        _poolRoot.parent = transform;
        _poolRoot.localPosition = new Vector3(10000, 10000, 10000);
        _poolRoot.localScale = Vector3.one;
        _poolRoot.localRotation = Quaternion.identity;
        _poolRoot.gameObject.SetActive(false);

        _gameobjectPools = new Dictionary<string, Pool<PoolableGameObject>>();
    }
    #endregion


    #region UNITY EVENT

    private void Update()
    {
        string loadingPath = string.Empty;
        foreach (Pool<PoolableGameObject> pool in _gameobjectPools.Values)
        {
            if (pool.count > 0)
            {
                if (pool.CheckClear())
                {
                    if (pool.inUse == 0 && pool.count == 0)
                    {
                        loadingPath = pool[0].loadingPath;
                        AssetManager.Instance.UnloadAset(loadingPath);
                    }
                    break;
                }
            }
        }

        if (!string.IsNullOrEmpty(loadingPath))
        {
            _gameobjectPools.Remove(loadingPath);
        }
    }
    #endregion
}
