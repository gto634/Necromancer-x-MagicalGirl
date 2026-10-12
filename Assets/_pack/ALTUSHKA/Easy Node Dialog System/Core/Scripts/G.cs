using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ALTUSHKA.EasyNodeDialogSystem
{
    /// <summary> Runtime storage for dialog system variables and save.json. </summary>
    public static class G
    {
        private const int CURRENT_SAVE_VERSION = 1;

        private static readonly Dictionary<string, bool> boolVariables = new Dictionary<string, bool>();
        private static readonly Dictionary<string, int> intVariables = new Dictionary<string, int>();
        private static readonly Dictionary<string, float> floatVariables = new Dictionary<string, float>();
        private static readonly Dictionary<string, string> stringVariables = new Dictionary<string, string>();

        public static void SetBool(string name, bool value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Variable name cannot be empty.", nameof(name));

            boolVariables[name] = value;
        }

        public static void SetInt(string name, int value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Variable name cannot be empty.", nameof(name));

            intVariables[name] = value;
        }

        public static void SetFloat(string name, float value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Variable name cannot be empty.", nameof(name));

            floatVariables[name] = value;
        }

        public static void SetString(string name, string value)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Variable name cannot be empty.", nameof(name));

            stringVariables[name] = value;
        }

        public static void InitializeFromDefinitions(DialogDatabase database)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));

            boolVariables.Clear();
            intVariables.Clear();
            floatVariables.Clear();
            stringVariables.Clear();

            IReadOnlyList<VariableDefinition> definitions = database.Variables;

            if (definitions == null) throw new InvalidOperationException("DialogDatabase.Variables is null.");

            foreach (VariableDefinition def in definitions)
            {
                if (def == null || string.IsNullOrEmpty(def.variableName))
                    throw new InvalidOperationException("DialogDatabase has a null variable definition or empty variableName.");

                if (def.variableType == ConditionVariableType.Bool) boolVariables[def.variableName] = def.boolDefault;
                else if (def.variableType == ConditionVariableType.Int) intVariables[def.variableName] = def.intDefault;
                else if (def.variableType == ConditionVariableType.Float) floatVariables[def.variableName] = def.floatDefault;
                else if (def.variableType == ConditionVariableType.String) stringVariables[def.variableName] = def.stringDefault ?? "";
            }
        }

        public static string GetSavePath() => Path.Combine(Application.persistentDataPath, "save.json");

        public static void LoadGame(string filePath = null)
        {
            if (string.IsNullOrEmpty(filePath)) filePath = GetSavePath();

            if (!File.Exists(filePath))
            {
                Debug.Log($"Save file not found ({filePath}). Using default values from SO.");
                return;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                var data = JsonUtility.FromJson<SaveData>(json);

                if (data == null) throw new InvalidOperationException($"Save file is empty or invalid JSON: {filePath}");

                int loadedVersion = data.saveVersion <= 0 ? 1 : data.saveVersion;

                if (loadedVersion > CURRENT_SAVE_VERSION) Debug.LogWarning($"G: save.json version {loadedVersion} is newer than supported {CURRENT_SAVE_VERSION}. Loading known fields only.");

                ApplyLoadedSaveData(data);

                Debug.Log($"Game loaded from {filePath} (save version {loadedVersion}).");
            }
            catch (Exception e)
            {
                Debug.LogError($"Load error: {e.Message}. Keeping current G values.");
            }
        }

        private static void ApplyLoadedSaveData(SaveData data)
        {
            if (data.boolVars != null)
            {
                foreach (SaveData.BoolEntry e in data.boolVars)
                {
                    if (string.IsNullOrEmpty(e.k)) continue;
                    if (!boolVariables.ContainsKey(e.k)) continue;

                    boolVariables[e.k] = e.v;
                }
            }

            if (data.intVars != null)
            {
                foreach (SaveData.IntEntry e in data.intVars)
                {
                    if (string.IsNullOrEmpty(e.k)) continue;
                    if (!intVariables.ContainsKey(e.k)) continue;

                    intVariables[e.k] = e.v;
                }
            }

            if (data.floatVars != null)
            {
                foreach (SaveData.FloatEntry e in data.floatVars)
                {
                    if (string.IsNullOrEmpty(e.k)) continue;
                    if (!floatVariables.ContainsKey(e.k)) continue;

                    floatVariables[e.k] = e.v;
                }
            }

            if (data.stringVars != null)
            {
                foreach (SaveData.StringEntry e in data.stringVars)
                {
                    if (string.IsNullOrEmpty(e.k)) continue;
                    if (!stringVariables.ContainsKey(e.k)) continue;

                    stringVariables[e.k] = e.v ?? "";
                }
            }
        }

        public static void SaveGame(string filePath = null, bool logResult = true)
        {
            if (string.IsNullOrEmpty(filePath)) filePath = GetSavePath();

            try
            {
                var data = new SaveData
                {
                    saveVersion = CURRENT_SAVE_VERSION,
                    boolVars = boolVariables.Select(kv => new SaveData.BoolEntry { k = kv.Key, v = kv.Value }).ToList(),
                    intVars = intVariables.Select(kv => new SaveData.IntEntry { k = kv.Key, v = kv.Value }).ToList(),
                    floatVars = floatVariables.Select(kv => new SaveData.FloatEntry { k = kv.Key, v = kv.Value }).ToList(),
                    stringVars = stringVariables.Select(kv => new SaveData.StringEntry { k = kv.Key, v = kv.Value ?? "" }).ToList()
                };
                string json = JsonUtility.ToJson(data, true);
                WriteAllTextAtomic(filePath, json);

                if (logResult) Debug.Log($"Saving variables: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Save error: {e.Message}");
            }
        }

        private static void WriteAllTextAtomic(string filePath, string contents)
        {
            string directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string tempPath = filePath + ".tmp";
            File.WriteAllText(tempPath, contents);

            if (File.Exists(filePath)) File.Replace(tempPath, filePath, null);
            else File.Move(tempPath, filePath);
        }

        public static void RenameVariable(string oldName, string newName, ConditionVariableType type)
        {
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName) || oldName == newName) return;

            switch (type)
            {
                case ConditionVariableType.Bool:
                    if (boolVariables.TryGetValue(oldName, out bool boolValue))
                    {
                        boolVariables[newName] = boolValue;
                        boolVariables.Remove(oldName);
                    }

                    break;
                case ConditionVariableType.Int:
                    if (intVariables.TryGetValue(oldName, out int intValue))
                    {
                        intVariables[newName] = intValue;
                        intVariables.Remove(oldName);
                    }

                    break;
                case ConditionVariableType.Float:
                    if (floatVariables.TryGetValue(oldName, out float floatValue))
                    {
                        floatVariables[newName] = floatValue;
                        floatVariables.Remove(oldName);
                    }

                    break;
                case ConditionVariableType.String:
                    if (stringVariables.TryGetValue(oldName, out string stringValue))
                    {
                        stringVariables[newName] = stringValue;
                        stringVariables.Remove(oldName);
                    }

                    break;
            }
        }

        public static void RemoveVariable(string name, ConditionVariableType type)
        {
            if (string.IsNullOrEmpty(name)) return;

            switch (type)
            {
                case ConditionVariableType.Bool:
                    boolVariables.Remove(name);
                    break;
                case ConditionVariableType.Int:
                    intVariables.Remove(name);
                    break;
                case ConditionVariableType.Float:
                    floatVariables.Remove(name);
                    break;
                case ConditionVariableType.String:
                    stringVariables.Remove(name);
                    break;
            }
        }

        public static void SetVariableFromDefinition(VariableDefinition def)
        {
            if (def == null || string.IsNullOrEmpty(def.variableName)) return;

            if (def.variableType == ConditionVariableType.Bool) SetBool(def.variableName, def.boolDefault);
            else if (def.variableType == ConditionVariableType.Int) SetInt(def.variableName, def.intDefault);
            else if (def.variableType == ConditionVariableType.Float) SetFloat(def.variableName, def.floatDefault);
            else if (def.variableType == ConditionVariableType.String) SetString(def.variableName, def.stringDefault ?? "");
        }

        [Serializable]
        private class SaveData
        {
            [Serializable] public class BoolEntry { public string k; public bool v; }
            [Serializable] public class IntEntry { public string k; public int v; }
            [Serializable] public class FloatEntry { public string k; public float v; }
            [Serializable] public class StringEntry { public string k; public string v; }

            public int saveVersion;
            public List<BoolEntry> boolVars;
            public List<IntEntry> intVars;
            public List<FloatEntry> floatVars;
            public List<StringEntry> stringVars;
        }

        public static bool GetBool(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Variable name cannot be empty.", nameof(name));

            return boolVariables.TryGetValue(name, out bool v) && v;
        }

        public static int GetInt(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Variable name cannot be empty.", nameof(name));

            return intVariables.TryGetValue(name, out int v) ? v : 0;
        }

        public static float GetFloat(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Variable name cannot be empty.", nameof(name));

            return floatVariables.TryGetValue(name, out float v) ? v : 0f;
        }

        public static string GetString(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("Variable name cannot be empty.", nameof(name));

            return stringVariables.TryGetValue(name, out string v) ? v : "";
        }

        public static string[] GetBoolVariableNames() => boolVariables.Keys.ToArray();
        public static string[] GetIntVariableNames() => intVariables.Keys.ToArray();
        public static string[] GetFloatVariableNames() => floatVariables.Keys.ToArray();
        public static string[] GetStringVariableNames() => stringVariables.Keys.ToArray();
    }
}