using System.Collections.Generic;
using UnityEngine;

public class RotateTransforms : MonoBehaviour
{
    [SerializeField]
    private List<Transform> targetTransforms;

    [SerializeField]
    private float angularSpeed = 10.0f;

    [SerializeField]
    private float offset = 10.0f;

    private List<float> rotationOffsets;

    private void Awake()
    {
        // Initialize the rotationOffsets list
        rotationOffsets = new List<float>();

        // Generate rotation offsets based on the offset variable
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            rotationOffsets.Add(offset * (i + 1));
        }
    }

    private void Update()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            float angle = angularSpeed * Time.deltaTime + rotationOffsets[i];
            targetTransforms[i].Rotate(0, 0, angle);
        }
    }
}
