//48:1c:b9:e9:24:0e MAC telloE9240E
//48:1c:b9:e9:24:06 MAC telloE92406
//const char* tello_ip = "192.168.10.1"; IP predefinito del Tello

#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <stdio.h>
#include <ctype.h>

#define DIM 2
#define DIM_PACCHETTO 64

const char* ssid = "Tello-APuntocaldo";
const char* password = "capebomb";

const IPAddress controller_ip(192, 168, 4, 2);
const IPAddress logger_ip(192, 168, 4, 5);
const int controllogger_port = 58203; //porta destinata al MetaQuest e il server di Node-Red
const IPAddress tello_ips[DIM] = {IPAddress(192, 168, 4, 3), IPAddress(192, 168, 4, 4)};
const int tello_port = 8889; //Porta UDP del Tello
char incoming_packet[DIM_PACCHETTO];
char response_packet[DIM_PACCHETTO];

unsigned long last_time_infos = 0;  // Memorizza l'ultimo tempo in cui l'istruzione è stata eseguita
const unsigned long infos_interval = 2000; // Intervallo di 2 secondo (2000 millisecondi)
enum Info{INFO_BATTERY, INFO_TIME}; // Informazioni da chiedere ai droni
Info current_info[DIM] = {INFO_BATTERY, INFO_BATTERY}; // informazioni correnti richieste

WiFiUDP udp;

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

  udp.begin(tello_port);
}

void loop() {
  int packet_size = udp.parsePacket();
  if (packet_size) {
    //ricevi il comando
    udp.read(incoming_packet, DIM_PACCHETTO);

    //termina la stringa
    incoming_packet[packet_size] = '\0';
    Serial.printf("Contenuto del pacchetto: %s\n", incoming_packet);

    bool is_drone = false;
    //forward al controller e al logger delle risposte dei droni
    for(int i = 0; i < DIM; i++) {
      if (udp.remoteIP() == tello_ips[i]) {
        if (isdigit(incoming_packet[0])){
          //se il pacchetto udp corrente contiene inizia con un intero, siamo nel caso in cui si è chiesto uno tra BATTERY e TIME
          sprintf(response_packet, "%d: %s %s", i, infoToString(current_info[i]), incoming_packet);
          current_info[i] = nextInfo(current_info[i]);
        } else {
          //tratta il pacchetto come una risposta generica
          sprintf(response_packet, "%d: %s", i, incoming_packet);
        }

        udp.beginPacket(controller_ip, controllogger_port);
        udp.write(response_packet);
        udp.endPacket();

        udp.beginPacket(logger_ip, controllogger_port);
        udp.write(response_packet);
        udp.endPacket();
        
        is_drone = true;
        break;
      }
    }

    if (!is_drone) {
      //invia il comando ai droni
      for(int i = 0; i < DIM; i++){
        udp.beginPacket(tello_ips[i], tello_port);
        udp.write(incoming_packet);
        udp.endPacket();
      }
    }
  } //fine if controllo pacchetto udp

  //invio delle richieste sulle informazioni di batteria e tempo
  unsigned long current_millis = millis(); // Ottiene il tempo corrente
  // Verifica se è trascorso il delay
  if (current_millis - last_time_infos >= infos_interval) {
    last_time_infos = current_millis; // Aggiorna il tempo dell'ultima esecuzione

    for(int i = 0; i < DIM; i++){
      udp.beginPacket(tello_ips[i], tello_port);
      udp.write(infoToString(current_info[i]));
      udp.write("?");
      udp.endPacket();
    }
  }
}

// Funzione per ottenere il nome come stringa
const char* infoToString(Info info) {
  switch (info) {
    case INFO_BATTERY:
      return "battery";
    case INFO_TIME:
      return "time";
    default:
      return "UNKNOWN";
  }
}

Info nextInfo(Info info) {
  switch (info) {
    case INFO_BATTERY:
      return INFO_TIME;
    case INFO_TIME:
      return INFO_BATTERY;
    default:
      return INFO_BATTERY;
  }
}
