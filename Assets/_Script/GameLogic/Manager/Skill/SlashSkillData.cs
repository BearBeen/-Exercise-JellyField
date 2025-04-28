using UnityEngine;

[CreateAssetMenu(fileName = "SlashSkillData", menuName = "ScriptableObjects/Skill/SlashSkillData")]
public class SlashSkillData : AbsSkillData<SlashSkillData, SlashSkillInstance>
{
    [SerializeField, AssetPath(typeof(GameObject))] private string _swordPath;
    [SerializeField] private ParticleSystem _slashEffect;//TODO: make zip-cleng effect. later in polish state
    [SerializeField] private int _slashLength;
    [SerializeField] private float _slashSpeed;

    public string swordPath => _swordPath;
    public int slashLength => _slashLength;
    public float slashSpeed => _slashSpeed;
}

public class SlashSkillInstance : AbsSkillInstance<SlashSkillData, SlashSkillInstance>
{
    private static readonly Vector3[] SLASH_DIRS = new Vector3[4]
    {
        Vector3.forward,
        Vector3.back,
        Vector3.right,
        Vector3.left
    };

    private PoolableAnimatedObject _sword;
    private int _targetJellyIndex;
    private Vector3 _slashDir = Vector3.right;
    private Vector3 _slashingPos;
    private int _slashingBoxX;
    private int _slashingBoxY;
    private int _slashedBoxCount;

    public override void Init(Jelly caster, int targetJellyIndex)
    {
        _targetJellyIndex = targetJellyIndex;
        _slashDir = SLASH_DIRS.GetRandom();
        _sword = ResourceManager.Instance.GetGameObjectAsync<PoolableAnimatedObject>(_skillData.swordPath);
        _sword.AddLoadCallback((obj) =>
        {
            obj.gameObject.transform.SetParent(caster.dockerTrs, false);
            obj.gameObject.transform.SetLocalPositionAndRotation(0.5f * Vector3.up, Quaternion.LookRotation(_slashDir, Vector3.up));
        });
    }

    protected override void ClearExcute()
    {
        _sword.Recycle();
    }

    protected override void Excute()
    {
        _slashingPos += Time.deltaTime * _skillData.slashSpeed * _slashDir;
        //TODO: this is for test only
        _sword.gameObject.transform.position = _slashingPos;
        GameBoardManager.PositionToBoardIndex(_slashingPos, out int newSlashBoxX, out int newSlashBoxY);
        if (newSlashBoxX != _slashingBoxX || _slashingBoxY != newSlashBoxY)
        {
            _slashingBoxX = newSlashBoxX;
            _slashingBoxY = newSlashBoxY;
            _slashedBoxCount++;
            GameBoardManager.Instance.KillJelly(_targetJellyIndex, _slashingBoxX, _slashingBoxY, 0, 0);
        }
    }

    protected override void InitExcute(Jelly caster)
    {
        //TODO: this is for test only
        _sword.gameObject.transform.SetParent(null, false);
        _slashingPos = caster.transform.position;
        GameBoardManager.PositionToBoardIndex(_slashingPos, out _slashingBoxX, out _slashingBoxY);
        _slashedBoxCount = 0;
    }

    protected override bool IsExcuteCompleted()
    {
        return _slashedBoxCount >= _skillData.slashLength;
    }
}
