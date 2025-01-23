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

const char* controller_mac = "28:39:26:e7:e0:03"; // mac quest 16:dd:e1:9c:98:d8
const char* logger_mac = "d2:c5:e2:11:01:22";
const char* tello_macs[NUM_DRONI] = {"48:1c:b9:e9:24:0e", "48:1c:b9:e9:24:06"};

const int controllogger_port = 58203; //porta di comunicazione col MetaQuest e col server di Node-Red
const int tello_port = 8889; //porta per la comunicazione col Tello
const int battery_port = 8890; //porta per la comunicazione della percentuale di batteria
const int time_port = 8891; //porta per la comunicazione del tempo di volo

const unsigned long infos_interval = 2000; // Intervallo per la richiesta delle informazioni di stato (2000 millisecondi)

char incoming_packet[DIM_PACCHETTO]; // buffer di ricezione
char response_packet[DIM_PACCHETTO]; // buffer di invio

unsigned long last_time_infos = 0;  // Memorizza l'ultimo istante in cui sono state richieste le informazioni di stato

int connected_devices = 0;

IPAddress controller_ip;
IPAddress logger_ip;
IPAddress tello_ips[NUM_DRONI];

WiFiUDP udp_controllogger;
WiFiUDP udp_tello;
WiFiUDP udp_battery;
WiFiUDP udp_time;

void receiveResponse(WiFiUDP& udp, const char* info = "");

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
  uint8_t phyMode = WiFi.getPhyMode();
  Serial.print("Current PHY mode: ");
  if (phyMode == WIFI_PHY_MODE_11B) Serial.println("802.11b");
  else if (phyMode == WIFI_PHY_MODE_11G) Serial.println("802.11g");
  else if (phyMode == WIFI_PHY_MODE_11N) Serial.println("802.11n");

  // Avvia i client udp
  beginUDP(udp_controllogger, controllogger_port, "Controllogger");
  beginUDP(udp_tello, tello_port, "Tello");
  beginUDP(udp_battery, battery_port, "Battery");
  beginUDP(udp_time, time_port, "Time");
}

void loop() {
  receiveCommand();
  receiveResponse(udp_tello);
  receiveResponse(udp_battery, "battery");
  receiveResponse(udp_time, "time");

  sendInfoRequest();

  updateConnectedDevices();
}

// Funzione per ricevere dei comandi dal controller e inoltrarli ai droni
void receiveCommand() {
  if (readPacket(udp_controllogger)) {
    Serial.println(incoming_packet);
    //invia il comando ai droni
    for(int i = 0; i < NUM_DRONI; i++) {
      sendPacket(udp_tello, tello_ips[i], tello_port, incoming_packet);
    }
  }
}

// Funzione per ricevere pacchetti di risposta dai droni e inoltrarli al controller e al logger
void receiveResponse(WiFiUDP& udp, const char* info) {
  if (readPacket(udp)) {
    Serial.println(incoming_packet);
    //forward al controller e al logger delle risposte dei droni
    for(int i = 0; i < NUM_DRONI; i++) {
      if (udp.remoteIP() == tello_ips[i]) {
        if (udp.localPort() == udp_tello.localPort()) {
          //risposta dei droni sulle azioni standard (command, takeoff, land, etc.)
          sprintf(response_packet, "%d: %s", i, incoming_packet);
        } else {
          //risposta dei droni sulle informazioni (battery e time)
          sprintf(response_packet, "%d: %s %s", i, info, incoming_packet);
        }

        Serial.println(response_packet);
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
      sendPacket(udp_battery, tello_ips[i], tello_port, "battery?");
      sendPacket(udp_time, tello_ips[i], tello_port, "time?");
    }
  }
}

// Funzione per aggiornare gli IP dei dispositivi connessi, vengono riconosciuti in base ai mac
void updateConnectedDevices() {
  int new_connected_devices = wifi_softap_get_station_num();
  if (new_connected_devices != connected_devices) {
    connected_devices = 0;

    //azzero tutti gli ip impostati in precedenza
    controller_ip = IPAddress();
    logger_ip = IPAddress();
    for (int i = 0; i < NUM_DRONI; i++) {
      tello_ips[i] = IPAddress();
    }

    //restituisce la lista dei dispositivi connessi all'ESP
    struct station_info* stationList = wifi_softap_get_station_info();

    if (stationList == NULL) {
      if (new_connected_devices == 0) {
        Serial.println("Dispositivi connessi aggiornati");
        Serial.println("Nessun dispositivo connesso trovato.");
      }
      wifi_softap_free_station_info();  //dealloca la lista dei dispositivi connessi all'ESP
      return;
    }

    String macs[new_connected_devices];
    IPAddress ips[new_connected_devices];
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
          }
        }
      }

      connected_devices++;
      stationList = STAILQ_NEXT(stationList, next);
    }

    if (new_connected_devices == connected_devices) {
      Serial.println("Dispositivi connessi aggiornati");
      for (int i = 0; i < connected_devices; i++) {
        Serial.printf("Dispositivo connesso - MAC: %s, IP: %s\n", macs[i].c_str(), ips[i].toString().c_str());
      }
    }

    wifi_softap_free_station_info();  //dealloca la lista dei dispositivi connessi all'ESP
  }
}

// Funzione per convertire il MAC address in stringa
String macToString(const uint8_t* mac) {
  if (mac == nullptr) return String("00:00:00:00:00:00");

  char macStr[18];
  snprintf(macStr, sizeof(macStr), "%02X:%02X:%02X:%02X:%02X:%02X", 
           mac[0], mac[1], mac[2], mac[3], mac[4], mac[5]);
  return String(macStr);
}

/* Funzione per avviare un client udp sulla porta specificata. Stampa sul seriale
il risultato dell'operazione*/
void beginUDP(WiFiUDP& udp, const int port, const char* name) {
  if (udp.begin(port)) {
    Serial.printf("Porta %s avviata correttamente\n", name);
  } else {
    Serial.printf("Errore nell'avvio Porta %s\n", name);
  }
}

// Funzione per inviare un pacchetto udp
void sendPacket(WiFiUDP& udp, const IPAddress& destination_ip, const int port, const char* message) {
  udp.beginPacket(destination_ip, port);
  udp.write(message);
  udp.endPacket();
}

// Funzione per leggere un pacchetto udp dalla porta specificata, se presente
int readPacket(WiFiUDP& udp) {
  int packet_size = udp.parsePacket();
  if (packet_size) {
    //ricevi il comando
    udp.read(incoming_packet, DIM_PACCHETTO);
    //termina la stringa
    incoming_packet[packet_size] = '\0';
  }
  return packet_size;
}
