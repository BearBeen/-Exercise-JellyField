using UnityEngine;

public static partial class AssetConst
{
    public static readonly int LayerMask_Draggable = LayerMask.GetMask("Draggable");

    public static readonly string KEY_GAMEBOARDMANAGER_LVLINDEX = "KEY_GAMEBOARDMANAGER_LVLINDEX";


    //global shader property name
    public static readonly int SHADERPROP_UNITY_DELTATIME = Shader.PropertyToID("unity_DeltaTime");


    //custoom shader property name
    public static readonly int SHADERPROP_JELLY_MOVE = Shader.PropertyToID("_JellyMove");

    public static readonly int SHADERPROP_MESHED_SPRING = Shader.PropertyToID("_MeshedSpring");
    public static readonly int SHADERPROP_PINCH = Shader.PropertyToID("_Pinch");
    public static readonly int SHADERPROP_STIFFNESS = Shader.PropertyToID("_Stiffness");
    public static readonly int SHADERPROP_LOSS = Shader.PropertyToID("_Loss");
    public static readonly int SHADERPROP_X_SIZE = Shader.PropertyToID("_XSize");
    public static readonly int SHADERPROP_Y_SIZE = Shader.PropertyToID("_YSize");
    public static readonly int SHADERPROP_Z_SIZE = Shader.PropertyToID("_ZSize");

}
