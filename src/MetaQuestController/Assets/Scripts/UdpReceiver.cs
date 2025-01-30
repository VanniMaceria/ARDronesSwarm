using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UdpReceiver : MonoBehaviour
{
    private const int Port = 58203; // Porta su cui ricevere i pacchetti UDP

    private UdpClient _udpClient;
    private IPEndPoint _remoteEndPoint = new(IPAddress.Any, 0); // Oggetto per contenere ip e porta di chi ha inviato il pacchetto

    [Header("Drones UI")]
    public TextMeshProUGUI[] batteries;
    public TextMeshProUGUI[] timesOfFly;
    public Image[] droneSprites;
    public Sprite sdkSprite;
    public Sprite errorSprite;

    private void Start()
    {
        _udpClient = new UdpClient(Port); // Bind della porta
        _udpClient.Client.Blocking = false; // Imposta il socket in modalità non bloccante

        Debug.Log($"UDP Receiver started on port {Port}");
    }

    private void Update()
    {
        if (_udpClient.Available > 0) // Controlla se ci sono dati disponibili
        {
            byte[] receiveBytes = _udpClient.Receive(ref _remoteEndPoint); // Riceve i dati
                
            string returnData = Encoding.ASCII.GetString(receiveBytes);
            //Debug.Log($"Messaggio ricevuto: {returnData}");
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

    private void OnApplicationQuit()
    {
        _udpClient.Close();
        Debug.Log("UDP Receiver stopped.");
    }

    private void UpdateDronesUI(int droneIndex, string response)
    {
        switch (response)
        {
            case "ok":
                droneSprites[droneIndex].sprite = sdkSprite;
                break;
            case "error":
                droneSprites[droneIndex].sprite = errorSprite;
                break;
        }
    }

    private void UpdateDronePanels(int droneIndex, string response)
    {
        //time 10s
        //battery 50%

        if (response.Contains("time"))
        {
            //estrai solo il valore
            timesOfFly[droneIndex].text = response.Substring(5);
        }
        else if(response.Contains("battery"))
        {
            //estrai solo il valore
            batteries[droneIndex].text = response.Substring(8);
        }
    }
}
