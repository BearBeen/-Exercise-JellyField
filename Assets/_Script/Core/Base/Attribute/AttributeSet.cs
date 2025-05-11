using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class AttributeSet<T> where T : unmanaged, Enum
{
    private Dictionary<int, AttributeInstance<T>> _attributeDict = new Dictionary<int, AttributeInstance<T>>();

    public AttributeInstance<T> this[T attribute]
    {
        get
        {
            int key = attribute.ToInt();
            if (key > (int)Attribute.ADD_REGION) key -= (int)Attribute.ADD_REGION;
            else if (key > (int)Attribute.PER_REGION) key -= (int)Attribute.PER_REGION;
            if (_attributeDict.TryGetValue(key, out AttributeInstance<T> attributeInstance))
            {
                return attributeInstance;
            }
            return null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetFloat(T attribute)
    {
        return this[attribute]?.value ?? 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetInt(T attribute)
    {
        return this[attribute]?.valueInt ?? 0;
    }

    public void Add(T attribute, int value)
    {
        int key = attribute.ToInt();
        if (key > (int)Attribute.ADD_REGION) key -= (int)Attribute.ADD_REGION;
        else if (key > (int)Attribute.PER_REGION) key -= (int)Attribute.PER_REGION;
        if (_attributeDict.TryGetValue(key, out AttributeInstance<T> myAttributeInstance))
        {
            myAttributeInstance.Change(attribute, value);
        }
    }

    public void Add(AttributeInstance<T> attributeInstance, bool isAcceptNew = true)
    {
        if (!_attributeDict.TryGetValue(attributeInstance.attribute.ToInt(), out AttributeInstance<T> myAttributeInstance))
        {
            if (isAcceptNew)
            {
                myAttributeInstance = new AttributeInstance<T>(attributeInstance);
                _attributeDict[myAttributeInstance.attribute.ToInt()] = myAttributeInstance;
            }
            return;
        }
        myAttributeInstance.Change(attributeInstance);
    }

    public void Add(AttributeSet<T> attributeSet, bool isAcceptNew = true)
    {
        foreach (KeyValuePair<int, AttributeInstance<T>> keyValue in attributeSet._attributeDict)
        {
            Add(keyValue.Value, isAcceptNew);
        }
    }

    public void Add(IEnumerable<AttributeInstance<T>> attributeInstances, bool isAcceptNew = true)
    {
        foreach(AttributeInstance<T> attributeInstance in attributeInstances)
        {
            Add(attributeInstance, isAcceptNew);
        }
    }
}
