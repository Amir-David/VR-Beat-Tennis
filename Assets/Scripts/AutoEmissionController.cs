using UnityEngine;

public class AutoEmissionController : MonoBehaviour
{
    [SerializeField]
    public AudioSource audioSource;
    [SerializeField]
    private Renderer targetRenderer;
    [SerializeField]
    private Color emissiveColor = Color.white;
    [SerializeField]
    [Range(1, 500)]
    private float sensitivity = 100f;
    [SerializeField]
    [Range(0, 1)]
    private float smoothing = 0.5f;
    [SerializeField]
    private float threshold = 0.01f;
    [SerializeField]
    private string emissionPropertyName = "_EmissionColor";
    [SerializeField]
    private float defaultIntensity = 0f;

    private Material material;
    private int emissionPropertyID;
    private float currentIntensity;

    private void Awake()
    {
        if (targetRenderer == null)
        {
            // Debug.LogError("Target Renderer is not set. Please assign a Renderer in the inspector.");
            enabled = false;
            return;
        }

        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                // Only log if we're in the Unity Editor
                #if UNITY_EDITOR
                // Debug.Log("AudioSource component not found. One has been added automatically.");
                #endif
            }
        }

        material = targetRenderer.material;
        emissionPropertyID = Shader.PropertyToID(emissionPropertyName);
    }

    private void Update()
    {
        if (audioSource == null)
        {
            // Debug.LogWarning("AudioSource is not set. Please assign an AudioSource in the inspector.");
            return;
        }

        float[] spectrumData = new float[1024];
        audioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);

        float audioAverage = 0;
        foreach (float sample in spectrumData)
        {
            audioAverage += sample;
        }

        audioAverage /= spectrumData.Length;

        if (audioAverage > threshold)
        {
            float targetIntensity = audioAverage * sensitivity;
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, smoothing);
        }
        else
        {
            currentIntensity = Mathf.Lerp(currentIntensity, defaultIntensity, smoothing);
        }

        material.SetColor(emissionPropertyID, emissiveColor * currentIntensity);
    }

    private void OnDestroy()
    {
        if (material != null)
        {
            Destroy(material);
        }
    }
}