using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace MochiFramework.Skill.Editor
{
    [CustomPropertyDrawer(typeof(SkillClip))]
    public class ClipInspector : UnityEditor.PropertyDrawer
    {
        private IntegerField startFrameField;
        private IntegerField durationField;
        
        protected VisualElement root;
        protected SkillEditor skillEditor;
        protected SkillClip SkillClip;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            skillEditor = SkillEditor.GetWindow<SkillEditor>();
            SkillClip = property.boxedValue as SkillClip;
            root = new VisualElement();
            DrawInspector();
            return root;
        }

        protected virtual void DrawInspector()
        {
            Label label = new Label(SkillClip.ClipName);
            root.Add(label);
            //创建开始帧字段
            startFrameField = new IntegerField("起始帧");
            //startFrameField.BindProperty(fieldInfo.GetValue().FindProperty("startFrame"));
            startFrameField.SetValueWithoutNotify(SkillClip.StartFrame);
            startFrameField.RegisterValueChangedCallback(arg =>
            {
                //禁止修改为负值
                if (arg.newValue < 0)
                {
                    startFrameField.value = arg.previousValue;
                }
                
                //当值发生变化时更新技能编辑器
                if (arg.previousValue != arg.newValue)
                {
                    SkillClip.StartFrame = arg.newValue;
                    UpdateSkillEditor();
                }
            });
            root.Add(startFrameField);
            
            //创建时长字段
            durationField = new IntegerField("总帧数");
            //durationField.BindProperty(serializedObject.FindProperty("duration"));
            durationField.SetValueWithoutNotify(SkillClip.Duration);
            durationField.RegisterValueChangedCallback(arg =>
            {
                //禁止长度小于1
                if (arg.newValue < 1)
                {
                    durationField.value = arg.previousValue;
                }
                
                //当值发生变化时更新技能编辑器
                if (arg.previousValue != arg.newValue)
                {
                    SkillClip.Duration = arg.newValue;
                    UpdateSkillEditor();
                }
            });
            root.Add(durationField);

        }
        
        protected void UpdateSkillEditor()
        {
            if (skillEditor != null)
            {
                skillEditor.UpdateTrack();
            }
        }
        
    }
}
