using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "ColorData", menuName = "ScriptableObjects/ColorData", order = 1)] [System.Serializable] 
public class ColorData : ScriptableObject{

    [SerializeField] Color variant1;
    [SerializeField] Color variant2;
    [SerializeField] Color variant3;
    [SerializeField] Color variant4;

    public Color V1 => variant1;
    public Color V2 => variant2;
    public Color V3 => variant3;
    public Color V4 => variant4;

    public Color Get(Variant variant) {
        switch (variant) {
            case Variant.First: return variant1; 
            case Variant.Second: return variant2;
            case Variant.Third: return variant3;
            case Variant.Fourth: return variant4;
        }
        return variant1;
    }

    public static Color GetWithAlpha(float alpha, Color color) {
        color.a = alpha;
        return color;
    }

    public enum Variant {
        First, Second, Third, Fourth
    }

}

[System.Serializable]
public class ColorDataReference{
    [SerializeField] ColorData colorData;
    [SerializeField] ColorData.Variant variant;

    public Color Get() => colorData.Get(variant);
}

#if UNITY_EDITOR

[UnityEditor.CustomPropertyDrawer(typeof(ColorDataReference))]
public class IngredientDrawerUIE : UnityEditor.PropertyDrawer {

    public override VisualElement CreatePropertyGUI(UnityEditor.SerializedProperty property) {
        // Create property container element.
        var container = new VisualElement();

        // Create property 
        var colorData = new UnityEditor.UIElements.PropertyField(property.FindPropertyRelative("colorData"));
        var variant = new UnityEditor.UIElements.PropertyField(property.FindPropertyRelative("variant"));

        // Add fields to the container.

        container.Add(colorData);
        container.Add(variant);

        return container;
    }
}

#endif