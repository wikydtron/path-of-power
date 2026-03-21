using System.IO;
using UnityEngine;
using PathOfPower.Core;

namespace PathOfPower.Core
{
    public static class SaveSystem
    {
        private static readonly string _saveFileName = "playerdata.json";
        private static string SavePath => Path.Combine(Application.persistentDataPath, _saveFileName);

        public static void Save(PlayerData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                File.WriteAllText(SavePath, json);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to save: {ex.Message}");
            }
        }

        public static PlayerData Load()
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[SaveSystem] No save file found — creating new PlayerData.");
                return new PlayerData();
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);
                if (data == null) return new PlayerData();
                return data;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to load save: {ex.Message}. Returning fresh data.");
                return new PlayerData();
            }
        }

        public static void DeleteSave()
        {
            if (File.Exists(SavePath))
                File.Delete(SavePath);
        }
    }
}
