using UnityEngine;

public class PoolableParticle : PoolableGameObject
{
    private ParticleSystem _rootParticle;
    private ParticleSystem.MainModule _mainModule;

    public float speed
    {
        get
        {
            return _mainModule.simulationSpeed;
        }
        set
        {
            _mainModule.simulationSpeed = value;
        }
    }

    public PoolableParticle(string loadingPath) : base(loadingPath)
    {
    }

    protected override void OnGameObjectCreated()
    {
        _rootParticle = _gameObject.GetComponent<ParticleSystem>();
        if (_rootParticle == null)
        {
            _rootParticle = _gameObject.AddComponent<ParticleSystem>();
            _gameObject.GetComponent<ParticleSystemRenderer>().enabled = false;
        }
        _mainModule = _rootParticle.main;
        StopEffect();
        base.OnGameObjectCreated();
    }

    public override void OnReturnToPool()
    {
        StopEffect();
        base.OnReturnToPool();
    }

    public void StopEffect()
    {
        _rootParticle.Stop();
    }

    public void StartEffect()
    {
        _rootParticle.Play();
    }

    public void PauseEffect()
    {
        _rootParticle.Pause();
    }
}
