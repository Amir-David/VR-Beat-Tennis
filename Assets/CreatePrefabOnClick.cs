using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.SceneManagement;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class ButtonPrefabCreator : MonoBehaviour
{
    public Button newLevelBtn;
    public GameObject levelPrefab;
    public GameObject mainMenu;
    public GameObject levelEditor;
    public string levelsPath = "Assets/levels";

    private List<GameObject> levelButtons = new List<GameObject>();

    private void Start()
    {
        newLevelBtn.onClick.AddListener(CreateNewLevel);
        LoadExistingLevels();
    }

    private void LoadExistingLevels()
    {
        string[] levelFiles = Directory.GetFiles(levelsPath, "*.json");

        if (levelFiles.Length > 0)
        {
            string[] levelNames = levelFiles
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(name => ExtractLevelNumber(name))
                .ToArray();

            foreach (string levelName in levelNames)
            {
                CreateLevelButton(levelName);
            }
        }
    }

    private void CreateNewLevel()
    {
        if (levelPrefab != null)
        {
            string[] levelFiles = Directory.GetFiles(levelsPath, "*.json");
            string newLevelName;

            if (levelFiles.Length > 0)
            {
                string[] levelNames = levelFiles
                    .Select(Path.GetFileNameWithoutExtension)
                    .OrderBy(name => ExtractLevelNumber(name))
                    .ToArray();

                string lastLevelName = levelNames.Last();
                newLevelName = IncrementLevelName(lastLevelName);
            }
            else
            {
                newLevelName = "LEVEL 1";
            }

            CreateLevelButton(newLevelName);

            string newLevelPath = Path.Combine(levelsPath, $"{newLevelName}.json");
            File.WriteAllText(newLevelPath, "{}");

            LevelManager.CurrentLevel = newLevelName;

            HideMenus();
            levelEditor.SetActive(true);
        }
    }

    private void CreateLevelButton(string levelName)
    {
        GameObject levelBtn = Instantiate(levelPrefab, transform);
        levelButtons.Add(levelBtn);
        levelBtn.transform.localPosition = Vector3.zero;
        levelBtn.transform.localRotation = Quaternion.identity;

        Transform levelTextTransform = levelBtn.transform
            .Find("ButtonLevelListContentV")
            ?.Find("lineLayer0")
            ?.Find("LEVEL");

        if (levelTextTransform != null)
        {
            TextMeshProUGUI levelText = levelTextTransform.GetComponent<TextMeshProUGUI>();
            if (levelText != null)
            {
                levelText.text = levelName;
            }

            Button parentBtn = levelTextTransform.GetComponentInParent<Button>();
            if (parentBtn != null)
            {
                parentBtn.onClick.RemoveAllListeners();
            }

            Button[] btnComponents = levelBtn.GetComponentsInChildren<Button>();
            foreach (Button btn in btnComponents)
            {
                btn.onClick.AddListener(() => OnLevelButtonClick(btn, levelBtn));
            }

            UpdatePlayButtonVisibility(levelName, levelBtn);
        }
    }

    private void UpdatePlayButtonVisibility(string currentLevelName, GameObject levelBtn)
    {
        int currentLevelNum = ExtractLevelNumber(currentLevelName);
        if (currentLevelNum <= 1) return;

        string prevLevelName = $"LEVEL {currentLevelNum - 1}";
        string prevLevelPath = Path.Combine(levelsPath, $"{prevLevelName}.json");

        bool hidePlayButton = !File.Exists(prevLevelPath);

        if (!hidePlayButton)
        {
            string json = File.ReadAllText(prevLevelPath);
            JObject levelData = JObject.Parse(json);

            JToken requiredScore = levelData["scoreData"]?["requiredScore"];
            JToken highScore = levelData["playerData"]?["highScore"];

            hidePlayButton = requiredScore == null || highScore == null ||
                             highScore.Value<int>() <= requiredScore.Value<int>();
        }

        if (hidePlayButton)
        {
            Button playBtn = levelBtn.GetComponentsInChildren<Button>()
                .FirstOrDefault(b => b.name == "PLAY BUTTON");
            
            if (playBtn != null)
            {
                playBtn.gameObject.SetActive(false);
            }
        }
    }

    private int ExtractLevelNumber(string levelName)
    {
        Match match = Regex.Match(levelName, @"\d+");
        return match.Success ? int.Parse(match.Value) : 0;
    }

    private string IncrementLevelName(string levelName)
    {
        Match match = Regex.Match(levelName, @"LEVEL (\d+)");
        if (match.Success)
        {
            int levelNum = int.Parse(match.Groups[1].Value) + 1;
            return $"LEVEL {levelNum}";
        }
        return levelName;
    }

    private void DeleteLevel(string levelName)
    {
        string levelPath = Path.Combine(levelsPath, $"{levelName}.json");
        
        if (File.Exists(levelPath))
        {
            File.Delete(levelPath);
            UpdateRemainingLevels(levelName);
        }
    }

    private void UpdateRemainingLevels(string deletedLevelName)
    {
        int deletedLevelNum = ExtractLevelNumber(deletedLevelName);
        string[] levelFiles = Directory.GetFiles(levelsPath, "*.json")
            .OrderBy(f => ExtractLevelNumber(Path.GetFileNameWithoutExtension(f)))
            .ToArray();

        foreach (string filePath in levelFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            int currentLevelNum = ExtractLevelNumber(fileName);

            if (currentLevelNum > deletedLevelNum)
            {
                string newLevelName = $"LEVEL {currentLevelNum - 1}";
                string newPath = Path.Combine(levelsPath, $"{newLevelName}.json");
                File.Move(filePath, newPath);
            }
        }
    }

    private void OnLevelButtonClick(Button clickedBtn, GameObject levelBtn)
    {
        Transform levelTextTransform = levelBtn.transform
            .Find("ButtonLevelListContentV")
            ?.Find("lineLayer0")
            ?.Find("LEVEL");

        if (levelTextTransform != null)
        {
            TextMeshProUGUI levelText = levelTextTransform.GetComponent<TextMeshProUGUI>();
            if (levelText != null)
            {
                LevelManager.CurrentLevel = levelText.text;
            }
        }

        if (clickedBtn.name == "PLAY BUTTON")
        {
            HideMenus();
            SceneManager.LoadScene(1);
        }
        else if (clickedBtn.name == "EDIT BUTTON")
        {
            HideMenus();
            levelEditor.SetActive(true);
        }
        else if (clickedBtn.name == "DELETE BUTTON")
        {
            DeleteLevel(LevelManager.CurrentLevel);
            ClearLevelButtons();
            LoadExistingLevels();
        }
    }

    private void HideMenus()
    {
        mainMenu.SetActive(false);
        levelEditor.SetActive(false);
    }

    private void ClearLevelButtons()
    {
        foreach (GameObject levelBtn in levelButtons)
        {
            Destroy(levelBtn);
        }
        levelButtons.Clear();
    }
}