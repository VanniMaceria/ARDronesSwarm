using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UdpReceiver : MonoBehaviour
{
    private const int port = 58203; // Porta su cui ricevere i pacchetti UDP

    private UdpClient _udpClient;
    private IPEndPoint _remoteEndPoint;

    [Header("Drones UI")]
    public TextMeshProUGUI[] batteries;
    public TextMeshProUGUI[] timesOfFly;
    public Image[] droneSprites;
    public Sprite sdkSprite;
    public Sprite errorSprite;

    void Start()
    {
        _udpClient = new UdpClient(port); // Bind della porta
        _udpClient.Client.Blocking = false; // Imposta il socket in modalità non bloccante
        _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0); // Qualsiasi IP e porta per la ricezione iniziale

        Debug.Log($"UDP Receiver started on port {port}");

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
                
                string returnData = Encoding.ASCII.GetString(receiveBytes);
                Debug.Log($"Messaggio ricevuto: {returnData}");
                //Debug.Log($"Da: {_remoteEndPoint.Address}:{_remoteEndPoint.Port}");

                //prendo l'indice del drone che ha mandato il messaggio
                int index = int.Parse(returnData[0].ToString());
                //estrapolo solo la risposta del drone
                string responseMessage = returnData.Substring(3);

                //Debug.Log($"Drone {index} risponde {responseMessage}");

                // Aggiorna lo stato delle immagini e delle info sui droni
                UpdateDronesUI(index, responseMessage);
                UpdateDronePanels(index, responseMessage);
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

    void UpdateDronesUI(int droneIndex, string response)
    {
        switch (response)
        {
            case "ok":
                droneSprites[droneIndex].sprite = sdkSprite;
                break;
            case "error":
                droneSprites[droneIndex].sprite = errorSprite;
                break;
            default:
                break;
        }
    }

    void UpdateDronePanels(int droneIndex, string response)
    {
        //time 10s
        //battery 50

        if (response.Contains("time"))
        {
            //estrai solo il valore
            response = response.Substring(5);
            timesOfFly[droneIndex].text = response;
    
        }
        else if(response.Contains("battery"))
        {
            //estrai solo il valore
            response = response.Substring(8);
            batteries[droneIndex].text = response;
            
        }
    }


}
