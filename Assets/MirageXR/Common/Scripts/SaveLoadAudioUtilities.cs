using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SaveLoadAudioUtilities
{
    private const string WAV_EXTENSION = ".wav";
    private const string FILE_PREFIX = "file://";
    private const string FORMAT = "{0}{1}";
    
    private static readonly byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
    private static readonly byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
    private static readonly byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
    private static readonly byte[] subChunk = BitConverter.GetBytes(16);
    private static readonly byte[] audioFormat = BitConverter.GetBytes(1);
    private static readonly byte[] bitsPerSample = BitConverter.GetBytes(16);
    private static readonly byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
    
    public static AudioClip LoadAudioFile(string filename, AudioType audioType = AudioType.WAV)
    {
        if (!filename.StartsWith(FILE_PREFIX))
        {
            filename = string.Format(FORMAT, FILE_PREFIX, filename);
        }
        
        using (var request = UnityWebRequestMultimedia.GetAudioClip(filename, audioType)) {
            request.SendWebRequest();

            while (!request.isDone)
            {
                Debug.Log(request.downloadProgress);
            }
            
            if (!request.isNetworkError && !request.isHttpError)
            {
                var audioClip = DownloadHandlerAudioClip.GetContent(request);
                audioClip.LoadAudioData();
                return audioClip;
            }
            Debug.Log($"Loading Audio error: {request.error}");                
            return null;
        }
    }
    
    public static IEnumerator LoadAudioFileAsync(string filename, AudioType audioType, Action<AudioClip> onSuccess, Action<string> onFailure) {
        if (!filename.StartsWith(FILE_PREFIX))
        {
            filename = string.Format(FORMAT, FILE_PREFIX, filename);
        }
        
        using (var request = UnityWebRequestMultimedia.GetAudioClip(filename, audioType)) {
            yield return request.SendWebRequest();

            if (!request.isNetworkError && !request.isHttpError)
            {
                var audioClip = DownloadHandlerAudioClip.GetContent(request);
                audioClip.LoadAudioData();
                onSuccess?.Invoke(audioClip);
            }
            else
            {
                onFailure?.Invoke(request.error);
            }
        }
    }
    
    public static bool Save(string filePath, AudioClip audioClip)
    {
        if (audioClip == null) return false;

        if (!filePath.ToLower().EndsWith(WAV_EXTENSION)) filePath += WAV_EXTENSION;

        var directoryName = Path.GetDirectoryName(filePath);

        if (string.IsNullOrEmpty(directoryName)) return false;
        
        if (!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
        
        using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate))
        {
            WriteHeader(fileStream, audioClip);
            ConvertAndWrite(fileStream, audioClip);
        }
        return true;
    }

    private static void ConvertAndWrite(Stream memStream, AudioClip audioClip)
    {
        var samples = new float[audioClip.samples * audioClip.channels];
        audioClip.GetData(samples, 0);
        var intData = new short[samples.Length];
        var bytesData = new byte[samples.Length * 2];

        const float rescaleFactor = short.MaxValue; //to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short) (samples[i] * rescaleFactor);
        }

        Buffer.BlockCopy(intData, 0, bytesData, 0, bytesData.Length);
        memStream.Write(bytesData, 0, bytesData.Length);
    }

    private static void WriteHeader(Stream fileStream, AudioClip clip)
    {
        var hz = clip.frequency;
        var channels = clip.channels;
        var samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);
        fileStream.Write(riff, 0, 4);

        var chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);
        
        fileStream.Write(wave, 0, 4);
        fileStream.Write(fmt, 0, 4);
        fileStream.Write(subChunk, 0, 4);
        fileStream.Write(audioFormat, 0, 2);

        var numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        var sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        var byteRate = BitConverter.GetBytes(hz * channels * 2);
        fileStream.Write(byteRate, 0, 4);

        var blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        fileStream.Write(bitsPerSample, 0, 2);
        fileStream.Write(dataString, 0, 4);

        var subChunk2 = BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);
    }
}