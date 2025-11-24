using System;
using System.Collections;
using System.Collections.Generic;
using MochiFramework.Skill;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MochiFramewrok.Skill.Editor
{
    [CustomEditor(typeof(SlotMapper))]
    public class SlotMapperEditor : UnityEditor.Editor
    {
        private SlotMapper _slotMapper;
        private VisualElement root;
        private VisualElement slotMapContainer;

        public override VisualElement CreateInspectorGUI()
        {
            _slotMapper = serializedObject.targetObject as SlotMapper;
            if (_slotMapper == null) return null;
            root = new VisualElement();

            Redraw();

            return root;
        }

        public void Redraw()
        {
            root.Clear();

            slotMapContainer = new VisualElement();
            for (int i = 0; i < _slotMapper.SlotMaps.Count; i++)
            {
                VisualElement slotMapElement = DrawSlotMap(_slotMapper.SlotMaps[i]);
                slotMapContainer.Add(slotMapElement);
            }

            if (_slotMapper.SlotMaps.Count <= 0)
            {
                Label label = new Label("No Slot Map");
                slotMapContainer.Add(label);
            }

            //添加SlotMap功能
            VisualElement addSlotMapContainer = new VisualElement();
            addSlotMapContainer.style.flexDirection = FlexDirection.Row;
            addSlotMapContainer.style.marginTop = 20;

            ObjectField addSlotMapField = new ObjectField();
            addSlotMapField.objectType = typeof(SlotDefinition);
            addSlotMapField.style.flexShrink = 1;
            addSlotMapField.style.flexGrow = 1;
            Button addSlotMapButton = new Button(() =>
            {
                if (addSlotMapField.value == null) return;
                _slotMapper.AddSlotMap(addSlotMapField.value as SlotDefinition);
                EditorUtility.SetDirty(_slotMapper);
                Redraw();
            });
            addSlotMapButton.text = "Add Slot Map";
            addSlotMapButton.style.flexShrink = 1;

            addSlotMapContainer.Add(addSlotMapField);
            addSlotMapContainer.Add(addSlotMapButton);

            root.Add(slotMapContainer);
            root.Add(addSlotMapContainer);
            root.MarkDirtyRepaint();
        }

        public VisualElement DrawSlotMap(SlotMap map)
        {
            Foldout foldout = new Foldout();
            foldout.text = map.SlotDefinition.name;
            foldout.value = true;
            for (int i = 0; i < map.SlotTransforms.Count; i++)
            {
                ObjectField objectField = new ObjectField();
                objectField.objectType = typeof(Transform);
                objectField.label = map.SlotDefinition.SlotNames[i];
                objectField.SetValueWithoutNotify(map.SlotTransforms[i]);
                int index = i;
                objectField.RegisterValueChangedCallback((evt) =>
                {
                    map.SlotTransforms[index] = (Transform)evt.newValue;
                });
                foldout.Add(objectField);
            }

            //删除按钮
            Button removeSlotMapButton = new Button(() =>
            {
                _slotMapper.SlotMaps.Remove(map);
                EditorUtility.SetDirty(_slotMapper);
                Redraw();
            });
            removeSlotMapButton.text = "Remove";
            removeSlotMapButton.style.paddingLeft = 5;

            //Foldout的头部parent元素
            VisualElement foldoutHead = foldout.Q<Label>().parent;
            //Foldout内部添加一个元素，修改布局从右向左
            VisualElement foldoutHeadRight = new VisualElement();
            foldoutHeadRight.style.flexDirection = FlexDirection.RowReverse;
            foldoutHeadRight.style.flexGrow = 1;
            foldoutHeadRight.Add(removeSlotMapButton);
            foldoutHead.Add(foldoutHeadRight);

            return foldout;
        }
    }
}
