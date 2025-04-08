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
        private Track track;
        private Clip clip;
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
        
        public void Init(SkillEditor skillEditor,VisualElement parent,Track track,Clip clip,float frameUnitWidth)
        {
            //初始化成员变量
            this.skillEditor = skillEditor;
            this.track = track;
            this.clip = clip;
            this.frameUnitWidth = frameUnitWidth;
            clipTypeInfo = clip.GetType().GetCustomAttribute<CustomClipAttribute>();
            
            //构建View
            root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CLIP_VIEW_ASSET_PATH).Instantiate().Q("ClipView");
            nameLabel = root.Q<Label>("ClipName");
            nameLabel.text = clip.ClipName;
            parent.Add(root);
            
            //NOTE 调整位置模式为绝对位置，而不是相对自动布局后的位置
            root.style.position = Position.Absolute;
            SetPosition(clip.StartFrame);
            
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
            Selection.activeObject = clip;
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
            track.ResetClipDuration(clip);
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
            root.style.width = this.clip.Duration * this.frameUnitWidth;
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
            //TODO 重新设置Clip的位置，并且提供撤回功能
            Debug.Log("Apply Drag");
            int frame = skillEditor.GetFrameIndexByMousePos(dragStartPos);
            if (frame < 0)
            {
                frame = 0;
            }
            
            Undo.RegisterCompleteObjectUndo(clip,"Move Clip");
            track.MoveClipToFrame(clip,frame);
            
            //重新设置View的位置
            SetPosition(clip.StartFrame);
        }
        
        private void Delete()
        {
            Undo.RegisterCompleteObjectUndo(new Object[]{track,clip},"Delete Clip");
            
            track.RemoveClip(clip);
            Object.DestroyImmediate(clip,true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            skillEditor.UpdateTrack();
        }
        
    }
}
