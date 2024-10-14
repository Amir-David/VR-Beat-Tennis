using UnityEngine;
using System.IO;

public class BlueBallController : MonoBehaviour
{
    public Rigidbody rb;
    public float speedMagnitude = 1;
    public Vector3 moveDirection = Vector3.back;
    public float moveSpeed;

    private int totalMissScore;

    private void Start()
    {
        LoadScoreData();
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
                    totalMissScore = levelData.scoreData.totalMiss;
                }
            }
        }
    }

    private void OnDisable()
    {
        GetComponentInChildren<Light>().enabled = false;
    }

    public void Initialize(BeatPaddleBallSpawner.BeatPaddleBallData ballData)
    {
        rb = GetComponent<Rigidbody>();
        rb.velocity = moveDirection * moveSpeed;
    }

    private void Update()
    {
        if (transform.position.magnitude > 50)
        {
            Destroy(gameObject);
        }

        if (transform.position.z < -5)
        {
            DisplayScore.score += totalMissScore;
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("RightHand"))
        {
            Vector3 reflectDir = Vector3.Reflect(rb.velocity, collision.contacts[0].normal);
            rb.velocity = reflectDir.normalized * speedMagnitude;
        }
        else
        {
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
        }
    }

    [System.Serializable]
    private class ScoreData
    {
        public int totalMiss;
    }

    [System.Serializable]
    private class LevelData
    {
        public ScoreData scoreData;
    }
}