using UnityEngine;
using TMPro;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class DisplayScore : MonoBehaviour
{
    public Button returnButton;
    public Button resumeButton;
    private AudioSource audioSource;
    public GameObject gameMenue;
    [SerializeField] private TextMeshPro textMeshPro;
    public static int score;
    private static int lastUpdatedScore;
    private bool timesPlayedUpdated = false;
    private bool continueFlag = true;

    private void Start()
    {
        score = 0;
        lastUpdatedScore = 0;

        if (textMeshPro == null)
        {
            textMeshPro = GetComponent<TextMeshPro>();
            
            if (textMeshPro == null)
            {
                enabled = false;
                return;
            }
        }

        UpdateTimesPlayed();
        audioSource = FindObjectOfType<PlayMusicWithDelay>().GetAudioSource();
        gameMenue.SetActive(false);
        
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnButtons);
        }
        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeChanges);
        }

        CheckHighScoreCondition();
    }

    private void Update()
    {
        if (textMeshPro != null)
        {
            textMeshPro.text = score.ToString();
            UpdateScoreInJsonIfChanged();
        }
    }

    private void UpdateTimesPlayed()
    {
        if (!timesPlayedUpdated && !string.IsNullOrEmpty(LevelManager.CurrentLevel))
        {
            string levelPath = Path.Combine(Application.dataPath, "levels", LevelManager.CurrentLevel + ".json");
            
            if (File.Exists(levelPath))
            {
                try
                {
                    string json = File.ReadAllText(levelPath);
                    JObject levelData = JObject.Parse(json);
                    
                    int timesPlayed = levelData["playerData"]["timesPlayed"].Value<int>();
                    levelData["playerData"]["timesPlayed"] = timesPlayed + 1;
                    
                    File.WriteAllText(levelPath, levelData.ToString());
                    timesPlayedUpdated = true;
                }
                catch (System.Exception) { }
            }
        }
    }

    private void UpdateScoreInJsonIfChanged()
    {
        if (score != lastUpdatedScore && !string.IsNullOrEmpty(LevelManager.CurrentLevel))
        {
            string levelPath = Path.Combine(Application.dataPath, "levels", LevelManager.CurrentLevel + ".json");
            
            if (File.Exists(levelPath))
            {
                try
                {
                    string json = File.ReadAllText(levelPath);
                    JObject levelData = JObject.Parse(json);
                    
                    levelData["playerData"]["lastScore"] = score;
                    
                    int highScore = levelData["playerData"]["highScore"].Value<int>();

                    if (score > highScore)
                    {
                        levelData["playerData"]["highScore"] = score;
                    }
                    
                    File.WriteAllText(levelPath, levelData.ToString());
                    lastUpdatedScore = score;

                    CheckHighScoreCondition();
                }
                catch (System.Exception) { }
            }
        }
    }

    private void CheckHighScoreCondition()
    {
        if (!string.IsNullOrEmpty(LevelManager.CurrentLevel))
        {
            string levelPath = Path.Combine(Application.dataPath, "levels", LevelManager.CurrentLevel + ".json");
            
            if (File.Exists(levelPath))
            {
                try
                {
                    string json = File.ReadAllText(levelPath);
                    JObject levelData = JObject.Parse(json);
                    
                    int highScore = levelData["playerData"]["highScore"].Value<int>();
                    int requiredScore = levelData["scoreData"]["requiredScore"].Value<int>();

                    if (highScore > requiredScore && continueFlag)
                    {
                        StartCoroutine(DelayedPause());
                    }
                }
                catch (System.Exception) { }
            }
        }
    }

    private IEnumerator DelayedPause()
    {
        yield return new WaitForSeconds(0.5f);
        Pause();
    }

    public void Pause()
    {
        Time.timeScale = 0;
        audioSource.Pause();
        gameMenue.SetActive(true);
    }

    public void Continue()
    {
        Time.timeScale = 1;
        audioSource.UnPause();
        gameMenue.SetActive(false);
    }

    public void ReturnButtons()
    {
        SceneManager.LoadScene(0);
        Continue();
    }

    public void ResumeChanges()
    {
        continueFlag = false;
        Continue();
    }
}