/* ThreeLS
 *
 * Author: Gerard Llorach
 * Date: 20/03/2018
 * Version: 0.0
 *
 * Refer to:
 * G. Llorach, A. Evans, J. Blat, G. Grimm, V. Hohmann. Web-based live
 * speech-driven lip-sync, In Proceedings of VS-Games 2016, September 2016,
 * Barcelona
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ThreeLS Class
internal class ThreeLS
{
    // Lipsync frequency bins
    private float[] refFBins = { 0, 500, 700, 3000, 6000 };
    public float[] freqBins;
    // Lipsync blendshape weights
    public float[] LSbsw = { 0, 0, 0 };
    // Lipsync parameters
    public float threshold;
    public float smoothness;
    public float vocalTractFactor;
    // Energies
    public float[] energies = { 0, 0, 0, 0 };

    // Audio file
    public AudioSource audioSignal;
    // FFT parameters
    private FFTWindow blackmannWindow;
    public int fftSize = 1024;
    public int fs; // Sampling frequency
    private float[] spectrum;
    private float[] smoothSpectrumRaw;
    public float[] smoothSpectrum;

    // Constructor
    public ThreeLS(AudioSource audioIn, float thres, float smooth, float vocalTract)
    {
        // Audio source
        audioSignal = audioIn;
        // Parameters
        threshold = thres;
        smoothness = smooth;
        vocalTractFactor = vocalTract;
        // Initialize
        init();
    }
    // Empty constructor
    public ThreeLS(AudioSource audioIn)
    {
        // Audio source
        audioSignal = audioIn;
        // Parameters
        threshold = 0.5f;
        smoothness = 0.6f;
        vocalTractFactor = 1.0f;
        // Initialize
        init();
    }

    private void init()
    {
        // Init frequency bins
        updateFrequencyBins(vocalTractFactor);
        // Initialize FFT
        spectrum = new float[fftSize / 2];
        smoothSpectrumRaw = new float[fftSize / 2];
        smoothSpectrum = new float[fftSize / 2];
        // FS
        fs = AudioSettings.outputSampleRate;
    }

    // Update frequency bins
    public void updateFrequencyBins(float InVocalTractFactor)
    {
        vocalTractFactor = InVocalTractFactor;
        freqBins = new float[refFBins.Length];
        for (int i = 0; i < refFBins.Length; i++)
        {
            freqBins[i] = refFBins[i] * vocalTractFactor;
        }
    }


    // Update lipsync
    public void updateLS()
    {
        if (!audioSignal)
            return;
        if (!audioSignal.isPlaying)
            return;
        // FFT
        smoothFFT();
        // Energies
        binAnalysis();
        // Lip values
        lipAnalysis();
    }


    // Smooth spectrum
    private void smoothFFT()
    {
        audioSignal.GetSpectrumData(spectrum, 0, FFTWindow.Blackman);
        for (var i = 0; i < spectrum.Length; i++)
        {
            smoothSpectrumRaw[i] = smoothSpectrumRaw[i] * (smoothness) + spectrum[i] * (1 - smoothness);
            smoothSpectrum[i] = 20 * Mathf.Log10(smoothSpectrumRaw[i]);
        }
    }


    // Analyze energies
    private void binAnalysis()
    {
        int halfFFTSize = fftSize / 2; // Half the FFT size

        // Energies inside bins
        for (int bindInd = 0; bindInd < freqBins.Length - 1; bindInd++)
        {
            // Start and end of bins
            int indxIn = Mathf.RoundToInt(freqBins[bindInd] * halfFFTSize / (fs / 2));
            int indxEnd = Mathf.RoundToInt(freqBins[bindInd + 1] * halfFFTSize / (fs / 2));

            // Sum of frequency values
            energies[bindInd] = 0;
            for (int i = indxIn; i < indxEnd; i++)
            {
                // Data goes from -25 to -160?
                float vv = threshold + (smoothSpectrum[i] + 20) / 140;
                // Zeroes negative values
                vv = vv > 0 ? vv : 0;

                energies[bindInd] += vv;
            }
            // Normalize (divide by number of samples)
            energies[bindInd] /= (indxEnd - indxIn);
        }
    }


    // Calculate lipsync blend shape weights
    private void lipAnalysis()
    {
        // Kiss blend shape
        float vv = (0.5f - (energies[2])) * 2;
        if (energies[1] < 0.2f)
            vv = vv * (energies[1] * 5.0f);
        vv = Mathf.Clamp01(vv); // Clamp
        LSbsw[0] = vv;

        // Lips closed blend shape
        vv = energies[3] * 3;
        vv = Mathf.Clamp01(vv); // Clamp
        LSbsw[1] = vv;

        // Mouth open blend shape
        vv = energies[1] * 0.8f - energies[3] * 0.8f;
        vv = Mathf.Clamp01(vv);
        LSbsw[2] = vv;
    }
}
