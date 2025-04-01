using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MochiFramework.Skill
{
    public class ClipView
    {
        private const string CLIP_VIEW_ASSET_PATH = "Assets/MochiFramework/SkillEditor/Editor/ClipView.uxml";
        private Clip clip;
        private VisualElement root;
        private Label nameLabel;
        private float frameUnitWidth;

        public void Init(VisualElement parent,Clip clip,float frameUnitWidth)
        {
            this.clip = clip;
            this.frameUnitWidth = frameUnitWidth;
            root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CLIP_VIEW_ASSET_PATH).Instantiate().Q("ClipView");
            nameLabel = root.Q<Label>("ClipName");
            nameLabel.text = clip.ClipName;
            parent.Add(root);
            
            //NOTE 调整位置模式为绝对位置，而不是相对自动布局后的位置
            root.style.position = Position.Absolute;
            //计算自身位置
            Vector3 pos = root.transform.position;
            pos.x = this.clip.StartFrame * this.frameUnitWidth;
            root.transform.position = pos;
            root.style.width = this.clip.Duration * this.frameUnitWidth;
        }
    }
}
