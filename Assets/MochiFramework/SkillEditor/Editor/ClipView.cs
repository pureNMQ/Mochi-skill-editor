using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MochiFramework.Skill.Editor
{
    public class ClipView
    {
        private const string CLIP_VIEW_ASSET_PATH = "Assets/MochiFramework/SkillEditor/Editor/ClipView.uxml";
        private SkillTrack _skillTrack;
        private SkillClip _skillClip;
        private CustomClipAttribute clipTypeInfo;
        
        private VisualElement root;
        private Label nameLabel;
        private float frameUnitWidth;
        private SkillEditor skillEditor;
        
        private Color normalColor;
        private Color hoverColor;
        private Color selectedColor;

        private bool isDrag = false;
        private Vector2 dragStartPos;
        private Vector2 dragOffestPos;
        //拖拽时最后一个有效帧
        private int lastValidFrame = -1;
        
        public void Init(SkillEditor skillEditor,VisualElement parent,SkillTrack skillTrack,SkillClip skillClip,float frameUnitWidth)
        {
            //初始化成员变量
            this.skillEditor = skillEditor;
            this._skillTrack = skillTrack;
            this._skillClip = skillClip;
            this.frameUnitWidth = frameUnitWidth;
            clipTypeInfo = skillClip.GetType().GetCustomAttribute<CustomClipAttribute>();
            
            //构建View
            root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CLIP_VIEW_ASSET_PATH).Instantiate().Q("ClipView");
            nameLabel = root.Q<Label>("ClipName");
            nameLabel.text = skillClip.ClipName;
            parent.Add(root);
            
            //NOTE 调整位置模式为绝对位置，而不是相对自动布局后的位置
            root.style.position = Position.Absolute;
            SetPosition(skillClip.StartFrame);
            
            //设置三种状态的颜色
            SetCustomColor();
            root.style.backgroundColor = normalColor;
            
            //绑定事件
            root.RegisterCallback<MouseDownEvent>(OnMouseDown);
            root.RegisterCallback<MouseUpEvent>(OnMouseUp);
            root.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            root.RegisterCallback<MouseOutEvent>(OnMouseOut);
            root.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            root.RegisterCallback<FocusEvent>(OnFocus);
        }
        
        private void OnFocus(FocusEvent evt)
        {
            skillEditor.ShowObjectOnInspector(_skillClip);
        }


        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                root.style.backgroundColor = selectedColor;
                isDrag = true;
                dragOffestPos = (Vector2)root.worldTransform.GetPosition() - evt.mousePosition;
            }
            else if(evt.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("重置长度"), false, ResetDuration);
                menu.AddItem(new GUIContent("删除"), false, Delete);
                menu.ShowAsContext();
            }
        }

        private void ResetDuration()
        {
            _skillTrack.ResetClipDuration(_skillClip);
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            if (isDrag)
            {
                dragStartPos = evt.mousePosition + dragOffestPos;
                int frame = skillEditor.GetFrameIndexByMousePos(dragStartPos);
                if (frame < 0)
                {
                    frame = 0;
                }
                //TODO 添加新的判断方式，不会修改长度，只会调整位置
                //判断是否可以移动到该为止
                if (_skillTrack.CanInsertClipAtFrame(frame, _skillClip.Duration, out int correctionDuration, _skillClip))
                {
                    if (_skillClip.Duration == correctionDuration)
                    {
                        lastValidFrame = frame;
                    }
                }
                
                SetPosition(frame);
                //NOTE 该元素将在视觉上位于任何重叠的同级元素前面
                root.BringToFront();
            }
        }
        
        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
            {
                root.style.backgroundColor = hoverColor;
                isDrag = false;
                ApplyDrag();
            }
        }
        
        private void OnMouseEnter(MouseEnterEvent evt)
        {
            root.style.backgroundColor = hoverColor;
        }
        
        private void OnMouseOut(MouseOutEvent evt)
        {
            root.style.backgroundColor = normalColor;
            if (isDrag)
            {
                isDrag = false;
                ApplyDrag();
            }
        }

        private void SetPosition(int frame)
        {
            //计算自身位置
            Vector3 pos = root.transform.position;
            pos.x = frame * this.frameUnitWidth;
            root.transform.position = pos;
            root.style.width = this._skillClip.Duration * this.frameUnitWidth;
        }

        private void SetCustomColor()
        {
            if (clipTypeInfo is not null)
            {
                normalColor = ColorHelper.HexToColor(clipTypeInfo.HexColor);
            }
            else
            {
                normalColor = ColorHelper.HexToColor("#757575");
            }

            hoverColor = normalColor * 1.05f;
            selectedColor = normalColor * 0.9f;
        }

        private void ApplyDrag()
        {
            Undo.RegisterCompleteObjectUndo(_skillTrack.SkillConfig,"Move Clip");
            _skillTrack.MoveClipToFrame(_skillClip,lastValidFrame);
            //重新设置View的位置
            SetPosition(_skillClip.StartFrame);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            skillEditor.UpdateInspector();
        }
        
        private void Delete()
        {
            Undo.RegisterCompleteObjectUndo(_skillTrack.SkillConfig,"Delete Clip");
            _skillTrack.RemoveClip(_skillClip);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            skillEditor.UpdateTrack();
        }
        
    }
}
