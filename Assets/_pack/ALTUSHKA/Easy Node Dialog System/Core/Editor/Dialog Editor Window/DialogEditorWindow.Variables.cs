#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace ALTUSHKA.EasyNodeDialogSystem.Editor
{
    public partial class DialogEditorWindow
    {
        #region Constants

        private const string VAR_NAME_CONTROL = "variableNameField";
        private const int VAR_ROW_HEIGHT = 22;
        private const int VAR_FIELD_NAME_WIDTH = 180;
        private const int VAR_FIELD_TYPE_WIDTH = 100;
        private const int VAR_FIELD_VALUE_WIDTH = 180;
        private const int VAR_FIELD_CURRENT_VALUE_WIDTH = 120;
        private const int VAR_BUTTON_WIDTH = 24;
        private const int VAR_EDIT_DEFAULT_BUTTON_WIDTH = 72;
        private const int VAR_EDIT_CURRENT_BUTTON_WIDTH = 72;
        private const int VAR_ADD_NAME_COLUMN_WIDTH = 200;
        private const int VAR_ADD_TYPE_COLUMN_WIDTH = 120;
        private const int VAR_ADD_VALUE_MIN_WIDTH = 140;
        private const int VAR_ADD_BUTTON_COLUMN_WIDTH = 140;
        private const int VAR_ADD_BUTTON_HEIGHT = 18;
        private const int VAR_ADD_SECTION_SPACING = 1;
        private const int VAR_RUNTIME_SAVE_BUTTON_WIDTH = 220;

        #endregion Constants

        #region Fields

        private Vector2 variableEditorScrollPos;
        private string newVariableName = "";
        private string newIntStr = "";
        private string newFloatStr = "";
        private readonly Dictionary<int, string> _editingIntStr = new Dictionary<int, string>();
        private readonly Dictionary<int, string> _editingFloatStr = new Dictionary<int, string>();
        private readonly Dictionary<string, string> _editingCurrentIntStr = new Dictionary<string, string>();
        private readonly Dictionary<string, string> _editingCurrentFloatStr = new Dictionary<string, string>();
        private DialogDatabase _variableRuntimeSyncSource;
        private bool _variableRuntimeStateReady;
        private ConditionVariableType newVariableType = ConditionVariableType.Bool;
        private bool newBoolDefault;
        private int newIntDefault;
        private float newFloatDefault;
        private string newStringDefault = "";
        private readonly HashSet<string> _variableDefinitionEditKeys = new HashSet<string>();
        private readonly HashSet<string> _variableCurrentValueEditKeys = new HashSet<string>();
        private readonly Dictionary<string, VariableDefinitionEditSnapshot> _variableDefinitionEditSnapshots = new Dictionary<string, VariableDefinitionEditSnapshot>();
        private readonly HashSet<string> _variableDefinitionRenameKeys = new HashSet<string>();
        private readonly Dictionary<string, string> _variableRenameDrafts = new Dictionary<string, string>();

        private struct VariableDefinitionEditSnapshot
        {
            public string VariableName;
            public ConditionVariableType VariableType;
            public bool BoolDefault;
            public int IntDefault;
            public float FloatDefault;
            public string StringDefault;
        }

        #endregion Fields

        #region Variable editor

        private void DrawVariableEditor()
        {
            BeginAssetSlotActionRow();
            DrawInlineDialogDatabaseSlot();
            FillAssetSlotToolbarRow();
            EndAssetSlotActionRow();
            EndInlineHeaderSection();

            if (database == null)
            {
                EditorGUILayout.HelpBox("Assign a Dialog Database.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));
            EditorGUILayout.Space(EDITOR_SPACE_MEDIUM);
            EditorGUILayout.LabelField("Dialog system variables", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Variables are stored in the Dialog Database. Template - name, type, and default value. Current - value in G; save.json is written only via the Save Current to save.json button.", MessageType.Info);
            EnsureVariableRuntimeStateForEditor();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Load Current from save.json", GUILayout.Width(VAR_RUNTIME_SAVE_BUTTON_WIDTH)))
            {
                _variableRuntimeStateReady = false;
                EnsureVariableRuntimeStateForEditor(forceReload: true);
            }

            if (GUILayout.Button("Save Current to save.json", GUILayout.Width(VAR_RUNTIME_SAVE_BUTTON_WIDTH))) G.SaveGame();
            if (GUILayout.Button("Open save.json", GUILayout.Width(VAR_RUNTIME_SAVE_BUTTON_WIDTH))) OpenVariableRuntimeSaveJson();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(EDITOR_SPACE_MEDIUM);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandWidth(true));   
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(GUILayout.Width(VAR_ADD_NAME_COLUMN_WIDTH));
            EditorGUILayout.LabelField("Variable name", EditorStyles.miniLabel);
            GUI.SetNextControlName(VAR_NAME_CONTROL + "_add");
            FilterVariableNameInput();
            newVariableName = Regex.Replace(EditorGUILayout.TextField(newVariableName), @"[^a-zA-Z0-9_]", "");
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUILayout.Width(VAR_ADD_TYPE_COLUMN_WIDTH));
            EditorGUILayout.LabelField("Variable type", EditorStyles.miniLabel);
            newVariableType = (ConditionVariableType)EditorGUILayout.EnumPopup(newVariableType);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(VAR_ADD_VALUE_MIN_WIDTH), GUILayout.ExpandWidth(true));
            EditorGUILayout.LabelField("Default value", EditorStyles.miniLabel);

            switch (newVariableType)
            {
                case ConditionVariableType.Bool:
                    newBoolDefault = EditorGUILayout.Toggle(newBoolDefault);
                    break;
                case ConditionVariableType.Int:
                    {
                        string intDisplay;

                        if (string.IsNullOrEmpty(newIntStr)) intDisplay = newIntDefault.ToString(CultureInfo.InvariantCulture);
                        else intDisplay = newIntStr;

                        string filtered = FilterIntString(EditorGUILayout.TextField(intDisplay));

                        if (int.TryParse(filtered, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
                        {
                            newIntDefault = parsedInt;
                            newIntStr = "";
                        }
                        else newIntStr = filtered;
                        
                    }
                    break;
                case ConditionVariableType.Float:
                    {
                        string floatDisplay;

                        if (string.IsNullOrEmpty(newFloatStr)) floatDisplay = newFloatDefault.ToString(CultureInfo.InvariantCulture);
                        else floatDisplay = newFloatStr;

                        string filtered = FilterFloatString(EditorGUILayout.TextField(floatDisplay));

                        if (float.TryParse(filtered, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedFloat))
                        {
                            newFloatDefault = parsedFloat;
                            newFloatStr = "";
                        }
                        else newFloatStr = filtered;         
                    }
                    break;
                case ConditionVariableType.String:
                    newStringDefault = EditorGUILayout.TextField(newStringDefault ?? "");
                    break;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(GUILayout.Width(VAR_ADD_BUTTON_COLUMN_WIDTH));
            EditorGUILayout.LabelField(" ", EditorStyles.miniLabel);
            GUI.enabled = !string.IsNullOrWhiteSpace(newVariableName);

            if (GUILayout.Button("+ Create variable", GUILayout.Height(VAR_ADD_BUTTON_HEIGHT)))
            {
                AddNewVariable();
            }

            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(EDITOR_SPACE_MEDIUM);
            EditorGUILayout.LabelField("Variable list", EditorStyles.boldLabel); 
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(VAR_ADD_SECTION_SPACING*2);
            EditorGUILayout.LabelField("Variable name", EditorStyles.miniLabel, GUILayout.Width(VAR_FIELD_NAME_WIDTH));
            GUILayout.Space(VAR_ADD_SECTION_SPACING);
            EditorGUILayout.LabelField("Type", EditorStyles.miniLabel, GUILayout.Width(VAR_FIELD_TYPE_WIDTH));
            GUILayout.Space(VAR_ADD_SECTION_SPACING);
            EditorGUILayout.LabelField("Default value", EditorStyles.miniLabel, GUILayout.Width(VAR_FIELD_VALUE_WIDTH));
            GUILayout.Space(VAR_ADD_SECTION_SPACING);
            EditorGUILayout.LabelField("Edit", EditorStyles.miniLabel, GUILayout.Width(VAR_EDIT_DEFAULT_BUTTON_WIDTH));
            GUILayout.Space(VAR_ADD_SECTION_SPACING);
            EditorGUILayout.LabelField("Current value in G", EditorStyles.miniLabel, GUILayout.Width(VAR_FIELD_CURRENT_VALUE_WIDTH ));
            GUILayout.Space(VAR_ADD_SECTION_SPACING);
            EditorGUILayout.LabelField("Edit", EditorStyles.miniLabel, GUILayout.Width(VAR_EDIT_DEFAULT_BUTTON_WIDTH));
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            variableEditorScrollPos = EditorGUILayout.BeginScrollView(variableEditorScrollPos, EditorStyles.helpBox, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            var vars = database.Variables;

            if (vars == null || vars.Count == 0)
            {
                EditorGUILayout.HelpBox("No variables. Add one above.", MessageType.None);
            }
            else
            {
                for (int i = 0; i < vars.Count; i++)
                {
                    DrawVariableRow(i, vars[i]);
                }
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        private void OpenVariableRuntimeSaveJson()
        {
            string savePath = G.GetSavePath();

            if (!File.Exists(savePath))
            {
                EditorUtility.DisplayDialog("save.json", $"File not found:\n{savePath}", "OK");
                return;
            }

            EditorUtility.OpenWithDefaultApp(savePath);
        }

        private static string FilterIntString(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            s = Regex.Replace(s, @"[^\d\-]", "");
            bool neg = s.StartsWith("-");
            s = s.Replace("-", "");

            if (neg) return "-" + s;

            return s;
        }

        private static string FilterFloatString(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";

            s = Regex.Replace(s, @"[^\d.\-]", "");
            bool neg = s.StartsWith("-");
            s = s.Replace("-", "");
            int sep = s.IndexOf('.');

            if (sep >= 0)
            {
                string before = s.Substring(0, sep);
                string after = Regex.Replace(s.Substring(sep + 1), @"\.", "");
                s = before + "." + after;
            }

            if (neg) return "-" + s;
            return s;
        }

        private static bool IsValidVariableNameChar(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '_';
        }

        private static bool IsVariableNameFieldFocused()
        {
            string focused = GUI.GetNameOfFocusedControl();
            return !string.IsNullOrEmpty(focused) && focused.StartsWith(VAR_NAME_CONTROL);
        }

        private static void FilterVariableNameInput()
        {
            if (!IsVariableNameFieldFocused()) return;

            var evt = Event.current;

            if (evt.type == EventType.KeyDown && evt.character != 0)
            {
                if (!IsValidVariableNameChar(evt.character))
                {
                    evt.character = '\0';
                    evt.Use();
                }
                return;
            }

            if (evt.type == EventType.ExecuteCommand && evt.commandName == "Paste")
            {
                string paste = GUIUtility.systemCopyBuffer ?? "";
                string filtered = Regex.Replace(paste, @"[^a-zA-Z0-9_]", "");
                GUIUtility.systemCopyBuffer = filtered;
                EditorApplication.delayCall += () => { GUIUtility.systemCopyBuffer = paste; };
            }
        }

        private void AddNewVariable()
        {
            if (database == null) return;

            var name = newVariableName.Trim();

            if (string.IsNullOrEmpty(name)) return;

            var existing = database.Variables.FirstOrDefault(v => v.variableName == name);

            if (existing != null)
            {
                EditorUtility.DisplayDialog("Error", $"Variable '{name}' already exists.", "OK");
                return;
            }

            Undo.RecordObject(database, "Add Variable");
            var def = new VariableDefinition
            {
                variableName = name,
                variableType = newVariableType,
                boolDefault = newBoolDefault,
                intDefault = newIntDefault,
                floatDefault = newFloatDefault,
                stringDefault = newStringDefault ?? ""
            };
            database.AddVariable(def);
            SaveDialogRegistry("Add variable");
            InvalidateReferenceIndex();
            G.SetVariableFromDefinition(def);
            G.SaveGame(logResult: false);
            newVariableName = "";
            newBoolDefault = false;
            newIntDefault = 0;
            newFloatDefault = 0f;
            newIntStr = "";
            newFloatStr = "";
            newStringDefault = "";
        }

        private void ClearVariableRegistryRowState(string variableName)
        {
            if (string.IsNullOrEmpty(variableName)) return;

            _variableDefinitionEditKeys.Remove(variableName);
            _variableCurrentValueEditKeys.Remove(variableName);
            _variableDefinitionRenameKeys.Remove(variableName);
            _variableRenameDrafts.Remove(variableName);
            _variableDefinitionEditSnapshots.Remove(variableName);
            _editingCurrentIntStr.Remove(variableName);
            _editingCurrentFloatStr.Remove(variableName);
        }

        private static VariableDefinitionEditSnapshot CaptureVariableDefinitionSnapshot(VariableDefinition def)
        {
            return new VariableDefinitionEditSnapshot
            {
                VariableName = def.variableName,
                VariableType = def.variableType,
                BoolDefault = def.boolDefault,
                IntDefault = def.intDefault,
                FloatDefault = def.floatDefault,
                StringDefault = def.stringDefault ?? ""
            };
        }

        private void RevertVariableDefinitionFromSnapshot(VariableDefinition def)
        {
            if (def == null) return;
            if (!_variableDefinitionEditSnapshots.TryGetValue(def.variableName, out VariableDefinitionEditSnapshot snapshot)) return;

            def.variableType = snapshot.VariableType;
            def.boolDefault = snapshot.BoolDefault;
            def.intDefault = snapshot.IntDefault;
            def.floatDefault = snapshot.FloatDefault;
            def.stringDefault = snapshot.StringDefault;
        }

        private void ExitVariableDefinitionEditMode(string variableName, int rowIndex, bool revertDefinition)
        {
            if (revertDefinition && database?.Variables != null)
            {
                for (int i = 0; i < database.Variables.Count; i++)
                {
                    VariableDefinition def = database.Variables[i];
                    if (def == null || def.variableName != variableName) continue;

                    RevertVariableDefinitionFromSnapshot(def);
                    break;
                }
            }

            _variableDefinitionEditKeys.Remove(variableName);
            _variableDefinitionRenameKeys.Remove(variableName);
            _variableRenameDrafts.Remove(variableName);
            _variableDefinitionEditSnapshots.Remove(variableName);
            _editingIntStr.Remove(rowIndex);
            _editingFloatStr.Remove(rowIndex);
        }

        private bool HasVariableDefinitionEditChanges(VariableDefinition def, int rowIndex)
        {
            if (def == null) return false;
            if (!_variableDefinitionEditSnapshots.TryGetValue(def.variableName, out VariableDefinitionEditSnapshot snapshot)) return true;

            string draftName = def.variableName;
            if (_variableRenameDrafts.TryGetValue(def.variableName, out string pendingRename)) draftName = Regex.Replace(pendingRename ?? "", @"[^a-zA-Z0-9_]", "").Trim();

            if (draftName != snapshot.VariableName) return true;
            if (def.variableType != snapshot.VariableType) return true;

            if (_editingIntStr.TryGetValue(rowIndex, out string pendingInt) && !string.IsNullOrEmpty(pendingInt)) return true;
            if (_editingFloatStr.TryGetValue(rowIndex, out string pendingFloat) && !string.IsNullOrEmpty(pendingFloat)) return true;

            if (def.variableType == ConditionVariableType.Bool && def.boolDefault != snapshot.BoolDefault) return true;
            if (def.variableType == ConditionVariableType.Int && def.intDefault != snapshot.IntDefault) return true;
            if (def.variableType == ConditionVariableType.Float && !Mathf.Approximately(def.floatDefault, snapshot.FloatDefault)) return true;
            if (def.variableType == ConditionVariableType.String && (def.stringDefault ?? "") != snapshot.StringDefault) return true;

            return false;
        }

        private void BeginVariableDefinitionEdit(VariableDefinition def)
        {
            _variableDefinitionEditKeys.Add(def.variableName);
            _variableRenameDrafts[def.variableName] = def.variableName;
            _variableDefinitionEditSnapshots[def.variableName] = CaptureVariableDefinitionSnapshot(def);
        }

        private void EnsureVariableRuntimeStateForEditor(bool forceReload = false)
        {
            if (database == null)
            {
                _variableRuntimeStateReady = false;
                return;
            }

            if (Application.isPlaying)
            {
                _variableRuntimeSyncSource = database;
                _variableRuntimeStateReady = true;
                return;
            }

            if (!forceReload && _variableRuntimeStateReady && _variableRuntimeSyncSource == database) return;

            G.InitializeFromDefinitions(database);
            G.LoadGame();
            _variableRuntimeSyncSource = database;
            _variableRuntimeStateReady = true;
        }

        private void DrawVariableCurrentValueField(VariableDefinition def)
        {
            if (def == null || string.IsNullOrEmpty(def.variableName)) return;

            switch (def.variableType)
            {
                case ConditionVariableType.Bool:
                    {
                        bool currentValue = G.GetBool(def.variableName);
                        bool newValue = EditorGUILayout.Toggle(currentValue, GUILayout.Width(VAR_FIELD_CURRENT_VALUE_WIDTH));
                        if (newValue == currentValue) break;

                        G.SetBool(def.variableName, newValue);
                    }
                    break;
                case ConditionVariableType.Int:
                    {
                        string intDisplay;
                        if (_editingCurrentIntStr.TryGetValue(def.variableName, out string pendingInt)) intDisplay = pendingInt;
                        else intDisplay = G.GetInt(def.variableName).ToString(CultureInfo.InvariantCulture);

                        string filtered = FilterIntString(EditorGUILayout.TextField(intDisplay, GUILayout.Width(VAR_FIELD_CURRENT_VALUE_WIDTH)));
                        if (int.TryParse(filtered, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
                        {
                            _editingCurrentIntStr.Remove(def.variableName);
                            if (parsedInt == G.GetInt(def.variableName)) break;

                            G.SetInt(def.variableName, parsedInt);
                        }
                        else
                        {
                            _editingCurrentIntStr[def.variableName] = filtered;
                        }
                    }
                    break;
                case ConditionVariableType.Float:
                    {
                        string floatDisplay;

                        if (_editingCurrentFloatStr.TryGetValue(def.variableName, out string pendingFloat)) floatDisplay = pendingFloat;
                        else floatDisplay = G.GetFloat(def.variableName).ToString(CultureInfo.InvariantCulture);

                        string filtered = FilterFloatString(EditorGUILayout.TextField(floatDisplay, GUILayout.Width(VAR_FIELD_CURRENT_VALUE_WIDTH)));

                        if (float.TryParse(filtered, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedFloat))
                        {
                            _editingCurrentFloatStr.Remove(def.variableName);
                            if (Mathf.Approximately(parsedFloat, G.GetFloat(def.variableName))) break;

                            G.SetFloat(def.variableName, parsedFloat);
                        }
                        else _editingCurrentFloatStr[def.variableName] = filtered;
                        
                    }
                    break;
                case ConditionVariableType.String:
                    {
                        string currentValue = G.GetString(def.variableName);
                        string newValue = EditorGUILayout.TextField(currentValue ?? "", GUILayout.Width(VAR_FIELD_CURRENT_VALUE_WIDTH));
                        if (newValue == currentValue) break;

                        G.SetString(def.variableName, newValue ?? "");
                    }
                    break;
            }
        }

        private bool TryCommitVariableCurrentValueEdit(string variableName)
        {
            if (string.IsNullOrEmpty(variableName)) return false;

            _variableCurrentValueEditKeys.Remove(variableName);
            _editingCurrentIntStr.Remove(variableName);
            _editingCurrentFloatStr.Remove(variableName);
            return true;
        }

        private bool TryCommitVariableDefinitionEdit(VariableDefinition def, int rowIndex)
        {
            if (def == null || database == null) return false;

            if (!HasVariableDefinitionEditChanges(def, rowIndex))
            {
                ExitVariableDefinitionEditMode(def.variableName, rowIndex, revertDefinition: true);
                return true;
            }

            string key = def.variableName;
            if (_variableRenameDrafts.TryGetValue(key, out string draftName))
            {
                draftName = Regex.Replace(draftName ?? "", @"[^a-zA-Z0-9_]", "").Trim();
                if (string.IsNullOrEmpty(draftName))
                {
                    EditorUtility.DisplayDialog("Error", "Variable name cannot be empty.", "OK");
                    return false;
                }

                if (draftName != key && !TryCommitVariableRename(def)) return false;
            }

            Undo.RecordObject(database, "Edit Variable");
            SaveDialogRegistry("Edit variable definition");
            _variableDefinitionEditSnapshots.Remove(key);

            if (def.variableName != key) _variableDefinitionEditSnapshots.Remove(def.variableName);

            ExitVariableDefinitionEditMode(def.variableName, rowIndex, revertDefinition: false);
            InvalidateReferenceIndex();
            return true;
        }

        private bool TryCommitVariableRename(VariableDefinition def)
        {
            if (def == null || database == null) return false;

            string oldName = def.variableName;

            if (!_variableRenameDrafts.TryGetValue(oldName, out string newName)) newName = oldName;

            newName = Regex.Replace(newName ?? "", @"[^a-zA-Z0-9_]", "").Trim();
            if (string.IsNullOrEmpty(newName))
            {
                EditorUtility.DisplayDialog("Error", "Variable name cannot be empty.", "OK");
                return false;
            }

            if (newName == oldName)
            {
                ClearVariableRegistryRowState(oldName);
                return true;
            }

            if (database.Variables.Any(v => v != null && v != def && v.variableName == newName))
            {
                EditorUtility.DisplayDialog("Error", $"Variable '{newName}' already exists.", "OK");
                return false;
            }

            Undo.RecordObject(database, "Rename Variable");
            if (database != null) Undo.RecordObject(database, "Rename Variable References");

            ConditionVariableType variableType = def.variableType;
            def.variableName = newName;

            int updatedReferences = 0;

            if (database != null) updatedReferences = DialogReferenceRename.RenameVariableInDialogs(database, oldName, newName);

            G.RenameVariable(oldName, newName, variableType);

            if (_editingCurrentIntStr.TryGetValue(oldName, out string pendingInt)) { _editingCurrentIntStr.Remove(oldName); _editingCurrentIntStr[newName] = pendingInt; }
            if (_editingCurrentFloatStr.TryGetValue(oldName, out string pendingFloat)) { _editingCurrentFloatStr.Remove(oldName); _editingCurrentFloatStr[newName] = pendingFloat; }

            G.SaveGame(logResult: false);

            if (_variableDefinitionEditKeys.Remove(oldName)) _variableDefinitionEditKeys.Add(newName);
            if (_variableCurrentValueEditKeys.Remove(oldName)) _variableCurrentValueEditKeys.Add(newName);

            ClearVariableRegistryRowState(oldName);
            SaveDialogRegistry("Rename variable");

            if (updatedReferences > 0) SaveRegistryDialogGraphPatch( "Rename variable - update dialog references", () => DialogReferenceRename.RenameVariableInDialogs(database, oldName, newName));
            

            InvalidateReferenceIndex();
            return true;
        }

        private void DrawVariableRow(int index, VariableDefinition def)
        {
            bool isEditingDefinition = _variableDefinitionEditKeys.Contains(def.variableName);
            bool isEditingCurrentValue = _variableCurrentValueEditKeys.Contains(def.variableName);
            EditorGUILayout.BeginHorizontal(GUILayout.Height(VAR_ROW_HEIGHT));
            EditorGUI.BeginDisabledGroup(!isEditingDefinition);
            GUI.SetNextControlName(VAR_NAME_CONTROL + "_" + index);
            FilterVariableNameInput();
            string renameDraft = def.variableName;

            if (isEditingDefinition && _variableRenameDrafts.TryGetValue(def.variableName, out string pendingRename)) renameDraft = pendingRename;

            string renameFieldValue = EditorGUILayout.TextField(renameDraft, GUILayout.Width(VAR_FIELD_NAME_WIDTH));
            renameFieldValue = Regex.Replace(renameFieldValue, @"[^a-zA-Z0-9_]", "");

            if (isEditingDefinition) _variableRenameDrafts[def.variableName] = renameFieldValue;

            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!isEditingDefinition);
            var newType = (ConditionVariableType)EditorGUILayout.EnumPopup(def.variableType, GUILayout.Width(VAR_FIELD_TYPE_WIDTH));

            switch (newType)
            {
                case ConditionVariableType.Bool:
                    def.boolDefault = EditorGUILayout.Toggle(def.boolDefault, GUILayout.Width(VAR_FIELD_VALUE_WIDTH));
                    break;
                case ConditionVariableType.Int:
                    {
                        _editingFloatStr.Remove(index);
                        string intDisplay;
                        if (_editingIntStr.TryGetValue(index, out var istr)) intDisplay = istr;
                        else intDisplay = def.intDefault.ToString(CultureInfo.InvariantCulture);
                        string filtered = FilterIntString(EditorGUILayout.TextField(intDisplay, GUILayout.Width(VAR_FIELD_VALUE_WIDTH)));
                        if (int.TryParse(filtered, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedInt))
                        {
                            def.intDefault = parsedInt;
                            _editingIntStr.Remove(index);
                        }
                        else
                        {
                            _editingIntStr[index] = filtered;
                        }
                    }
                    break;
                case ConditionVariableType.Float:
                    {
                        _editingIntStr.Remove(index);
                        string floatDisplay;
                        if (_editingFloatStr.TryGetValue(index, out var fstr)) floatDisplay = fstr;
                        else floatDisplay = def.floatDefault.ToString(CultureInfo.InvariantCulture);
                        string filtered = FilterFloatString(EditorGUILayout.TextField(floatDisplay, GUILayout.Width(VAR_FIELD_VALUE_WIDTH)));
                        if (float.TryParse(filtered, NumberStyles.Float, CultureInfo.InvariantCulture, out float parsedFloat))
                        {
                            def.floatDefault = parsedFloat;
                            _editingFloatStr.Remove(index);
                        }
                        else
                        {
                            _editingFloatStr[index] = filtered;
                        }
                    }
                    break;
                case ConditionVariableType.String:
                    def.stringDefault = EditorGUILayout.TextField(def.stringDefault ?? "", GUILayout.Width(VAR_FIELD_VALUE_WIDTH));
                    break;
            }

            if (isEditingDefinition) def.variableType = newType;
            
            EditorGUI.EndDisabledGroup();
            string editDefaultButtonLabel;

            if (isEditingDefinition) editDefaultButtonLabel = "Save";
            else editDefaultButtonLabel = "Template";

            if (GUILayout.Button(new GUIContent(editDefaultButtonLabel, "Edit name, type, and default value in the SO. Click again to save."), EditorStyles.miniButton, GUILayout.Width(VAR_EDIT_DEFAULT_BUTTON_WIDTH)))
            {
                if (!isEditingDefinition) BeginVariableDefinitionEdit(def);
                else TryCommitVariableDefinitionEdit(def, index);
            }

            EditorGUI.BeginDisabledGroup(!isEditingCurrentValue);
            DrawVariableCurrentValueField(def);
            EditorGUI.EndDisabledGroup();
            string editCurrentButtonLabel;

            if (isEditingCurrentValue) editCurrentButtonLabel = "Save";
            else editCurrentButtonLabel = "Current";

            if (GUILayout.Button(new GUIContent(editCurrentButtonLabel, "Edit current value in G. Click again to finish editing. save.json - button at the top."), EditorStyles.miniButton, GUILayout.Width(VAR_EDIT_CURRENT_BUTTON_WIDTH)))
            {
                if (!isEditingCurrentValue) _variableCurrentValueEditKeys.Add(def.variableName);
                else TryCommitVariableCurrentValueEdit(def.variableName);
            }

            GUI.backgroundColor = DELETE_BUTTON_COLOR;

            if (GUILayout.Button("×", GUILayout.Width(VAR_BUTTON_WIDTH)))
            {
                if (TryRemoveVariableFromRegistry(index, def)) ClearVariableRegistryRowState(def.variableName);
            }

            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private bool TryRemoveVariableFromRegistry(int index, VariableDefinition def)
        {
            if (def == null || database == null) return false;

            DialogReferenceIndex referenceIndex = GetLiveDialogReferenceIndex();
            IReadOnlyList<DialogReferenceUsage> usages = referenceIndex.GetVariableUsages(def.variableName);
            IReadOnlyList<DialogReferenceUsage> savedUsages = _savedDialogReferenceIndex.GetVariableUsages(def.variableName);

            string message;
            if (usages.Count > 0) message = BuildRegistryDeleteUsageMessage(usages, savedUsages, "Variable will be removed from the registry, and references in dialogs will be marked as missing.");
            else message = $"Delete variable '{def.variableName}'?";

            if (!EditorUtility.DisplayDialog("Delete variable", message, "Delete", "Cancel")) return false;

            if (usages.Count > 0 && database != null)
            {
                string variableName = def.variableName;
                Undo.RecordObject(database, "Mark Missing Variable References");
                DialogReferenceCleanup.MarkVariableMissingInDialogs(database, variableName);
                SaveRegistryDialogGraphPatch("Delete variable - mark missing dialog references", () => DialogReferenceCleanup.MarkVariableMissingInDialogs(database, variableName));
            }

            Undo.RecordObject(database, "Remove Variable");
            database.RemoveVariableAt(index);
            G.RemoveVariable(def.variableName, def.variableType);
            G.SaveGame(logResult: false);
            ClearVariableRegistryRowState(def.variableName);
            SaveDialogRegistry("Delete variable");
            InvalidateReferenceIndex();
            return true;
        }

        #endregion Variable editor
    }
}
#endif
