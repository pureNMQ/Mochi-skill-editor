using MochiFramework.Skill;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MochiFramewrok.Skill.Editor
{
    [CustomPropertyDrawer(typeof(SlotEntry))]
    public class SlotEntryInspector : PropertyDrawer
    {
        private bool isFoldout = false;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            SerializedProperty slotDefinitionProp = property.FindPropertyRelative("_slotDefinition");
            SerializedProperty slotIndexProp = property.FindPropertyRelative("_slotIndex");

            float widthUnit = position.width / 4;
            
            Rect LabelRect = new Rect(position.x, position.y, widthUnit * 2, EditorGUIUtility.singleLineHeight);
            Rect slotDefinitionRect = new Rect(position.x + widthUnit * 2, position.y, widthUnit, EditorGUIUtility.singleLineHeight);
            Rect slotIndexRect = new Rect(position.x  + widthUnit * 3, position.y, widthUnit, EditorGUIUtility.singleLineHeight);
            
            EditorGUI.LabelField(LabelRect,label);
            
            EditorGUI.PropertyField(slotDefinitionRect, slotDefinitionProp, GUIContent.none);

            if (slotDefinitionProp.objectReferenceValue != null)
            {
                var slotDefinition = (SlotDefinition)slotDefinitionProp.objectReferenceValue;
                string[] slots = slotDefinition.SlotNames;
                int selectedIndex = EditorGUI.Popup(slotIndexRect, slotIndexProp.intValue, slots);
                slotIndexProp.intValue = selectedIndex;
            }
            else
            {
                EditorGUI.LabelField(slotIndexRect, "请先设置SlotDefinition");
            }

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}