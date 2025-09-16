using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace MochiFramework.Skill.Editor
{
    public sealed class TrackView : IDisposable
    {
        private const string MENU_ASSET_PATH = "Assets/MochiFramework/SkillEditor/Editor/TrackMenuView.uxml";
        private const string TRACK_CLIP_ASSET_PATH = "Assets/MochiFramework/SkillEditor/Editor/ClipTrackView.uxml";

        private VisualElement trackHeadParent;
        private VisualElement trackClipParent;

        //位于编辑器左侧，显示轨道的总览信息
        private VisualElement trackHeadView;

        //位于编辑器右侧，显示轨道中的片段
        private VisualElement trackClipView;

        private Label trackTitle;

        private List<ClipView> clipViews;

        private float frameUnitWidth;

        private Track track;
        private SkillEditor skillEditor;

        public TrackView(Track track, VisualElement trackHeadParent, VisualElement trackClipParent,
            SkillEditor skillEditor)
        {
            this.track = track;
            this.skillEditor = skillEditor;

            this.trackHeadParent = trackHeadParent;
            this.trackClipParent = trackClipParent;
            trackHeadView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MENU_ASSET_PATH).Instantiate().ElementAt(0);
            trackHeadParent.Add(trackHeadView);
            trackTitle = trackHeadView.Q<Label>();
            trackTitle.text = track.TrackName;

            //TODO 设置trackClipView的长度为SkillConf的最长长度
            trackClipView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TRACK_CLIP_ASSET_PATH).Instantiate()
                .ElementAt(0);
            trackClipView.style.width = track.SkillConfig.frameCount * frameUnitWidth;
            trackClipParent.Add(trackClipView);

            clipViews = new List<ClipView>();

            trackClipView.RegisterCallback<DragUpdatedEvent>(OnTrackClipDragUpdate);
            trackClipView.RegisterCallback<DragExitedEvent>(OnTrackClipDragExited);
            trackHeadView.RegisterCallback<FocusEvent>(OnTrackHeadFocus);
            trackHeadView.RegisterCallback<MouseDownEvent>(OnTrackHeadMouseDown);
        }

        private void OnTrackHeadMouseDown(MouseDownEvent evt)
        {
            if (evt.button == 1)
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("置于顶层"),false,BringToFront);
                menu.AddItem(new GUIContent("上移"), false,MoveUp);
                menu.AddItem(new GUIContent("下移"), false,MoveDown);
                menu.AddItem(new GUIContent("删除"), false, Delete);
                menu.ShowAsContext();
            }
        }


        private void OnTrackHeadFocus(FocusEvent evt)
        {
            skillEditor.ShowObjectOnInspector(track);
            Undo.RegisterFullObjectHierarchyUndo(track.SkillConfig, "Insert Clip");
        }

        private void OnTrackClipDragExited(DragExitedEvent evt)
        {
            UnityEngine.Object[] objects = DragAndDrop.objectReferences;
            if (track.CanConvertToClip(objects[0]))
            {
                Undo.RegisterCompleteObjectUndo(track.SkillConfig, "Insert Clip");
                int selectFrameIndex = skillEditor.GetFrameIndexByMousePos(evt.mousePosition);
                track.InsertClipAtFrame(selectFrameIndex, objects[0]);
                //NOTE 如果不合并当前组就会被立即撤回，原因尚不清楚
                Undo.IncrementCurrentGroup();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //刷新View
                Redraw();
            }
        }

        private void OnTrackClipDragUpdate(DragUpdatedEvent evt)
        {
            UnityEngine.Object[] objects = DragAndDrop.objectReferences;

            //如果拖拽的资源可以转换为轨道的片段，则改变鼠标样式为复制
            if (track.CanConvertToClip(objects[0]))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }

        }


        public void Redraw(float frameUnitWidth, bool isClear = true, object changeObject = null)
        {
            if (this.frameUnitWidth != frameUnitWidth)
            {
                this.frameUnitWidth = frameUnitWidth;
                trackClipView.style.width = skillEditor.SkillConfig.frameCount * frameUnitWidth;
            }


            if (isClear && (changeObject == null || changeObject == track))
            {
                clipViews.Clear();
                trackClipView.Clear();
                //生成新的ClipView
                foreach (var clip in track)
                {
                    ClipView cv = new ClipView();
                    cv.Init(skillEditor, trackClipView, track, clip, frameUnitWidth);
                    clipViews.Add(cv);
                }
            }
            else if (!isClear && (changeObject == null || changeObject == track))
            {
                trackClipView.style.width = track.SkillConfig.frameCount * frameUnitWidth;
                foreach (var cv in clipViews)
                {
                    cv.Redraw(frameUnitWidth, null);
                }
            }
        }

        public void Dispose()
        {
            trackHeadParent.Remove(trackHeadView);
            trackClipParent.Remove(trackClipView);
        }

        private void Delete()
        {
            Undo.RegisterCompleteObjectUndo(track.SkillConfig,$"Delete Track : {track}");
            track.SkillConfig.tracks.Remove(track);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            skillEditor.UpdateTrack();
        }

        private void BringToFront()
        {
            AdjustOrder(0);
        }

        private void MoveUp()
        {
            int index = track.SkillConfig.tracks.IndexOf(track);
            if (index <= 0 || index >= track.SkillConfig.tracks.Count) return;
            AdjustOrder(index - 1);
            Debug.Log($"调整至{index - 1}");
        }

        private void MoveDown()
        {
            int index = track.SkillConfig.tracks.IndexOf(track);
            if (index < 0 || index >= track.SkillConfig.tracks.Count - 1) return;
            AdjustOrder(index + 1);
        }
        
        private void AdjustOrder(int index)
        {
            Undo.RegisterCompleteObjectUndo(track.SkillConfig,$"Track Adjust Order : {track}");
            track.SkillConfig.tracks.Remove(track);
            track.SkillConfig.tracks.Insert(index,track);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            skillEditor.UpdateTrack();
        }

        private void Redraw(bool isClear = true, object changeObject = null)
        {
            Redraw(this.frameUnitWidth, isClear, changeObject);
        }

    }
}
