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
            skillEditor.ShowObjectOnInspector(track);
            Undo.RegisterFullObjectHierarchyUndo(track.SkillConfig,"Insert Clip");
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
                Undo.RegisterCompleteObjectUndo(track.SkillConfig,"Insert Clip");
                int selectFrameIndex = skillEditor.GetFrameIndexByMousePos(evt.mousePosition);
                track.InsertClipAtFrame(selectFrameIndex, objects[0]);
                //NOTE 如果不合并当前组就会被立即撤回，原因尚不清楚
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
