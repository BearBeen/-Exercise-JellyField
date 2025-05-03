using DG.Tweening;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AbsPanel : MonoBehaviour
{
    [SerializeField] protected Canvas _canvas;
    [SerializeField] protected CanvasGroup _canvasGroup;
    [SerializeField] protected Image _blackCurtant;

    protected bool _isShowed = false;
    protected bool _isShowing = false;
    protected bool _isHiding = false;
    protected Action _showCb;
    protected Action _hideCb;

    protected float animTime => 0.5f;
    protected Ease ease => Ease.Linear;
    protected bool isCoverUI { get; private set; } = true;
    protected Vector3 startPos { get; private set; } = Vector3.zero;
    protected Vector3 endPos { get; private set; } = Vector3.zero;
    protected Color curtantHideColor { get; private set; } = new Color(0, 0, 0, 0);
    protected Color curtantShowColor { get; private set; } = new Color(0, 0, 0, 0.7f);
    public bool isShowed => _isShowed;

    private void Awake()
    {
        _isShowed = _isShowing = _isHiding = false;
        transform.localScale = Vector3.zero;
        transform.localPosition = Vector3.zero;
        _canvas.enabled = false;
        _canvasGroup.interactable = false;
        _blackCurtant.color = new Color(0, 0, 0, 0);
        Init();
    }

    private void OnDestroy()
    {
        Clear();
    }

    protected virtual void Init()
    {

    }

    protected virtual void Clear()
    {

    }

    public void Show(Action showCb = null)
    {
        if (_isHiding)
        {
            return;
        }
        if (_isShowing)
        {
            _showCb += showCb;
            return;
        }
        if (_isShowed)
        {
            showCb?.Invoke();
            return;
        }
        if (isCoverUI)
        {
            PanelManager.Instance.CheckShowStackOnShow(this);
        }
        _isShowing = true;
        _showCb += showCb;
        _canvas.enabled = true;
        transform.localPosition = startPos;
        ShowAnim().AppendCallback(OnShowAnimDone).SetEase(ease).Play();
    }

    public void Hide(Action hideCb = null)
    {
        if (_isShowing)
        {
            return;
        }
        if (_isHiding)
        {
            _hideCb += hideCb;
            return;
        }
        if (!_isShowed)
        {
            hideCb?.Invoke();
            return;
        }
        if (isCoverUI)
        {
            PanelManager.Instance.CheckShowStackOnHide(this);
        }
        _isHiding = true;
        _isShowed = false;
        _hideCb += hideCb;
        _canvasGroup.interactable = false;
        HideAnim().AppendCallback(OnHideAnimDone).SetEase(ease).Play();
    }

    protected virtual Sequence ShowAnim()
    {
        return DOTween.Sequence().Pause()
            .Append(transform.DOScale(Vector3.one, animTime))
            .Join(transform.DOLocalMove(endPos, animTime))
            .Join(_blackCurtant.DOColor(curtantShowColor, animTime));
    }

    protected virtual void OnShowAnimDone()
    {
        _canvasGroup.interactable = true;
        _isShowing = false;
        _isShowed = true;
        _showCb?.Invoke();
        _showCb = null;        
    }

    protected virtual Sequence HideAnim()
    {
        return DOTween.Sequence().Pause()
            .Append(transform.DOScale(Vector3.zero, animTime))
            .Join(transform.DOLocalMove(startPos, animTime))
            .Join(_blackCurtant.DOColor(curtantHideColor, animTime));
    }

    protected virtual void OnHideAnimDone()
    {
        _canvas.enabled = false;
        _isHiding = false;
        _hideCb?.Invoke();
        _hideCb = null;        
    }
}
