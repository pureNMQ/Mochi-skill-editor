using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using System.Collections.Generic;
using MochiFramework.Skill.MochiFramework.SkillEditor.Editor;

namespace MochiFramework.Skill.Editor
{
    public sealed class SkillEditor : EditorWindow
    {
        private VisualElement root;
        private const string iconPath = "Assets/MochiFramework/SkillEditor/Editor/Icon/Icon.png";
        public bool IsPreview => EditorSceneManager.GetActiveScene().path == previewScenePath;

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
            CreateSkillPreviewPlayer();
            _skillPreviewPlayer.ShowPreviewCharacter();
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
        public GameObject PreviewPrefab => previewPrefab;
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
                CreateSkillPreviewPlayer();
                _skillPreviewPlayer.ShowPreviewCharacter();
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
            _skillPreviewPlayer.ShowPreviewCharacter();
        }


        private void OnSkillConfigValueChanged(ChangeEvent<UnityEngine.Object> evt)
        {
            SetSkillConfig(evt.newValue as SkillConfig);
        }
        #endregion

        #region TimeShaft
        private IMGUIContainer TimeShaft;
        private IMGUIContainer SelectLine;

        // private Label SelectFrameTip;
        // private Label MoveClipStartTip;
        // private Label MoveClipEndTip;
        
        /// <summary>
        /// 轨道容器容器的X位置（局部坐标）
        /// </summary>
        /// <returns></returns>
        private float contentOffsetX => Mathf.Abs(HorizontalScroller.value);

        private void InitTimeShaft()
        {
            TimeShaft = root.Q<IMGUIContainer>(nameof(TimeShaft));
            SelectLine = root.Q<IMGUIContainer>(nameof(SelectLine));
            
            // SelectFrameTip = root.Q<Label>(nameof(SelectFrameTip));
            // MoveClipStartTip = root.Q<Label>(nameof(MoveClipStartTip));
            // MoveClipEndTip = root.Q<Label>(nameof(MoveClipEndTip));

            TimeShaft.onGUIHandler += DrawTimeShaft;
            SelectLine.onGUIHandler += DrawSelectLine;
            
            TimeShaft.RegisterCallback<MouseMoveEvent>(OnMouseMoveTimeShaft);
            TimeShaft.RegisterCallback<MouseDownEvent>(OnMouseDownTimeShaft);
            TimeShaft.RegisterCallback<MouseUpEvent>(OnMouseUpTimeShaft);
            TimeShaft.RegisterCallback<MouseOutEvent>(OnMouseOutTimeShaft);


            // SelectFrameTip.style.display = DisplayStyle.None;
            // MoveClipStartTip.style.display = DisplayStyle.None;
            // MoveClipEndTip.style.display = DisplayStyle.None;
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
        
        private void DrawSelectLine()
        {
            if (skillConfig == null) return;
            
            Handles.BeginGUI();
            Handles.color = Color.white;
            float posX = SelectFrame * skillEditorConfig.frameUnitWidth - contentOffsetX;
            float sizeY = TimeShaft.contentRect.size.y + TrackContainerMask.contentRect.size.y;

            Vector2 pos = default;
            Vector2 size = default;
            Rect rect = default;
            
            if (posX >= TrackContainerMask.contentRect.width)
            {
                pos = default;
                size = default;
            }
            else if(posX + skillEditorConfig.frameUnitWidth > TrackContainerMask.contentRect.width)
            {
                pos = new Vector2(posX, 0);
                size = new Vector2(TrackContainerMask.contentRect.width - posX, sizeY);
            }
            else if (posX >= 0)
            {
                pos = new Vector2(posX, 0);
                size = new Vector2(skillEditorConfig.frameUnitWidth, sizeY);
            }
            else if (posX + skillEditorConfig.frameUnitWidth > 0)
            {
                pos = new Vector2(0, 0);
                size = new Vector2(skillEditorConfig.frameUnitWidth + posX, sizeY);
            }
            
            rect = new Rect(pos, size);
            Handles.DrawSolidRectangleWithOutline(rect, new Color(1, 1, 1, 0.1f), new Color(1, 1, 1, 0.1f));
            Handles.EndGUI();
            if (skillEditorConfig.selectLineDragging)
            {
                DrawFrameTip(SelectFrame,$"{SelectFrame}",new Color(0,0,0,0.5f),Color.white);
            }

            
        }

        private void DrawTimeShaft()
        {
            Handles.BeginGUI();

            Handles.color = Color.white;
            Rect rect = TimeShaft.contentRect;

            int index = Mathf.CeilToInt(HorizontalScroller.value / skillEditorConfig.frameUnitWidth);

            //绘制的第一帧的起始位置
            float startOffset = 0;
            if (index > 0)
            {
                startOffset = HorizontalScroller.value % skillEditorConfig.frameUnitWidth;
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
                    Handles.Label(new Vector3(x - indexStr.Length * 3f, rect.height - 25), indexStr);
                }
                else if (index % SmallStep == 0)
                {
                    Handles.DrawLine(new Vector3(x, rect.height - 10), new Vector3(x, rect.height));

                    if (skillEditorConfig.frameUnitWidth > 6)
                    {
                        string indexStr = index.ToString();
                        Handles.Label(new Vector3(x - indexStr.Length * 3f, rect.height - 20), indexStr);
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

        private void DrawFrameTip(int frame,string text,Color backgroundColor,Color textColor)
        {
            Handles.BeginGUI();
            Handles.color =textColor;
            float textOffset = text.Length * 3f;
            float posX = frame * skillEditorConfig.frameUnitWidth - contentOffsetX;
            Vector3 labelPos = new Vector3(posX + skillEditorConfig.frameUnitWidth / 2f - textOffset, TimeShaft.contentRect.size.y / 2);
                
            Rect backgroundRect = new Rect(new Vector2(labelPos.x - 2,labelPos.y - 10), new Vector2(text.Length * 6 + 5, 20));
            Handles.DrawSolidRectangleWithOutline(backgroundRect,backgroundColor, backgroundColor);
            Handles.Label(labelPos, text);
        }

        #endregion

        #region TrackView
        private VisualElement Content;
        private VisualElement TrackContainerMask;
        private VisualElement TrackContainer;
        private VisualElement TrackMenuContainer;
        private Scroller VerticalScroller;
        private Scroller HorizontalScroller;
        private List<TrackView> trackViews;

        private void InitTrackView()
        {
            Content = root.Q<VisualElement>(nameof(Content));
            TrackContainer = root.Q<VisualElement>(nameof(TrackContainer));
            TrackMenuContainer = root.Q<VisualElement>(nameof(TrackMenuContainer));
            TrackContainerMask = root.Q<VisualElement>(nameof(TrackContainerMask));
            VerticalScroller = root.Q<Scroller>(nameof(VerticalScroller));
            HorizontalScroller = root.Q<Scroller>(nameof(HorizontalScroller));
            trackViews = new List<TrackView>();
            //NOTE 此处注册两个是事件是由于Scroller的样式由一对父子视窗元素决定，两者其中之一发生变化Scroller就需要修改
            TrackContainerMask.RegisterCallback<GeometryChangedEvent>(OnTrackContainerChanged);
            TrackContainer.RegisterCallback<GeometryChangedEvent>(OnTrackContainerChanged);
            
            //TODO 鼠标操作事件
            Content.RegisterCallback<WheelEvent>(OnWheelContent);
            
            
            VerticalScroller.valueChanged += OnVerticalScrollerChange;
            HorizontalScroller.valueChanged += OnHorizontalScrollerChange;
        }
        
        private void OnTrackContainerChanged(GeometryChangedEvent evt)
        {
            float viewWidth = TrackContainerMask.contentRect.width;
            float viewHeight = TrackContainerMask.contentRect.height;
            
            float contentWidth = TrackContainer.contentRect.width + 1;
            float contentHeight = TrackContainer.contentRect.height + 40;
            
            HorizontalScroller.highValue = Mathf.Max(0,contentWidth - viewWidth);
            VerticalScroller.highValue = Mathf.Max(0,contentHeight - viewHeight);
            
            HorizontalScroller.Adjust(viewWidth/contentWidth);
            VerticalScroller.Adjust(viewHeight/contentHeight);
        }
        
        private void OnWheelContent(WheelEvent evt)
        {
            if (evt.ctrlKey)
            {
                WheelVerticalMove(evt.delta.y);
            }
            else if(evt.shiftKey)
            {
                WheelHorizontalMove(evt.delta.x);
            }
            else
            {
                WheelScale(evt.delta.y);    
            }
        }

        private void WheelHorizontalMove(float delta)
        {
            //轨道的水平轴移动，根据帧单位大小进行缩放，方便操作
            HorizontalScroller.value += delta * skillEditorConfig.frameUnitWidth;
        }

        private void WheelVerticalMove(float delta)
        {
            VerticalScroller.value += delta;
        }

        private void WheelScale(float delta)
        {
            int sign = (int)Mathf.Sign(delta) * 2;
            float rePos = contentOffsetX / skillEditorConfig.frameUnitWidth;
            skillEditorConfig.frameUnitWidth = Mathf.Clamp(skillEditorConfig.frameUnitWidth - sign, skillEditorConfig.standFrameUnitWidth, skillEditorConfig.maxFrameUnitWidth);
            HorizontalScroller.value = rePos * skillEditorConfig.frameUnitWidth;
            UpdateTrackViewSize();
            UpdateTrack(false);
            TimeShaft.MarkDirtyLayout();
            SelectLine.MarkDirtyLayout();
        }

        private void OnHorizontalScrollerChange(float value)
        {
            Vector3 position = TrackContainer.transform.position;
            position.x = -value;
            TrackContainer.transform.position = position;
        }

        private void OnVerticalScrollerChange(float value)
        {
            Vector3 position = TrackMenuContainer.transform.position;
            position.y = -value;
            TrackMenuContainer.transform.position = position;
            
            position = TrackContainer.transform.position;
            position.y = -value;
            TrackContainer.transform.position = position;
        }
        
        private void UpdateTrackViewSize()
        {
            TrackContainer.style.width = TotalFrame * skillEditorConfig.frameUnitWidth;
        }
        
        /// <summary>
        /// 当Track数量发生变化或者更换技能配置文件时，请将isClear设为true
        /// </summary>
        /// <param name="isClear"></param>
        public void UpdateTrack(bool isClear = true,object changeObject = null)
        {
            if (isClear || skillConfig is null)
            {
                ClearTrack();
                if (skillConfig == null) return;
                foreach (var track in skillConfig.tracks)
                {
                    TrackView tv = new TrackView(track, TrackMenuContainer, TrackContainer, this);
                    trackViews.Add(tv);
                    tv.Redraw(skillEditorConfig.frameUnitWidth,isClear,changeObject);
                }
            }
            else if(trackViews is not null)
            {
                foreach (var tv in trackViews)
                {
                    tv.Redraw(skillEditorConfig.frameUnitWidth,isClear,changeObject);
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
            //NOTE: 容器宽度被撑开（垂直排列）后，不会自动缩回
            TrackContainer.style.width = 0;
        }

        #endregion

        #region Console
        
        private Button AddTrackButton;
        
        private Button StartFrameButton;
        private Button PreviousFrameButton;
        private Button PlayOrStopButton;
        private Button NextFrameButton;
        private Button EndFrameButton;
        
        private IntegerField SelectionFrameField;
        private IntegerField TotalFrameField;
        

        private void InitController()
        {
            AddTrackButton = root.Q<Button>(nameof(AddTrackButton));
            StartFrameButton = root.Q<Button>(nameof(StartFrameButton));
            PreviousFrameButton = root.Q<Button>(nameof(PreviousFrameButton));
            PlayOrStopButton = root.Q<Button>(nameof(PlayOrStopButton));
            NextFrameButton = root.Q<Button>(nameof(NextFrameButton));
            EndFrameButton = root.Q<Button>(nameof(EndFrameButton));

            SelectionFrameField = root.Q<IntegerField>(nameof(SelectionFrameField));
            TotalFrameField = root.Q<IntegerField>(nameof(TotalFrameField));
            
            AddTrackButton.clicked += OnClickedAddTrack;
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
                UpdateTrack(false);
                TimeShaft.MarkDirtyLayout();
            }
        }

        private void OnSelectionFrameFieldChange(ChangeEvent<int> evt)
        {
            if (evt.newValue < 0)
            {
                SelectionFrameField.value = evt.previousValue;
            }
            else
            {
                skillEditorConfig.selectFrame = evt.newValue;
                SelectLine.MarkDirtyLayout();
            }
        }


        private void OnClickedAddTrack()
        {
            if(SkillConfig == null) return;
            GenericMenu menu = new GenericMenu();
            //遍历所有程序集中的Track子类
            foreach (var type in TypeCache.GetTypesDerivedFrom<Track>())
            {
                menu.AddItem(new GUIContent(type.Name), false,
                    () =>
                    {
                        if(skillConfig == null) return;
                        if (skillConfig.tracks == null)
                        {
                            skillConfig.tracks = new List<Track>();
                        }
                        skillConfig.tracks.Add(TrackBuilder.Build(type,skillConfig));
                        UpdateTrack(true);
                    });
            }
            menu.ShowAsContext();
        }

        private void OnClickedEndFrame()
        {
            SelectFrame = TotalFrame - 1;
            if (HorizontalScroller.highValue > 0)
            {
                HorizontalScroller.value = HorizontalScroller.highValue;
            }
        }


        private void OnClickedNextFrame()
        {
            PreviewStop();
            SelectFrame++;
            
        }


        private void OnClickedPlayOrStop()
        {
            _skillPreviewPlayer.PlayOrPause();
        }


        private void OnClickedPreviousFrame()
        {
            PreviewStop();
            SelectFrame--;
        }


        private void OnClickedStartFrame()
        {
            SelectFrame = 0;
            HorizontalScroller.value = HorizontalScroller.lowValue;
        }

        #endregion

        #region SkillConfig

        public SkillConfig SkillConfig => skillConfig;
        private SkillConfig skillConfig;
        private SkillEditorConfig skillEditorConfig;

        public int SelectFrame
        {
            get => skillEditorConfig.selectFrame;

            set
            {
                if (value == skillEditorConfig.selectFrame) return;
                if (value < 0 || value >= TotalFrame) return;
                skillEditorConfig.selectFrame = value;
                SelectionFrameField.value = value;
                SelectLine.MarkDirtyLayout();
                
                if (!_skillPreviewPlayer.IsPlaying)
                {
                    _skillPreviewPlayer.Evaluate(value);
                }
            }

        }

        private int TotalFrame
        {
            get
            {
                if (skillConfig == null)
                {
                    return 0;
                }
                else
                {
                    return skillConfig.frameCount;
                }
            }

            set
            {
                if (skillConfig != null && value > 0)
                {
                    skillConfig.frameCount = value;
                    UpdateTrackViewSize();
                }
            }
        }


        public void SetSkillConfig(SkillConfig skillConfig)
        {
            if (this.skillConfig != null)
            {
                this.skillConfig.onValidateAction -= RedrawEditor;
            }

            this.skillConfig = skillConfig;

            if (skillConfig != null)
            {
                if (this.skillConfig.tracks == null ||this.skillConfig.tracks.Count == 0)
                {
                    //TODO 修改为更方便的初始化skillConfig方式
                    // AnimationTrack animationTrack = AnimationTrack.CreateAnimationTrack(skillConfig);
                    // skillConfig.tracks = new List<Track>();
                    // skillConfig.tracks.Add(animationTrack);
                    //Debug.Log("重新构造动画轨道");
                    // AssetDatabase.SaveAssets();
                    // AssetDatabase.Refresh();
                }

                this.skillConfig.onValidateAction += RedrawEditor;
                _skillPreviewPlayer.Rebuild();
            }
            else
            {
                TotalFrameField.SetValueWithoutNotify(0);
                SelectionFrameField.SetValueWithoutNotify(0);
            }

            SelectFrame = 0;
            HorizontalScroller.value = 0;
            VerticalScroller.value = 0;
            
            RedrawEditor();
        }

        #endregion

        #region Preview
        
        private SkillPreviewPlayer _skillPreviewPlayer;

        private void CreateSkillPreviewPlayer()
        {
            _skillPreviewPlayer = new SkillPreviewPlayer(this);
            _skillPreviewPlayer.OnPlay += () => PlayOrStopButton.AddToClassList("playing");
            _skillPreviewPlayer.OnPause += () => PlayOrStopButton.RemoveFromClassList("playing");
            _skillPreviewPlayer.OnStop += () => PlayOrStopButton.RemoveFromClassList("playing");
        }
        

        private void PreviewStop()
        {
            _skillPreviewPlayer.StopCurrentSkill();
        }
        
        private void Update()
        {
            if (!IsPreview)
            {
                PreviewStop();
            }
            
            _skillPreviewPlayer.Update();

            if (_skillPreviewPlayer.IsPlaying)
            {
                SelectFrame = _skillPreviewPlayer.CurrentFrame;
            }
        }

        #endregion

        #region Utility

        public int GetFrameIndexByMousePos(Vector2 mousePos)
        {
            float localPos = TrackContainerMask.WorldToLocal(mousePos).x + contentOffsetX;
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
                TotalFrameField.value = skillConfig.frameCount;
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