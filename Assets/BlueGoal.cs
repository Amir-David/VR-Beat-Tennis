using UnityEngine;
using System.IO;

public class BlueGoal : MonoBehaviour
{
    private int wrongColorGoalScore;
    private int correctColorGoalScore;
    private MeshRenderer meshRenderer;
    private Rigidbody rb;
    private float originalSpeed;
    private bool isInSlowZone = false;
    private BallPairIdentifier pairIdentifier;

    void Start()
    {
        LoadScoreData();
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
        originalSpeed = rb.velocity.magnitude;
        pairIdentifier = GetComponent<BallPairIdentifier>();
    }

    void Update()
    {
        float zPos = transform.position.z;
        if (zPos >= 3f && zPos <= 7f)
        {
            meshRenderer.enabled = true;
            if (!isInSlowZone)
            {
                isInSlowZone = true;
                rb.velocity *= 0.5f;
            }
        }
        else
        {
            meshRenderer.enabled = false;
            if (isInSlowZone)
            {
                isInSlowZone = false;
                rb.velocity = rb.velocity.normalized * originalSpeed;
            }
        }
    }

    private void LoadScoreData()
    {
        if (!string.IsNullOrEmpty(LevelManager.CurrentLevel))
        {
            string levelPath = Path.Combine(Application.dataPath, "levels", LevelManager.CurrentLevel + ".json");
            if (File.Exists(levelPath))
            {
                string json = File.ReadAllText(levelPath);
                LevelData levelData = JsonUtility.FromJson<LevelData>(json);
                if (levelData != null && levelData.scoreData != null)
                {
                    wrongColorGoalScore = levelData.scoreData.wrongColorGoal;
                    correctColorGoalScore = levelData.scoreData.correctColorGoal;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (transform.position.z >= 3f && transform.position.z <= 7f)
        {
            if (other.CompareTag("RedBall"))
            {
                BallPairIdentifier otherPairIdentifier = other.GetComponent<BallPairIdentifier>();
                if (otherPairIdentifier != null && otherPairIdentifier.pairId == pairIdentifier.pairId)
                {
                    DisplayScore.score += correctColorGoalScore;
                    Destroy(other.gameObject);
                }
            }
            else if (other.CompareTag("BlueBall"))
            {
                Destroy(other.gameObject);
            }
        }
    }

    [System.Serializable]
    private class ScoreData
    {
        public int wrongColorGoal;
        public int correctColorGoal;
    }

    [System.Serializable]
    private class LevelData
    {
        public ScoreData scoreData;
    }
}