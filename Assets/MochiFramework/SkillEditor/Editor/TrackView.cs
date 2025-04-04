using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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

            trackClipView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TRACK_CLIP_ASSET_PATH).Instantiate().ElementAt(0);
            trackClipParent.Add(trackClipView);

            clipViews = new List<ClipView>();
            
            trackClipView.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            trackClipView.RegisterCallback<DragExitedEvent>(OnDragExited);
        }

        public void Update()
        {
            //TODO 只刷新内容
            //清空ClipViews
            clipViews.Clear();
            trackClipView.Clear();
            
            //生成新的ClipView
            foreach (var clip in track)
            {
                ClipView cv = new ClipView();
                cv.Init(skillEditor,trackClipView,track,clip,frameUnitWidth);
            }
        }
        
        public void Update(float frameUnitWidth)
        {
            //TODO 刷新视图
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
                int selectFramIndex = skillEditor.GetFrameIndexByMousePos(evt.mousePosition);
                track.InsertClipAtFrame(selectFramIndex, objects[0]);
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
