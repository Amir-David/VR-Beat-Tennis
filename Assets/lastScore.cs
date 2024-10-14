using UnityEngine;
using TMPro;
using System.IO;

public class lastScore : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComponent;

    void Start()
    {
        if (textComponent == null)
        {
            return;
        }

        LoadAndDisplayLastScore();
    }

    public void LoadAndDisplayLastScore()
    {
        if (string.IsNullOrEmpty(LevelManager.CurrentLevel))
        {
            return;
        }

        string levelPath = Path.Combine(Application.dataPath, "levels", LevelManager.CurrentLevel + ".json");
        if (File.Exists(levelPath))
        {
            string json = File.ReadAllText(levelPath);
            LevelData levelData = JsonUtility.FromJson<LevelData>(json);
            if (levelData != null && levelData.playerData != null)
            {
                textComponent.text = levelData.playerData.lastScore.ToString();
            }
        }
    }

    [System.Serializable]
    private class PlayerData
    {
        public int lastScore;
    }

    [System.Serializable]
    private class LevelData
    {
        public PlayerData playerData;
    }
}