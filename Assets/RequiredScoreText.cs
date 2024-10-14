using UnityEngine;
using TMPro;
using System.IO;

public class RequiredScoreText : MonoBehaviour
{
    private TextMeshProUGUI textComponent;

    void Start()
    {
        textComponent = GetComponent<TextMeshProUGUI>();
        if (textComponent == null)
        {
            return;
        }

        LoadAndDisplayRequiredScore();
    }

    private void LoadAndDisplayRequiredScore()
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
            if (levelData != null && levelData.scoreData != null)
            {
                textComponent.text = levelData.scoreData.requiredScore.ToString();
            }
        }
    }

    [System.Serializable]
    private class ScoreData
    {
        public int requiredScore;
    }

    [System.Serializable]
    private class LevelData
    {
        public ScoreData scoreData;
    }
}