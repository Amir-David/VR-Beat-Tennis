using UnityEngine;

public class HoopBuilder : MonoBehaviour
{
    public GameObject cylinderPrefab;
    public int numberOfCylinders = 20;
    public float radius = 1.0f;

    void Start()
    {
        for (int i = 0; i < numberOfCylinders; i++)
        {
            float angle = i * Mathf.PI * 2 / numberOfCylinders;
            Vector3 position = new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            GameObject cylinder = Instantiate(cylinderPrefab, position, Quaternion.identity);
            cylinder.transform.Rotate(Vector3.right, 90f);
            cylinder.transform.SetParent(transform);
        }
    }
}
