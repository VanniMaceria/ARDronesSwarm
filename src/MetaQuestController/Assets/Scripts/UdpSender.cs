using System;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UdpSender : MonoBehaviour
{
    [Header("UDP Settings")]
    public string espIp = "192.168.4.1"; // IP dell'Access Point dell'ESP8266
    public int espPort = 8889;          // Porta UDP dell'ESP8266

    private UdpClient udpClient;
    private float sendInterval = 0.1f; // Intervallo tra i pacchetti (100 ms)
    private float lastSendTime = 0f;

    [Header("XR Input Actions")]
    public InputActionProperty takeoffAction;  // Azione di decollo
    public InputActionProperty landAction;     // Azione di atterraggio
    public InputActionProperty moveAction;     // Azione di movimento sx dx up down (analogico destro)
    public InputActionProperty secondaryMoveAction; //Azione di movimento sopra/sotto e rotazione sul proprio asse (analogico sinistro)

    private bool isFlying = false;

    void Start()
    {
        udpClient = new UdpClient();
    }

    void OnEnable()
    {
        // Abilita le azioni di input
        takeoffAction.action.Enable();
        landAction.action.Enable();
        moveAction.action.Enable();
    }

    void OnDisable()
    {
        // Disabilita le azioni di input
        takeoffAction.action.Disable();
        landAction.action.Disable();
        moveAction.action.Disable();
    }

    void OnDestroy()
    {
        udpClient?.Close();
    }

    void Update()
    {
        // Gestione del decollo
        if (takeoffAction.action.triggered)
        {
            SendCommand("takeoff");
            isFlying = true;
        }

        // Gestione dell'atterraggio
        if (landAction.action.triggered)
        {
            SendCommand("land");
            isFlying = false;
        }

        if (Time.time - lastSendTime > sendInterval)
        {
            if (isFlying)
            {
                // Gestione del movimento con l'analogico destro
                Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
                Vector2 secondaryMoveInput = secondaryMoveAction.action.ReadValue<Vector2>();

                float y = moveInput.y > 0.1f ? moveInput.y : 0.0f;  // Movimento avanti/indietro
                float x = moveInput.x > 0.1f ? moveInput.x : 0.0f;    // Movimento destra/sinistra
                float z = secondaryMoveInput.y > 0.1f ? secondaryMoveInput.y : 0.0f; // Movimento su/giù
                float rotation = secondaryMoveInput.x > 0.1f ? secondaryMoveInput.x : 0.0f; //Rotazione

                // Comando di movimento per il drone (SDK specifico)
                string movementCommand = $"rc {Mathf.RoundToInt(x * 100)} {Mathf.RoundToInt(y * 100)} {Mathf.RoundToInt(z * 100)} {Mathf.RoundToInt(rotation * 100)}";
                SendCommand(movementCommand);
            }

            lastSendTime = Time.time;
        }
    }

    // Metodo da chiamare al clic sui pannelli
    public void SendCommandByPanel(Button button)
    {
        SendCommand(button.gameObject.tag);
    }

    private void SendCommand(string command)
    {
        try
        {
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
