using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(
        SerializedProperty property)
    {
        var container = new VisualElement();
        var field = new PropertyField(property);

        field.SetEnabled(false);
        container.Add(field);

        return container;
    }
}