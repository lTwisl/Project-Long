using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using EditorAttributes.Editor.Utility;

namespace EditorAttributes.Editor
{
	[CustomPropertyDrawer(typeof(HideFieldAttribute))]
    public class HideFieldDrawer : PropertyDrawerBase
    {
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			var hideAttribute = attribute as HideFieldAttribute;
			var conditionalProperty = ReflectionUtility.GetValidMemberInfo(hideAttribute.ConditionName, property);

			var root = new VisualElement();
			var errorBox = new HelpBox();

			var propertyField = new PropertyField(property);

			root.Add(propertyField);

            if (string.IsNullOrEmpty(hideAttribute.ConditionName))
            {
                RemoveElement(root, propertyField);
            }
            else
            {
                UpdateVisualElement(root, () =>
				{
					if (!GetConditionValue(conditionalProperty, hideAttribute, property, errorBox))
					{
						AddElement(root, propertyField);
					}
					else
					{
						RemoveElement(root, propertyField);
					}

					DisplayErrorBox(root, errorBox);
				});
			}
			return root;
		}
	}
}
