using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolableAnimatedObject : PoolableGameObject
{
    private Animator _animator;
    public Animator animator => _animator;

    public PoolableAnimatedObject(string loadingPath) : base(loadingPath)
    {
    }

    protected override void OnGameObjectCreated()
    {
        _animator = _gameObject.GetComponentInChildren<Animator>();
        base.OnGameObjectCreated();
    }

    public override void OnReturnToPool()
    {
        //reset the animator
        _animator.Rebind();
        _animator.Update(0f);
        _animator.enabled = false;
        base.OnReturnToPool();
    }

    public override void OnGetFomPool()
    {
        if (_animator)
        {
            _animator.enabled = true;
        }
        base.OnGetFomPool();
    }
}
