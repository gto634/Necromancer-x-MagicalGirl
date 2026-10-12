#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ALTUSHKA.EasyNodeDialogSystem.Editor
{
    public partial class DialogEditorWindow
    {
        #region Constants

        private const int SPRITE_CARD_WIDTH = 328;
        private const int SPRITE_CARD_PADDING = 20;
        private const int SPRITE_CARD_HEIGHT = 130;
        private const int SPRITE_CARD_BOTTOM_SPACING = 7;
        private const int SPRITE_EMOTION_LABEL_WIDTH = 120;
        private const int SPRITE_EMOTION_SPACING = 6;
        private const int SPRITE_DELETE_BUTTON_WIDTH = 60;
        private const int SPRITE_DELETE_BUTTON_HEIGHT = 25;
        private const int SPRITE_DELETE_GROUP_BUTTON_SIZE = 25;
        private const int SPRITE_GROUP_HEADER_HEIGHT = 30;
        private const int SPRITE_GROUP_ROW_SPACING = 20;
        private const int SPRITE_PREVIEW_SIZE = 350;
        private const int SPRITE_SECTION_SPACING = 30;
        private const int SPRITE_WINDOW_PADDING = 40;
        private const int SPRITE_ADD_BUTTON_HEIGHT = 25;
        private const int SPRITE_CARD_TOOLBAR_HEIGHT = 100;
        private const int SPRITE_LABEL_WIDTH = 60;
        private const int SPRITE_OBJECT_FIELD_SIZE = 60;
        private const int SPRITE_CONTENT_MAX_WIDTH_OFFSET = 40;
        private const int SPRITE_CHARACTER_NAME_FIELD_WIDTH = 800;
        private const int SPRITE_BUTTON_ADD_CHARACTER_WIDTH = 150;
        private const int SPRITE_REGISTRY_RENAME_ACTION_BUTTON_WIDTH = 108;
        private const int SPRITE_ROW_HEIGHT = 140;

        private static readonly Color SPRITE_CHARACTER_ROW_BG = new Color(0.95f, 0.95f, 0.95f);

        #endregion Constants 

        #region Fields

        private Vector2 characterScrollPos;
        private string characterSearchFilter = "";
        private Dictionary<string, bool> characterGroupFoldouts = new Dictionary<string, bool>();
        private readonly HashSet<int> _characterRenameIndices = new HashSet<int>();
        private readonly Dictionary<int, string> _characterRenameDrafts = new Dictionary<int, string>();
        private readonly HashSet<string> _spriteRenameKeys = new HashSet<string>();
        private readonly Dictionary<string, string> _spriteRenameDrafts = new Dictionary<string, string>();

        #endregion Fields

        #region Character sprite editor

        private void EnsureCharacterDatabaseData()
        {
            if (database == null) return;
            if (database.CharacterGroups == null) database.CharacterGroups = new List<CharacterGroup>();
        }

        private void DrawCharacterEditor()
        {
            BeginAssetSlotActionRow();
            DrawInlineDialogDatabaseSlot();
            GUILayout.FlexibleSpace();

            GUI.enabled = database != null;
            if (AssetRowToolbarButton("+ Add character", SPRITE_BUTTON_ADD_CHARACTER_WIDTH))
            {
                EnsureCharacterDatabaseData();
                Undo.RecordObject(database, "Add Character Group");
                string baseName = "New character";
                string newCharacterName = baseName;
                int index = 1;

                while (database.CharacterGroups.Any(g => g != null && g.characterName == newCharacterName))
                {
                    newCharacterName = $"{baseName} {index}";
                    index++;
                }

                var newGroup = new CharacterGroup { characterName = newCharacterName };
                newGroup.sprites.Add(new CharacterSprite { spriteName = GetUniqueSpriteName(newGroup, "New sprite") });
                database.CharacterGroups.Add(newGroup);
                SaveDialogRegistry("Add character");
            }

            GUI.enabled = true;
            EndAssetSlotActionRow();
            EditorGUILayout.Space(INLINE_HEADER_SECTION_PADDING);
            DrawSearchRow(ref characterSearchFilter, showClearButton: true);
            EndInlineHeaderSection();

            if (database == null)
            {
                EditorGUILayout.HelpBox("Assign a Dialog Database.", MessageType.Warning);
                return;
            }

            EnsureCharacterDatabaseData();
            int visibleCount = database.CharacterGroups.Count(g => string.IsNullOrEmpty(characterSearchFilter) || (g.characterName != null && g.characterName.ToLower().Contains(characterSearchFilter.ToLower())));
            EditorGUILayout.LabelField($"Characters found: {visibleCount} of {database.CharacterGroups.Count}");
            characterScrollPos = EditorGUILayout.BeginScrollView(characterScrollPos, GUILayout.ExpandHeight(true));
            List<int> groupsToRemove = new List<int>();

            for (int i = 0; i < database.CharacterGroups.Count; i++)
            {
                CharacterGroup group = database.CharacterGroups[i];

                if (group == null) continue;
                if (!string.IsNullOrEmpty(characterSearchFilter) && (group.characterName == null || !group.characterName.ToLower().Contains(characterSearchFilter.ToLower()))) continue;
                if (!characterGroupFoldouts.ContainsKey(group.characterName)) characterGroupFoldouts[group.characterName] = true;

                EditorGUILayout.BeginHorizontal(GUILayout.Height(SPRITE_GROUP_HEADER_HEIGHT));
                Rect groupRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(SPRITE_GROUP_HEADER_HEIGHT), GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(groupRect, CATEGORY_BG);
                GUILayout.Space(EDITOR_SPACE_SMALL);
                characterGroupFoldouts[group.characterName] = EditorGUILayout.Foldout(characterGroupFoldouts[group.characterName], $"{group.characterName} (Count: {group.sprites.Count})", true, categoryStyle);
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("+ Add sprite", GUILayout.Height(SPRITE_ADD_BUTTON_HEIGHT)))
                {
                    Undo.RecordObject(database, "Add Character Sprite");
                    group.sprites.Add(new CharacterSprite { spriteName = GetUniqueSpriteName(group, "New sprite") });
                    SaveDialogRegistry("Add sprite");
                }

                GUI.backgroundColor = DELETE_BUTTON_COLOR;

                if (GUILayout.Button("×", GUILayout.Width(SPRITE_DELETE_GROUP_BUTTON_SIZE), GUILayout.Height(SPRITE_DELETE_GROUP_BUTTON_SIZE)))
                {
                    if (TryRemoveCharacterFromRegistry(group.characterName)) groupsToRemove.Add(i);
                }

                EditorGUILayout.Space(SPRITE_GROUP_ROW_SPACING);
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                if (characterGroupFoldouts[group.characterName])
                {
                    EditorGUILayout.BeginVertical(GUILayout.MaxWidth(Screen.width - SPRITE_CONTENT_MAX_WIDTH_OFFSET));
                    bool isRenamingCharacter = _characterRenameIndices.Contains(i);
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Character name:", GUILayout.Width(SPRITE_LABEL_WIDTH));
                    EditorGUI.BeginDisabledGroup(!isRenamingCharacter);
                    string characterRenameDraft = group.characterName;

                    if (isRenamingCharacter && _characterRenameDrafts.TryGetValue(i, out string pendingCharacterRename)) characterRenameDraft = pendingCharacterRename;

                    string characterNameDraft = EditorGUILayout.TextField(characterRenameDraft, GUILayout.Width(SPRITE_CHARACTER_NAME_FIELD_WIDTH));

                    if (isRenamingCharacter) _characterRenameDrafts[i] = characterNameDraft;

                    EditorGUI.EndDisabledGroup();
                    string characterRenameButtonLabel;

                    if (isRenamingCharacter) characterRenameButtonLabel = "Apply";
                    else characterRenameButtonLabel = "Rename";

                    if (GUILayout.Button(new GUIContent(characterRenameButtonLabel, "Rename character and update references in dialogs. Click again to save."), GUILayout.Width(SPRITE_REGISTRY_RENAME_ACTION_BUTTON_WIDTH)))
                    {
                        if (!isRenamingCharacter)
                        {
                            _characterRenameIndices.Add(i);
                            _characterRenameDrafts[i] = group.characterName;
                        }
                        else TryCommitCharacterRename(i, group);
                    }

                    EditorGUILayout.EndHorizontal();
                    List<int> spritesToRemove = new List<int>();

                    if (group.sprites.Count > 0)
                    {
                        float windowWidth = EditorGUIUtility.currentViewWidth - SPRITE_WINDOW_PADDING;
                        int spritesPerRow = Mathf.Max(1, (int)(windowWidth / SPRITE_CARD_WIDTH));

                        for (int row = 0; row < Mathf.CeilToInt((float)group.sprites.Count / spritesPerRow); row++)
                        {
                            EditorGUILayout.BeginHorizontal(GUILayout.Height(SPRITE_ROW_HEIGHT));
                            GUI.backgroundColor = SPRITE_CHARACTER_ROW_BG;

                            for (int j = row * spritesPerRow; j < Math.Min((row + 1) * spritesPerRow, group.sprites.Count); j++)
                            {
                                CharacterSprite sprite = group.sprites[j];
                                EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(SPRITE_CARD_WIDTH - SPRITE_CARD_PADDING), GUILayout.Height(SPRITE_CARD_HEIGHT));
                                EditorGUILayout.BeginHorizontal();
                                EditorGUILayout.Space(EDITOR_SPACE_SMALL);
                                GUILayout.Label("Emotion name:", GUILayout.Width(SPRITE_EMOTION_LABEL_WIDTH));
                                string spriteRenameKey = BuildSpriteRenameKey(i, j);
                                bool isRenamingSprite = _spriteRenameKeys.Contains(spriteRenameKey);
                                EditorGUI.BeginDisabledGroup(!isRenamingSprite);
                                string spriteRenameDraft = sprite.spriteName;

                                if (isRenamingSprite && _spriteRenameDrafts.TryGetValue(spriteRenameKey, out string pendingSpriteRename)) spriteRenameDraft = pendingSpriteRename;

                                string spriteNameDraft = EditorGUILayout.TextField(spriteRenameDraft, GUILayout.ExpandWidth(true));

                                if (isRenamingSprite) _spriteRenameDrafts[spriteRenameKey] = spriteNameDraft;

                                EditorGUI.EndDisabledGroup();
                                string spriteRenameButtonLabel;

                                if (isRenamingSprite) spriteRenameButtonLabel = "Save";
                                else spriteRenameButtonLabel = "Rename";

                                if (GUILayout.Button(new GUIContent(spriteRenameButtonLabel, "Rename emotion and update references in dialogs. Click again to save."), GUILayout.Width(SPRITE_REGISTRY_RENAME_ACTION_BUTTON_WIDTH)))
                                {
                                    if (!isRenamingSprite)
                                    {
                                        _spriteRenameKeys.Add(spriteRenameKey);
                                        _spriteRenameDrafts[spriteRenameKey] = sprite.spriteName;
                                    }
                                    else TryCommitSpriteRename(i, j, group, sprite);
                                }

                                EditorGUILayout.Space(SPRITE_EMOTION_SPACING);
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space(EDITOR_SPACE_SMALL);
                                EditorGUILayout.BeginHorizontal(GUILayout.Height(SPRITE_CARD_TOOLBAR_HEIGHT));
                                EditorGUILayout.Space(EDITOR_SPACE_SMALL);
                                EditorGUILayout.BeginVertical();
                                GUILayout.Label(" Sprite:", GUILayout.Width(SPRITE_LABEL_WIDTH));
                                EditorGUI.BeginChangeCheck();
                                Sprite newSprite = (Sprite)EditorGUILayout.ObjectField(sprite.sprite, typeof(Sprite), false, GUILayout.Width(SPRITE_OBJECT_FIELD_SIZE), GUILayout.Height(SPRITE_OBJECT_FIELD_SIZE));

                                if (EditorGUI.EndChangeCheck() && newSprite != sprite.sprite)
                                {
                                    Undo.RecordObject(database, "Change Sprite");
                                    sprite.sprite = newSprite;
                                    SaveDialogRegistry("Assign sprite texture");
                                }

                                GUI.backgroundColor = DELETE_BUTTON_COLOR;
                                GUILayout.FlexibleSpace();

                                if (GUILayout.Button("Delete", GUILayout.Width(SPRITE_DELETE_BUTTON_WIDTH), GUILayout.Height(SPRITE_DELETE_BUTTON_HEIGHT)))
                                {
                                    if (TryRemoveSpriteFromRegistry(group.characterName, sprite.spriteName)) spritesToRemove.Add(j);
                                }

                                EditorGUILayout.EndVertical();

                                if (sprite.sprite != null)
                                {
                                    Rect previewRect = GUILayoutUtility.GetRect(SPRITE_PREVIEW_SIZE, SPRITE_PREVIEW_SIZE);
                                    DrawSpritePreview(previewRect, sprite.sprite);
                                }
                                else
                                {
                                    Rect previewRect = GUILayoutUtility.GetRect(SPRITE_PREVIEW_SIZE, SPRITE_PREVIEW_SIZE);
                                    GUI.Box(previewRect, "Select a sprite", new GUIStyle(GUI.skin.box) { alignment = TextAnchor.MiddleCenter });
                                }

                                EditorGUILayout.Space(EDITOR_SPACE_MEDIUM);
                                EditorGUILayout.EndHorizontal();
                                EditorGUILayout.Space(SPRITE_CARD_BOTTOM_SPACING);
                                GUI.backgroundColor = Color.white;
                                EditorGUILayout.EndVertical();
                            }

                            GUI.backgroundColor = Color.white;
                            EditorGUILayout.EndHorizontal();
                        }

                        EditorGUILayout.Space(SPRITE_SECTION_SPACING);
                    }

                    if (spritesToRemove.Count > 0)
                    {
                        Undo.RecordObject(database, "Remove Sprites");

                        foreach (int index in spritesToRemove.OrderByDescending(k => k))
                        {
                            group.sprites.RemoveAt(index);
                        }

                        SaveDialogRegistry("Remove sprites from character");
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            if (groupsToRemove.Count > 0)
            {
                Undo.RecordObject(database, "Remove Character Groups");

                foreach (int index in groupsToRemove.OrderByDescending(i => i))
                {
                    database.CharacterGroups.RemoveAt(index);
                }

                SaveDialogRegistry("Remove character groups");
            }

            EditorGUILayout.EndScrollView();

            if (visibleCount == 0)
            {
                string helpMessage;

                if (!string.IsNullOrEmpty(characterSearchFilter)) helpMessage = "Nothing found.";
                else helpMessage = "Character database is empty.";

                EditorGUILayout.HelpBox(helpMessage, MessageType.Info);
            }
        }

        private static void DrawSpritePreview(Rect rect, Sprite sprite)
        {
            if (sprite == null || sprite.texture == null) return;

            float spriteW = sprite.rect.width;
            float spriteH = sprite.rect.height;
            float spriteAspect = spriteW / spriteH;
            float rectAspect = rect.width / rect.height;
            float drawW;
            float drawH;

            if (spriteAspect > rectAspect)
            {
                drawW = rect.width;
                drawH = rect.width / spriteAspect;
            }
            else
            {
                drawH = rect.height;
                drawW = rect.height * spriteAspect;
            }

            float drawX = rect.x + (rect.width - drawW) * 0.5f;
            float drawY = rect.y + (rect.height - drawH) * 0.5f;
            Rect drawRect = new Rect(drawX, drawY, drawW, drawH);
            GUI.DrawTexture(drawRect, sprite.texture, ScaleMode.StretchToFill, true);
        }

        private static string BuildSpriteRenameKey(int groupIndex, int spriteIndex)
        {
            return groupIndex + ":" + spriteIndex;
        }

        private static string GetUniqueSpriteName(CharacterGroup group, string baseName)
        {
            if (string.IsNullOrWhiteSpace(baseName)) baseName = "New sprite";
            else baseName = baseName.Trim();

            if (group?.sprites == null || group.sprites.Count == 0) return baseName;

            string candidate = baseName;
            int index = 1;

            while (group.sprites.Any(sprite => sprite != null && sprite.spriteName == candidate))
            {
                candidate = $"{baseName} {index}";
                index++;
            }

            return candidate;
        }

        private bool TryCommitCharacterRename(int groupIndex, CharacterGroup group)
        {
            if (group == null || database == null) return false;

            string oldName = group.characterName;

            if (!_characterRenameDrafts.TryGetValue(groupIndex, out string newName)) newName = oldName;

            newName = newName?.Trim() ?? "";
            if (string.IsNullOrEmpty(newName))
            {
                EditorUtility.DisplayDialog("Error", "Character name cannot be empty.", "OK");
                return false;
            }

            if (newName == oldName)
            {
                _characterRenameIndices.Remove(groupIndex);
                _characterRenameDrafts.Remove(groupIndex);
                return true;
            }

            if (database.CharacterGroups.Any(g => g != null && g != group && g.characterName == newName))
            {
                EditorUtility.DisplayDialog("Error", $"Character '{newName}' already exists.", "OK");
                return false;
            }

            Undo.RecordObject(database, "Rename Character");

            if (database != null) Undo.RecordObject(database, "Rename Character References");

            group.characterName = newName;
            int updatedReferences = 0;

            if (database != null) updatedReferences = DialogReferenceRename.RenameCharacterActorInDialogs(database, oldName, newName);

            if (characterGroupFoldouts.TryGetValue(oldName, out bool foldoutOpen))
            {
                characterGroupFoldouts.Remove(oldName);
                characterGroupFoldouts[newName] = foldoutOpen;
            }

            _characterRenameIndices.Remove(groupIndex);
            _characterRenameDrafts.Remove(groupIndex);
            SaveDialogRegistry("Rename character");

            if (updatedReferences > 0)
            {
                SaveRegistryDialogGraphPatch(
                    "Rename character - update dialog references",
                    () => DialogReferenceRename.RenameCharacterActorInDialogs(database, oldName, newName));
            }

            InvalidateReferenceIndex();

            return true;
        }

        private bool TryCommitSpriteRename(int groupIndex, int spriteIndex, CharacterGroup group, CharacterSprite sprite)
        {
            if (group == null || sprite == null || database == null) return false;

            string renameKey = BuildSpriteRenameKey(groupIndex, spriteIndex);
            string oldName = sprite.spriteName;

            if (!_spriteRenameDrafts.TryGetValue(renameKey, out string newName)) newName = oldName;

            newName = newName?.Trim() ?? "";

            if (string.IsNullOrEmpty(newName))
            {
                EditorUtility.DisplayDialog("Error", "Emotion name cannot be empty.", "OK");
                return false;
            }

            if (newName == oldName)
            {
                _spriteRenameKeys.Remove(renameKey);
                _spriteRenameDrafts.Remove(renameKey);
                return true;
            }

            if (group.sprites.Any(s => s != null && s != sprite && s.spriteName == newName))
            {
                EditorUtility.DisplayDialog("Error", $"Emotion '{newName}' already exists for this character.", "OK");
                return false;
            }

            Undo.RecordObject(database, "Rename Sprite");

            if (database != null) Undo.RecordObject(database, "Rename Sprite References");

            sprite.spriteName = newName;
            int updatedReferences = 0;

            if (database != null) updatedReferences = DialogReferenceRename.RenameSpriteInDialogs(database, group.characterName, oldName, newName);

            _spriteRenameKeys.Remove(renameKey);
            _spriteRenameDrafts.Remove(renameKey);
            SaveDialogRegistry("Rename sprite");

            if (updatedReferences > 0)
            {
                SaveRegistryDialogGraphPatch(
                    "Rename sprite - update dialog references",
                    () => DialogReferenceRename.RenameSpriteInDialogs(database, group.characterName, oldName, newName));
            }

            InvalidateReferenceIndex();
            return true;
        }

        private bool TryRemoveCharacterFromRegistry(string characterName)
        {
            if (string.IsNullOrEmpty(characterName)) return false;

            DialogReferenceIndex referenceIndex = GetLiveDialogReferenceIndex();
            IReadOnlyList<DialogReferenceUsage> usages = referenceIndex.GetCharacterActionUsages(characterName);
            IReadOnlyList<DialogReferenceUsage> savedUsages = _savedDialogReferenceIndex.GetCharacterActionUsages(characterName);

            string message;

            if (usages.Count > 0) message = BuildRegistryDeleteUsageMessage(usages, savedUsages, "Character (sprites) will be removed from the registry. Sprite actions in dialogs will be marked as missing. Name colors are not affected.");
            else message = $"Delete character '{characterName}'?";

            if (!EditorUtility.DisplayDialog("Delete character", message, "Delete", "Cancel")) return false;
            if (usages.Count > 0 && database != null)
            {
                Undo.RecordObject(database, "Mark Missing Sprite Character Actions");
                DialogReferenceCleanup.MarkCharacterActionsMissingInDialogs(database, characterName);
                SaveRegistryDialogGraphPatch(
                    "Delete character - mark missing sprite actions",
                    () => DialogReferenceCleanup.MarkCharacterActionsMissingInDialogs(database, characterName));
            }

            return true;
        }

        private bool TryRemoveSpriteFromRegistry(string characterName, string emotionName)
        {
            if (string.IsNullOrEmpty(characterName) || string.IsNullOrEmpty(emotionName)) return false;

            DialogReferenceIndex referenceIndex = GetLiveDialogReferenceIndex();
            IReadOnlyList<DialogReferenceUsage> usages = referenceIndex.GetSpriteUsages(characterName, emotionName);
            IReadOnlyList<DialogReferenceUsage> savedUsages = _savedDialogReferenceIndex.GetSpriteUsages(characterName, emotionName);
            string message;

            if (usages.Count > 0) message = BuildRegistryDeleteUsageMessage(usages, savedUsages, "Sprite will be removed from the registry, and references in dialogs will be marked as missing.");
            else message = $"Delete sprite '{characterName}/{emotionName}'?";

            if (!EditorUtility.DisplayDialog("Delete sprite", message, "Delete", "Cancel")) return false;

            if (usages.Count > 0 && database != null)
            {
                Undo.RecordObject(database, "Mark Missing Sprite References");
                DialogReferenceCleanup.MarkSpriteMissingInDialogs(database, characterName, emotionName);
                SaveRegistryDialogGraphPatch(
                    "Delete sprite - mark missing dialog references",
                    () => DialogReferenceCleanup.MarkSpriteMissingInDialogs(database, characterName, emotionName));
            }

            return true;
        }

        #endregion Character sprite editor
    }
}
#endif
