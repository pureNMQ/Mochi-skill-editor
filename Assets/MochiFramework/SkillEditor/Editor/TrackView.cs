using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private VisualElement trackClipsView;

        private Label trackTitle;

        private List<ClipView> clipViews;

        private float frameUnitWidth;

        private ITrack track;
        private SkillEditor skillEditor;

        public TrackView(ITrack track, VisualElement trackHeadParent, VisualElement trackClipParent,
            SkillEditor skillEditor)
        {
            this.track = track;
            this.skillEditor = skillEditor;

            this.trackHeadParent = trackHeadParent;
            this.trackClipParent = trackClipParent;
            trackHeadView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MENU_ASSET_PATH).Instantiate().ElementAt(0);
            trackHeadParent.Add(trackHeadView);
            trackTitle = trackHeadView.Q<Label>();
            if (string.IsNullOrEmpty(track.TrackName))
            {
                CustomTrackAttribute customTrackAttribute = track.GetType().GetCustomAttribute<CustomTrackAttribute>();

                string defaultName = customTrackAttribute is null
                    ? track.GetType().Name
                    : customTrackAttribute.DefaultName;

                track.TrackName = defaultName;

            }

            trackTitle.text = track.TrackName;

            //TODO 设置trackClipView的长度为SkillConf的最长长度
            trackClipsView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(TRACK_CLIP_ASSET_PATH).Instantiate()
                .ElementAt(0);
            trackClipsView.style.width = track.SkillConfig.frameCount * frameUnitWidth;
            trackClipParent.Add(trackClipsView);

            clipViews = new List<ClipView>();

            trackHeadView.RegisterCallback<FocusEvent>(OnTrackHeadFocus);
            //trackHeadView.RegisterCallback<MouseDownEvent>(OnTrackHeadMouseDown);

            trackClipsView.RegisterCallback<DragUpdatedEvent>(OnTrackClipsViewDragUpdate);
            trackClipsView.RegisterCallback<DragExitedEvent>(OnTrackClipsViewDragExited);

            trackHeadView.AddManipulator(new ContextualMenuManipulator(OnTrackHeadContextualMenuPopulate));
            trackClipsView.AddManipulator(new ContextualMenuManipulator(OnTrackClipsViewContextualMenuPopulate));
        }

        private void OnTrackClipsViewContextualMenuPopulate(ContextualMenuPopulateEvent evt)
        {
            int frame = skillEditor.GetFrameIndexByMousePos(evt.mousePosition);

            //获取Clip类型
            Type baseTrackType = track.GetType().BaseType;
            if (baseTrackType == null) return;
            Type[] genericTypes = baseTrackType.GetGenericArguments();
            if (genericTypes.Length <= 0) return;
            Type clipType = genericTypes[0];
            List<Type> types = TypeCache.GetTypesDerivedFrom(clipType).ToList();
            types.Add(clipType);
            foreach (var type in types)
            {
                var clipAttribute = type.GetCustomAttribute<CustomClipAttribute>();
                if (clipAttribute == null || clipAttribute.IsCreateMenuItem)
                {
                    evt.menu.AppendAction(type.Name, (x) =>
                    {
                        Clip newClip = Activator.CreateInstance(type) as Clip;
                        newClip!.duration = newClip.OriginalDuration;
                        track.InsertClipAtFrame(frame, newClip);
                        Redraw();
                    });
                }
            }
        }

        private void OnTrackHeadContextualMenuPopulate(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("排序/置于顶层", _ => BringToFront());
            evt.menu.AppendAction("排序/上移", _ => MoveUp());
            evt.menu.AppendAction("排序/下移", _ => MoveDown());
            evt.menu.AppendAction("重命名", _ =>
                TextPopupWindow.Open(OnRenameConfirmed, "重命名轨道", track.TrackName, "请输入新的轨道名:")
            );
            evt.menu.AppendAction("删除", _ => Delete());
        }

        private void OnRenameConfirmed(string newName)
        {
            Undo.RegisterCompleteObjectUndo(track.SkillConfig, $"Rename Track : {track}");
            track.TrackName = newName;
            trackTitle.text = newName;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }


        private void OnTrackHeadFocus(FocusEvent evt)
        {
            skillEditor.ShowObjectOnInspector(track);
            Undo.RegisterFullObjectHierarchyUndo(track.SkillConfig, "Insert Clip");
        }

        private void OnTrackClipsViewDragExited(DragExitedEvent evt)
        {
            object dragObject = GetDragObject();

            if (dragObject is null) return;

            if (track.CanConvertToClip(dragObject))
            {
                Undo.RegisterCompleteObjectUndo(track.SkillConfig, "Insert Clip");

                int selectFrameIndex = skillEditor.GetFrameIndexByMousePos(evt.mousePosition);

                track.InsertObjectAtFrame(selectFrameIndex, dragObject);

                //NOTE 如果不合并当前组就会被立即撤回，原因尚不清楚
                Undo.IncrementCurrentGroup();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                //刷新View
                //Redraw();
                skillEditor.UpdateTrack();
            }
        }

        private void OnTrackClipsViewDragUpdate(DragUpdatedEvent evt)
        {
            object dragObject = GetDragObject();
            if (dragObject is null) return;

            //如果拖拽的资源可以转换为轨道的片段，则改变鼠标样式为复制
            if (track.CanConvertToClip(dragObject))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }

        }


        public void Redraw(float frameUnitWidth, bool isClear = true, object changeObject = null)
        {
            if (this.frameUnitWidth != frameUnitWidth)
            {
                this.frameUnitWidth = frameUnitWidth;
                trackClipsView.style.width = skillEditor.SkillConfig.frameCount * frameUnitWidth;
            }


            if (isClear && (changeObject == null || changeObject == track))
            {
                clipViews.Clear();
                trackClipsView.Clear();
                //生成新的ClipView
                foreach (Clip clip in track)
                {
                    ClipView cv = new ClipView();
                    cv.Init(skillEditor, trackClipsView, track, clip, frameUnitWidth);
                    clipViews.Add(cv);
                }
            }
            else if (!isClear && (changeObject == null || changeObject == track))
            {
                trackClipsView.style.width = track.SkillConfig.frameCount * frameUnitWidth;
                foreach (var cv in clipViews)
                {
                    cv.Redraw(frameUnitWidth, null);
                }
            }
            else if (changeObject is Clip)
            {
                foreach (var cv in clipViews)
                {
                    cv.Redraw(frameUnitWidth, changeObject);
                }
            }


        }

        public void Dispose()
        {
            trackHeadParent.Remove(trackHeadView);
            trackClipParent.Remove(trackClipsView);
        }

        private void Delete()
        {
            Undo.RegisterCompleteObjectUndo(track.SkillConfig, $"Delete Track : {track}");
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
        }

        private void MoveDown()
        {
            int index = track.SkillConfig.tracks.IndexOf(track);
            if (index < 0 || index >= track.SkillConfig.tracks.Count - 1) return;
            AdjustOrder(index + 1);
        }

        private void AdjustOrder(int index)
        {
            Undo.RegisterCompleteObjectUndo(track.SkillConfig, $"Track Adjust Order : {track}");
            track.SkillConfig.tracks.Remove(track);
            track.SkillConfig.tracks.Insert(index, track);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            skillEditor.UpdateTrack();
        }

        private void Redraw(bool isClear = true, object changeObject = null)
        {
            Redraw(this.frameUnitWidth, isClear, changeObject);
        }

        private object GetDragObject()
        {
            object dragObject;
            if (DragAndDrop.objectReferences is not null && DragAndDrop.objectReferences.Length > 0)
            {
                dragObject = DragAndDrop.objectReferences[0];
            }
            else
            {
                dragObject = DragAndDrop.GetGenericData("skill clip");
            }

            return dragObject;
        }

    }
}
