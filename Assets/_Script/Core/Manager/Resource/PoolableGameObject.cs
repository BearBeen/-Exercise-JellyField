using System.Collections.Generic;
using UnityEngine;

public class PoolableGameObject : IPoolable
{
    protected string _loadingPath;
    protected GameObject _gameObject;
    protected List<PoolableGameObject> _child = new List<PoolableGameObject>();

    public string loadingPath
    {
        get
        {
            return _loadingPath;
        }
    }

    public GameObject gameObject
    {
        get
        {
            return _gameObject;
        }
    }

    public PoolableGameObject(string loadingPath, GameObject gameObject)
    {
        _loadingPath = loadingPath;
        _gameObject = gameObject;
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
        Object.Destroy(_gameObject);
    }

    public virtual void OnEnabled()
    {
    }

    public virtual void OnDisabled()
    {        
        ResourceManager.Instance.ResetGameObject(this);
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
}
