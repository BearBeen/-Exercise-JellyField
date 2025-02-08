using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoxDragger : MonoBehaviour, IDraggable
{
    public static readonly Vector3 DRAG_OFFSET = new Vector3(0, 0.7f, 1.5f);
    public static readonly float OFFSET_SPEED = 7f;

    [SerializeField] private Collider _collider;
    [SerializeField] private JellyBox _jellyBox;

    private Camera _mainCamera;
    private bool _dragging = false;
    private int _index;

    Collider IDraggable.collider => _collider;
    public JellyBox jellyBox => _jellyBox;


    public void InitBoxDragger(int index)
    {
        _index = index;
    }

    public void Refill(JellyBox box)
    {
        _jellyBox = box;
        _jellyBox = box;
        _jellyBox.transform.parent = transform;
        _jellyBox.transform.localPosition = Vector3.zero;
        transform.localPosition = Vector3.zero;
    }

    public void Clear()
    {
        _jellyBox?.Recycle();
    }

    public void OnDrag(Vector3 screenPos)
    {
        Vector3 cameraWorldPos = _mainCamera.transform.position;
        screenPos.z = cameraWorldPos.y;
        Vector3 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
        worldPos = cameraWorldPos + (worldPos - cameraWorldPos) * (cameraWorldPos.y - 1) / (cameraWorldPos.y - worldPos.y);
        transform.position = worldPos;
        _dragging = true;
        if (_jellyBox != null)
        {
            GameBoardManager.Instance.ShowDropPoint(_jellyBox.transform.position);
        }
    }

    public void OnDragEnd()
    {
        _dragging = false;
        if (_jellyBox != null)
        {
            GameBoardManager.Instance.TryPutBox(_jellyBox.transform.position, _index);
        }
        transform.localPosition = Vector3.zero;
    }

    private void Update()
    {
        if (_jellyBox == null)
        {
            return;
        }

        Vector3 diffVector;
        float moveDis = OFFSET_SPEED * Time.deltaTime;

        if (_dragging)
        {
            diffVector = DRAG_OFFSET - _jellyBox.transform.localPosition;
        }
        else
        {
            diffVector = Vector3.zero - _jellyBox.transform.localPosition;
        }

        if (diffVector.magnitude >= moveDis)
        {
            _jellyBox.transform.localPosition += moveDis * diffVector.normalized;
        }
    }

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        InputManager.Instance.SubscribeDraggable(this);
    }

    private void OnDisable()
    {
        InputManager.Instance.UnsubscribeDraggable(this);
    }
}
