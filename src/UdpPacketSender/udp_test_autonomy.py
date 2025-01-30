import socket
import random
from time import sleep

# Configurazione server UDP
UDP_IP = "127.0.0.1"  # Indirizzo IP del server
UDP_PORT = 58203  # Porta del server


def udp_autonomy_sender():
    # Creazione del socket UDP
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    for i in range(10):
        # Genera un valore incrementale per il tempo e un valore casuale per la batteria
        time_value = i * 2
        battery_value = random.randint(0, 100)

        # Creazione dei messaggi
        MESSAGE = f"0: time {time_value}s"
        MESSAGE2 = f"0: battery {battery_value}"

        # Invio dei messaggi al server
        sock.sendto(MESSAGE.encode(), (UDP_IP, UDP_PORT))
        sock.sendto(MESSAGE2.encode(), (UDP_IP, UDP_PORT))

        # Stampa dei messaggi inviati
        print(f"Messaggio inviato a {UDP_IP}:{UDP_PORT}: {MESSAGE}")
        print(f"Messaggio inviato a {UDP_IP}:{UDP_PORT}: {MESSAGE2}")

        # Pausa di 1 secondo tra i messaggi
        sleep(1)


# Esegui la funzione
udp_autonomy_sender()