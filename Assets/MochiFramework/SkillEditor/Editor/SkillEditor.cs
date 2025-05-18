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
            root.styleSheets.Add(styleSheet);
            
            skillEditorConfig = new SkillEditorConfig();
            
            InitTopMenu();
            InitTimeShaft();
            InitTrackView();
            InitController();

            Undo.undoRedoEvent += OnUndoRedo;
            skillConfig = null;
            ShowPreviewCharacter();
        }

        private void OnUndoRedo(in UndoRedoInfo undo)
        {
            Debug.Log("接受到撤回消息:" + undo.undoName);
            switch (undo.undoName)
            {
                case "Move Clip":
                case "Insert Clip":
                case "Delete Clip":
                    UpdateTrack();
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    break;
            }
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
            PreviewPrefabField.RegisterValueChangedCallback(OnPreviewPrefabValueChanged);

            SkillConfigField.objectType = typeof(SkillConfig);
            SkillConfigField.RegisterValueChangedCallback(OnSkillConfigValueChanged);
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

        private void OnClickGameSceneButton()
        {
            if (!IsPreview || string.IsNullOrEmpty(lastGameScenePath)) return;

            EditorSceneManager.OpenScene(lastGameScenePath);
        }

        private void OnClickSkillInfoButton()
        {
            if(skillConfig is null) return;
            
            Selection.activeObject = skillConfig;
        }

        private void OnPreviewPrefabValueChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            previewPrefab = evt.newValue as GameObject;

            ShowPreviewCharacter();
        }


        private void OnSkillConfigValueChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            SetSkillConfig(evt.newValue as SkillConfig);
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
            SelectFrame = GetFrameIndexByMousePos(evt.mousePosition);
            
            //预览相关
            PreviewStop();
            Debug.Log("预览暂停");
        }

        private void OnMouseUpTimeShaft(MouseUpEvent evt)
        {
            skillEditorConfig.selectLineDragging = false;
        }
        private void OnMouseMoveTimeShaft(MouseMoveEvent evt)
        {
            if (skillEditorConfig.selectLineDragging)
            {
                SelectFrame = GetFrameIndexByMousePos(evt.mousePosition);
            }
        }

        private void OnWheelTimeShaft(WheelEvent evt)
        {
            int delta = (int)Mathf.Sign(evt.delta.y) * 2;

            float rePos = contentOffsetX / skillEditorConfig.frameUnitWidth;

            skillEditorConfig.frameUnitWidth = Mathf.Clamp(skillEditorConfig.frameUnitWidth - delta, skillEditorConfig.standFrameUnitWidth, skillEditorConfig.maxFrameUnitWidth);

            TrackContainerView.horizontalScroller.value = rePos * skillEditorConfig.frameUnitWidth;

            UpdateTrackViewSize();
            UpdateTrack(false);
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
        
        /// <summary>
        /// 当Track数量发生变化或者更换技能配置文件时，请将isClear设为true
        /// </summary>
        /// <param name="isClear"></param>
        public void UpdateTrack(bool isClear = true)
        {
            if (isClear || skillConfig is null)
            {
                ClearTrack();
                if (skillConfig == null) return;
                foreach (var track in skillConfig.tracks)
                {
                    TrackView tv = new TrackView(track, TrackMenuContainer, ClipTrackContainer, this);
                    trackViews.Add(tv);
                    tv.Update(skillEditorConfig.frameUnitWidth);
                }
            }
            else if(trackViews is not null)
            {
                foreach (var tv in trackViews)
                {
                    tv.Update(skillEditorConfig.frameUnitWidth);
                }
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

        #region Console

        private Button StartFrameButton;
        private Button PreviousFrameButton;
        private Button PlayOrStopButton;
        private Button NextFrameButton;
        private Button EndFrameButton;
        
        private IntegerField SelectionFrameField;
        private IntegerField TotalFrameField;
        
        private VisualElement PlayOrStopIcon;

        private void InitController()
        {
            StartFrameButton = root.Q<Button>(nameof(StartFrameButton));
            PreviousFrameButton = root.Q<Button>(nameof(PreviousFrameButton));
            PlayOrStopButton = root.Q<Button>(nameof(PlayOrStopButton));
            NextFrameButton = root.Q<Button>(nameof(NextFrameButton));
            EndFrameButton = root.Q<Button>(nameof(EndFrameButton));

            SelectionFrameField = root.Q<IntegerField>(nameof(SelectionFrameField));
            TotalFrameField = root.Q<IntegerField>(nameof(TotalFrameField));
            
            PlayOrStopIcon = PlayOrStopButton.Q<VisualElement>(nameof(PlayOrStopIcon));

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
            if (isPlay && !isPause)
            {
                PreviewPause();
            }
            else
            {
                PreviewPlay();    
            }
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

        public SkillConfig SkillConfig => skillConfig;
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
                
                if (IsPreview && !isPlay && _skillPreviewPlayer != null)
                {
                    _skillPreviewPlayer.PreviewUpdate(SelectFrame * skillConfig.frameTime,SelectFrame,false);
                }
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
                if (this.skillConfig.tracks == null ||this.skillConfig.tracks.Count == 0)
                {
                    //TODO 修改为更方便的初始化skillConfig方式
                    AnimationTrack animationTrack = AnimationTrack.CreateAnimationTrack(skillConfig);
                    skillConfig.tracks = new List<Track>();
                    skillConfig.tracks.Add(animationTrack);
                    Debug.Log("重新构造动画轨道");
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            SelectFrame = 0;
            TrackContainerView.horizontalScroller.value = 0;
            TrackContainerView.verticalScroller.value = 0;

            if (_skillPreviewPlayer != null)
            {
                _skillPreviewPlayer.SetSkillConfig(skillConfig);
            }

            RedrawEditor();
        }

        #endregion

        #region Preview
        private double lastTime = 0;
        private double detleTime = 0;

        private double previewPlayTime = 0;
        private bool isPlay = false;
        private bool isPause;
        
        private SkillPreviewPlayer _skillPreviewPlayer;
        private void ShowPreviewCharacter()
        {
            if (!IsPreview) return;
            
            //GameObject previewCharacterRoot = GameObject.Find(previewCharacterRootPath);
            _skillPreviewPlayer = FindObjectOfType<SkillPreviewPlayer>();
            if (_skillPreviewPlayer is null)
            {
                _skillPreviewPlayer = new GameObject("PreviewObject").AddComponent<SkillPreviewPlayer>();
            }
            
           _skillPreviewPlayer.ShowPreviewCharacter(previewPrefab);
           _skillPreviewPlayer.SetSkillConfig(skillConfig);
        }

        private void PreviewPlay()
        {
            if(skillConfig is null) return;
            
            if (isPause && isPlay)
            {
                isPause = false;
            }
            else
            {
                previewPlayTime = SelectFrame * skillConfig.frameTime;
                isPlay = true;
                isPause = false;
            }
            
            PlayOrStopIcon.AddToClassList("playing");
        }

        private void PreviewPause()
        {
            if (isPlay)
            {
                isPause = true;
                PlayOrStopIcon.RemoveFromClassList("playing");
            }
        }

        private void PreviewStop()
        {
            if (isPlay)
            {
                _skillPreviewPlayer.PreviewStop();
            }
            isPause = false;
            isPlay = false;
            PlayOrStopIcon.RemoveFromClassList("playing");
        }
        
        private void Update()
        {
            if (!IsPreview)
            {
                PreviewStop();
            }
            
            detleTime = EditorApplication.timeSinceStartup - lastTime;
            lastTime = EditorApplication.timeSinceStartup;

            if (isPlay && !isPause)
            {
                if (skillConfig is null)
                {
                    PreviewStop();
                    return;
                }
                previewPlayTime += detleTime;

                if (previewPlayTime >= skillConfig.totalTime)
                {
                    PreviewStop();
                }
                
                SelectFrame = Convert.ToInt32(previewPlayTime * skillConfig.frameRate);
                Debug.Log($"播放帧:{SelectFrame}");
                _skillPreviewPlayer.PreviewUpdate((float)previewPlayTime,SelectFrame);
            }
        }

        #endregion

        #region Utility

        public int GetFrameIndexByMousePos(Vector2 mousePos)
        {
            float localPos = TrackContainerView.WorldToLocal(mousePos).x + contentOffsetX;
            int index = Mathf.FloorToInt(localPos / skillEditorConfig.frameUnitWidth);

            if (index > TotalFrame)
            {
                return TotalFrame - 1;
            }

            return index;
        }


        public void RedrawEditor()
        {
            if (skillConfig != null)
            {
                if (SkillConfigField.value != skillConfig)
                {
                    SkillConfigField.SetValueWithoutNotify(skillConfig);
                }
                
                TotalFrameField.value = skillConfig.FrameCount;
            }
            
            TimeShaft.MarkDirtyRepaint();
            SelectLine.MarkDirtyRepaint();
            UpdateTrack();
        }

        private AgencyInspectorObject _agencyInspectorObject;
        public void ShowObjectOnInspector(object target)
        {
            _agencyInspectorObject = CreateInstance<AgencyInspectorObject>();
            _agencyInspectorObject.target = target;
            Selection.activeObject = _agencyInspectorObject;
        }

        public void UpdateInspector()
        {
            //TODO SetDirty()没有效果,目前是重新创建一个_agencyInspectorObject
            
            //EditorUtility.SetDirty(_agencyInspectorObject);
            if (_agencyInspectorObject != null)
            {
                ShowObjectOnInspector(_agencyInspectorObject.target);
            }
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