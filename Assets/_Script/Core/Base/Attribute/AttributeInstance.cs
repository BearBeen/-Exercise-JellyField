using System;
using UnityEngine;

[Serializable]
public class AttributeInstance<T> where T: unmanaged, Enum
{
    public const int PER_BASE = 10000;

    [SerializeField] private T _attribute;
    [SerializeField] private int _baseValue = 0;
    [SerializeField] private int _perValue = 0;
    [SerializeField] private int _addValue = 0;

    private bool _isValueChanged = true;
    private float _finalValue;

    public T attribute => _attribute;
    public float value
    {
        get
        {
            if (_isValueChanged)
            {
                CalFinal();
            }
            return _finalValue;
        }
    }

    public int valueInt
    {
        get
        {
            return Mathf.RoundToInt(value);
        }
    }

    public AttributeInstance(AttributeInstance<T> attributeInstance)
    {
        _attribute = attributeInstance._attribute;
        _baseValue = attributeInstance._baseValue;
        _perValue = attributeInstance._perValue;
        _addValue = attributeInstance._addValue;
        _isValueChanged = true;
    }

    public AttributeInstance(T attribute, int baseValue = 0, int perValue = 0, int addValue = 0)
    {
        _attribute = attribute;
        _baseValue = baseValue;
        _perValue = perValue;
        _addValue = addValue;
        _isValueChanged = true;
    }

    public static string GetValueString(T attribute, float value)
    {
        int key = attribute.ToInt();
        if (key > (int)Attribute.ADD_REGION) return value.ToString("0.00");
        if (key > (int)Attribute.PER_REGION) return (value / PER_BASE).ToString("0.00x");
        return value.ToString("0.00");
    }

    private void CalFinal()
    {
        _isValueChanged = false;
        _finalValue = 1f * _baseValue * (_perValue + PER_BASE) / PER_BASE + _addValue;
    }

    public void Change(T attribute, int change)
    {
        int myKey = _attribute.ToInt();
        int changeKey = attribute.ToInt();
        if (myKey + (int)Attribute.PER_REGION == changeKey)
        {
            _isValueChanged = true;
            _perValue += change;
            return;
        }
        if (myKey + (int)Attribute.ADD_REGION == changeKey)
        {
            _isValueChanged = true;
            _addValue += change;
            return;
        }
        if (myKey == changeKey)
        {
            _isValueChanged = true;
            _baseValue += change;
            return;
        }
    }

    public void Change(AttributeInstance<T> change)
    {
        _isValueChanged = true;
        _perValue += change._perValue;
        _addValue += change._addValue;
        _baseValue += change._addValue;
    }

    public static AttributeInstance<T> operator + (AttributeInstance<T> left, AttributeInstance<T> right)
    {
        return new AttributeInstance<T>(left._attribute,
            left._baseValue + right._baseValue,
            left._perValue + right._perValue,
            left._addValue + right._addValue);
    }
}

//other attribute must copy that structure and clone enum for PER_REGION and ADD_REGION themself
public enum Attribute
{
    BASE_REGION = 0,

    PER_REGION = 10000,

    ADD_REGION = 20000,
}