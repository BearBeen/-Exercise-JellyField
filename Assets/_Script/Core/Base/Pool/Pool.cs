using System;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T> where T : class, IPoolable
{
    private int _designCapacity;
    private List<T> _pool;
    private Func<T> _instanceCreator;
    private int _inUse = 0;
    private float _lastUseTime = 0;
    private float _timeFromLastUse = 0;
    private float _softClearDelay = 0;
    private float _hardClearDelay = 0;

    public T this[int index]
    {
        get
        {
            return index < 0 || index >= _pool.Count ? null : _pool[index];
        }
    }

    public int count
    {
        get
        {
            return _pool.Count;
        }
    }

    public int inUse
    {
        get
        {
            return _inUse;
        }
    }

    //public bool isCanClear
    //{
    //    get
    //    {
    //        return _pool.Count > _designCapacity;
    //    }
    //}

    public Pool(Func<T> instanceCreator, int designCapacity = 16, float softClearDelay = 2, float hardClearDelay = 10)
    {
        _designCapacity = designCapacity;
        _softClearDelay = softClearDelay;
        _hardClearDelay = hardClearDelay;
        _pool = new List<T>(_designCapacity);
        _instanceCreator = instanceCreator;
        _lastUseTime = Time.time;
    }

    public T Get()
    {
        T result;
        if (_pool.Count > 0)
        {
            result = _pool[_pool.Count - 1];
            _pool.RemoveAt(_pool.Count - 1);
        }
        else
        {
            result = _instanceCreator();
        }
        result.OnEnabled();
        _inUse++;
        if (_pool.Count < _designCapacity) //if we don't exhauting the pool, there is free space to release
        {
            _lastUseTime = Time.time;
        }
        return result;
    }

    public void Return(T instance)
    {
        //if (_pool.Count > (_designCapacity * 4))
        //{
        //    instance.Destroy();
        //}
        //else
        {
            instance.OnDisabled();
            _pool.Add(instance);
        }
        _inUse = _inUse <= 0 ? 0 : _inUse - 1;
        if (_pool.Count < _designCapacity) //if we don't exhauting the pool, there is free space to release
        {
            _lastUseTime = Time.time;
        }
    }

    public bool CheckClear()
    {
        if (_pool.Count > 0)
        {
            _timeFromLastUse = Time.time - _lastUseTime;
            if (_timeFromLastUse > _hardClearDelay)
            {
                Debug.LogError($"{_timeFromLastUse} {_hardClearDelay}");
                for (int i = 0; i < _pool.Count; i++)
                {
                    _pool[i].Destroy();
                }
                _pool.Clear();
                return true;
            }
            else if (_timeFromLastUse > _softClearDelay && _pool.Count > _designCapacity)
            {
                DebugManager.Instance.LogWarning($"pool size: {_pool.Count} exceed designed size {_designCapacity}");
                for (int i = _designCapacity; i < _pool.Count; i++)
                {
                    _pool[i].Destroy();
                }
                _pool.RemoveRange(_designCapacity, _pool.Count - _designCapacity);
                return true;
            }
        }
        return false;
    }

    public void ForceClear()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            _pool[i].Destroy();
        }
        _pool.Clear();
    }
}
