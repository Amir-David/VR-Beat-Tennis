using UnityEngine;
using System.IO;

public class LeftPadd : MonoBehaviour
{
    public Vector3 normal;
    public float timeOutTime;
    public Collider col;
    public ParticleSystem slashParticles;
    public AudioSource audioSource;

    private int correctColorPaddleScore;
    private int wrongColorPaddleScore;

    private void Start()
    {
        LoadScoreData();
    }

    private void LoadScoreData()
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
                correctColorPaddleScore = levelData.scoreData.correctColorPaddle;
                wrongColorPaddleScore = levelData.scoreData.wrongColorPaddle;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("RedBall"))
        {
            DisplayScore.score += correctColorPaddleScore;

            if (audioSource != null)
            {
                audioSource.Play();
            }

            RedBallController redBall = other.GetComponent<RedBallController>();
            if (redBall != null && redBall.rb != null)
            {
                Vector3 worldNormal = transform.TransformDirection(normal);
                Vector3 newVelocity = Vector3.Reflect(redBall.rb.velocity, worldNormal);
                redBall.rb.velocity = newVelocity;

                if (col != null)
                {
                    col.enabled = false;
                    Invoke("EnableCollider", timeOutTime);
                }

                slashParticles.transform.position = other.transform.position;
                Vector3 directionXZ = new Vector3(worldNormal.x, 0, worldNormal.z);
                slashParticles.transform.rotation = Quaternion.LookRotation(directionXZ, Vector3.up);
                slashParticles.Play();
            }
        }
        // else if (other.CompareTag("BlueBall"))
        // {
        //     DisplayScore.score += wrongColorPaddleScore;
        //     Destroy(other.gameObject);
        // }
        // else
        // {
        //     Destroy(other.gameObject);
        // }
    }

    public void EnableCollider()
    {
        if (col != null)
        {
            col.enabled = true;
        }
    }

    [System.Serializable]
    private class ScoreData
    {
        public int correctColorPaddle;
        public int wrongColorPaddle;
    }

    [System.Serializable]
    private class LevelData
    {
        public ScoreData scoreData;
    }
}