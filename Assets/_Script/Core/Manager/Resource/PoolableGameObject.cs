using System;
using System.Collections.Generic;
using UnityEngine;

public class PoolableGameObject : IPoolable
{
    protected bool _isReady = false;
    protected bool _isRecycled = true;
    protected string _loadingPath;
    protected GameObject _gameObject;
    protected Action<PoolableGameObject> _onLoadCallback;
    protected List<PoolableGameObject> _child = new List<PoolableGameObject>();

    public string loadingPath => _loadingPath;
    public GameObject gameObject => _gameObject;
    public bool isReady => _isReady;

    //Do i need immediate load mode?
    public PoolableGameObject(string loadingPath)
    {
        _loadingPath = loadingPath;
        _isReady = false;
        _gameObject = null;
        _onLoadCallback = null;
        AssetManager.Instance.LoadAssetAsync(loadingPath, OnAssetLoaded);
    }

    public void AddLoadCallback(Action<PoolableGameObject> onLoadCallback)
    {
        if (_isReady)
        {
            onLoadCallback(this);
        }
        else
        {
            _onLoadCallback += onLoadCallback;
        }
    }

    public void AddPoolableChild(PoolableGameObject poolableGameObject)
    {
        if (!_child.Contains(poolableGameObject))
        {
            _child.Add(poolableGameObject);
        }
    }

    public void RemovePoolableChild(PoolableGameObject poolableGameObject)
    {
        _child.Remove(poolableGameObject);
    }

    public virtual void Destroy()
    {
        UnityEngine.Object.Destroy(_gameObject);
    }

    public virtual void OnGetFomPool()
    {
        _isRecycled = false;
    }

    public virtual void OnReturnToPool()
    {
        ResourceManager.Instance.ResetGameObject(this);
        _isRecycled = true;
        _onLoadCallback = null;
        if (_gameObject)
        {
            for (int i = 0, length = _child.Count; i < length; i++)
            {
                if (_child[i].gameObject.transform.IsChildOf(_gameObject.transform))
                {
                    _child[i].gameObject.transform.parent = null;
                }
            }
            _gameObject.transform.localPosition = Vector3.zero;
            _gameObject.transform.localScale = Vector3.one;
            _gameObject.transform.localRotation = Quaternion.identity;
        }
        _child.Clear();
    }

    public virtual void Recycle()
    {
        ResourceManager.Instance.RecycleGameObject(this);
    }

    protected virtual void OnGameObjectCreated()
    {
        _onLoadCallback?.Invoke(this);
    }

    private void OnAssetLoaded(UnityEngine.Object unityObject)
    {
        if (unityObject is GameObject gameObject)
        {
            _isReady = true;
            _gameObject = UnityEngine.Object.Instantiate(gameObject);
            OnGameObjectCreated();
        }
        else
        {
            DebugManager.Instance.LogError($"Load GameObject failed as path: {_loadingPath}");
        }
    }
}