#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ALTUSHKA.EasyNodeDialogSystem.Editor
{
    internal static class DialogEditorPopupLayout
    {
        internal const int CATEGORY_WIDTH = 340;
        internal const int CATEGORY_HEIGHT = 400;
        internal const int BUTTON_ADD_WIDTH = 80;
        internal const int ROW_ACTION_BUTTON_WIDTH = 64;
        internal const int ROW_BUTTON_HEIGHT = 20;
        internal const int HEADER_HEIGHT = 24;
        internal const int CLOSE_BUTTON_SIZE = HEADER_HEIGHT;
        internal const int CLOSE_BUTTON_FONT_SIZE = 13;
        internal const int HEADER_TITLE_FONT_SIZE = 12;
        internal const int CONTENT_TOP_PADDING = 5;
        internal const int EDITOR_SPACE_MEDIUM = 10;
        internal const int CONTENT_SIDE_PADDING = 5;
        internal const int HEADER_SEPARATOR_HEIGHT = 1;
        internal static readonly Color DELETE_BUTTON_COLOR = new Color(1f, 0.7f, 0.7f);

        internal static void BeginContentPadding()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(CONTENT_SIDE_PADDING);
            EditorGUILayout.BeginVertical();
        }

        internal static void EndContentPadding()
        {
            EditorGUILayout.EndVertical();
            GUILayout.Space(CONTENT_SIDE_PADDING);
            EditorGUILayout.EndHorizontal();
        }

        internal static void DrawTitleHeader(string title, Action onClose)
        {
            Rect headerRect = GUILayoutUtility.GetRect(0, HEADER_HEIGHT, GUILayout.ExpandWidth(true));

            Color headerBg;
            Color separatorColor;

            if (EditorGUIUtility.isProSkin)
            {
                headerBg = new Color(0.22f, 0.22f, 0.22f, 1f);
                separatorColor = new Color(0f, 0f, 0f, 0.45f);
            }
            else
            {
                headerBg = new Color(0.76f, 0.76f, 0.76f, 1f);
                separatorColor = new Color(0f, 0f, 0f, 0.2f);
            }

            EditorGUI.DrawRect(headerRect, headerBg);
            EditorGUI.DrawRect(new Rect(headerRect.x, headerRect.yMax - HEADER_SEPARATOR_HEIGHT, headerRect.width, HEADER_SEPARATOR_HEIGHT), separatorColor);

            float closeSize = CLOSE_BUTTON_SIZE;
            Rect closeRect = new Rect(headerRect.xMax - closeSize, headerRect.y, closeSize, closeSize);
            Rect titleRect = new Rect(headerRect.x + closeSize, headerRect.y, headerRect.width - closeSize * 2, headerRect.height);

            GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = HEADER_TITLE_FONT_SIZE,
                padding = new RectOffset(0, 0, 0, 0)
            };
            GUI.Label(titleRect, title, titleStyle);

            GUIStyle closeStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = CLOSE_BUTTON_FONT_SIZE,
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                border = new RectOffset(2, 2, 2, 2)
            };

            if (GUI.Button(closeRect, new GUIContent("×", "Close"), closeStyle)) onClose?.Invoke();
        }
    }

    public abstract class DialogEditorManagedListPopup : PopupWindowContent
    {
        protected Vector2 scrollPos;
        protected int editingIndex = -1;
        protected string draftValue = "";
        protected string draftOriginalValue = "";
        protected string newItemDraft = "";
        protected readonly Action<string> persistChanges;
        protected readonly DialogDatabase database;
        protected readonly List<string> items;

        protected DialogEditorManagedListPopup(DialogDatabase database, List<string> items, Action<string> persistChanges)
        {
            this.database = database;
            this.items = items ?? new List<string>();
            this.persistChanges = persistChanges;
        }

        protected abstract string PopupTitle { get; }
        protected abstract string NewItemLabel { get; }
        protected abstract string ExistingItemsLabel { get; }
        protected abstract string UndoAddLabel { get; }
        protected abstract string UndoRenameLabel { get; }
        protected abstract string UndoDeleteLabel { get; }
        protected abstract string RenameDialogTitle { get; }
        protected abstract string DeleteDialogTitle { get; }
        protected abstract string PersistLogItemKind { get; }

        protected abstract void WriteItemsToDatabase();
        protected abstract string BuildRenameConfirmMessage(string oldName, string newName);
        protected abstract string BuildDeleteConfirmMessage(string name);
        protected abstract void ApplyRename(string oldName, string newName);
        protected abstract void ApplyRemove(string name);
        protected virtual bool CanMutateItem(string item) => true;

        public override void OnOpen()
        {
            editorWindow.titleContent = new GUIContent(PopupTitle);
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(DialogEditorPopupLayout.CATEGORY_WIDTH, DialogEditorPopupLayout.CATEGORY_HEIGHT);
        }

        public override void OnGUI(Rect rect)
        {
            DialogEditorPopupLayout.DrawTitleHeader(PopupTitle, () => TryClosePopup());
            EditorGUILayout.Space(DialogEditorPopupLayout.CONTENT_TOP_PADDING);
            DialogEditorPopupLayout.BeginContentPadding();

            DrawAddSection();
            
            EditorGUILayout.Space(DialogEditorPopupLayout.EDITOR_SPACE_MEDIUM);
            GUILayout.Label(ExistingItemsLabel, EditorStyles.boldLabel);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            for (int i = 0; i < items.Count; i++)
            {
                DrawManagedRow(i);
            }

            EditorGUILayout.EndScrollView();
            DrawAdditionalSections();
            DialogEditorPopupLayout.EndContentPadding();
        }

        protected virtual void DrawAdditionalSections()
        {
        }

        private void DrawAddSection()
        {
            GUILayout.Label(NewItemLabel, EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            newItemDraft = EditorGUILayout.TextField(newItemDraft ?? "", GUILayout.ExpandWidth(true), GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT));
            GUI.enabled = !string.IsNullOrWhiteSpace(newItemDraft) && !items.Contains(newItemDraft.Trim());

            if (GUILayout.Button("Add", GUILayout.Width(DialogEditorPopupLayout.BUTTON_ADD_WIDTH), GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT)))
            {
                Undo.RecordObject(database, UndoAddLabel);
                items.Add(newItemDraft.Trim());
                WriteItemsToDatabase();
                newItemDraft = "";
                persistChanges?.Invoke("Add " + PersistLogItemKind);
                GUI.FocusControl(null);
            }

            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawManagedRow(int index)
        {
            string item = items[index];
            bool isEditing = editingIndex == index;
            bool canMutate = CanMutateItem(item);

            EditorGUILayout.BeginHorizontal(GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT));
            DrawRowTextField(index, item, isEditing);

            if (isEditing)
            {
                DrawRowActionButtonsPair(
                    () => { if (TryApplyDraft()) CancelDraft(); },
                    "Save",
                    CancelDraft,
                    "Cancel");
            }
            else if (canMutate)
            {
                DrawRowActionButtonsPair(
                    () => BeginDraft(index, item),
                    "Rename",
                    () => TryRemoveAt(index),
                    "Delete",
                    secondIsDanger: true);
            }
            else DrawRowActionButtonsPlaceholder();
                
                EditorGUILayout.EndHorizontal();
            }

        private void DrawRowTextField(int index, string displayValue, bool isEditing)
        {
            string controlName;
            if (isEditing) controlName = $"DialogEditorPopupEdit_{GetType().Name}_{index}";
            else controlName = $"DialogEditorPopupRead_{GetType().Name}_{index}";

            GUI.SetNextControlName(controlName);

            if (isEditing)
            {
                draftValue = EditorGUILayout.TextField(draftValue ?? "", GUILayout.ExpandWidth(true), GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT));
                return;
            }

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField(displayValue ?? "", GUILayout.ExpandWidth(true), GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT));
            EditorGUI.EndDisabledGroup();
        }

        private void DrawRowActionButtonsPair(Action firstAction, string firstLabel, Action secondAction, string secondLabel, bool secondIsDanger = false)
        {
            if (GUILayout.Button(firstLabel, GUILayout.Width(DialogEditorPopupLayout.ROW_ACTION_BUTTON_WIDTH), GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT)))
                firstAction?.Invoke();

            if (secondIsDanger) GUI.backgroundColor = DialogEditorPopupLayout.DELETE_BUTTON_COLOR;

            if (GUILayout.Button(secondLabel, GUILayout.Width(DialogEditorPopupLayout.ROW_ACTION_BUTTON_WIDTH), GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT)))
                secondAction?.Invoke();

            if (secondIsDanger) GUI.backgroundColor = Color.white;
        }

        private void DrawRowActionButtonsPlaceholder()
        {
            GUILayout.Space(DialogEditorPopupLayout.ROW_ACTION_BUTTON_WIDTH * 2);
        }

        private bool HasUnsavedDraft => editingIndex >= 0;

        private bool TryClosePopup()
        {
            if (!HasUnsavedDraft)
            {
                editorWindow.Close();
                return true;
            }

            int choice = EditorUtility.DisplayDialogComplex(
                "Unsaved changes",
                "You have an unfinished edit. Save it before closing?",
                "Save",
                "Stay",
                "Discard");

            if (choice == 1) return false;
            if (choice == 0 && !TryApplyDraft()) return false;

            CancelDraft();
            editorWindow.Close();
            return true;
        }

        private void BeginDraft(int index, string value)
        {
            editingIndex = index;
            draftOriginalValue = value ?? "";
            draftValue = draftOriginalValue;
            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(null);
        }

        private void CancelDraft()
        {
            draftValue = draftOriginalValue;
            draftOriginalValue = "";
            editingIndex = -1;
            GUIUtility.keyboardControl = 0;
            GUI.FocusControl(null);
            if (editorWindow != null) editorWindow.Repaint();
        }

        private bool TryApplyDraft()
        {
            if (editingIndex < 0 || editingIndex >= items.Count) return false;

            string oldName = items[editingIndex];
            string newName = (draftValue ?? "").Trim();
            if (string.IsNullOrWhiteSpace(newName)) return false;
            if (newName == oldName) return true;

            if (items.Contains(newName))
            {
                EditorUtility.DisplayDialog(RenameDialogTitle, $"\"{newName}\" already exists.", "OK");
                return false;
            }

            if (!EditorUtility.DisplayDialog(RenameDialogTitle, BuildRenameConfirmMessage(oldName, newName), "Rename", "Cancel")) return false;

            Undo.RecordObject(database, UndoRenameLabel);
            items[editingIndex] = newName;
            WriteItemsToDatabase();
            ApplyRename(oldName, newName);
            persistChanges?.Invoke("Rename " + PersistLogItemKind);
            return true;
        }

        private void TryRemoveAt(int index)
        {
            if (index < 0 || index >= items.Count) return;

            string name = items[index];
            if (!CanMutateItem(name)) return;
            if (!EditorUtility.DisplayDialog(DeleteDialogTitle, BuildDeleteConfirmMessage(name), "Delete", "Cancel")) return;

            Undo.RecordObject(database, UndoDeleteLabel);
            items.RemoveAt(index);
            WriteItemsToDatabase();
            ApplyRemove(name);

            if (editingIndex == index) CancelDraft();
            else if (editingIndex > index) editingIndex--;

            persistChanges?.Invoke("Delete " + PersistLogItemKind);
        }
    }

    public class DialogCategoryPopup : DialogEditorManagedListPopup
    {
        public DialogCategoryPopup(DialogDatabase database, Action<string> persistChanges)
            : base(database, database?.Categories != null ? new List<string>(database.Categories) : new List<string>(), persistChanges)
        {
        }

        protected override string PopupTitle => "Category management";
        protected override string NewItemLabel => "New category";
        protected override string ExistingItemsLabel => "Existing categories";
        protected override string UndoAddLabel => "Add Category";
        protected override string UndoRenameLabel => "Rename Category";
        protected override string UndoDeleteLabel => "Delete Category";
        protected override string RenameDialogTitle => "Rename category";
        protected override string DeleteDialogTitle => "Delete category";
        protected override string PersistLogItemKind => "Category";

        protected override bool CanMutateItem(string item)
        {
            return item != DialogEditorDatabaseMaintenance.DefaultCategoryName;
        }

        protected override void WriteItemsToDatabase()
        {
            database.Categories = new List<string>(items);
        }

        protected override string BuildRenameConfirmMessage(string oldName, string newName)
        {
            int dialogCount = DialogEditorDatabaseMaintenance.CountDialogsInCategory(database, oldName);
            if (dialogCount > 0) return $"Category \"{oldName}\" has {dialogCount} dialog(s). They will be moved to \"{newName}\".";
            return $"Rename category \"{oldName}\" to \"{newName}\"?";
        }

        protected override string BuildDeleteConfirmMessage(string name)
        {
            int dialogCount = DialogEditorDatabaseMaintenance.CountDialogsInCategory(database, name);
            if (dialogCount > 0)
                return $"Category \"{name}\" has {dialogCount} dialog(s). They will be moved to \"{DialogEditorDatabaseMaintenance.DefaultCategoryName}\".";
            return $"Delete category \"{name}\"?";
        }

        protected override void ApplyRename(string oldName, string newName)
        {
            DialogEditorDatabaseMaintenance.ApplyCategoryRename(database, oldName, newName);
        }

        protected override void ApplyRemove(string name)
        {
            DialogEditorDatabaseMaintenance.ApplyCategoryRemoval(database, name);
        }
    }

    public class LanguageManagementPopup : DialogEditorManagedListPopup
    {
        public LanguageManagementPopup(DialogDatabase database, Action<string> persistChanges)
            : base(database, database?.AvailableLanguages != null ? new List<string>(database.AvailableLanguages) : new List<string>(), persistChanges)
        {
        }

        protected override string PopupTitle => "Language management";
        protected override string NewItemLabel => "New language";
        protected override string ExistingItemsLabel => "Existing languages";
        protected override string UndoAddLabel => "Add Language";
        protected override string UndoRenameLabel => "Rename Language";
        protected override string UndoDeleteLabel => "Delete Language";
        protected override string RenameDialogTitle => "Rename language";
        protected override string DeleteDialogTitle => "Delete language";
        protected override string PersistLogItemKind => "Language";

        protected override void WriteItemsToDatabase()
        {
            database.AvailableLanguages = new List<string>(items);
        }

        protected override string BuildRenameConfirmMessage(string oldName, string newName)
        {
            int usageCount = DialogEditorDatabaseMaintenance.CountLanguageUsages(database, oldName);
            if (usageCount > 0) return $"Language \"{oldName}\" is used in {usageCount} text entry(ies). They will be moved to \"{newName}\".";
            return $"Rename language \"{oldName}\" to \"{newName}\"?";
        }

        protected override string BuildDeleteConfirmMessage(string name)
        {
            int usageCount = DialogEditorDatabaseMaintenance.CountLanguageUsages(database, name);
            if (usageCount > 0) return $"Language \"{name}\" is used in {usageCount} text entry(ies). They will be removed.";
            return $"Delete language \"{name}\"?";
        }

        protected override void ApplyRename(string oldName, string newName)
        {
            DialogEditorDatabaseMaintenance.ApplyLanguageRename(database, oldName, newName);
        }

        protected override void ApplyRemove(string name)
        {
            DialogEditorDatabaseMaintenance.ApplyLanguageRemoval(database, name);
        }

        public override Vector2 GetWindowSize()
        {
            int orphanCount = DialogEditorDatabaseMaintenance.GetOrphanLanguageUsages(database).Count;
            float height = DialogEditorPopupLayout.CATEGORY_HEIGHT;

            if (orphanCount > 0) height += ORPHAN_SECTION_BASE_HEIGHT + orphanCount * ORPHAN_ROW_HEIGHT;

            return new Vector2(DialogEditorPopupLayout.CATEGORY_WIDTH, height);
        }

        protected override void DrawAdditionalSections()
        {
            List<DialogEditorDatabaseMaintenance.OrphanLanguageUsage> orphans = DialogEditorDatabaseMaintenance.GetOrphanLanguageUsages(database);
            if (orphans.Count == 0) return;

            EditorGUILayout.Space(DialogEditorPopupLayout.EDITOR_SPACE_MEDIUM);
            GUILayout.Label("Texts without language slot", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("These language codes exist in dialog or name-color texts but not in Available languages.", MessageType.Info);

            for (int i = 0; i < orphans.Count; i++)
            {
                DialogEditorDatabaseMaintenance.OrphanLanguageUsage orphan = orphans[i];
                EditorGUILayout.BeginHorizontal(GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT));
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(
                    $"{orphan.LanguageCode} ({orphan.UsageCount} entries)",
                    GUILayout.ExpandWidth(true),
                    GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT));
                EditorGUI.EndDisabledGroup();

                if (GUILayout.Button("Add", GUILayout.Width(DialogEditorPopupLayout.ROW_ACTION_BUTTON_WIDTH), GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT)))
                    TryAddOrphanLanguageToList(orphan.LanguageCode);

                GUI.backgroundColor = DialogEditorPopupLayout.DELETE_BUTTON_COLOR;

                if (GUILayout.Button("Delete texts", GUILayout.Width(ORPHAN_DELETE_BUTTON_WIDTH), GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT)))
                    TryRemoveOrphanLanguageTexts(orphan.LanguageCode);

                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();
            }
        }

        private const int ORPHAN_SECTION_BASE_HEIGHT = 56;
        private const int ORPHAN_ROW_HEIGHT = 24;
        private const int ORPHAN_DELETE_BUTTON_WIDTH = 88;

        private void TryAddOrphanLanguageToList(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode) || items.Contains(languageCode)) return;

            Undo.RecordObject(database, "Add Orphan Language");
            items.Add(languageCode);
            WriteItemsToDatabase();
            persistChanges?.Invoke("Add orphan language to Available languages");
        }

        private void TryRemoveOrphanLanguageTexts(string languageCode)
        {
            if (string.IsNullOrEmpty(languageCode)) return;

            int usageCount = DialogEditorDatabaseMaintenance.CountLanguageUsages(database, languageCode);
            string message = $"Delete {usageCount} text entry(ies) for language \"{languageCode}\"? Available languages list will not change.";

            if (!EditorUtility.DisplayDialog("Delete orphan texts", message, "Delete", "Cancel")) return;

            Undo.RecordObject(database, "Delete Orphan Language Texts");
            DialogEditorDatabaseMaintenance.RemoveLanguageTextsOnly(database, languageCode);
            persistChanges?.Invoke("Delete orphan language texts");
        }
    }

    internal static class DialogEditorDatabaseMaintenance
    {
        internal const string DefaultCategoryName = "Unnamed";

        public static int CountDialogsInCategory(DialogDatabase database, string category)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(category)) return 0;

            int count = 0;
            for (int i = 0; i < database.Dialogs.Count; i++)
            {
                DialogEntry dialog = database.Dialogs[i];
                if (dialog == null) continue;
                if (dialog.initiator == category) count++;
            }

            return count;
        }

        public static void ApplyCategoryRename(DialogDatabase database, string oldName, string newName)
        {
            if (database == null || string.IsNullOrEmpty(oldName) || string.IsNullOrWhiteSpace(newName)) return;

            if (database.Categories != null)
            {
                for (int i = 0; i < database.Categories.Count; i++)
                {
                    if (database.Categories[i] == oldName) database.Categories[i] = newName;
                }
            }

            if (database.Dialogs != null)
            {
                for (int i = 0; i < database.Dialogs.Count; i++)
                {
                    DialogEntry dialog = database.Dialogs[i];
                    if (dialog == null) continue;
                    if (dialog.initiator == oldName) dialog.initiator = newName;

                    if (dialog.nodes == null) continue;

                    for (int j = 0; j < dialog.nodes.Count; j++)
                    {
                        DialogNode node = dialog.nodes[j];
                        if (node?.RedirectData == null) continue;
                        if (node.RedirectData.TargetCategory == oldName) node.RedirectData.TargetCategory = newName;
                    }
                }
            }
        }

        public static void ApplyCategoryRemoval(DialogDatabase database, string category)
        {
            if (database == null || string.IsNullOrEmpty(category)) return;

            if (database.Categories != null) database.Categories.Remove(category);

            if (database.Dialogs == null) return;

            for (int i = 0; i < database.Dialogs.Count; i++)
            {
                DialogEntry dialog = database.Dialogs[i];
                if (dialog == null) continue;
                if (dialog.initiator != category) continue;

                dialog.initiator = DefaultCategoryName;

                if (dialog.nodes == null) continue;

                for (int j = 0; j < dialog.nodes.Count; j++)
                {
                    DialogNode node = dialog.nodes[j];
                    if (node?.RedirectData == null) continue;
                    if (node.RedirectData.TargetCategory == category) node.RedirectData.TargetCategory = DefaultCategoryName;
                }
            }

            if (database.Categories != null && !database.Categories.Contains(DefaultCategoryName)) database.Categories.Insert(0, DefaultCategoryName);
        }

        public static int CountLanguageUsages(DialogDatabase database, string languageCode)
        {
            if (database == null || string.IsNullOrEmpty(languageCode)) return 0;

            int count = 0;
            CountLanguageInDialogs(database.Dialogs, languageCode, ref count);
            CountLanguageInNameColors(database.CharacterNameColors, languageCode, ref count);
            return count;
        }

        public readonly struct OrphanLanguageUsage
        {
            public readonly string LanguageCode;
            public readonly int UsageCount;

            public OrphanLanguageUsage(string languageCode, int usageCount)
            {
                LanguageCode = languageCode;
                UsageCount = usageCount;
            }
        }

        public static List<OrphanLanguageUsage> GetOrphanLanguageUsages(DialogDatabase database)
        {
            List<OrphanLanguageUsage> orphans = new List<OrphanLanguageUsage>();
            if (database == null) return orphans;

            Dictionary<string, int> usageByCode = new Dictionary<string, int>();
            CollectLanguageUsageCounts(database.Dialogs, database.CharacterNameColors, usageByCode);

            foreach (KeyValuePair<string, int> pair in usageByCode)
            {
                if (string.IsNullOrEmpty(pair.Key)) continue;
                if (IsAvailableLanguage(database, pair.Key)) continue;

                orphans.Add(new OrphanLanguageUsage(pair.Key, pair.Value));
            }

            orphans.Sort((a, b) => string.Compare(a.LanguageCode, b.LanguageCode, StringComparison.Ordinal));

            return orphans;
        }

        public static void RemoveLanguageTextsOnly(DialogDatabase database, string languageCode)
        {
            if (database == null || string.IsNullOrEmpty(languageCode)) return;

            RemoveLanguageFromDialogs(database.Dialogs, languageCode);
            RemoveLanguageFromNameColors(database.CharacterNameColors, languageCode);
        }

        private static bool IsAvailableLanguage(DialogDatabase database, string languageCode)
        {
            if (database?.AvailableLanguages == null) return false;

            return database.AvailableLanguages.Contains(languageCode);
        }

        private static void CollectLanguageUsageCounts( List<DialogEntry> dialogs, List<CharacterNameColor> nameColors, Dictionary<string, int> usageByCode)
        {
            if (usageByCode == null) return;
            if (dialogs != null)
            {
                for (int i = 0; i < dialogs.Count; i++)
                {
                    DialogEntry dialog = dialogs[i];

                    if (dialog?.nodes == null) continue;

                    for (int j = 0; j < dialog.nodes.Count; j++)
                    {
                        DialogNode node = dialog.nodes[j];

                        if (node == null) continue;

                        if (node.NodeType == NodeType.Line && node.LineData?.DialogsText != null)
                        {
                            for (int k = 0; k < node.LineData.DialogsText.Count; k++)
                            {
                                LineText text = node.LineData.DialogsText[k];
                                if (text == null || string.IsNullOrEmpty(text.LanguageCode)) continue;

                                IncrementUsageCount(usageByCode, text.LanguageCode);
                            }
                        }
                        else if (node.NodeType == NodeType.Choice && node.ChoiceData != null)
                        {
                            for (int c = 0; c < node.ChoiceData.Count; c++)
                            {
                                ChoiceNode choice = node.ChoiceData[c];

                                if (choice?.Translations == null) continue;

                                for (int k = 0; k < choice.Translations.Count; k++)
                                {
                                    LineText text = choice.Translations[k];

                                    if (text == null || string.IsNullOrEmpty(text.LanguageCode)) continue;

                                    IncrementUsageCount(usageByCode, text.LanguageCode);
                                }
                            }
                        }
                    }
                }
            }

            if (nameColors == null) return;

            for (int i = 0; i < nameColors.Count; i++)
            {
                CharacterNameColor nameColor = nameColors[i];

                if (nameColor?.Translations == null) continue;

                for (int j = 0; j < nameColor.Translations.Count; j++)
                {
                    NameTranslation translation = nameColor.Translations[j];

                    if (translation == null || string.IsNullOrEmpty(translation.languageCode)) continue;

                    IncrementUsageCount(usageByCode, translation.languageCode);
                }
            }
        }

        private static void IncrementUsageCount(Dictionary<string, int> usageByCode, string languageCode)
        {
            if (usageByCode.TryGetValue(languageCode, out int count)) usageByCode[languageCode] = count + 1;
            else usageByCode[languageCode] = 1;
        }

        public static void ApplyLanguageRename(DialogDatabase database, string oldCode, string newCode)
        {
            if (database == null || string.IsNullOrEmpty(oldCode) || string.IsNullOrWhiteSpace(newCode)) return;

            if (database.AvailableLanguages != null)
            {
                for (int i = 0; i < database.AvailableLanguages.Count; i++)
                {
                    if (database.AvailableLanguages[i] == oldCode) database.AvailableLanguages[i] = newCode;
                }
            }

            if (database.CurrentLanguage == oldCode) database.CurrentLanguage = newCode;

            RenameLanguageInDialogs(database.Dialogs, oldCode, newCode);
            RenameLanguageInNameColors(database.CharacterNameColors, oldCode, newCode);
        }

        public static void ApplyLanguageRemoval(DialogDatabase database, string languageCode)
        {
            if (database == null || string.IsNullOrEmpty(languageCode)) return;
            if (database.AvailableLanguages != null) database.AvailableLanguages.Remove(languageCode);

            if (database.CurrentLanguage == languageCode)
            {
                if (database.AvailableLanguages != null && database.AvailableLanguages.Count > 0) database.CurrentLanguage = database.AvailableLanguages[0];
                else database.CurrentLanguage = "en";
            }

            RemoveLanguageFromDialogs(database.Dialogs, languageCode);
            RemoveLanguageFromNameColors(database.CharacterNameColors, languageCode);
        }

        private static void CountLanguageInDialogs(List<DialogEntry> dialogs, string languageCode, ref int count)
        {
            if (dialogs == null) return;

            for (int i = 0; i < dialogs.Count; i++)
            {
                DialogEntry dialog = dialogs[i];
                if (dialog?.nodes == null) continue;

                for (int j = 0; j < dialog.nodes.Count; j++)
                {
                    DialogNode node = dialog.nodes[j];
                    if (node == null) continue;

                    if (node.NodeType == NodeType.Line && node.LineData?.DialogsText != null)
                    {
                        for (int k = 0; k < node.LineData.DialogsText.Count; k++)
                        {
                            LineText text = node.LineData.DialogsText[k];
                            if (text != null && text.LanguageCode == languageCode) count++;
                        }
                    }
                    else if (node.NodeType == NodeType.Choice && node.ChoiceData != null)
                    {
                        for (int c = 0; c < node.ChoiceData.Count; c++)
                        {
                            ChoiceNode choice = node.ChoiceData[c];
                            if (choice?.Translations == null) continue;

                            for (int k = 0; k < choice.Translations.Count; k++)
                            {
                                LineText text = choice.Translations[k];
                                if (text != null && text.LanguageCode == languageCode) count++;
                            }
                        }
                    }
                }
            }
        }

        private static void CountLanguageInNameColors(List<CharacterNameColor> nameColors, string languageCode, ref int count)
        {
            if (nameColors == null) return;

            for (int i = 0; i < nameColors.Count; i++)
            {
                CharacterNameColor nameColor = nameColors[i];
                if (nameColor?.Translations == null) continue;

                for (int j = 0; j < nameColor.Translations.Count; j++)
                {
                    NameTranslation translation = nameColor.Translations[j];
                    if (translation != null && translation.languageCode == languageCode) count++;
                }
            }
        }

        private static void RenameLanguageInDialogs(List<DialogEntry> dialogs, string oldCode, string newCode)
        {
            if (dialogs == null) return;

            for (int i = 0; i < dialogs.Count; i++)
            {
                DialogEntry dialog = dialogs[i];
                if (dialog?.nodes == null) continue;

                for (int j = 0; j < dialog.nodes.Count; j++)
                {
                    DialogNode node = dialog.nodes[j];
                    if (node == null) continue;

                    if (node.NodeType == NodeType.Line && node.LineData?.DialogsText != null)
                    {
                        for (int k = 0; k < node.LineData.DialogsText.Count; k++)
                        {
                            LineText text = node.LineData.DialogsText[k];
                            if (text != null && text.LanguageCode == oldCode) text.LanguageCode = newCode;
                        }
                    }
                    else if (node.NodeType == NodeType.Choice && node.ChoiceData != null)
                    {
                        for (int c = 0; c < node.ChoiceData.Count; c++)
                        {
                            ChoiceNode choice = node.ChoiceData[c];
                            if (choice?.Translations == null) continue;

                            for (int k = 0; k < choice.Translations.Count; k++)
                            {
                                LineText text = choice.Translations[k];
                                if (text != null && text.LanguageCode == oldCode) text.LanguageCode = newCode;
                            }
                        }
                    }
                }
            }
        }

        private static void RenameLanguageInNameColors(List<CharacterNameColor> nameColors, string oldCode, string newCode)
        {
            if (nameColors == null) return;

            for (int i = 0; i < nameColors.Count; i++)
            {
                CharacterNameColor nameColor = nameColors[i];
                if (nameColor?.Translations == null) continue;

                for (int j = 0; j < nameColor.Translations.Count; j++)
                {
                    NameTranslation translation = nameColor.Translations[j];
                    if (translation != null && translation.languageCode == oldCode) translation.languageCode = newCode;
                }
            }
        }

        private static void RemoveLanguageFromDialogs(List<DialogEntry> dialogs, string languageCode)
        {
            if (dialogs == null) return;

            for (int i = 0; i < dialogs.Count; i++)
            {
                DialogEntry dialog = dialogs[i];
                if (dialog?.nodes == null) continue;

                for (int j = 0; j < dialog.nodes.Count; j++)
                {
                    DialogNode node = dialog.nodes[j];
                    if (node == null) continue;

                    if (node.NodeType == NodeType.Line && node.LineData?.DialogsText != null)
                    {
                        node.LineData.DialogsText.RemoveAll(t => t != null && t.LanguageCode == languageCode);
                    }
                    else if (node.NodeType == NodeType.Choice && node.ChoiceData != null)
                    {
                        for (int c = 0; c < node.ChoiceData.Count; c++)
                        {
                            ChoiceNode choice = node.ChoiceData[c];
                            if (choice?.Translations == null) continue;
                            choice.Translations.RemoveAll(t => t != null && t.LanguageCode == languageCode);
                        }
                    }
                }
            }
        }

        private static void RemoveLanguageFromNameColors(List<CharacterNameColor> nameColors, string languageCode)
        {
            if (nameColors == null) return;

            for (int i = 0; i < nameColors.Count; i++)
            {
                CharacterNameColor nameColor = nameColors[i];
                if (nameColor?.Translations == null) continue;
                nameColor.Translations.RemoveAll(t => t != null && t.languageCode == languageCode);
            }
        }
    }

    internal enum DialogLocalizationFileFormat
    {
        Csv = 0
    }

    public class DialogLocalizationImportExportPopup : PopupWindowContent
    {
        private const int POPUP_WIDTH = 340;
        private const int POPUP_HEIGHT = 145;
        private const int FORMAT_LABEL_WIDTH = 50;
        private const int ACTION_BUTTON_HEIGHT = 24;

        private static readonly string[] FormatLabels = { "CSV" };

        private readonly DialogDatabase database;
        private readonly Action onImportApplied;
        private DialogLocalizationFileFormat selectedFormat = DialogLocalizationFileFormat.Csv;

        public DialogLocalizationImportExportPopup(DialogDatabase database, Action onImportApplied)
        {
            this.database = database;
            this.onImportApplied = onImportApplied;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(POPUP_WIDTH, POPUP_HEIGHT);
        }

        public override void OnOpen()
        {
            editorWindow.titleContent = new GUIContent("Import / Export");
        }

        public override void OnGUI(Rect rect)
        {
            DialogEditorPopupLayout.DrawTitleHeader("Import / Export", () => editorWindow.Close());

            EditorGUILayout.Space(DialogEditorPopupLayout.CONTENT_TOP_PADDING);
            DialogEditorPopupLayout.BeginContentPadding();

            if (database == null)
            {
                EditorGUILayout.HelpBox("No Dialog Database assigned.", MessageType.Warning);
                DialogEditorPopupLayout.EndContentPadding();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Format:", GUILayout.Width(FORMAT_LABEL_WIDTH), GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT));

            selectedFormat = (DialogLocalizationFileFormat)EditorGUILayout.Popup((int)selectedFormat, FormatLabels, GUILayout.Height(DialogEditorPopupLayout.ROW_BUTTON_HEIGHT));

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(DialogEditorPopupLayout.EDITOR_SPACE_MEDIUM);
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Export", GUILayout.ExpandWidth(true), GUILayout.Height(ACTION_BUTTON_HEIGHT))) ExportSelectedFormat();

            if (GUILayout.Button("Import", GUILayout.ExpandWidth(true), GUILayout.Height(ACTION_BUTTON_HEIGHT))) ImportSelectedFormat();

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(DialogEditorPopupLayout.CONTENT_TOP_PADDING);
            EditorGUILayout.HelpBox("Exports Line and Choice texts. Do not edit the Key column. Keys use node id (and choice index).", MessageType.Info);
            DialogEditorPopupLayout.EndContentPadding();
        }

        private void ExportSelectedFormat()
        {
            if (selectedFormat != DialogLocalizationFileFormat.Csv)
            {
                EditorUtility.DisplayDialog("Export", "Selected format is not implemented yet.", "OK");

                return;
            }

            string defaultName = database.name + "_localization.csv";
            string path = EditorUtility.SaveFilePanel("Export localization CSV", Application.dataPath, defaultName, "csv");

            if (string.IsNullOrEmpty(path)) return;

            try
            {
                DialogLocalizationCsv.WriteCsvFile(database, path);
                EditorUtility.DisplayDialog("Export", "Exported:\n" + path, "OK");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Export", "Export failed:\n" + ex.Message, "OK");
            }
        }

        private void ImportSelectedFormat()
        {
            if (selectedFormat != DialogLocalizationFileFormat.Csv)
            {
                EditorUtility.DisplayDialog("Import", "Selected format is not implemented yet.", "OK");
                return;
            }

            string path = EditorUtility.OpenFilePanel("Import localization CSV", Application.dataPath, "csv");
            
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                string csv = File.ReadAllText(path, Encoding.UTF8);
                List<DialogLocalizationCsv.CsvMissingLanguageColumn> missingLanguages = DialogLocalizationCsv.GetMissingLanguageColumns(csv, database);

                if (missingLanguages.Count > 0)
                {
                    string prompt = DialogLocalizationCsv.BuildMissingLanguagesImportPromptMessage(missingLanguages);
                    int choice = EditorUtility.DisplayDialogComplex("Import localization", prompt, "Add", "Import only", "Cancel");

                    if (choice == 2 || choice == -1) return;

                    if (choice == 0)
                    {
                        Undo.RecordObject(database, "Add CSV Languages");
                        DialogLocalizationCsv.AddLanguagesToAvailableList(database, missingLanguages);
                    }
                }

                Undo.RecordObject(database, "Import Localization CSV");
                DialogLocalizationCsv.ImportResult result = DialogLocalizationCsv.ApplyCsv(database, csv);

                if (!result.Ok)
                {
                    EditorUtility.DisplayDialog("Import", result.BuildMessage(), "OK");

                    return;
                }

                EditorUtility.SetDirty(database);
                AssetDatabase.SaveAssetIfDirty(database);
                onImportApplied?.Invoke();
                EditorUtility.DisplayDialog("Import", result.BuildMessage(), "OK");
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                EditorUtility.DisplayDialog("Import", "Import failed:\n" + ex.Message, "OK");
            }
        }
    }

    internal static class DialogEditorAudioPreview
    {
        private static MethodInfo _playMethod;
        private static MethodInfo _pauseMethod;
        private static MethodInfo _resumeMethod;
        private static MethodInfo _stopMethod;
        private static MethodInfo _isPlayingMethod;
        private static MethodInfo _getPositionMethod;
        private static bool _resolved;

        private static AudioClip _activeClip;
        private static bool _isPaused;
        private static bool _playbackActive;
        private static float _manualTimeSeconds;
        private static EditorWindow _repaintTarget;
        private static bool _updateSubscribed;

        internal static AudioClip ActiveClip => _activeClip;

        internal static void SetRepaintTarget(EditorWindow window)
        {
            _repaintTarget = window;
        }

        internal static bool IsActivelyPlaying(AudioClip clip)
        {
            if (clip == null || _activeClip != clip || _isPaused) return false;
            return IsPreviewPlaying();
        }

        internal static float GetDuration(AudioClip clip)
        {
            if (clip == null) return 0f;
            return clip.length;
        }

        internal static float GetCurrentTime(AudioClip clip)
        {
            if (clip == null || _activeClip != clip) return 0f;

            if (!_playbackActive || _isPaused) return Mathf.Clamp(_manualTimeSeconds, 0f, clip.length);

            if (IsPreviewPlaying())
            {
                float position = GetPreviewPositionSeconds();
                return Mathf.Clamp(position, 0f, clip.length);
            }

            return Mathf.Clamp(_manualTimeSeconds, 0f, clip.length);
        }

        internal static void TogglePlayPause(AudioClip clip)
        {
            if (clip == null) return;

            if (IsActivelyPlaying(clip))
            {
                Pause();
                return;
            }

            if (_activeClip == clip && _isPaused)
            {
                Resume();
                return;
            }

            float startTime = 0f;
            if (_activeClip == clip) startTime = _manualTimeSeconds;
            PlayFromTime(clip, startTime);
        }

        internal static void PlayFromTime(AudioClip clip, float timeSeconds)
        {
            if (clip == null) return;
            Resolve();
            if (_playMethod == null) return;

            StopPreviewEngine();
            _activeClip = clip;
            _isPaused = false;
            _playbackActive = true;
            _manualTimeSeconds = Mathf.Clamp(timeSeconds, 0f, clip.length);

            int startSample = 0;
            if (clip.samples > 0 && clip.length > 0f) startSample = Mathf.Clamp(Mathf.RoundToInt((_manualTimeSeconds / clip.length) * clip.samples), 0, Mathf.Max(0, clip.samples - 1));

            if (_playMethod.GetParameters().Length >= 3) _playMethod.Invoke(null, new object[] { clip, startSample, false });
            else _playMethod.Invoke(null, new object[] { clip });

            SubscribeUpdate();
            if (_repaintTarget != null) _repaintTarget.Repaint();
        }

        internal static void Seek(AudioClip clip, float timeSeconds)
        {
            if (clip == null) return;
            Resolve();

            // Move playhead only - never start/restart audio here (scrub uses pause + resume on mouse up).
            _activeClip = clip;
            _manualTimeSeconds = Mathf.Clamp(timeSeconds, 0f, clip.length);
            _isPaused = false;
            _playbackActive = false;
            StopPreviewEngine();
            if (_repaintTarget != null) _repaintTarget.Repaint();
        }

        internal static void Pause()
        {
            if (_activeClip == null) return;
            Resolve();

            _manualTimeSeconds = GetCurrentTime(_activeClip);
            if (_pauseMethod != null) _pauseMethod.Invoke(null, null);
            else StopPreviewEngine();

            _isPaused = true;
            _playbackActive = false;
            SubscribeUpdate();
            if (_repaintTarget != null) _repaintTarget.Repaint();
        }

        internal static void Resume()
        {
            if (_activeClip == null) return;
            Resolve();

            if (_resumeMethod != null)
            {
                _isPaused = false;
                _playbackActive = true;
                _resumeMethod.Invoke(null, null);
                if (!IsPreviewPlaying()) PlayFromTime(_activeClip, _manualTimeSeconds);
                else
                {
                    SubscribeUpdate();
                    if (_repaintTarget != null) _repaintTarget.Repaint();
                }
                return;
            }

            PlayFromTime(_activeClip, _manualTimeSeconds);
        }

        internal static void Stop()
        {
            Resolve();
            StopPreviewEngine();
            _isPaused = false;
            _playbackActive = false;
            _activeClip = null;
            _manualTimeSeconds = 0f;
            UnsubscribeUpdate();
            if (_repaintTarget != null) _repaintTarget.Repaint();
        }

        internal static string FormatTime(float timeSeconds)
        {
            if (timeSeconds < 0f) timeSeconds = 0f;
            int totalSec = Mathf.FloorToInt(timeSeconds);
            int minutes = totalSec / 60;
            int seconds = totalSec % 60;
            int hundredths = Mathf.FloorToInt((timeSeconds - totalSec) * 100f);
            if (hundredths > 99) hundredths = 99;
            return minutes + ":" + seconds.ToString("00") + "." + hundredths.ToString("00");
        }

        private static bool IsPreviewPlaying()
        {
            Resolve();
            if (_isPlayingMethod == null) return false;
            object result = _isPlayingMethod.Invoke(null, null);
            if (result is bool playing) return playing;
            return false;
        }

        private static float GetPreviewPositionSeconds()
        {
            Resolve();
            if (_getPositionMethod == null) return _manualTimeSeconds;
            object result = _getPositionMethod.Invoke(null, null);
            if (result is float seconds) return seconds;
            return _manualTimeSeconds;
        }

        private static void StopPreviewEngine()
        {
            if (_stopMethod == null) return;
            if (_stopMethod.GetParameters().Length == 0) _stopMethod.Invoke(null, null);
            else _stopMethod.Invoke(null, new object[] { true });
        }

        private static void SubscribeUpdate()
        {
            if (_updateSubscribed) return;
            EditorApplication.update += OnEditorUpdate;
            _updateSubscribed = true;
        }

        private static void UnsubscribeUpdate()
        {
            if (!_updateSubscribed) return;
            EditorApplication.update -= OnEditorUpdate;
            _updateSubscribed = false;
        }

        private static void OnEditorUpdate()
        {
            if (_activeClip == null)
            {
                UnsubscribeUpdate();
                return;
            }

            if (_playbackActive && !_isPaused && !IsPreviewPlaying())
            {
                _manualTimeSeconds = 0f;
                _isPaused = false;
                _playbackActive = false;
                _activeClip = null;
                UnsubscribeUpdate();
            }

            if (_repaintTarget != null) _repaintTarget.Repaint();
        }

        private static void Resolve()
        {
            if (_resolved) return;
            _resolved = true;

            Type audioUtilType = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
            if (audioUtilType == null) return;

            _playMethod = audioUtilType.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
            if (_playMethod == null) _playMethod = audioUtilType.GetMethod("PlayClip", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(AudioClip), typeof(int), typeof(bool) }, null);
            if (_playMethod == null) _playMethod = audioUtilType.GetMethod("PlayClip", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(AudioClip) }, null);

            _pauseMethod = audioUtilType.GetMethod("PausePreviewClip", BindingFlags.Static | BindingFlags.Public);
            _resumeMethod = audioUtilType.GetMethod("ResumePreviewClip", BindingFlags.Static | BindingFlags.Public);
            _isPlayingMethod = audioUtilType.GetMethod("IsPreviewClipPlaying", BindingFlags.Static | BindingFlags.Public);
            _getPositionMethod = audioUtilType.GetMethod("GetPreviewClipPosition", BindingFlags.Static | BindingFlags.Public);

            _stopMethod = audioUtilType.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public);
            if (_stopMethod == null) _stopMethod = audioUtilType.GetMethod("StopAllClips", BindingFlags.Static | BindingFlags.Public);
        }
    }

    public class DialogEditorSettingsPopup : PopupWindowContent
    {
        private const int POPUP_WIDTH = 320;
        private const int POPUP_HEIGHT = 134;
        private const int LABEL_WIDTH = 140;
        private const int ROW_HEIGHT = 20;

        private readonly DialogDatabase database;
        private readonly Action<string> onDatabaseChanged;
        private readonly Action onEditorPreferencesChanged;

        public DialogEditorSettingsPopup(DialogDatabase database, Action<string> onDatabaseChanged, Action onEditorPreferencesChanged)
        {
            this.database = database;
            this.onDatabaseChanged = onDatabaseChanged;
            this.onEditorPreferencesChanged = onEditorPreferencesChanged;
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(POPUP_WIDTH, POPUP_HEIGHT);
        }

        public override void OnOpen()
        {
            editorWindow.titleContent = new GUIContent("Settings");
        }

        public override void OnGUI(Rect rect)
        {
            DialogEditorPopupLayout.DrawTitleHeader("Settings", () => editorWindow.Close());
            EditorGUILayout.Space(DialogEditorPopupLayout.CONTENT_TOP_PADDING);
            DialogEditorPopupLayout.BeginContentPadding();

            if (database == null)
            {
                EditorGUILayout.HelpBox("No Dialog Database assigned.", MessageType.Warning);
                DialogEditorPopupLayout.EndContentPadding();
                return;
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Include voice clips:", GUILayout.Width(LABEL_WIDTH), GUILayout.Height(ROW_HEIGHT));
            bool voiceEnabled = database.VoiceClipsEnabled;
            bool newVoiceEnabled = EditorGUILayout.Toggle(voiceEnabled, GUILayout.Height(ROW_HEIGHT));
            EditorGUILayout.EndHorizontal();

            if (newVoiceEnabled != voiceEnabled)
            {
                if (newVoiceEnabled)
                {
                    Undo.RecordObject(database, "Enable Voice Clips");
                    database.VoiceClipsEnabled = true;
                    onDatabaseChanged?.Invoke("Enable voice clips");
                }
                else
                {
                    int clipCount = database.CountAssignedVoiceClips();
                    string message = "Voice clip slots will be hidden.\n\n" + clipCount + " assigned audio clip(s) in dialogs will be cleared.\n\nContinue?";
                    if (EditorUtility.DisplayDialog("Disable Voice Clips", message, "Clear and disable", "Cancel"))
                    {
                        Undo.RecordObject(database, "Disable Voice Clips");
                        database.ClearAllVoiceClips();
                        database.VoiceClipsEnabled = false;
                        onDatabaseChanged?.Invoke("Disable voice clips");
                    }
                }
            }

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Save logs in Console:", GUILayout.Width(LABEL_WIDTH), GUILayout.Height(ROW_HEIGHT));
            bool saveLogsEnabled = EditorPrefs.GetBool(DialogEditorWindow.PREFS_DIALOG_EDITOR_SAVE_LOGS_KEY, true);
            bool newSaveLogsEnabled = EditorGUILayout.Toggle(saveLogsEnabled, GUILayout.Height(ROW_HEIGHT));
            EditorGUILayout.EndHorizontal();

            if (newSaveLogsEnabled != saveLogsEnabled)
            {
                EditorPrefs.SetBool(DialogEditorWindow.PREFS_DIALOG_EDITOR_SAVE_LOGS_KEY, newSaveLogsEnabled);
                onEditorPreferencesChanged?.Invoke();
            }

            EditorGUILayout.Space(DialogEditorPopupLayout.EDITOR_SPACE_MEDIUM);
            EditorGUILayout.HelpBox("Assign clips via ObjectField (drag-drop or Unity picker search). Large VO: split Dialog Databases by chapter.", MessageType.Info);
            DialogEditorPopupLayout.EndContentPadding();
        }
    }
}
#endif
