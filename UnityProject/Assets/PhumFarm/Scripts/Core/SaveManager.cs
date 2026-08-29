using System;
using System.IO;
using UnityEngine;

namespace PhumFarm.Core
{
    public sealed class SaveManager
    {
        public const int CurrentSaveVersion = 2;

        private readonly GameState state;
        private readonly string savePath;
        private string TempPath => savePath + ".tmp";
        private string BackupPath => savePath + ".bak";

        public string LastError { get; private set; } = string.Empty;
        public bool HasSave => File.Exists(savePath) || File.Exists(BackupPath);

        public SaveManager(GameState state, string overridePath = null)
        {
            this.state = state;
            savePath = overridePath ?? Path.Combine(Application.persistentDataPath, "phum_farm_unity_save.json");
        }

        public bool Save()
        {
            LastError = string.Empty;
            try
            {
                state.Data.saveVersion = CurrentSaveVersion;
                state.Data.savedAtUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                string json = JsonUtility.ToJson(state.Data, true);
                File.WriteAllText(TempPath, json);
                if (JsonUtility.FromJson<GameSaveData>(File.ReadAllText(TempPath)) == null) throw new InvalidDataException("Temporary save verification failed");
                if (File.Exists(BackupPath)) File.Delete(BackupPath);
                if (File.Exists(savePath)) File.Move(savePath, BackupPath);
                File.Move(TempPath, savePath);
                state.NotifyMessage("Farm saved");
                return true;
            }
            catch (Exception exception)
            {
                LastError = exception.Message;
                if (File.Exists(TempPath)) File.Delete(TempPath);
                if (!File.Exists(savePath) && File.Exists(BackupPath)) File.Move(BackupPath, savePath);
                Debug.LogError($"Phum Farm save failed: {exception}");
                return false;
            }
        }

        public bool Load()
        {
            LastError = string.Empty;
            foreach (string candidate in new[] { savePath, BackupPath })
            {
                if (!File.Exists(candidate)) continue;
                try
                {
                    GameSaveData loaded = JsonUtility.FromJson<GameSaveData>(File.ReadAllText(candidate));
                    if (loaded == null || loaded.player == null || loaded.inventory == null) continue;
                    Migrate(loaded);
                    state.Replace(loaded);
                    if (candidate == BackupPath) Save();
                    return true;
                }
                catch (Exception exception)
                {
                    LastError = exception.Message;
                }
            }
            return false;
        }

        public void Delete()
        {
            foreach (string path in new[] { savePath, TempPath, BackupPath }) if (File.Exists(path)) File.Delete(path);
        }

        private static void Migrate(GameSaveData value)
        {
            if (value.saveVersion < 2)
            {
                value.buildings ??= new BuildingState();
                value.quests ??= new QuestState();
                value.orders ??= new OrderState();
            }
            value.saveVersion = CurrentSaveVersion;
        }
    }
}
