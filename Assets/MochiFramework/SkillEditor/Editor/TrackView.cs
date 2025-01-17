using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MochiFramework.Skill.Editor
{
    public class TrackView : IDisposable
    {

        private const string MENU_ASSET_PATH = "Assets/MochiFramework/SkillEditor/Editor/TrackMenuView.uxml";
        private const string CLIP_TRACK_ASSET_PATH = "Assets/MochiFramework/SkillEditor/Editor/ClipTrackView.uxml";

        private VisualElement trackMenuParent;
        private VisualElement clipTrackParent;

        //位于编辑器左侧，显示轨道的总览信息
        private VisualElement trackMenuView;
        //位于编辑器右侧，显示轨道中的片段
        private VisualElement clipTrackView;

        private Track track;
        private SkillEditor skillEditor;

        public TrackView(Track track, VisualElement trackMenuParent, VisualElement clipTrackParent, SkillEditor skillEditor)
        {
            this.track = track;
            this.skillEditor = skillEditor;

            this.trackMenuParent = trackMenuParent;
            this.clipTrackParent = clipTrackParent;
            trackMenuView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(MENU_ASSET_PATH).Instantiate();
            trackMenuParent.Add(trackMenuView);

            clipTrackView = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(CLIP_TRACK_ASSET_PATH).Instantiate();
            clipTrackParent.Add(clipTrackView);


            clipTrackView.RegisterCallback<DragUpdatedEvent>(OnDragUpdate);
            clipTrackView.RegisterCallback<DragExitedEvent>(OnDragExited);
        }

        public void Dispose()
        {
            trackMenuParent.Remove(trackMenuView);
            clipTrackParent.Remove(clipTrackView);
        }

        private void OnDragExited(DragExitedEvent evt)
        {
            UnityEngine.Object[] objects = DragAndDrop.objectReferences;

            if (track.CanConvertToClip(objects[0]))
            {
                Debug.Log("在第" + skillEditor.GetFrameIndexByMousePos(evt.localMousePosition) + "帧，添加了一个资源节点" + objects[0].name);
            }
        }

        private void OnDragUpdate(DragUpdatedEvent evt)
        {
            UnityEngine.Object[] objects = DragAndDrop.objectReferences;

            if (track.CanConvertToClip(objects[0]))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }

        }
    }
}
