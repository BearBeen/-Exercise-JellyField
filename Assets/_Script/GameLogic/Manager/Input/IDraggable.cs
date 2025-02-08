using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDraggable
{
    Collider collider { get; }
    void OnDrag(Vector3 screenPos);
    void OnDragEnd();
}
