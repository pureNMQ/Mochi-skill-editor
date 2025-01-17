using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using UnityEditor.SceneManagement;
using UnityEngine.PlayerLoop;
using System.Collections.Generic;

namespace MochiFramework.Skill.Editor
{
    public class SkillEditor : EditorWindow
    {
        private VisualElement root;
        private const string iconPath = "Assets/MochiFramework/SkillEditor/Editor/Icon/Icon.png";
        private bool IsPreview => EditorSceneManager.GetActiveScene().path == previewScenePath;

        [MenuItem("MochiFramework/技能编辑器")]
        public static void ShowExample()
        {
            SkillEditor wnd = GetWindow<SkillEditor>();
            wnd.titleContent = new GUIContent("技能编辑器");
            wnd.titleContent.image = AssetDatabase.LoadAssetAtPath<Texture>(iconPath);

        }

        public void CreateGUI()
        {
            // Each editor window contains a root VisualElement object
            root = rootVisualElement;

            // Import UXML
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/MochiFramework/SkillEditor/Editor/SkillEditor.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/MochiFramework/SkillEditor/Editor/SkillEditor.uss");

            skillEditorConfig = new SkillEditorConfig();
            InitTopMenu();
            InitTimeShaft();
            InitTrackView();
            InitController();


            //临时测试
            SkillConfig tempConfig = ScriptableObject.CreateInstance<SkillConfig>();
            tempConfig.name = "Temp Skill";
            tempConfig.SkillName = "Temp Skill";
            tempConfig.FrameCount = 120;

            tempConfig.tracks = new List<Track>();
            tempConfig.tracks.Add(new AnimationTrack());

            SkillConfigField.value = tempConfig;
        }

        #region TopMenu

        private const string previewScenePath = "Assets/MochiFramework/SkillEditor/Editor/Scenes/SkillEditorScene.unity";
        private const string previewCharacterRootPath = "PreviewCharacterRoot";
        private string lastGameScenePath;
        private GameObject previewPrefab;

        private Button PreviewSceneButton;
        private Button GameSceneButton;
        private Button SkillInfoButton;

        private ObjectField PreviewPrefabField;
        private ObjectField SkillConfigField;

        private void InitTopMenu()
        {
            PreviewSceneButton = root.Q<Button>(nameof(PreviewSceneButton));
            GameSceneButton = root.Q<Button>(nameof(GameSceneButton));
            SkillInfoButton = root.Q<Button>(nameof(SkillInfoButton));
            PreviewPrefabField = root.Q<ObjectField>(nameof(PreviewPrefabField));
            SkillConfigField = root.Q<ObjectField>(nameof(SkillConfigField));

            PreviewSceneButton.clicked += OnClickPreviewSceneButton;
            GameSceneButton.clicked += OnClickGameSceneButton;
            SkillInfoButton.clicked += OnClickSkillInfoButton;

            PreviewPrefabField.objectType = typeof(GameObject);
            SkillConfigField.objectType = typeof(SkillConfig);

            PreviewPrefabField.RegisterValueChangedCallback(OnPreviewPrefabValueChanged);
            SkillConfigField.RegisterValueChangedCallback(OnSkillConfigValueChanged);
        }

        private void OnPreviewPrefabValueChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            previewPrefab = evt.newValue as GameObject;

            if (!IsPreview) return;

            ShowPreviewCharacter();
        }


        private void OnSkillConfigValueChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            SetSkillConfig(evt.newValue as SkillConfig);
        }


        private void OnClickSkillInfoButton()
        {

        }


        private void OnClickGameSceneButton()
        {
            if (!IsPreview || string.IsNullOrEmpty(lastGameScenePath)) return;

            EditorSceneManager.OpenScene(lastGameScenePath);
        }

        //切换至预览场景
        private void OnClickPreviewSceneButton()
        {
            if (IsPreview) return;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                lastGameScenePath = EditorSceneManager.GetActiveScene().path;
                EditorSceneManager.OpenScene(previewScenePath);
                ShowPreviewCharacter();
            }

        }
        #endregion

        #region TimeShaft
        private IMGUIContainer TimeShaft;
        private IMGUIContainer SelectLine;

        /// <summary>
        /// 轨道容器容器的X位置（局部坐标）
        /// </summary>
        /// <returns></returns>
        private float contentOffsetX => Mathf.Abs(TrackContainerView.horizontalScroller.value);

        private void InitTimeShaft()
        {
            TimeShaft = root.Q<IMGUIContainer>(nameof(TimeShaft));
            SelectLine = root.Q<IMGUIContainer>(nameof(SelectLine));

            TimeShaft.onGUIHandler += DrawTimeShaft;
            SelectLine.onGUIHandler += DrawSelectLine;

            TimeShaft.RegisterCallback<WheelEvent>(OnWheelTimeShaft);
            TimeShaft.RegisterCallback<MouseMoveEvent>(OnMouseMoveTimeShaft);
            TimeShaft.RegisterCallback<MouseDownEvent>(OnMouseDownTimeShaft);
            TimeShaft.RegisterCallback<MouseUpEvent>(OnMouseUpTimeShaft);
            TimeShaft.RegisterCallback<MouseOutEvent>(OnMouseOutTimeShaft);
        }

        private void OnMouseOutTimeShaft(MouseOutEvent evt)
        {
            skillEditorConfig.selectLineDragging = false;
        }


        private void OnMouseDownTimeShaft(MouseDownEvent evt)
        {
            skillEditorConfig.selectLineDragging = true;
            SelectFrame = GetFrameIndexByMousePos(evt.localMousePosition);
        }

        private void OnMouseUpTimeShaft(MouseUpEvent evt)
        {
            skillEditorConfig.selectLineDragging = false;
        }
        private void OnMouseMoveTimeShaft(MouseMoveEvent evt)
        {
            if (skillEditorConfig.selectLineDragging)
            {
                SelectFrame = GetFrameIndexByMousePos(evt.localMousePosition);

            }
        }

        private void OnWheelTimeShaft(WheelEvent evt)
        {
            int delta = (int)Mathf.Sign(evt.delta.y) * 2;

            float rePos = contentOffsetX / skillEditorConfig.frameUnitWidth;

            skillEditorConfig.frameUnitWidth = Mathf.Clamp(skillEditorConfig.frameUnitWidth - delta, skillEditorConfig.standFrameUnitWidth, skillEditorConfig.maxFrameUnitWidth);

            TrackContainerView.horizontalScroller.value = rePos * skillEditorConfig.frameUnitWidth;

            UpdateTrackViewSize();
            TimeShaft.MarkDirtyLayout();
            SelectLine.MarkDirtyLayout();
        }

        private void DrawSelectLine()
        {
            if (skillConfig == null) return;


            Handles.BeginGUI();
            Handles.color = Color.white;
            float posX = SelectFrame * skillEditorConfig.frameUnitWidth - contentOffsetX;
            float sizeY = TimeShaft.contentRect.size.y + TrackContainerView.contentRect.size.y - TrackContainerView.horizontalScroller.contentRect.height;

            Vector2 pos = default;
            Vector2 size = default;
            Rect rect = default;

            if (posX >= 0)
            {
                pos = new Vector2(posX, 0);
                size = new Vector2(skillEditorConfig.frameUnitWidth, sizeY);
                rect = new Rect(pos, size);
                Handles.DrawSolidRectangleWithOutline(rect, new Color(1, 1, 1, 0.1f), new Color(1, 1, 1, 0.1f));
            }
            else if (posX + skillEditorConfig.frameUnitWidth > 0)
            {
                pos = new Vector2(0, 0);
                size = new Vector2(skillEditorConfig.frameUnitWidth + posX, sizeY);
                rect = new Rect(pos, size);
                Handles.DrawSolidRectangleWithOutline(rect, new Color(1, 1, 1, 0.1f), new Color(1, 1, 1, 0.1f));
            }


            Handles.EndGUI();
        }

        private void DrawTimeShaft()
        {
            Handles.BeginGUI();

            Handles.color = Color.white;
            Rect rect = TimeShaft.contentRect;

            int index = Mathf.CeilToInt(TrackContainerView.horizontalScroller.value / skillEditorConfig.frameUnitWidth);

            //绘制的第一帧的起始位置
            float startOffset = 0;
            if (index > 0)
            {
                startOffset = TrackContainerView.horizontalScroller.value % skillEditorConfig.frameUnitWidth;
                if (startOffset > 0) startOffset = skillEditorConfig.frameUnitWidth - startOffset;
            }

            //每5帧一个长条，方便查看
            int SmallStep = 5;
            //每30帧绘制一个更长的长条，表示一秒，方便查看
            int BigStep = 30;

            //Tip: 绘制的位置会超出范围一个像素，防止显示的最后一帧起始位置刚好在时间轴的边界导致未显示
            for (float x = startOffset; x < rect.width + 1; x += skillEditorConfig.frameUnitWidth)
            {
                Handles.color = Color.gray;
                if (index % BigStep == 0)
                {
                    Handles.color = Color.white;
                    Handles.DrawLine(new Vector3(x, rect.height - 15), new Vector3(x, rect.height));
                    string indexStr = index.ToString();
                    Handles.Label(new Vector3(x - indexStr.Length * 4f, rect.height - 25), indexStr);
                }
                else if (index % SmallStep == 0)
                {
                    Handles.DrawLine(new Vector3(x, rect.height - 10), new Vector3(x, rect.height));

                    if (skillEditorConfig.frameUnitWidth > 6)
                    {
                        string indexStr = index.ToString();
                        Handles.Label(new Vector3(x - indexStr.Length * 4f, rect.height - 20), indexStr);
                    }
                }
                else if (skillEditorConfig.frameUnitWidth > 6)
                {
                    Handles.DrawLine(new Vector3(x, rect.height - 5), new Vector3(x, rect.height));
                }

                index++;

                if (index > TotalFrame)
                {
                    break;
                }
            }


            Handles.color = Color.black;
            Handles.DrawLine(new Vector3(0, TimeShaft.contentRect.height), new Vector3(TimeShaft.contentRect.width, TimeShaft.contentRect.height));

            Handles.EndGUI();
        }


        #endregion

        #region TrackView
        private ScrollView TrackContainerView;
        private VisualElement ClipTrackContainer => TrackContainerView.contentContainer;
        private VisualElement TrackMenuContainer;
        private List<TrackView> trackViews;

        private void InitTrackView()
        {
            TrackContainerView = root.Q<ScrollView>(nameof(TrackContainerView));
            TrackMenuContainer = root.Q<VisualElement>(nameof(TrackMenuContainer));
            trackViews = new List<TrackView>();
        }

        private void UpdateTrackViewSize()
        {
            TrackContainerView.contentContainer.style.width = TotalFrame * skillEditorConfig.frameUnitWidth;
        }


        private void UpdateTrack()
        {
            ClearTrack();
            if (skillConfig == null) return;

            foreach (var track in skillConfig.tracks)
            {
                Debug.Log("添加一个轨道");
                TrackView tv = new TrackView(track, TrackMenuContainer, ClipTrackContainer, this);
                trackViews.Add(tv);
            }

        }

        /// <summary>
        /// 清空显示在时间轴上的轨道
        /// </summary>
        private void ClearTrack()
        {
            if (trackViews == null) return;

            foreach (var trackView in trackViews)
            {
                if (trackView != null)
                {
                    trackView.Dispose();
                }
            }

            trackViews.Clear();
        }

        #endregion

        #region Controller

        private Button StartFrameButton;
        private Button PreviousFrameButton;
        private Button PlayOrStopButton;
        private Button NextFrameButton;
        private Button EndFrameButton;

        private IntegerField SelectionFrameField;
        private IntegerField TotalFrameField;

        private void InitController()
        {
            StartFrameButton = root.Q<Button>(nameof(StartFrameButton));
            PreviousFrameButton = root.Q<Button>(nameof(PreviousFrameButton));
            PlayOrStopButton = root.Q<Button>(nameof(PlayOrStopButton));
            NextFrameButton = root.Q<Button>(nameof(NextFrameButton));
            EndFrameButton = root.Q<Button>(nameof(EndFrameButton));

            SelectionFrameField = root.Q<IntegerField>(nameof(SelectionFrameField));
            TotalFrameField = root.Q<IntegerField>(nameof(TotalFrameField));

            StartFrameButton.clicked += OnClickedStartFrame;
            PreviousFrameButton.clicked += OnClickedPreviousFrame;
            PlayOrStopButton.clicked += OnClickedPlayOrStop;
            NextFrameButton.clicked += OnClickedNextFrame;
            EndFrameButton.clicked += OnClickedEndFrame;

            SelectionFrameField.RegisterValueChangedCallback<int>(OnSelectionFrameFieldChange);
            TotalFrameField.RegisterValueChangedCallback<int>(OnTotalFrameFieldChange);

        }

        private void OnTotalFrameFieldChange(ChangeEvent<int> evt)
        {
            if (evt.newValue < 1)
            {
                TotalFrameField.value = evt.previousValue;
            }
            else if (skillConfig != null)
            {
                TotalFrame = evt.newValue;
                TimeShaft.MarkDirtyLayout();
            }


        }

        private void OnSelectionFrameFieldChange(ChangeEvent<int> evt)
        {
            if (evt.newValue < 1)
            {
                SelectionFrameField.value = evt.previousValue;
            }
            else
            {
                skillEditorConfig.selectFrame = evt.newValue - 1;
                SelectLine.MarkDirtyLayout();
            }
        }


        private void OnClickedEndFrame()
        {
            SelectFrame = TotalFrame - 1;
            if (TrackContainerView.horizontalScroller.highValue > 0)
            {
                TrackContainerView.horizontalScroller.value = TrackContainerView.horizontalScroller.highValue;
            }
        }


        private void OnClickedNextFrame()
        {
            SelectFrame++;
        }


        private void OnClickedPlayOrStop()
        {
            //TODO 播放或停止
        }


        private void OnClickedPreviousFrame()
        {
            SelectFrame--;
        }


        private void OnClickedStartFrame()
        {
            SelectFrame = 0;
            TrackContainerView.horizontalScroller.value = TrackContainerView.horizontalScroller.lowValue;
        }

        #endregion

        #region SkillConfig
        private SkillConfig skillConfig;
        private SkillEditorConfig skillEditorConfig;

        private int SelectFrame
        {
            get => skillEditorConfig.selectFrame;

            set
            {
                if (value == skillEditorConfig.selectFrame) return;
                if (value < 0 || value >= TotalFrame) return;
                skillEditorConfig.selectFrame = value;
                SelectionFrameField.value = value + 1;
                SelectLine.MarkDirtyLayout();
            }

        }

        private int TotalFrame
        {
            get
            {
                if (skillConfig == null)
                {
                    return -1;
                }
                else
                {
                    return skillConfig.FrameCount;
                }
            }

            set
            {
                if (skillConfig != null && value > 0)
                {
                    //TODO 对于帧数变少做出警告
                    skillConfig.FrameCount = value;
                    UpdateTrackViewSize();
                }
            }
        }


        private void SetSkillConfig(SkillConfig skillConfig)
        {
            this.skillConfig = skillConfig;

            if (skillConfig != null)
            {
                TotalFrameField.value = skillConfig.FrameCount;
            }

            SelectFrame = 0;
            TrackContainerView.horizontalScroller.value = 0;
            TrackContainerView.verticalScroller.value = 0;

            TimeShaft.MarkDirtyLayout();
            SelectLine.MarkDirtyLayout();

            UpdateTrack();
        }

        #endregion

        #region Preview
        private void ShowPreviewCharacter()
        {
            GameObject previewCharacterRoot = GameObject.Find(previewCharacterRootPath);

            if (previewCharacterRoot != null)
            {
                while (previewCharacterRoot.transform.childCount > 0)
                {
                    DestroyImmediate(previewCharacterRoot.transform.GetChild(0).gameObject);
                }
            }
            else
            {
                previewCharacterRoot = new GameObject(previewCharacterRootPath);
            }

            if (previewPrefab != null)
            {
                Instantiate(previewPrefab, previewCharacterRoot.transform);
            }

        }

        #endregion

        #region Utility

        public int GetFrameIndexByMousePos(Vector2 mousePos)
        {
            float localPos = mousePos.x + contentOffsetX;
            int index = Mathf.FloorToInt(localPos / skillEditorConfig.frameUnitWidth);

            if (index > TotalFrame)
            {
                return -1;
            }

            return index;
        }
        #endregion
    }


    public class SkillEditorConfig
    {
        public int standFrameUnitWidth = 2;
        public int maxFrameUnitWidth = 100;
        public int frameUnitWidth = 10;
        public int selectFrame = 1;
        public bool selectLineDragging = false;
    }
}