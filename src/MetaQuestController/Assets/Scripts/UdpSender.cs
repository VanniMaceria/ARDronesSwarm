using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UdpSender : MonoBehaviour
{
    private const string EspIp = "192.168.4.1"; // IP dell'Access Point dell'ESP8266
    private const int EspPort = 8889; // Porta UDP dell'ESP8266
    
    private const float SendInterval = 0.05f; // Intervallo tra i pacchetti (50 ms)

    private UdpClient _udpClient;
    private float _lastSendTime;
    
    private bool _isFlying;

    [Header("XR Input Actions")]
    public InputActionProperty takeoffAction;  // Azione di decollo
    public InputActionProperty landAction;     // Azione di atterraggio
    public InputActionProperty moveAction;     // Azione di movimento sx dx up down (analogico destro)
    public InputActionProperty secondaryMoveAction; //Azione di movimento sopra/sotto e rotazione sul proprio asse (analogico sinistro)

    private void Start()
    {
        _udpClient = new UdpClient();
    }

    private void OnEnable()
    {
        // Abilita le azioni di input
        takeoffAction.action.Enable();
        landAction.action.Enable();
        moveAction.action.Enable();
        secondaryMoveAction.action.Enable();
    }

    private void OnDisable()
    {
        // Disabilita le azioni di input
        takeoffAction.action.Disable();
        landAction.action.Disable();
        moveAction.action.Disable();
        secondaryMoveAction.action.Disable();
    }

    private void OnDestroy()
    {
        _udpClient?.Close();
    }

    private void Update()
    {
        // Gestione del decollo
        if (takeoffAction.action.triggered)
        {
            SendCommand("takeoff");
            _isFlying = true;
        }

        // Gestione dell'atterraggio
        if (landAction.action.triggered)
        {
            SendCommand("land");
            _isFlying = false;
        }

        if (Time.time - _lastSendTime > SendInterval)
        {
            if (_isFlying)
            {
                // Gestione del movimento con l'analogico destro
                Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
                Vector2 secondaryMoveInput = secondaryMoveAction.action.ReadValue<Vector2>();

                float y = moveInput.y;  // Movimento avanti/indietro
                float x = moveInput.x;    // Movimento destra/sinistra
                float z = secondaryMoveInput.y; // Movimento su/giù
                float rotation = secondaryMoveInput.x; //Rotazione

                // Comando di movimento per il drone (SDK specifico)
                string movementCommand = 
                    $"rc {Mathf.RoundToInt(x * 100)} {Mathf.RoundToInt(y * 100)} {Mathf.RoundToInt(z * 100)} {Mathf.RoundToInt(rotation * 100)}";
                
                SendCommand(movementCommand);
            }

            _lastSendTime = Time.time;
        }
    }

    // Metodo da chiamare al clic sui pannelli
    public void SendCommandByPanel(Button button)
    {
        SendCommand(button.gameObject.tag);
    }

    private void SendCommand(string command)
    {
        byte[] data = Encoding.UTF8.GetBytes(command);
        _udpClient.Send(data, data.Length, EspIp, EspPort);
    }
}
