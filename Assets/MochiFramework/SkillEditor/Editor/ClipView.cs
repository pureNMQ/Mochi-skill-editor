using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MochiFramework.Skill.Editor
{
    public sealed class ClipView
    {
        private const string CLIP_VIEW_ASSET_PATH = "Assets/MochiFramework/SkillEditor/Editor/ClipView.uxml";
        private ITrack track;
        private Clip clip;
        private CustomClipAttribute clipTypeInfo;

        private VisualElement root;
        private Label nameLabel;
        private VisualElement leftEdge;
        private VisualElement rightEdge;

        private float frameUnitWidth;
        private SkillEditor skillEditor;

        private Color normalColor;
        private Color hoverColor;
        private Color selectedColor;

        private bool isDrag = false;
        private bool isRightEdgeDrag = false;
        private bool isLeftEdgeDrag = false;
        private Vector2 dragStartPos;
        private Vector2 dragOffsetPos;
        //拖拽时最后一个有效帧
        private int lastValidFrame = -1;

        public void Init(SkillEditor skillEditor, VisualElement parent, ITrack track, Clip clip, float frameUnitWidth)
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
            leftEdge = root.Q<VisualElement>("LeftEdge");
            rightEdge = root.Q<VisualElement>("RightEdge");

            //NOTE 调整位置模式为绝对位置，而不是相对自动布局后的位置
            root.style.position = Position.Absolute;
            SetViewPosition(clip.startFrame);

            //设置三种状态的颜色
            SetCustomColor();
            root.style.backgroundColor = normalColor;

            //绑定事件
            root.RegisterCallback<MouseDownEvent>(OnMouseDown);
            root.parent.RegisterCallback<MouseUpEvent>(OnMouseUp);
            root.parent.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            root.parent.RegisterCallback<MouseOutEvent>(OnMouseOut);
            root.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
            root.RegisterCallback<FocusEvent>(OnFocus);

            leftEdge.RegisterCallback<MouseDownEvent>(OnEdgeMouseDown);
            rightEdge.RegisterCallback<MouseDownEvent>(OnEdgeMouseDown);

            root.AddManipulator(new ContextualMenuManipulator(OnContextualMenuPopulate));
        }


        private void OnEdgeMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                if (evt.target == rightEdge)
                {
                    isRightEdgeDrag = true;
                }
                else if (evt.target == leftEdge)
                {
                    isLeftEdgeDrag = true;
                }
                Debug.Log("EdgeMouseDown");
                evt.StopPropagation();
            }
        }

        public void Redraw(float frameUnitWidth, object changeObject = null)
        {
            if (this.frameUnitWidth != frameUnitWidth || changeObject == null || changeObject == clip)
            {
                this.frameUnitWidth = frameUnitWidth;
                SetViewPosition(clip.startFrame);
                nameLabel.text = clip.ClipName;
            }
        }

        private void OnContextualMenuPopulate(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("重置长度", _ => ResetDuration());
            evt.menu.AppendAction("删除", _ => Delete());
            //阻止事件向父级传播，确保仅对当前Clip进行操作
            evt.StopPropagation();
        }

        private void OnFocus(FocusEvent evt)
        {
            skillEditor.ShowObjectOnInspector(clip);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                root.style.backgroundColor = selectedColor;
                isDrag = true;
                dragOffsetPos = (Vector2)root.worldTransform.GetPosition() - evt.mousePosition;
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
                dragStartPos = evt.mousePosition + dragOffsetPos;
                int frame = skillEditor.GetFrameIndexByMousePos(dragStartPos);
                if (frame < 0)
                {
                    frame = 0;
                }

                //判断是否可以移动到该为止
                int correctionDuration = track.CalculateCorrectionDuration(frame, clip.duration, clip);
                if (clip.duration == correctionDuration)
                {
                    lastValidFrame = frame;
                }

                SetViewPosition(frame);
                //NOTE 该元素将在视觉上位于任何重叠的同级元素前面
                root.BringToFront();
            }
            else if (isRightEdgeDrag || isLeftEdgeDrag)
            {
                int frame = skillEditor.GetFrameIndexByMousePos(evt.mousePosition);
                if (frame < 0)
                {
                    frame = 0;
                }
                if (isLeftEdgeDrag)
                {
                    if (frame < clip.EndFrame)
                    {
                        int endFrame = clip.startFrame + clip.duration;
                        clip.startFrame = frame;

                        int correctionDuration = track.CalculateCorrectionDuration(frame, endFrame - frame, clip);

                        clip.duration = correctionDuration;
                    }
                }
                else if (isRightEdgeDrag)
                {
                    //为了让调整更加跟手,修改数值
                    frame++;
                    if (frame > clip.startFrame)
                    {
                        int correctionDuration = track.CalculateCorrectionDuration(clip.startFrame, frame - clip.startFrame, clip);

                        clip.duration = correctionDuration;
                    }
                }
            }
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if (evt.button == 0)
            {
                root.style.backgroundColor = hoverColor;
                if (isDrag)
                {
                    ApplyDrag();
                    isDrag = false;
                }

                isRightEdgeDrag = false;
                isLeftEdgeDrag = false;
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
                DragAndDrop.SetGenericData("skill clip", clip);
                DragAndDrop.StartDrag("skill clip");
                ApplyDrag();
            }
        }

        private void SetViewPosition(int frame)
        {
            //计算自身位置
            Vector3 pos = root.transform.position;
            pos.x = frame * this.frameUnitWidth;
            root.transform.position = pos;
            root.style.width = this.clip.duration * this.frameUnitWidth;
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
            Undo.RegisterCompleteObjectUndo(clip.SkillConfig, "Move Clip");
            track.MoveClipToFrame(clip, lastValidFrame);
            //重新设置View的位置
            SetViewPosition(clip.startFrame);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void Delete()
        {
            Undo.RegisterCompleteObjectUndo(clip.SkillConfig, "Delete Clip");
            track.RemoveClip(clip);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            skillEditor.UpdateTrack();
        }

    }
}
