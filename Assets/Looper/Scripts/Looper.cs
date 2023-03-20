using UnityEngine;
using System.IO;
using System;
using System.Collections;

//an attempt at recording audio into a wav file - this works but the encoding is poor and sounds bad; this is ideal for improving when one wants to export their loops
public class Looper : MonoBehaviour
{
    private int bufferSize;
    private int numBuffers;
    private int outputRate = 44100;
    private string fileName = Application.dataPath + "/recTest";
    private string fileExt = ".wav";
    private int headerSize = 44; //default for uncompressed wav
    private bool recOutput;
    private FileStream fileStream;
    private int totalLoops = 0;


    private WWW www;

    private void Awake()
    {
        AudioSettings.outputSampleRate = outputRate;
    }

    private void Start()
    {
        AudioSettings.GetDSPBufferSize(out bufferSize, out numBuffers);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("rec");
            if (!recOutput)
            {
                string name = fileName + totalLoops + fileExt;
                StartWriting(name);
                recOutput = true;
            }
            else
            {
                recOutput = false;
                WriteHeader();
                string name = fileName + totalLoops + fileExt;
                StartCoroutine(StartSong(name));
                totalLoops++;
                //add the waveFile 
                Debug.Log("rec stop");
            }
        }
    }

    IEnumerator StartSong(string path)
    {
        www = new WWW(path);
        if (www.error != null)
        {
            Debug.Log(www.error);
        }
        else
        {
            AudioClip cl = www.GetAudioClip();
            while (cl.loadState != AudioDataLoadState.Loaded)
                yield return new WaitForSeconds(0.1f);

            var obj = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);
            var source = obj.AddComponent<AudioSource>();
            source.clip = cl;
            source.loop = true;
            source.Play();
        }
    }

    private void StartWriting(string name)
    {
        fileStream = new FileStream(name, FileMode.Create);

        byte emptyByte = new byte();

        for (int i = 0; i < headerSize; i++) //preparing the header
        {
            fileStream.WriteByte(emptyByte);
        }
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (recOutput)
        {
            ConvertAndWrite(data); //audio data is interlaced
        }
    }

    private void ConvertAndWrite(float[] dataSource)
    {
        Int16[] intData = new Int16[dataSource.Length];
        //converting in 2 steps : float[] to Int16[], //then Int16[] to Byte[]

        Byte[] bytesData = new Byte[dataSource.Length * 2];
        //bytesData array is twice the size of
        //dataSource array because a float converted in Int16 is 2 bytes.

        int rescaleFactor = 32767; //to convert float to Int16

        for (int i = 0; i < dataSource.Length; i++)
        {
            intData[i] = (short)(dataSource[i] * rescaleFactor);
            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    void WriteHeader()
    {
        fileStream.Seek(0, SeekOrigin.Begin);

        byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        ushort two = 2;
        ushort one = 1;

        byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        byte[] numChannels = BitConverter.GetBytes(two);
        fileStream.Write(numChannels, 0, 2);

        byte[] sampleRate = BitConverter.GetBytes(outputRate);
        fileStream.Write(sampleRate, 0, 4);

        byte[] byteRate = BitConverter.GetBytes(outputRate * 4);
        // sampleRate * bytesPerSample * number of channels, here 44100 * 2 * 2

        fileStream.Write(byteRate, 0, 4);

        ushort four = 4;
        byte[] blockAlign = BitConverter.GetBytes(four);
        fileStream.Write(blockAlign, 0, 2);

        ushort sixteen = 16;
        byte[] bitsPerSample = BitConverter.GetBytes(sixteen);
        fileStream.Write(bitsPerSample, 0, 2);

        byte[] dataString = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(dataString, 0, 4);

        byte[] subChunk2 = BitConverter.GetBytes(fileStream.Length - headerSize);
        fileStream.Write(subChunk2, 0, 4);

        fileStream.Close();
    }
}