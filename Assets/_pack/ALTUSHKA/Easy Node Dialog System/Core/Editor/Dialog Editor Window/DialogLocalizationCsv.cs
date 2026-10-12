#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ALTUSHKA.EasyNodeDialogSystem.Editor
{
    /// <summary>
    /// CSV localization for Line and Choice texts.
    /// Key format (opaque to translators):
    ///   {nodeId}.line
    ///   {nodeId}.choice.{index}
    /// Columns: Key, then one column per language code from the database.
    /// </summary>
    internal static class DialogLocalizationCsv
    {
        private const string KEY_COLUMN = "Key";
        private const string LINE_SUFFIX = ".line";
        private const string CHOICE_INFIX = ".choice.";
        private const char CSV_SEPARATOR = ',';
        private const char CSV_QUOTE = '"';

        public static string BuildCsv(DialogDatabase database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            List<string> languages = GetLanguageColumns(database);
            StringBuilder sb = new StringBuilder();
            sb.Append('\uFEFF');
            AppendCsvRow(sb, BuildHeader(languages));

            if (database.Dialogs == null) return sb.ToString();

            for (int d = 0; d < database.Dialogs.Count; d++)
            {
                DialogEntry dialog = database.Dialogs[d];
                if (dialog?.nodes == null) continue;

                for (int n = 0; n < dialog.nodes.Count; n++)
                {
                    DialogNode node = dialog.nodes[n];
                    if (node == null || string.IsNullOrEmpty(node.nodeId)) continue;

                    if (node.NodeType == NodeType.Line && node.LineData != null)
                        AppendTextRow(sb, node.nodeId + LINE_SUFFIX, node.LineData.DialogsText, languages);

                    if (node.NodeType != NodeType.Choice || node.ChoiceData == null) continue;

                    for (int c = 0; c < node.ChoiceData.Count; c++)
                    {
                        ChoiceNode choice = node.ChoiceData[c];
                        if (choice == null) continue;
                        AppendTextRow(sb, node.nodeId + CHOICE_INFIX + c.ToString(CultureInfo.InvariantCulture), choice.Translations, languages);
                    }
                }
            }

            return sb.ToString();
        }

        public static ImportResult ApplyCsv(DialogDatabase database, string csvText)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (string.IsNullOrEmpty(csvText)) return ImportResult.EmptyCsv();

            List<string> rows = SplitCsvRows(csvText);
            if (rows.Count == 0) return ImportResult.EmptyCsv();

            List<string> header = ParseCsvLine(rows[0]);
            if (header.Count < 2 || !string.Equals(header[0], KEY_COLUMN, StringComparison.OrdinalIgnoreCase))
                return ImportResult.Fail("First column must be \"" + KEY_COLUMN + "\".");

            List<string> languages = new List<string>();
            for (int i = 1; i < header.Count; i++)
            {
                string lang = (header[i] ?? "").Trim();
                if (string.IsNullOrEmpty(lang)) continue;
                languages.Add(lang);
            }

            if (languages.Count == 0) return ImportResult.Fail("No language columns found.");

            Dictionary<string, LocalizationSlot> slots = BuildSlotIndex(database);
            int updated = 0;
            int skippedUnknown = 0;
            int skippedMalformed = 0;
            List<string> unknownKeys = new List<string>();

            for (int r = 1; r < rows.Count; r++)
            {
                string rowText = rows[r];
                if (string.IsNullOrWhiteSpace(rowText)) continue;

                List<string> cells = ParseCsvLine(rowText);
                if (cells.Count == 0) continue;

                string key = (cells[0] ?? "").Trim();
                if (string.IsNullOrEmpty(key)) continue;

                if (!TryParseKey(key, out string nodeId, out bool isLine, out int choiceIndex))
                {
                    skippedMalformed++;
                    continue;
                }

                string slotKey = BuildSlotLookupKey(nodeId, isLine, choiceIndex);
                if (!slots.TryGetValue(slotKey, out LocalizationSlot slot))
                {
                    skippedUnknown++;
                    if (unknownKeys.Count < 20) unknownKeys.Add(key);
                    continue;
                }

                bool rowChanged = false;
                for (int li = 0; li < languages.Count; li++)
                {
                    string language = languages[li];
                    int cellIndex = li + 1;
                    string newText = "";
                    if (cellIndex < cells.Count) newText = cells[cellIndex] ?? "";

                    if (SetLocalizedText(slot.Texts, language, newText)) rowChanged = true;
                }

                if (rowChanged) updated++;
            }

            return new ImportResult(updated, skippedUnknown, skippedMalformed, unknownKeys);
        }

        public readonly struct CsvMissingLanguageColumn
        {
            public readonly string LanguageCode;
            public readonly int RowCountWithText;

            public CsvMissingLanguageColumn(string languageCode, int rowCountWithText)
            {
                LanguageCode = languageCode;
                RowCountWithText = rowCountWithText;
            }
        }

        public static List<CsvMissingLanguageColumn> GetMissingLanguageColumns(string csvText, DialogDatabase database)
        {
            List<CsvMissingLanguageColumn> missing = new List<CsvMissingLanguageColumn>();
            if (string.IsNullOrEmpty(csvText) || database == null) return missing;

            List<string> rows = SplitCsvRows(csvText);
            if (rows.Count == 0) return missing;

            List<string> header = ParseCsvLine(rows[0]);
            if (header.Count < 2 || !string.Equals(header[0], KEY_COLUMN, StringComparison.OrdinalIgnoreCase)) return missing;

            List<string> csvLanguages = new List<string>();
            for (int i = 1; i < header.Count; i++)
            {
                string lang = (header[i] ?? "").Trim();
                if (string.IsNullOrEmpty(lang)) continue;
                csvLanguages.Add(lang);
            }

            for (int li = 0; li < csvLanguages.Count; li++)
            {
                string language = csvLanguages[li];
                if (IsListedLanguage(database, language)) continue;

                int rowCountWithText = CountCsvRowsWithLanguageText(rows, li + 1);
                missing.Add(new CsvMissingLanguageColumn(language, rowCountWithText));
            }

            return missing;
        }

        public static string BuildMissingLanguagesImportPromptMessage(IReadOnlyList<CsvMissingLanguageColumn> missingLanguages)
        {
            if (missingLanguages == null || missingLanguages.Count == 0) return "";

            if (missingLanguages.Count == 1)
            {
                CsvMissingLanguageColumn column = missingLanguages[0];
                return "CSV contains \"" + column.LanguageCode + "\" (" + column.RowCountWithText + " rows) not in Available languages. Add to Available languages?";
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("CSV contains languages not in Available languages:").Append('\n').Append('\n');

            for (int i = 0; i < missingLanguages.Count; i++)
            {
                CsvMissingLanguageColumn column = missingLanguages[i];
                sb.Append("- ").Append(column.LanguageCode).Append(" (").Append(column.RowCountWithText).Append(" rows)").Append('\n');
            }

            sb.Append('\n').Append("Add them to Available languages?");
            return sb.ToString();
        }

        public static void AddLanguagesToAvailableList(DialogDatabase database, IReadOnlyList<CsvMissingLanguageColumn> missingLanguages)
        {
            if (database == null || missingLanguages == null || missingLanguages.Count == 0) return;

            if (database.AvailableLanguages == null) database.AvailableLanguages = new List<string>();

            for (int i = 0; i < missingLanguages.Count; i++)
            {
                string languageCode = missingLanguages[i].LanguageCode;
                if (string.IsNullOrEmpty(languageCode)) continue;
                if (database.AvailableLanguages.Contains(languageCode)) continue;

                database.AvailableLanguages.Add(languageCode);
            }
        }

        private static bool IsListedLanguage(DialogDatabase database, string languageCode)
        {
            if (database?.AvailableLanguages == null) return false;

            return database.AvailableLanguages.Contains(languageCode);
        }

        private static int CountCsvRowsWithLanguageText(List<string> rows, int languageCellIndex)
        {
            int count = 0;

            for (int r = 1; r < rows.Count; r++)
            {
                string rowText = rows[r];

                if (string.IsNullOrWhiteSpace(rowText)) continue;

                List<string> cells = ParseCsvLine(rowText);

                if (cells.Count <= languageCellIndex) continue;

                string text = cells[languageCellIndex] ?? "";

                if (string.IsNullOrWhiteSpace(text)) continue;

                count++;
            }

            return count;
        }

        private static List<string> GetLanguageColumns(DialogDatabase database)
        {
            List<string> languages = new List<string>();
            if (database.AvailableLanguages == null) return languages;

            for (int i = 0; i < database.AvailableLanguages.Count; i++)
            {
                string lang = database.AvailableLanguages[i];
                if (string.IsNullOrEmpty(lang)) continue;
                if (languages.Contains(lang)) continue;
                languages.Add(lang);
            }

            return languages;
        }

        private static List<string> BuildHeader(List<string> languages)
        {
            List<string> header = new List<string>(languages.Count + 1) { KEY_COLUMN };
            for (int i = 0; i < languages.Count; i++)
            {
                header.Add(languages[i]);
            }

            return header;
        }

        private static void AppendTextRow(StringBuilder sb, string key, List<LineText> texts, List<string> languages)
        {
            List<string> cells = new List<string>(languages.Count + 1) { key };
            for (int i = 0; i < languages.Count; i++)
            {
                cells.Add(GetTextForLanguage(texts, languages[i]));
            }

            AppendCsvRow(sb, cells);
        }

        private static string GetTextForLanguage(List<LineText> texts, string language)
        {
            if (texts == null) return "";

            for (int i = 0; i < texts.Count; i++)
            {
                LineText entry = texts[i];
                if (entry == null) continue;
                if (entry.LanguageCode != language) continue;
                if (entry.Text == null) return "";
                return entry.Text;
            }

            return "";
        }

        private static bool SetLocalizedText(List<LineText> texts, string language, string newText)
        {
            if (texts == null) return false;

            for (int i = 0; i < texts.Count; i++)
            {
                LineText entry = texts[i];
                if (entry == null) continue;
                if (entry.LanguageCode != language) continue;

                string current = entry.Text ?? "";
                if (current == newText) return false;
                entry.Text = newText;
                return true;
            }

            texts.Add(new LineText
            {
                LanguageCode = language,
                Text = newText,
                SayerName = ""
            });
            return true;
        }

        private static Dictionary<string, LocalizationSlot> BuildSlotIndex(DialogDatabase database)
        {
            Dictionary<string, LocalizationSlot> slots = new Dictionary<string, LocalizationSlot>();
            if (database.Dialogs == null) return slots;

            for (int d = 0; d < database.Dialogs.Count; d++)
            {
                DialogEntry dialog = database.Dialogs[d];
                if (dialog?.nodes == null) continue;

                for (int n = 0; n < dialog.nodes.Count; n++)
                {
                    DialogNode node = dialog.nodes[n];
                    if (node == null || string.IsNullOrEmpty(node.nodeId)) continue;

                    if (node.NodeType == NodeType.Line && node.LineData != null)
                    {
                        if (node.LineData.DialogsText == null) node.LineData.DialogsText = new List<LineText>();
                        slots[BuildSlotLookupKey(node.nodeId, true, 0)] = new LocalizationSlot(node.LineData.DialogsText);
                    }

                    if (node.NodeType != NodeType.Choice || node.ChoiceData == null) continue;

                    for (int c = 0; c < node.ChoiceData.Count; c++)
                    {
                        ChoiceNode choice = node.ChoiceData[c];
                        if (choice == null) continue;
                        if (choice.Translations == null) choice.Translations = new List<LineText>();
                        slots[BuildSlotLookupKey(node.nodeId, false, c)] = new LocalizationSlot(choice.Translations);
                    }
                }
            }

            return slots;
        }

        private static string BuildSlotLookupKey(string nodeId, bool isLine, int choiceIndex)
        {
            if (isLine) return nodeId + LINE_SUFFIX;
            return nodeId + CHOICE_INFIX + choiceIndex.ToString(CultureInfo.InvariantCulture);
        }

        private static bool TryParseKey(string key, out string nodeId, out bool isLine, out int choiceIndex)
        {
            nodeId = null;
            isLine = false;
            choiceIndex = 0;

            if (key.EndsWith(LINE_SUFFIX, StringComparison.Ordinal))
            {
                nodeId = key.Substring(0, key.Length - LINE_SUFFIX.Length);
                if (string.IsNullOrEmpty(nodeId)) return false;
                isLine = true;
                return true;
            }

            int choicePos = key.LastIndexOf(CHOICE_INFIX, StringComparison.Ordinal);
            if (choicePos <= 0) return false;

            nodeId = key.Substring(0, choicePos);
            string indexPart = key.Substring(choicePos + CHOICE_INFIX.Length);
            if (string.IsNullOrEmpty(nodeId) || string.IsNullOrEmpty(indexPart)) return false;
            if (!int.TryParse(indexPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out choiceIndex)) return false;
            if (choiceIndex < 0) return false;

            return true;
        }

        private static void AppendCsvRow(StringBuilder sb, List<string> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                if (i > 0) sb.Append(CSV_SEPARATOR);
                sb.Append(EscapeCsvCell(cells[i]));
            }

            sb.Append("\r\n");
        }

        private static string EscapeCsvCell(string value)
        {
            if (value == null) return "";

            bool needsQuotes = value.IndexOf(CSV_SEPARATOR) >= 0
                || value.IndexOf(CSV_QUOTE) >= 0
                || value.IndexOf('\n') >= 0
                || value.IndexOf('\r') >= 0;

            if (!needsQuotes) return value;

            return CSV_QUOTE + value.Replace("\"", "\"\"") + CSV_QUOTE;
        }

        private static List<string> SplitCsvRows(string csvText)
        {
            List<string> rows = new List<string>();
            if (csvText.Length > 0 && csvText[0] == '\uFEFF') csvText = csvText.Substring(1);

            StringBuilder current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < csvText.Length; i++)
            {
                char ch = csvText[i];

                if (ch == CSV_QUOTE)
                {
                    inQuotes = !inQuotes;
                    current.Append(ch);
                    continue;
                }

                if (!inQuotes && (ch == '\n' || ch == '\r'))
                {
                    if (ch == '\r' && i + 1 < csvText.Length && csvText[i + 1] == '\n') i++;
                    rows.Add(current.ToString());
                    current.Length = 0;
                    continue;
                }

                current.Append(ch);
            }

            if (current.Length > 0) rows.Add(current.ToString());
            return rows;
        }

        private static List<string> ParseCsvLine(string line)
        {
            List<string> cells = new List<string>();
            if (line == null) return cells;

            StringBuilder current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];

                if (inQuotes)
                {
                    if (ch == CSV_QUOTE)
                    {
                        if (i + 1 < line.Length && line[i + 1] == CSV_QUOTE)
                        {
                            current.Append(CSV_QUOTE);
                            i++;
                        }
                        else inQuotes = false;
                    }
                    else current.Append(ch);

                    continue;
                }

                if (ch == CSV_QUOTE)
                {
                    inQuotes = true;
                    continue;
                }

                if (ch == CSV_SEPARATOR)
                {
                    cells.Add(current.ToString());
                    current.Length = 0;
                    continue;
                }

                current.Append(ch);
            }

            cells.Add(current.ToString());
            return cells;
        }

        private readonly struct LocalizationSlot
        {
            public readonly List<LineText> Texts;

            public LocalizationSlot(List<LineText> texts)
            {
                Texts = texts;
            }
        }

        public readonly struct ImportResult
        {
            public readonly bool Ok;
            public readonly string Error;
            public readonly int UpdatedRows;
            public readonly int UnknownKeys;
            public readonly int MalformedKeys;
            public readonly IReadOnlyList<string> SampleUnknownKeys;

            public ImportResult(int updatedRows, int unknownKeys, int malformedKeys, List<string> sampleUnknownKeys)
            {
                Ok = true;
                Error = null;
                UpdatedRows = updatedRows;
                UnknownKeys = unknownKeys;
                MalformedKeys = malformedKeys;
                if (sampleUnknownKeys == null) SampleUnknownKeys = Array.Empty<string>();
                else SampleUnknownKeys = sampleUnknownKeys;
            }

            private ImportResult(string error)
            {
                Ok = false;
                Error = error;
                UpdatedRows = 0;
                UnknownKeys = 0;
                MalformedKeys = 0;
                SampleUnknownKeys = Array.Empty<string>();
            }

            public static ImportResult Fail(string error) => new ImportResult(error);

            public static ImportResult EmptyCsv() => Fail("CSV is empty.");

            public string BuildMessage()
            {
                if (!Ok) return Error;

                StringBuilder sb = new StringBuilder();
                sb.Append("Updated rows: ").Append(UpdatedRows).Append('.');
                if (UnknownKeys > 0) sb.Append(" Unknown keys: ").Append(UnknownKeys).Append('.');
                if (MalformedKeys > 0) sb.Append(" Malformed keys: ").Append(MalformedKeys).Append('.');

                if (SampleUnknownKeys != null && SampleUnknownKeys.Count > 0)
                {
                    sb.Append("\n\nUnknown key samples:\n");
                    for (int i = 0; i < SampleUnknownKeys.Count; i++)
                    {
                        sb.Append("- ").Append(SampleUnknownKeys[i]).Append('\n');
                    }
                }

                return sb.ToString();
            }
        }

        public static void WriteCsvFile(DialogDatabase database, string path)
        {
            string csv = BuildCsv(database);
            File.WriteAllText(path, csv, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        }

        public static ImportResult ReadAndApplyCsvFile(DialogDatabase database, string path)
        {
            string csv = File.ReadAllText(path, Encoding.UTF8);
            return ApplyCsv(database, csv);
        }
    }
}
#endif
