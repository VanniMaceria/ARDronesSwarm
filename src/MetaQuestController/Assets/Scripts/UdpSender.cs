using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class UdpSender : MonoBehaviour
{
    [Header("UDP Settings")]
    public string espIp = "192.168.4.1"; // IP dell'Access Point dell'ESP8266
    public int espPort = 8889;          // Porta UDP dell'ESP8266

    private UdpClient udpClient;

    void Start()
    {
        udpClient = new UdpClient();
    }

    void OnDestroy()
    {
        udpClient?.Close();
    }

    // Metodo da chiamare al clic sul pannello
    public void SendCommandByPanel(GameObject panel)
    {
        try
        {
            string command = "";

            // Determina il comando in base al tag del pannello
            switch (panel.tag)
            {
                case "command":
                    command = "command"; // Attiva modalità SDK
                    break;

                case "takeoff":
                    command = "takeoff"; // Decollo del drone
                    break;

                case "land":
                    command = "land"; // Atterraggio del drone
                    break;

                default:
                    Debug.LogError($"Tag sconosciuto: {panel.tag}");
                    return;
            }

            byte[] data = Encoding.UTF8.GetBytes(command);
            udpClient.Send(data, data.Length, espIp, espPort);

            Debug.Log($"Messaggio inviato a {espIp}:{espPort}: {command}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Errore durante l'invio del messaggio UDP: {ex.Message}");
        }
    }
}
