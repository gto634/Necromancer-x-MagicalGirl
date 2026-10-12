#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace ALTUSHKA.EasyNodeDialogSystem.Editor
{

    public partial class DialogEditorWindow : EditorWindow
    {
        #region Constants

        // Colors
        private static readonly Color LINE_COLOR_1 = new Color(0.9f, 0.9f, 0.9f, 0.3f);
        private static readonly Color LINE_COLOR_2 = new Color(0.85f, 0.85f, 0.85f, 0.3f);
        private static readonly Color CATEGORY_BG = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        private static readonly Color ORANGE_TOOLBAR = new Color(0.96f, 0.6f, 0.3f);
        private static readonly Color TEXT_AREA_BG = new Color(1f, 1f, 0.95f);
        private static readonly Color GRID_BG = new Color(0.2f, 0.2f, 0.2f, 0.5f);
        private static readonly Color GRID_LINE = new Color(0.5f, 0.5f, 0.5f, 0.8f);
        private static readonly Color CONDITION_BOX_BG = new Color(0.9f, 0.9f, 0.9f);
        private static readonly Color DELETE_BUTTON_COLOR = new Color(1f, 0.7f, 0.7f);
        private static readonly Color INSTANT_LINE_COLOR = new Color(1f, 0.9f, 0.3f);
        private static readonly Color DEFAULT_LINE_COLOR = new Color(0.7f, 0.7f, 0.7f);
        private static readonly Color CONDITION_TRUE_COLOR = new Color(0.1f, 0.5f, 0.1f);
        private static readonly Color CONDITION_FALSE_COLOR = new Color(0.5f, 0.1f, 0.1f);
        private static readonly Color START_LABEL_COLOR = new Color(0.23f, 0.18f, 0.17f);
        private static readonly Color HINT_TEXT_COLOR = new Color(0.9f, 0.9f, 0.9f);
        private static readonly Color HINT_COPY_TEXT_COLOR = new Color(0.85f, 0.95f, 0.85f);

        // Graph canvas
        private const int GRID_TILE_SIZE = 20;
        private const float DEFAULT_ZOOM_SCALE = 1f;
        private const float MIN_NODE_HEIGHT = 40f;
        private const float ZOOM_SENSITIVITY = 0.05f;
        private const float ZOOM_MIN = 0.1f;
        private const float ZOOM_MAX = 2.95f;
        private const float ZOOM_EPSILON = 0.001f;
        private const int BEZIER_OFFSET = 50;
        private const float BEZIER_THICKNESS = 2f;
        private const int SELECTION_FRAME_PADDING = 10;
        private const float BOX_SELECT_MIN_DRAG = 4f;
        private const int DEFAULT_POSITION_XY = 100;
        private const int DUPLICATE_NODE_OFFSET = 50;

        // Shared node chrome
        private const int NODE_TOP_PADDING = 22;
        private const int NODE_LEFT_RIGHT_PADDING = 10;
        private const int NODE_BOTTOM_PADDING = 5;
        private const int NODE_PORT_SIZE = 40;
        private const int PORT_HALF_SIZE = NODE_PORT_SIZE / 2;
        private const int BASE_NODE_WIDTH = 450;
        private const int NODE_SPACE_OFFSET = 5;
        private const int NODE_TOOLBAR_HEIGHT = 22;
        private const int NODE_BUTTON_SIZE = 27;
        private const int NODE_ID_DISPLAY_LENGTH = 8;
        private const int HEADER_FONT_SIZE = 14;
        private const int BASE_FONT_SIZE = 12;
        private const int CATEGORY_FONT_SIZE = 12;
        private const int FIELD_ROW_HEIGHT = 20;
        private const int TITLE_LABEL_OFFSET = 25;
        private const int TITLE_LABEL_WIDTH_ZOOM = 200;
        private const int FOLDOUT_ARROW_WIDTH = 20;
        private const int ACTION_BUTTON_SIZE = 22;
        private const int ACTION_BUTTON_GAP = 2;
        private const string MISSING_WARNING_EMOJI_PREFIX = "⚠ ";
        private const int HINT_EXTRA_WIDTH = 16;

        // Start node
        private const int START_NODE_WIDTH = 100;
        private const int START_NODE_HEIGHT = 45;
        private const int INPUT_OFFSET_START = 50;

        // Line node
        private const int INPUT_OFFSET_LINE = 200;
        private const int LINE_NODE_BASE_HEIGHT = 27;
        private const int LINE_NODE_HEADER_OFFSET = 40;
        private const int LINE_NODE_ACTIONS_FOLDOUT = 25;
        private const int LINE_NODE_ACTION_ITEM_HEIGHT = 60;
        private const int LINE_NODE_ACTION_SHOW_EXTRA = 25;
        private const int LINE_NODE_LANG_FOLDOUT = 25;
        private const int LINE_NODE_TEXT_AREA_HEIGHT = 110;
        private const int LINE_NODE_AUDIO_ROW_HEIGHT = 25;
        private const int LINE_NODE_TOOLBAR_RIGHT_BUTTONS_COUNT = 2;
        private const int LINE_NODE_INSTANT_BUTTON_SPACING_COUNT = 4;
        private const int SPRITE_BUTTON_WIDTH = 90;
        private const int LINE_TEXT_AREA_HEIGHT = 80;
        private const int LINE_CHARACTER_LABEL_WIDTH = 105;
        private const int LINE_CHARACTER_POPUP_WIDTH = 200;
        private const int LINE_CHARACTER_ROW_HEIGHT = 21;
        private const int LINE_SAYER_LABEL_WIDTH = 120;
        private const int LINE_SAYER_LABEL_SPACING = 8;
        private const int LINE_SAYER_FIELD_WIDTH = 150;
        private const int LINE_SAYER_FIELD_SPACING = 8;
        private const int LINE_VOICE_LABEL_WIDTH = 35;
        private const int LINE_VOICE_OBJECT_FIELD_WIDTH = 90;
        private const int LINE_VOICE_COMBINED_TIME_WIDTH = 94;
        private const int LINE_VOICE_PLAY_WIDTH = 22;
        private const int LINE_VOICE_FIELD_GAP = 4;
        private const int LINE_VOICE_OBJECT_TO_SCRUB_GAP = 8;
        private const int LINE_VOICE_SCRUBBER_MIN_WIDTH = 40;
        private const int LINE_VOICE_SCRUBBER_HEIGHT = 10;
        private const int SAYER_PREVIEW_X = LINE_SAYER_LABEL_WIDTH + LINE_SAYER_LABEL_SPACING + LINE_SAYER_FIELD_WIDTH + LINE_SAYER_FIELD_SPACING;
        private const int SAYER_PREVIEW_WIDTH = 100;
        private const int COLOR_PREVIEW_OFFSET = 315;
        private const int COLOR_PREVIEW_SIZE = 12;
        private const int ACTION_LABEL_WIDTH = 70;
        private const int ACTION_POPUP_WIDTH = 105;
        private const int ACTION_ZOOM_LABEL_X = 185;
        private const int ACTION_ZOOM_LABEL_WIDTH = 50;
        private const int ACTION_ZOOM_SLIDER_X = 230;
        private const int ACTION_XY_LABEL_WIDTH = 30;
        private const int ACTION_XY_OFFSET = 60;
        private const int ACTION_ROW_HEIGHT = 25;
        private const int ACTION_EXTRA_HEIGHT = 60;

        // Choice node
        private const int CHOICE_NODE_ENTER_SPACING = 22;
        private const int CHOICE_NODE_EXIT_SPACING = CHOICE_NODE_ENTER_SPACING + NODE_SPACE_OFFSET + PORT_HALF_SIZE;
        private const int CHOICE_HEADER_HEIGHT = 25;
        private const int CHOICE_LANG_FOLDOUT = 25;
        private const int CHOICE_TEXT_AREA_HEIGHT = 85;
        private const int CHOICE_EVENT_ITEM_HEIGHT = 27;
        private const int CHOICE_NODE_EMPTY_HEIGHT = 44;
        private const int CHOICE_ROW_LABEL_WIDTH = 110;
        private const int CHOICE_BUTTON_GAP = 5;
        private const int CHOICE_CLOSE_BUTTON_OFFSET = 35;
        private const int CHOICE_CONSEQUENCE_BUTTON_WIDTH = 95;
        private const int CHOICE_CONSEQUENCE_BUTTON_OFFSET = CHOICE_CLOSE_BUTTON_OFFSET + CHOICE_CONSEQUENCE_BUTTON_WIDTH + CHOICE_BUTTON_GAP;
        private const int CHOICE_CONDITION_BUTTON_WIDTH = 80;
        private const int CHOICE_CONDITION_BUTTON_OFFSET = CHOICE_CONSEQUENCE_BUTTON_OFFSET + CHOICE_CONDITION_BUTTON_WIDTH + CHOICE_BUTTON_GAP;
        private const int CHOICE_FOLDOUT_WIDTH = 180;
        private const int CHOICE_LOGIC_TOGGLE_SIZE = 20;
        private const int CHOICE_LOGIC_RIGHT_PADDING = 15;
        private const int CHOICE_LOGIC_LABEL_WIDTH = 140;
        private const int CHOICE_LOGIC_GAP = 5;
        private const int CHOICE_LOGIC_ROW_Y_OFFSET = 3;
        private const int CHOICE_ITEM_CLOSE_OFFSET = 25;
        private const float CHOICE_EVENT_FIELD_TOP_PADDING = 2.5f;
        private const int CHOICE_EVENT_TYPE_LABEL_WIDTH = 32;
        private const int CHOICE_EVENT_TYPE_POPUP_WIDTH = 45;
        private const int CHOICE_EVENT_NAME_LABEL_WIDTH = 38;
        private const int CHOICE_EVENT_VAR_COND_POPUP_WIDTH = 72;
        private const int CHOICE_EVENT_VAR_POPUP_WIDTH_CONS = 105;
        private const int CHOICE_EVENT_VAR_SPACING = 2;
        private const int CHOICE_EVENT_OP_POPUP_WIDTH = 135;
        private const int CHOICE_EVENT_OP_TO_VAL_GAP = 105;
        private const int CHOICE_EVENT_STANET_LABEL_WIDTH = 53;
        private const int CHOICE_EVENT_VALUE_FIELD_WIDTH = 70;
        private const int CHOICE_EVENT_ITEM_SPACING = 2;

        // Condition node + start-conditions panel
        private const int CONDITION_NODE_WIDTH = 380;
        private const int CONDITION_NODE_ENTER_SPACING = 22;
        private const int CONDITION_NODE_EXIT_SPACING = CONDITION_NODE_ENTER_SPACING + NODE_SPACE_OFFSET + PORT_HALF_SIZE;
        private const int CONDITION_NODE_CONDITION_HEIGHT = 130;
        private const int CONDITION_NODE_BASE_HEIGHT = 117;
        private const int CONDITION_NODE_FOLDOUT_OFFSET = 10;
        private const int CONDITION_ITEM_SPACING = 20;
        private const int CONDITIONS_BOX_PADDING = 2;
        private const int CONDITION_LABEL_X = 10;
        private const int CONDITION_TOP_PADDING = 5;
        private const int CONDITION_FIELD_Y_OFFSET = 30;
        private const int CONDITION_LABEL_WIDTH = 80;
        private const int CONDITION_FIELD_OFFSET = CONDITION_LABEL_X + CONDITION_LABEL_WIDTH;
        private const int CONDITION_FIELD_WIDTH_OFFSET = 100;
        private const int CONDITION_TITLE_LABEL_WIDTH = 100;
        private const int CONDITION_CLOSE_BUTTON_WIDTH = 25;
        private const int CONDITION_CLOSE_BUTTON_OFFSET = 35;
        private const int CONDITION_AND_SPACING = 20;
        private const int CONDITION_LOGIC_LABEL_X = 100;
        private const int CONDITION_LOGIC_LABEL_WIDTH = 140;
        private const int CONDITION_LOGIC_GAP = 5;
        private const int CONDITION_LOGIC_TOGGLE_X = CONDITION_LOGIC_LABEL_X + CONDITION_LOGIC_LABEL_WIDTH + CONDITION_LOGIC_GAP;
        private const int CONDITION_PORT_LABEL_OFFSET = 80;
        private const int CONDITION_PORT_LABEL_WIDTH = 40;
        private const int CONDITION_TRUE_PORT_Y = 15;
        private const int CONDITION_HEADER_OFFSET = 34;
        private const int CONDITION_TOGGLE_OFFSET = 35;

        // Redirect node
        private const int REDIRECT_NODE_ENTER_SPACING = 22;
        private const int REDIRECT_CONTENT_TOP_PADDING = 5;
        private const int REDIRECT_SECTION_GAP = 5;
        private const int REDIRECT_FIELD_GAP = 5;
        private const int REDIRECT_CONTENT_BOTTOM_PADDING = 3;
        private const int REDIRECT_HINT_LEFT_PADDING = 14;
        private const int REDIRECT_HINT_HEIGHT = 40;
        private const int REDIRECT_FIELD_ROW_HEIGHT = 22;
        private const int REDIRECT_CATEGORY_LABEL_WIDTH = 72;
        private const int REDIRECT_DIALOG_LABEL_WIDTH = 72;

        // Dialogs list tab
        private const int TOOLBAR_HEIGHT = 0;
        private const int ASSET_ACTION_ROW_HEIGHT = 30;
        private const int BUTTON_BACK_WIDTH = 130;
        private const int SEARCH_LABEL_WIDTH = 50;
        private const int CLEAR_BUTTON_WIDTH = 25;
        private const int DIALOG_ROW_HEIGHT = 26;
        private const int CATEGORY_HEADER_HEIGHT = 25;
        private const int DIALOG_ICON_SIZE = 20;
        private const int SPACE_AFTER_CATEGORY = 15;
        private const int INLINE_ASSET_LABEL_WIDTH = 100;
        private const int INLINE_ASSET_FIELD_WIDTH = 400;
        private const int INLINE_ASSET_SLOT_INDENT = 8;
        private const int INLINE_ASSET_NEW_BUTTON_WIDTH = 52;
        private const int INLINE_HEADER_SECTION_PADDING = 6;
        private const int INLINE_ASSET_CONTROL_HEIGHT = 18;
        private const int INLINE_ASSET_LOCKED_HINT_WIDTH = 14;
        private const string DIALOG_DATABASE_SWITCH_LOCKED_TOOLTIP = "Close the dialog graph with Back to list before switching Dialog Database.";

        // Graph editor header / toolbar
        private const int EDITOR_SPACE_SMALL = 5;
        private const int EDITOR_SPACE_MEDIUM = 10;
        private const int ADD_LINE_BUTTON_WIDTH = 130;
        private const int ADD_LINE_BUTTON_HEIGHT = 21;
        private const int LINES_HEADER_BUTTON_OFFSET = 2;

        // Validation
        private const int BROKEN_LINKS_MAX_SHOW = 15;
        private const int START_CONDITION_ISSUES_MAX_SHOW = 15;

        // Prefs / defaults / placeholders
        private const string PREFS_DIALOG_DATABASE_GUID_KEY = "DialogEditor.DialogDatabaseGuid";
        private const string PREFS_DIALOG_EDITOR_AUTO_SAVE_KEY = "DialogEditor.AutoSaveEnabled";
        private const string PREFS_DIALOG_EDITOR_AUTO_SAVE_MINUTES_KEY = "DialogEditor.AutoSaveMinutes";
        internal const string PREFS_DIALOG_EDITOR_SAVE_LOGS_KEY = "DialogEditor.SaveLogsEnabled";
        private const int SETTINGS_TOOLBAR_BUTTON_WIDTH = 28;
        private const int AUTO_SAVE_MIN_MINUTES = 1;
        private const int AUTO_SAVE_MAX_MINUTES = 10;
        private const int AUTO_SAVE_DEFAULT_MINUTES = 5;
        private const string NO_VARS_HINT = "-";
        private const string SELECT_VAR_PLACEHOLDER = "-";
        private const string NO_VARS_PLACEHOLDER = "-";
        private const string DIALOG_NAME_FIELD_CONTROL = "DialogEditorDialogNameField";
        private const string DEFAULT_DIALOG_BASE_NAME = "New Dialog";
        private const string DEFAULT_CATEGORY_NAME = "Unnamed";
        private static readonly int[] AUTO_SAVE_MINUTE_VALUES = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        private static readonly string[] AUTO_SAVE_MINUTE_LABELS = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10" };

        #endregion Constants


        #region Fields

        private DialogDatabase database;

        private Vector2 dialogListScrollPos;
        private Vector2 conditionsScrollPos;
        private Vector2 nameColorsScrollPos;

        private readonly DialogReferenceIndex _dialogReferenceIndex = new DialogReferenceIndex();
        private readonly DialogReferenceIndex _savedDialogReferenceIndex = new DialogReferenceIndex();
        private bool _dialogReferenceIndexStale = true;
        private string _dialogEditorDirtyBaseline = "";

        private bool _dialogEditorAutoSaveEnabled;
        private int _dialogEditorAutoSaveIntervalMinutes = AUTO_SAVE_DEFAULT_MINUTES;
        private double _lastDialogEditorAutoSaveTime = -1;
        private bool _dialogEditorSaveLogsEnabled = true;

        private string dialogSearchFilter = "";

        private List<string> dialogCategories
        {
            get
            {
                if (database == null || database.Categories == null) return new List<string>();
                return database.Categories;
            }
        }

        private List<string> availableLanguages => database.AvailableLanguages;

        private int currentMainTab = 0;
        private int dialogSubTab = 0;
        private string selectedDialogName = null;
        private string _pendingDialogNameEdit = "";
        private string _pendingDialogNameSelectedKey;
        private DialogEntry selectedDialog => GetSelectedDialog();
        private bool showConditions = false;
        private bool isEditingDialog = false;
        private bool isDatabasesLoaded = false;

        private GUIStyle dialogButtonStyle;
        private GUIStyle categoryStyle;
        private Texture2D lineBgTexture;
        private Color[] lineColors = new Color[] { LINE_COLOR_1, LINE_COLOR_2 };
        private bool stylesInitialized = false;

        private Vector2 editorOffset;
        private Vector2 dragStartPos;
        private bool isDragging;
        private Texture2D gridTexture;
        private float zoomScale = DEFAULT_ZOOM_SCALE;
        private Rect editorRect;

        private Texture2D nodeInactiveInputTexture;
        private Texture2D nodeInactiveOutputTexture;
        private Texture2D nodeActiveInputTexture;
        private Texture2D nodeActiveOutputTexture;

        private List<SelectedPort> selectedPorts = new List<SelectedPort>();

        private SelectedPort pendingPort = null;
        private bool isContextMenuOpen;
        private bool actionWasPerformed;

        private HashSet<DialogNode> selectedNodes = new HashSet<DialogNode>();
        private bool isBoxSelecting;
        private Vector2 boxSelectStart;
        private Vector2 boxSelectCurrent;
        private bool isDraggingSelection;
        private bool graphPortClickHandled;
        private int selectionDragControlId;
        private Vector2 selectionDragStartPos;
        private Dictionary<DialogNode, Vector2> selectionDragStartNodePositions;
        private Texture2D selectedNodeBgTexture;

        private Dictionary<string, bool> languageTextFoldouts = new Dictionary<string, bool>();
        private bool _voiceScrubKeepPlaying;
        private Dictionary<string, bool> actionFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> choiceLanguageFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> variableChangeFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> conditionFoldouts = new Dictionary<string, bool>();
        private Dictionary<string, bool> categoryFoldouts = new Dictionary<string, bool>();

        private static CharacterActionData copiedActionData = null;
        private static string copiedActionSourceNodeId = null;

        private class SelectedPort
        {
            public DialogNode Node { get; }
            public bool IsInput { get; }
            public int ChoiceIndex { get; }

            public SelectedPort(DialogNode node, bool isInput, int choiceIndex = -1)
            {
                Node = node;
                IsInput = isInput;
                ChoiceIndex = choiceIndex;
            }
        }

        private DialogEntry GetSelectedDialog()
        {
            if (string.IsNullOrEmpty(selectedDialogName) || database == null || database.Dialogs == null) return null;
            return database.Dialogs.FirstOrDefault(d => d != null && d.dialogName == selectedDialogName);
        }


        [MenuItem("Tools/Easy Node Dialog System")]
        public static void ShowWindow()
        {
            GetWindow<DialogEditorWindow>("Dialog Editor");
        }

        #endregion Fields


        #region Loading

        private void OnEnable()//might need to be done via Start
        {
            EditorApplication.update += DelayedSave;

            LoadDatabases();

            _dialogEditorAutoSaveEnabled = EditorPrefs.GetBool(PREFS_DIALOG_EDITOR_AUTO_SAVE_KEY, false);
            _dialogEditorAutoSaveIntervalMinutes = Mathf.Clamp(EditorPrefs.GetInt(PREFS_DIALOG_EDITOR_AUTO_SAVE_MINUTES_KEY, AUTO_SAVE_DEFAULT_MINUTES), AUTO_SAVE_MIN_MINUTES, AUTO_SAVE_MAX_MINUTES);
            ReloadDialogEditorPreferences();

            foreach (string category in dialogCategories)
            {
                if (!categoryFoldouts.ContainsKey(category)) categoryFoldouts[category] = true;
            }

            gridTexture = new Texture2D(GRID_TILE_SIZE, GRID_TILE_SIZE);
            Color backgroundColor = GRID_BG;
            Color lineColor = GRID_LINE;

            for (int x = 0; x < GRID_TILE_SIZE; x++)
            {
                for (int y = 0; y < GRID_TILE_SIZE; y++)
                {
                    if (x == 0 || y == 0) gridTexture.SetPixel(x, y, lineColor);        
                    else gridTexture.SetPixel(x, y, backgroundColor);         
                }
            }

            gridTexture.Apply();
            gridTexture.wrapMode = TextureWrapMode.Repeat;

            nodeInactiveInputTexture = Resources.Load<Texture2D>("Buttons/NodeInactiveInput");
            nodeInactiveOutputTexture = Resources.Load<Texture2D>("Buttons/NodeInactiveOutput");
            nodeActiveInputTexture = Resources.Load<Texture2D>("Buttons/NodeActiveInput");
            nodeActiveOutputTexture = Resources.Load<Texture2D>("Buttons/NodeActiveOutput");
    
            if (nodeInactiveInputTexture == null || nodeInactiveOutputTexture == null || nodeActiveInputTexture == null || nodeActiveOutputTexture == null)
            {
                Debug.LogError("One or more port textures not found in Resources!");
            }
        }

        private void OnDisable()
        {
            EditorApplication.update -= DelayedSave;

            if (isEditingDialog) SaveDialogEditorSession("Window close (graph session flush)");
            else if (database != null && EditorUtility.IsDirty(database)) SaveDialogGraphChanges("Window close (dirty asset flush)");

            ClearDialogEditorSessionTracking();
        }

        private void ReloadDialogEditorPreferences()
        {
            _dialogEditorSaveLogsEnabled = EditorPrefs.GetBool(PREFS_DIALOG_EDITOR_SAVE_LOGS_KEY, true);
        }

        private void InvalidateReferenceIndex()
        {
            _dialogReferenceIndexStale = true;
        }

        private DialogReferenceIndex GetDialogReferenceIndex()
        {
            if (_dialogReferenceIndexStale)
            {
                _dialogReferenceIndex.Rebuild(database);
                _dialogReferenceIndexStale = false;
            }

            return _dialogReferenceIndex;
        }

        private DialogReferenceIndex GetLiveDialogReferenceIndex()
        {
            _dialogReferenceIndex.Rebuild(database);
            _dialogReferenceIndexStale = false;
            return _dialogReferenceIndex;
        }

        private void RefreshSavedDialogReferenceIndex()
        {
            if (database == null)
            {
                _savedDialogReferenceIndex.Rebuild(null);
                return;
            }

            DialogDatabase savedSnapshot = Instantiate(database);
            try
            {
                _savedDialogReferenceIndex.Rebuild(savedSnapshot);
            }
            finally
            {
                DestroyImmediate(savedSnapshot);
            }
        }

        private string BuildRegistryDeleteUsageMessage(IReadOnlyList<DialogReferenceUsage> liveUsages, IReadOnlyList<DialogReferenceUsage> savedUsages, string footer)
        {
            return DialogReferenceIndex.BuildUsageSummary(liveUsages, savedUsages) + "\n\n" + footer;
        }

        private bool IsOrphanedVariableReference(string variableName, DialogReferenceState referenceState)
        {
            if (DialogReferenceUtility.IsMissing(referenceState)) return false;
            if (string.IsNullOrEmpty(variableName)) return false;
            if (database?.Variables == null) return true;

            return !database.Variables.Any(v => v != null && v.variableName == variableName);
        }

        private bool ShouldDrawMissingVariableSlot(DialogReferenceState referenceState, string variableName)
        {
            if (DialogReferenceUtility.IsMissing(referenceState)) return true;
            return IsOrphanedVariableReference(variableName, referenceState);
        }

        private static string GetMissingVariableDisplayLabel(DialogReferenceState referenceState, string variableName, string missingReferenceLabel)
        {
            if (DialogReferenceUtility.IsMissing(referenceState)) return DialogReferenceMessages.FormatMissingVariableLabel(missingReferenceLabel);
            return DialogReferenceMessages.FormatMissingVariableLabel(variableName);
        }

        private enum ConditionRowShellLayout
        {
            DialogStartPanel,
            GraphConditionNode,
            ChoiceConditionRow
        }

        private struct ConditionRowShellParams
        {
            public ConditionRowShellLayout Layout;
            public int Index;
            public DialogReferenceState ReferenceState;
            public string VariableName;
            public string MissingReferenceLabel;
            public float ContentWidth;
            public float RowBaseY;
            public float ZoomScale;
            public Rect WarningBoundsRect;
            public GUIStyle LabelStyle;
            public GUIStyle ButtonStyle;
            public bool RequireMouseButton;
            public Event CurrentEvent;
            public string DeleteTooltip;
            public bool UseRedDeleteButton;
        }

        /// <returns>true - row is only missing-slot, fields don't need to be drawn</returns>
        private bool DrawConditionRowShell(ConditionRowShellParams shellParams, List<int> removeIndices, out float fieldsStartY)
        {
            float zs = shellParams.ZoomScale;
            fieldsStartY = shellParams.RowBaseY;

            float headerY;
            if (shellParams.Layout == ConditionRowShellLayout.DialogStartPanel) headerY = shellParams.RowBaseY + CONDITION_TOP_PADDING;
            else if (shellParams.Layout == ConditionRowShellLayout.GraphConditionNode) headerY = shellParams.RowBaseY + 5 * zs;
            else headerY = shellParams.RowBaseY + CHOICE_EVENT_FIELD_TOP_PADDING * zs;

            bool showTitle = shellParams.Layout != ConditionRowShellLayout.ChoiceConditionRow;

            GUIStyle titleStyle;
            if (shellParams.LabelStyle != null) titleStyle = shellParams.LabelStyle;
            else titleStyle = EditorStyles.boldLabel;

            float deleteButtonX;
            float deleteButtonWidth;
            string deleteTooltip;

            if (shellParams.Layout == ConditionRowShellLayout.ChoiceConditionRow)
            {
                deleteButtonX = shellParams.ContentWidth - CHOICE_ITEM_CLOSE_OFFSET * zs;
                deleteButtonWidth = ACTION_BUTTON_SIZE * zs;
                if (string.IsNullOrEmpty(shellParams.DeleteTooltip)) deleteTooltip = "Delete condition";
                else deleteTooltip = shellParams.DeleteTooltip;
            }
            else
            {
                deleteButtonX = shellParams.ContentWidth - CONDITION_CLOSE_BUTTON_OFFSET * zs;
                deleteButtonWidth = CONDITION_CLOSE_BUTTON_WIDTH * zs;
                deleteTooltip = null;
            }

            Rect deleteRect = new Rect(deleteButtonX, headerY, deleteButtonWidth, FIELD_ROW_HEIGHT * zs);

            if (ShouldDrawMissingVariableSlot(shellParams.ReferenceState, shellParams.VariableName))
            {
                DrawMissingReferenceWarning(
                    shellParams.WarningBoundsRect,
                    GetMissingVariableDisplayLabel(shellParams.ReferenceState, shellParams.VariableName, shellParams.MissingReferenceLabel),
                    zs,
                    shellParams.LabelStyle);

                if (showTitle) GUI.Label(new Rect(CONDITION_LABEL_X * zs, headerY, CONDITION_TITLE_LABEL_WIDTH * zs, FIELD_ROW_HEIGHT * zs), $"Condition {shellParams.Index + 1}", titleStyle);

                if (DrawConditionRowDeleteButton(deleteRect, deleteTooltip, shellParams)) removeIndices.Add(shellParams.Index);

                return true;
            }

            if (showTitle) GUI.Label(new Rect(CONDITION_LABEL_X * zs, headerY, CONDITION_TITLE_LABEL_WIDTH * zs, FIELD_ROW_HEIGHT * zs), $"Condition {shellParams.Index + 1}", titleStyle);

            if (DrawConditionRowDeleteButton(deleteRect, deleteTooltip, shellParams)) removeIndices.Add(shellParams.Index);

            if (shellParams.Layout == ConditionRowShellLayout.GraphConditionNode) fieldsStartY = headerY + ACTION_ROW_HEIGHT * zs;
            else if (shellParams.Layout == ConditionRowShellLayout.DialogStartPanel) fieldsStartY = shellParams.RowBaseY + CONDITION_FIELD_Y_OFFSET;
            else fieldsStartY = headerY;

            return false;
        }

        private static bool DrawConditionRowDeleteButton(Rect deleteRect, string deleteTooltip, ConditionRowShellParams shellParams)
        {
            bool clicked;

            if (shellParams.Layout == ConditionRowShellLayout.ChoiceConditionRow)
            {
                GUIContent deleteContent = new GUIContent("×", deleteTooltip);
                if (shellParams.ButtonStyle != null) clicked = GUI.Button(deleteRect, deleteContent, shellParams.ButtonStyle);
                else clicked = GUI.Button(deleteRect, deleteContent);
            }
            else
            {
                if (shellParams.UseRedDeleteButton) GUI.backgroundColor = DELETE_BUTTON_COLOR;

                if (shellParams.ButtonStyle != null) clicked = GUI.Button(deleteRect, "×", shellParams.ButtonStyle);
                else clicked = GUI.Button(deleteRect, "×", EditorStyles.miniButton);

                if (shellParams.UseRedDeleteButton) GUI.backgroundColor = Color.white;
            }

            if (!clicked) return false;
            if (shellParams.RequireMouseButton && shellParams.CurrentEvent != null && shellParams.CurrentEvent.button != 0) return false;

            return true;
        }

        private void SaveDialogGraphChanges(string logTrigger)
        {
            InvalidateReferenceIndex();
            SaveDialogDatabase(logTrigger, graphOnDiskIsSessionSnapshot: false);
        }

        private void DelayedSave()
        {
            TryAutoSaveDialogEditor();
        }

        private void TryAutoSaveDialogEditor()
        {
            if (!_dialogEditorAutoSaveEnabled || !isEditingDialog)
            {
                _lastDialogEditorAutoSaveTime = -1;
                return;
            }

            if (_lastDialogEditorAutoSaveTime < 0)
            {
                _lastDialogEditorAutoSaveTime = EditorApplication.timeSinceStartup;
                return;
            }

            if (EditorApplication.timeSinceStartup - _lastDialogEditorAutoSaveTime >= _dialogEditorAutoSaveIntervalMinutes * 60.0) SaveDialogEditorSession("Auto-save timer");
        }

        private void PersistDialogDatabaseChanges(string logTrigger)
        {
            SaveDialogGraphChanges(logTrigger);
            RefreshManagedListFoldouts();
        }

        private void RefreshManagedListFoldouts()
        {
            foreach (string category in dialogCategories)
            {
                if (!categoryFoldouts.ContainsKey(category)) categoryFoldouts[category] = true;
            }

            foreach (string lang in availableLanguages)
            {
                if (!languageTextFoldouts.ContainsKey(lang)) languageTextFoldouts[lang] = true;
            }
        }

        private void CommitPendingDialogNameEditIfUnfocused()
        {
            if (!isEditingDialog || database == null) return;
            if (GUI.GetNameOfFocusedControl() == DIALOG_NAME_FIELD_CONTROL) return;

            CommitPendingDialogNameEdit();
        }

        private void LoadDatabases()
        {
            try
            {
                database = LoadAssetFromPrefs<DialogDatabase>(PREFS_DIALOG_DATABASE_GUID_KEY, "DialogDatabase");

                if (database != null) EnsureDialogDatabaseData(false);

                if (database != null && string.IsNullOrEmpty(EditorPrefs.GetString(PREFS_DIALOG_DATABASE_GUID_KEY, ""))) SaveAssetGuidToPrefs(PREFS_DIALOG_DATABASE_GUID_KEY, database);

                RefreshSavedDialogReferenceIndex();
                InvalidateReferenceIndex();
                isDatabasesLoaded = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load databases: {ex.Message}");
                database = null;
                isDatabasesLoaded = true;
            }
        }

        private static T LoadAssetFromPrefs<T>(string prefsKey, string resourcesFallbackName) where T : ScriptableObject
        {
            string guid = EditorPrefs.GetString(prefsKey, "");

            if (!string.IsNullOrEmpty(guid))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!string.IsNullOrEmpty(path))
                {
                    var fromGuid = AssetDatabase.LoadAssetAtPath<T>(path);

                    if (fromGuid != null) return fromGuid;
                }
            }

            return Resources.Load<T>(resourcesFallbackName);
        }

        private static void SaveAssetGuidToPrefs(string prefsKey, ScriptableObject asset)
        {
            if (asset == null)
            {
                EditorPrefs.DeleteKey(prefsKey);
                return;
            }

            string path = AssetDatabase.GetAssetPath(asset);

            if (string.IsNullOrEmpty(path)) return;

            EditorPrefs.SetString(prefsKey, AssetDatabase.AssetPathToGUID(path));
        }

        private void ResetDialogEditorSessionState()
        {
            isEditingDialog = false;
            selectedDialogName = null;
            selectedNodes.Clear();
            pendingPort = null;
            selectedPorts.Clear();
            editorOffset = Vector2.zero;
            showConditions = false;
            ClearDialogEditorSessionTracking();
        }

        private void ClearDialogEditorSessionTracking()
        {
            _dialogEditorDirtyBaseline = "";
        }

        private void OnDialogDatabaseChanged(DialogDatabase newDatabase)
        {
            if (database == newDatabase) return;

            if (isEditingDialog)
            {
                EditorUtility.DisplayDialog( "Dialog graph open", "Close the dialog graph with Back to list before switching Dialog Database.", "OK");
                Repaint();

                return;
            }

            string previousAssetLabel = GetDialogDatabaseAssetLabel(database);
            SaveDialogDatabase("Dialog database switch (flushing '" + previousAssetLabel + "' before loading new asset)", graphOnDiskIsSessionSnapshot: false);
            database = newDatabase;
            SaveAssetGuidToPrefs(PREFS_DIALOG_DATABASE_GUID_KEY, database);
            ResetDialogEditorSessionState();
            _variableRuntimeStateReady = false;

            if (database != null) EnsureDialogDatabaseData(true);

            RefreshSavedDialogReferenceIndex();
            InvalidateReferenceIndex();
            Repaint();
        }

        private void EnsureDialogDatabaseData(bool saveAfterMigration)
        {
            if (database == null) return;
            if (database.Dialogs == null) database.Dialogs = new List<DialogEntry>();
            if (database.Categories == null || database.Categories.Count == 0)
            {
                database.Categories = new List<string> { DEFAULT_CATEGORY_NAME, "Main Story", "Side Quests", "NPC", "Tutorial" };
                EditorUtility.SetDirty(database);
            }
            if (database.CharacterNameColors == null) database.CharacterNameColors = new List<CharacterNameColor>();
            if (database.CharacterGroups == null) database.CharacterGroups = new List<CharacterGroup>();

            foreach (var dialog in database.Dialogs)
            {
                if (dialog == null) continue;
                if (dialog.initiator == null) dialog.initiator = DEFAULT_CATEGORY_NAME;
                if (dialog.nodes == null) continue;

                foreach (var node in dialog.nodes)
                {
                    if (node?.NodeType != NodeType.Line || node.LineData == null) continue;
                    if (node.LineData.CharacterName == null) node.LineData.CharacterName = "";
                    if (node.LineData.CharacterActions == null) node.LineData.CharacterActions = new List<CharacterActionData>();
                    if (node.LineData.DialogsText == null)node.LineData.DialogsText = new List<LineText>();
                }
            }

            foreach (string category in dialogCategories)
            {
                if (!categoryFoldouts.ContainsKey(category)) categoryFoldouts[category] = true;
            }

            foreach (string lang in availableLanguages)
            {
                if (!languageTextFoldouts.ContainsKey(lang)) languageTextFoldouts[lang] = true;
            }

            if (saveAfterMigration) SaveDialogDatabase("Database migration / default fields", graphOnDiskIsSessionSnapshot: false);
        }


        private DialogDatabase CreateNewDialogDatabaseAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject( "Create Dialog Database", "DialogDatabase", "asset", "Choose path for the new dialog database");
                
            if (string.IsNullOrEmpty(path)) return null;

            var asset = CreateInstance<DialogDatabase>();
            asset.Categories = new List<string> { DEFAULT_CATEGORY_NAME, "Main Story", "Side Quests", "NPC", "Tutorial" };
            asset.Dialogs = new List<DialogEntry>();
            asset.CharacterNameColors = new List<CharacterNameColor>();
            asset.CharacterGroups = new List<CharacterGroup>();
            asset.SetVariables(new List<VariableDefinition>());
            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();
            EditorGUIUtility.PingObject(asset);

            return asset;
        }

        #endregion Loading


        #region Window Header

        private void DrawInlineDialogDatabaseSlot()
        {
            DrawInlineScriptableObjectSlot(
                "Dialog Database",
                database,
                OnDialogDatabaseChanged,
                CreateNewDialogDatabaseAsset,
                isEditingDialog ? DIALOG_DATABASE_SWITCH_LOCKED_TOOLTIP : null);
        }

        private void DrawToolbarTabRow(ref int selectedTab, GUIContent[] tabs)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            selectedTab = GUILayout.Toolbar(selectedTab, tabs, EditorStyles.toolbarButton, GUILayout.Height(TOOLBAR_HEIGHT));
            EditorGUILayout.EndHorizontal();
        }

        private void EndInlineHeaderSection()
        {
            EditorGUILayout.Space(INLINE_HEADER_SECTION_PADDING);
        }

        private bool AssetRowToolbarButton(GUIContent content, int width)
        {
            return GUILayout.Button(content, EditorStyles.toolbarButton, GUILayout.Width(width), GUILayout.Height(ASSET_ACTION_ROW_HEIGHT));
        }

        private bool AssetRowToolbarButton(string text, int width)
        {
            return AssetRowToolbarButton(new GUIContent(text), width);
        }

        private void BeginAssetSlotActionRow()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(ASSET_ACTION_ROW_HEIGHT));
        }

        private void EndAssetSlotActionRow()
        {
            EditorGUILayout.EndHorizontal();
        }

        private void FillAssetSlotToolbarRow()
        {
            GUILayout.FlexibleSpace();
        }

        private void DrawSearchRow(ref string filter, Action<string> onFilterChanged = null, bool showClearButton = false)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.Height(ASSET_ACTION_ROW_HEIGHT));
            GUILayout.Label("Search:", GUILayout.Width(SEARCH_LABEL_WIDTH), GUILayout.Height(INLINE_ASSET_CONTROL_HEIGHT));
            string newFilter = EditorGUILayout.TextField(filter ?? "", GUILayout.Height(INLINE_ASSET_CONTROL_HEIGHT));

            if (newFilter != filter)
            {
                filter = newFilter;
                onFilterChanged?.Invoke(newFilter);
            }

            if (showClearButton && GUILayout.Button("×", EditorStyles.toolbarButton, GUILayout.Width(CLEAR_BUTTON_WIDTH), GUILayout.Height(ASSET_ACTION_ROW_HEIGHT)) && !string.IsNullOrEmpty(filter))
            {
                filter = "";
                onFilterChanged?.Invoke("");
                GUI.FocusControl(null);
            }

            EditorGUILayout.EndHorizontal();
        }

        private void DrawInlineScriptableObjectSlot<T>( string label, T asset, Action<T> onChanged, Func<T> createNew, string switchLockedTooltip = null) where T : ScriptableObject
        {
            bool switchLocked = !string.IsNullOrEmpty(switchLockedTooltip);
            EditorGUILayout.LabelField( new GUIContent(label, switchLocked ? switchLockedTooltip : ""), EditorStyles.miniLabel, GUILayout.Width(INLINE_ASSET_LABEL_WIDTH), GUILayout.Height(INLINE_ASSET_CONTROL_HEIGHT));
            GUILayout.Space(INLINE_ASSET_SLOT_INDENT);
            EditorGUI.BeginDisabledGroup(switchLocked);
            EditorGUI.BeginChangeCheck();
            var newAsset = (T)EditorGUILayout.ObjectField( new GUIContent("", switchLocked ? switchLockedTooltip : ""), asset, typeof(T), false, GUILayout.Width(INLINE_ASSET_FIELD_WIDTH), GUILayout.Height(INLINE_ASSET_CONTROL_HEIGHT));

            if (EditorGUI.EndChangeCheck()) onChanged(newAsset);
            if (GUILayout.Button( new GUIContent("New", switchLocked ? switchLockedTooltip : ""), EditorStyles.toolbarButton, GUILayout.Width(INLINE_ASSET_NEW_BUTTON_WIDTH), GUILayout.Height(INLINE_ASSET_CONTROL_HEIGHT)))
            {
                T created = createNew();
                if (created != null) onChanged(created);
            }

            EditorGUI.EndDisabledGroup();

            if (switchLocked) GUILayout.Label( new GUIContent("⚠️", switchLockedTooltip), EditorStyles.miniLabel, GUILayout.Width(INLINE_ASSET_LOCKED_HINT_WIDTH), GUILayout.Height(INLINE_ASSET_CONTROL_HEIGHT));
        }

        #endregion Window Header


        #region Saving and Validation

        private bool IsDialogNameTaken(string dialogName, DialogEntry excludeDialog)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(dialogName)) return false;

            return database.Dialogs.Any(d => d != null && d != excludeDialog && d.dialogName == dialogName);
        }

        private string GetUniqueDialogName(string baseName, DialogEntry excludeDialog = null)
        {
            if (string.IsNullOrWhiteSpace(baseName)) baseName = DEFAULT_DIALOG_BASE_NAME;
            else baseName = baseName.Trim();

            string candidate = baseName;
            int index = 1;

            while (IsDialogNameTaken(candidate, excludeDialog))
            {
                candidate = $"{baseName}({index})";
                index++;
            }

            return candidate;
        }

        private void SyncPendingDialogNameEdit()
        {
            if (selectedDialog == null)
            {
                _pendingDialogNameSelectedKey = null;
                _pendingDialogNameEdit = "";

                return;
            }

            if (_pendingDialogNameSelectedKey == selectedDialogName) return;

            _pendingDialogNameSelectedKey = selectedDialogName;
            _pendingDialogNameEdit = selectedDialog.dialogName ?? "";
        }

        private void CommitPendingDialogNameEdit()
        {
            if (!isEditingDialog || selectedDialog == null || database == null) return;

            string finalName = GetUniqueDialogName(_pendingDialogNameEdit, selectedDialog);

            if (selectedDialog.dialogName == finalName)
            {
                _pendingDialogNameEdit = finalName;

                return;
            }

            Undo.RecordObject(database, "Change Dialog Name");
            string oldName = selectedDialog.dialogName;
            selectedDialog.dialogName = finalName;
            DialogReferenceRename.RenameDialogInRedirectNodes(database, oldName, finalName);
            selectedDialogName = finalName;
            _pendingDialogNameSelectedKey = finalName;
            _pendingDialogNameEdit = finalName;
            EditorUtility.SetDirty(database);
        }

        private void SaveDialogEditorSession(string logTrigger)
        {
            CommitPendingDialogNameEdit();
            SaveDialogGraphChanges(logTrigger);
            CaptureDialogEditorDirtyBaseline();
            _lastDialogEditorAutoSaveTime = EditorApplication.timeSinceStartup;
        }

        private bool HasPendingDialogNameEdit()
        {
            if (selectedDialog == null) return false;

            string pending = _pendingDialogNameEdit ?? "";
            string current = selectedDialog.dialogName ?? "";

            if (pending == current) return false;

            return GetUniqueDialogName(pending, selectedDialog) != current;
        }

        private void RestoreScriptableObjectAssetName(ScriptableObject asset)
        {
            if (asset == null) return;

            string path = AssetDatabase.GetAssetPath(asset);

            if (string.IsNullOrEmpty(path)) return;

            asset.name = Path.GetFileNameWithoutExtension(path);
        }

        private void CaptureDialogEditorDirtyBaseline()
        {
            if (database == null)
            {
                _dialogEditorDirtyBaseline = "";
                return;
            }

            _dialogEditorDirtyBaseline = SerializeDialogsBaseline(database.Dialogs);
        }

        private bool HasUnsavedDialogGraphChanges()
        {
            if (database == null) return false;
            if (HasPendingDialogNameEdit()) return true;
            if (string.IsNullOrEmpty(_dialogEditorDirtyBaseline)) return false;

            return SerializeDialogsBaseline(database.Dialogs) != _dialogEditorDirtyBaseline;
        }

        private bool HasUnsavedDialogEditorChanges()
        {
            if (!isEditingDialog || database == null) return false;

            return HasUnsavedDialogGraphChanges();
        }

        private bool DiscardUnsavedDialogDatabaseChanges()
        {
            if (database == null) return false;

            string restoreDialogName = selectedDialogName;
            string assetPath = AssetDatabase.GetAssetPath(database);
            string assetName = string.IsNullOrEmpty(assetPath) ? database.name : Path.GetFileNameWithoutExtension(assetPath);
            bool graphRestored = false;
            List<DialogEntry> baselineDialogs = GetDialogsFromGraphBaseline();

            if (baselineDialogs != null)
            {
                database.Dialogs = CloneDialogs(baselineDialogs);
                graphRestored = true;
            }

            if (!graphRestored)
            {
                EditorUtility.DisplayDialog( "Graph session lost", "Cannot discard unsaved graph edits - the in-memory snapshot was lost (for example after a script reload). The open graph was not changed. Save the graph or reload the Dialog Database asset if needed.", "OK");

                return false;
            }

            Undo.ClearUndo(database);
            RestoreScriptableObjectAssetName(database);

            if (!string.IsNullOrEmpty(assetName)) database.name = assetName;

            EnsureDialogDatabaseData(false);
            selectedDialogName = restoreDialogName;
            _pendingDialogNameSelectedKey = null;
            SyncPendingDialogNameEdit();
            selectedNodes.Clear();
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssetIfDirty(database);
            RefreshSavedDialogReferenceIndex();
            InvalidateReferenceIndex();
            LogDialogEditorDiscard(restoreDialogName);

            return true;
        }

        private void SaveDialogRegistry(string logTrigger)
        {
            if (database == null) return;

            if (isEditingDialog && HasUnsavedDialogGraphChanges())
            {
                List<DialogEntry> editingDialogs = CloneDialogs(database.Dialogs);
                RestoreDialogsFromGraphBaseline();
                SaveDialogDatabase(logTrigger, graphOnDiskIsSessionSnapshot: true);
                database.Dialogs = editingDialogs;
            }
            else SaveDialogDatabase(logTrigger, graphOnDiskIsSessionSnapshot: false);

            RefreshSavedDialogReferenceIndex();
            InvalidateReferenceIndex();
        }

        /// <summary> Saves registry-driven dialog graph patches to the asset and graph session baseline without committing unsaved layout edits. </summary>
        private void SaveRegistryDialogGraphPatch(string logTrigger, Action applyPatchToBaselineDialogs)
        {
            if (database == null) return;

            List<DialogEntry> patchedDialogsForDisk = null;

            if (isEditingDialog && !string.IsNullOrEmpty(_dialogEditorDirtyBaseline))
            {
                List<DialogEntry> baselineDialogs = GetDialogsFromGraphBaseline();

                if (baselineDialogs != null && applyPatchToBaselineDialogs != null)
                {
                    List<DialogEntry> memoryDialogs = database.Dialogs;
                    List<DialogEntry> baselineClone = CloneDialogs(baselineDialogs);

                    database.Dialogs = baselineClone;
                    applyPatchToBaselineDialogs();
                    _dialogEditorDirtyBaseline = SerializeDialogsBaseline(database.Dialogs);
                    patchedDialogsForDisk = CloneDialogs(database.Dialogs);
                    database.Dialogs = memoryDialogs;
                }
            }

            if (isEditingDialog && patchedDialogsForDisk != null && HasUnsavedDialogGraphChanges())
            {
                List<DialogEntry> editingDialogs = CloneDialogs(database.Dialogs);
                database.Dialogs = patchedDialogsForDisk;
                SaveDialogDatabase(logTrigger, graphOnDiskIsSessionSnapshot: false);
                database.Dialogs = editingDialogs;
            }
            else SaveDialogGraphChanges(logTrigger);

            RefreshSavedDialogReferenceIndex();
            InvalidateReferenceIndex();
        }

        private static List<DialogEntry> CloneDialogs(List<DialogEntry> source)
        {
            if (source == null) return null;

            DialogGraphSnapshot wrapper = new DialogGraphSnapshot { Dialogs = source };
            string json = JsonUtility.ToJson(wrapper);
            DialogGraphSnapshot clone = JsonUtility.FromJson<DialogGraphSnapshot>(json);

            return clone?.Dialogs;
        }

        private static string SerializeDialogsBaseline(List<DialogEntry> dialogs)
        {
            DialogGraphSnapshot wrapper = new DialogGraphSnapshot { Dialogs = dialogs };
            return JsonUtility.ToJson(wrapper);
        }

        private List<DialogEntry> GetDialogsFromGraphBaseline()
        {
            if (string.IsNullOrEmpty(_dialogEditorDirtyBaseline)) return null;

            DialogGraphSnapshot wrapper = JsonUtility.FromJson<DialogGraphSnapshot>(_dialogEditorDirtyBaseline);
            return wrapper?.Dialogs;
        }

        private void RestoreDialogsFromGraphBaseline()
        {
            List<DialogEntry> baselineDialogs = GetDialogsFromGraphBaseline();
            if (baselineDialogs != null) database.Dialogs = CloneDialogs(baselineDialogs);
        }

        private bool TryExitDialogEditorToList()
        {
            CommitPendingDialogNameEdit();

            if (HasUnsavedDialogEditorChanges())
            {
                string dialogLabel = selectedDialog?.dialogName ?? selectedDialogName ?? "dialog";
                int choice = EditorUtility.DisplayDialogComplex( "Unsaved changes", $"Dialog '{dialogLabel}' has unsaved changes. Save before closing?", "Save", "Stay", "Discard");

                if (choice == 1) return false;

                if (choice == 0) SaveDialogEditorSession("Back to list - Save");
                else if (choice == 2 && !DiscardUnsavedDialogDatabaseChanges()) return false;
            }

            isEditingDialog = false;
            ClearDialogEditorSessionTracking();
            GUI.FocusControl(null);

            return true;
        }

        private void SaveDialogDatabase(string logTrigger, bool graphOnDiskIsSessionSnapshot)
        {
            if (database == null) return;

            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssetIfDirty(database);
            RefreshSavedDialogReferenceIndex();
            LogDialogEditorSave(logTrigger, graphOnDiskIsSessionSnapshot);
        }

        private void SaveAllDialogEditorAssets(string logTrigger)
        {
            SaveDialogDatabase(logTrigger, graphOnDiskIsSessionSnapshot: false);
        }

        private void ValidateBrokenLinks()
        {
            if (selectedDialog == null || selectedDialog.nodes == null) return;

            var broken = ValidateAndGetBrokenLinks(selectedDialog);

            if (broken.Count == 0)
            {
                EditorUtility.DisplayDialog("Integrity Check", "No broken links found.", "OK");
                return;
            }

            const int maxShow = BROKEN_LINKS_MAX_SHOW;
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Broken links found: {broken.Count}");
            sb.AppendLine();

            for (int i = 0; i < Math.Min(broken.Count, maxShow); i++)
            {
                sb.AppendLine($"• {broken[i].description}");
            }

            if (broken.Count > maxShow)  sb.AppendLine($"... and {broken.Count - maxShow} more");

            bool fix = EditorUtility.DisplayDialog("Integrity Check", sb.ToString(), "Fix", "Close");

            if (fix)
            {
                Undo.RecordObject(database, "Fix Broken Links");

                foreach (var (_, fixAction) in broken)
                {
                    fixAction();
                }

                // SaveDialogGraphChanges();
                EditorUtility.DisplayDialog("Done", $"Fixed {broken.Count} broken links.", "OK");
            }
        }

        private List<DialogStartConditionOverlapValidator.ValidationIssue> GetCategoryStartConditionIssues(string category)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(category)) return new List<DialogStartConditionOverlapValidator.ValidationIssue>();

            return DialogStartConditionOverlapValidator.ValidateCategory(database.Dialogs, category);
        }

        private void ShowDialogStartConditionValidationResults(string scopeTitle, IReadOnlyList<DialogStartConditionOverlapValidator.ValidationIssue> issues)
        {
            if (issues == null || issues.Count == 0)
            {
                EditorUtility.DisplayDialog(scopeTitle, "No overlaps or ambiguities found.", "OK");

                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Issues found: {issues.Count}");
            sb.AppendLine("When conditions match, the first suitable dialog in the Dialog Database list is triggered.");
            sb.AppendLine();
            int maxShow = START_CONDITION_ISSUES_MAX_SHOW;

            for (int i = 0; i < Math.Min(issues.Count, maxShow); i++)
            {
                sb.AppendLine($"• {issues[i].Message}");
            }

            if (issues.Count > maxShow) sb.AppendLine($"... and {issues.Count - maxShow} more");

            EditorUtility.DisplayDialog(scopeTitle, sb.ToString(), "OK");
        }

        private void ValidateCurrentCategoryStartConditions()
        {
            if (selectedDialog == null) return;

            string category = selectedDialog.initiator;

            if (string.IsNullOrEmpty(category)) category = DEFAULT_CATEGORY_NAME;

            List<DialogStartConditionOverlapValidator.ValidationIssue> issues = GetCategoryStartConditionIssues(category);
            ShowDialogStartConditionValidationResults($"Conditions for category '{category}'", issues);
        }

        private void ValidateAllCategoriesStartConditions()
        {
            if (database?.Dialogs == null) return;

            var categories = new HashSet<string>();

            for (int i = 0; i < database.Dialogs.Count; i++)
            {
                DialogEntry dialog = database.Dialogs[i];
                if (dialog == null) continue;

                string category = dialog.initiator;
                if (string.IsNullOrEmpty(category)) category = DEFAULT_CATEGORY_NAME;
                categories.Add(category);
            }

            var allIssues = new List<DialogStartConditionOverlapValidator.ValidationIssue>();

            foreach (string category in categories)
            {
                allIssues.AddRange(GetCategoryStartConditionIssues(category));
            }

            ShowDialogStartConditionValidationResults("Conditions of all categories", allIssues);
        }

        private List<(string description, Action fix)> ValidateAndGetBrokenLinks(DialogEntry dialog)
        {
            var result = new List<(string, Action)>();

            if (dialog?.nodes == null) return result;

            var validIds = new HashSet<string>();

            foreach (var n in dialog.nodes)
            {
                if (n != null) validIds.Add(n.nodeId);
            }

            string NodeShortInfo(DialogNode node)
            {
                if (node == null) return "?";

                string type = node.NodeType switch { NodeType.Start => "Start", NodeType.Line => "Line", NodeType.Choice => "Choice", NodeType.Condition => "Condition", NodeType.Redirect => "Redirect", _ => "?" };
                string id;

                if (node.nodeId.Length >= 8) id = node.nodeId.Substring(0, 8);
                else id = node.nodeId;
                
                return $"{type} ({id})";
            }

            foreach (var node in dialog.nodes)
            {
                if (node == null) continue;

                string info = NodeShortInfo(node);

                if (node.NodeType == NodeType.Line && node.LineData != null && !string.IsNullOrEmpty(node.LineData.NextNodeId) && !validIds.Contains(node.LineData.NextNodeId)) result.Add(($"{info} → nextNodeId '{node.LineData.NextNodeId}' not found", () => node.LineData.NextNodeId = null));
                if (node.NodeType == NodeType.Start && node.StartData != null && !string.IsNullOrEmpty(node.StartData.NextNodeId) && !validIds.Contains(node.StartData.NextNodeId)) result.Add(($"{info} → nextNodeId '{node.StartData.NextNodeId}' not found", () => node.StartData.NextNodeId = null));
                if (node.ConditionData != null)
                {
                    if (!string.IsNullOrEmpty(node.ConditionData.TrueNodeId) && !validIds.Contains(node.ConditionData.TrueNodeId))

                    result.Add(($"{info} → trueNodeId '{node.ConditionData.TrueNodeId}' not found", () => node.ConditionData.TrueNodeId = null));

                    if (!string.IsNullOrEmpty(node.ConditionData.FalseNodeId) && !validIds.Contains(node.ConditionData.FalseNodeId))
                    
                    result.Add(($"{info} → falseNodeId '{node.ConditionData.FalseNodeId}' not found", () => node.ConditionData.FalseNodeId = null));
                }

                if (node.ChoiceData != null)
                {
                    for (int i = 0; i < node.ChoiceData.Count; i++)
                    {
                        var choice = node.ChoiceData[i];

                        if (!string.IsNullOrEmpty(choice.NextNodeId) && !validIds.Contains(choice.NextNodeId))
                        {
                            int idx = i;
                            result.Add(($"{info} choice {i + 1} → nextNodeId '{choice.NextNodeId}' not found", () => node.ChoiceData[idx].NextNodeId = null));
                        }
                    }
                }
                if (node.NodeType == NodeType.Redirect) ValidateRedirectNodeLinks(dialog, node, info, result);
            }

            return result;
        }

        private static void DisconnectIncomingLinksToNode(DialogEntry dialog, string removedNodeId)
        {
            if (dialog?.nodes == null || string.IsNullOrEmpty(removedNodeId)) return;

            foreach (DialogNode otherNode in dialog.nodes)
            {
                if (otherNode == null) continue;

                if (otherNode.NodeType == NodeType.Line && otherNode.LineData != null && otherNode.LineData.NextNodeId == removedNodeId) otherNode.LineData.NextNodeId = null;
                else if (otherNode.NodeType == NodeType.Start && otherNode.StartData != null && otherNode.StartData.NextNodeId == removedNodeId) otherNode.StartData.NextNodeId = null;
                else if (otherNode.NodeType == NodeType.Choice && otherNode.ChoiceData != null)
                {
                    for (int i = 0; i < otherNode.ChoiceData.Count; i++)
                    {
                        ChoiceNode choice = otherNode.ChoiceData[i];
                        if (choice != null && choice.NextNodeId == removedNodeId) choice.NextNodeId = null;
                    }
                }
                else if (otherNode.NodeType == NodeType.Condition && otherNode.ConditionData != null)
                {
                    if (otherNode.ConditionData.TrueNodeId == removedNodeId) otherNode.ConditionData.TrueNodeId = null;
                    if (otherNode.ConditionData.FalseNodeId == removedNodeId) otherNode.ConditionData.FalseNodeId = null;
                }
            }
        }

        private void RemoveDialogNode(DialogEntry dialog, DialogNode node)
        {
            if (dialog?.nodes == null || node == null) return;

            DisconnectIncomingLinksToNode(dialog, node.nodeId);
            selectedPorts.RemoveAll(p => p.Node == node);

            if (pendingPort != null && pendingPort.Node == node) pendingPort = null;

            selectedNodes.Remove(node);
            dialog.nodes.Remove(node);
        }

        #endregion Saving and Validation


        #region OnGUI

        private void OnGUI()
        {
            if (!stylesInitialized)
            {
                dialogButtonStyle = new GUIStyle(GUI.skin.button) { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(NODE_LEFT_RIGHT_PADDING, NODE_LEFT_RIGHT_PADDING, NODE_BOTTOM_PADDING, NODE_BOTTOM_PADDING) };
                categoryStyle = new GUIStyle(EditorStyles.foldout) { fontStyle = FontStyle.Bold, fontSize = CATEGORY_FONT_SIZE, margin = new RectOffset(4, 4, 6, 2) };
                lineBgTexture = new Texture2D(1, 1);
                lineBgTexture.SetPixel(0, 0, Color.white);
                lineBgTexture.Apply();
                stylesInitialized = true;
            }

            if (!isDatabasesLoaded)
            {
                EditorGUILayout.HelpBox("Initializing editor...", MessageType.Info);
                return;
            }

            GUIContent[] mainTabContents =
            {
                new GUIContent("💬  Text"),
                new GUIContent("🎭  Character Sprites"),
                new GUIContent("🗝️  Variables")
            };

            DrawToolbarTabRow(ref currentMainTab, mainTabContents);

            switch (currentMainTab)
            {
                case 0:
                    GUIContent[] dialogSubTabContents =
                    {
                        new GUIContent("💬  Dialogs"),
                        new GUIContent("🎨  Names")
                    };
                    DrawToolbarTabRow(ref dialogSubTab, dialogSubTabContents);

                    if (dialogSubTab == 0)
                    {
                        if (isEditingDialog) DrawDialogEditor();
                        else DrawDialogList();
                    }
                    else DrawCharacterNameColors();

                    break;

                case 1:
                    DrawCharacterEditor();
                    break;

                case 2:
                    DrawVariableEditor();
                    break;
            }

            // Escape - cancel connection waiting
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && pendingPort != null)
            {
                selectedPorts.Clear();
                pendingPort = null;
            }
            // Clear ports only when not waiting for connection (pendingPort), otherwise the user won't have time to click the target port
            else if (!isContextMenuOpen && !actionWasPerformed && selectedPorts.Count > 0 && pendingPort == null)
            {
                selectedPorts.Clear();
                pendingPort = null;
            }
        }

        #endregion OnGUI


        #region Dialog List

        private void DrawDialogList()
        {
            const int DIALOG_LIST_TOOLBAR_BUTTON_WIDTH = 150;
            const int SPACE_BEFORE_DIALOG_LIST = 18;
            const int MENU_BUTTON_WIDTH = 30;

            BeginAssetSlotActionRow();
            DrawInlineDialogDatabaseSlot();
            GUILayout.FlexibleSpace();
            GUI.enabled = database != null;

            if (AssetRowToolbarButton("+ Add new dialog", DIALOG_LIST_TOOLBAR_BUTTON_WIDTH))
            {
                GenericMenu menu = new GenericMenu();
                foreach (string category in dialogCategories)
                {
                    string cat = category;
                    menu.AddItem(new GUIContent(cat), false, () =>
                    {
                        Undo.RecordObject(database, "Add Dialog");
                        string newDialogName = GetUniqueDialogName(DEFAULT_DIALOG_BASE_NAME);

                        database.Dialogs.Add(new DialogEntry
                        {
                            dialogName = newDialogName,
                            initiator = cat,
                            conditions = new List<DialogCondition>(),
                            nodes = new List<DialogNode>
                            {
                                new DialogNode
                                {
                                    NodeType = NodeType.Start,
                                    Position = new Vector2(DEFAULT_POSITION_XY, DEFAULT_POSITION_XY),
                                    nodeId = Guid.NewGuid().ToString()
                                }
                            }
                        });
                        SaveDialogGraphChanges("Add new dialog");
                    });
                }
                menu.ShowAsContext();
            }

            if (AssetRowToolbarButton("Manage categories", DIALOG_LIST_TOOLBAR_BUTTON_WIDTH)) PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), new DialogCategoryPopup(database, PersistDialogDatabaseChanges));
            if (AssetRowToolbarButton("Manage languages", DIALOG_LIST_TOOLBAR_BUTTON_WIDTH)) PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), new LanguageManagementPopup(database, PersistDialogDatabaseChanges));
            if (AssetRowToolbarButton("Import / Export", DIALOG_LIST_TOOLBAR_BUTTON_WIDTH)) PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), new DialogLocalizationImportExportPopup(database, OnLocalizationImportApplied));
            if (AssetRowToolbarButton("Check conditions", DIALOG_LIST_TOOLBAR_BUTTON_WIDTH)) ValidateAllCategoriesStartConditions();
            if (AssetRowToolbarButton(new GUIContent("⚙", "Editor settings"), SETTINGS_TOOLBAR_BUTTON_WIDTH))
            {
                PopupWindow.Show(new Rect(Event.current.mousePosition, Vector2.zero), new DialogEditorSettingsPopup(database, logTrigger =>
                {
                    PersistDialogDatabaseChanges(logTrigger);
                    Repaint();
                },
                () =>
                {
                    ReloadDialogEditorPreferences();
                    Repaint();
                }));
            }

            GUI.enabled = true;
            EndAssetSlotActionRow();

            if (database == null)
            {
                EndInlineHeaderSection();
                EditorGUILayout.HelpBox("Assign or create a Dialog Database.", MessageType.Warning);
                return;
            }

            if (database.Dialogs == null) database.Dialogs = new List<DialogEntry>();

            EditorGUILayout.Space(INLINE_HEADER_SECTION_PADDING);

            DrawSearchRow(ref dialogSearchFilter, _ =>
            {
                if (string.IsNullOrEmpty(dialogSearchFilter)) return;

                foreach (string category in dialogCategories)
                {
                    categoryFoldouts[category] = true;
                }

            });

            EndInlineHeaderSection();
            dialogListScrollPos = EditorGUILayout.BeginScrollView(dialogListScrollPos);

            var groupedDialogs = database.Dialogs.GroupBy(d => d.initiator ?? DEFAULT_CATEGORY_NAME).OrderBy(g =>
            {
                if (dialogCategories.IndexOf(g.Key) >= 0) return dialogCategories.IndexOf(g.Key);
                return int.MaxValue;
            }).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var categoryGroup in groupedDialogs)
            {
                string category = categoryGroup.Key;
                List<DialogEntry> dialogs = categoryGroup.Value;

                if (!string.IsNullOrEmpty(dialogSearchFilter))
                {
                    dialogs = dialogs.Where(d => d.dialogName != null && d.dialogName.ToLower().Contains(dialogSearchFilter.ToLower())).ToList();

                    if (dialogs.Count == 0) continue;
                }

                if (!categoryFoldouts.ContainsKey(category)) categoryFoldouts[category] = true;

                GUILayout.Space(SPACE_AFTER_CATEGORY);
                EditorGUILayout.BeginHorizontal(GUILayout.Height(CATEGORY_HEADER_HEIGHT));
                Rect catRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(CATEGORY_HEADER_HEIGHT), GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(catRect, CATEGORY_BG);
                GUILayout.Space(SPACE_BEFORE_DIALOG_LIST);
                categoryFoldouts[category] = EditorGUILayout.Foldout(categoryFoldouts[category], $"Category name: {category}", true, categoryStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                if (categoryFoldouts[category])
                {
                    List<DialogStartConditionOverlapValidator.ValidationIssue> categoryIssues = GetCategoryStartConditionIssues(category);

                    if (categoryIssues.Count > 0)
                    {
                        EditorGUILayout.HelpBox(DialogStartConditionOverlapValidator.FormatSummary(categoryIssues, 2), MessageType.Warning);
                    }

                    for (int i = 0; i < dialogs.Count; i++)
                    {
                        var dialog = dialogs[i];
                        EditorGUILayout.BeginHorizontal(GUILayout.Height(DIALOG_ROW_HEIGHT));
                        GUILayout.Space(SPACE_AFTER_CATEGORY);
                        string icon;

                        if (dialog.isLocked) icon = "🔒";
                        else if (dialog.conditions != null && dialog.conditions.Count > 0) icon = "🗝️";
                        else icon = "📋";

                        GUILayout.Label(icon, GUILayout.Width(DIALOG_ICON_SIZE), GUILayout.Height(DIALOG_ICON_SIZE));

                        if (GUILayout.Button(dialog.dialogName ?? DEFAULT_CATEGORY_NAME, dialogButtonStyle, GUILayout.Height(CATEGORY_HEADER_HEIGHT)))
                        {
                            if (database.Dialogs.Any(d => d != null && d.dialogName == dialog.dialogName))
                            {
                                selectedDialogName = dialog.dialogName;
                                selectedNodes.Clear();
                                isDraggingSelection = false;
                                isEditingDialog = true;
                                CaptureDialogEditorDirtyBaseline();
                                var startNode = dialog.nodes.Find(n => n.NodeType == NodeType.Start);

                                if (startNode != null) editorOffset = startNode.Position - new Vector2(DEFAULT_POSITION_XY, DEFAULT_POSITION_XY);
                                else editorOffset = Vector2.zero;
                            }
                        }

                        if (GUILayout.Button("⋮", GUILayout.Width(MENU_BUTTON_WIDTH), GUILayout.Height(CATEGORY_HEADER_HEIGHT))) ShowDialogContextMenu(dialog);

                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void ShowDialogContextMenu(DialogEntry dialog)
        {
            GenericMenu menu = new GenericMenu();
            
            menu.AddItem(new GUIContent("Duplicate"), false, () =>
            {
                Undo.RecordObject(database, "Duplicate Dialog");
                DuplicateDialog(dialog);
                SaveDialogGraphChanges("Duplicate dialog");
            });
            menu.AddSeparator("");

            foreach (string cat in dialogCategories)
            {
                string category = cat;
                menu.AddItem(new GUIContent("Move to/" + category), dialog.initiator == category, () =>
                {
                    Undo.RecordObject(database, "Move Dialog");
                    dialog.initiator = category;
                    SaveDialogGraphChanges("Move dialog to category");
                });
            }

            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                string confirmMessage = BuildDeleteDialogConfirmMessage(dialog.dialogName);
                if (EditorUtility.DisplayDialog("Delete dialog", confirmMessage, "Yes", "Cancel"))
                {
                    if (database.Dialogs.Contains(dialog))
                    {
                        Undo.RecordObject(database, "Remove Dialog");
                        DialogReferenceCleanup.MarkDialogMissingInDialogs(database, dialog.dialogName);
                        database.Dialogs.Remove(dialog);

                        if (selectedDialogName == dialog.dialogName)
                        {
                            selectedDialogName = null;
                            isEditingDialog = false;
                            selectedPorts.Clear();
                            selectedNodes.Clear();
                            isDraggingSelection = false;
                            pendingPort = null;
                        }

                        SaveDialogGraphChanges("Delete dialog");
                    }
                }
            });

            menu.ShowAsContext();
        }

        #endregion Dialog List


        #region Duplicator

        private void DuplicateDialog(DialogEntry original)
        {
            if (original == null)
            {
                Debug.LogError("Attempting to duplicate Dialog");
                return;
            }

            string baseName = original.dialogName + " (copy)";
            string newDialogName = baseName;
            int copyIndex = 1;

            while (database.Dialogs.Any(d => d != null && d.dialogName == newDialogName))
            {
                newDialogName = $"{baseName} {copyIndex}";
                copyIndex++;
            }

            DialogEntry duplicate = new DialogEntry
            {
                dialogName = newDialogName,
                initiator = original.initiator,
                isAndLogic = original.isAndLogic,
                isLocked = original.isLocked,
                nodes = new List<DialogNode>(),
                conditions = new List<DialogCondition>()
            };

            if (original.nodes != null)
            {
                Dictionary<string, string> nodeIdMap = new Dictionary<string, string>();

                foreach (var originalNode in original.nodes)
                {
                    if (originalNode == null) continue; // Skip null nodes

                    DialogNode newNode = new DialogNode
                    {
                        nodeId = Guid.NewGuid().ToString(),
                        Position = originalNode.Position,
                        NodeType = originalNode.NodeType
                    };

                    if (originalNode.NodeType == NodeType.Line)
                    {
                        List<LineText> duplicateDialogsText;

                        if (originalNode.LineData?.DialogsText != null) duplicateDialogsText = originalNode.LineData.DialogsText.Select(t => new LineText
                            {
                                LanguageCode = t.LanguageCode,
                                SayerName = t.SayerName,
                                Text = t.Text,
                                VoiceClip = t.VoiceClip
                            }).ToList();
                        else duplicateDialogsText = new List<LineText>();

                        List<CharacterActionData> duplicateCharacterActions;
                        
                        if (originalNode.LineData?.CharacterActions != null) duplicateCharacterActions = originalNode.LineData.CharacterActions.Select(CopyCharacterActionData).ToList();
                        else duplicateCharacterActions = new List<CharacterActionData>();

                        newNode.LineData = new LineNode
                        {
                            CharacterName = originalNode.LineData?.CharacterName ?? "",
                            CharacterReferenceState = originalNode.LineData?.CharacterReferenceState ?? DialogReferenceState.Valid,
                            MissingCharacterReferenceLabel = originalNode.LineData?.MissingCharacterReferenceLabel ?? "",
                            DialogsText = duplicateDialogsText,
                            CharacterActions = duplicateCharacterActions,
                            NextNodeId = "", // Explicitly do not copy connections
                            isInstant = originalNode.LineData?.isInstant ?? false
                        };
                    }
                    else if (originalNode.NodeType == NodeType.Choice)
                    {
                        List<ChoiceNode> duplicateChoiceData;

                        if (originalNode.ChoiceData != null)
                        {
                            duplicateChoiceData = originalNode.ChoiceData.Select(c =>
                            {
                                List<LineText> translations;

                                if (c?.Translations != null)translations = c.Translations.Select(t => new LineText
                                    {
                                        LanguageCode = t.LanguageCode,
                                        Text = t.Text
                                    }).ToList();
                                else translations = new List<LineText>();

                                List<ChoiceCondition> choiceCondition;

                                if (c?.ChoiceCondition != null) choiceCondition = c.ChoiceCondition.Select(CopyChoiceCondition).ToList();
                                else choiceCondition = new List<ChoiceCondition>();

                                List<ChoiceConsequence> choiceConsequence;

                                if (c?.ChoiceConsequence != null) choiceConsequence = c.ChoiceConsequence.Select(CopyChoiceConsequence).ToList();
                                else choiceConsequence = new List<ChoiceConsequence>();

                                return new ChoiceNode
                                {
                                    Translations = translations,
                                    ChoiceCondition = choiceCondition,
                                    IsAndLogic = c?.IsAndLogic ?? true,
                                    ChoiceConsequence = choiceConsequence,
                                    NextNodeId = "" // Explicitly do not copy connections
                                };
                            }).ToList();
                        }
                        else duplicateChoiceData = new List<ChoiceNode>();

                        newNode.ChoiceData = duplicateChoiceData;
                    }
                    else if (originalNode.NodeType == NodeType.Condition)
                    {
                        List<DialogCondition> duplicateDialogConditions;

                        if (originalNode.ConditionData?.DialogConditions != null) duplicateDialogConditions = originalNode.ConditionData.DialogConditions.Select(CopyDialogCondition).ToList();
                        else duplicateDialogConditions = new List<DialogCondition>();

                        newNode.ConditionData = new ConditionNode
                        {
                            IsAndLogic = originalNode.ConditionData?.IsAndLogic ?? false,
                            DialogConditions = duplicateDialogConditions,
                            TrueNodeId = "",
                            FalseNodeId = ""
                        };
                    }
                    else if (originalNode.NodeType == NodeType.Redirect)
                    {
                        RedirectNode sourceRedirect = originalNode.RedirectData ?? new RedirectNode();
                        newNode.RedirectData = new RedirectNode
                        {
                            TargetCategory = sourceRedirect.TargetCategory,
                            TargetDialogName = sourceRedirect.TargetDialogName,
                            DialogReferenceState = sourceRedirect.DialogReferenceState,
                            MissingDialogReferenceLabel = sourceRedirect.MissingDialogReferenceLabel
                        };
                    }


                    nodeIdMap[originalNode.nodeId] = newNode.nodeId;
                    duplicate.nodes.Add(newNode);
                }
            }

            if (original.conditions != null)
            {
                duplicate.conditions = original.conditions.Select(CopyDialogCondition).ToList();
            }

            database.Dialogs.Add(duplicate);
            SaveDialogGraphChanges("Duplicate dialog (internal)");
        }

        #endregion Duplicator


        #region Variable Helpers (Graph and Editor)

        private string[] GetVariableNamesByType(ConditionVariableType type)
        {
            if (database == null) return Array.Empty<string>();

            switch (type)
            {
                case ConditionVariableType.Bool:
                    return database.GetBoolVariableNames();
                case ConditionVariableType.Int:
                    return database.GetIntVariableNames();
                case ConditionVariableType.Float:
                    return database.GetFloatVariableNames();
                case ConditionVariableType.String:
                    return database.GetStringVariableNames();
                default:
                    Debug.LogWarning($"Unknown variable type: {type}");
                    return database.GetVariableNamesByType(type);
            }
        }

        private bool TryGetFirstAvailableVariableType(out ConditionVariableType type)
        {
            foreach (ConditionVariableType candidate in Enum.GetValues(typeof(ConditionVariableType)))
            {
                if (GetVariableNamesByType(candidate).Length > 0)
                {
                    type = candidate;
                    return true;
                }
            }

            type = ConditionVariableType.Bool;
            return false;
        }

        private ConditionVariableType ResolveEditorVariableType(ConditionVariableType currentType)
        {
            if (GetVariableNamesByType(currentType).Length > 0) return currentType;
            if (TryGetFirstAvailableVariableType(out ConditionVariableType firstType)) return firstType;

            return currentType;
        }

        private void ApplyDialogConditionVariableTypeChange(DialogCondition condition, ConditionVariableType newType)
        {
            condition.VariableType = newType;
            condition.VariableName = null;

            if (newType == ConditionVariableType.Bool && condition.ConditionOperator != ConditionOperator.Equals && condition.ConditionOperator != ConditionOperator.NotEquals) condition.ConditionOperator = ConditionOperator.Equals;

            DialogReferenceEditorUtility.ResetVariableReferenceState(condition);
        }

        private void ApplyChoiceConditionVariableTypeChange(ChoiceCondition condition, ConditionVariableType newType)
        {
            condition.VariableType = newType;
            condition.VariableName = null;

            if (newType == ConditionVariableType.Bool && condition.ConditionOperator != ConditionOperator.Equals && condition.ConditionOperator != ConditionOperator.NotEquals) condition.ConditionOperator = ConditionOperator.Equals;

            DialogReferenceEditorUtility.ResetVariableReferenceState(condition);
        }

        private void ApplyChoiceConsequenceVariableTypeChange(ChoiceConsequence consequence, ConditionVariableType newType)
        {
            consequence.VariableType = newType;
            consequence.VariableName = null;
            DialogReferenceEditorUtility.ResetVariableReferenceState(consequence);
        }

        private DialogCondition CreateDefaultDialogCondition()
        {
            DialogCondition condition = new DialogCondition();

            if (TryGetFirstAvailableVariableType(out ConditionVariableType firstType)) condition.VariableType = firstType;

            return condition;
        }

        private ChoiceCondition CreateDefaultChoiceCondition()
        {
            ChoiceCondition condition = new ChoiceCondition { VariableName = null };

            if (TryGetFirstAvailableVariableType(out ConditionVariableType firstType)) condition.VariableType = firstType;

            return condition;
        }

        private ChoiceConsequence CreateDefaultChoiceConsequence()
        {
            ChoiceConsequence consequence = new ChoiceConsequence { VariableName = null };

            if (TryGetFirstAvailableVariableType(out ConditionVariableType firstType)) consequence.VariableType = firstType;

            return consequence;
        }

        private bool EnsureDialogConditionVariableType(DialogCondition condition)
        {
            if (condition == null) return false;

            ConditionVariableType resolvedType = ResolveEditorVariableType(condition.VariableType);

            if (resolvedType == condition.VariableType) return false;

            Undo.RecordObject(database, "Resolve Condition Variable Type");
            ApplyDialogConditionVariableTypeChange(condition, resolvedType);
            InvalidateReferenceIndex();

            return true;
        }

        private bool EnsureChoiceConditionVariableType(ChoiceCondition condition)
        {
            if (condition == null) return false;

            ConditionVariableType resolvedType = ResolveEditorVariableType(condition.VariableType);

            if (resolvedType == condition.VariableType) return false;

            Undo.RecordObject(database, "Resolve Choice Condition Variable Type");
            ApplyChoiceConditionVariableTypeChange(condition, resolvedType);
            InvalidateReferenceIndex();

            return true;
        }

        private bool EnsureChoiceConsequenceVariableType(ChoiceConsequence consequence)
        {
            if (consequence == null) return false;

            ConditionVariableType resolvedType = ResolveEditorVariableType(consequence.VariableType);

            if (resolvedType == consequence.VariableType) return false;

            Undo.RecordObject(database, "Resolve Choice Consequence Variable Type");
            ApplyChoiceConsequenceVariableTypeChange(consequence, resolvedType);
            InvalidateReferenceIndex();

            return true;
        }

        private static string[] FilterVariableNamesForSelection(string[] variableNames, string currentVariableName, IReadOnlyList<string> excludedVariableNames)
        {
            if (variableNames == null || variableNames.Length == 0) return Array.Empty<string>();
            if (excludedVariableNames == null || excludedVariableNames.Count == 0) return variableNames;

            HashSet<string> excluded = new HashSet<string>();

            for (int i = 0; i < excludedVariableNames.Count; i++)
            {
                string name = excludedVariableNames[i];
                if (string.IsNullOrEmpty(name)) continue;
                if (name == currentVariableName) continue;
                excluded.Add(name);
            }

            if (excluded.Count == 0) return variableNames;

            List<string> filtered = new List<string>(variableNames.Length);

            for (int i = 0; i < variableNames.Length; i++)
            {
                string name = variableNames[i];

                if (excluded.Contains(name)) continue;

                filtered.Add(name);
            }

            return filtered.ToArray();
        }

        private static readonly ConditionOperator[] NumericConditionOperatorsAll =
        {
            ConditionOperator.Equals,
            ConditionOperator.NotEquals,
            ConditionOperator.GreaterThan,
            ConditionOperator.LessThan,
            ConditionOperator.GreaterThanOrEquals,
            ConditionOperator.LessThanOrEquals
        };


        private static bool IsNumericEqualsConditionOperator(ConditionOperator conditionOperator)
        {
            return conditionOperator == ConditionOperator.Equals;
        }

        private static void AddNumericOperatorReuseForbidden(HashSet<ConditionOperator> forbidden, ConditionOperator siblingOperator)
        {
            if (siblingOperator == ConditionOperator.NotEquals) return;

            if (siblingOperator == ConditionOperator.GreaterThan || siblingOperator == ConditionOperator.GreaterThanOrEquals)
            {
                forbidden.Add(ConditionOperator.GreaterThan);
                forbidden.Add(ConditionOperator.GreaterThanOrEquals);
                forbidden.Add(ConditionOperator.Equals);
                return;
            }

            if (siblingOperator == ConditionOperator.LessThan || siblingOperator == ConditionOperator.LessThanOrEquals)
            {
                forbidden.Add(ConditionOperator.LessThan);
                forbidden.Add(ConditionOperator.LessThanOrEquals);
                forbidden.Add(ConditionOperator.Equals);
            }
        }

        private static ConditionOperator[] GetAllowedNumericConditionOperators(IReadOnlyList<DialogCondition> siblingConditions, int excludeIndex, string variableName)
        {
            if (string.IsNullOrEmpty(variableName)) return NumericConditionOperatorsAll;

            HashSet<ConditionOperator> forbidden = new HashSet<ConditionOperator>();
            bool hasSameVariableSibling = false;

            if (siblingConditions != null)
            {
                for (int i = 0; i < siblingConditions.Count; i++)
                {
                    if (i == excludeIndex) continue;

                    DialogCondition sibling = siblingConditions[i];

                    if (sibling == null || DialogReferenceUtility.IsMissing(sibling.ReferenceState)) continue;
                    if (string.IsNullOrEmpty(sibling.VariableName)) continue;
                    if (sibling.VariableName != variableName) continue;

                    hasSameVariableSibling = true;
                    AddNumericOperatorReuseForbidden(forbidden, sibling.ConditionOperator);
                }
            }

            if (!hasSameVariableSibling) return NumericConditionOperatorsAll;

            List<ConditionOperator> allowed = new List<ConditionOperator>(NumericConditionOperatorsAll.Length);

            for (int i = 0; i < NumericConditionOperatorsAll.Length; i++)
            {
                ConditionOperator op = NumericConditionOperatorsAll[i];

                if (forbidden.Contains(op)) continue;

                allowed.Add(op);
            }

            if (allowed.Count == 0) return Array.Empty<ConditionOperator>();

            return allowed.ToArray();
        }

        private static ConditionOperator[] GetAllowedNumericConditionOperators(IReadOnlyList<ChoiceCondition> siblingConditions, int excludeIndex, string variableName)
        {
            if (string.IsNullOrEmpty(variableName)) return NumericConditionOperatorsAll;

            HashSet<ConditionOperator> forbidden = new HashSet<ConditionOperator>();
            bool hasSameVariableSibling = false;

            if (siblingConditions != null)
            {
                for (int i = 0; i < siblingConditions.Count; i++)
                {
                    if (i == excludeIndex) continue;

                    ChoiceCondition sibling = siblingConditions[i];

                    if (sibling == null || DialogReferenceUtility.IsMissing(sibling.ReferenceState)) continue;
                    if (string.IsNullOrEmpty(sibling.VariableName)) continue;
                    if (sibling.VariableName != variableName) continue;

                    hasSameVariableSibling = true;
                    AddNumericOperatorReuseForbidden(forbidden, sibling.ConditionOperator);
                }
            }

            if (!hasSameVariableSibling) return NumericConditionOperatorsAll;

            List<ConditionOperator> allowed = new List<ConditionOperator>(NumericConditionOperatorsAll.Length);

            for (int i = 0; i < NumericConditionOperatorsAll.Length; i++)
            {
                ConditionOperator op = NumericConditionOperatorsAll[i];

                if (forbidden.Contains(op)) continue;

                allowed.Add(op);
            }

            if (allowed.Count == 0) return Array.Empty<ConditionOperator>();

            return allowed.ToArray();
        }

        private static void CollectNumericConditionVariableExclusions<T>(
            IReadOnlyList<T> conditions,
            int excludeIndex,
            HashSet<string> excludedNames,
            Func<T, ConditionVariableType> getVariableType,
            Func<T, string> getVariableName,
            Func<T, ConditionOperator> getConditionOperator,
            Func<T, DialogReferenceState> getReferenceState,
            Func<IReadOnlyList<T>, int, string, ConditionOperator[]> getAllowedOperators)
        {
            HashSet<string> numericNamesToCheck = new HashSet<string>();

            if (conditions == null) return;

            for (int i = 0; i < conditions.Count; i++)
            {
                if (i == excludeIndex) continue;

                T condition = conditions[i];

                if (condition == null || DialogReferenceUtility.IsMissing(getReferenceState(condition))) continue;

                string variableName = getVariableName(condition);

                if (string.IsNullOrEmpty(variableName)) continue;

                ConditionVariableType variableType = getVariableType(condition);

                if (variableType == ConditionVariableType.Bool || variableType == ConditionVariableType.String)
                {
                    excludedNames.Add(variableName);
                    continue;
                }

                if (variableType == ConditionVariableType.Int || variableType == ConditionVariableType.Float)
                {
                    if (IsNumericEqualsConditionOperator(getConditionOperator(condition))) excludedNames.Add(variableName);
                    else numericNamesToCheck.Add(variableName);
                    continue;
                }

                excludedNames.Add(variableName);
            }

            foreach (string variableName in numericNamesToCheck)
            {
                if (excludedNames.Contains(variableName)) continue;

                ConditionOperator[] allowedOperators = getAllowedOperators(conditions, excludeIndex, variableName);

                if (allowedOperators.Length == 0) excludedNames.Add(variableName);
            }
        }

        private static List<string> CollectUsedVariableNames(IReadOnlyList<DialogCondition> conditions, int excludeIndex)
        {
            HashSet<string> excludedNames = new HashSet<string>();
            CollectNumericConditionVariableExclusions(
                conditions,
                excludeIndex,
                excludedNames,
                condition => condition.VariableType,
                condition => condition.VariableName,
                condition => condition.ConditionOperator,
                condition => condition.ReferenceState,
                GetAllowedNumericConditionOperators);

            return new List<string>(excludedNames);
        }

        private static List<string> CollectUsedVariableNames(IReadOnlyList<ChoiceCondition> conditions, int excludeIndex)
        {
            HashSet<string> excludedNames = new HashSet<string>();
            CollectNumericConditionVariableExclusions(
                conditions,
                excludeIndex,
                excludedNames,
                condition => condition.VariableType,
                condition => condition.VariableName,
                condition => condition.ConditionOperator,
                condition => condition.ReferenceState,
                GetAllowedNumericConditionOperators);

            return new List<string>(excludedNames);
        }

        private static List<string> CollectUsedVariableNames(IReadOnlyList<ChoiceConsequence> consequences, int excludeIndex)
        {
            List<string> usedNames = new List<string>();

            if (consequences == null) return usedNames;

            for (int i = 0; i < consequences.Count; i++)
            {
                if (i == excludeIndex) continue;

                ChoiceConsequence consequence = consequences[i];

                if (consequence == null || DialogReferenceUtility.IsMissing(consequence.ReferenceState)) continue;
                if (string.IsNullOrEmpty(consequence.VariableName)) continue;

                usedNames.Add(consequence.VariableName);
            }

            return usedNames;
        }

        private static string DrawVariableNamePopup(Rect rect, string[] variableNames, string currentVariableName, GUIStyle style = null, IReadOnlyList<string> excludedVariableNames = null)
        {
            string[] selectableVariableNames = FilterVariableNamesForSelection(variableNames, currentVariableName, excludedVariableNames);

            if (selectableVariableNames == null || selectableVariableNames.Length == 0)
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.TextField(rect, NO_VARS_PLACEHOLDER, style ?? EditorStyles.textField);
                EditorGUI.EndDisabledGroup();
                return currentVariableName;
            }

            string[] displayOptions = new string[selectableVariableNames.Length + 1];
            displayOptions[0] = SELECT_VAR_PLACEHOLDER;

            for (int i = 0; i < selectableVariableNames.Length; i++) 
            {
                displayOptions[i + 1] = selectableVariableNames[i];
            }

            int selectedIndex;

            if (string.IsNullOrEmpty(currentVariableName)) selectedIndex = 0;
            else selectedIndex = Array.IndexOf(selectableVariableNames, currentVariableName) + 1;
            
            if (selectedIndex <= 0) selectedIndex = 0;

            int newIndex = EditorGUI.Popup(rect, selectedIndex, displayOptions, style ?? EditorStyles.popup);

            if (newIndex == 0) return null;

            return selectableVariableNames[newIndex - 1];
        }

        private static void DrawMissingReferenceWarning(Rect rect, string message, float zoomScale, GUIStyle scaledLabelStyle = null)
        {
            GUIStyle style;

            if (scaledLabelStyle != null) style = new GUIStyle(scaledLabelStyle);
            else style = new GUIStyle(EditorStyles.wordWrappedMiniLabel);

            style.wordWrap = true;
            style.alignment = TextAnchor.MiddleCenter;
            style.clipping = TextClipping.Clip;

            if (scaledLabelStyle == null) style.fontSize = Mathf.Max(1, Mathf.RoundToInt(style.fontSize * zoomScale));

            string fullText = MISSING_WARNING_EMOJI_PREFIX + message;
            float textHeight = style.CalcHeight(new GUIContent(fullText), rect.width);

            if (textHeight > rect.height) textHeight = rect.height;

            float textY = rect.y + (rect.height - textHeight) * 0.5f;
            Rect textRect = new Rect(rect.x, textY, rect.width, textHeight);
            Color previousColor = GUI.color;
            GUI.color = new Color(1f, 0.82f, 0.35f);
            GUI.Label(textRect, fullText, style);
            GUI.color = previousColor;
        }

        private float DrawLineVoiceClipRow(LineText localizedText, float currentY, float contentWidthZS, float zoomScale, GUIStyle labelStyle, GUIStyle buttonStyle, Event e)
        {
            DialogEditorAudioPreview.SetRepaintTarget(this);
            float x = 0f;
            float rowH = FIELD_ROW_HEIGHT * zoomScale;
            float gap = LINE_VOICE_FIELD_GAP * zoomScale;
            Rect labelRect = new Rect(x, currentY, LINE_VOICE_LABEL_WIDTH * zoomScale, rowH);
            GUI.Label(labelRect, "Voice:", labelStyle);
            x += LINE_VOICE_LABEL_WIDTH * zoomScale + gap;
            float fieldW = LINE_VOICE_OBJECT_FIELD_WIDTH * zoomScale;
            Rect fieldRect = new Rect(x, currentY, fieldW, rowH);
            EditorGUI.BeginChangeCheck();
            AudioClip newClip = (AudioClip)EditorGUI.ObjectField(fieldRect, localizedText.VoiceClip, typeof(AudioClip), false);

            if (EditorGUI.EndChangeCheck())
            {
                if (DialogEditorAudioPreview.ActiveClip == localizedText.VoiceClip) DialogEditorAudioPreview.Stop();
                Undo.RecordObject(database, "Change Line Voice Clip");
                localizedText.VoiceClip = newClip;
            }

            x += fieldW + LINE_VOICE_OBJECT_TO_SCRUB_GAP * zoomScale;
            AudioClip clip = localizedText.VoiceClip;
            float duration = DialogEditorAudioPreview.GetDuration(clip);
            float currentTime = DialogEditorAudioPreview.GetCurrentTime(clip);
            float playW = LINE_VOICE_PLAY_WIDTH * zoomScale;
            float timeW = LINE_VOICE_COMBINED_TIME_WIDTH * zoomScale;
            float scrubW = contentWidthZS - x - timeW - playW - gap;

            if (scrubW < LINE_VOICE_SCRUBBER_MIN_WIDTH * zoomScale) scrubW = LINE_VOICE_SCRUBBER_MIN_WIDTH * zoomScale;

            Rect scrubRect = new Rect(x, currentY + (rowH - LINE_VOICE_SCRUBBER_HEIGHT * zoomScale) * 0.5f, scrubW, LINE_VOICE_SCRUBBER_HEIGHT * zoomScale);
            Rect scrubHitRect = new Rect(x, currentY, scrubW, rowH);
            currentTime = DrawVoiceScrubber(scrubRect, scrubHitRect, clip, duration, currentTime, e);
            x += scrubW + gap;

            GUIStyle timeStyle = new GUIStyle(labelStyle)
            {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip
            };

            Rect timeRect = new Rect(x, currentY, timeW, rowH);
            string timeLabel = DialogEditorAudioPreview.FormatTime(currentTime) + "/" + DialogEditorAudioPreview.FormatTime(duration);
            GUI.Label(timeRect, timeLabel, timeStyle);
            x += timeW;
            Rect playRect = new Rect(x, currentY, playW, rowH);
            string playLabel = "▶";

            if (DialogEditorAudioPreview.IsActivelyPlaying(clip)) playLabel = "||";

            EditorGUI.BeginDisabledGroup(clip == null);

            if (GUI.Button(playRect, new GUIContent(playLabel, "Play / Pause preview"), buttonStyle) && e.button == 0) DialogEditorAudioPreview.TogglePlayPause(clip);

            EditorGUI.EndDisabledGroup();

            return currentY + LINE_NODE_AUDIO_ROW_HEIGHT * zoomScale;
        }

        private float DrawVoiceScrubber(Rect scrubRect, Rect hitRect, AudioClip clip, float duration, float currentTime, Event e)
        {
            Color trackColor;
            Color fillColor;
            Color handleColor;

            if (EditorGUIUtility.isProSkin)
            {
                trackColor = new Color(0.15f, 0.15f, 0.15f, 1f);
                fillColor = new Color(0.28f, 0.45f, 0.72f, 1f);
                handleColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            }
            else
            {
                trackColor = new Color(0.65f, 0.65f, 0.65f, 1f);
                fillColor = new Color(0.35f, 0.55f, 0.85f, 1f);
                handleColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            }

            if (clip == null || duration <= 0f)
            {
                EditorGUI.DrawRect(scrubRect, trackColor);
                return currentTime;
            }

            int controlId = GUIUtility.GetControlID(FocusType.Passive);
            bool isScrubbing = GUIUtility.hotControl == controlId;
            EventType eventType = e.GetTypeForControl(controlId);
            Vector2 mousePos = Event.current.mousePosition;

            if (eventType == EventType.MouseDown && e.button == 0 && hitRect.Contains(mousePos))
            {
                GUIUtility.hotControl = controlId;
                GUIUtility.keyboardControl = 0;
                _voiceScrubKeepPlaying = DialogEditorAudioPreview.IsActivelyPlaying(clip);

                if (_voiceScrubKeepPlaying) DialogEditorAudioPreview.Pause();

                currentTime = SeekTimeFromMouse(scrubRect, duration, mousePos.x);
                DialogEditorAudioPreview.Seek(clip, currentTime);
                e.Use();
                isScrubbing = true;
            }
            else if (eventType == EventType.MouseDrag && GUIUtility.hotControl == controlId)
            {
                currentTime = SeekTimeFromMouse(scrubRect, duration, mousePos.x);
                DialogEditorAudioPreview.Seek(clip, currentTime);
                e.Use();
                isScrubbing = true;
            }
            else if (eventType == EventType.MouseUp && GUIUtility.hotControl == controlId)
            {
                currentTime = SeekTimeFromMouse(scrubRect, duration, mousePos.x);
                DialogEditorAudioPreview.Seek(clip, currentTime);

                if (_voiceScrubKeepPlaying) DialogEditorAudioPreview.PlayFromTime(clip, currentTime);

                GUIUtility.hotControl = 0;
                _voiceScrubKeepPlaying = false;
                e.Use();
            }
            else if (isScrubbing) currentTime = DialogEditorAudioPreview.GetCurrentTime(clip);

            float progress = Mathf.Clamp01(currentTime / duration);
            EditorGUI.DrawRect(scrubRect, trackColor);
            Rect fillRect = new Rect(scrubRect.x, scrubRect.y, scrubRect.width * progress, scrubRect.height);
            EditorGUI.DrawRect(fillRect, fillColor);
            float handleX = scrubRect.x + scrubRect.width * progress;
            const float HANDLE_WIDTH = 3f;
            EditorGUI.DrawRect(new Rect(handleX - HANDLE_WIDTH * 0.5f, scrubRect.y, HANDLE_WIDTH, scrubRect.height), handleColor);

            return currentTime;
        }

        private static float SeekTimeFromMouse(Rect scrubRect, float duration, float mouseX)
        {
            float width = scrubRect.width;

            if (width <= 0.0001f) return 0f;

            float normalized = (mouseX - scrubRect.x) / width;

            if (normalized < 0f) normalized = 0f;
            else if (normalized > 1f) normalized = 1f;

            return normalized * duration;
        }

        private struct ConditionFieldsDrawContext
        {
            public float ZoomScale;
            public float ContentWidth;
            public GUIStyle LabelStyle;
            public GUIStyle PopupStyle;
            public GUIStyle TextFieldStyle;
            public GUIStyle ButtonStyle;
            public bool BlockInteraction;
            public bool RequireMouseButton;
            public Event CurrentEvent;
            public bool CompactOperatorLabels;
        }

        private bool DrawZoomScaledBoolToggle(Rect rect, bool value, float zoomScale, GUIStyle buttonStyle)
        {
            float size = Mathf.Min(rect.width, rect.height);

            if (size <= 0f) size = FIELD_ROW_HEIGHT * zoomScale;

            Rect boxRect = new Rect(rect.x, rect.y + (rect.height - size) * 0.5f, size, size);

            GUIStyle style = buttonStyle;

            if (style == null)
            {
                style = new GUIStyle(GUI.skin.button)
                {
                    fontSize = Mathf.Max(1, (int)(BASE_FONT_SIZE * zoomScale)),
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset(0, 0, 0, 0)
                };
            }

            string label = "";

            if (value) label = "✓";
            if (GUI.Button(boxRect, label, style)) return !value;

            return value;
        }

        private void DrawConditionOperatorValueUI(
            ConditionVariableType variableType,
            string variableName,
            ConditionOperator conditionOperator,
            bool boolValue,
            int intValue,
            float floatValue,
            string stringValue,
            Rect operatorFieldRect,
            Rect valueFieldRect,
            ConditionFieldsDrawContext ctx,
            Action<ConditionOperator> setOperator,
            Action<bool> setBool,
            Action<int> setInt,
            Action<float> setFloat,
            Action<string> setString,
            string undoOperatorLabel,
            string undoValueLabelPrefix,
            ConditionOperator[] allowedNumericOperators = null)
        {
            GUIStyle textFieldStyle = ctx.TextFieldStyle ?? EditorStyles.textField;

            if (ctx.BlockInteraction) GUI.enabled = false;
            else GUI.enabled = !string.IsNullOrEmpty(variableName);

            if (variableType == ConditionVariableType.Bool || variableType == ConditionVariableType.String)
            {
                string[] stringOperators;

                if (ctx.CompactOperatorLabels) stringOperators = new[] { "Equals", "Not Equals" };
                else stringOperators = new[] { "Equals", "NotEquals" };

                int opIndex;

                if (conditionOperator == ConditionOperator.Equals) opIndex = 0;
                else opIndex = 1;

                if (variableType == ConditionVariableType.String && conditionOperator != ConditionOperator.Equals && conditionOperator != ConditionOperator.NotEquals) opIndex = 0;

                int newOpIndex = EditorGUI.Popup(operatorFieldRect, opIndex, stringOperators, ctx.PopupStyle ?? EditorStyles.popup);

                if (newOpIndex != opIndex && (!ctx.RequireMouseButton || ctx.CurrentEvent.button == 0))
                {
                    Undo.RecordObject(database, undoOperatorLabel);
                    if (newOpIndex == 0) setOperator(ConditionOperator.Equals);
                    else setOperator(ConditionOperator.NotEquals);
                }
            }
            else
            {
                ConditionOperator[] selectableOperators = allowedNumericOperators ?? NumericConditionOperatorsAll;

                if (selectableOperators.Length == 0)
                {
                    string blockedOperatorLabel = conditionOperator.ToString();
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.Popup(operatorFieldRect, 0, new[] { blockedOperatorLabel }, ctx.PopupStyle ?? EditorStyles.popup);
                    EditorGUI.EndDisabledGroup();
                }
                else
                {
                    int operatorIndex = Array.IndexOf(selectableOperators, conditionOperator);

                    if (operatorIndex < 0)
                    {
                        operatorIndex = 0;

                        if (!string.IsNullOrEmpty(variableName) && Event.current.type == EventType.Layout) setOperator(selectableOperators[0]);
                    }

                    string[] operatorLabels = new string[selectableOperators.Length];

                    for (int i = 0; i < selectableOperators.Length; i++) 
                    {
                        operatorLabels[i] = selectableOperators[i].ToString();  
                    }

                    int newOperatorIndex = EditorGUI.Popup(operatorFieldRect, operatorIndex, operatorLabels, ctx.PopupStyle ?? EditorStyles.popup);

                    if (newOperatorIndex != operatorIndex && (!ctx.RequireMouseButton || ctx.CurrentEvent.button == 0))
                    {
                        Undo.RecordObject(database, undoOperatorLabel);
                        setOperator(selectableOperators[newOperatorIndex]);
                    }
                }
            }

            switch (variableType)
            {
                case ConditionVariableType.Bool:
                    bool newBoolValue = DrawZoomScaledBoolToggle(valueFieldRect, boolValue, ctx.ZoomScale, ctx.ButtonStyle);

                    if (newBoolValue != boolValue && (!ctx.RequireMouseButton || ctx.CurrentEvent.button == 0))
                    {
                        Undo.RecordObject(database, undoValueLabelPrefix + " Bool Value");
                        setBool(newBoolValue);
                    }

                    break;
                case ConditionVariableType.Int:
                    EditorGUI.BeginChangeCheck();
                    Undo.RecordObject(database, undoValueLabelPrefix + " Int Value");
                    int newIntValue = EditorGUI.IntField(valueFieldRect, intValue, textFieldStyle);
                    if (EditorGUI.EndChangeCheck() && (!ctx.RequireMouseButton || ctx.CurrentEvent.button == 0)) setInt(newIntValue);
                    break;
                case ConditionVariableType.Float:
                    EditorGUI.BeginChangeCheck();
                    Undo.RecordObject(database, undoValueLabelPrefix + " Float Value");
                    float newFloatValue = EditorGUI.FloatField(valueFieldRect, floatValue, textFieldStyle);
                    if (EditorGUI.EndChangeCheck() && (!ctx.RequireMouseButton || ctx.CurrentEvent.button == 0)) setFloat(newFloatValue);
                    break;
                case ConditionVariableType.String:
                    EditorGUI.BeginChangeCheck();
                    Undo.RecordObject(database, undoValueLabelPrefix + " String Value");
                    string newStringValue = EditorGUI.TextField(valueFieldRect, stringValue ?? "", textFieldStyle);
                    if (EditorGUI.EndChangeCheck() && (!ctx.RequireMouseButton || ctx.CurrentEvent.button == 0)) setString(newStringValue);
                    break;
            }

            GUI.enabled = true;
        }

        private void DrawDialogConditionEditableFields(
            DialogCondition condition,
            int conditionIndex,
            IReadOnlyList<DialogCondition> siblingConditions,
            ref float fieldY,
            ConditionFieldsDrawContext ctx)
        {
            float zs = ctx.ZoomScale;
            GUIStyle labelStyle = ctx.LabelStyle ?? EditorStyles.label;
            float fieldWidth = ctx.ContentWidth - CONDITION_FIELD_WIDTH_OFFSET * zs;
            EnsureDialogConditionVariableType(condition);
            string[] variableNamesForType = GetVariableNamesByType(condition.VariableType);
            GUI.Label(new Rect(CONDITION_LABEL_X * zs, fieldY, CONDITION_LABEL_WIDTH * zs, FIELD_ROW_HEIGHT * zs), "Type:", labelStyle);

            DrawVariableTypeDropdown(new Rect(CONDITION_FIELD_OFFSET * zs, fieldY, fieldWidth, FIELD_ROW_HEIGHT * zs), condition.VariableType, newVariableType =>
                {
                    Undo.RecordObject(database, "Change Condition Type");
                    ApplyDialogConditionVariableTypeChange(condition, newVariableType);
                },
            ctx.PopupStyle);

            fieldY += ACTION_ROW_HEIGHT * zs;

            GUI.Label(new Rect(CONDITION_LABEL_X * zs, fieldY, CONDITION_LABEL_WIDTH * zs, FIELD_ROW_HEIGHT * zs), "Name:", labelStyle);

            string newVarName = DrawVariableNamePopup(
                new Rect(CONDITION_FIELD_OFFSET * zs, fieldY, fieldWidth, FIELD_ROW_HEIGHT * zs),
                variableNamesForType,
                condition.VariableName,
                ctx.PopupStyle,
            CollectUsedVariableNames(siblingConditions, conditionIndex));

            if (newVarName != condition.VariableName)
            {
                Undo.RecordObject(database, "Change Condition Variable");
                condition.VariableName = newVarName;
                InvalidateReferenceIndex();
            }

            fieldY += ACTION_ROW_HEIGHT * zs;
            GUI.Label(new Rect(CONDITION_LABEL_X * zs, fieldY, CONDITION_LABEL_WIDTH * zs, FIELD_ROW_HEIGHT * zs), "Operator:", labelStyle);
            Rect operatorFieldRect = new Rect(CONDITION_FIELD_OFFSET * zs, fieldY, fieldWidth, FIELD_ROW_HEIGHT * zs);
            fieldY += ACTION_ROW_HEIGHT * zs;
            GUI.Label(new Rect(CONDITION_LABEL_X * zs, fieldY, CONDITION_LABEL_WIDTH * zs, FIELD_ROW_HEIGHT * zs), "Value:", labelStyle);
            Rect valueFieldRect = new Rect(CONDITION_FIELD_OFFSET * zs, fieldY, fieldWidth, FIELD_ROW_HEIGHT * zs);

            DrawConditionOperatorValueUI(
                condition.VariableType,
                condition.VariableName,
                condition.ConditionOperator,
                condition.BoolValue,
                condition.IntValue,
                condition.FloatValue,
                condition.StringValue,
                operatorFieldRect,
                valueFieldRect,
                ctx,
                op => condition.ConditionOperator = op,
                val => condition.BoolValue = val,
                val => condition.IntValue = val,
                val => condition.FloatValue = val,
                val => condition.StringValue = val,
                "Change Condition Operator",
                "Change Condition",
                GetAllowedNumericConditionOperators(siblingConditions, conditionIndex, condition.VariableName));

            fieldY += ACTION_ROW_HEIGHT * zs;
        }

        private void DrawChoiceConditionEditableFields(ChoiceCondition condition, int conditionIndex, IReadOnlyList<ChoiceCondition> siblingConditions, float fieldY, ref float fieldX, ConditionFieldsDrawContext ctx)
        {
            float zs = ctx.ZoomScale;
            GUIStyle labelStyle = ctx.LabelStyle ?? EditorStyles.label;

            EnsureChoiceConditionVariableType(condition);

            GUI.Label(new Rect(fieldX, fieldY, CHOICE_EVENT_TYPE_LABEL_WIDTH * zs, FIELD_ROW_HEIGHT * zs), "Type:", labelStyle);
            fieldX += CHOICE_EVENT_TYPE_LABEL_WIDTH * zs;

            DrawVariableTypeDropdown(
                new Rect(fieldX, fieldY, CHOICE_EVENT_TYPE_POPUP_WIDTH * zs, FIELD_ROW_HEIGHT * zs),
                condition.VariableType,
                newType =>
                {
                    Undo.RecordObject(database, "Change Variable Type");
                    ApplyChoiceConditionVariableTypeChange(condition, newType);
                },
                ctx.PopupStyle);

            fieldX += (CHOICE_EVENT_TYPE_POPUP_WIDTH + CHOICE_EVENT_VAR_SPACING)  * zs;

            GUI.Label(new Rect(fieldX, fieldY, CHOICE_EVENT_NAME_LABEL_WIDTH * zs, FIELD_ROW_HEIGHT * zs), "Name:", labelStyle);
            fieldX += CHOICE_EVENT_NAME_LABEL_WIDTH * zs;
            string[] varNames = GetVariableNamesByType(condition.VariableType);

            string newCondVarName = DrawVariableNamePopup(
                new Rect(fieldX, fieldY, CHOICE_EVENT_VAR_COND_POPUP_WIDTH * zs, FIELD_ROW_HEIGHT * zs),
                varNames,
                condition.VariableName,
                ctx.PopupStyle,
                CollectUsedVariableNames(siblingConditions, conditionIndex));

            fieldX += (CHOICE_EVENT_VAR_COND_POPUP_WIDTH + CHOICE_EVENT_VAR_SPACING) * zs;
            
            if (newCondVarName != condition.VariableName)
            {
                Undo.RecordObject(database, "Change Variable Name");
                condition.VariableName = newCondVarName;
                InvalidateReferenceIndex();
            }


            Rect operatorFieldRect = new Rect(fieldX, fieldY, CHOICE_EVENT_OP_POPUP_WIDTH * zs, FIELD_ROW_HEIGHT * zs);
            fieldX += (CHOICE_EVENT_OP_POPUP_WIDTH + CHOICE_EVENT_VAR_SPACING) * zs;
            Rect valueFieldRect = new Rect(fieldX, fieldY, CHOICE_EVENT_VALUE_FIELD_WIDTH * zs, FIELD_ROW_HEIGHT * zs);

            if (condition.VariableType == ConditionVariableType.Bool) valueFieldRect = new Rect(fieldX, fieldY, FIELD_ROW_HEIGHT * zs, FIELD_ROW_HEIGHT * zs);

            DrawConditionOperatorValueUI(
                condition.VariableType,
                condition.VariableName,
                condition.ConditionOperator,
                condition.BoolValue,
                condition.IntValue,
                condition.FloatValue,
                condition.StringValue,
                operatorFieldRect,
                valueFieldRect,
                ctx,
                op => condition.ConditionOperator = op,
                val => condition.BoolValue = val,
                val => condition.IntValue = val,
                val => condition.FloatValue = val,
                val => condition.StringValue = val,
                "Change Condition Operator",
                "Change",
                GetAllowedNumericConditionOperators(siblingConditions, conditionIndex, condition.VariableName));

            fieldX += CHOICE_EVENT_VALUE_FIELD_WIDTH * zs;
        }

        private static DialogCondition CopyDialogCondition(DialogCondition source)
        {
            if (source == null) return new DialogCondition();

            return new DialogCondition
            {
                VariableName = source.VariableName,
                VariableType = source.VariableType,
                ConditionOperator = source.ConditionOperator,
                BoolValue = source.BoolValue,
                IntValue = source.IntValue,
                FloatValue = source.FloatValue,
                StringValue = source.StringValue,
                ReferenceState = source.ReferenceState,
                MissingReferenceLabel = source.MissingReferenceLabel
            };
        }

        private static ChoiceCondition CopyChoiceCondition(ChoiceCondition source)
        {
            if (source == null) return new ChoiceCondition();

            return new ChoiceCondition
            {
                VariableType = source.VariableType,
                VariableName = source.VariableName,
                ConditionOperator = source.ConditionOperator,
                BoolValue = source.BoolValue,
                IntValue = source.IntValue,
                FloatValue = source.FloatValue,
                StringValue = source.StringValue,
                ReferenceState = source.ReferenceState,
                MissingReferenceLabel = source.MissingReferenceLabel
            };
        }

        private static ChoiceConsequence CopyChoiceConsequence(ChoiceConsequence source)
        {
            if (source == null) return new ChoiceConsequence();

            return new ChoiceConsequence
            {
                VariableType = source.VariableType,
                VariableName = source.VariableName,
                BoolValue = source.BoolValue,
                IntValue = source.IntValue,
                FloatValue = source.FloatValue,
                StringValue = source.StringValue,
                ReferenceState = source.ReferenceState,
                MissingReferenceLabel = source.MissingReferenceLabel
            };
        }

        private static CharacterActionData CopyCharacterActionData(CharacterActionData source)
        {
            if (source == null) return new CharacterActionData();

            return new CharacterActionData
            {
                CharacterName = source.CharacterName,
                EmotionName = source.EmotionName,
                Action = source.Action,
                PosX = source.PosX,
                PosY = source.PosY,
                Zoom = source.Zoom,
                ReferenceState = source.ReferenceState,
                MissingReferenceLabel = source.MissingReferenceLabel
            };
        }

        private bool TryRemoveCharacterNameColorFromRegistry(int index, CharacterNameColor colorEntry)
        {
            if (colorEntry == null || database?.CharacterNameColors == null) return false;
            if (index < 0 || index >= database.CharacterNameColors.Count) return false;

            string name = colorEntry.Name;
            DialogReferenceIndex referenceIndex = GetLiveDialogReferenceIndex();
            IReadOnlyList<DialogReferenceUsage> usages = referenceIndex.GetLineCharacterNameUsages(name);
            IReadOnlyList<DialogReferenceUsage> savedUsages = _savedDialogReferenceIndex.GetLineCharacterNameUsages(name);

            string message;
            if (usages.Count > 0) message = BuildRegistryDeleteUsageMessage(usages, savedUsages, "The name color will be removed from the registry, and references in lines will be marked as missing.");
            else message = $"Delete name color '{name}'?";

            if (!EditorUtility.DisplayDialog("Delete name color", message, "Delete", "Cancel")) return false;

            if (usages.Count > 0)
            {
                Undo.RecordObject(database, "Mark Missing Line Character Name References");
                DialogReferenceCleanup.MarkLineCharacterNameMissingInDialogs(database, name);
                SaveRegistryDialogGraphPatch(
                    "Delete name color - mark missing line references",
                    () => DialogReferenceCleanup.MarkLineCharacterNameMissingInDialogs(database, name));
            }

            Undo.RecordObject(database, "Remove Character Name Color");
            database.CharacterNameColors.RemoveAt(index);
            SaveDialogRegistry("Delete name color");
            InvalidateReferenceIndex();
            return true;
        }



        private void DrawVariableTypeDropdown(Rect rect, ConditionVariableType currentType, Action<ConditionVariableType> onTypeChanged, GUIStyle style = null)
        {
            if (EditorGUI.DropdownButton(rect, new GUIContent(currentType.ToString()), FocusType.Passive, style ?? EditorStyles.popup))
            {
                GenericMenu menu = new GenericMenu();
                foreach (ConditionVariableType t in Enum.GetValues(typeof(ConditionVariableType)))
                {
                    int count = GetVariableNamesByType(t).Length;
                    if (count > 0)
                    {
                        ConditionVariableType typeCopy = t;
                        menu.AddItem(new GUIContent(t.ToString()), t == currentType, () => onTypeChanged(typeCopy));
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent(t.ToString() + NO_VARS_HINT));
                    }
                }
                menu.DropDown(rect);
            }
        }

        #endregion Variable Helpers (Graph and Editor)


        #region Dialog Editor (Graph)

        private void DrawDialogEditor()
        {
            const int TITLE_LABEL_WIDTH = 80;
            const int DIALOG_NAME_FIELD_WIDTH = 500;
            const int ADD_BUTTON_GAP = 10;
            const int ADD_CHOICE_BUTTON_OFFSET = ADD_BUTTON_GAP + ADD_LINE_BUTTON_WIDTH;
            const int ADD_CONDITION_BUTTON_OFFSET = ADD_CHOICE_BUTTON_OFFSET + ADD_BUTTON_GAP + ADD_LINE_BUTTON_WIDTH;
            const int ADD_REDIRECT_BUTTON_OFFSET = ADD_CONDITION_BUTTON_OFFSET + ADD_BUTTON_GAP + ADD_LINE_BUTTON_WIDTH;
            const int LINES_HEADER_WIDTH = ADD_REDIRECT_BUTTON_OFFSET + ADD_LINE_BUTTON_WIDTH;
            const int EDITOR_HEADER_SPACE = 10;
            const int INITIATOR_LABEL_WIDTH = 75;
            const int INITIATOR_POPUP_WIDTH = 180;
            const int BUTTON_VALIDATE_LINKS_WIDTH = 150;
            const int BUTTON_VALIDATE_START_CONDITIONS_WIDTH = 150;
            const int BUTTON_GO_TO_START_WIDTH = 130;
            const int BUTTON_SAVE_WIDTH = 90;
            const int AUTO_SAVE_TOGGLE_WIDTH = 52;
            const int AUTO_SAVE_INTERVAL_WIDTH = 40;
            const int AUTO_SAVE_SUFFIX_WIDTH = 24;

            Event e = Event.current;
            graphPortClickHandled = false;

            if (selectedDialog == null)
            {
                Debug.LogWarning("Selected dialog is missing, returning to the list");
                isEditingDialog = false;
                return;
            }

            if (editorOffset == Vector2.zero)
            {
                var startNode = selectedDialog.nodes.Find(n => n.NodeType == NodeType.Start);
                if (startNode != null) editorOffset = startNode.Position - new Vector2(DEFAULT_POSITION_XY, DEFAULT_POSITION_XY);
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Height(ASSET_ACTION_ROW_HEIGHT));

            if (GUILayout.Button(new GUIContent("← Back to list", "Return to the dialog list"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_BACK_WIDTH)))
            {
                if (TryExitDialogEditorToList()) GUIUtility.ExitGUI();
            }

            if (GUILayout.Button(new GUIContent("Show conditions", "Open or close the playback conditions window"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_BACK_WIDTH))) showConditions = !showConditions;

            if (GUILayout.Button(new GUIContent("Check integrity", "Find and fix broken links to non-existent nodes"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_VALIDATE_LINKS_WIDTH))) ValidateBrokenLinks();

            if (GUILayout.Button(new GUIContent("Check conditions", "Find overlaps of start conditions in the current category"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_VALIDATE_START_CONDITIONS_WIDTH))) ValidateCurrentCategoryStartConditions();

            if (GUILayout.Button(new GUIContent("Go to start", "Center view on the start node"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_GO_TO_START_WIDTH)))
            {
                var startNode = selectedDialog.nodes.Find(n => n.NodeType == NodeType.Start);
                if (startNode != null) editorOffset = startNode.Position - new Vector2(DEFAULT_POSITION_XY, DEFAULT_POSITION_XY);
            }

            if (GUILayout.Button(new GUIContent("Save", "Save all changes to the dialog and databases"), EditorStyles.toolbarButton, GUILayout.Width(BUTTON_SAVE_WIDTH))) SaveDialogEditorSession("Toolbar Save button");

            EditorGUI.BeginChangeCheck();
            bool newAutoSaveEnabled = EditorGUILayout.ToggleLeft("Auto", _dialogEditorAutoSaveEnabled, GUILayout.Width(AUTO_SAVE_TOGGLE_WIDTH));
            GUI.enabled = newAutoSaveEnabled;
            int newAutoSaveMinutes = EditorGUILayout.IntPopup(_dialogEditorAutoSaveIntervalMinutes, AUTO_SAVE_MINUTE_LABELS, AUTO_SAVE_MINUTE_VALUES, EditorStyles.toolbarPopup, GUILayout.Width(AUTO_SAVE_INTERVAL_WIDTH));
            GUI.enabled = true;
            GUILayout.Label("min", EditorStyles.miniLabel, GUILayout.Width(AUTO_SAVE_SUFFIX_WIDTH));

            if (EditorGUI.EndChangeCheck())
            {
                if (newAutoSaveEnabled != _dialogEditorAutoSaveEnabled || newAutoSaveMinutes != _dialogEditorAutoSaveIntervalMinutes) _lastDialogEditorAutoSaveTime = -1;
                _dialogEditorAutoSaveEnabled = newAutoSaveEnabled;
                _dialogEditorAutoSaveIntervalMinutes = newAutoSaveMinutes;
                EditorPrefs.SetBool(PREFS_DIALOG_EDITOR_AUTO_SAVE_KEY, _dialogEditorAutoSaveEnabled);
                EditorPrefs.SetInt(PREFS_DIALOG_EDITOR_AUTO_SAVE_MINUTES_KEY, _dialogEditorAutoSaveIntervalMinutes);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(EDITOR_SPACE_MEDIUM);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(EDITOR_SPACE_SMALL);
            GUILayout.Label("Name:", EditorStyles.boldLabel, GUILayout.Width(TITLE_LABEL_WIDTH));
            SyncPendingDialogNameEdit();
            GUI.SetNextControlName(DIALOG_NAME_FIELD_CONTROL);
            _pendingDialogNameEdit = EditorGUILayout.TextField(_pendingDialogNameEdit ?? "", GUILayout.Width(DIALOG_NAME_FIELD_WIDTH));

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape && GUI.GetNameOfFocusedControl() == DIALOG_NAME_FIELD_CONTROL)
            {
                _pendingDialogNameEdit = selectedDialog.dialogName ?? "";
                GUI.FocusControl(null);
                Event.current.Use();
            }

            GUILayout.Space(EDITOR_SPACE_MEDIUM);
            Rect linesHeaderRect = EditorGUILayout.GetControlRect(false, GUILayout.Width(LINES_HEADER_WIDTH));

            if (GUI.Button(new Rect(linesHeaderRect.x, linesHeaderRect.y - LINES_HEADER_BUTTON_OFFSET, ADD_LINE_BUTTON_WIDTH, ADD_LINE_BUTTON_HEIGHT), "+ Add line")) AddLineNode(GetToolbarNewNodePosition());
            if (GUI.Button(new Rect(linesHeaderRect.x + ADD_CHOICE_BUTTON_OFFSET, linesHeaderRect.y - LINES_HEADER_BUTTON_OFFSET, ADD_LINE_BUTTON_WIDTH, ADD_LINE_BUTTON_HEIGHT), "+ Add choice")) AddChoiceNode(GetToolbarNewNodePosition());
            if (GUI.Button(new Rect(linesHeaderRect.x + ADD_CONDITION_BUTTON_OFFSET, linesHeaderRect.y - LINES_HEADER_BUTTON_OFFSET, ADD_LINE_BUTTON_WIDTH, ADD_LINE_BUTTON_HEIGHT), "+ Add condition")) AddConditionNode(GetToolbarNewNodePosition());
            if (GUI.Button(new Rect(linesHeaderRect.x + ADD_REDIRECT_BUTTON_OFFSET, linesHeaderRect.y - LINES_HEADER_BUTTON_OFFSET, ADD_LINE_BUTTON_WIDTH, ADD_LINE_BUTTON_HEIGHT), "+ Add Redirect")) AddRedirectNode(GetToolbarNewNodePosition());

            GUILayout.Space(EDITOR_HEADER_SPACE);
            GUILayout.Label("Initiator:", EditorStyles.boldLabel, GUILayout.Width(INITIATOR_LABEL_WIDTH));
            int initiatorIndex = dialogCategories.IndexOf(selectedDialog.initiator ?? DEFAULT_CATEGORY_NAME);

            if (initiatorIndex < 0) initiatorIndex = 0;

            int newInitiatorIndex = EditorGUILayout.Popup(initiatorIndex, dialogCategories.ToArray(), GUILayout.Width(INITIATOR_POPUP_WIDTH));

            if (newInitiatorIndex != initiatorIndex)
            {
                Undo.RecordObject(database, "Change Dialog Initiator");
                selectedDialog.initiator = dialogCategories[newInitiatorIndex];
                // SaveDialogGraphChanges();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EDITOR_SPACE_MEDIUM);
            EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));

            if (selectedDialog.nodes == null) selectedDialog.nodes = new List<DialogNode>();

            editorRect = GUILayoutUtility.GetRect(0, 0, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            Rect conditionsRect = new Rect(editorRect.x, editorRect.y, CONDITION_NODE_WIDTH, editorRect.height);
            GUI.BeginGroup(editorRect);
            Rect bgRect = new Rect(0, 0, editorRect.width, editorRect.height);

            if (gridTexture != null)
            {
                float scaledTileSize = GRID_TILE_SIZE * zoomScale;
                GUI.DrawTextureWithTexCoords(bgRect, gridTexture, new Rect(editorOffset.x * zoomScale / scaledTileSize, -editorOffset.y * zoomScale / scaledTileSize, editorRect.width / scaledTileSize, editorRect.height / scaledTileSize));
            }

            Handles.BeginGUI();
            Vector2 graphLocalMouse = e.mousePosition; // Inside GUI.BeginGroup + Handles.BeginGUI coordinates are already in graph local space.

            foreach (var node in selectedDialog.nodes)
            {
                if (node == null) continue;

                Vector2 nodeOutputPos = (node.Position - editorOffset) * zoomScale + new Vector2(GetScaledNodeInputOffset(node), GetNodeHeight(node) * zoomScale);

                if (node.NodeType == NodeType.Line)
                {
                    if (node.LineData != null && !string.IsNullOrEmpty(node.LineData.NextNodeId))
                    {
                        var nextNode = selectedDialog.nodes.Find(n => n != null && n.nodeId == node.LineData.NextNodeId);

                        if (nextNode != null)
                        {
                            Vector2 nextNodeInputPos = GetNodeInputPosition(nextNode);
                            DrawNodeConnection(nodeOutputPos, nextNodeInputPos);
                        }
                    }
                }
                else if (node.NodeType == NodeType.Start)
                {
                    if (node.StartData != null && !string.IsNullOrEmpty(node.StartData.NextNodeId))
                    {
                        var nextNode = selectedDialog.nodes.Find(n => n != null && n.nodeId == node.StartData.NextNodeId);

                        if (nextNode != null)
                        {
                            Vector2 nextNodeInputPos = GetNodeInputPosition(nextNode);
                            DrawNodeConnection(nodeOutputPos, nextNodeInputPos);
                        }
                    }
                }
                else if (node.NodeType == NodeType.Choice)
                {
                    if (node.ChoiceData != null)
                    {
                        float choiceY = CHOICE_NODE_EXIT_SPACING;

                        for (int i = 0; i < node.ChoiceData.Count; i++)
                        {
                            if (node.ChoiceData[i] == null) continue;

                            var nextNode = selectedDialog.nodes.Find(n => n != null && n.nodeId == node.ChoiceData[i].NextNodeId);

                            if (nextNode != null)
                            {
                                Vector2 choicePort = (node.Position - editorOffset) * zoomScale + new Vector2(BASE_NODE_WIDTH * zoomScale, choiceY * zoomScale);
                                Vector2 nextNodeInputPos = GetNodeInputPosition(nextNode);
                                DrawNodeConnection(choicePort, nextNodeInputPos);
                            }

                            choiceY += GetChoiceHeight(node.ChoiceData[i], node.nodeId);
                        }
                    }
                }
                else if (node.NodeType == NodeType.Condition)
                {
                    if (node.ConditionData != null)
                    {
                        if (!string.IsNullOrEmpty(node.ConditionData.TrueNodeId))
                        {
                            var trueNode = selectedDialog.nodes.Find(n => n != null && n.nodeId == node.ConditionData.TrueNodeId);

                            if (trueNode != null)
                            {
                                Vector2 truePort = (node.Position - editorOffset) * zoomScale + new Vector2(BASE_NODE_WIDTH * zoomScale, CONDITION_NODE_EXIT_SPACING * zoomScale);
                                Vector2 nextNodeInputPos = GetNodeInputPosition(trueNode);
                                DrawNodeConnection(truePort, nextNodeInputPos);
                            }
                        }

                        if (!string.IsNullOrEmpty(node.ConditionData.FalseNodeId))
                        {
                            var falseNode = selectedDialog.nodes.Find(n => n != null && n.nodeId == node.ConditionData.FalseNodeId);

                            if (falseNode != null)
                            {
                                Vector2 falsePort = (node.Position - editorOffset) * zoomScale + new Vector2(BASE_NODE_WIDTH * zoomScale, (CONDITION_NODE_EXIT_SPACING + NODE_SPACE_OFFSET + NODE_PORT_SIZE) * zoomScale);
                                Vector2 nextNodeInputPos = GetNodeInputPosition(falseNode);
                                DrawNodeConnection(falsePort, nextNodeInputPos);
                            }
                        }
                    }
                }
            }

            if (selectedNodes.Count == 0)
            {
                for (int i = 0; i < selectedDialog.nodes.Count; i++)
                {
                    var node = selectedDialog.nodes[i];
                    
                    if (node == null) continue;

                    float nodeHeight = Mathf.Max(GetNodeHeight(node), MIN_NODE_HEIGHT) * zoomScale;
                    bool isInputConnected = selectedDialog.nodes.Any(n => n != null &&
                    (
                        (n.NodeType == NodeType.Line && n.LineData != null && n.LineData.NextNodeId == node.nodeId) ||
                        (n.NodeType == NodeType.Choice && n.ChoiceData != null && n.ChoiceData.Any(c => c != null && c.NextNodeId == node.nodeId)) ||
                        (n.NodeType == NodeType.Start && n.StartData != null && n.StartData.NextNodeId == node.nodeId) ||
                        (n.NodeType == NodeType.Condition && n.ConditionData != null && (n.ConditionData.TrueNodeId == node.nodeId || n.ConditionData.FalseNodeId == node.nodeId)))
                    );

                    Rect inputPort;
                    Vector2 inputPos;

                    if (node.NodeType == NodeType.Line)
                    {
                        inputPos = (node.Position - editorOffset) * zoomScale + new Vector2(INPUT_OFFSET_LINE * zoomScale, 0);
                        inputPort = new Rect(inputPos.x - PORT_HALF_SIZE * zoomScale, inputPos.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);
                    }
                    else if (node.NodeType == NodeType.Choice)
                    {
                        inputPos = (node.Position - editorOffset) * zoomScale + new Vector2(0, CHOICE_NODE_ENTER_SPACING * zoomScale);
                        inputPort = new Rect(inputPos.x - PORT_HALF_SIZE * zoomScale, inputPos.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);
                    }
                    else if (node.NodeType == NodeType.Condition)
                    {
                        inputPos = (node.Position - editorOffset) * zoomScale + new Vector2(0, CONDITION_NODE_ENTER_SPACING * zoomScale);
                        inputPort = new Rect(inputPos.x - PORT_HALF_SIZE * zoomScale, inputPos.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);
                    }
                    else if (node.NodeType == NodeType.Redirect)
                    {
                        inputPos = (node.Position - editorOffset) * zoomScale + new Vector2(0, REDIRECT_NODE_ENTER_SPACING * zoomScale);
                        inputPort = new Rect(inputPos.x - PORT_HALF_SIZE * zoomScale, inputPos.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);
                    }
                    else
                    {
                        inputPos = Vector2.zero;
                        inputPort = Rect.zero;
                    }

                    if (node.NodeType != NodeType.Start && e.type == EventType.MouseDown && e.button == 0 && inputPort.Contains(graphLocalMouse))
                    {
                        graphPortClickHandled = true;
                        if (TryDeselectSamePortClick(node, true))
                        {
                            GUI.changed = true;
                            e.Use();
                        }
                        else if (TryCompleteConnectionByClick(node, true, -1)) e.Use();
                        else if (isInputConnected)
                        {
                            Undo.RecordObject(database, "Disconnect Input Port");
                            DisconnectIncomingLinksToNode(selectedDialog, node.nodeId);

                            selectedPorts.Clear();
                            pendingPort = null;
                            GUI.changed = true;
                        }
                        else
                        {
                            selectedPorts.Clear();
                            selectedPorts.Add(new SelectedPort(node, true));
                            pendingPort = new SelectedPort(node, true);
                            GUI.changed = true;
                        }

                        e.Use();
                    }

                    if (node.NodeType == NodeType.Line || node.NodeType == NodeType.Start)
                    {
                        if (node.NodeType == NodeType.Start && e.type == EventType.MouseDown && e.button == 0 && pendingPort != null && pendingPort.IsInput && pendingPort.Node != node)
                        {
                            Vector2 startTopLeft = (node.Position - editorOffset) * zoomScale;
                            Rect startNodeRect = new Rect(startTopLeft.x, startTopLeft.y, GetScaledNodeWidth(node), nodeHeight);
                            if (startNodeRect.Contains(graphLocalMouse))
                            {
                                graphPortClickHandled = true;
                                if (TryCompleteConnectionByClick(node, false, -1))
                                {
                                    e.Use();
                                    continue;
                                }
                            }
                        }

                        Vector2 outputPos = (node.Position - editorOffset) * zoomScale + new Vector2(GetScaledNodeInputOffset(node), nodeHeight);
                        Rect outputPort = new Rect(outputPos.x - PORT_HALF_SIZE * zoomScale, outputPos.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);

                        if (e.type == EventType.MouseDown && e.button == 0 && outputPort.Contains(graphLocalMouse))
                        {
                            graphPortClickHandled = true;
                            if (TryDeselectSamePortClick(node, false))
                            {
                                GUI.changed = true;
                                e.Use();
                            }
                            else if (TryCompleteConnectionByClick(node, false, -1)) e.Use();
                            else if ((node.NodeType == NodeType.Line && node.LineData != null && !string.IsNullOrEmpty(node.LineData.NextNodeId)) || 
                            (node.NodeType == NodeType.Start && node.StartData != null && !string.IsNullOrEmpty(node.StartData.NextNodeId)))
                            {
                                Undo.RecordObject(database, "Disconnect Output Port");
                                DisconnectPort(node, -1);
                                selectedPorts.Clear();
                                pendingPort = null;
                                GUI.changed = true;
                                e.Use();
                            }
                            else
                            {
                                selectedPorts.Clear();
                                selectedPorts.Add(new SelectedPort(node, false));
                                pendingPort = new SelectedPort(node, false);
                                GUI.changed = true;
                                e.Use();
                            }
                        }
                    }
                    else if (node.NodeType == NodeType.Choice)
                    {
                        if (node.ChoiceData == null) continue;

                        float choiceY = CHOICE_NODE_EXIT_SPACING;

                        for (int j = 0; j < node.ChoiceData.Count; j++)
                        {
                            if (node.ChoiceData[j] == null) continue;

                            Vector2 choicePort = (node.Position - editorOffset) * zoomScale + new Vector2(BASE_NODE_WIDTH * zoomScale, choiceY * zoomScale);
                            Rect choiceOutputPort = new Rect(choicePort.x - PORT_HALF_SIZE * zoomScale, choicePort.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);

                            if (e.type == EventType.MouseDown && e.button == 0 && choiceOutputPort.Contains(graphLocalMouse))
                            {
                                graphPortClickHandled = true;

                                if (TryDeselectSamePortClick(node, false, j))
                                {
                                    GUI.changed = true;
                                    e.Use();
                                }
                                else if (TryCompleteConnectionByClick(node, false, j)) e.Use();                   
                                else if (node.ChoiceData[j] != null && !string.IsNullOrEmpty(node.ChoiceData[j].NextNodeId))
                                {
                                    Undo.RecordObject(database, "Disconnect Output Port");
                                    DisconnectPort(node, j);
                                    selectedPorts.Clear();
                                    pendingPort = null;
                                    GUI.changed = true;
                                    e.Use();
                                }
                                else
                                {
                                    selectedPorts.Clear();
                                    selectedPorts.Add(new SelectedPort(node, false, j));
                                    pendingPort = new SelectedPort(node, false, j);
                                    GUI.changed = true;
                                    e.Use();
                                }
                            }

                            choiceY += GetChoiceHeight(node.ChoiceData[j], node.nodeId);
                        }
                    }
                    else if (node.NodeType == NodeType.Condition)
                    {
                        Vector2 truePort = (node.Position - editorOffset) * zoomScale + new Vector2(BASE_NODE_WIDTH * zoomScale, CONDITION_NODE_EXIT_SPACING * zoomScale);
                        Rect trueOutputPort = new Rect(truePort.x - PORT_HALF_SIZE * zoomScale, truePort.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);

                        if (e.type == EventType.MouseDown && e.button == 0 && trueOutputPort.Contains(graphLocalMouse))
                        {
                            graphPortClickHandled = true;

                            if (TryDeselectSamePortClick(node, false, 0))
                            {
                                GUI.changed = true;
                                e.Use();
                            }
                            else if (TryCompleteConnectionByClick(node, false, 0)) e.Use();      
                            else if (node.ConditionData != null && !string.IsNullOrEmpty(node.ConditionData.TrueNodeId))
                            {
                                Undo.RecordObject(database, "Disconnect Output Port");
                                DisconnectPort(node, 0);
                                selectedPorts.Clear();
                                pendingPort = null;
                                GUI.changed = true;
                                e.Use();
                            }
                            else
                            {
                                selectedPorts.Clear();
                                selectedPorts.Add(new SelectedPort(node, false, 0));
                                pendingPort = new SelectedPort(node, false, 0);
                                GUI.changed = true;
                                e.Use();
                            }
                        }

                        Vector2 falsePort = (node.Position - editorOffset) * zoomScale + new Vector2(BASE_NODE_WIDTH * zoomScale, (CONDITION_NODE_EXIT_SPACING + NODE_SPACE_OFFSET + NODE_PORT_SIZE) * zoomScale);
                        Rect falseOutputPort = new Rect(falsePort.x - PORT_HALF_SIZE * zoomScale, falsePort.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);

                        if (e.type == EventType.MouseDown && e.button == 0 && falseOutputPort.Contains(graphLocalMouse))
                        {
                            graphPortClickHandled = true;

                            if (TryDeselectSamePortClick(node, false, 1))
                            {
                                GUI.changed = true;
                                e.Use();
                            }
                            else if (TryCompleteConnectionByClick(node, false, 1)) e.Use();    
                            else if (node.ConditionData != null && !string.IsNullOrEmpty(node.ConditionData.FalseNodeId))
                            {
                                Undo.RecordObject(database, "Disconnect Output Port");
                                DisconnectPort(node, 1);
                                selectedPorts.Clear();
                                pendingPort = null;
                                GUI.changed = true;
                                e.Use();
                            }
                            else
                            {
                                selectedPorts.Clear();
                                selectedPorts.Add(new SelectedPort(node, false, 1));
                                pendingPort = new SelectedPort(node, false, 1);
                                GUI.changed = true;
                                e.Use();
                            }
                        }
                    }
                }
            }

            if (e.type == EventType.MouseDown && e.button == 0 && pendingPort != null && !graphPortClickHandled && !IsGraphLocalMouseOverAnyNode(graphLocalMouse))
            {
                ResetGraphInputFocus();
                selectedPorts.Clear();
                pendingPort = null;
                copiedActionData = null;
                copiedActionSourceNodeId = null;
                selectedNodes.Clear();
                isDraggingSelection = false;
                selectionDragStartNodePositions = null;
                graphPortClickHandled = true;
                e.Use();
            }

            bool inSelectionZone = selectedNodes.Count > 1 && !isBoxSelecting
                && TryGetNodeSelectionOuterRectLocal(out Rect selectionOuterLocal)
                && selectionOuterLocal.Contains(graphLocalMouse);

            if (inSelectionZone && e.type == EventType.MouseDown && e.button == 0 && !graphPortClickHandled)
            {
                BeginSelectionDrag(e);
                graphPortClickHandled = true;
                e.Use();
            }

            HandleSelectionDragInput(e);

            BeginWindows();

            for (int i = 0; i < selectedDialog.nodes.Count; i++)
            {
                var node = selectedDialog.nodes[i];

                if (node == null) continue;

                float baseNodeHeight = GetNodeHeight(node);
                float minHeight = MIN_NODE_HEIGHT;
                float nodeHeight = Mathf.Max(baseNodeHeight, minHeight) * zoomScale;
                float nodeWidth = GetScaledNodeWidth(node);
                Rect nodeRect = new Rect((node.Position.x - editorOffset.x) * zoomScale, (node.Position.y - editorOffset.y) * zoomScale, nodeWidth, nodeHeight);

                float headerHeight = NODE_TOOLBAR_HEIGHT * zoomScale;
                Rect headerRect = new Rect(nodeRect.x, nodeRect.y, nodeWidth, headerHeight);
                bool isNodeSelected = selectedNodes.Contains(node);
                bool onNodeHeader = headerRect.Contains(graphLocalMouse);

                if (e.type == EventType.MouseDown && e.button == 0 && !graphPortClickHandled && nodeRect.Contains(graphLocalMouse) && !onNodeHeader)
                {
                    if (inSelectionZone)
                        e.Use();
                    else if (!isNodeSelected)
                    {
                        if (e.control || e.command)
                        {
                            if (selectedNodes.Contains(node)) selectedNodes.Remove(node);
                            else selectedNodes.Add(node);
                        }
                        else
                        {
                            selectedNodes.Clear();
                            selectedNodes.Add(node);
                        }
                        isBoxSelecting = false;
                        ResetGraphInputFocus();
                        e.Use();
                    }
                }

                Vector2 oldPos = node.Position;
                Rect windowRectBefore = nodeRect;
                GUIStyle nodeStyle = new GUIStyle(GUI.skin.window);
                if (selectedNodes.Contains(node) && isBoxSelecting)
                {
                    if (selectedNodeBgTexture == null) selectedNodeBgTexture = MakeTex(2, 2, new Color(0.4f, 0.6f, 0.9f, 0.25f));
                    nodeStyle.normal.background = selectedNodeBgTexture;
                }
                nodeRect = GUI.Window(i, nodeRect, (id) => DrawNodeWindow(id, node), "", nodeStyle);
                if (isDraggingSelection && isNodeSelected && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag))
                    GUIUtility.hotControl = selectionDragControlId;
                // Do not rewrite Position every frame: screen <-> world round-trip drifts floats while panning/zooming
                // and falsely marks the dialog dirty. Only apply when GUI.Window actually moved the rect.
                // During multi-select drag HandleSelectionDragInput owns Position - ignore GUI.Window moves.
                if (!(isDraggingSelection && isNodeSelected) && (nodeRect.x != windowRectBefore.x || nodeRect.y != windowRectBefore.y))
                {
                    Vector2 newPos = new Vector2(nodeRect.x / zoomScale + editorOffset.x, nodeRect.y / zoomScale + editorOffset.y);
                    Vector2 delta = newPos - oldPos;
                    if (delta.sqrMagnitude > 0.0001f && isNodeSelected && selectedNodes.Count > 1)
                    {
                        Undo.RecordObject(database, "Move Nodes");
                        foreach (var n in selectedNodes)
                        {
                            if (n != node) n.Position += delta;
                        }
                    }
                    node.Position = newPos;
                }
            }

            EndWindows();

            for (int i = 0; i < selectedDialog.nodes.Count; i++)
            {
                var node = selectedDialog.nodes[i];

                if (node == null) continue;

                float nodeHeight = Mathf.Max(GetNodeHeight(node), MIN_NODE_HEIGHT) * zoomScale;

                bool isInputConnected = selectedDialog.nodes.Any(n => n != null &&
                (
                    (n.NodeType == NodeType.Line && n.LineData.NextNodeId == node.nodeId) ||
                    (n.NodeType == NodeType.Choice && n.ChoiceData != null && n.ChoiceData.Any(c => c != null && c.NextNodeId == node.nodeId)) ||
                    (n.NodeType == NodeType.Start && n.StartData != null && n.StartData.NextNodeId == node.nodeId) ||
                    (n.NodeType == NodeType.Condition && n.ConditionData != null && (n.ConditionData.TrueNodeId == node.nodeId || n.ConditionData.FalseNodeId == node.nodeId)))
                );

                Rect inputPort;
                Vector2 inputPos;

                if (node.NodeType == NodeType.Line)
                {
                    inputPos = (node.Position - editorOffset) * zoomScale + new Vector2(INPUT_OFFSET_LINE * zoomScale, 0);
                    inputPort = new Rect(inputPos.x - PORT_HALF_SIZE * zoomScale, inputPos.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);
                }
                else if (node.NodeType == NodeType.Choice)
                {
                    inputPos = (node.Position - editorOffset) * zoomScale + new Vector2(0, CHOICE_NODE_ENTER_SPACING * zoomScale);
                    inputPort = new Rect(inputPos.x - PORT_HALF_SIZE * zoomScale, inputPos.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);
                }
                else if (node.NodeType == NodeType.Condition)
                {
                    inputPos = (node.Position - editorOffset) * zoomScale + new Vector2(0, CONDITION_NODE_ENTER_SPACING * zoomScale);
                    inputPort = new Rect(inputPos.x - PORT_HALF_SIZE * zoomScale, inputPos.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);
                }
                else if (node.NodeType == NodeType.Redirect)
                {
                    inputPos = (node.Position - editorOffset) * zoomScale + new Vector2(0, REDIRECT_NODE_ENTER_SPACING * zoomScale);
                    inputPort = new Rect(inputPos.x - PORT_HALF_SIZE * zoomScale, inputPos.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);
                }
                else
                {
                    inputPos = Vector2.zero;
                    inputPort = Rect.zero;
                }

                if (node.NodeType != NodeType.Start)
                {
                    bool isInputSelected = isInputConnected || selectedPorts.Any(p => p.Node == node && p.IsInput) || (pendingPort != null && pendingPort.Node == node && pendingPort.IsInput);
                    Texture2D inputTexture;
                    if (isInputSelected) inputTexture = nodeActiveInputTexture;
                    else inputTexture = nodeInactiveInputTexture;

                    if (inputTexture != null)
                    {
                        GUI.DrawTexture(inputPort, inputTexture, ScaleMode.StretchToFill);
                    }
                }

                if (node.NodeType == NodeType.Line || node.NodeType == NodeType.Start)
                {
                    Vector2 outputPos = (node.Position - editorOffset) * zoomScale + new Vector2(GetScaledNodeInputOffset(node), nodeHeight);
                    Rect outputPort = new Rect(outputPos.x - PORT_HALF_SIZE * zoomScale, outputPos.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);

                    bool isOutputConnected = (node.NodeType == NodeType.Line && node.LineData != null && !string.IsNullOrEmpty(node.LineData.NextNodeId)) || (node.NodeType == NodeType.Start && node.StartData != null && !string.IsNullOrEmpty(node.StartData.NextNodeId));
                    bool isOutputSelected = isOutputConnected || selectedPorts.Any(p => p.Node == node && !p.IsInput && p.ChoiceIndex == -1) || (pendingPort != null && pendingPort.Node == node && !pendingPort.IsInput && pendingPort.ChoiceIndex == -1);
                    
                    Texture2D outputTexture;

                    if (isOutputSelected) outputTexture = nodeActiveOutputTexture;
                    else outputTexture = nodeInactiveOutputTexture;

                    if (outputTexture != null) GUI.DrawTexture(outputPort, outputTexture, ScaleMode.StretchToFill);            
                }
                else if (node.NodeType == NodeType.Choice)
                {
                    if (node.ChoiceData == null) continue;

                    float choiceY = CHOICE_NODE_EXIT_SPACING;

                    for (int j = 0; j < node.ChoiceData.Count; j++)
                    {
                        if (node.ChoiceData[j] == null) continue;

                        Vector2 choicePort = (node.Position - editorOffset) * zoomScale + new Vector2(BASE_NODE_WIDTH * zoomScale, choiceY * zoomScale);
                        Rect choiceOutputPort = new Rect(choicePort.x - PORT_HALF_SIZE * zoomScale, choicePort.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);

                        bool isChoiceConnected = !string.IsNullOrEmpty(node.ChoiceData[j].NextNodeId);
                        bool isChoiceSelected = isChoiceConnected || selectedPorts.Any(p => p.Node == node && !p.IsInput && p.ChoiceIndex == j) || (pendingPort != null && pendingPort.Node == node && !pendingPort.IsInput && pendingPort.ChoiceIndex == j);

                        Texture2D choiceOutputTexture;

                        if (isChoiceSelected) choiceOutputTexture = nodeActiveOutputTexture;
                        else choiceOutputTexture = nodeInactiveOutputTexture;

                        if (choiceOutputTexture != null) GUI.DrawTexture(choiceOutputPort, choiceOutputTexture, ScaleMode.StretchToFill);
                        
                        choiceY += GetChoiceHeight(node.ChoiceData[j], node.nodeId);
                    }
                }
                else if (node.NodeType == NodeType.Condition)
                {
                    if (node.ConditionData == null) continue;

                    Vector2 truePort = (node.Position - editorOffset) * zoomScale + new Vector2(BASE_NODE_WIDTH * zoomScale, CONDITION_NODE_EXIT_SPACING * zoomScale);
                    Rect trueOutputPort = new Rect(truePort.x - PORT_HALF_SIZE * zoomScale, truePort.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);

                    bool isTrueConnected = !string.IsNullOrEmpty(node.ConditionData.TrueNodeId);
                    bool isTrueSelected = isTrueConnected || selectedPorts.Any(p => p.Node == node && !p.IsInput && p.ChoiceIndex == 0) || (pendingPort != null && pendingPort.Node == node && !pendingPort.IsInput && pendingPort.ChoiceIndex == 0);
                    Texture2D trueOutputTexture;

                    if (isTrueSelected) trueOutputTexture = nodeActiveOutputTexture;
                    else trueOutputTexture = nodeInactiveOutputTexture;

                    if (trueOutputTexture != null) GUI.DrawTexture(trueOutputPort, trueOutputTexture, ScaleMode.StretchToFill);
                    
                    Vector2 falsePort = (node.Position - editorOffset) * zoomScale + new Vector2(BASE_NODE_WIDTH * zoomScale, (CONDITION_NODE_EXIT_SPACING + NODE_SPACE_OFFSET + NODE_PORT_SIZE) * zoomScale);
                    Rect falseOutputPort = new Rect(falsePort.x - PORT_HALF_SIZE * zoomScale, falsePort.y - PORT_HALF_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale, NODE_PORT_SIZE * zoomScale);

                    bool isFalseConnected = !string.IsNullOrEmpty(node.ConditionData.FalseNodeId);
                    bool isFalseSelected = isFalseConnected || selectedPorts.Any(p => p.Node == node && !p.IsInput && p.ChoiceIndex == 1) || (pendingPort != null && pendingPort.Node == node && !pendingPort.IsInput && pendingPort.ChoiceIndex == 1);
                    
                    Texture2D falseOutputTexture;

                    if (isFalseSelected) falseOutputTexture = nodeActiveOutputTexture;
                    else falseOutputTexture = nodeInactiveOutputTexture;

                    if (falseOutputTexture != null) GUI.DrawTexture(falseOutputPort, falseOutputTexture, ScaleMode.StretchToFill);     
                }
            }

            Handles.EndGUI();
            GUI.EndGroup();

            if (showConditions)
            {
                if (selectedDialog.conditions == null) selectedDialog.conditions = new List<DialogCondition>();

                const int CONDITIONS_WINDOW_BORDER = 10;
                const int CONDITIONS_ADD_BUTTON_HEIGHT = 30;
                const int CONDITIONS_ADD_BUTTON_GAP = 5;
                const int CONDITIONS_LOGIC_TOGGLE_SIZE = 30;
                const int CONDITIONS_LOCK_BUTTON_WIDTH = 40;
                const int CONDITIONS_HELPBOX_HEIGHT = 48;
                const int CONDITIONS_VALIDATION_HELPBOX_HEIGHT = 72;
                const int CONDITIONS_SECTION_GAP = 5;
                const int CONDITIONS_SCROLL_BOTTOM = 135;
                const int CONDITIONS_HEADER_OFFSET = 30;
                const int CONDITIONS_CLOSE_BUTTON_SIZE = 30;
                const int CONDITIONS_CLOSE_OFFSET = 40;
                const int SCROLLBAR_WIDTH = 15;
                const int CONDITIONS_BOX_BORDER = 5;

                GUI.BeginGroup(conditionsRect);
                GUIStyle conditionsBackgroundStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    border = new RectOffset(CONDITIONS_WINDOW_BORDER, CONDITIONS_WINDOW_BORDER, CONDITIONS_WINDOW_BORDER, CONDITIONS_WINDOW_BORDER)
                };

                GUI.backgroundColor = new Color(LINE_COLOR_2.r, LINE_COLOR_2.g, LINE_COLOR_2.b);
                GUI.Box(new Rect(0, 0, CONDITION_NODE_WIDTH, editorRect.height), "", conditionsBackgroundStyle);
                GUI.backgroundColor = Color.white;
                string conditionsPanelTitle;

                if (selectedDialog.isLocked) conditionsPanelTitle = "Locked (redirect-only)";
                else conditionsPanelTitle = $"Playback conditions ({selectedDialog.conditions.Count})";

                GUI.Label(new Rect(CONDITIONS_WINDOW_BORDER, CONDITIONS_WINDOW_BORDER, CONDITION_NODE_WIDTH - 2 * CONDITIONS_WINDOW_BORDER, CATEGORY_HEADER_HEIGHT), conditionsPanelTitle, EditorStyles.boldLabel);

                if (GUI.Button(new Rect(CONDITION_NODE_WIDTH - CONDITIONS_CLOSE_OFFSET, CONDITIONS_WINDOW_BORDER, CONDITIONS_CLOSE_BUTTON_SIZE, FIELD_ROW_HEIGHT), "→", EditorStyles.miniButton)) showConditions = false;

                float conditionsPanelY = CONDITIONS_HEADER_OFFSET + CONDITIONS_WINDOW_BORDER;
                float conditionsContentWidth = CONDITION_NODE_WIDTH - 2 * CONDITIONS_WINDOW_BORDER;
                EditorGUI.HelpBox(new Rect(CONDITIONS_WINDOW_BORDER, conditionsPanelY, conditionsContentWidth, CONDITIONS_HELPBOX_HEIGHT), GetDialogStartConditionsHelpMessage(selectedDialog.isAndLogic, selectedDialog.isLocked), MessageType.Info);
                conditionsPanelY += CONDITIONS_HELPBOX_HEIGHT + CONDITIONS_SECTION_GAP;
                string selectedCategory = selectedDialog.initiator;

                if (string.IsNullOrEmpty(selectedCategory)) selectedCategory = DEFAULT_CATEGORY_NAME;

                List<DialogStartConditionOverlapValidator.ValidationIssue> categoryConditionIssues = GetCategoryStartConditionIssues(selectedCategory);

                if (categoryConditionIssues.Count > 0)
                {
                    string validationSummary = DialogStartConditionOverlapValidator.FormatSummary(categoryConditionIssues, 3);
                    EditorGUI.HelpBox(new Rect(CONDITIONS_WINDOW_BORDER, conditionsPanelY, conditionsContentWidth, CONDITIONS_VALIDATION_HELPBOX_HEIGHT), validationSummary, MessageType.Warning);
                    conditionsPanelY += CONDITIONS_VALIDATION_HELPBOX_HEIGHT + CONDITIONS_SECTION_GAP;
                }

                string lockButtonLabel;

                if (selectedDialog.isLocked) lockButtonLabel = "🔒";
                else lockButtonLabel = "🔓";

                string lockButtonTooltip;

                if (selectedDialog.isLocked) lockButtonTooltip = "Click to unlock. Then the dialog can work through the initiator and you can edit conditions.";
                else lockButtonTooltip = "Click to lock. Then the dialog can start only from Redirect node.";

                if (GUI.Button(new Rect(CONDITIONS_WINDOW_BORDER, conditionsPanelY, CONDITIONS_LOCK_BUTTON_WIDTH, CONDITIONS_ADD_BUTTON_HEIGHT), new GUIContent(lockButtonLabel, lockButtonTooltip)))
                {
                    Undo.RecordObject(database, "Toggle Dialog Lock");
                    selectedDialog.isLocked = !selectedDialog.isLocked;
                    EditorUtility.SetDirty(database);
                }

                EditorGUI.BeginDisabledGroup(selectedDialog.isLocked);

                float addButtonX = CONDITIONS_WINDOW_BORDER + CONDITIONS_LOCK_BUTTON_WIDTH + CONDITIONS_ADD_BUTTON_GAP;
                float addButtonWidth = conditionsContentWidth - CONDITIONS_LOCK_BUTTON_WIDTH - CONDITIONS_ADD_BUTTON_GAP;

                if (selectedDialog.conditions.Count >= 2) addButtonWidth -= CONDITIONS_LOGIC_TOGGLE_SIZE + CONDITIONS_ADD_BUTTON_GAP;

                if (GUI.Button(new Rect(addButtonX, conditionsPanelY, addButtonWidth, CONDITIONS_ADD_BUTTON_HEIGHT), new GUIContent("+ Add condition🗝️", "Add a new condition for the dialog")))
                {
                    Undo.RecordObject(database, "Add Dialog Condition");

                    if (selectedDialog.conditions == null) selectedDialog.conditions = new List<DialogCondition>();

                    selectedDialog.conditions.Add(CreateDefaultDialogCondition());
                }

                if (selectedDialog.conditions.Count >= 2)
                {
                    Rect logicToggleRect = new Rect(addButtonX + addButtonWidth + CONDITIONS_ADD_BUTTON_GAP, conditionsPanelY, CONDITIONS_LOGIC_TOGGLE_SIZE, CONDITIONS_ADD_BUTTON_HEIGHT);
                    string logicButtonLabel;

                    if (selectedDialog.isAndLogic) logicButtonLabel = "&&";
                    else logicButtonLabel = "||";

                    if (GUI.Button(logicToggleRect, logicButtonLabel))
                    {
                        Undo.RecordObject(database, "Toggle Dialog Start Condition Logic");
                        selectedDialog.isAndLogic = !selectedDialog.isAndLogic;
                    }
                }

                conditionsPanelY += CONDITIONS_ADD_BUTTON_HEIGHT + CONDITIONS_SECTION_GAP;

                string conditionLogicSeparator = GetDialogStartConditionLogicSeparator(selectedDialog.isAndLogic);

                Rect scrollViewRect = new Rect(CONDITIONS_WINDOW_BORDER, conditionsPanelY, conditionsContentWidth, conditionsRect.height - conditionsPanelY - CONDITIONS_SCROLL_BOTTOM);
                float totalConditionsHeight = 0;

                for (int i = 0; i < selectedDialog.conditions.Count; i++)
                {
                    totalConditionsHeight += CONDITION_NODE_CONDITION_HEIGHT + CONDITION_ITEM_SPACING;
                }

                bool conditionsNeedScrollbar = totalConditionsHeight > scrollViewRect.height;
                float scrollContentWidth = conditionsContentWidth;
                if (conditionsNeedScrollbar) scrollContentWidth -= SCROLLBAR_WIDTH;

                Rect scrollContentRect = new Rect(0, 0, scrollContentWidth, Mathf.Max(scrollViewRect.height, totalConditionsHeight));
                conditionsScrollPos = GUI.BeginScrollView(scrollViewRect, conditionsScrollPos, scrollContentRect);
                float currentY = 0;
                List<int> conditionsToRemove = new List<int>();

                for (int i = 0; i < selectedDialog.conditions.Count; i++)
                {
                    float conditionHeight = CONDITION_NODE_CONDITION_HEIGHT;
                    Rect conditionRect = new Rect(0, currentY, scrollContentRect.width, conditionHeight);

                    EditorGUI.DrawRect(conditionRect, lineColors[i % lineColors.Length]);

                    GUIStyle conditionBoxStyle = new GUIStyle(GUI.skin.box)
                    {
                        border = new RectOffset(CONDITIONS_BOX_BORDER, CONDITIONS_BOX_BORDER, CONDITIONS_BOX_BORDER, CONDITIONS_BOX_BORDER)
                    };

                    GUI.backgroundColor = CONDITION_BOX_BG;
                    Rect innerConditionBoxRect = new Rect(0, currentY + CONDITIONS_BOX_PADDING, scrollContentRect.width, conditionHeight - 2 * CONDITIONS_BOX_PADDING);
                    GUI.Box(innerConditionBoxRect, GUIContent.none, conditionBoxStyle);
                    GUI.backgroundColor = Color.white;

                    var startCondition = selectedDialog.conditions[i];
                    var shellParams = new ConditionRowShellParams
                    {
                        Layout = ConditionRowShellLayout.DialogStartPanel,
                        Index = i,
                        ReferenceState = startCondition.ReferenceState,
                        VariableName = startCondition.VariableName,
                        MissingReferenceLabel = startCondition.MissingReferenceLabel,
                        ContentWidth = scrollContentRect.width,
                        RowBaseY = currentY,
                        ZoomScale = 1f,
                        WarningBoundsRect = innerConditionBoxRect,
                        UseRedDeleteButton = true
                    };

                    if (DrawConditionRowShell(shellParams, conditionsToRemove, out float fieldY))
                    {
                        currentY += conditionHeight;

                        if (i < selectedDialog.conditions.Count - 1)
                        {
                            GUI.Label(new Rect(0, currentY, scrollContentRect.width, FIELD_ROW_HEIGHT), conditionLogicSeparator, EditorStyles.boldLabel);
                            currentY += CONDITION_AND_SPACING;
                        }

                        continue;
                    }

                    DrawDialogConditionEditableFields(
                        startCondition,
                        i,
                        selectedDialog.conditions,
                        ref fieldY,
                        new ConditionFieldsDrawContext
                        {
                            ZoomScale = 1f,
                            ContentWidth = scrollContentRect.width
                        });

                    currentY += conditionHeight;

                    if (i < selectedDialog.conditions.Count - 1)
                    {
                        GUI.Label(new Rect(0, currentY, scrollContentRect.width, FIELD_ROW_HEIGHT), conditionLogicSeparator, EditorStyles.boldLabel);
                        currentY += CONDITION_AND_SPACING;
                    }
                }

                if (conditionsToRemove.Count > 0)
                {
                    Undo.RecordObject(database, "Remove Dialog Condition");

                    foreach (int index in conditionsToRemove.OrderByDescending(i => i))
                    {
                        selectedDialog.conditions.RemoveAt(index);
                    }
                }

                GUI.EndScrollView();

                EditorGUI.EndDisabledGroup();

                GUI.EndGroup();
            }

            bool mouseOverDialogStartConditionsPanel = showConditions && conditionsRect.Contains(e.mousePosition);

            if (e.type == EventType.MouseDown && e.button == 0 && editorRect.Contains(e.mousePosition) && !mouseOverDialogStartConditionsPanel && !IsMouseOverAnyGraphNode(e) && !IsMouseOverSelectionOuterRect(e))
            {
                bool dismissPendingState = pendingPort != null || copiedActionData != null;
                ResetGraphInputFocus();
                selectedPorts.Clear();
                pendingPort = null;
                copiedActionData = null;
                copiedActionSourceNodeId = null;
                selectedNodes.Clear();
                isDraggingSelection = false;
                selectionDragStartNodePositions = null;
                isBoxSelecting = false;
                if (!dismissPendingState)
                {
                    isBoxSelecting = true;
                    boxSelectStart = e.mousePosition;
                    boxSelectCurrent = e.mousePosition;
                }
                e.Use();
            }

            if (e.type == EventType.MouseDrag && e.button == 0 && isBoxSelecting)
            {
                boxSelectCurrent = e.mousePosition;
                Rect boxRect = Rect.MinMaxRect(
                    Mathf.Min(boxSelectStart.x, boxSelectCurrent.x),
                    Mathf.Min(boxSelectStart.y, boxSelectCurrent.y),
                    Mathf.Max(boxSelectStart.x, boxSelectCurrent.x),
                    Mathf.Max(boxSelectStart.y, boxSelectCurrent.y));
                selectedNodes.Clear();
                foreach (var n in selectedDialog.nodes)
                {
                    if (n == null) continue;
                    float h = Mathf.Max(GetNodeHeight(n), MIN_NODE_HEIGHT) * zoomScale;
                    float w = GetScaledNodeWidth(n);
                    Rect nodeRect = new Rect(editorRect.x + (n.Position.x - editorOffset.x) * zoomScale, editorRect.y + (n.Position.y - editorOffset.y) * zoomScale, w, h);
                    if (boxRect.Overlaps(nodeRect)) selectedNodes.Add(n);
                }
                Repaint();
                e.Use();
            }

            if (e.type == EventType.MouseUp && e.button == 0 && isBoxSelecting)
            {
                if ((boxSelectStart - boxSelectCurrent).sqrMagnitude < BOX_SELECT_MIN_DRAG * BOX_SELECT_MIN_DRAG)
                    selectedNodes.Clear();
                isBoxSelecting = false;
                ResetGraphInputFocus();
                e.Use();
            }

            if (selectedNodes.Count > 0 && !isBoxSelecting && e.type == EventType.Repaint && TryGetNodeSelectionOuterRectScreen(out Rect selectionOuterRect))
            {
                if (selectedNodes.Count > 1) EditorGUIUtility.AddCursorRect(selectionOuterRect, MouseCursor.MoveArrow);
                EditorGUI.DrawRect(selectionOuterRect, new Color(0.3f, 0.5f, 0.9f, 0.08f));
                DrawBox(selectionOuterRect, new Color(0.3f, 0.5f, 0.9f, 0.6f), 2);
            }

            if (isBoxSelecting && e.type == EventType.Repaint)
            {
                Rect boxRect = Rect.MinMaxRect(
                    Mathf.Min(boxSelectStart.x, boxSelectCurrent.x),
                    Mathf.Min(boxSelectStart.y, boxSelectCurrent.y),
                    Mathf.Max(boxSelectStart.x, boxSelectCurrent.x),
                    Mathf.Max(boxSelectStart.y, boxSelectCurrent.y));

                EditorGUI.DrawRect(boxRect, new Color(0.3f, 0.5f, 0.9f, 0.2f));
                DrawBox(boxRect, new Color(0.3f, 0.5f, 0.9f, 0.8f), 2);
            }

            const int SCALE_LABEL_WIDTH = 100;
            const int SCALE_LABEL_HEIGHT = 20;
            const int SCALE_LABEL_RIGHT_OFFSET = 80;
            const int SCALE_LABEL_FONT_SIZE = 12;
            const int HINT_FONT_SIZE = 13;
            const int HINT_OFFSET = 44;
            const int HINT_PADDING = 8;
            const int HINT_MIN_WIDTH_OFFSET = 100;


            GUIStyle scaleLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = SCALE_LABEL_FONT_SIZE,
                normal = { textColor = Color.white }
            };

            Rect scaleLabelRect = new Rect(editorRect.x + editorRect.width - SCALE_LABEL_RIGHT_OFFSET, editorRect.y + editorRect.height - SCALE_LABEL_HEIGHT, SCALE_LABEL_WIDTH, SCALE_LABEL_HEIGHT);

            if (GUI.Button(scaleLabelRect, new GUIContent($"Scale: {(zoomScale * 100):F0}%", "Click - reset to 100%"), scaleLabelStyle) && Event.current.button == 0)
            {
                zoomScale = DEFAULT_ZOOM_SCALE;
            }

            if (pendingPort != null)
            {
                GUIStyle hintStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = HINT_FONT_SIZE,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(8, 8, 4, 4),
                    normal = { textColor = HINT_TEXT_COLOR }
                };

                string portDesc = GetPortDescription(pendingPort);
                string hintMessage;

                if (pendingPort.Node.NodeType == NodeType.Start) hintMessage = $"{portDesc} is selected and waiting for connection. LMB on the same port or empty space to clear selection, RMB to open port menu.";
                else
                {
                    string nodeIdShort;
                    if (pendingPort.Node.nodeId.Length >= NODE_ID_DISPLAY_LENGTH) nodeIdShort = pendingPort.Node.nodeId.Substring(0, NODE_ID_DISPLAY_LENGTH);
                    else nodeIdShort = pendingPort.Node.nodeId;
                    hintMessage = $"{portDesc} {nodeIdShort} is selected and waiting for connection. LMB on the same port or empty space to clear selection, RMB to open port menu.";
                }

                GUIContent hintContent = new GUIContent(hintMessage);
                Vector2 hintSize = hintStyle.CalcSize(hintContent);
                Rect hintRect = new Rect(editorRect.x + HINT_PADDING, editorRect.y + editorRect.height - hintSize.y - HINT_OFFSET, Mathf.Min(hintSize.x + HINT_EXTRA_WIDTH, editorRect.width - HINT_MIN_WIDTH_OFFSET), hintSize.y + HINT_PADDING);
                GUI.Box(hintRect, hintContent, hintStyle);
            }

            if (copiedActionData != null)
            {
                GUIStyle copyHintStyle = new GUIStyle(GUI.skin.box)
                {
                    fontSize = HINT_FONT_SIZE,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(8, 8, 4, 4),
                    normal = { textColor = HINT_COPY_TEXT_COLOR }
                };

                string charInfo;

                if (string.IsNullOrEmpty(copiedActionData.CharacterName)) charInfo = "-";
                else charInfo = copiedActionData.CharacterName;

                string emoInfo;

                if (string.IsNullOrEmpty(copiedActionData.EmotionName)) emoInfo = "";
                else emoInfo = $", {copiedActionData.EmotionName}";
                
                GUIContent copyHintContent = new GUIContent($"📋 Action copied ({charInfo}{emoInfo}). Click in empty space to reset.");
                Vector2 copyHintSize = copyHintStyle.CalcSize(copyHintContent);
                float copyHintOffset;

                if (pendingPort != null) copyHintOffset = copyHintSize.y + FIELD_ROW_HEIGHT;
                else copyHintOffset = 0;

                float copyHintY = editorRect.y + editorRect.height - copyHintSize.y - HINT_OFFSET - copyHintOffset;
                Rect copyHintRect = new Rect(editorRect.x + HINT_PADDING, copyHintY, Mathf.Min(copyHintSize.x + 16, editorRect.width - HINT_MIN_WIDTH_OFFSET), copyHintSize.y + HINT_PADDING);
                GUI.Box(copyHintRect, copyHintContent, copyHintStyle);
            }

            EditorGUILayout.EndVertical();

            if (e.type == EventType.MouseDown && e.button == 1 && editorRect.Contains(e.mousePosition))
            {
                if (pendingPort != null) ShowPortContextMenu(pendingPort.Node, pendingPort.IsInput, pendingPort.ChoiceIndex);
                
                else
                {
                    Vector2 worldPos = (e.mousePosition / zoomScale) + editorOffset;
                    ShowNodeContextMenu(worldPos);
                }

                e.Use();
            }
            if (e.type == EventType.MouseDown && e.button == 2 && editorRect.Contains(e.mousePosition))
            {
                isDragging = true;
                dragStartPos = e.mousePosition;
                e.Use();
            }
            else if (e.type == EventType.MouseDrag && isDragging && e.button == 2)
            {
                Vector2 delta = (e.mousePosition - dragStartPos) / zoomScale;
                editorOffset -= delta;
                dragStartPos = e.mousePosition;
                e.Use();
            }
            else if (e.type == EventType.MouseUp && e.button == 2)
            {
                isDragging = false;
                e.Use();
            }

            if (e.type == EventType.ScrollWheel && editorRect.Contains(e.mousePosition) && !mouseOverDialogStartConditionsPanel)
            {
                Vector2 mousePos = e.mousePosition;
                float oldZoom = zoomScale;
                float zoomDelta = e.delta.y * -ZOOM_SENSITIVITY;
                zoomScale = Mathf.Clamp(zoomScale + zoomDelta, ZOOM_MIN, ZOOM_MAX);

                if (Mathf.Abs(zoomScale - oldZoom) > ZOOM_EPSILON)
                {
                    Vector2 mouseRelativeToEditor = mousePos - new Vector2(editorRect.x, editorRect.y);
                    Vector2 mouseWorldBeforeZoom = editorOffset + (mouseRelativeToEditor / oldZoom);
                    Vector2 mouseWorldAfterZoom = editorOffset + (mouseRelativeToEditor / zoomScale);
                    editorOffset += (mouseWorldBeforeZoom - mouseWorldAfterZoom);
                }

                Repaint();
            }

            CommitPendingDialogNameEditIfUnfocused();
        }

        private Vector2 GetNodeInputPosition(DialogNode node)
        {
            if (node.NodeType == NodeType.Line)
            {
                return (node.Position - editorOffset) * zoomScale + new Vector2(INPUT_OFFSET_LINE * zoomScale, 0);
            }
            else if (node.NodeType == NodeType.Choice)
            {
                return (node.Position - editorOffset) * zoomScale + new Vector2(0, CHOICE_NODE_ENTER_SPACING * zoomScale);
            }
            else if (node.NodeType == NodeType.Condition)
            {
                return (node.Position - editorOffset) * zoomScale + new Vector2(0, CONDITION_NODE_ENTER_SPACING * zoomScale);
            }
            else if (node.NodeType == NodeType.Redirect)
            {
                return (node.Position - editorOffset) * zoomScale + new Vector2(0, REDIRECT_NODE_ENTER_SPACING * zoomScale);
            }
            else
            {
                return Vector2.zero;
            }
        }

        #region Port Context Menu

        private string GetPortDescription(SelectedPort port)
        {
            if (port == null || port.Node == null) return "port";
            var node = port.Node;
            if (node.NodeType == NodeType.Start) return "Start node";

            string nodeTypeName = node.NodeType switch
            {
                NodeType.Line => "line",
                NodeType.Choice => "choice",
                NodeType.Condition => "condition",
                NodeType.Redirect => "redirect",
                _ => "node"
            };
            if (port.IsInput) return $"Input port of {nodeTypeName} node";
            if (node.NodeType == NodeType.Choice && port.ChoiceIndex >= 0 && node.ChoiceData != null && port.ChoiceIndex < node.ChoiceData.Count)
            {
                var choice = node.ChoiceData[port.ChoiceIndex];
                string choiceLabel = $"option {port.ChoiceIndex + 1}";
                if (choice?.Translations != null)
                {
                    var t = choice.Translations.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x?.Text));
                    if (t != null)
                    {
                        if (t.Text.Length > 20) choiceLabel = $"\"{t.Text.Substring(0, 20)}…\"";
                        else choiceLabel = $"\"{t.Text}\"";
                    }
                }
                return $"Output port {choiceLabel} of the choice node";
            }
            if (node.NodeType == NodeType.Condition)
            {
                string portLabel;
                if (port.ChoiceIndex == 0) portLabel = "TRUE";
                else portLabel = "FALSE";
                return $"Output port {portLabel} of the condition node";
            }
            return $"Output port of the {nodeTypeName} node";
        }

        private void ShowPortContextMenu(DialogNode node, bool isInput, int choiceIndex)
        {
            if (node == null || selectedDialog == null || selectedDialog.nodes == null)
            {
                pendingPort = null;
                return;
            }

            string nodeId = node.nodeId;
            DialogNode targetNode = selectedDialog.nodes.Find(n => n != null && n.nodeId == nodeId);

            if (targetNode == null)
            {
                pendingPort = null;
                return;
            }

            GenericMenu menu = new GenericMenu();
            actionWasPerformed = false; // Reset flag before showing menu
            isContextMenuOpen = true;   // Set flag before showing menu

            if (isInput)
            {
                HandleInputPortMenu(targetNode, nodeId, menu);
            }
            else
            {
                HandleOutputPortMenu(targetNode, nodeId, choiceIndex, menu);
            }

            menu.ShowAsContext();
            isContextMenuOpen = false; // Reset flag after closing menu
        }

        private void HandleInputPortMenu(DialogNode targetNode, string nodeId, GenericMenu menu)
        {
            bool isConnected = IsNodeConnected(nodeId);

            if (isConnected)
            {
                menu.AddItem(new GUIContent("Disconnect"), false, () =>
                {
                    Undo.RecordObject(database, "Disconnect Input Port");

                    foreach (var sourceNode in selectedDialog.nodes)
                    {
                        if (sourceNode == null) continue;
                        DisconnectPort(sourceNode, -1);
                    }

                    selectedPorts.Clear();
                    pendingPort = null;
                    actionWasPerformed = true; // Set flag
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Node is not connected"));
            }
        }

        private void HandleOutputPortMenu(DialogNode targetNode, string nodeId, int choiceIndex, GenericMenu menu)
        {
            string currentConnection = GetCurrentConnection(targetNode, choiceIndex);

            if (!string.IsNullOrEmpty(currentConnection))
            {
                menu.AddItem(new GUIContent("Disconnect"), false, () =>
                {
                    Undo.RecordObject(database, "Disconnect Output Port");
                    DisconnectPort(targetNode, choiceIndex);
                    selectedPorts.Clear();
                    pendingPort = null;
                    actionWasPerformed = true; // Set flag
                });
            }

            foreach (var target in selectedDialog.nodes)
            {
                if (target == null || target.nodeId == nodeId || target.NodeType == NodeType.Start) continue;

                if (IsAlreadyConnected(targetNode, target.nodeId, choiceIndex)) continue;

                string targetName = $"Input {target.nodeId.Substring(0, NODE_ID_DISPLAY_LENGTH)}";
                string targetId = target.nodeId;

                menu.AddItem(new GUIContent(targetName), false, () =>
                {
                    Undo.RecordObject(database, "Connect Port");
                    ConnectPort(targetNode, targetId, choiceIndex);
                    selectedPorts.Clear();
                    pendingPort = null;
                    actionWasPerformed = true; // Set flag
                });
            }
        }

        private bool IsNodeConnected(string nodeId)
        {
            return selectedDialog?.nodes?.Any(n => n != null &&
            (
                (n.NodeType == NodeType.Line && n.LineData != null && n.LineData.NextNodeId == nodeId) ||
                (n.NodeType == NodeType.Choice && n.ChoiceData != null && n.ChoiceData.Any(c => c != null && c.NextNodeId == nodeId)) ||
                (n.NodeType == NodeType.Start && n.StartData != null && n.StartData.NextNodeId == nodeId) ||
                (n.NodeType == NodeType.Condition && n.ConditionData != null && (n.ConditionData.TrueNodeId == nodeId || n.ConditionData.FalseNodeId == nodeId)))
            ) ?? false;
        }

        private void DisconnectPort(DialogNode node, int choiceIndex = -1)
        {
            if (node.NodeType == NodeType.Line && node.LineData != null)
            {
                node.LineData.NextNodeId = null;
            }
            else if (node.NodeType == NodeType.Start && node.StartData != null)
            {
                node.StartData.NextNodeId = null;
            }
            else if (node.NodeType == NodeType.Choice && choiceIndex >= 0 && node.ChoiceData != null && choiceIndex < node.ChoiceData.Count && node.ChoiceData[choiceIndex] != null)
            {
                node.ChoiceData[choiceIndex].NextNodeId = null;
            }
            else if (node.NodeType == NodeType.Condition && node.ConditionData != null)
            {
                if (choiceIndex == 0) node.ConditionData.TrueNodeId = null;
                else if (choiceIndex == 1) node.ConditionData.FalseNodeId = null;
            }
        }

        private void ConnectPort(DialogNode sourceNode, string targetId, int choiceIndex = -1)
        {
            if (sourceNode.NodeType == NodeType.Line)
            {
                if (sourceNode.LineData == null) sourceNode.LineData = new LineNode();

                sourceNode.LineData.NextNodeId = targetId;
            }
            else if (sourceNode.NodeType == NodeType.Start)
            {
                if (sourceNode.StartData == null) sourceNode.StartData = new StartNode();

                sourceNode.StartData.NextNodeId = targetId;
            }
            else if (sourceNode.NodeType == NodeType.Choice && choiceIndex >= 0 && sourceNode.ChoiceData != null && choiceIndex < sourceNode.ChoiceData.Count && sourceNode.ChoiceData[choiceIndex] != null)
            {
                sourceNode.ChoiceData[choiceIndex].NextNodeId = targetId;
            }
            else if (sourceNode.NodeType == NodeType.Condition && sourceNode.ConditionData != null)
            {
                if (choiceIndex == 0) sourceNode.ConditionData.TrueNodeId = targetId;
                else if (choiceIndex == 1) sourceNode.ConditionData.FalseNodeId = targetId;
            }
        }

        private string GetCurrentConnection(DialogNode node, int choiceIndex)
        {
            if (node.NodeType == NodeType.Line) return node.LineData?.NextNodeId;
            if (node.NodeType == NodeType.Start) return node.StartData?.NextNodeId;
            if (node.NodeType == NodeType.Choice && choiceIndex >= 0 && node.ChoiceData != null && choiceIndex < node.ChoiceData.Count && node.ChoiceData[choiceIndex] != null) return node.ChoiceData[choiceIndex].NextNodeId;
            if (node.NodeType == NodeType.Condition && node.ConditionData != null)
            {
                if (choiceIndex == 0) return node.ConditionData.TrueNodeId;
                return node.ConditionData.FalseNodeId;
            }
            return null;
        }

        private bool IsAlreadyConnected(DialogNode sourceNode, string targetId, int choiceIndex)
        {
            if (sourceNode.NodeType == NodeType.Line) return sourceNode.LineData?.NextNodeId == targetId;
            if (sourceNode.NodeType == NodeType.Start) return sourceNode.StartData?.NextNodeId == targetId;
            if (sourceNode.NodeType == NodeType.Choice && choiceIndex >= 0 && sourceNode.ChoiceData != null && choiceIndex < sourceNode.ChoiceData.Count && sourceNode.ChoiceData[choiceIndex] != null)
                return sourceNode.ChoiceData[choiceIndex].NextNodeId == targetId;
            if (sourceNode.NodeType == NodeType.Condition && sourceNode.ConditionData != null)
                return (choiceIndex == 0 && sourceNode.ConditionData.TrueNodeId == targetId) || (choiceIndex == 1 && sourceNode.ConditionData.FalseNodeId == targetId);
            return false;
        }

        /// <summary>
        /// If clicking an already selected port, deselect it. Returns true if deselected.
        /// </summary>
        private Rect GetGraphLocalNodeRect(DialogNode node)
        {
            float w = GetScaledNodeWidth(node);
            float h = Mathf.Max(GetNodeHeight(node), MIN_NODE_HEIGHT) * zoomScale;
            return new Rect(
                (node.Position.x - editorOffset.x) * zoomScale,
                (node.Position.y - editorOffset.y) * zoomScale,
                w, h);
        }

        private bool IsGraphLocalMouseOverAnyNode(Vector2 graphLocalMouse)
        {
            if (selectedDialog?.nodes == null) return false;

            foreach (var n in selectedDialog.nodes)
            {
                if (n == null) continue;
                if (GetGraphLocalNodeRect(n).Contains(graphLocalMouse)) return true;
            }

            return false;
        }

        private bool IsMouseOverAnyGraphNode(Event evt)
        {
            if (selectedDialog?.nodes == null || !editorRect.Contains(evt.mousePosition)) return false;
            Vector2 graphLocalMouse = new Vector2(evt.mousePosition.x - editorRect.x, evt.mousePosition.y - editorRect.y);
            return IsGraphLocalMouseOverAnyNode(graphLocalMouse);
        }

        private bool TryGetNodeSelectionOuterRectScreen(out Rect outerRect)
        {
            outerRect = default;
            if (selectedNodes.Count <= 1) return false;

            float xMin = float.MaxValue, yMin = float.MaxValue, xMax = float.MinValue, yMax = float.MinValue;
            foreach (var n in selectedNodes)
            {
                if (n == null) continue;
                float w = GetScaledNodeWidth(n);
                float h = Mathf.Max(GetNodeHeight(n), MIN_NODE_HEIGHT) * zoomScale;
                float nx = editorRect.x + (n.Position.x - editorOffset.x) * zoomScale;
                float ny = editorRect.y + (n.Position.y - editorOffset.y) * zoomScale;
                xMin = Mathf.Min(xMin, nx);
                yMin = Mathf.Min(yMin, ny);
                xMax = Mathf.Max(xMax, nx + w);
                yMax = Mathf.Max(yMax, ny + h);
            }

            if (xMin >= float.MaxValue) return false;

            outerRect = new Rect(
                xMin - SELECTION_FRAME_PADDING,
                yMin - SELECTION_FRAME_PADDING,
                xMax - xMin + SELECTION_FRAME_PADDING * 2,
                yMax - yMin + SELECTION_FRAME_PADDING * 2);
            return true;
        }

        private bool TryGetNodeSelectionOuterRectLocal(out Rect outerRect)
        {
            outerRect = default;
            if (!TryGetNodeSelectionOuterRectScreen(out Rect screenRect)) return false;
            outerRect = new Rect(screenRect.x - editorRect.x, screenRect.y - editorRect.y, screenRect.width, screenRect.height);
            return true;
        }

        private bool IsMouseOverSelectionOuterRect(Event evt)
        {
            return selectedNodes.Count > 1 && !isBoxSelecting
                && TryGetNodeSelectionOuterRectScreen(out Rect r) && r.Contains(evt.mousePosition);
        }

        private void ResetGraphInputFocus()
        {
            GUIUtility.hotControl = 0;
            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(null);
            GUI.FocusWindow(-1);
        }

        private void BeginSelectionDrag(Event evt)
        {
            selectionDragControlId = GUIUtility.GetControlID(FocusType.Passive);
            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(null);
            GUI.FocusWindow(-1);
            GUIUtility.hotControl = selectionDragControlId;
            isDraggingSelection = true;
            selectionDragStartPos = evt.mousePosition;
            selectionDragStartNodePositions = new Dictionary<DialogNode, Vector2>();
            foreach (var n in selectedNodes)
            {
                if (n != null) selectionDragStartNodePositions[n] = n.Position;
            }
            Undo.RecordObject(database, "Move Nodes");
        }

        private static string GetDialogStartConditionsHelpMessage(bool isAndLogic, bool isLocked)
        {
            if (isLocked)
            {
                return "This dialog is locked - redirect only. Start conditions are disabled and will not run from the initiator.";
            }

            string logicPart;
            if (isAndLogic) logicPart = "All conditions must be met.";
            else logicPart = "One met condition is enough.";

            return "Add playback conditions for the dialog. " + logicPart +
                " The category checks the order of dialogs in the Dialog Database - the first suitable one is triggered.";
        }

        private static string GetDialogStartConditionLogicSeparator(bool isAndLogic)
        {
            if (isAndLogic) return "AND";
            return "OR";
        }

        private void HandleSelectionDragInput(Event evt)
        {
            if (!isDraggingSelection || selectionDragStartNodePositions == null) return;

            if (evt.type == EventType.MouseDrag && evt.button == 0)
            {
                GUIUtility.hotControl = selectionDragControlId;
                Vector2 deltaGraph = (evt.mousePosition - selectionDragStartPos) / zoomScale;
                foreach (var kvp in selectionDragStartNodePositions)
                {
                    if (kvp.Key != null) kvp.Key.Position = kvp.Value + deltaGraph;
                }
                evt.Use();
                Repaint();
            }
            else if (evt.type == EventType.MouseUp && evt.button == 0)
            {
                isDraggingSelection = false;
                selectionDragStartNodePositions = null;
                ResetGraphInputFocus();
                evt.Use();
            }
        }

        private bool TryDeselectSamePortClick(DialogNode node, bool isInput, int choiceIndex = -1)
        {
            if (pendingPort == null || pendingPort.Node != node || pendingPort.IsInput != isInput || pendingPort.ChoiceIndex != choiceIndex)
                return false;
            selectedPorts.Clear();
            pendingPort = null;
            return true;
        }

        /// <summary>
        /// If connection is expected (pendingPort) and clicking opposite port of another node, connect directly.
        /// Returns true if connected.
        /// </summary>
        private bool TryCompleteConnectionByClick(DialogNode clickedNode, bool clickedIsInput, int clickedChoiceIndex)
        {
            if (pendingPort == null || selectedDialog == null || clickedNode == null) return false;
            if (pendingPort.Node == clickedNode) return false; // cannot connect a node to itself

            if (pendingPort.IsInput)
            {
                if (!clickedIsInput)
                {
                    Undo.RecordObject(database, "Connect Port");
                    if (clickedNode.NodeType == NodeType.Start) ConnectPort(clickedNode, pendingPort.Node.nodeId, -1);
                    else ConnectPort(clickedNode, pendingPort.Node.nodeId, clickedChoiceIndex);
                    selectedPorts.Clear();
                    pendingPort = null;
                    actionWasPerformed = true;
                    return true;
                }
            }
            else
            {
                // Source is output (pendingPort), target is input of clicked node
                if (clickedIsInput && clickedNode.NodeType != NodeType.Start)
                {
                    Undo.RecordObject(database, "Connect Port");
                    ConnectPort(pendingPort.Node, clickedNode.nodeId, pendingPort.ChoiceIndex);
                    selectedPorts.Clear();
                    pendingPort = null;
                    actionWasPerformed = true;
                    return true;
                }
            }
            return false;
        }

        #endregion Port Context Menu



        #region Node Height Calculation

        private float GetScaledNodeWidth(DialogNode node)
        {
            if (node.NodeType == NodeType.Start) return START_NODE_WIDTH * zoomScale;
            return BASE_NODE_WIDTH * zoomScale;
        }

        private float GetScaledNodeInputOffset(DialogNode node)
        {
            if (node.NodeType == NodeType.Start) return INPUT_OFFSET_START * zoomScale;
            return INPUT_OFFSET_LINE * zoomScale;
        }

        private int GetNodeHeight(DialogNode node)//if method returns 0, height will be 15
        {
            if (node.NodeType == NodeType.Start) return START_NODE_HEIGHT;

            int baseHeight = LINE_NODE_BASE_HEIGHT;

            if (node.NodeType == NodeType.Line)
            {
                baseHeight += LINE_NODE_HEADER_OFFSET;
                bool hasActions = node.LineData != null && node.LineData.CharacterActions != null && node.LineData.CharacterActions.Count > 0;

                if (hasActions)
                {
                    baseHeight += LINE_NODE_ACTIONS_FOLDOUT;

                    if (actionFoldouts.ContainsKey(node.nodeId) && actionFoldouts[node.nodeId])
                    {
                        baseHeight += node.LineData.CharacterActions.Count * (LINE_NODE_ACTION_ITEM_HEIGHT + NODE_SPACE_OFFSET);

                        foreach (var action in node.LineData.CharacterActions)
                        {
                            if (action.Action == CharacterAction.Show) baseHeight += LINE_NODE_ACTION_SHOW_EXTRA;
                        }
                    }
                }

                foreach (string lang in availableLanguages)
                {
                    bool langVisible;
                    if (languageTextFoldouts.ContainsKey(lang)) langVisible = languageTextFoldouts[lang];
                    else langVisible = true;
                    baseHeight += LINE_NODE_LANG_FOLDOUT;

                    if (langVisible)
                    {
                        baseHeight += LINE_NODE_TEXT_AREA_HEIGHT;
                        if (database != null && database.VoiceClipsEnabled) baseHeight += LINE_NODE_AUDIO_ROW_HEIGHT;
                    }
                }

                return baseHeight;
            }
            else if (node.NodeType == NodeType.Condition)
            {
                baseHeight = CONDITION_NODE_BASE_HEIGHT;

                if (node.ConditionData != null && node.ConditionData.DialogConditions != null && node.ConditionData.DialogConditions.Count != 0)
                {
                    bool conditionsVisible;
                    if (conditionFoldouts.ContainsKey(node.nodeId)) conditionsVisible = conditionFoldouts[node.nodeId];
                    else conditionsVisible = true;
                    baseHeight += CONDITION_NODE_FOLDOUT_OFFSET;
                    if (conditionsVisible)
                    {
                        baseHeight += node.ConditionData.DialogConditions.Count * CONDITION_NODE_CONDITION_HEIGHT;
                    }
                }

                return baseHeight;
            }
            else if (node.NodeType == NodeType.Redirect)
            {
                return GetRedirectNodeContentHeight() + NODE_TOP_PADDING + NODE_BOTTOM_PADDING;
            }
            else if (node.NodeType == NodeType.Choice)
            {
                if (node.ChoiceData != null && node.ChoiceData.Any())
                {
                    foreach (var choice in node.ChoiceData)
                    {
                        if (choice == null) continue;
                        baseHeight += GetChoiceHeight(choice, node.nodeId);
                    }
                }
                else
                {
                    baseHeight = CHOICE_NODE_EMPTY_HEIGHT;
                }
            }

            return baseHeight;
        }

        private int GetChoiceHeight(ChoiceNode choice, string nodeId)
        {
            int height = CHOICE_HEADER_HEIGHT;

            foreach (string lang in availableLanguages)
            {
                bool langVisible;
                if (choiceLanguageFoldouts.ContainsKey(lang)) langVisible = choiceLanguageFoldouts[lang];
                else langVisible = true;
                height += CHOICE_LANG_FOLDOUT;

                if (langVisible)
                {
                    height += CHOICE_TEXT_AREA_HEIGHT;
                }
            }

            if (choice.ChoiceCondition != null && choice.ChoiceCondition.Count > 0)
            {
                bool varVisible;
                if (conditionFoldouts.ContainsKey(nodeId)) varVisible = conditionFoldouts[nodeId];
                else varVisible = true;
                height += CHOICE_HEADER_HEIGHT;

                if (varVisible)
                {
                    height += choice.ChoiceCondition.Count * CHOICE_EVENT_ITEM_HEIGHT;
                }
            }

            if (choice.ChoiceConsequence != null && choice.ChoiceConsequence.Count > 0)
            {
                bool varVisible;
                if (variableChangeFoldouts.ContainsKey(nodeId)) varVisible = variableChangeFoldouts[nodeId];
                else varVisible = true;
                height += CHOICE_HEADER_HEIGHT;

                if (varVisible)
                {
                    height += choice.ChoiceConsequence.Count * CHOICE_EVENT_ITEM_HEIGHT;
                }
            }

            return height;
        }

        #endregion Node Height Calculation

        #region Drawing Node Windows

        private void DrawNodeWindow(int id, DialogNode node)
        {
            const float ZOOM_SLIDER_MIN = 0.0001f;
            const float ZOOM_SLIDER_MAX = 10f;
            const float POSITION_SLIDER_MIN = -2000f;
            const float POSITION_SLIDER_MAX = 2000f;

            // Style definition (at start of method)
            GUIStyle labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = (int)(BASE_FONT_SIZE * zoomScale),
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft
            };
            GUIStyle labelCenterStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = (int)(BASE_FONT_SIZE * zoomScale),
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter
            };
            GUIStyle foldoutToggleStyle = new GUIStyle(GUI.skin.label)// Change from MiddleCenter to MiddleLeft
            {
                fontSize = (int)(BASE_FONT_SIZE * zoomScale),
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleLeft 
            };
            GUIStyle startlabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = (int)(BASE_FONT_SIZE * zoomScale),
                normal = { textColor = START_LABEL_COLOR },
                padding = new RectOffset(0, 0, 0, 0),
            };
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField)
            {
                fontSize = (int)(BASE_FONT_SIZE * zoomScale),
                padding = new RectOffset((int)(2 * zoomScale), (int)(2 * zoomScale), (int)(2 * zoomScale), (int)(2 * zoomScale))
            };
            GUIStyle popupStyle = new GUIStyle(EditorStyles.popup)
            {
                fontSize = (int)(BASE_FONT_SIZE * zoomScale),
                fixedHeight = FIELD_ROW_HEIGHT * zoomScale,
                padding = new RectOffset((int)(5 * zoomScale), (int)(5 * zoomScale), (int)(2 * zoomScale), (int)(2 * zoomScale))
            };
            GUIStyle textAreaStyle = new GUIStyle(GUI.skin.textArea)
            {
                fontSize = (int)(BASE_FONT_SIZE * zoomScale),
                padding = new RectOffset((int)(5 * zoomScale), (int)(5 * zoomScale), (int)(2 * zoomScale), (int)(2 * zoomScale))
            };
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = (int)(BASE_FONT_SIZE * zoomScale),
                padding = new RectOffset((int)(2 * zoomScale), (int)(2 * zoomScale), (int)(2 * zoomScale), (int)(2 * zoomScale))
            };
            GUIStyle toolbarStyle = new GUIStyle(EditorStyles.toolbar)
            {
                fontSize = (int)(BASE_FONT_SIZE * zoomScale),
                fixedHeight = NODE_TOOLBAR_HEIGHT * zoomScale,
                padding = new RectOffset((int)(2 * zoomScale), (int)(2 * zoomScale), 0, 0)
            };
            GUIStyle orangeToolbarStyle = new GUIStyle(EditorStyles.toolbar)         // Orange style for the start node header
            {
                fontSize = (int)(BASE_FONT_SIZE * zoomScale),
                fixedHeight = NODE_TOOLBAR_HEIGHT * zoomScale,
                padding = new RectOffset((int)(2 * zoomScale), (int)(2 * zoomScale), 0, 0),
                normal = { background = MakeTex(1, 1, ORANGE_TOOLBAR) }
            };

            Event e = Event.current; // Current event
            bool isNodeSelected = selectedNodes.Contains(node);
            bool blockNodeInteraction = isNodeSelected && selectedNodes.Count > 1;
            bool wasGuiEnabled = GUI.enabled;
            float toolbarHeightZS = NODE_TOOLBAR_HEIGHT * zoomScale;

            // Scalable paddings like in standard Unity window
            float paddingLeftZS = NODE_LEFT_RIGHT_PADDING * zoomScale;
            float paddingRightZS = paddingLeftZS;
            float paddingTopZS = NODE_TOP_PADDING * zoomScale;
            float paddingBottomZS = NODE_BOTTOM_PADDING * zoomScale;
            float spacingZS = NODE_SPACE_OFFSET * zoomScale;
            float nodeWidthZS = GetScaledNodeWidth(node);
            float contentWidthZS = nodeWidthZS - paddingLeftZS - paddingRightZS;

            float baseHeight = GetNodeHeight(node);
            float minHeight = MIN_NODE_HEIGHT;
            float totalHeight = Mathf.Max(baseHeight, minHeight) * zoomScale;
            Rect contentRect = new Rect(paddingLeftZS, paddingTopZS, contentWidthZS, totalHeight - paddingTopZS - paddingBottomZS);


            if (node.NodeType == NodeType.Start)    // Disable standard window background for Start
            {
                GUI.Box(new Rect(0, 0, nodeWidthZS, totalHeight), GUIContent.none, new GUIStyle());
            }

            Rect toolbarRect = new Rect(0, 0, nodeWidthZS, NODE_TOOLBAR_HEIGHT * zoomScale);
            GUIStyle toolbarStyleForNode;
            if (node.NodeType == NodeType.Start) toolbarStyleForNode = orangeToolbarStyle;
            else toolbarStyleForNode = toolbarStyle;
            GUI.Box(toolbarRect, GUIContent.none, toolbarStyleForNode);

            string title;
            if (node.NodeType == NodeType.Line) title = $"Line {node.nodeId.Substring(0, NODE_ID_DISPLAY_LENGTH)}";
            else if (node.NodeType == NodeType.Choice) title = $"Choice Menu {node.nodeId.Substring(0, NODE_ID_DISPLAY_LENGTH)}";
            else if (node.NodeType == NodeType.Condition) title = $"Condition {node.nodeId.Substring(0, NODE_ID_DISPLAY_LENGTH)}";
            else if (node.NodeType == NodeType.Redirect) title = $"Redirect {node.nodeId.Substring(0, NODE_ID_DISPLAY_LENGTH)}";
            else title = "Start";

            GUIStyle titleLabelStyle;
            if (node.NodeType == NodeType.Start) titleLabelStyle = startlabelStyle;
            else titleLabelStyle = labelStyle;
            EditorGUI.LabelField(new Rect(TITLE_LABEL_OFFSET * zoomScale, 0, TITLE_LABEL_WIDTH_ZOOM * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), title, titleLabelStyle);

            if (blockNodeInteraction) GUI.enabled = false;

            if (node.NodeType == NodeType.Line)// Header buttons
            {
                GUI.backgroundColor = DELETE_BUTTON_COLOR;

                if (GUI.Button(new Rect(nodeWidthZS - spacingZS - NODE_BUTTON_SIZE * zoomScale, 0, NODE_BUTTON_SIZE * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "×", buttonStyle) && e.button == 0)
                {
                    Undo.RecordObject(database, "Remove Node");
                    RemoveDialogNode(selectedDialog, node);
                    GUIUtility.ExitGUI();
                }

                GUI.backgroundColor = Color.white;

                if (GUI.Button(new Rect(nodeWidthZS - 2 * spacingZS - 2 * NODE_BUTTON_SIZE * zoomScale, 0, NODE_BUTTON_SIZE * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "✂", buttonStyle) && e.button == 0)
                {
                    Undo.RecordObject(database, "Duplicate Node");
                    List<LineText> duplicateDialogsText;
                    if (node.LineData.DialogsText != null)
                        duplicateDialogsText = node.LineData.DialogsText.Select(t => new LineText
                        {
                            LanguageCode = t.LanguageCode,
                            SayerName = t.SayerName,
                            Text = t.Text,
                            VoiceClip = t.VoiceClip
                        }).ToList();
                    else
                        duplicateDialogsText = new List<LineText>();

                    List<CharacterActionData> duplicateCharacterActions;
                    if (node.LineData.CharacterActions != null)
                        duplicateCharacterActions = node.LineData.CharacterActions.Select(a => new CharacterActionData
                        {
                            CharacterName = a.CharacterName,
                            EmotionName = a.EmotionName,
                            Action = a.Action,
                            PosX = a.PosX,
                            PosY = a.PosY,
                            Zoom = a.Zoom
                        }).ToList();
                    else
                        duplicateCharacterActions = new List<CharacterActionData>();

                    var duplicate = new DialogNode
                    {
                        NodeType = NodeType.Line,
                        LineData = new LineNode
                        {
                            CharacterName = node.LineData.CharacterName,
                            DialogsText = duplicateDialogsText,
                            CharacterActions = duplicateCharacterActions,
                            NextNodeId = "",
                            isInstant = node.LineData.isInstant
                        },
                        Position = node.Position + new Vector2(DUPLICATE_NODE_OFFSET, DUPLICATE_NODE_OFFSET),
                        nodeId = Guid.NewGuid().ToString(),
                    };
                    selectedDialog.nodes.Add(duplicate);
                }

                if (GUI.Button(new Rect(nodeWidthZS - 3 * spacingZS - 2 * NODE_BUTTON_SIZE * zoomScale - SPRITE_BUTTON_WIDTH * zoomScale, 0, SPRITE_BUTTON_WIDTH * zoomScale,
                    NODE_TOOLBAR_HEIGHT * zoomScale), "+ Sprite", buttonStyle) && e.button == 0)
                {
                    Undo.RecordObject(database, "Add Character Action");
                    node.LineData.CharacterActions.Add(new CharacterActionData { Zoom = DEFAULT_ZOOM_SCALE });
                }

                // "Instant" button - node doesn't wait for click, proceeds immediately
                if (node.LineData.isInstant) GUI.backgroundColor = INSTANT_LINE_COLOR;
                else GUI.backgroundColor = DEFAULT_LINE_COLOR;
                if (GUI.Button(new Rect(nodeWidthZS - LINE_NODE_INSTANT_BUTTON_SPACING_COUNT * spacingZS - LINE_NODE_TOOLBAR_RIGHT_BUTTONS_COUNT * NODE_BUTTON_SIZE
                    * zoomScale - SPRITE_BUTTON_WIDTH * zoomScale - NODE_BUTTON_SIZE * zoomScale, 0, NODE_BUTTON_SIZE * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale),
                    new GUIContent("⚡", /*INSTANT_BUTTON_TOOLTIP*/"Instant line: text and sprites are displayed without waiting for a click"), buttonStyle) && e.button == 0)
                {
                    Undo.RecordObject(database, "Toggle Instant");
                    node.LineData.isInstant = !node.LineData.isInstant;
                }
                GUI.backgroundColor = Color.white;
            }
            else if (node.NodeType == NodeType.Choice)
            {
                GUI.backgroundColor = DELETE_BUTTON_COLOR;    // "Delete" button (5 spacing from right edge)

                if (GUI.Button(new Rect(nodeWidthZS - spacingZS - NODE_BUTTON_SIZE * zoomScale, 0, NODE_BUTTON_SIZE * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "×", buttonStyle) && e.button == 0)
                {
                    Undo.RecordObject(database, "Remove Node");
                    RemoveDialogNode(selectedDialog, node);
                    GUIUtility.ExitGUI();
                }

                GUI.backgroundColor = Color.white;

                if (GUI.Button(new Rect(nodeWidthZS - 2 * spacingZS - 2 * NODE_BUTTON_SIZE * zoomScale, 0, NODE_BUTTON_SIZE * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "✂", buttonStyle) && e.button == 0)   // "Duplicate" button (5 spacing from "Delete" button)
                {
                    Undo.RecordObject(database, "Duplicate Node");
                    List<ChoiceNode> duplicateChoiceData;
                    if (node.ChoiceData != null)
                    {
                        duplicateChoiceData = node.ChoiceData.Select(c =>
                        {
                            List<LineText> translations;
                            if (c?.Translations != null)
                                translations = c.Translations.Select(t => new LineText
                                {
                                    LanguageCode = t.LanguageCode,
                                    SayerName = t.SayerName,
                                    Text = t.Text
                                }).ToList();
                            else
                                translations = new List<LineText>();

                            List<ChoiceCondition> choiceCondition;
                            if (c?.ChoiceCondition != null)
                                choiceCondition = c.ChoiceCondition.Select(cond => new ChoiceCondition
                                {
                                    VariableType = cond.VariableType,
                                    VariableName = cond.VariableName,
                                    ConditionOperator = cond.ConditionOperator,
                                    BoolValue = cond.BoolValue,
                                    IntValue = cond.IntValue,
                                    FloatValue = cond.FloatValue,
                                    StringValue = cond.StringValue
                                }).ToList();
                            else
                                choiceCondition = new List<ChoiceCondition>();

                            List<ChoiceConsequence> choiceConsequence;
                            if (c?.ChoiceConsequence != null)
                                choiceConsequence = c.ChoiceConsequence.Select(cons => new ChoiceConsequence
                                {
                                    VariableType = cons.VariableType,
                                    VariableName = cons.VariableName,
                                    BoolValue = cons.BoolValue,
                                    IntValue = cons.IntValue,
                                    FloatValue = cons.FloatValue,
                                    StringValue = cons.StringValue
                                }).ToList();
                            else
                                choiceConsequence = new List<ChoiceConsequence>();

                            return new ChoiceNode
                            {
                                Translations = translations,
                                ChoiceCondition = choiceCondition,
                                IsAndLogic = c?.IsAndLogic ?? true,
                                ChoiceConsequence = choiceConsequence,
                                NextNodeId = ""
                            };
                        }).ToList();
                    }
                    else
                        duplicateChoiceData = new List<ChoiceNode>();

                    var duplicate = new DialogNode
                    {
                        NodeType = NodeType.Choice,
                        ChoiceData = duplicateChoiceData,
                        Position = node.Position + new Vector2(DUPLICATE_NODE_OFFSET, DUPLICATE_NODE_OFFSET),
                        nodeId = Guid.NewGuid().ToString()
                    };
                    selectedDialog.nodes.Add(duplicate);
                }

                if (GUI.Button(new Rect(nodeWidthZS - 3 * spacingZS - 2 * NODE_BUTTON_SIZE * zoomScale - SPRITE_BUTTON_WIDTH * zoomScale, 0, SPRITE_BUTTON_WIDTH * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "+ Choice", buttonStyle) && e.button == 0)
                {
                    Undo.RecordObject(database, "Add Choice Option");
                    node.ChoiceData.Add(new ChoiceNode());
                    node.ConditionData.DialogConditions.Add(CreateDefaultDialogCondition());
                }
            }
            else if (node.NodeType == NodeType.Condition)
            {
                GUI.backgroundColor = DELETE_BUTTON_COLOR;    // "Delete" button (5 spacing from right edge)

                if (GUI.Button(new Rect(nodeWidthZS - spacingZS - NODE_BUTTON_SIZE * zoomScale, 0, NODE_BUTTON_SIZE * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "×", buttonStyle) && e.button == 0)
                {
                    Undo.RecordObject(database, "Remove Node");
                    RemoveDialogNode(selectedDialog, node);
                    GUIUtility.ExitGUI();
                }

                GUI.backgroundColor = Color.white;

                if (GUI.Button(new Rect(nodeWidthZS - 2 * spacingZS - 2 * NODE_BUTTON_SIZE * zoomScale, 0, NODE_BUTTON_SIZE * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "✂", buttonStyle) && e.button == 0)   // "Duplicate" button (5 spacing from "Delete" button)
                {
                    Undo.RecordObject(database, "Duplicate Node");
                    var duplicate = new DialogNode
                    {
                        NodeType = NodeType.Condition,
                        ConditionData = new ConditionNode
                        {
                            IsAndLogic = node.ConditionData.IsAndLogic,
                            DialogConditions = new List<DialogCondition>(node.ConditionData.DialogConditions.Select(CopyDialogCondition)),
                            TrueNodeId = null,
                            FalseNodeId = null
                        },
                        Position = node.Position + new Vector2(DUPLICATE_NODE_OFFSET, DUPLICATE_NODE_OFFSET),
                        nodeId = Guid.NewGuid().ToString()
                    };
                    selectedDialog.nodes.Add(duplicate);
                }

                if (GUI.Button(new Rect(nodeWidthZS - 3 * spacingZS - 2 * NODE_BUTTON_SIZE * zoomScale - SPRITE_BUTTON_WIDTH * zoomScale, 0, SPRITE_BUTTON_WIDTH * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "+ Condition", buttonStyle) && e.button == 0)
                {
                    Undo.RecordObject(database, "Add Condition");
                    node.ConditionData.DialogConditions.Add(CreateDefaultDialogCondition());
                }
            }
            else if (node.NodeType == NodeType.Redirect)
            {
                DrawRedirectNodeToolbar(node, nodeWidthZS, spacingZS, zoomScale, buttonStyle, e);
            }
            else if (node.NodeType == NodeType.Start)
            {
                GUI.backgroundColor = DELETE_BUTTON_COLOR;    // "Delete" button (5 spacing from right edge)

                if (GUI.Button(new Rect(nodeWidthZS - spacingZS - NODE_BUTTON_SIZE * zoomScale, 0, NODE_BUTTON_SIZE * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "×", buttonStyle) && e.button == 0)
                {
                    Undo.RecordObject(database, "Remove Node");
                    RemoveDialogNode(selectedDialog, node);
                    GUIUtility.ExitGUI();
                }

                GUI.backgroundColor = Color.white;
            }

            GUILayout.BeginArea(contentRect);   // Content of the node window after the header
            Rect conditionAreaRect = new Rect(0, 0, contentWidthZS, totalHeight - paddingTopZS - paddingBottomZS);
            float currentY = 0;

            if (node.NodeType == NodeType.Start)
            {

            }
            else if (node.NodeType == NodeType.Condition)
            {
                if (node.ConditionData == null)
                {
                    node.ConditionData = new ConditionNode();
                }

                GUILayout.BeginArea(conditionAreaRect);
                currentY += CONDITION_HEADER_OFFSET * zoomScale;

                // Move the AND/OR button under the label
                if (node.ConditionData.DialogConditions.Count >= 2)
                {

                    GUI.Label(new Rect(CONDITION_LOGIC_LABEL_X * zoomScale, currentY, CONDITION_LOGIC_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), "AND/OR toggle:", labelCenterStyle);
                    bool currentLogic = node.ConditionData.IsAndLogic;

                    Rect logicToggleRect = new Rect(CONDITION_LOGIC_TOGGLE_X * zoomScale, currentY, FIELD_ROW_HEIGHT * zoomScale, FIELD_ROW_HEIGHT * zoomScale);

                    string logicButtonLabel;
                    if (currentLogic) logicButtonLabel = "&&";
                    else logicButtonLabel = "||";
                    if (GUI.Button(logicToggleRect, logicButtonLabel, buttonStyle) && e.button == 0)
                    {
                        Undo.RecordObject(database, "Toggle Condition Logic");
                        node.ConditionData.IsAndLogic = !node.ConditionData.IsAndLogic;
                    }
                }

                float truePortY_ZS = CONDITION_TRUE_PORT_Y * zoomScale;
                float falsePortY_ZS = (CONDITION_TRUE_PORT_Y + NODE_SPACE_OFFSET + NODE_PORT_SIZE) * zoomScale;

                Rect trueLabelRect = new Rect(nodeWidthZS - CONDITION_PORT_LABEL_OFFSET * zoomScale, truePortY_ZS, CONDITION_PORT_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                EditorGUI.DrawRect(new Rect(nodeWidthZS - CONDITION_PORT_LABEL_OFFSET * zoomScale, truePortY_ZS, CONDITION_PORT_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), CONDITION_TRUE_COLOR);
                GUI.Label(trueLabelRect, "TRUE", labelCenterStyle);

                Rect falseLabelRect = new Rect(nodeWidthZS - CONDITION_PORT_LABEL_OFFSET * zoomScale, falsePortY_ZS, CONDITION_PORT_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                EditorGUI.DrawRect(new Rect(nodeWidthZS - CONDITION_PORT_LABEL_OFFSET * zoomScale, falsePortY_ZS, CONDITION_PORT_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), CONDITION_FALSE_COLOR);
                GUI.Label(falseLabelRect, "FALSE", labelCenterStyle);

                currentY += CONDITION_TOGGLE_OFFSET * zoomScale;


                if (node.ConditionData.DialogConditions != null && node.ConditionData.DialogConditions.Count > 0)   // Condition list
                {
                    string foldoutKey = node.nodeId;
                    if (!conditionFoldouts.ContainsKey(foldoutKey))
                    {
                        conditionFoldouts[foldoutKey] = true;
                    }

                    Rect toggleRect = new Rect(0, currentY, contentWidthZS, ACTION_ROW_HEIGHT * zoomScale);
                    bool isFolded = conditionFoldouts[foldoutKey];
                    if (e.button == 0 && e.type == EventType.MouseDown && toggleRect.Contains(e.mousePosition))
                    {
                        conditionFoldouts[foldoutKey] = !conditionFoldouts[foldoutKey];
                        e.Use();
                    }
                    string conditionsFoldoutLabel;
                    if (isFolded) conditionsFoldoutLabel = "▽ Conditions";
                    else conditionsFoldoutLabel = "▶ Conditions";
                    GUI.Toggle(toggleRect, isFolded, conditionsFoldoutLabel, foldoutToggleStyle);
                    currentY += ACTION_ROW_HEIGHT * zoomScale;

                    if (conditionFoldouts[foldoutKey])
                    {
                        List<int> conditionsToRemove = new List<int>();
                        for (int i = 0; i < node.ConditionData.DialogConditions.Count; i++)
                        {
                            var condition = node.ConditionData.DialogConditions[i];
                            float conditionHeight = CONDITION_NODE_CONDITION_HEIGHT * zoomScale;
                            Rect conditionRect = new Rect(0, currentY, contentWidthZS, conditionHeight);
                            GUI.Box(conditionRect, GUIContent.none, EditorStyles.helpBox);

                            float fieldY = currentY + 5 * zoomScale;

                            var shellParams = new ConditionRowShellParams
                            {
                                Layout = ConditionRowShellLayout.GraphConditionNode,
                                Index = i,
                                ReferenceState = condition.ReferenceState,
                                VariableName = condition.VariableName,
                                MissingReferenceLabel = condition.MissingReferenceLabel,
                                ContentWidth = contentWidthZS,
                                RowBaseY = currentY,
                                ZoomScale = zoomScale,
                                WarningBoundsRect = conditionRect,
                                LabelStyle = labelStyle,
                                ButtonStyle = buttonStyle,
                                RequireMouseButton = true,
                                CurrentEvent = e
                            };

                            if (DrawConditionRowShell(shellParams, conditionsToRemove, out fieldY))
                            {
                                currentY += conditionHeight;
                                continue;
                            }

                            DrawDialogConditionEditableFields(
                                condition,
                                i,
                                node.ConditionData.DialogConditions,
                                ref fieldY,
                                new ConditionFieldsDrawContext
                                {
                                    ZoomScale = zoomScale,
                                    ContentWidth = contentWidthZS,
                                    LabelStyle = labelStyle,
                                    PopupStyle = popupStyle,
                                    TextFieldStyle = textFieldStyle,
                                    ButtonStyle = buttonStyle,
                                    BlockInteraction = blockNodeInteraction,
                                    RequireMouseButton = true,
                                    CurrentEvent = e
                                });

                            currentY += conditionHeight;
                        }

                        if (conditionsToRemove.Count > 0)
                        {
                            Undo.RecordObject(database, "Remove Condition");
                            foreach (int index in conditionsToRemove.OrderByDescending(i => i))
                            {
                                node.ConditionData.DialogConditions.RemoveAt(index);
                            }
                        }
                    }
                }

                GUILayout.EndArea();
            }
            else if (node.NodeType == NodeType.Redirect)
            {
                if (node.RedirectData == null) node.RedirectData = CreateDefaultRedirectNodeData();

                GUILayout.BeginArea(conditionAreaRect);
                DrawRedirectNodeContent(node, contentWidthZS, zoomScale, labelStyle, popupStyle);
                GUILayout.EndArea();
            }
            else if (node.NodeType == NodeType.Line)
            {
                if (node.LineData == null)
                {
                    node.LineData = new LineNode { CharacterActions = new List<CharacterActionData>(), DialogsText = new List<LineText>() };
                }

                bool hasActions = node.LineData.CharacterActions != null && node.LineData.CharacterActions.Count > 0;
                if (hasActions)
                {
                    if (!actionFoldouts.ContainsKey(node.nodeId))
                    {
                        actionFoldouts[node.nodeId] = true;
                    }

                    Rect toggleRect = new Rect(0, currentY, FOLDOUT_ARROW_WIDTH + contentWidthZS - ACTION_ROW_HEIGHT * zoomScale, ACTION_ROW_HEIGHT * zoomScale);
                    string actionsFoldoutLabel;
                    if (actionFoldouts[node.nodeId]) actionsFoldoutLabel = "▽ Character actions";
                    else actionsFoldoutLabel = "▶ Character actions";
                    actionFoldouts[node.nodeId] = GUI.Toggle(toggleRect, actionFoldouts[node.nodeId], actionsFoldoutLabel, foldoutToggleStyle);
                    currentY += ACTION_ROW_HEIGHT * zoomScale;

                    if (actionFoldouts[node.nodeId])
                    {
                        List<int> actionsToRemove = new List<int>();
                        for (int k = 0; k < node.LineData.CharacterActions.Count; k++)
                        {
                            var action = node.LineData.CharacterActions[k];

                            float showActionExtra;
                            if (action.Action == CharacterAction.Show) showActionExtra = ACTION_ROW_HEIGHT * zoomScale;
                            else showActionExtra = 0;
                            float actionHeight = ACTION_EXTRA_HEIGHT * zoomScale + showActionExtra;
                            float actionBoxStartY = currentY;
                            Rect actionBoxRect = new Rect(0, actionBoxStartY, contentWidthZS, actionHeight);
                            GUI.Box(actionBoxRect, GUIContent.none, EditorStyles.helpBox);
                            float innerWidth = contentWidthZS - 2 * spacingZS;
                            currentY += spacingZS;

                            if (DialogReferenceUtility.IsMissing(action.ReferenceState))
                            {
                                string missingMessage;
                                if (string.IsNullOrEmpty(action.MissingReferenceLabel)) missingMessage = DialogReferenceMessages.MISSING_SPRITE_SLOT;
                                else missingMessage = action.MissingReferenceLabel;

                                DrawMissingReferenceWarning(
                                    new Rect(spacingZS, actionBoxStartY + spacingZS, innerWidth, actionHeight - 2 * spacingZS),
                                    missingMessage,
                                    zoomScale,
                                    labelStyle);

                                if (GUI.Button(new Rect(contentWidthZS - spacingZS - ACTION_BUTTON_SIZE * zoomScale, actionBoxStartY + spacingZS, ACTION_BUTTON_SIZE * zoomScale, FIELD_ROW_HEIGHT * zoomScale), "×", buttonStyle) && e.button == 0)
                                    actionsToRemove.Add(k);

                                currentY = actionBoxStartY + actionHeight + spacingZS;
                                continue;
                            }

                            Rect characterLabelRect = new Rect(spacingZS, currentY, 70 * zoomScale, 20 * zoomScale);
                            GUI.Label(characterLabelRect, "Character:", labelStyle);

                            if (database != null && database.CharacterGroups != null && database.CharacterGroups.Count > 0)
                            {
                                List<string> characterNamesActions = new List<string> { "None" };
                                characterNamesActions.AddRange(database.CharacterGroups.Select(g => g.characterName));

                                HashSet<string> usedCharactersInLine = new HashSet<string>();
                                for (int m = 0; m < node.LineData.CharacterActions.Count; m++)
                                {
                                    if (m != k && !string.IsNullOrEmpty(node.LineData.CharacterActions[m].CharacterName))
                                    {
                                        usedCharactersInLine.Add(node.LineData.CharacterActions[m].CharacterName);
                                    }
                                }

                                List<string> availableCharacterNames = characterNamesActions.Where(name => name == "None" || !usedCharactersInLine.Contains(name)).ToList();

                                int characterIndex;
                                if (string.IsNullOrEmpty(action.CharacterName)) characterIndex = 0;
                                else characterIndex = availableCharacterNames.IndexOf(action.CharacterName);
                                if (characterIndex == -1) characterIndex = 0;

                                Rect popupRect = new Rect(70 * zoomScale + spacingZS, currentY, 105 * zoomScale, 20 * zoomScale);
                                EditorGUI.BeginChangeCheck();
                                int newCharacterIndex = EditorGUI.Popup(popupRect, characterIndex, availableCharacterNames.ToArray(), popupStyle);
                                if (EditorGUI.EndChangeCheck() && newCharacterIndex != characterIndex && e.button == 0)
                                {
                                    Undo.RecordObject(database, "Change Character Name in Action");
                                    if (newCharacterIndex > 0)
                                    {
                                        string selectedCharacterName = availableCharacterNames[newCharacterIndex];
                                        CharacterGroup characterGroup = database.CharacterGroups.FirstOrDefault(g => g.characterName == selectedCharacterName);
                                        action.CharacterName = selectedCharacterName;
                                        if (characterGroup != null && characterGroup.sprites.Count > 0) action.EmotionName = characterGroup.sprites[0].spriteName;
                                        else action.EmotionName = "";
                                    }
                                    else
                                    {
                                        action.CharacterName = "";
                                        action.EmotionName = "";
                                    }
                                }

                                if (!string.IsNullOrEmpty(action.CharacterName))
                                {
                                    string selectedCharacterName = availableCharacterNames[newCharacterIndex];
                                    CharacterGroup characterGroup = database.CharacterGroups.FirstOrDefault(g => g.characterName == selectedCharacterName);

                                    if (characterGroup != null && characterGroup.sprites.Count > 0)
                                    {
                                        Rect emotionLabelRect = new Rect(185 * zoomScale + spacingZS, currentY, 60 * zoomScale, 20 * zoomScale);
                                        GUI.Label(emotionLabelRect, "Emotion:", labelStyle);

                                        List<string> spriteNames = characterGroup.sprites.Select(s => s.spriteName).ToList();
                                        int emotionIndex;
                                        if (string.IsNullOrEmpty(action.EmotionName)) emotionIndex = 0;
                                        else emotionIndex = spriteNames.IndexOf(action.EmotionName);
                                        if (emotionIndex == -1) emotionIndex = 0;

                                        Rect emotionPopupRect = new Rect(240 * zoomScale + spacingZS, currentY, 105 * zoomScale, 20 * zoomScale);
                                        EditorGUI.BeginChangeCheck();
                                        int newEmotionIndex = EditorGUI.Popup(emotionPopupRect, emotionIndex, spriteNames.ToArray(), popupStyle);
                                        if (EditorGUI.EndChangeCheck() && newEmotionIndex != emotionIndex && e.button == 0)
                                        {
                                            Undo.RecordObject(database, "Change Emotion Name in Action");
                                            action.EmotionName = spriteNames[newEmotionIndex];
                                        }
                                    }
                                }
                            }

                            float btnW = ACTION_BUTTON_SIZE * zoomScale;
                            float btnGap = ACTION_BUTTON_GAP * zoomScale;
                            float rightPad = spacingZS;
                            float xRemove = contentWidthZS - rightPad - ACTION_BUTTON_SIZE * zoomScale;
                            float xCopyOrPaste = xRemove - btnGap - btnW;
                            float xPreview = xCopyOrPaste - btnGap - btnW;

                            // Sprite preview button
                            if (GUI.Button(new Rect(xPreview, currentY, btnW, FIELD_ROW_HEIGHT * zoomScale), new GUIContent("👁", "Preview sprite"), buttonStyle) && e.button == 0)
                            {
                                if (database != null && !string.IsNullOrEmpty(action.CharacterName)) SpritePreviewWindow.ShowPreview(action, database, node.LineData?.CharacterActions);
                            }

                            // Copy or paste button (replaced depending on buffer data)
                            bool canPasteCopiedAction = copiedActionData != null && copiedActionSourceNodeId != node.nodeId;

                            if (canPasteCopiedAction)
                            {
                                if (GUI.Button(new Rect(xCopyOrPaste, currentY, btnW, FIELD_ROW_HEIGHT * zoomScale), new GUIContent("📥", "Paste action parameters"), buttonStyle) && e.button == 0)
                                {
                                    Undo.RecordObject(database, "Paste Action Parameters");
                                    action.CharacterName = copiedActionData.CharacterName;
                                    action.EmotionName = copiedActionData.EmotionName;
                                    action.Action = copiedActionData.Action;
                                    action.PosX = copiedActionData.PosX;
                                    action.PosY = copiedActionData.PosY;
                                    action.Zoom = copiedActionData.Zoom;
                                    action.ReferenceState = DialogReferenceState.Valid;
                                    action.MissingReferenceLabel = "";
                                    // SaveDialogGraphChanges();
                                    GUI.changed = true;
                                }
                            }
                            else
                            {
                                if (GUI.Button(new Rect(xCopyOrPaste, currentY, btnW, FIELD_ROW_HEIGHT * zoomScale), new GUIContent("📋", "Copy action parameters"), buttonStyle) && e.button == 0)
                                {
                                    copiedActionSourceNodeId = node.nodeId;
                                    copiedActionData = new CharacterActionData
                                    {
                                        CharacterName = action.CharacterName,
                                        EmotionName = action.EmotionName,
                                        Action = action.Action,
                                        PosX = action.PosX,
                                        PosY = action.PosY,
                                        Zoom = action.Zoom
                                    };
                                }
                            }

                            //GUI.backgroundColor = new Color(1f, 0.7f, 0.7f);
                            Rect removeButtonRect = new Rect(xRemove, currentY, ACTION_BUTTON_SIZE * zoomScale, FIELD_ROW_HEIGHT * zoomScale);

                            if (GUI.Button(removeButtonRect, "×", buttonStyle) && e.button == 0) actionsToRemove.Add(k);
                            
                            GUI.backgroundColor = Color.white;
                            currentY += ACTION_ROW_HEIGHT * zoomScale;
                            Rect actionLabelRect = new Rect(spacingZS, currentY, ACTION_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                            GUI.Label(actionLabelRect, "Action:", labelStyle);
                            Rect actionPopupRect = new Rect(ACTION_LABEL_WIDTH * zoomScale + spacingZS, currentY, ACTION_POPUP_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                            EditorGUI.BeginChangeCheck();
                            CharacterAction newAction = (CharacterAction)EditorGUI.EnumPopup(actionPopupRect, action.Action, popupStyle);

                            if (EditorGUI.EndChangeCheck() && e.button == 0)
                            {
                                Undo.RecordObject(database, "Change Action in Action");
                                action.Action = newAction;
                            }

                            if (action.Action == CharacterAction.Show)
                            {
                                Rect zoomLabelRect = new Rect(ACTION_ZOOM_LABEL_X * zoomScale + spacingZS, currentY, ACTION_ZOOM_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                                GUI.Label(zoomLabelRect, "Zoom:", labelStyle);
                                Rect zoomSliderRect = new Rect(ACTION_ZOOM_SLIDER_X * zoomScale + spacingZS, currentY, innerWidth - ACTION_ZOOM_SLIDER_X * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                                EditorGUI.BeginChangeCheck();
                                Undo.RecordObject(database, "Change Zoom in Action");
                                float newZoom = EditorGUI.Slider(zoomSliderRect, action.Zoom, ZOOM_SLIDER_MIN, ZOOM_SLIDER_MAX);

                                if (EditorGUI.EndChangeCheck() && e.button == 0) action.Zoom = newZoom;

                                currentY += ACTION_ROW_HEIGHT * zoomScale;
                                Rect xLabelRect = new Rect(spacingZS, currentY, ACTION_XY_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                                GUI.Label(xLabelRect, "X:", labelStyle);
                                float sliderWidth = (innerWidth - ACTION_XY_OFFSET * zoomScale - spacingZS) / 2;
                                Rect xSliderRect = new Rect(ACTION_XY_LABEL_WIDTH * zoomScale + spacingZS, currentY, sliderWidth, FIELD_ROW_HEIGHT * zoomScale);
                                EditorGUI.BeginChangeCheck();
                                Undo.RecordObject(database, "Change X Position in Action");
                                float newXPos = EditorGUI.Slider(xSliderRect, action.PosX, POSITION_SLIDER_MIN, POSITION_SLIDER_MAX);

                                if (EditorGUI.EndChangeCheck() && e.button == 0) action.PosX = newXPos;

                                Rect yLabelRect = new Rect(ACTION_XY_LABEL_WIDTH * zoomScale + spacingZS + sliderWidth + spacingZS, currentY, ACTION_XY_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                                GUI.Label(yLabelRect, "Y:", labelStyle);
                                Rect ySliderRect = new Rect(ACTION_XY_OFFSET * zoomScale + spacingZS + sliderWidth + spacingZS, currentY, sliderWidth, FIELD_ROW_HEIGHT * zoomScale);
                                EditorGUI.BeginChangeCheck();
                                Undo.RecordObject(database, "Change Y Position in Action");
                                float newYPos = EditorGUI.Slider(ySliderRect, action.PosY, POSITION_SLIDER_MIN, POSITION_SLIDER_MAX);

                                if (EditorGUI.EndChangeCheck() && e.button == 0) action.PosY = newYPos;

                                currentY += ACTION_ROW_HEIGHT * zoomScale;
                            }
                            else
                            {
                                currentY += ACTION_ROW_HEIGHT * zoomScale;
                            }

                            currentY += spacingZS; // Bottom padding of action block
                            currentY += spacingZS;       // Additional padding between action blocks
                        }

                        if (actionsToRemove.Count > 0)
                        {
                            Undo.RecordObject(database, "Remove Character Action");
                            foreach (int index in actionsToRemove.OrderByDescending(i => i))
                            {
                                node.LineData.CharacterActions.RemoveAt(index);
                            }
                        }
                    }
                }

                currentY += spacingZS;

                Rect characterLabelRect2 = new Rect(0, currentY, LINE_CHARACTER_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                GUI.Label(characterLabelRect2, "Name color:", labelStyle);

                if (DialogReferenceUtility.IsMissing(node.LineData.CharacterReferenceState))
                {
                    DrawMissingReferenceWarning(
                        new Rect(LINE_CHARACTER_LABEL_WIDTH * zoomScale, currentY, contentWidthZS - LINE_CHARACTER_LABEL_WIDTH * zoomScale, LINE_CHARACTER_ROW_HEIGHT * zoomScale),
                        DialogReferenceMessages.FormatMissingCharacterLabel(node.LineData.MissingCharacterReferenceLabel),
                        zoomScale,
                        labelStyle);
                }
                else
                {
                    List<string> characterNames = database.CharacterNameColors?.Select(c => c.Name).ToList() ?? new List<string>();
                    characterNames.Insert(0, "None");
                    int selectedCharacterIndex;

                    if (string.IsNullOrEmpty(node.LineData.CharacterName)) selectedCharacterIndex = 0;
                    else selectedCharacterIndex = characterNames.IndexOf(node.LineData.CharacterName);

                    if (selectedCharacterIndex == -1) selectedCharacterIndex = 0;

                    Rect characterPopupRect = new Rect(LINE_CHARACTER_LABEL_WIDTH * zoomScale, currentY, LINE_CHARACTER_POPUP_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                    EditorGUI.BeginChangeCheck();
                    int newSelectedCharacterIndex = EditorGUI.Popup(characterPopupRect, selectedCharacterIndex, characterNames.ToArray(), popupStyle);

                    if (EditorGUI.EndChangeCheck() && newSelectedCharacterIndex != selectedCharacterIndex && e.button == 0)
                    {

                        Undo.RecordObject(database, "Change Character Name for Line");
                        string selectedName;

                        if (newSelectedCharacterIndex > 0) selectedName = characterNames[newSelectedCharacterIndex];
                        else selectedName = "";

                        node.LineData.CharacterName = selectedName;

                        if (!string.IsNullOrEmpty(selectedName))
                        {
                            var colorEntry = database.CharacterNameColors?.FirstOrDefault(c => c.Name == selectedName);

                            if (colorEntry != null)
                            {
                                foreach (string lang in availableLanguages)
                                {
                                    var lt = node.LineData.DialogsText?.Find(t => t.LanguageCode == lang);
                                    
                                    if (lt == null)
                                    {
                                        lt = new LineText { LanguageCode = lang, SayerName = "", Text = "" };

                                        if (node.LineData.DialogsText == null) node.LineData.DialogsText = new List<LineText>();

                                        node.LineData.DialogsText.Add(lt);
                                    }

                                    lt.SayerName = colorEntry.GetDisplayNameForLanguage(lang);
                                }
                            }
                        }
                        else
                        {
                            foreach (var lt in node.LineData.DialogsText ?? Enumerable.Empty<LineText>())
                            {
                                lt.SayerName = "";                            
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(node.LineData.CharacterName))
                    {
                        var colorEntry = database.CharacterNameColors?.FirstOrDefault(c => c.Name == node.LineData.CharacterName);

                        if (colorEntry != null && IsValidHexColor(colorEntry.ColorHex) && ColorUtility.TryParseHtmlString(colorEntry.ColorHex, out Color color))
                        {
                            float size = COLOR_PREVIEW_SIZE * zoomScale;
                            float centerY = currentY + (LINE_CHARACTER_ROW_HEIGHT * zoomScale) * 0.5f;
                            Rect colorRect = new Rect(COLOR_PREVIEW_OFFSET * zoomScale, centerY - size * 0.5f, size, size);
                            EditorGUI.DrawRect(colorRect, color);
                        }
                    }

                    currentY += LINE_CHARACTER_ROW_HEIGHT * zoomScale;
                }

                foreach (string lang in availableLanguages)
                {
                    if (!languageTextFoldouts.ContainsKey(lang)) languageTextFoldouts[lang] = true;

                    Rect langToggleRect = new Rect(0, currentY, contentWidthZS, ACTION_ROW_HEIGHT * zoomScale);
                    bool langFolded = languageTextFoldouts[lang];

                    if (e.button == 0)
                    {
                        string langFoldoutLabel;

                        if (langFolded) langFoldoutLabel = $"▽ Language: {lang}";
                        else langFoldoutLabel = $"▶ Language: {lang}";

                        languageTextFoldouts[lang] = GUI.Toggle(langToggleRect, languageTextFoldouts[lang], langFoldoutLabel, foldoutToggleStyle);
                    }
                    currentY += ACTION_ROW_HEIGHT * zoomScale;

                    if (languageTextFoldouts[lang])
                    {
                        Rect sayerLabelRect = new Rect(0, currentY, LINE_SAYER_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                        GUI.Label(sayerLabelRect, "Speaker name:", labelStyle);
                        LineText localizedText = node.LineData.DialogsText.FirstOrDefault(t => t.LanguageCode == lang);

                        if (localizedText == null)
                        {
                            localizedText = new LineText { LanguageCode = lang, SayerName = "", Text = "" };
                            node.LineData.DialogsText.Add(localizedText);
                        }

                        Rect sayerFieldRect = new Rect((LINE_SAYER_LABEL_WIDTH + LINE_SAYER_LABEL_SPACING) * zoomScale, currentY, LINE_SAYER_FIELD_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                        EditorGUI.BeginChangeCheck();
                        string newSayerName = EditorGUI.TextField(sayerFieldRect, localizedText.SayerName ?? "", textFieldStyle);
                        string colorHex = "#000000";

                        if (!string.IsNullOrEmpty(node.LineData.CharacterName))
                        {
                            var colorEntry = database.CharacterNameColors?.FirstOrDefault(c => c.Name == node.LineData.CharacterName);
                            if (colorEntry != null && IsValidHexColor(colorEntry.ColorHex))
                            {
                                colorHex = colorEntry.ColorHex;
                            }
                        }

                        GUIStyle styledLabel = new GUIStyle(labelStyle);

                        if (ColorUtility.TryParseHtmlString(colorHex, out Color color)) styledLabel.normal.textColor = color;
                        
                        Rect sayerPreviewRect = new Rect(SAYER_PREVIEW_X * zoomScale, currentY, SAYER_PREVIEW_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale);
                        GUI.Label(sayerPreviewRect, localizedText.SayerName ?? "", styledLabel);

                        if (EditorGUI.EndChangeCheck() && e.button == 0)
                        {
                            Undo.RecordObject(database, "Change Sayer Name");
                            localizedText.SayerName = newSayerName;
                        }

                        currentY += ACTION_ROW_HEIGHT * zoomScale;
                        GUI.backgroundColor = TEXT_AREA_BG;
                        Rect textAreaRect = new Rect(0, currentY, contentWidthZS, LINE_TEXT_AREA_HEIGHT * zoomScale);
                        EditorGUI.BeginChangeCheck();
                        Undo.RecordObject(database, "Change Dialog Text");
                        string newText = EditorGUI.TextArea(textAreaRect, localizedText.Text ?? "", textAreaStyle);

                        if (EditorGUI.EndChangeCheck() && e.button == 0) localizedText.Text = newText;

                        GUI.backgroundColor = Color.white;
                        currentY += CHOICE_TEXT_AREA_HEIGHT * zoomScale;

                        if (database.VoiceClipsEnabled) currentY = DrawLineVoiceClipRow(localizedText, currentY, contentWidthZS, zoomScale, labelStyle, buttonStyle, e);
                    }
                }
            }
            else if (node.NodeType == NodeType.Choice) 
            {
                currentY += spacingZS; // Initial spacing
                List<int> choicesToRemove = new List<int>();

                for (int i = 0; i < node.ChoiceData.Count; i++)
                {
                    ChoiceNode choice = node.ChoiceData[i];

                    if (choice == null) continue;

                    Rect horizontalRect = new Rect(0, currentY, contentWidthZS, FIELD_ROW_HEIGHT * zoomScale);

                    GUI.BeginGroup(horizontalRect);
                    {
                        float x = spacingZS * 2;

                        GUI.Label(new Rect(x, 0, CHOICE_ROW_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), $"  {i + 1} Choice Option", labelStyle);
                        //x += 115 * zoomScale; // 110+5

                        if (GUI.Button(new Rect(contentWidthZS - CHOICE_CLOSE_BUTTON_OFFSET * zoomScale, 0, ACTION_BUTTON_SIZE * zoomScale, FIELD_ROW_HEIGHT * zoomScale), new GUIContent("×", "Delete choice option"), buttonStyle)) // -35 = -20 (button width) -10 (1/4 port width) - 5 (padding)
                        {
                            Undo.RecordObject(database, "Remove Choice");
                            choicesToRemove.Add(i);
                        }

                        if (GUI.Button(new Rect(contentWidthZS - CHOICE_CONSEQUENCE_BUTTON_OFFSET * zoomScale, 0, CHOICE_CONSEQUENCE_BUTTON_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), "+ Consequence", buttonStyle)) // contentWidthZS  - (35 - 95 - 5) * zoomScale
                        {
                            Undo.RecordObject(database, "Add Choice Consequence");
                            choice.ChoiceConsequence.Add(CreateDefaultChoiceConsequence());
                        }

                        if (GUI.Button(new Rect(contentWidthZS - CHOICE_CONDITION_BUTTON_OFFSET * zoomScale, 0, CHOICE_CONDITION_BUTTON_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), "+ Condition", buttonStyle)) // contentWidthZS  - (35 - 95 - 5 - 80 - 5) * zoomScale
                        {
                            Undo.RecordObject(database, "Add Choice Condition");
                            choice.ChoiceCondition.Add(CreateDefaultChoiceCondition());
                        }
                    }

                    GUI.EndGroup();
                    currentY += FIELD_ROW_HEIGHT * zoomScale;

                    if (choice.ChoiceCondition != null && choice.ChoiceCondition.Count > 0) // Conditions
                    {
                        string varFoldoutKey = node.nodeId;
                        if (!conditionFoldouts.ContainsKey(varFoldoutKey))
                        {
                            conditionFoldouts[varFoldoutKey] = true;
                        }

                        Rect varToggleRect = new Rect(0, currentY, CHOICE_FOLDOUT_WIDTH * zoomScale, ACTION_ROW_HEIGHT * zoomScale);
                        bool varFolded = conditionFoldouts[varFoldoutKey];

                        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && varToggleRect.Contains(Event.current.mousePosition))
                        {
                            conditionFoldouts[varFoldoutKey] = !conditionFoldouts[varFoldoutKey];
                            Event.current.Use();
                        }

                        string choiceConditionsFoldoutLabel;

                        if (varFolded) choiceConditionsFoldoutLabel = "▽ Choice visibility conditions";
                        else choiceConditionsFoldoutLabel = "▶ Choice visibility conditions";

                        GUI.Toggle(varToggleRect, varFolded, choiceConditionsFoldoutLabel, foldoutToggleStyle);

                        if (conditionFoldouts[varFoldoutKey])
                        {

                            if (choice.ChoiceCondition != null && choice.ChoiceCondition.Count >= 2)
                            {
                                float logicRowY = currentY + CHOICE_LOGIC_ROW_Y_OFFSET * zoomScale;
                                float logicToggleSize = CHOICE_LOGIC_TOGGLE_SIZE * zoomScale;
                                float logicGap = CHOICE_LOGIC_GAP * zoomScale;
                                float logicLabelWidth = CHOICE_LOGIC_LABEL_WIDTH * zoomScale;
                                float logicToggleX = contentWidthZS - (CHOICE_LOGIC_RIGHT_PADDING + CHOICE_LOGIC_TOGGLE_SIZE) * zoomScale;
                                float logicLabelX = logicToggleX - logicGap - logicLabelWidth;
                                GUIStyle logicLabelStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleRight };
                                GUI.Label(new Rect(logicLabelX, logicRowY, logicLabelWidth, logicToggleSize), "AND/OR toggle:", logicLabelStyle);
                                Rect varToggleOrAnd = new Rect(logicToggleX, logicRowY, logicToggleSize, logicToggleSize);

                                if (choice.IsAndLogic)
                                {
                                    if (GUI.Button(varToggleOrAnd, "&&", buttonStyle))
                                    {
                                        Undo.RecordObject(database, "Toggle Choice Condition Logic");
                                        choice.IsAndLogic = false;
                                    }
                                }
                                else
                                {
                                    if (GUI.Button(varToggleOrAnd, "||", buttonStyle))
                                    {
                                        Undo.RecordObject(database, "Toggle Choice Condition Logic");
                                        choice.IsAndLogic = true;
                                    }
                                }

                            }
                        }

                        currentY += ACTION_ROW_HEIGHT * zoomScale;

                        if (conditionFoldouts[varFoldoutKey])
                        {
                            List<int> varsToRemove = new List<int>();

                            for (int j = 0; j < choice.ChoiceCondition.Count; j++)
                            {
                                var varChange = choice.ChoiceCondition[j];
                                float eventHeight = ACTION_ROW_HEIGHT * zoomScale;
                                Rect eventBoxRect = new Rect(0, currentY, contentWidthZS, eventHeight);
                                GUI.Box(eventBoxRect, GUIContent.none, EditorStyles.helpBox);
                                float fieldY = currentY + CHOICE_EVENT_FIELD_TOP_PADDING * zoomScale;

                                var shellParams = new ConditionRowShellParams
                                {
                                    Layout = ConditionRowShellLayout.ChoiceConditionRow,
                                    Index = j,
                                    ReferenceState = varChange.ReferenceState,
                                    VariableName = varChange.VariableName,
                                    MissingReferenceLabel = varChange.MissingReferenceLabel,
                                    ContentWidth = contentWidthZS,
                                    RowBaseY = currentY,
                                    ZoomScale = zoomScale,
                                    WarningBoundsRect = eventBoxRect,
                                    LabelStyle = labelStyle,
                                    ButtonStyle = buttonStyle,
                                    DeleteTooltip = "Delete condition"
                                };

                                if (DrawConditionRowShell(shellParams, varsToRemove, out fieldY))
                                {
                                    currentY += eventHeight + CHOICE_EVENT_ITEM_SPACING * zoomScale;
                                    continue;
                                }

                                float fieldX = spacingZS;
                                DrawChoiceConditionEditableFields(
                                    varChange,
                                    j,
                                    choice.ChoiceCondition,
                                    fieldY,
                                    ref fieldX,
                                    new ConditionFieldsDrawContext
                                    {
                                        ZoomScale = zoomScale,
                                        ContentWidth = contentWidthZS,
                                        LabelStyle = labelStyle,
                                        PopupStyle = popupStyle,
                                        TextFieldStyle = textAreaStyle,
                                        ButtonStyle = buttonStyle,
                                        BlockInteraction = blockNodeInteraction,
                                        CompactOperatorLabels = true
                                    });

                                currentY += eventHeight + CHOICE_EVENT_ITEM_SPACING * zoomScale;
                            }

                            if (varsToRemove.Count > 0)
                            {
                                Undo.RecordObject(database, "Remove Condition");

                                foreach (int index in varsToRemove.OrderByDescending(idx => idx))
                                {
                                    choice.ChoiceCondition.RemoveAt(index);
                                }
                            }
                        }
                    }

                    if (choice.ChoiceConsequence != null && choice.ChoiceConsequence.Count > 0) // variable consequences
                    {
                        string varFoldoutKey = node.nodeId;

                        if (!variableChangeFoldouts.ContainsKey(varFoldoutKey))
                        {
                            variableChangeFoldouts[varFoldoutKey] = true;
                        }

                        Rect varToggleRect = new Rect(0, currentY, contentWidthZS, ACTION_ROW_HEIGHT * zoomScale);
                        bool varFolded = variableChangeFoldouts[varFoldoutKey];

                        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && varToggleRect.Contains(Event.current.mousePosition))
                        {
                            variableChangeFoldouts[varFoldoutKey] = !variableChangeFoldouts[varFoldoutKey];
                            Event.current.Use();
                        }

                        string choiceConsequencesFoldoutLabel;

                        if (varFolded) choiceConsequencesFoldoutLabel = "▽ Choice consequences";
                        else choiceConsequencesFoldoutLabel = "▶ Choice consequences";

                        GUI.Toggle(varToggleRect, varFolded, choiceConsequencesFoldoutLabel, foldoutToggleStyle);
                        currentY += ACTION_ROW_HEIGHT * zoomScale;

                        if (variableChangeFoldouts[varFoldoutKey])
                        {
                            List<int> varsToRemove = new List<int>();

                            for (int j = 0; j < choice.ChoiceConsequence.Count; j++)
                            {
                                var varChange = choice.ChoiceConsequence[j];
                                float eventHeight = ACTION_ROW_HEIGHT * zoomScale;
                                Rect eventBoxRect = new Rect(0, currentY, contentWidthZS, eventHeight);

                                GUI.Box(eventBoxRect, GUIContent.none, EditorStyles.helpBox);

                                float fieldY = currentY + CHOICE_EVENT_FIELD_TOP_PADDING * zoomScale;

                                var shellParams = new ConditionRowShellParams
                                {
                                    Layout = ConditionRowShellLayout.ChoiceConditionRow,
                                    Index = j,
                                    ReferenceState = varChange.ReferenceState,
                                    VariableName = varChange.VariableName,
                                    MissingReferenceLabel = varChange.MissingReferenceLabel,
                                    ContentWidth = contentWidthZS,
                                    RowBaseY = currentY,
                                    ZoomScale = zoomScale,
                                    WarningBoundsRect = eventBoxRect,
                                    LabelStyle = labelStyle,
                                    ButtonStyle = buttonStyle,
                                    DeleteTooltip = "Delete consequence"
                                };

                                if (DrawConditionRowShell(shellParams, varsToRemove, out fieldY))
                                {
                                    currentY += eventHeight + CHOICE_EVENT_ITEM_SPACING * zoomScale;
                                    continue;
                                }

                                EnsureChoiceConsequenceVariableType(varChange);
                                float fieldX = spacingZS;

                                GUI.Label(new Rect(fieldX, fieldY, CHOICE_EVENT_TYPE_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), "Type:", labelStyle);

                                fieldX += CHOICE_EVENT_TYPE_LABEL_WIDTH * zoomScale;
                                var varChangeRefCons = varChange;
                                DrawVariableTypeDropdown(
                                    new Rect(fieldX, fieldY, CHOICE_EVENT_TYPE_POPUP_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale),
                                    varChange.VariableType,
                                    newType =>
                                    {
                                        Undo.RecordObject(database, "Change Variable Type");
                                        varChangeRefCons.VariableType = newType;
                                        varChangeRefCons.VariableName = null;
                                        DialogReferenceEditorUtility.ResetVariableReferenceState(varChangeRefCons);
                                    },
                                    popupStyle);

                                fieldX += (CHOICE_EVENT_TYPE_POPUP_WIDTH + CHOICE_EVENT_VAR_SPACING) * zoomScale;

                                GUI.Label(new Rect(fieldX, fieldY, CHOICE_EVENT_NAME_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), "Name:", labelStyle);

                                fieldX += CHOICE_EVENT_NAME_LABEL_WIDTH * zoomScale;
                                string[] varNames = GetVariableNamesByType(varChange.VariableType);

                                string newConsVarName = DrawVariableNamePopup(
                                    new Rect(fieldX, fieldY, CHOICE_EVENT_VAR_POPUP_WIDTH_CONS * zoomScale, FIELD_ROW_HEIGHT * zoomScale),
                                    varNames,
                                    varChange.VariableName,
                                    popupStyle,
                                    CollectUsedVariableNames(choice.ChoiceConsequence, j));
                                if (newConsVarName != varChange.VariableName)
                                {
                                    Undo.RecordObject(database, "Change Variable Name");
                                    varChange.VariableName = newConsVarName;
                                    InvalidateReferenceIndex();
                                }

                                fieldX += (CHOICE_EVENT_OP_TO_VAL_GAP + CHOICE_EVENT_VAR_SPACING) * zoomScale;

                                if (blockNodeInteraction) GUI.enabled = false;
                                else GUI.enabled = !string.IsNullOrEmpty(varChange.VariableName);

                                GUI.Label(new Rect(fieldX, fieldY, CHOICE_EVENT_STANET_LABEL_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), "Becomes:", labelCenterStyle);
                                fieldX += (CHOICE_EVENT_STANET_LABEL_WIDTH + CHOICE_EVENT_VAR_SPACING) * zoomScale;

                                switch (varChange.VariableType)
                                {
                                    case ConditionVariableType.Bool:
                                        bool newBool = DrawZoomScaledBoolToggle(
                                            new Rect(fieldX, fieldY, FIELD_ROW_HEIGHT * zoomScale, FIELD_ROW_HEIGHT * zoomScale),
                                            varChange.BoolValue,
                                            zoomScale,
                                            buttonStyle);

                                        if (newBool != varChange.BoolValue)
                                        {
                                            Undo.RecordObject(database, "Change Bool Value");
                                            varChange.BoolValue = newBool;
                                        }

                                        break;

                                    case ConditionVariableType.Int:

                                        int newInt = EditorGUI.IntField(new Rect(fieldX, fieldY, CHOICE_EVENT_VALUE_FIELD_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), varChange.IntValue, textAreaStyle);

                                        if (newInt != varChange.IntValue)
                                        {
                                            Undo.RecordObject(database, "Change Int Value");
                                            varChange.IntValue = newInt;
                                        }

                                        break;

                                    case ConditionVariableType.Float:

                                        float newFloat = EditorGUI.FloatField(new Rect(fieldX, fieldY, CHOICE_EVENT_VALUE_FIELD_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), varChange.FloatValue, textAreaStyle);

                                        if (newFloat != varChange.FloatValue)
                                        {
                                            Undo.RecordObject(database, "Change Float Value");
                                            varChange.FloatValue = newFloat;
                                        }

                                        break;

                                    case ConditionVariableType.String:

                                        string newString = EditorGUI.TextField(new Rect(fieldX, fieldY, CHOICE_EVENT_VALUE_FIELD_WIDTH * zoomScale, FIELD_ROW_HEIGHT * zoomScale), varChange.StringValue, textAreaStyle);

                                        if (newString != varChange.StringValue)
                                        {
                                            Undo.RecordObject(database, "Change String Value");
                                            varChange.StringValue = newString;
                                        }

                                        break;
                                }

                                if (blockNodeInteraction) GUI.enabled = false;
                                else GUI.enabled = true;

                                currentY += eventHeight + CHOICE_EVENT_ITEM_SPACING * zoomScale;
                            }

                            if (varsToRemove.Count > 0)
                            {
                                Undo.RecordObject(database, "Remove Consequence");

                                foreach (int index in varsToRemove.OrderByDescending(idx => idx))
                                {
                                    choice.ChoiceConsequence.RemoveAt(index);
                                }
                            }
                        }
                    }

                    foreach (string lang in availableLanguages) // Translations
                    {
                        if (!choiceLanguageFoldouts.ContainsKey(lang))
                        {
                            choiceLanguageFoldouts[lang] = true;
                        }

                        Rect langToggleRect = new Rect(0, currentY, contentWidthZS, ACTION_ROW_HEIGHT * zoomScale);    // Language toggle
                        bool langFolded = choiceLanguageFoldouts[lang];

                        if (Event.current.button == 0 && Event.current.type == EventType.MouseDown && langToggleRect.Contains(Event.current.mousePosition))
                        {
                            choiceLanguageFoldouts[lang] = !choiceLanguageFoldouts[lang];
                            Event.current.Use();
                        }

                        string langFoldoutLabel;

                        if (langFolded) langFoldoutLabel = $"▽ Language: {lang}";
                        else langFoldoutLabel = $"▶ Language: {lang}";

                        GUI.Toggle(langToggleRect, langFolded, langFoldoutLabel, foldoutToggleStyle);
                        currentY += ACTION_ROW_HEIGHT * zoomScale;

                        if (choiceLanguageFoldouts[lang])
                        {
                            var textEntry = choice.Translations.Find(t => t.LanguageCode == lang);
                            if (textEntry == null)
                            {
                                textEntry = new LineText { LanguageCode = lang };
                                choice.Translations.Add(textEntry);
                            }

                            GUI.backgroundColor = TEXT_AREA_BG;
                            EditorGUI.BeginChangeCheck();

                            string newText = EditorGUI.TextArea(new Rect(0, currentY, contentWidthZS, 80 * zoomScale), textEntry.Text ?? "", textAreaStyle);

                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(database, "Edit Choice Text");
                                textEntry.Text = newText;
                            }

                            GUI.backgroundColor = Color.white;
                            currentY += 80 * zoomScale + spacingZS;
                        }
                    }

                    currentY += spacingZS; // Spacing after choice
                }

                if (choicesToRemove.Count > 0)
                {
                    Undo.RecordObject(database, "Remove Choice Option");
                    foreach (int index in choicesToRemove.OrderByDescending(idx => idx))
                    {
                        if (pendingPort != null && pendingPort.Node == node && pendingPort.ChoiceIndex >= 0)
                        {
                            if (pendingPort.ChoiceIndex == index) { selectedPorts.Clear(); pendingPort = null; }
                            else if (pendingPort.ChoiceIndex > index) pendingPort = new SelectedPort(node, false, pendingPort.ChoiceIndex - 1);
                        }
                        node.ChoiceData.RemoveAt(index);
                    }
                }

                currentY += spacingZS; // Final spacing
            }

            GUILayout.EndArea();

            if (blockNodeInteraction) GUI.enabled = wasGuiEnabled;

            if (e.button == 0) GUI.DragWindow(new Rect(0, 0, nodeWidthZS, toolbarHeightZS));
        }

        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];

            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }

            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }

        private void DrawBox(Rect rect, Color color, int thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        #endregion 

        #region Graph Node Creation

        private void AddDialogGraphNode(string undoLabel, DialogNode node)
        {
            Undo.RecordObject(database, undoLabel);
            selectedDialog.nodes.Add(node);
        }

        private Vector2 GetToolbarNewNodePosition()
        {
            return editorOffset + (new Vector2(DEFAULT_POSITION_XY, DEFAULT_POSITION_XY) / zoomScale);
        }

        private void AddStartNode(Vector2 position)
        {
            AddDialogGraphNode("Add Start Node", new DialogNode
            {
                NodeType = NodeType.Start,
                Position = position,
                nodeId = Guid.NewGuid().ToString()
            });
        }

        private void AddLineNode(Vector2 position)
        {
            AddDialogGraphNode("Add Line Node", new DialogNode
            {
                NodeType = NodeType.Line,
                LineData = new LineNode { CharacterActions = new List<CharacterActionData>(), DialogsText = new List<LineText>() },
                Position = position,
                nodeId = Guid.NewGuid().ToString()
            });
        }

        private void AddChoiceNode(Vector2 position)
        {
            AddDialogGraphNode("Add Choice Node", new DialogNode
            {
                NodeType = NodeType.Choice,
                ChoiceData = new List<ChoiceNode>(),
                Position = position,
                nodeId = Guid.NewGuid().ToString()
            });
        }

        private void AddConditionNode(Vector2 position)
        {
            AddDialogGraphNode("Add Condition Node", new DialogNode
            {
                NodeType = NodeType.Condition,
                ConditionData = new ConditionNode(),
                Position = position,
                nodeId = Guid.NewGuid().ToString()
            });
        }

        private void AddRedirectNode(Vector2 position)
        {
            AddDialogGraphNode("Add Redirect Node", new DialogNode
            {
                NodeType = NodeType.Redirect,
                RedirectData = CreateDefaultRedirectNodeData(),
                Position = position,
                nodeId = Guid.NewGuid().ToString()
            });
        }

        #endregion

        #region Redirect Node

        private RedirectNode CreateDefaultRedirectNodeData()
        {
            string category = DEFAULT_CATEGORY_NAME;

            if (selectedDialog != null && !string.IsNullOrEmpty(selectedDialog.initiator)) category = selectedDialog.initiator;

            return new RedirectNode { TargetCategory = category };
        }

        private int GetRedirectNodeContentHeight()
        {
            int height = REDIRECT_CONTENT_TOP_PADDING + REDIRECT_HINT_HEIGHT + REDIRECT_SECTION_GAP;
            height += REDIRECT_FIELD_ROW_HEIGHT + REDIRECT_FIELD_GAP + REDIRECT_FIELD_ROW_HEIGHT + REDIRECT_CONTENT_BOTTOM_PADDING;
            return height;
        }

        private string BuildDeleteDialogConfirmMessage(string dialogName)
        {
            string message = $"Are you sure you want to delete the dialog \"{dialogName}\"?";

            IReadOnlyList<DialogReferenceUsage> redirectUsages = GetDialogReferenceIndex().GetRedirectTargetUsages(dialogName);

            if (redirectUsages == null || redirectUsages.Count == 0) return message;

            HashSet<string> sourceDialogNames = new HashSet<string>();
            for (int i = 0; i < redirectUsages.Count; i++) sourceDialogNames.Add(redirectUsages[i].DialogName);

            message += $"\n\nThis dialog is used as a redirect target in ({sourceDialogNames.Count}) dialogs across ({redirectUsages.Count}) redirect nodes.\n\nReferences will be marked as missing.";

            return message;
        }

        private List<DialogEntry> GetRedirectTargetDialogs(string category)
        {
            List<DialogEntry> entries = new List<DialogEntry>();

            if (database?.Dialogs == null || string.IsNullOrEmpty(category)) return entries;

            foreach (DialogEntry entry in database.Dialogs)
            {
                if (entry == null || string.IsNullOrEmpty(entry.dialogName)) continue;
                if (entry.initiator != category) continue;
                if (!DialogEntryUtility.IsRedirectTargetCandidate(entry)) continue;

                entries.Add(entry);
            }

            entries.Sort((a, b) => string.Compare(a.dialogName, b.dialogName, StringComparison.Ordinal));
            return entries;
        }

        private List<string> GetRedirectTargetDialogNames(string category)
        {
            List<DialogEntry> entries = GetRedirectTargetDialogs(category);
            List<string> names = new List<string>(entries.Count);

            for (int i = 0; i < entries.Count; i++) names.Add(entries[i].dialogName);

            return names;
        }

        private static string FormatRedirectTargetDialogLabel(DialogEntry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.dialogName)) return "Unnamed";
            if (entry.isLocked) return "🔒 " + entry.dialogName;

            return entry.dialogName;
        }

        private bool ShouldDrawMissingDialogSlot(RedirectNode redirect)
        {
            if (redirect == null) return false;
            if (DialogReferenceUtility.IsMissing(redirect.DialogReferenceState)) return true;
            if (string.IsNullOrEmpty(redirect.TargetDialogName)) return false;

            DialogEntry entry = database?.Dialogs?.Find(d => d != null && d.dialogName == redirect.TargetDialogName);

            if (entry == null) return true;
            if (entry.initiator != redirect.TargetCategory) return true;
            if (!DialogEntryUtility.IsRedirectTargetCandidate(entry)) return true;

            List<string> namesInCategory = GetRedirectTargetDialogNames(redirect.TargetCategory);

            if (!namesInCategory.Contains(redirect.TargetDialogName)) return true;

            return false;
        }

        private string GetMissingDialogDisplayLabel(RedirectNode redirect)
        {
            if (redirect == null) return DialogReferenceMessages.MISSING_DIALOG_SLOT;
            if (DialogReferenceUtility.IsMissing(redirect.DialogReferenceState)) return DialogReferenceMessages.FormatMissingDialogLabel(redirect.MissingDialogReferenceLabel);
            if (string.IsNullOrEmpty(redirect.TargetDialogName)) return DialogReferenceMessages.MISSING_DIALOG_SLOT;

            DialogEntry entry = database?.Dialogs?.Find(d => d != null && d.dialogName == redirect.TargetDialogName);

            if (entry == null) return DialogReferenceMessages.FormatMissingDialogLabel(redirect.TargetDialogName);
            if (!DialogEntryUtility.IsRedirectTargetCandidate(entry)) return $"Dialog \"{redirect.TargetDialogName}\" is not a redirect target.";

            return DialogReferenceMessages.MISSING_DIALOG_SLOT;
        }

        private void DrawRedirectNodeToolbar(DialogNode node, float nodeWidthZS, float spacingZS, float zoomScale, GUIStyle buttonStyle, Event e)
        {
            GUI.backgroundColor = DELETE_BUTTON_COLOR;

            if (GUI.Button(new Rect(nodeWidthZS - spacingZS - NODE_BUTTON_SIZE * zoomScale, 0, NODE_BUTTON_SIZE * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "×", buttonStyle) && e.button == 0)
            {
                Undo.RecordObject(database, "Remove Node");
                RemoveDialogNode(selectedDialog, node);
                GUIUtility.ExitGUI();
            }

            GUI.backgroundColor = Color.white;

            if (GUI.Button(new Rect(nodeWidthZS - 2 * spacingZS - 2 * NODE_BUTTON_SIZE * zoomScale, 0, NODE_BUTTON_SIZE * zoomScale, NODE_TOOLBAR_HEIGHT * zoomScale), "✂", buttonStyle) && e.button == 0)
            {
                Undo.RecordObject(database, "Duplicate Node");
                RedirectNode sourceRedirect = node.RedirectData ?? CreateDefaultRedirectNodeData();
                DialogNode duplicate = new DialogNode
                {
                    NodeType = NodeType.Redirect,
                    RedirectData = new RedirectNode
                    {
                        TargetCategory = sourceRedirect.TargetCategory,
                        TargetDialogName = sourceRedirect.TargetDialogName,
                        DialogReferenceState = sourceRedirect.DialogReferenceState,
                        MissingDialogReferenceLabel = sourceRedirect.MissingDialogReferenceLabel
                    },
                    Position = node.Position + new Vector2(DUPLICATE_NODE_OFFSET, DUPLICATE_NODE_OFFSET),
                    nodeId = Guid.NewGuid().ToString()
                };
                selectedDialog.nodes.Add(duplicate);
            }
        }

        private const string REDIRECT_TARGET_LOCK_PREFIX = "🔒 ";

        private static string NormalizeRedirectTargetDialogName(string dialogName)
        {
            if (string.IsNullOrEmpty(dialogName)) return dialogName;
            if (!dialogName.StartsWith(REDIRECT_TARGET_LOCK_PREFIX)) return dialogName;

            return dialogName.Substring(REDIRECT_TARGET_LOCK_PREFIX.Length);
        }

        private void FixRedirectTargetDialogNameIfNeeded(RedirectNode redirect)
        {
            if (redirect == null || string.IsNullOrEmpty(redirect.TargetDialogName)) return;

            string normalized = NormalizeRedirectTargetDialogName(redirect.TargetDialogName);

            if (normalized == redirect.TargetDialogName) return;

            Undo.RecordObject(database, "Fix Redirect Target Dialog Name");
            redirect.TargetDialogName = normalized;
        }

        private void DrawRedirectHintHelpBox(Rect rect, string message, float zoomScale)
        {
            GUIStyle hintStyle = new GUIStyle(EditorStyles.helpBox)
            {
                fontSize = Mathf.Max(1, Mathf.RoundToInt(BASE_FONT_SIZE * zoomScale)),
                wordWrap = true,
                alignment = TextAnchor.MiddleCenter,
                clipping = TextClipping.Clip,
                padding = new RectOffset((int)(4 * zoomScale), (int)(4 * zoomScale), (int)(2 * zoomScale), (int)(2 * zoomScale))
            };

            GUI.Label(rect, message, hintStyle);
        }

        private void DrawRedirectHelpBoxArea(Rect rect, RedirectNode redirect, float zoomScale, GUIStyle labelStyle)
        {
            if (ShouldDrawMissingDialogSlot(redirect))
            {
                GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
                DrawMissingReferenceWarning(rect, GetMissingDialogDisplayLabel(redirect), zoomScale, labelStyle);
                return;
            }

            DrawRedirectHintHelpBox(rect, "Dialog must be without conditions or locked.", zoomScale);
        }

        private void DrawRedirectNodeContent(DialogNode node, float contentWidthZS, float zoomScale, GUIStyle labelStyle, GUIStyle popupStyle)
        {
            if (node.RedirectData == null) node.RedirectData = CreateDefaultRedirectNodeData();

            RedirectNode redirect = node.RedirectData;
            FixRedirectTargetDialogNameIfNeeded(redirect);
            float topPadZS = REDIRECT_CONTENT_TOP_PADDING * zoomScale;
            float sectionGapZS = REDIRECT_SECTION_GAP * zoomScale;
            float fieldGapZS = REDIRECT_FIELD_GAP * zoomScale;
            float hintLeftZS = REDIRECT_HINT_LEFT_PADDING * zoomScale;
            float hintWidthZS = contentWidthZS - hintLeftZS;
            float currentY = topPadZS;

            DrawRedirectHelpBoxArea(
                new Rect(hintLeftZS, currentY, hintWidthZS, REDIRECT_HINT_HEIGHT * zoomScale),
                redirect,
                zoomScale,
                labelStyle);
            currentY += REDIRECT_HINT_HEIGHT * zoomScale + sectionGapZS;

            List<string> categories = dialogCategories != null ? dialogCategories : new List<string> { DEFAULT_CATEGORY_NAME };
            string[] categoryArray = categories.ToArray();
            int categoryIndex = categories.IndexOf(redirect.TargetCategory ?? "");

            if (categoryIndex < 0) categoryIndex = 0;

            GUI.Label(new Rect(0, currentY, REDIRECT_CATEGORY_LABEL_WIDTH * zoomScale, REDIRECT_FIELD_ROW_HEIGHT * zoomScale), "Category:", labelStyle);
            EditorGUI.BeginChangeCheck();

            int newCategoryIndex = EditorGUI.Popup(
                new Rect(REDIRECT_CATEGORY_LABEL_WIDTH * zoomScale, currentY, contentWidthZS - REDIRECT_CATEGORY_LABEL_WIDTH * zoomScale, REDIRECT_FIELD_ROW_HEIGHT * zoomScale),
                categoryIndex,
                categoryArray,
                popupStyle);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(database, "Change Redirect Category");
                redirect.TargetCategory = categoryArray[newCategoryIndex];
                redirect.TargetDialogName = "";
                redirect.DialogReferenceState = DialogReferenceState.Valid;
                redirect.MissingDialogReferenceLabel = "";
            }

            currentY += REDIRECT_FIELD_ROW_HEIGHT * zoomScale + fieldGapZS;

            List<DialogEntry> redirectTargets = GetRedirectTargetDialogs(redirect.TargetCategory);
            string[] dialogArray;
            int dialogIndex;
            List<string> dialogNames = new List<string>(redirectTargets.Count);

            for (int i = 0; i < redirectTargets.Count; i++) dialogNames.Add(redirectTargets[i].dialogName);

            if (redirectTargets.Count == 0)
            {
                dialogArray = new[] { "(no redirect targets)" };
                dialogIndex = 0;
                EditorGUI.BeginDisabledGroup(true);
            }
            else
            {
                dialogArray = new string[redirectTargets.Count];
                for (int i = 0; i < redirectTargets.Count; i++) dialogArray[i] = FormatRedirectTargetDialogLabel(redirectTargets[i]);

                EnsureRedirectTargetDialogName(redirect, dialogNames);
                dialogIndex = dialogNames.IndexOf(redirect.TargetDialogName ?? "");

                if (dialogIndex < 0) dialogIndex = 0;
            }

            GUI.Label(new Rect(0, currentY, REDIRECT_DIALOG_LABEL_WIDTH * zoomScale, REDIRECT_FIELD_ROW_HEIGHT * zoomScale), "Dialog:", labelStyle);

            EditorGUI.BeginChangeCheck();

            int newDialogIndex = EditorGUI.Popup(
                new Rect(REDIRECT_DIALOG_LABEL_WIDTH * zoomScale, currentY, contentWidthZS - REDIRECT_DIALOG_LABEL_WIDTH * zoomScale, REDIRECT_FIELD_ROW_HEIGHT * zoomScale),
                dialogIndex,
                dialogArray,
                popupStyle);

            if (EditorGUI.EndChangeCheck() && dialogNames.Count > 0)
            {
                Undo.RecordObject(database, "Change Redirect Target Dialog");
                redirect.TargetDialogName = dialogNames[newDialogIndex];
                redirect.DialogReferenceState = DialogReferenceState.Valid;
                redirect.MissingDialogReferenceLabel = "";
            }

            if (dialogNames.Count == 0) EditorGUI.EndDisabledGroup();
        }

        private void EnsureRedirectTargetDialogName(RedirectNode redirect, List<string> dialogNames)
        {
            if (redirect == null || dialogNames == null || dialogNames.Count == 0) return;
            if (DialogReferenceUtility.IsMissing(redirect.DialogReferenceState)) return;
            if (!string.IsNullOrEmpty(redirect.TargetDialogName) && dialogNames.Contains(redirect.TargetDialogName)) return;

            Undo.RecordObject(database, "Assign Redirect Target Dialog");
            redirect.TargetDialogName = dialogNames[0];
            redirect.DialogReferenceState = DialogReferenceState.Valid;
            redirect.MissingDialogReferenceLabel = "";
            EditorUtility.SetDirty(database);
        }

        private void ValidateRedirectNodeLinks(DialogEntry dialog, DialogNode node, string info, List<(string description, System.Action fix)> result)
        {
            if (node?.RedirectData == null) return;

            RedirectNode redirect = node.RedirectData;

            if (DialogReferenceUtility.IsMissing(redirect.DialogReferenceState)) return;

            List<string> candidates = GetRedirectTargetDialogNames(redirect.TargetCategory);

            if (string.IsNullOrEmpty(redirect.TargetDialogName))
            {
                if (candidates.Count == 0) return;

                result.Add(($"{info} → target dialog not selected", () => EnsureRedirectTargetDialogName(redirect, candidates)));
                return;
            }

            DialogEntry target = database?.Dialogs?.Find(d => d != null && d.dialogName == redirect.TargetDialogName);

            if (target == null)
            {
                string capturedName = redirect.TargetDialogName;
                result.Add(($"{info} → dialog '{capturedName}' not found", () => DialogReferenceEditorUtility.MarkDialogMissing(redirect, capturedName)));
                return;
            }

            if (!DialogEntryUtility.IsRedirectTargetCandidate(target))
            {
                result.Add(($"{info} → dialog '{redirect.TargetDialogName}' is not a redirect target", () =>
                {
                    redirect.TargetDialogName = "";
                    redirect.DialogReferenceState = DialogReferenceState.Valid;
                    redirect.MissingDialogReferenceLabel = "";
                }));
                return;
            }

            if (target.initiator != redirect.TargetCategory)
            {
                result.Add(($"{info} → dialog '{redirect.TargetDialogName}' is in a different category", () =>
                {
                    redirect.TargetDialogName = "";
                    redirect.DialogReferenceState = DialogReferenceState.Valid;
                    redirect.MissingDialogReferenceLabel = "";
                }));
            }
        }

        #endregion 

        #region Connections Between Ports

        private void DrawNodeConnection(Vector2 start, Vector2 end)
        {
            Handles.BeginGUI();
            Handles.color = Color.white;
            Handles.DrawBezier(start, end, start + Vector2.up * BEZIER_OFFSET, end + Vector2.down * BEZIER_OFFSET, UnityEngine.Color.white, null, BEZIER_THICKNESS); //right, left was here
            Handles.EndGUI();
        }

        #endregion 



        #region RMB Node Creation Menu

        private void ShowNodeContextMenu(Vector2 position)
        {
            GenericMenu menu = new GenericMenu();
            bool hasStartNode = selectedDialog.nodes.Any(n => n.NodeType == NodeType.Start);

            if (!hasStartNode) menu.AddItem(new GUIContent("Add Start Node"), false, () => AddStartNode(position));
            else menu.AddDisabledItem(new GUIContent("Add Start Node (already exists)"));

            menu.AddItem(new GUIContent("Add Line Node"), false, () => AddLineNode(position));
            menu.AddItem(new GUIContent("Add Choice Node"), false, () => AddChoiceNode(position));
            menu.AddItem(new GUIContent("Add Condition Node"), false, () => AddConditionNode(position));
            menu.AddItem(new GUIContent("Add Redirect Node"), false, () => AddRedirectNode(position));

            menu.ShowAsContext();
        }

        #endregion RMB Node Creation Menu

        #endregion Dialog Editor (Graph)


        #region Localization CSV

        private void OnLocalizationImportApplied()
        {
            CaptureDialogEditorDirtyBaseline();
            InvalidateReferenceIndex();
        }

        #endregion Localization CSV


        #region Character Names Editor

        private void DrawCharacterNameColors()
        {
            const int BUTTON_ADD_NAME_COLOR_WIDTH = 150;
            const int DELETE_BUTTON_SIZE = 20;
            const int LABEL_WIDTH = 50;
            const int NAME_FIELD_WIDTH = 120;
            const int COLOR_FIELD_WIDTH = 80;
            const int COLOR_FIELD_HEIGHT = 18;
            const int LANG_LABEL_WIDTH = 30;
            const int TRANSLATION_FIELD_WIDTH = 150;
            const int PREVIEW_WIDTH = 120;
            const int FIELD_SPACING = 8;
            const int ROW_SPACING = 20;

            if (database == null || database.CharacterNameColors == null)
            {
                BeginAssetSlotActionRow();
                DrawInlineDialogDatabaseSlot();
                FillAssetSlotToolbarRow();
                EndAssetSlotActionRow();
                EndInlineHeaderSection();
                EditorGUILayout.HelpBox("Assign or create a Dialog Database.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            BeginAssetSlotActionRow();
            DrawInlineDialogDatabaseSlot();
            GUILayout.FlexibleSpace();

            if (AssetRowToolbarButton("+ Add name color", BUTTON_ADD_NAME_COLOR_WIDTH))
            {
                Undo.RecordObject(database, "Add Character Name Color");
                var newEntry = new CharacterNameColor { Name = "New Name", ColorHex = "#000000" };

                if (newEntry.Translations == null) newEntry.Translations = new List<NameTranslation>();

                database.CharacterNameColors.Add(newEntry);
                SaveDialogRegistry("Add name color");
            }

            EndAssetSlotActionRow();
            EndInlineHeaderSection();

            nameColorsScrollPos = EditorGUILayout.BeginScrollView(nameColorsScrollPos, GUILayout.ExpandHeight(true));
            for (int i = 0; i < database.CharacterNameColors.Count; i++)
            {
                var colorEntry = database.CharacterNameColors[i];
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label($"Color for character name {i + 1}", EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = DELETE_BUTTON_COLOR;

                if (GUILayout.Button("×", GUILayout.Width(DELETE_BUTTON_SIZE), GUILayout.Height(DELETE_BUTTON_SIZE)))
                {
                    if (TryRemoveCharacterNameColorFromRegistry(i, colorEntry)) GUIUtility.ExitGUI();
                }

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Name:", GUILayout.Width(LABEL_WIDTH));
                EditorGUI.BeginChangeCheck();
                string newName = EditorGUILayout.TextField(colorEntry.Name ?? "", GUILayout.Width(NAME_FIELD_WIDTH));

                if (EditorGUI.EndChangeCheck() && newName != colorEntry.Name)
                {
                    Undo.RecordObject(database, "Change Character Name");
                    colorEntry.Name = newName;
                }

                GUILayout.Space(ROW_SPACING);
                GUILayout.Label("Color:", GUILayout.Width(LABEL_WIDTH));
                Color currentColor = Color.black;

                if (IsValidHexColor(colorEntry.ColorHex)) ColorUtility.TryParseHtmlString(colorEntry.ColorHex, out currentColor);

                EditorGUI.BeginChangeCheck();
                Color newColor = EditorGUILayout.ColorField(currentColor, GUILayout.Width(COLOR_FIELD_WIDTH), GUILayout.Height(COLOR_FIELD_HEIGHT));

                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(database, "Change Character Color");
                    colorEntry.ColorHex = "#" + ColorUtility.ToHtmlStringRGB(newColor);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.LabelField("Name translations (substituted when selecting a character in a line):", EditorStyles.miniLabel);

                if (colorEntry.Translations == null) colorEntry.Translations = new List<NameTranslation>();

                foreach (string lang in availableLanguages)
                {
                    var trans = colorEntry.Translations.Find(t => t.languageCode == lang);

                    if (trans == null)
                    {
                        trans = new NameTranslation { languageCode = lang, displayName = colorEntry.Name ?? "" };
                        colorEntry.Translations.Add(trans);
                    }

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(ROW_SPACING);
                    GUILayout.Label($"{lang}:", GUILayout.Width(LANG_LABEL_WIDTH));
                    EditorGUI.BeginChangeCheck();
                    string newDisplay = EditorGUILayout.TextField(trans.displayName ?? "", GUILayout.Width(TRANSLATION_FIELD_WIDTH));

                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(database, "Change Name Translation");
                        trans.displayName = newDisplay;
                    }

                    GUILayout.Space(FIELD_SPACING);
                    string previewText;

                    if (!string.IsNullOrEmpty(newDisplay)) previewText = newDisplay;
                    else previewText = "-";

                    if (IsValidHexColor(colorEntry.ColorHex) && ColorUtility.TryParseHtmlString(colorEntry.ColorHex, out Color nameColor))
                    {
                        var nameStyle = new GUIStyle(EditorStyles.boldLabel);
                        nameStyle.normal.textColor = nameColor;
                        nameStyle.hover.textColor = nameColor;
                        nameStyle.active.textColor = nameColor;
                        GUILayout.Label(previewText, nameStyle, GUILayout.Width(PREVIEW_WIDTH));
                    }
                    else GUILayout.Label(previewText, EditorStyles.boldLabel, GUILayout.Width(PREVIEW_WIDTH));
                    
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private bool IsValidHexColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return false;
            hex = hex.TrimStart('#');
            return hex.Length == 6 && Regex.IsMatch(hex, "^[0-9A-Fa-f]{6}$");
        }

        #endregion Character Names Editor

        [Serializable]
        private sealed class DialogGraphSnapshot
        {
            public List<DialogEntry> Dialogs;
        }
    }
}
#endif