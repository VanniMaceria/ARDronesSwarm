using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpReceiver : MonoBehaviour
{
    private readonly IPAddress EspIp = IPAddress.Parse("192.168.4.1"); // IP dell'Access Point dell'ESP8266
    private const int port = 58203; // Porta su cui ricevere i pacchetti UDP

    private UdpClient _udpClient;
    private IPEndPoint _remoteEndPoint;

    [Header("Drones UI")]
    public GameObject[] icons;
    public GameObject[] batteries;
    public GameObject[] speeds;
    public GameObject[] wifiSignals;

    void Start()
    {
        _udpClient = new UdpClient(port); // Bind della porta
        _udpClient.Client.Blocking = false; // Imposta il socket in modalità non bloccante
        _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Qualsiasi IP e porta per la ricezione iniziale

        Debug.Log($"UDP Receiver started on port {port}");
    }

    void Update()
    {
        try
        {
            if (_udpClient.Available > 0) // Controlla se ci sono dati disponibili
            {
                byte[] receiveBytes = _udpClient.Receive(ref _remoteEndPoint); // Riceve i dati
                if (_remoteEndPoint.Address.Equals(EspIp)) // Confronta con l'IP specifico
                {
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    Debug.Log($"Messaggio ricevuto: {returnData}");
                    Debug.Log($"Da: {_remoteEndPoint.Address}:{_remoteEndPoint.Port}");
                }
                else
                {
                    Debug.LogWarning($"Pacchetto scartato da {_remoteEndPoint.Address}:{_remoteEndPoint.Port}");
                }
            }
        }
        catch (SocketException socketException)
        {
            // Ignora l'errore "Would block" in modalità non bloccante
            if (socketException.SocketErrorCode != SocketError.WouldBlock)
            {
                Debug.LogError($"Errore UDP: {socketException.Message}");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Errore generico UDP: {e.Message}");
        }
    }

    void OnApplicationQuit()
    {
        _udpClient.Close();
        Debug.Log("UDP Receiver stopped.");
    }
}
