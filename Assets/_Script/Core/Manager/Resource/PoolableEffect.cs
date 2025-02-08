using UnityEngine;

public class PoolableEffect : PoolableGameObject
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

    public PoolableEffect(string loadingPath, GameObject gameObject) : base(loadingPath, gameObject)
    {
        _rootParticle = _gameObject.GetComponent<ParticleSystem>();
        if (_rootParticle == null)
        {
            _rootParticle = _gameObject.AddComponent<ParticleSystem>();
            _gameObject.GetComponent<ParticleSystemRenderer>().enabled = false;
        }
        _mainModule = _rootParticle.main;
        StopEffect();
    }

    public override void OnDisabled()
    {
        StopEffect();
        base.OnDisabled();
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
