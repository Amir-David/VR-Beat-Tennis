using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json.Linq;

public class BeatPaddleBallSpawner : MonoBehaviour
{
    public GameObject redBallPrefab;
    public GameObject blueBallPrefab;
    public Transform spawnPoint;
    public Button returnButton;
    public Button retryButton;
    public GameObject gameMenue;
    public float horizontalSpacing = 1.0f;
    public float verticalSpacing = 2.0f;
    public float bpm = 105;
    public float noteJumpMovementSpeed = 10f;
    public float noteJumpStartBeatOffset = 1f;
    public float blueBallZOffset = 5f;
    public lastScore lastScoreScript;
    public highestScore highestScoreScript;

    private int pairIdCounter = 0;

    private void Start()
    {
        StartCoroutine(Initialize());
        gameMenue.SetActive(false);
        
        if (returnButton != null)
        {
            returnButton.onClick.AddListener(ReturnButtons);
        }
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(RetryButtons);
        }
    }

    private IEnumerator Initialize()
    {
        if (string.IsNullOrEmpty(LevelManager.CurrentLevel))
        {
            yield break;
        }

        string levelPath = Path.Combine(Application.dataPath, "levels", LevelManager.CurrentLevel + ".json");

        if (File.Exists(levelPath))
        {
            yield return StartCoroutine(ReadLevelData(levelPath, ProcessMapData));
        }
        else
        {
            string defaultMapPath = Path.Combine(Application.streamingAssetsPath, "Easy.dat");
            if (File.Exists(defaultMapPath))
            {
                yield return StartCoroutine(ReadDefaultMap(defaultMapPath, ProcessMapData));
            }
        }
    }

    private IEnumerator ReadLevelData(string filePath, System.Action<string> onComplete)
    {
        string jsonContent = File.ReadAllText(filePath);
        onComplete?.Invoke(jsonContent);
        yield return null;
    }

    private IEnumerator ReadDefaultMap(string filePath, System.Action<string> onComplete)
    {
        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            string jsonContent = File.ReadAllText(filePath);
            onComplete?.Invoke(jsonContent);
        }
        else
        {
            onComplete?.Invoke(null);
        }
        yield return null;
    }

    private void ProcessMapData(string jsonString)
    {
        if (!string.IsNullOrEmpty(jsonString))
        {
            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonString);
            if (levelData != null && levelData.notes != null && levelData.notes.Length > 0)
            {
                StartCoroutine(SpawnBalls(levelData.notes));
            }
        }
    }

    private IEnumerator SpawnBalls(BeatPaddleBallData[] notes)
    {
        if (notes == null)
        {
            yield break;
        }

        float startTime = Time.time;
        float gameTime = 0f;

        foreach (BeatPaddleBallData ballData in notes)
        {
            if (ballData == null)
            {
                continue;
            }

            float targetTime = (ballData._time / bpm) * 60;

            while (gameTime < targetTime)
            {
                gameTime = Time.time - startTime;
                yield return null;
            }

            SpawnBall(ballData);
        }
        StartCoroutine(DelayedOver());
    }

    private void SpawnBall(BeatPaddleBallData ballData)
    {
        int pairId = pairIdCounter++;
        GameObject redBallInstance = SpawnSpecificBall(redBallPrefab, ballData._lineIndex, ballData._lineLayer, "RedBall", 0f, pairId);
        GameObject blueBallInstance = SpawnSpecificBall(blueBallPrefab, ballData._lineIndex2, ballData._lineLayer2, "BlueBall", blueBallZOffset, pairId);

        SetBallVelocity(redBallInstance);
        SetBallVelocity(blueBallInstance);
    }

    private GameObject SpawnSpecificBall(GameObject ballPrefab, float lineIndex, float lineLayer, string expectedTag, float zOffset, int pairId)
    {
        if (ballPrefab == null)
        {
            return null;
        }

        Vector3 spawnPosition = spawnPoint.position;
        spawnPosition.x += lineIndex * horizontalSpacing;
        spawnPosition.y += lineLayer * verticalSpacing;
        spawnPosition.z += zOffset;

        GameObject ballInstance = Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
        
        if (ballInstance.tag != expectedTag)
        {
            ballInstance.tag = expectedTag;
        }

        Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = ballInstance.AddComponent<Rigidbody>();
        }

        BallPairIdentifier pairIdentifier = ballInstance.AddComponent<BallPairIdentifier>();
        pairIdentifier.pairId = pairId;

        return ballInstance;
    }

    private void SetBallVelocity(GameObject ballInstance)
    {
        if (ballInstance != null)
        {
            Rigidbody rb = ballInstance.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = -spawnPoint.forward * noteJumpMovementSpeed;
            }
        }
    }

    private IEnumerator DelayedOver()
    {
        yield return new WaitForSeconds(10.5f);
        
        if (lastScoreScript != null)
        {
            lastScoreScript.LoadAndDisplayLastScore();
        }

        if (highestScoreScript != null)
        {
            highestScoreScript.LoadAndDisplayHighScore();
        }

        Over();
    }

    public void Over()
    {
        gameMenue.SetActive(true);
    }

    public void ReturnButtons()
    {
        gameMenue.SetActive(false);
        SceneManager.LoadScene(0);
    }

    public void RetryButtons()
    {
        gameMenue.SetActive(false);
        SceneManager.LoadScene(1);
    }

    [System.Serializable]
    private class LevelData
    {
        public BeatPaddleBallData[] notes;
    }

    [System.Serializable]
    public class BeatPaddleBallData
    {
        public float _time;
        public float _lineIndex;
        public float _lineLayer;
        public float _lineIndex2;
        public float _lineLayer2;
        public int _type;
    }
}

public class BallPairIdentifier : MonoBehaviour
{
    public int pairId;
}