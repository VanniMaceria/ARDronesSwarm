import socket

from time import sleep

# Configurazione
UDP_IP = "127.0.0.1"  # IP del server Node-RED
UDP_PORT = 58203      # Porta del nodo UDP
MESSAGE = "speed"    # Stringa da inviare



def udp_speed_sender():
# Creazione del socket UDP
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    # Creazione del socket UDP
    sock2 = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    for i in range(5):
        # Crea il messaggio con il numero della iterazione
        MESSAGE = f"1: speed {i}"  # 'command' seguito dal numero i
        sock.sendto(MESSAGE.encode(), (UDP_IP, UDP_PORT))
        print(f"Messaggio inviato a {UDP_IP}:{UDP_PORT}: {MESSAGE}")
        sleep(1)



if __name__ == "__main__":
    udp_speed_sender()



