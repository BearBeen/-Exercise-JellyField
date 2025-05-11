using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "MissileSkillData", menuName = "ScriptableObjects/Skill/MissileSkillData")]
public class MissileSkillData : AbsSkillData<MissileSkillData, MissileSkillInstance>
{
    [SerializeField, AssetPath(typeof(GameObject))] private string _missileTemplatePath;
    [SerializeField] private ParticleSystem _exploseEffect;////TODO: make boom effect. later, in polish state
    [SerializeField] private float _missileMaxSpeed;
    [SerializeField] private float _missileAccelerate;
    [SerializeField] private float _missileMaxTurnSpeed;
    [SerializeField] private float _missileTurnAccelerate;
    [SerializeField] private float _missileSpeedStartTurn;
    [SerializeField] private int _shootCount;

    private float _missileAdd;
    private float _missilePer;

    public override int skillID => (int)SkillID.Missile;
    public string missileTemplatePath => _missileTemplatePath;
    public ParticleSystem exploseEffect => _exploseEffect;
    public float missileMaxSpeed => _missileMaxSpeed;
    public float missileAccelerate => _missileAccelerate;
    public float missileMaxTurnSpeed => _missileMaxTurnSpeed;
    public float missileTurnAccelerate => _missileTurnAccelerate;
    public float missileSpeedStartTurn => _missileSpeedStartTurn;
    public int shootCount => (int)((_shootCount + _missileAdd) * (_missilePer + 1));

    protected override bool InternalIsUpgradable(SkillUpgradeType skillUpgradeType, ISkillUpgrade skillUpgrade)
    {
        switch (skillUpgradeType)
        {
            case SkillUpgradeType.MissileCountUpgrade:
                return IsMissileCountUpgradable(shootCount, skillUpgrade);
            default:
                return false;
        }
    }

    protected override void InternalUpgrade(SkillUpgradeType skillUpgradeType, ISkillUpgrade skillUpgrade)
    {
        switch (skillUpgradeType)
        {
            case SkillUpgradeType.MissileCountUpgrade:
                UpgradeMissileCount(skillUpgrade as IMissileCountUpgrade, ref _missilePer, ref _missileAdd);
                break;
        }
    }
}

public class MissileSkillInstance : AbsSkillInstance<MissileSkillData, MissileSkillInstance>
{
    private List<PoolableGameObject> _missiles = new List<PoolableGameObject>();
    private Vector3[] _endPositions;
    private float[] _speed;
    private float[] _turnSpeed;
    private int _missileArrivedFlag;//limit is 32 missile;
    private int _allMissileArrivedFlag;
    private int _targetJellyIndex;
    private Vector3 _casterPos;

    public override void Init(Jelly caster, int targetJellyIndex)
    {
        //init first model.
        _targetJellyIndex = targetJellyIndex;
        for (int i = 0; i < _skillData.shootCount; i++)
        {
            PoolableGameObject poolableGameObject = ResourceManager.Instance.GetGameObjectAsync<PoolableGameObject>(_skillData.missileTemplatePath);
            poolableGameObject.AddLoadCallback((obj) =>
            {
                Vector3 pos = 0.25f * Random.insideUnitSphere + 0.5f * Vector3.up;
                Vector3 forward = pos + 0.55f * Vector3.up;
                obj.gameObject.transform.SetParent(caster.dockerTrs, false);
                obj.gameObject.transform.SetLocalPositionAndRotation(pos, Quaternion.LookRotation(forward, Vector3.left));
            });
            _missiles.Add(poolableGameObject);
        }
    }

    protected override void InitExcute(Jelly caster)
    {
        GameBoardManager gameBoardMgr = GameBoardManager.Instance;
        _casterPos = caster.transform.position;
        GameBoardManager.PositionToBoardIndex(_casterPos, out int boxX, out int boxY);
        List<Jelly> jellies = new List<Jelly>();
        int shootCount = _skillData.shootCount;
        gameBoardMgr.FindAllJellies(_targetJellyIndex, boxX, boxY, gameBoardMgr.xSize, gameBoardMgr.ySize, false, ref jellies);
        jellies = jellies.GetRandoms(shootCount);
        _endPositions = new Vector3[shootCount];
        _speed = new float[shootCount];
        _turnSpeed = new float[shootCount];
        for (int i = 0; i < jellies.Count; i++)
        {
            _endPositions[i] = jellies[i].transform.position;
        }
        for (int i = jellies.Count; i < shootCount; i++)
        {
            Vector3 pos = Random.insideUnitCircle;
            pos.z = pos.y;
            pos.y = 0;
            _endPositions[i] = 1.25f * Mathf.Max(gameBoardMgr.xSize, gameBoardMgr.ySize) * pos + 20f * Vector3.up;
        }
        _missileArrivedFlag = 0;
        _allMissileArrivedFlag = (1 << shootCount) - 1;
        for (int i = 0; i < _missiles.Count; i++)
        {
            PoolableGameObject missile = _missiles[i];
            if (!missile.isReady || missile.gameObject == null)
            {
                DebugManager.Instance.LogError("Missile prefab is not ready yet when we are about to fire ???");
                continue;
            }
            missile.gameObject.transform.SetParent(null, true);
            _speed[i] = 0;
            _turnSpeed[i] = 0;
        }
    }

    protected override void Excute()
    {
        for (int i = 0; i < _missiles.Count; i++)
        {
            PoolableGameObject missile = _missiles[i];
            if (((1 << i) & _missileArrivedFlag) != 0 || !missile.isReady)
            {
                continue;
            }
            Transform missileTrs = missile.gameObject.transform;
            Vector3 missilePos = missileTrs.position;
            Vector3 endPos = _endPositions[i];
            float deltaTime = Time.deltaTime;
            _speed[i] = Mathf.Min(_skillData.missileMaxSpeed, _speed[i] + deltaTime * _skillData.missileAccelerate);
            if (_speed[i] >= _skillData.missileSpeedStartTurn)
            {
                _turnSpeed[i] = Mathf.Min(_skillData.missileMaxTurnSpeed, _turnSpeed[i] + deltaTime * _skillData.missileTurnAccelerate);
            }
            Vector3 me2End = endPos - missilePos;
            missileTrs.localRotation = Quaternion.RotateTowards(
                missileTrs.localRotation,
                Quaternion.LookRotation(me2End, Vector3.up),
                _turnSpeed[i]);
            if (me2End.magnitude < 2.5f * deltaTime * _speed[i])
            {
                GameBoardManager.PositionToBoardIndex(endPos, out int boxX, out int boxY);
                GameBoardManager.Instance.KillJelly(_targetJellyIndex, boxX, boxY, 0, 0);
                _missileArrivedFlag |= 1 << i;
                _missiles[i].Recycle();
                _missiles[i] = null;
            }
            else
            {
                missile.gameObject.transform.position += deltaTime * _speed[i] * missileTrs.forward;
            }
        }
    }

    protected override void ClearExcute()
    {
        for (int i = 0; i < _missiles.Count; i++)
        {
            if (_missiles[i] != null)
            {
                _missiles[i].Recycle();
            }
        }
        _missiles.Clear();
        _endPositions = null;
    }

    protected override bool IsExcuteCompleted()
    {
        //TODO: delay killing, wait for the effect
        return (_missileArrivedFlag & _allMissileArrivedFlag) == _allMissileArrivedFlag;
    }
}
