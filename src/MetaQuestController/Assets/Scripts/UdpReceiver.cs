using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UdpReceiver : MonoBehaviour
{
    private readonly IPAddress EspIp = IPAddress.Parse("192.168.4.1"); // IP dell'Access Point dell'ESP8266
    private const int port = 58203; // Porta su cui ricevere i pacchetti UDP

    private UdpClient _udpClient;
    private IPEndPoint _remoteEndPoint;

    [Header("Drones UI")]
    public TextMeshProUGUI[] batteries;
    public TextMeshProUGUI[] timesOfFly;
    public Image[] droneSprites;
    public Image sdkSprite;

    private bool[] isSDK;

    void Start()
    {
        _udpClient = new UdpClient(port); // Bind della porta
        _udpClient.Client.Blocking = false; // Imposta il socket in modalità non bloccante
        _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Qualsiasi IP e porta per la ricezione iniziale

        Debug.Log($"UDP Receiver started on port {port}");

        // Inizializza l'array isSDK con la stessa lunghezza di dronesSprite
        isSDK = new bool[droneSprites.Length];

        // Controlla che tutti i riferimenti siano impostati
        for (int i = 0; i < droneSprites.Length; i++)
        {
            if (droneSprites[i] == null)
            {
                Debug.LogError($"dronesSprite[{i}] non è impostato nell'Inspector!");
            }
        }
    }

    void Update()
    {
        try
        {
            if (_udpClient.Available > 0) // Controlla se ci sono dati disponibili
            {
                byte[] receiveBytes = _udpClient.Receive(ref _remoteEndPoint); // Riceve i dati
                if (_remoteEndPoint.Address.Equals(EspIp)) // Confronta con l'IP dell'Esp
                {
                    string returnData = Encoding.ASCII.GetString(receiveBytes);
                    Debug.Log($"Messaggio ricevuto: {returnData}");
                    Debug.Log($"Da: {_remoteEndPoint.Address}:{_remoteEndPoint.Port}");

                    // Aggiorna lo stato delle immagini e delle info sui droni
                    UpdateDronesUI(returnData);
                    UpdateDronePanels(returnData);
                }
                else
                {
                    Debug.LogWarning($"Pacchetto scartato da {_remoteEndPoint.Address}:{_remoteEndPoint.Port}");
                }
            }
        }
        catch (SocketException socketException)
        {
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

    void UpdateDronesUI(string response)
    {
        for (int i = 0; i < droneSprites.Length; i++)
        {
            // Salta il controllo se il drone è già in modalità SDK
            if (isSDK[i]) continue;

            // Controlla la modalità SDK solo per il drone corrente
            isSDK[i] = CheckSDK(response, i);
            if (isSDK[i] && droneSprites[i] != null)
            {
                droneSprites[i].sprite = sdkSprite.sprite;
                Debug.Log($"Drone {i} aggiornato in modalità SDK.");
            }
        }
    }


    bool CheckSDK(string response, int index)
    {
        // Crea il messaggio atteso per il drone con indice specifico
        string expectedResponse = $"{index}: ok";
        return response == expectedResponse;
    }


    void UpdateDronePanels(string response)
    {
        // Cerca se il messaggio contiene "time"
        if (response.Contains("time"))
        {
            for (int i = 0; i < timesOfFly.Length; i++)
            {
                if (response.Contains($"{i}:"))
                {
                    string timeData = response.Substring(response.IndexOf("time") + 5).Trim(); // Estrai i dati dopo "time"
                    timesOfFly[i].text = timeData;
                    Debug.Log($"Aggiornato tempo di volo per drone {i}: {timeData}");
                    break; // Interrompe il ciclo una volta trovato il drone
                }
            }
        }

        // Cerca se il messaggio contiene "battery"
        if (response.Contains("battery"))
        {
            for (int i = 0; i < batteries.Length; i++)
            {
                if (response.Contains($"{i}:"))
                {
                    string batteryData = response.Substring(response.IndexOf("battery") + 8).Trim(); // Estrai i dati dopo "battery"
                    batteries[i].text = batteryData;
                    Debug.Log($"Aggiornato livello batteria per drone {i}: {batteryData}");
                    break; // Interrompe il ciclo una volta trovato il drone
                }
            }
        }
    }

}
