using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MochiFramework.Skill.Editor
{
    public class TrackView : IDisposable
    {
        private const string MENU_ASSET_PATH = "Assets/MochiFramework/SkillEditor/Editor/TrackMenuView.uxml";
        private const string TRACK_CLIP_ASSET_PATH = "Assets/MochiFramework/SkillEditor/Editor/ClipTrackView.uxml";

        private VisualElement trackMenuParent;
        private VisualElement trackClipParent;

        //位于编辑器左侧，显示轨道的总览信息
        private VisualElement trackMenuView;
        //位于编辑器右侧，显示轨道中的片段
        private VisualElement trackClipView;

        private Label trackTitle;
        
        private List<ClipView> clipViews;

        private float frameUnitWidth;

        private Track track;
        private SkillEditor skillEditor;

        public TrackView(Track track, VisualElement trackMenuParent, VisualElement trackClipParent, SkillEditor skillEditor)
        {
            this.track = track;
            this.skillEditor = skillEditor;

            this.trackMenuParent = trackMenuParent;
            this.trackClipParent = trackClipParent;
            trackMenuView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MENU_ASSET_PATH).Instantiate().ElementAt(0);
            trackMenuParent.Add(trackMenuView);
            trackTitle = trackMenuView.Q<Label>();
            trackTitle.text = track.TrackName;

            //TODO 设置trackClipView的长度为SkillConf的最长长度
            trackClipView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TRACK_CLIP_ASSET_PATH).Instantiate().ElementAt(0);
            trackClipParent.Add(trackClipView);

            clipViews = new List<ClipView>();
            
            trackClipView.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            trackClipView.RegisterCallback<DragExitedEvent>(OnDragExited);
            trackMenuView.RegisterCallback<FocusEvent>(OnFocus);
        }

        private void OnFocus(FocusEvent evt)
        {
            //Selection.activeObject = track;
            //TODO 重构Inspector逻辑
        }

        public void Update()
        {
            //清空ClipViews
            clipViews.Clear();
            trackClipView.Clear();
            
            trackClipView.style.width = skillEditor.SkillConfig.FrameCount * frameUnitWidth;
            //生成新的ClipView
            foreach (var clip in track)
            {
                ClipView cv = new ClipView();
                cv.Init(skillEditor,trackClipView,track,clip,frameUnitWidth);
            }
        }
        
        public void Update(float frameUnitWidth)
        {
            this.frameUnitWidth = frameUnitWidth;
            Update();
        }

        public void Dispose()
        {
            trackMenuParent.Remove(trackMenuView);
            trackClipParent.Remove(trackClipView);
        }

        private void OnDragExited(DragExitedEvent evt)
        {
            UnityEngine.Object[] objects = DragAndDrop.objectReferences;

            if (track.CanConvertToClip(objects[0]))
            {
                //BUG 只能撤回，不能重做
                //可能是重做的过程中新建了一个Clip对象，而Track引用的还是旧的Clip对象，导致引用错误
                int selectFrameIndex = skillEditor.GetFrameIndexByMousePos(evt.mousePosition);
                Clip newClip = track.InsertClipAtFrame(selectFrameIndex, objects[0]);
                
                //使用Undo.RegisterCompleteObjectUndo可以避免无法重做的bug，但是撤回的时候该对象不会被销毁
                //ndo.RegisterCreatedObjectUndo(newClip,"Insert Clip");
                Undo.IncrementCurrentGroup();
                
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                //刷新View
                Update();
            }
        }

        private void OnDragUpdate(DragUpdatedEvent evt)
        {
            UnityEngine.Object[] objects = DragAndDrop.objectReferences;
            
            //如果拖拽的资源可以转换为轨道的片段，则改变鼠标样式为复制
            if (track.CanConvertToClip(objects[0]))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }

        }
    }
}
