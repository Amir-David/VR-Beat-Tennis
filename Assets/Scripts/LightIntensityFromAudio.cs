using UnityEngine;

public class LightIntensityFromAudio : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    [Range(1, 500)]
    private float sensitivity = 100f;

    [SerializeField]
    [Range(0, 1)]
    private float smoothing = 0.5f;

    [SerializeField]
    private float threshold = 0.01f;

    [SerializeField]
    private float defaultIntensity = 0f;

    [SerializeField]
    private float intensityMultiplier = 1f;

    private float currentIntensity;
    private Light lightComponent;

    private void Awake()
    {
        lightComponent = GetComponent<Light>();
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private void Update()
    {
        if (audioSource == null || !audioSource.isPlaying)
        {
            if (lightComponent != null)
            {
                lightComponent.intensity = defaultIntensity;
            }
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

        if (lightComponent != null)
        {
            lightComponent.intensity = currentIntensity * intensityMultiplier;
        }
    }
}