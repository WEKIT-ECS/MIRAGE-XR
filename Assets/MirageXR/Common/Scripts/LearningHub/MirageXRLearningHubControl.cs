using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using MirageXR;

#if !UNITY_EDITOR && UNITY_WSA
using Windows.Networking;
#endif

public class MirageXRLearningHubControl : MonoBehaviour
{
    // Ip of the pc that sensor hub is running on
    private string IPAddress = "192.168.137.1";
    private string port = "12345";
    public bool learningHubReady = true;

    private string areYouReady = "<ARE YOU READY?>";
    private string IamReady = "<I AM READY>";

    /*
    // Use this for initialization
    void Start () {
        //InvokeRepeating("checkLearningHubStatus", 0.0f, 30.0f);
    }
    */

    public async void CheckLearningHubStatus()
    {
        // try
        // {
        //     string response = await sendMessage(areYouReady, true);
        //     learningHubReady = IamReady.Equals(response);
        //     if (learningHubReady)
        //     {
        //         Debug.Log("Learning Hub Connection established");
        //         CancelInvoke();
        //     }
        //     else
        //     {
        //         Debug.Log("Learning Hub Connection failed: "+response);
        //         //learningHubReady = false;
        //     }

        // }
        // catch (Exception e)
        // {
        //     Debug.Log("Learning Hub Connection failed");
        //     //learningHubReady = false;
        // }
    }

    public async void SendMessage(string message)
    {
        Debug.Log("LearningHub.sendMessage 1: IP: " + IPAddress + ", port: " + port + ", message: " + message);
#if !UNITY_EDITOR && UNITY_WSA
        try
        {
            // Create the StreamSocket 
            using (var streamSocket = new Windows.Networking.Sockets.StreamSocket())
            {
                // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                var hostName = new Windows.Networking.HostName(IPAddress);
                Debug.Log("LearningHub.sendMessage 2: Host: "+hostName+", IP: " + IPAddress + ", port: " + port + ", message: " + message);

                await streamSocket.ConnectAsync(hostName, port);

                using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                {
                    using (var streamWriter = new StreamWriter(outputStream))
                    {
                        await streamWriter.WriteLineAsync(message);
                        await streamWriter.FlushAsync();
                    }
                }
                Debug.Log("LearningHub.sendMessage 3: Host: "+hostName+", IP: " + IPAddress + ", port: " + port + ", message: " + message);
            }

        }
        catch (Exception ex)
        {
            Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);
            Debug.Log("Learning hub not found");
        }
#endif
        Debug.Log("MirageXR LearningHUb: sending message to learning hub");

    }

    public async Task<string> SendMessage(string message, bool awaitResponse)
    {
        Debug.Log("sending message to learning hub: " + message);
        string responseMessage = "";
#if !UNITY_EDITOR && UNITY_WSA
        try
        {
            // Create the StreamSocket 
            using (Windows.Networking.Sockets.StreamSocket streamSocket = new Windows.Networking.Sockets.StreamSocket())
            {
                // The server hostname that we will be establishing a connection to. In this example, the server and client are in the same process.
                var hostName = new Windows.Networking.HostName(IPAddress);

                await streamSocket.ConnectAsync(hostName, port);

                using (Stream outputStream = streamSocket.OutputStream.AsStreamForWrite())
                {
                    using (StreamWriter streamWriter = new StreamWriter(outputStream))
                    {
                        await streamWriter.WriteLineAsync(message);
                        await streamWriter.FlushAsync();
                    }

                }

                if (awaitResponse)
                {
                    using (Stream inputStream = streamSocket.InputStream.AsStreamForRead())
                    {
                        using (StreamReader streamReader = new StreamReader(inputStream))
                        {
                            responseMessage = streamReader.ReadToEnd();
                        }
                    }
                }

            }

        }
        catch (Exception ex)
        {
            Windows.Networking.Sockets.SocketErrorStatus webErrorStatus = Windows.Networking.Sockets.SocketError.GetStatus(ex.GetBaseException().HResult);

        }
#endif
        Debug.Log("sent message to learning hub: " + message);
        if (awaitResponse)
        {
            Debug.Log("received response from learning hub: " + responseMessage);
        }
        return responseMessage;
    }
}
