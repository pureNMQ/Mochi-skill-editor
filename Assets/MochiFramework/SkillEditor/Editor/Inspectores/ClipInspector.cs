using System;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine;

namespace MochiFramework.Skill.Editor
{
    [CustomPropertyDrawer(typeof(Clip))]
    public class ClipInspector : UnityEditor.PropertyDrawer
    {
        private IntegerField startFrameField;
        private IntegerField durationField;
        
        protected VisualElement root;
        protected SkillEditor skillEditor;
        protected Clip clip;
        protected SerializedProperty property;

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            if (SkillEditor.HasOpenInstances<SkillEditor>())
            {
                skillEditor = SkillEditor.GetWindow<SkillEditor>();
            }
            clip = property.boxedValue as Clip;
            if (clip == null) return null;
            
            this.property = property;
            root = new VisualElement();
            DrawInspector();
            
            return root;
        }


        /// <summary>
        /// 在基础检视器绘制完成后
        /// </summary>
        /// <returns>返回最后一个自定义渲染的字段</returns>
        protected virtual SerializedProperty OnDrawInspector()
        {
            return property.FindPropertyRelative("duration");
        }
        
        protected void DrawInspector()
        {
            Label label = new Label(clip.ClipName);
            root.Add(label);
            //创建开始帧字段
            startFrameField = new IntegerField("起始帧");
            startFrameField.BindProperty(property.FindPropertyRelative("startFrame"));
            startFrameField.SetValueWithoutNotify(clip.startFrame);
            startFrameField.isDelayed = true;
            startFrameField.RegisterValueChangedCallback(arg =>
            {
                if(arg.previousValue == arg.newValue) return;
                //禁止修改为负值
                if (arg.newValue < 0)
                {
                    startFrameField.value = arg.previousValue;
                }
            
                if (clip.Track.MoveClipToFrame(clip, arg.newValue))
                {
                    UpdateSkillEditor();
                    Debug.Log("更新技能编辑器");
                }
                else
                {
                    startFrameField.value = arg.previousValue;
                }
            });
            root.Add(startFrameField);
            
            // //创建时长字段
            // //TODO 总帧数暂时不支持修改
            durationField = new IntegerField("总帧数");
            durationField.BindProperty(property.FindPropertyRelative("duration"));
            durationField.SetValueWithoutNotify(clip.duration);
            durationField.isReadOnly = true;
            durationField.focusable = false;
            durationField.RegisterValueChangedCallback(arg =>
            {
                if (arg.newValue < 1)
                {
                    durationField.value = arg.previousValue;
                    return;
                }
                
                //当值发生变化时更新技能编辑器
                if (arg.previousValue != arg.newValue)
                {
                    UpdateSkillEditor();
                }
            });
            root.Add(durationField);

            var childProp = OnDrawInspector();
            
            while (childProp.NextVisible(false))
            {
                PropertyField field = new PropertyField(childProp);
                root.Add(new PropertyField(childProp));
                root.Bind(childProp.serializedObject);
            }
        }
        
        protected void UpdateSkillEditor()
        {
            if (skillEditor != null)
            {
                skillEditor.UpdateTrack(false,clip);
            }
        }
    }
}
