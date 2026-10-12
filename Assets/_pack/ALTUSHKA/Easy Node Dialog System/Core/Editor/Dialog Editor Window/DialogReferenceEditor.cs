#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;

namespace ALTUSHKA.EasyNodeDialogSystem.Editor
{
    public static class DialogReferenceMessages
    {
        public const string MISSING_VARIABLE_SLOT = "Variable was removed";
        public const string MISSING_CHARACTER_SLOT = "Character was removed.";
        public const string MISSING_SPRITE_SLOT = "Sprite was removed.";
        public const string MISSING_DIALOG_SLOT = "Dialog was removed";

        public static string FormatMissingVariableLabel(string previousName)
        {   
            if (string.IsNullOrEmpty(previousName)) return MISSING_VARIABLE_SLOT;
            else return $"Variable \"{previousName}\" was removed.";
        }

        public static string FormatMissingCharacterLabel(string previousName)
        {       
            if (string.IsNullOrEmpty(previousName)) return MISSING_CHARACTER_SLOT;
            else return $"Character \"{previousName}\" was removed.";    
        }

        public static string FormatMissingSpriteLabel(string characterName, string emotionName)
        {
            if (string.IsNullOrEmpty(characterName) && string.IsNullOrEmpty(emotionName)) return MISSING_SPRITE_SLOT;
            return $"Sprite \"{emotionName}\" of character \"{characterName}\" was removed.";
        }

        public static string FormatMissingDialogLabel(string previousName)
        {
            if (string.IsNullOrEmpty(previousName)) return MISSING_DIALOG_SLOT;
            return $"Dialog \"{previousName}\" was removed.";
        }
    }

    public static class DialogReferenceEditorUtility
    {
        public static void MarkVariableMissing(DialogCondition condition, string previousName)
        {
            if (condition == null) return;

            condition.ReferenceState = DialogReferenceState.Missing;
            condition.MissingReferenceLabel = previousName ?? condition.VariableName ?? "";
            condition.VariableName = "";
        }

        public static void MarkVariableMissing(ChoiceCondition condition, string previousName)
        {
            if (condition == null) return;

            condition.ReferenceState = DialogReferenceState.Missing;
            condition.MissingReferenceLabel = previousName ?? condition.VariableName ?? "";
            condition.VariableName = "";
        }

        public static void MarkVariableMissing(ChoiceConsequence consequence, string previousName)
        {
            if (consequence == null) return;

            consequence.ReferenceState = DialogReferenceState.Missing;
            consequence.MissingReferenceLabel = previousName ?? consequence.VariableName ?? "";
            consequence.VariableName = "";
        }

        public static void MarkCharacterMissing(LineNode line, string previousName)
        {
            if (line == null) return;

            line.CharacterReferenceState = DialogReferenceState.Missing;
            line.MissingCharacterReferenceLabel = previousName ?? line.CharacterName ?? "";
            line.CharacterName = "";
        }

        public static void MarkSpriteActionMissing(CharacterActionData action, string characterName, string emotionName)
        {
            if (action == null) return;

            action.ReferenceState = DialogReferenceState.Missing;
            action.MissingReferenceLabel = DialogReferenceMessages.FormatMissingSpriteLabel(characterName, emotionName);
            action.CharacterName = "";
            action.EmotionName = "";
        }

        public static void MarkDialogMissing(RedirectNode redirect, string previousName)
        {
            if (redirect == null) return;

            redirect.DialogReferenceState = DialogReferenceState.Missing;
            redirect.MissingDialogReferenceLabel = previousName ?? redirect.TargetDialogName ?? "";
            redirect.TargetDialogName = "";
        }

        public static void RestoreVariableReference(DialogCondition condition, string variableName)
        {
            if (condition == null) return;

            condition.ReferenceState = DialogReferenceState.Valid;
            condition.MissingReferenceLabel = "";
            condition.VariableName = variableName;
        }

        public static void RestoreVariableReference(ChoiceCondition condition, string variableName)
        {
            if (condition == null) return;

            condition.ReferenceState = DialogReferenceState.Valid;
            condition.MissingReferenceLabel = "";
            condition.VariableName = variableName;
        }

        public static void RestoreVariableReference(ChoiceConsequence consequence, string variableName)
        {
            if (consequence == null) return;

            consequence.ReferenceState = DialogReferenceState.Valid;
            consequence.MissingReferenceLabel = "";
            consequence.VariableName = variableName;
        }

        public static void RestoreCharacterReference(LineNode line, string characterName)
        {
            if (line == null) return;

            line.CharacterReferenceState = DialogReferenceState.Valid;
            line.MissingCharacterReferenceLabel = "";
            line.CharacterName = characterName;
        }

        public static void RestoreSpriteAction(CharacterActionData action, string characterName, string emotionName)
        {
            if (action == null) return;

            action.ReferenceState = DialogReferenceState.Valid;
            action.MissingReferenceLabel = "";
            action.CharacterName = characterName;
            action.EmotionName = emotionName;
        }

        public static void RestoreDialogReference(RedirectNode redirect, string dialogName, string category)
        {
            if (redirect == null) return;

            redirect.DialogReferenceState = DialogReferenceState.Valid;
            redirect.MissingDialogReferenceLabel = "";
            redirect.TargetDialogName = dialogName;
            redirect.TargetCategory = category ?? "";
        }

        public static void ResetVariableReferenceState(DialogCondition condition)
        {
            if (condition == null) return;

            condition.ReferenceState = DialogReferenceState.Valid;
            condition.MissingReferenceLabel = "";
        }

        public static void ResetVariableReferenceState(ChoiceCondition condition)
        {
            if (condition == null) return;

            condition.ReferenceState = DialogReferenceState.Valid;
            condition.MissingReferenceLabel = "";
        }

        public static void ResetVariableReferenceState(ChoiceConsequence consequence)
        {
            if (consequence == null) return;

            consequence.ReferenceState = DialogReferenceState.Valid;
            consequence.MissingReferenceLabel = "";
        }
    }

    public enum DialogReferenceUsageKind
    {
        DialogStartCondition,
        NodeCondition,
        ChoiceCondition,
        ChoiceConsequence,
        LineCharacterName,
        CharacterAction,
        RedirectTarget
    }

    public readonly struct DialogReferenceUsage
    {
        public DialogReferenceUsage(string dialogName, DialogReferenceUsageKind kind, string nodeId, string detail)
        {
            DialogName = dialogName;
            Kind = kind;
            NodeId = nodeId;
            Detail = detail;
        }

        public string DialogName { get; }
        public DialogReferenceUsageKind Kind { get; }
        public string NodeId { get; }
        public string Detail { get; }

        public DialogReferenceUsageKey ToKey()
        {
            return new DialogReferenceUsageKey(DialogName, Kind, NodeId, Detail);
        }
    }

    public readonly struct DialogReferenceUsageKey : IEquatable<DialogReferenceUsageKey>
    {
        public DialogReferenceUsageKey(string dialogName, DialogReferenceUsageKind kind, string nodeId, string detail)
        {
            DialogName = dialogName ?? "";
            Kind = kind;
            NodeId = nodeId ?? "";
            Detail = detail ?? "";
        }

        public string DialogName { get; }
        public DialogReferenceUsageKind Kind { get; }
        public string NodeId { get; }
        public string Detail { get; }

        public bool Equals(DialogReferenceUsageKey other)
        {
            return DialogName == other.DialogName
                && Kind == other.Kind
                && NodeId == other.NodeId
                && Detail == other.Detail;
        }

        public override bool Equals(object obj)
        {
            if (obj is DialogReferenceUsageKey other) return Equals(other);
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = DialogName.GetHashCode();
                hash = (hash * 397) ^ (int)Kind;
                hash = (hash * 397) ^ NodeId.GetHashCode();
                hash = (hash * 397) ^ Detail.GetHashCode();
                return hash;
            }
        }
    }

    public sealed class DialogReferenceIndex
    {
        private readonly Dictionary<string, List<DialogReferenceUsage>> _variableUsages = new Dictionary<string, List<DialogReferenceUsage>>();
        private readonly Dictionary<string, List<DialogReferenceUsage>> _characterUsages = new Dictionary<string, List<DialogReferenceUsage>>();
        private readonly Dictionary<string, List<DialogReferenceUsage>> _spriteUsages = new Dictionary<string, List<DialogReferenceUsage>>();
        private readonly Dictionary<string, List<DialogReferenceUsage>> _redirectTargetUsages = new Dictionary<string, List<DialogReferenceUsage>>();

        public void Rebuild(DialogDatabase database)
        {
            _variableUsages.Clear();
            _characterUsages.Clear();
            _spriteUsages.Clear();
            _redirectTargetUsages.Clear();

            if (database?.Dialogs == null) return;

            foreach (DialogEntry dialog in database.Dialogs)
            {
                if (dialog == null || string.IsNullOrEmpty(dialog.dialogName)) continue;

                ScanDialogConditions(dialog.dialogName, null, dialog.conditions, DialogReferenceUsageKind.DialogStartCondition, "Dialog start condition");

                if (dialog.nodes == null) continue;

                foreach (DialogNode node in dialog.nodes)
                {
                    if (node == null) continue;

                    string nodeLabel = DescribeNode(node);

                    if (node.NodeType == NodeType.Condition && node.ConditionData?.DialogConditions != null) ScanDialogConditions(dialog.dialogName, node.nodeId, node.ConditionData.DialogConditions, DialogReferenceUsageKind.NodeCondition, nodeLabel);

                    if (node.NodeType == NodeType.Redirect && node.RedirectData != null && !string.IsNullOrEmpty(node.RedirectData.TargetDialogName))
                        AddRedirectTargetUsage(node.RedirectData.TargetDialogName, dialog.dialogName, node.nodeId, $"{nodeLabel}: redirect");

                    if (node.NodeType == NodeType.Line && node.LineData != null)
                    {
                        if (!string.IsNullOrEmpty(node.LineData.CharacterName)) AddCharacterUsage(node.LineData.CharacterName, dialog.dialogName, DialogReferenceUsageKind.LineCharacterName, node.nodeId, $"{nodeLabel}: line character");

                        if (node.LineData.CharacterActions == null) continue;

                        for (int i = 0; i < node.LineData.CharacterActions.Count; i++)
                        {
                            CharacterActionData action = node.LineData.CharacterActions[i];
                            if (action == null || action.Action != CharacterAction.Show) continue;
                            if (string.IsNullOrEmpty(action.CharacterName)) continue;

                            AddCharacterUsage(action.CharacterName, dialog.dialogName, DialogReferenceUsageKind.CharacterAction, node.nodeId, $"{nodeLabel}: action #{i + 1}");

                            if (!string.IsNullOrEmpty(action.EmotionName)) AddSpriteUsage(action.CharacterName, action.EmotionName, dialog.dialogName, node.nodeId, $"{nodeLabel}: sprite #{i + 1}");
                        }
                    }

                    if (node.NodeType != NodeType.Choice || node.ChoiceData == null) continue;

                    for (int choiceIndex = 0; choiceIndex < node.ChoiceData.Count; choiceIndex++)
                    {
                        ChoiceNode choice = node.ChoiceData[choiceIndex];
                        if (choice == null) continue;

                        string choiceLabel = $"{nodeLabel}, choice {choiceIndex + 1}";

                        if (choice.ChoiceCondition != null)
                        {
                            for (int i = 0; i < choice.ChoiceCondition.Count; i++)
                            {
                                ChoiceCondition condition = choice.ChoiceCondition[i];
                                if (condition == null || string.IsNullOrEmpty(condition.VariableName)) continue;

                                AddVariableUsage(condition.VariableName, dialog.dialogName, DialogReferenceUsageKind.ChoiceCondition, node.nodeId, $"{choiceLabel}, condition {i + 1}");
                            }
                        }

                        if (choice.ChoiceConsequence == null) continue;

                        for (int i = 0; i < choice.ChoiceConsequence.Count; i++)
                        {
                            ChoiceConsequence consequence = choice.ChoiceConsequence[i];
                            if (consequence == null || string.IsNullOrEmpty(consequence.VariableName)) continue;

                            AddVariableUsage(consequence.VariableName, dialog.dialogName, DialogReferenceUsageKind.ChoiceConsequence, node.nodeId, $"{choiceLabel}, consequence {i + 1}");
                        }
                    }
                }
            }
        }

        public IReadOnlyList<DialogReferenceUsage> GetVariableUsages(string variableName)
        {
            if (string.IsNullOrEmpty(variableName)) return Array.Empty<DialogReferenceUsage>();
            if (!_variableUsages.TryGetValue(variableName, out List<DialogReferenceUsage> usages)) return Array.Empty<DialogReferenceUsage>();

            return usages;
        }

        public IReadOnlyList<DialogReferenceUsage> GetCharacterUsages(string characterName)
        {
            if (string.IsNullOrEmpty(characterName)) return Array.Empty<DialogReferenceUsage>();
            if (!_characterUsages.TryGetValue(characterName, out List<DialogReferenceUsage> usages)) return Array.Empty<DialogReferenceUsage>();

            return usages;
        }

        public IReadOnlyList<DialogReferenceUsage> GetLineCharacterNameUsages(string characterName)
        {
            return FilterCharacterUsages(characterName, DialogReferenceUsageKind.LineCharacterName);
        }

        public IReadOnlyList<DialogReferenceUsage> GetCharacterActionUsages(string characterName)
        {
            return FilterCharacterUsages(characterName, DialogReferenceUsageKind.CharacterAction);
        }

        private IReadOnlyList<DialogReferenceUsage> FilterCharacterUsages(string characterName, DialogReferenceUsageKind kind)
        {
            IReadOnlyList<DialogReferenceUsage> usages = GetCharacterUsages(characterName);
            if (usages.Count == 0) return usages;

            List<DialogReferenceUsage> filtered = new List<DialogReferenceUsage>(usages.Count);
            for (int i = 0; i < usages.Count; i++)
            {
                DialogReferenceUsage usage = usages[i];
                if (usage.Kind != kind) continue;
                filtered.Add(usage);
            }

            return filtered;
        }

        public IReadOnlyList<DialogReferenceUsage> GetSpriteUsages(string characterName, string emotionName)
        {
            if (string.IsNullOrEmpty(characterName) || string.IsNullOrEmpty(emotionName)) return Array.Empty<DialogReferenceUsage>();
            if (!_spriteUsages.TryGetValue(SpriteKey(characterName, emotionName), out List<DialogReferenceUsage> usages)) return Array.Empty<DialogReferenceUsage>();

            return usages;
        }

        public IReadOnlyList<DialogReferenceUsage> GetRedirectTargetUsages(string dialogName)
        {
            if (string.IsNullOrEmpty(dialogName)) return Array.Empty<DialogReferenceUsage>();
            if (!_redirectTargetUsages.TryGetValue(dialogName, out List<DialogReferenceUsage> usages)) return Array.Empty<DialogReferenceUsage>();

            return usages;
        }

        public static string BuildUsageSummary(IReadOnlyList<DialogReferenceUsage> liveUsages, IReadOnlyList<DialogReferenceUsage> savedUsages, int maxLines = 8)
        {
            if (liveUsages == null || liveUsages.Count == 0) return "Not used in any dialogs.";

            HashSet<DialogReferenceUsageKey> savedKeys = new HashSet<DialogReferenceUsageKey>();

            if (savedUsages != null)
            {
                foreach (DialogReferenceUsage usage in savedUsages)
                {
                    savedKeys.Add(usage.ToKey()); 
                }

            }

            int savedUsageCount = 0;
            int unsavedUsageCount = 0;
            HashSet<string> savedDialogNames = new HashSet<string>();
            HashSet<string> unsavedDialogNames = new HashSet<string>();

            foreach (DialogReferenceUsage usage in liveUsages)
            {
                if (savedKeys.Contains(usage.ToKey()))
                {
                    savedUsageCount++;
                    savedDialogNames.Add(usage.DialogName);
                }
                else
                {
                    unsavedUsageCount++;
                    unsavedDialogNames.Add(usage.DialogName);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Dependencies found!");

            if (savedUsageCount > 0) sb.AppendLine($"({savedUsageCount}) usages in ({savedDialogNames.Count}) saved dialogs.");
            if (unsavedUsageCount > 0) sb.AppendLine($"({unsavedUsageCount}) usages in ({unsavedDialogNames.Count}) unsaved dialogs.");

            int lines = Math.Min(liveUsages.Count, maxLines);

            for (int i = 0; i < lines; i++)
            {
                sb.AppendLine($"- {liveUsages[i].DialogName}: {liveUsages[i].Detail}");
            }

            if (liveUsages.Count > maxLines) sb.AppendLine($"... and {liveUsages.Count - maxLines} more.");

            return sb.ToString().TrimEnd();
        }

        private void ScanDialogConditions(string dialogName, string nodeId, List<DialogCondition> conditions, DialogReferenceUsageKind kind, string detailPrefix)
        {
            if (conditions == null) return;

            for (int i = 0; i < conditions.Count; i++)
            {
                DialogCondition condition = conditions[i];
                if (condition == null || string.IsNullOrEmpty(condition.VariableName)) continue;

                AddVariableUsage(condition.VariableName, dialogName, kind, nodeId, $"{detailPrefix} #{i + 1}");
            }
        }

        private void AddVariableUsage(string variableName, string dialogName, DialogReferenceUsageKind kind, string nodeId, string detail)
        {
            if (!_variableUsages.TryGetValue(variableName, out List<DialogReferenceUsage> list))
            {
                list = new List<DialogReferenceUsage>();
                _variableUsages[variableName] = list;
            }

            list.Add(new DialogReferenceUsage(dialogName, kind, nodeId, detail));
        }

        private void AddCharacterUsage(string characterName, string dialogName, DialogReferenceUsageKind kind, string nodeId, string detail)
        {
            if (!_characterUsages.TryGetValue(characterName, out List<DialogReferenceUsage> list))
            {
                list = new List<DialogReferenceUsage>();
                _characterUsages[characterName] = list;
            }

            list.Add(new DialogReferenceUsage(dialogName, kind, nodeId, detail));
        }

        private void AddSpriteUsage(string characterName, string emotionName, string dialogName, string nodeId, string detail)
        {
            string key = SpriteKey(characterName, emotionName);

            if (!_spriteUsages.TryGetValue(key, out List<DialogReferenceUsage> list))
            {
                list = new List<DialogReferenceUsage>();
                _spriteUsages[key] = list;
            }

            list.Add(new DialogReferenceUsage(dialogName, DialogReferenceUsageKind.CharacterAction, nodeId, detail));
        }

        private void AddRedirectTargetUsage(string targetDialogName, string dialogName, string nodeId, string detail)
        {
            if (!_redirectTargetUsages.TryGetValue(targetDialogName, out List<DialogReferenceUsage> list))
            {
                list = new List<DialogReferenceUsage>();
                _redirectTargetUsages[targetDialogName] = list;
            }

            list.Add(new DialogReferenceUsage(dialogName, DialogReferenceUsageKind.RedirectTarget, nodeId, detail));
        }

        private static string SpriteKey(string characterName, string emotionName)
        {
            return characterName + "\u001f" + emotionName;
        }

        private static string DescribeNode(DialogNode node)
        {
            if (node == null) return "Node";

            string type = node.NodeType switch
            {
                NodeType.Start => "Start",
                NodeType.Line => "Line",
                NodeType.Choice => "Choice",
                NodeType.Condition => "Condition",
                NodeType.Redirect => "Redirect",
                _ => "Node"
            };

            string id = node.nodeId;

            if (!string.IsNullOrEmpty(id) && id.Length >= 8) id = id.Substring(0, 8);
            else if (string.IsNullOrEmpty(id)) id = "?";

            return $"{type} ({id})";
        }
    }

    public static class DialogReferenceCleanup
    {
        public static int MarkVariableMissingInDialogs(DialogDatabase database, string variableName)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(variableName)) return 0;

            int count = 0;

            foreach (DialogEntry dialog in database.Dialogs)
            {
                if (dialog == null) continue;

                count += MarkVariableMissingInConditions(dialog.conditions, variableName);

                if (dialog.nodes == null) continue;

                foreach (DialogNode node in dialog.nodes)
                {
                    if (node == null) continue;

                    if (node.NodeType == NodeType.Condition && node.ConditionData?.DialogConditions != null) count += MarkVariableMissingInConditions(node.ConditionData.DialogConditions, variableName);

                    if (node.NodeType != NodeType.Choice || node.ChoiceData == null) continue;

                    foreach (ChoiceNode choice in node.ChoiceData)
                    {
                        if (choice == null) continue;

                        if (choice.ChoiceCondition != null)
                        {
                            foreach (ChoiceCondition condition in choice.ChoiceCondition)
                            {
                                if (condition == null || condition.ReferenceState == DialogReferenceState.Missing) continue;
                                if (condition.VariableName != variableName) continue;

                                DialogReferenceEditorUtility.MarkVariableMissing(condition, variableName);
                                count++;
                            }
                        }

                        if (choice.ChoiceConsequence == null) continue;

                        foreach (ChoiceConsequence consequence in choice.ChoiceConsequence)
                        {
                            if (consequence == null || consequence.ReferenceState == DialogReferenceState.Missing) continue;
                            if (consequence.VariableName != variableName) continue;

                            DialogReferenceEditorUtility.MarkVariableMissing(consequence, variableName);
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        public static int MarkLineCharacterNameMissingInDialogs(DialogDatabase database, string characterName)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(characterName)) return 0;

            int count = 0;

            foreach (DialogEntry dialog in database.Dialogs)
            {
                if (dialog?.nodes == null) continue;

                foreach (DialogNode node in dialog.nodes)
                {
                    if (node?.NodeType != NodeType.Line || node.LineData == null) continue;

                    LineNode line = node.LineData;

                    if (line.CharacterReferenceState != DialogReferenceState.Missing && line.CharacterName == characterName)
                    {
                        DialogReferenceEditorUtility.MarkCharacterMissing(line, characterName);
                        count++;
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Marks sprite Show/Hide actions missing for a character actor.
        /// Does not touch line speaker / name-color identity (CharacterNameColors).
        /// </summary>
        public static int MarkCharacterActionsMissingInDialogs(DialogDatabase database, string characterName)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(characterName)) return 0;

            int count = 0;

            foreach (DialogEntry dialog in database.Dialogs)
            {
                if (dialog?.nodes == null) continue;

                foreach (DialogNode node in dialog.nodes)
                {
                    if (node?.NodeType != NodeType.Line || node.LineData?.CharacterActions == null) continue;

                    foreach (CharacterActionData action in node.LineData.CharacterActions)
                    {
                        if (action == null || action.ReferenceState == DialogReferenceState.Missing) continue;
                        if (action.CharacterName != characterName) continue;

                        DialogReferenceEditorUtility.MarkSpriteActionMissing(action, characterName, action.EmotionName);
                        count++;
                    }
                }
            }

            return count;
        }

        public static int MarkSpriteMissingInDialogs(DialogDatabase database, string characterName, string emotionName)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(characterName) || string.IsNullOrEmpty(emotionName)) return 0;

            int count = 0;

            foreach (DialogEntry dialog in database.Dialogs)
            {
                if (dialog?.nodes == null) continue;

                foreach (DialogNode node in dialog.nodes)
                {
                    if (node?.NodeType != NodeType.Line || node.LineData?.CharacterActions == null) continue;

                    foreach (CharacterActionData action in node.LineData.CharacterActions)
                    {
                        if (action == null || action.ReferenceState == DialogReferenceState.Missing) continue;
                        if (action.CharacterName != characterName || action.EmotionName != emotionName) continue;

                        DialogReferenceEditorUtility.MarkSpriteActionMissing(action, characterName, emotionName);
                        count++;
                    }
                }
            }

            return count;
        }

        public static int MarkDialogMissingInDialogs(DialogDatabase database, string dialogName)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(dialogName)) return 0;

            int count = 0;

            foreach (DialogEntry dialog in database.Dialogs)
            {
                if (dialog?.nodes == null) continue;

                foreach (DialogNode node in dialog.nodes)
                {
                    if (node?.NodeType != NodeType.Redirect || node.RedirectData == null) continue;

                    RedirectNode redirect = node.RedirectData;

                    if (redirect.DialogReferenceState == DialogReferenceState.Missing) continue;
                    if (redirect.TargetDialogName != dialogName) continue;

                    DialogReferenceEditorUtility.MarkDialogMissing(redirect, dialogName);
                    count++;
                }
            }

            return count;
        }

        private static int MarkVariableMissingInConditions(List<DialogCondition> conditions, string variableName)
        {
            if (conditions == null) return 0;

            int count = 0;

            foreach (DialogCondition condition in conditions)
            {
                if (condition == null || condition.ReferenceState == DialogReferenceState.Missing) continue;
                if (condition.VariableName != variableName) continue;

                DialogReferenceEditorUtility.MarkVariableMissing(condition, variableName);
                count++;
            }

            return count;
        }
    }

    public static class DialogReferenceRename
    {
        public static int RenameVariableInDialogs(DialogDatabase database, string oldName, string newName)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName) || oldName == newName) return 0;

            int count = 0;

            foreach (DialogEntry dialog in database.Dialogs)
            {
                if (dialog == null) continue;

                count += RenameVariableInConditions(dialog.conditions, oldName, newName);

                if (dialog.nodes == null) continue;

                foreach (DialogNode node in dialog.nodes)
                {
                    if (node == null) continue;

                    if (node.NodeType == NodeType.Condition && node.ConditionData?.DialogConditions != null) count += RenameVariableInConditions(node.ConditionData.DialogConditions, oldName, newName);

                    if (node.NodeType != NodeType.Choice || node.ChoiceData == null) continue;

                    foreach (ChoiceNode choice in node.ChoiceData)
                    {
                        if (choice == null) continue;

                        if (choice.ChoiceCondition != null)
                        {
                            foreach (ChoiceCondition condition in choice.ChoiceCondition)
                            {
                                if (condition == null) continue;
                                if (!ShouldRenameVariableReference(condition.VariableName, condition.ReferenceState, condition.MissingReferenceLabel, oldName)) continue;

                                DialogReferenceEditorUtility.RestoreVariableReference(condition, newName);
                                count++;
                            }
                        }

                        if (choice.ChoiceConsequence == null) continue;

                        foreach (ChoiceConsequence consequence in choice.ChoiceConsequence)
                        {
                            if (consequence == null) continue;
                            if (!ShouldRenameVariableReference(consequence.VariableName, consequence.ReferenceState, consequence.MissingReferenceLabel, oldName)) continue;

                            DialogReferenceEditorUtility.RestoreVariableReference(consequence, newName);
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        /// <summary>
        /// Renames character actor on sprite actions only.
        /// Does not rename CharacterNameColors or line speaker names.
        /// </summary>
        public static int RenameCharacterActorInDialogs(DialogDatabase database, string oldName, string newName)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName) || oldName == newName) return 0;

            int count = 0;

            foreach (DialogEntry dialog in database.Dialogs)
            {
                if (dialog?.nodes == null) continue;

                foreach (DialogNode node in dialog.nodes)
                {
                    if (node?.NodeType != NodeType.Line || node.LineData?.CharacterActions == null) continue;

                    foreach (CharacterActionData action in node.LineData.CharacterActions)
                    {
                        if (action == null || action.ReferenceState == DialogReferenceState.Missing) continue;
                        if (action.CharacterName != oldName) continue;

                        action.CharacterName = newName;
                        action.ReferenceState = DialogReferenceState.Valid;
                        action.MissingReferenceLabel = "";
                        count++;
                    }
                }
            }

            return count;
        }

        public static int RenameSpriteInDialogs(DialogDatabase database, string characterName, string oldEmotionName, string newEmotionName)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(characterName) || string.IsNullOrEmpty(oldEmotionName) || string.IsNullOrEmpty(newEmotionName) || oldEmotionName == newEmotionName) return 0;

            int count = 0;

            foreach (DialogEntry dialog in database.Dialogs)
            {
                if (dialog?.nodes == null) continue;

                foreach (DialogNode node in dialog.nodes)
                {
                    if (node?.NodeType != NodeType.Line || node.LineData?.CharacterActions == null) continue;

                    foreach (CharacterActionData action in node.LineData.CharacterActions)
                    {
                        if (action == null) continue;

                        if (action.ReferenceState != DialogReferenceState.Missing && action.CharacterName == characterName && action.EmotionName == oldEmotionName)
                        {
                            action.EmotionName = newEmotionName;
                            action.ReferenceState = DialogReferenceState.Valid;
                            action.MissingReferenceLabel = "";
                            count++;
                        }
                        else if (action.ReferenceState == DialogReferenceState.Missing && action.MissingReferenceLabel == DialogReferenceMessages.FormatMissingSpriteLabel(characterName, oldEmotionName))
                        {
                            DialogReferenceEditorUtility.RestoreSpriteAction(action, characterName, newEmotionName);
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        public static int RenameDialogInRedirectNodes(DialogDatabase database, string oldName, string newName)
        {
            if (database?.Dialogs == null || string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName) || oldName == newName) return 0;

            int count = 0;

            foreach (DialogEntry dialog in database.Dialogs)
            {
                if (dialog?.nodes == null) continue;

                foreach (DialogNode node in dialog.nodes)
                {
                    if (node?.NodeType != NodeType.Redirect || node.RedirectData == null) continue;

                    RedirectNode redirect = node.RedirectData;

                    if (redirect.DialogReferenceState != DialogReferenceState.Missing && redirect.TargetDialogName == oldName)
                    {
                        redirect.TargetDialogName = newName;
                        redirect.DialogReferenceState = DialogReferenceState.Valid;
                        redirect.MissingDialogReferenceLabel = "";
                        count++;
                    }
                    else if (redirect.DialogReferenceState == DialogReferenceState.Missing && redirect.MissingDialogReferenceLabel == oldName)
                    {
                        DialogReferenceEditorUtility.RestoreDialogReference(redirect, newName, redirect.TargetCategory);
                        count++;
                    }
                }
            }

            return count;
        }

        private static int RenameVariableInConditions(List<DialogCondition> conditions, string oldName, string newName)
        {
            if (conditions == null) return 0;

            int count = 0;

            foreach (DialogCondition condition in conditions)
            {
                if (condition == null) continue;
                if (!ShouldRenameVariableReference(condition.VariableName, condition.ReferenceState, condition.MissingReferenceLabel, oldName)) continue;

                DialogReferenceEditorUtility.RestoreVariableReference(condition, newName);
                count++;
            }

            return count;
        }

        private static bool ShouldRenameVariableReference(string variableName, DialogReferenceState referenceState, string missingReferenceLabel, string oldName)
        {
            if (referenceState != DialogReferenceState.Missing && variableName == oldName) return true;
            if (referenceState == DialogReferenceState.Missing && missingReferenceLabel == oldName) return true;

            return false;
        }
    }
}
#endif
