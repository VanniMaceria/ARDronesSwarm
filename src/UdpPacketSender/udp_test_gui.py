import socket
import random
from time import sleep
import tkinter as tk
from tkinter import ttk

# Configurazione server UDP
UDP_IP = "127.0.0.1"  # Indirizzo IP del server
UDP_PORT = 58203  # Porta del server

def send_udp_packets(drone_id, message_type):
    """Invia pacchetti UDP in base al drone selezionato e al tipo di messaggio."""
    # Creazione del socket UDP
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    for i in range(10):
        # Genera un valore incrementale per il tempo e un valore casuale per la batteria
        time_value = i * 2
        battery_value = random.randint(0, 100)

        if message_type in ["time", "both"]:
            MESSAGE = f"{drone_id}: time {time_value}s"
            sock.sendto(MESSAGE.encode(), (UDP_IP, UDP_PORT))
            log_text.insert(tk.END, f"Messaggio inviato a {UDP_IP}:{UDP_PORT}: {MESSAGE}\n")

        if message_type in ["battery", "both"]:
            MESSAGE2 = f"{drone_id}: battery {battery_value}"
            sock.sendto(MESSAGE2.encode(), (UDP_IP, UDP_PORT))
            log_text.insert(tk.END, f"Messaggio inviato a {UDP_IP}:{UDP_PORT}: {MESSAGE2}\n")

        log_text.see(tk.END)  # Scorri verso il basso
        root.update()

        # Pausa di 1 secondo tra i messaggi
        sleep(1)

    log_text.insert(tk.END, "Invio completato.\n")
    log_text.see(tk.END)


# Creazione dell'interfaccia grafica
root = tk.Tk()
root.title("UDP sender")

# Variabile per selezionare il drone
drone_var = tk.StringVar(value="0")

# Variabile per selezionare il tipo di messaggio
message_var = tk.StringVar(value="both")

# Frame per la selezione del drone
frame_drone = ttk.LabelFrame(root, text="Seleziona il Drone")
frame_drone.pack(padx=10, pady=10, fill="x")

# Bottoni radio per selezionare il drone
ttk.Radiobutton(frame_drone, text="Drone 0", variable=drone_var, value="0").pack(anchor="w", padx=5, pady=2)
ttk.Radiobutton(frame_drone, text="Drone 1", variable=drone_var, value="1").pack(anchor="w", padx=5, pady=2)

# Frame per la selezione del tipo di messaggio
frame_message = ttk.LabelFrame(root, text="Seleziona il Tipo di Messaggio")
frame_message.pack(padx=10, pady=10, fill="x")

# Bottoni radio per selezionare il tipo di messaggio
ttk.Radiobutton(frame_message, text="time", variable=message_var, value="time").pack(anchor="w", padx=5, pady=2)
ttk.Radiobutton(frame_message, text="battery", variable=message_var, value="battery").pack(anchor="w", padx=5, pady=2)
ttk.Radiobutton(frame_message, text="autonomy", variable=message_var, value="both").pack(anchor="w", padx=5, pady=2)

# Frame per i pulsanti di controllo
frame_controls = ttk.Frame(root)
frame_controls.pack(pady=10, fill="x")

# Pulsante per inviare i pacchetti
send_button = ttk.Button(frame_controls, text="Invia Pacchetti", command=lambda: send_udp_packets(drone_var.get(), message_var.get()))
send_button.pack(side="left", padx=5)


# Area di log per mostrare i messaggi inviati
log_frame = ttk.LabelFrame(root, text="Log dei Messaggi")
log_frame.pack(padx=10, pady=10, fill="both", expand=True)

log_text = tk.Text(log_frame, height=15, wrap="word", state="normal")
log_text.pack(padx=5, pady=5, fill="both", expand=True)

# Barra di scorrimento per il log
scrollbar = ttk.Scrollbar(log_frame, orient="vertical", command=log_text.yview)
scrollbar.pack(side="right", fill="y")
log_text.config(yscrollcommand=scrollbar.set)

# Avvio dell'interfaccia grafica
root.mainloop()