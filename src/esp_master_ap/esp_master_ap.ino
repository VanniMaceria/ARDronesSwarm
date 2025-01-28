//48:1c:b9:e9:24:0e MAC telloE9240E
//48:1c:b9:e9:24:06 MAC telloE92406
//const char* tello_ip = "192.168.10.1"; IP predefinito del Tello

#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <stdio.h>

#define NUM_DRONI 2
#define DIM_PACCHETTO 32

const char* ssid = "Tello-APuntocaldo";
const char* password = "capebomb";

const char* controller_mac = "DE:99:8D:44:FC:CB"; // mac quest DE:99:8D:44:FC:CB  //mac pc Andrea 28:39:26:e7:e0:03
const char* logger_mac = "1E:65:3F:59:D8:41"; //mac pc Amedeo
const char* tello_macs[NUM_DRONI] = {"48:1c:b9:e9:24:0e", "48:1c:b9:e9:24:06"};

const uint16_t controllogger_port = 58203; //porta di comunicazione col MetaQuest e col server di Node-Red
const uint16_t tello_port = 8889; //porta per la comunicazione col Tello

const unsigned long infos_interval = 2000; // Intervallo per la richiesta delle informazioni di stato (2000 millisecondi)

char incoming_packet[DIM_PACCHETTO]; // buffer di ricezione
char response_packet[DIM_PACCHETTO]; // buffer di invio

unsigned long last_time_infos = 0;  // Memorizza l'ultimo istante in cui sono state richieste le informazioni di stato

uint8 connected_devices = 0;

IPAddress controller_ip;
IPAddress logger_ip;
IPAddress tello_ips[NUM_DRONI];

WiFiUDP udp_controllogger;
WiFiUDP udp_tello;

void setup() {
  Serial.begin(9600);

  // Configura l'ESP8266 come Access Point
  Serial.println("Configurazione dell'Access Point...");
  WiFi.softAP(ssid, password); // Crea l'AP con il nome e la password specificati

  // Stampa l'indirizzo IP dell'Access Point
  IPAddress ip = WiFi.softAPIP();
  Serial.print("Access Point creato. IP address: ");
  Serial.println(ip);

  // Recupera la modalità corrente
  WiFiPhyMode_t phyMode = WiFi.getPhyMode();
  Serial.print("Current PHY mode: ");
  if (phyMode == WIFI_PHY_MODE_11B) Serial.println("802.11b");
  else if (phyMode == WIFI_PHY_MODE_11G) Serial.println("802.11g");
  else if (phyMode == WIFI_PHY_MODE_11N) Serial.println("802.11n");

  // Avvia i client udp
  beginUDP(udp_controllogger, controllogger_port, "Controllogger");
  beginUDP(udp_tello, tello_port, "Tello");
}

void loop() {
  receiveCommand();
  receiveResponse();

  sendInfoRequest();

  updateConnectedDevices();
}

// Funzione per ricevere dei comandi dal controller e inoltrarli ai droni
void receiveCommand() {
  if (readPacket(udp_controllogger)) {
    //invia il comando ai droni
    for(int i = 0; i < NUM_DRONI; i++) {
      sendPacket(udp_tello, tello_ips[i], tello_port, incoming_packet);
    }
  }
}

// Funzione per ricevere pacchetti di risposta dai droni e inoltrarli al controller e al logger
void receiveResponse() {
  int length = readPacket(udp_tello);
  if (length) {
    // forward al controller e al logger delle risposte dei droni
    for(int i = 0; i < NUM_DRONI; i++) {
      // trova il drone che ha inviato la risposta, per sapere il suo id (indice)
      if (udp_tello.remoteIP() == tello_ips[i]) {
        const char* info;

        if (incoming_packet[length - 1] == 's') {
          info = "time ";
        } else if (incoming_packet[length - 1] >= '0' && incoming_packet[length - 1] <= '9') {
          info = "battery ";
        } else {
          info = "";
        }

        sprintf(response_packet, "%d: %s%s", i, info, incoming_packet);
        
        sendPacket(udp_controllogger, controller_ip, controllogger_port, response_packet);
        sendPacket(udp_controllogger, logger_ip, controllogger_port, response_packet);
        
        break;
      }
    }
  }
}

/* Funzione per inviare pacchetti di richiesta delle informazioni di stato ai droni.
Invia le richieste solo se è passato un intervallo di tempo maggiore o uguale a infos_interval*/
void sendInfoRequest() {
  //invio delle richieste sulle informazioni di batteria e tempo
  unsigned long current_millis = millis(); // Ottiene il tempo corrente
  // Verifica se è trascorso il delay
  if (current_millis - last_time_infos >= infos_interval) {
    last_time_infos = current_millis; // Aggiorna il tempo dell'ultima esecuzione

    for(int i = 0; i < NUM_DRONI; i++){
      sendPacket(udp_tello, tello_ips[i], tello_port, "battery?");
      sendPacket(udp_tello, tello_ips[i], tello_port, "time?");
    }
  }
}

// Funzione per aggiornare gli IP dei dispositivi connessi, vengono riconosciuti in base ai mac
void updateConnectedDevices() {
  uint8 new_connected_devices = wifi_softap_get_station_num();

  // Se il numero di dispositivi connessi non è cambiato non c'è bisogno di fare nulla
  if (new_connected_devices == connected_devices) return;

  connected_devices = 0;

  //azzero tutti gli ip impostati in precedenza
  controller_ip = IPAddress();
  logger_ip = IPAddress();
  for (int i = 0; i < NUM_DRONI; i++) {
    tello_ips[i] = IPAddress();
  }

  //restituisce la lista dei dispositivi connessi all'ESP
  struct station_info* stationList = wifi_softap_get_station_info();

  // Array per memorizzare i dispositivi connessi e stamparli successivamente
  String macs[new_connected_devices];
  IPAddress ips[new_connected_devices];

  // Scorro la lista, aggiornando gli ip in base ai mac e aggiornando il numero di dispositivi effettivamente rilevati
  while (stationList != NULL) {
    String mac = macToString(stationList->bssid);
    IPAddress ip = IPAddress((uint32_t)stationList->ip.addr);

    macs[connected_devices] = mac;
    ips[connected_devices] = ip;

    if (mac.equalsIgnoreCase(controller_mac)) {
      controller_ip = ip;
    } else if (mac.equalsIgnoreCase(logger_mac)) {
      logger_ip = ip;
    } else {
      for (int i = 0; i < NUM_DRONI; i++) {
        if (mac.equalsIgnoreCase(tello_macs[i])) {
          tello_ips[i] = ip;
          break;
        }
      }
    }

    connected_devices++;
    stationList = STAILQ_NEXT(stationList, next);
  }

  // Se arrivo a convergenza, cioè rilevo tutti i dispositivi connessi, li stampo
  if (new_connected_devices == connected_devices) {
    Serial.println("Dispositivi connessi aggiornati");
    if (connected_devices == 0) {
      Serial.println("Nessun dispositivo connesso trovato");
    }
    for (int i = 0; i < connected_devices; i++) {
      Serial.printf("Dispositivo connesso - MAC: %s, IP: %s\n", macs[i].c_str(), ips[i].toString().c_str());
    }
  }

  wifi_softap_free_station_info();  //dealloca la lista dei dispositivi connessi all'ESP
}

// Funzione per convertire il MAC address in stringa
String macToString(const uint8* mac) {
  char macStr[18];
  snprintf(macStr, sizeof(macStr), "%02X:%02X:%02X:%02X:%02X:%02X", 
           mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);
  return String(macStr);
}

/* Funzione per avviare un client udp sulla porta specificata. Stampa sul seriale
il risultato dell'operazione*/
void beginUDP(WiFiUDP& udp, const uint16_t port, const char* name) {
  if (udp.begin(port)) {
    Serial.printf("Porta %s avviata correttamente\n", name);
  } else {
    Serial.printf("Errore nell'avvio Porta %s\n", name);
  }
}

// Funzione per inviare un pacchetto udp
void sendPacket(WiFiUDP& udp, const IPAddress& destination_ip, const uint16_t destination_port, const char* message) {
  udp.beginPacket(destination_ip, destination_port);
  udp.write(message);
  udp.endPacket();
}

// Funzione per leggere un pacchetto udp dalla porta specificata, se presente
int readPacket(WiFiUDP& udp) {
  int packet_size = udp.parsePacket();
  if (packet_size) {
    //ricevi il comando
    udp.read(incoming_packet, DIM_PACCHETTO);
    //termina la stringa. Se termina con uno \r\n, allora li elimina
    packet_size = incoming_packet[packet_size - 2] == '\r' ? packet_size - 2 : packet_size;
    incoming_packet[packet_size] = '\0';
  }
  return packet_size;
}
