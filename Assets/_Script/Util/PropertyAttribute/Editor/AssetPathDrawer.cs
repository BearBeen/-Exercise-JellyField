using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NUnit.Framework.Constraints;

[CustomPropertyDrawer(typeof(AssetPathAttribute))]
public class AssetPathDrawer : PropertyDrawer
{
    private static readonly string TOOLTIP = "A string path to the actual asset. The editor script only make it easier to set up " + 
    "by using the actual asset. But the path will not get update when the asset is changed it location.";
    private bool _isLoaded = false;
    private UnityEngine.Object _asset = null;
    private UnityEngine.Object _lastFrameAsset = null;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        AssetPathAttribute assetPathAttribute = attribute as AssetPathAttribute;

        // Now draw the property as a Slider or an IntSlider based on whether it's a float or integer.
        if (property.propertyType != SerializedPropertyType.String)
            EditorGUI.LabelField(position, label.text, "Use AssetPath with string only");
        else
        {
            if (!_isLoaded)
            {
                _isLoaded = true;
                _lastFrameAsset = _asset = AssetDatabase.LoadAssetAtPath(property.stringValue, assetPathAttribute.Type);
            }
            label.tooltip = TOOLTIP;
            Rect customPosition = new Rect(position);
            customPosition.height = position.height / 2f;
            GUI.enabled = false;
            EditorGUI.TextField(customPosition, label, property.stringValue);
            GUI.enabled = true;
            customPosition.y += customPosition.height;
            _asset = EditorGUI.ObjectField(customPosition, _asset, assetPathAttribute.Type, false);
            if (_asset != null && _asset != _lastFrameAsset)
            {
                property.stringValue = AssetDatabase.GetAssetPath(_asset);
            }
            _lastFrameAsset = _asset;
        }
    }
}
