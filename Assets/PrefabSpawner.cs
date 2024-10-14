using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using UnityEngine.SceneManagement;

public class PrefabSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Transform parentLayer;
    public Vector3 positionOffset = new Vector3(0, 0, 1);
    public Button resetButton;
    public Button saveButton;
    public TMP_Dropdown requiredScoreDropDown;
    public TMP_Dropdown totalMissDropDown;
    public TMP_Dropdown wrongColorPaddleDropDown;
    public TMP_Dropdown correctColorPaddleDropDown;
    public TMP_Dropdown wrongColorGoalDropDown;
    public TMP_Dropdown correctColorGoalDropDown;

    private Dictionary<string, Button> buttonReferences = new Dictionary<string, Button>();
    private Dictionary<string, int> buttonColorStates = new Dictionary<string, int>();
    private List<CustomNote> customNotes = new List<CustomNote>();
    private MapData originalMapData;
    private string currentLevelPath;
    private ScoreData scoreData = new ScoreData();
    private PlayerData playerData = new PlayerData();

    [System.Serializable]
    private class Event
    {
        public string _time;
    }

    [System.Serializable]
    private class Note
    {
        public string _time;
        public float _lineIndex;
        public float _lineLayer;
        public int _type;
    }

    [System.Serializable]
    private class CustomNote
    {
        public string _time;
        public float _lineIndex;
        public float _lineLayer;
        public float _lineIndex2;
        public float _lineLayer2;
        public int _type;
    }

    [System.Serializable]
    private class ScoreData
    {
        public int requiredScore;
        public int totalMiss;
        public int wrongColorPaddle;
        public int correctColorPaddle;
        public int wrongColorGoal;
        public int correctColorGoal;
    }

    [System.Serializable]
    public class PlayerData
    {
        public int highScore = 0;
        public int lastScore = 0;
        public int timesPlayed = 0;
    }

    [System.Serializable]
    private class LevelData
    {
        public List<CustomNote> notes;
        public ScoreData scoreData;
        public PlayerData playerData;
    }

    [System.Serializable]
    private class MapData
    {
        public List<Event> _events;
        public List<Note> _notes;
    }

    void Start()
    {
        LoadButtonStates();
        string jsonFilePath = Path.Combine(Application.dataPath, "Easy.dat");
        string jsonContent = File.ReadAllText(jsonFilePath);
        originalMapData = JsonUtility.FromJson<MapData>(jsonContent);
        if (originalMapData == null || (originalMapData._events == null && originalMapData._notes == null))
        {
            return;
        }
        SetCurrentLevelPath();
        if (File.Exists(currentLevelPath))
        {
            LoadLevelData();
        }
        else
        {
            InitializeCustomNotesFromEasyDat();
            SetupScoreDropdowns(new ScoreData());
        }
        ColorButtons();
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(ResetButtons);
        }
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveChanges);
        }
    }

    private void OnEnable()
    {
        SetCurrentLevelPath();
    }

    void SetCurrentLevelPath()
    {
        if (string.IsNullOrEmpty(LevelManager.CurrentLevel))
        {
            return;
        }
        string fileName = Path.GetFileNameWithoutExtension(LevelManager.CurrentLevel) + ".json";
        currentLevelPath = Path.Combine(Application.dataPath, "levels", fileName);
    }

    void SetupScoreDropdowns(ScoreData data)
    {
        SetupDropdown(requiredScoreDropDown, 0, 1000);
        SetupDropdown(totalMissDropDown, -10, 0);
        SetupDropdown(wrongColorPaddleDropDown, -10, 0);
        SetupDropdown(correctColorPaddleDropDown, 0, 10);
        SetupDropdown(wrongColorGoalDropDown, -10, 0);
        SetupDropdown(correctColorGoalDropDown, 0, 10);

        requiredScoreDropDown.value = ConvertToDropdownValue(data.requiredScore, 0, 1000);
        totalMissDropDown.value = ConvertToDropdownValue(data.totalMiss, -10, 0);
        wrongColorPaddleDropDown.value = ConvertToDropdownValue(data.wrongColorPaddle, -10, 0);
        correctColorPaddleDropDown.value = ConvertToDropdownValue(data.correctColorPaddle, 0, 10);
        wrongColorGoalDropDown.value = ConvertToDropdownValue(data.wrongColorGoal, -10, 0);
        correctColorGoalDropDown.value = ConvertToDropdownValue(data.correctColorGoal, 0, 10);

        requiredScoreDropDown.onValueChanged.AddListener(delegate { UpdateScoreData(); });
        totalMissDropDown.onValueChanged.AddListener(delegate { UpdateScoreData(); });
        wrongColorPaddleDropDown.onValueChanged.AddListener(delegate { UpdateScoreData(); });
        correctColorPaddleDropDown.onValueChanged.AddListener(delegate { UpdateScoreData(); });
        wrongColorGoalDropDown.onValueChanged.AddListener(delegate { UpdateScoreData(); });
        correctColorGoalDropDown.onValueChanged.AddListener(delegate { UpdateScoreData(); });
    }

    void SetupDropdown(TMP_Dropdown dropdown, int min, int max)
    {
        dropdown.ClearOptions();
        List<string> options = new List<string>();
        for (int i = min; i <= max; i++)
        {
            options.Add(i.ToString());
        }
        dropdown.AddOptions(options);
    }

    int ConvertToDropdownValue(int score, int min, int max)
    {
        return score - min;
    }

    int ConvertFromDropdownValue(int dropdownValue, int min, int max)
    {
        return dropdownValue + min;
    }

    void UpdateScoreData()
    {
        scoreData.requiredScore = ConvertFromDropdownValue(requiredScoreDropDown.value, 0, 1000);
        scoreData.totalMiss = ConvertFromDropdownValue(totalMissDropDown.value, -10, 0);
        scoreData.wrongColorPaddle = ConvertFromDropdownValue(wrongColorPaddleDropDown.value, -10, 0);
        scoreData.correctColorPaddle = ConvertFromDropdownValue(correctColorPaddleDropDown.value, 0, 10);
        scoreData.wrongColorGoal = ConvertFromDropdownValue(wrongColorGoalDropDown.value, -10, 0);
        scoreData.correctColorGoal = ConvertFromDropdownValue(correctColorGoalDropDown.value, 0, 10);
    }

    void UpdateText(GameObject obj, string text)
    {
        Transform textTransform = obj.transform.Find("Text (TMP)");
        if (textTransform != null)
        {
            TextMeshProUGUI tmpText = textTransform.GetComponent<TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = text;
            }
        }
    }


    void UpdateCustomNote(string time, float lineIndex, float lineLayer, float lineIndex2, float lineLayer2, int type)
    {
        CustomNote existingNote = customNotes.FirstOrDefault(n => 
            n._time == time && n._lineIndex == lineIndex && n._lineLayer == lineLayer);

        if (existingNote != null)
        {
            if (type == 2)
            {
                customNotes.Remove(existingNote);
            }
            else
            {
                existingNote._type = type;
            }
        }
        else if (type != 2)
        {
            customNotes.Add(new CustomNote
            {
                _time = time,
                _lineIndex = (float)Math.Round(lineIndex, 1),
                _lineLayer = (float)Math.Round(lineLayer, 1),
                _lineIndex2 = lineIndex2,
                _lineLayer2 = lineLayer2,
                _type = type
            });
        }
    }

    void AddCircleDraggedListener(GameObject obj, string time)
    {
        ImageClickHandler imageClickHandler = obj.GetComponent<ImageClickHandler>();
        if (imageClickHandler != null)
        {
            imageClickHandler.OnCircleDragged += (GameObject draggedCircle) =>
            {
                if (imageClickHandler.CirclePairs.TryGetValue(draggedCircle, out GameObject pairedCircle))
                {
                    Vector2 pos1 = imageClickHandler.GetNormalizedPositionForCircle(pairedCircle);
                    Vector2 pos2 = imageClickHandler.GetNormalizedPositionForCircle(draggedCircle);                    
                    UpdateCustomNote(time, (float)Math.Round(pos1.x, 1), (float)Math.Round(pos1.y, 1),0 ,0, 2);
                    UpdateCustomNote(time, (float)Math.Round(pos1.x, 1), (float)Math.Round(pos1.y, 1),pos2.x ,pos2.y, 0);
                }
            };
        }
    }

    void AddCircleAdditionListener(GameObject obj, string time)
    {
        ImageClickHandler imageClickHandler = obj.GetComponent<ImageClickHandler>();
        if (imageClickHandler != null)
        {
            imageClickHandler.OnCircleAdded += (float lineIndex1, float lineLayer1, float lineIndex2, float lineLayer2) =>
            {
                UpdateCustomNote(time, (float)Math.Round(lineIndex1, 1), (float)Math.Round(lineLayer1, 1), lineIndex2, lineLayer2, 0);
            };
        }
    }

    public void AddNewTimeObject(string time)
    {
        if (!timeToObjectMap.ContainsKey(time))
        {
            Vector3 spawnPosition = parentLayer.position + positionOffset * float.Parse(time, CultureInfo.InvariantCulture);
            GameObject newObj = Instantiate(prefabToSpawn, spawnPosition, parentLayer.rotation, parentLayer);
            UpdateText(newObj, $"{time} ROW BEAT");
            
            AddCircleAdditionListener(newObj, time);
            
            ImageClickHandler imageClickHandler = newObj.GetComponent<ImageClickHandler>();
            
            timeToObjectMap[time] = newObj;
        }
    }

    void AddCircleRemovalListener(GameObject obj, string time)
    {
        ImageClickHandler imageClickHandler = obj.GetComponent<ImageClickHandler>();
        if (imageClickHandler != null)
        {
            imageClickHandler.OnCircleRemoved += (float lineIndex1, float lineLayer1, float lineIndex2, float lineLayer2) =>
            {                
                UpdateCustomNote(time, (float)Math.Round(lineIndex1, 1), (float)Math.Round(lineLayer1, 1),0 ,0, 2);
            };
        }
    }

    void SaveLevelData()
    {
        customNotes = customNotes.OrderBy(n => float.Parse(n._time, CultureInfo.InvariantCulture)).ToList();
        LevelData levelData = new LevelData
        {
            notes = customNotes,
            scoreData = scoreData,
            playerData = playerData
            
        };
        string json = JsonUtility.ToJson(levelData, true);
        File.WriteAllText(currentLevelPath, json);

        #if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
        #endif
    }

    void LoadLevelData()
    {
        if (File.Exists(currentLevelPath))
        {
            string json = File.ReadAllText(currentLevelPath);
            if (json == "{}")
            {
                InitializeCustomNotesFromEasyDat();
                SetupScoreDropdowns(new ScoreData());
                playerData = new PlayerData();
            }
            else
            {
                LevelData levelData = JsonUtility.FromJson<LevelData>(json);
                if (levelData != null)
                {
                    customNotes = levelData.notes ?? new List<CustomNote>();
                    scoreData = levelData.scoreData ?? new ScoreData();
                    SetupScoreDropdowns(scoreData);
                    playerData = levelData.playerData ?? new PlayerData();
                }
            }
        }
    }

    void InitializeCustomNotesFromEasyDat()
    {
        customNotes.Clear();
        foreach (var note in originalMapData._notes)
        {
            if (note._type == 0 || note._type == 1)
            {
                customNotes.Add(new CustomNote
                {
                    _time = note._time,
                    _lineIndex = note._lineIndex,
                    _lineLayer = note._lineLayer,
                    _lineIndex2 = UnityEngine.Random.Range(0f, 3f),
                    _lineLayer2 = UnityEngine.Random.Range(0f, 2f),
                    _type = 0
                });
            }
        }
    }

    private Dictionary<string, GameObject> timeToObjectMap = new Dictionary<string, GameObject>();

    void ColorButtons()
    {
        foreach (var note in customNotes)
        {   
            GameObject obj;
            if (!timeToObjectMap.TryGetValue(note._time, out obj))
            {
                Vector3 spawnPosition = parentLayer.position + positionOffset * float.Parse(note._time, CultureInfo.InvariantCulture);
                obj = Instantiate(prefabToSpawn, spawnPosition, parentLayer.rotation, parentLayer);
                UpdateText(obj, $"{note._time} ROW BEAT");
                
                AddCircleAdditionListener(obj, note._time);
                AddCircleDraggedListener(obj, note._time);
                AddCircleRemovalListener(obj, note._time);
                
                timeToObjectMap[note._time] = obj;
            }
            ImageClickHandler imageClickHandler = obj.GetComponent<ImageClickHandler>();
            var (circle1, circle2) = imageClickHandler.CreateCircleFromCoord1(note._lineIndex, note._lineLayer, note._lineIndex2, note._lineLayer2);

            
            imageClickHandler.InitializeCircles();
        }
    }


    private class DestroyTracker : MonoBehaviour
    {
        public event System.Action OnDestroyed;
        
        void OnDestroy()
        {
            OnDestroyed?.Invoke();
        }
    }


    string GetButtonKey(string time, int lineLayer, int lineIndex)
    {
        return $"{float.Parse(time, CultureInfo.InvariantCulture):F16}_lineLayer{lineLayer}_lineIndex{lineIndex}";
    }

    void SaveButtonStates()
    {
        string saveData = string.Join(",", buttonColorStates.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
        PlayerPrefs.SetString("ButtonColorStates", saveData);
        PlayerPrefs.Save();
    }

    void LoadButtonStates()
    {
        string saveData = PlayerPrefs.GetString("ButtonColorStates", "");
        if (!string.IsNullOrEmpty(saveData))
        {
            buttonColorStates = saveData.Split(',')
                .Select(s => s.Split(':'))
                .ToDictionary(
                    s => s[0],
                    s => int.Parse(s[1])
                );
        }
    }

    public void ResetButtons()
    {
        // InitializeCustomNotesFromEasyDat();
        // ColorButtons();
        // SetupScoreDropdowns(new ScoreData());
    }

    public void SaveChanges()
    {
        UpdateScoreData();
        SaveLevelData();
        SaveButtonStates();
        SceneManager.LoadScene(0);
    }

    void OnApplicationQuit()
    {
        SaveChanges();
    }
}