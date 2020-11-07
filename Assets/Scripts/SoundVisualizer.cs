using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundVisualizer : MonoBehaviour
{
    public Transform volumeIndicator;

    int _bufferSize = -1;
    int bufferSize
    {
        get
        {
            if (_bufferSize < 0)
            {
                int numBuffers;
                AudioSettings.GetDSPBufferSize(out _bufferSize, out numBuffers);
            }
            return _bufferSize;
        }
    }

    LineRenderer _line;
    LineRenderer line
    {
        get
        {
            if (!_line)
            {
                _line = gameObject.GetComponentInChildren<LineRenderer>();
                if (!_line)
                {
                    _line = gameObject.GetComponentInChildren<LineRenderer>();
                }
                _line.positionCount = bufferSize;
            }
            return _line;
        }
    }

    AudioSource _microphone;
    AudioSource microphone
    {
        get
        {
            if (!_microphone)
            {
                _microphone = gameObject.GetComponent<AudioSource>();
                if (!_microphone)
                {
                    _microphone = gameObject.AddComponent<AudioSource>();
                }
            }
            return _microphone;
        }
    }

    GameObject[] _cubes;
    GameObject[] cubes
    {
        get
        {
            if (_cubes == null)
            {
                _cubes = new GameObject[bufferSize];
                Material[] cubeMaterials = new Material[bufferSize];
                Gradient gradient = CreateRainbowGradient();
                for (int i = 0; i < bufferSize; i++)
                {
                    _cubes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    _cubes[i].transform.SetParent(transform);
                    _cubes[i].transform.position = i * Vector3.right;
                    cubeMaterials[i] = _cubes[i].GetComponent<MeshRenderer>().material;
                    cubeMaterials[i].color = gradient.Evaluate(2f * i / (float)bufferSize);
                }
            }
            return _cubes;
        }
    }

    Gradient CreateRainbowGradient()
    {
        Gradient gradient = new Gradient();

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        GradientColorKey[] colorKey = new GradientColorKey[7];
        colorKey[0].color = Color.red;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.yellow;
        colorKey[1].time = 1.0f / 6f;
        colorKey[2].color = Color.green;
        colorKey[2].time = 2.0f / 6f;
        colorKey[3].color = Color.cyan;
        colorKey[3].time = 3.0f / 6f;
        colorKey[4].color = Color.blue;
        colorKey[4].time = 4.0f / 6f;
        colorKey[5].color = Color.magenta;
        colorKey[5].time = 5.0f / 6f;
        colorKey[6].color = Color.white;
        colorKey[6].time = 6.0f / 6f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        GradientAlphaKey[] alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);

        return gradient;
    }

    void Update()
    {
        float[] waveform = new float[bufferSize];
        microphone.GetOutputData(waveform, 1);
        
        DrawLine(waveform, 10F);

        float volume = ComputeVolume(waveform);

        DrawVolumeIndicator(1E4F * volume);

        float[] spec = new float[bufferSize];
        microphone.GetSpectrumData(spec, 0, FFTWindow.BlackmanHarris);

        DrawCubes(spec, 1e6F);
    }

    public static float ComputeVolume(float[] data)
    {
        if (data.Length <= 0)
        {  
            return 0;
        }
        // sum of squares
        float sos = 0f;
        float val;
        for( int i = 0; i < data.Length; i++ )
        {
            val = data[ i ];
            sos += val * val;
        }
        // return sqrt of average
        return Mathf.Sqrt( sos / data.Length );
    }

    void DrawVolumeIndicator(float volume)
    {
        volumeIndicator.localScale = volume * Vector3.one;
    }

    void DrawCubes(float[] data, float multiplier)
    {
        for (int i = 0; i < bufferSize; i++)
        {
            cubes[i].transform.localScale = 2f * multiplier * data[i] * Vector3.up + Vector3.one;
        }
    }

    void DrawLine(float[] data, float multiplier)
    {
        Vector3[] positions = new Vector3[bufferSize];
        for (int i = 0; i < bufferSize; i++)
        {
            positions[i] = multiplier * bufferSize * data[i] * Vector3.up + i * Vector3.right - Vector3.forward;
        }
        line.SetPositions(positions);
    }
}
