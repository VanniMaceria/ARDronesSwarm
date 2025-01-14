using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
    private readonly IPAddress EspIp = IPAddress.Parse("192.168.4.1"); // IP dell'Access Point dell'ESP8266
    private const int port = 58203; // Porta su cui ricevere i pacchetti UDP

    private UdpClient _udpClient;
    private Thread _receiveThread;  //serve a non bloccare il Thread principale della scena Unity
    private bool _isRunning;

    void Start()
    {
        _udpClient = new UdpClient(port); // Bind della porta
        _isRunning = true;
        _receiveThread = new Thread(ReceiveData);
        _receiveThread.IsBackground = true; // Il thread si chiude automaticamente con l'applicazione
        _receiveThread.Start();

        Debug.Log($"UDP Receiver started on port {port}");
    }

    private void ReceiveData()
    {
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Qualsiasi IP e porta per la ricezione iniziale

        while (_isRunning)
        {
            try
            {
                // Ricevi pacchetti UDP
                byte[] receiveBytes = _udpClient.Receive(ref remoteEndPoint); // Popola remoteEndPoint con i dettagli del mittente

                // Controlla che il mittente sia l'IP specificato
                if (remoteEndPoint.Address.Equals(EspIp)) // Confronta con IP specifico
                {
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    Debug.Log($"Messaggio ricevuto: {returnData}");
                    Debug.Log($"Da: {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                }
                else
                {
                    Debug.LogWarning($"Pacchetto scartato da {remoteEndPoint.Address}:{remoteEndPoint.Port}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Errore UDP: {e.Message}");
            }
        }
    }


    void OnApplicationQuit()
    {
        _isRunning = false;
        _udpClient.Close();
        if (_receiveThread != null && _receiveThread.IsAlive)
        {
            _receiveThread.Abort();
        }
        Debug.Log("UDP Receiver stopped.");
    }
}
