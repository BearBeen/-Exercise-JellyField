using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoSingleton<InputManager>
{
    private GameActionMap _actionMap;
    private Vector3 _pointerPos;
    private bool _isOnUI;
    private Camera _mainCamera;
    private Dictionary<Collider, IDraggable> _draggables;
    private IDraggable _dragging;

    //public Action<Vector3> onSelectPiece = null;

    public override void Init()
    {
        base.Init();

        _actionMap = new GameActionMap();
        _draggables = new Dictionary<Collider, IDraggable>();
        _dragging = null;

        _mainCamera = Camera.main;
        InputActionMap ui = _actionMap.UI;
        ui.Enable();

        ui.FindAction("Point").performed += OnPointer;
        ui.FindAction("Click").performed += OnClick;
    }

    public void SubscribeDraggable(IDraggable draggable)
    {
        if (draggable.collider == null)
        {
            DebugManager.Instance.LogError("Draggable's collider is not assigned");
        }
        else if (_draggables.ContainsKey(draggable.collider))
        {
            _draggables[draggable.collider] = draggable;
            DebugManager.Instance.LogError("Multi draggables have the same collider or are getting duplicated subscriptions");
        }
        else
        {
            _draggables.Add(draggable.collider, draggable);
        }
    }

    public void UnsubscribeDraggable(IDraggable draggable)
    {
        if (draggable.collider == null)
        {
            DebugManager.Instance.LogError("Draggable's collider is not assigned");
        }
        else if (!_draggables.ContainsKey(draggable.collider))
        {
            DebugManager.Instance.LogError("Draggable is not subscribed");
        }
        else
        {
            _draggables.Remove(draggable.collider);
            if (draggable == _dragging)
            {
                _dragging.OnDragEnd();
                _dragging = null;
            }
        }
    }

    private void Update()
    {
        _isOnUI = EventSystem.current.IsPointerOverGameObject();
    }

    private void OnPointer(InputAction.CallbackContext callback)
    {
        _pointerPos = callback.ReadValue<Vector2>();
        _dragging?.OnDrag(_pointerPos);
    }

    private void OnClick(InputAction.CallbackContext callback)
    {
        if (callback.ReadValueAsButton())
        {
            //skip if it's UI
            if (Physics.Raycast(_mainCamera.ScreenPointToRay(_pointerPos), out RaycastHit hit, Mathf.Infinity, AssetConst.LayerMask_Draggable)
                && _draggables.TryGetValue(hit.collider, out IDraggable draggable)
                && !_isOnUI)
            {
                _dragging?.OnDragEnd();//not support multiple drag. so i cancel old drag
                _dragging = draggable;
            }
        }
        else
        {
            _dragging?.OnDragEnd();
            _dragging = null;
        }
    }
}
