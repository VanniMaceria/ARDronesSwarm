using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
    [Header("UDP Settings")]
    public int port = 58203; // Porta su cui ricevere i pacchetti UDP

    private UdpClient udpClient;
    private Thread receiveThread;
    private bool isReceiving = false;

    void Start()
    {
        StartReceiving();
    }

    void OnDestroy()
    {
        StopReceiving();
    }

    private void StartReceiving()
    {
        try
        {
            udpClient = new UdpClient(port);
            isReceiving = true;

            receiveThread = new Thread(ReceiveData);
            receiveThread.IsBackground = true;
            receiveThread.Start();

            Debug.Log($"UDP Receiver avviato sulla porta {port}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Errore durante l'avvio del ricevitore UDP: {ex.Message}");
        }
    }

    private void StopReceiving()
    {
        try
        {
            isReceiving = false;

            if (receiveThread != null && receiveThread.IsAlive)
            {
                receiveThread.Abort();
            }

            udpClient?.Close();

            Debug.Log("UDP Receiver fermato.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Errore durante l'arresto del ricevitore UDP: {ex.Message}");
        }
    }

    private void ReceiveData()
    {
        while (isReceiving)
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, port);
                byte[] data = udpClient.Receive(ref remoteEndPoint);
                string receivedMessage = Encoding.UTF8.GetString(data);

                Debug.Log($"Messaggio ricevuto: {receivedMessage} da {remoteEndPoint}");

                //fai cose con il messaggio
                ProcessReceivedMessage(receivedMessage);
            }
            catch (SocketException ex)
            {
                if (isReceiving)
                {
                    Debug.LogError($"Errore socket: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Errore durante la ricezione dei dati UDP: {ex.Message}");
            }
        }
    }

    private void ProcessReceivedMessage(string message)
    {
        Debug.Log($"Elaborazione messaggio: {message}");
    }
}
