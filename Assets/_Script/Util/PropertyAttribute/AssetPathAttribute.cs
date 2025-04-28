using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetPathAttribute : PropertyAttribute
{
    public Type Type;

    public AssetPathAttribute(Type type)
    {
        Type = type;
    }

}
