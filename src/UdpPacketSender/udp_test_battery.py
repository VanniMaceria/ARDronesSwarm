import socket
import random  # Per generare numeri casuali
from time import sleep

# Configurazione
UDP_IP = "127.0.0.1"  # IP del server Node-RED
UDP_PORT = 58203      # Porta del nodo UDP


def udp_battery_sender():
    # Creazione del socket UDP
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    for i in range(5):
        # Genera un valore casuale tra 10 e 90
        battery_value = random.randint(0, 50)
        # Crea il messaggio con il valore casuale
        MESSAGE = f"0: battery {battery_value}"
        # Invia il messaggio al server
        sock.sendto(MESSAGE.encode(), (UDP_IP, UDP_PORT))
        print(f"Messaggio inviato a {UDP_IP}:{UDP_PORT}: {MESSAGE}")
        # Pausa di 1 secondo tra i messaggi
        sleep(1)


if __name__ == "__main__":
    udp_battery_sender()