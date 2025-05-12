using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MochiFramework.Skill.Editor
{
    [CustomEditor(typeof(AnimationClip))]
    public class AnimationClipInspector : ClipInspector
    {
        private ObjectField animationAssetField;
        private Box animationInfoBox;
        
        private AnimationClip _animationClip;

        public override VisualElement CreateInspectorGUI()
        {
            //_animationClip = serializedObject.targetObject as AnimationClip;
            return base.CreateInspectorGUI();
        }

        protected override void DrawInspector()
        { 
            base.DrawInspector();
            //创建动画资源
            animationAssetField = new ObjectField("动画资源");
            animationAssetField.objectType = typeof(UnityEngine.AnimationClip);
            animationAssetField.allowSceneObjects = false;
            animationAssetField.BindProperty(serializedObject.FindProperty("animationAsset"));
            animationAssetField.RegisterValueChangedCallback(arg =>
            {
                if (arg.newValue != arg.previousValue)
                {
                    //_animationClip.name = $"{nameof(AnimationClip)}({_animationClip.ClipName})";
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    UpdateSkillEditor();
                }
            });
            root.Add(animationAssetField);
            
            //TODO 显示动画资源的信息
            animationInfoBox = new Box();
            animationInfoBox.Add(new Label($"动画名称:\t{_animationClip.AnimationAsset.name}"));
            animationInfoBox.Add(new Label($"动画长度:\t{_animationClip.AnimationAsset.length :0.00}s"));
            animationInfoBox.Add(new Label($"帧率:\t\t{_animationClip.AnimationAsset.frameRate}FPS"));
            animationInfoBox.Add(new Label($"循环:\t\t{_animationClip.AnimationAsset.isLooping}"));
            root.Add(animationInfoBox);
        }
        
    }
}
