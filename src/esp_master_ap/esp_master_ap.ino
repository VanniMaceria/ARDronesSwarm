//48:1c:b9:e9:24:0e MAC telloE9240E
//48:1c:b9:e9:24:06 MAC telloE92406
//const char* tello_ip = "192.168.10.1"; IP predefinito del Tello

#include <ESP8266WiFi.h>
#include <WiFiUdp.h>
#include <stdio.h>

#define DIM 2
#define DIM_PACCHETTO 32

const char* ssid = "Tello-APuntocaldo";
const char* password = "capebomb";

const IPAddress controller_ip(192, 168, 4, 2);
const IPAddress logger_ip(192, 168, 4, 5);
const int controllogger_port = 58203; //porta destinata al MetaQuest e il server di Node-Red
const IPAddress tello_ips[DIM] = {IPAddress(192, 168, 4, 3), IPAddress(192, 168, 4, 4)};
const int tello_port = 8889; //Porta UDP del Tello
char incoming_packet[DIM_PACCHETTO];
char response_packet[DIM_PACCHETTO];

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
    //forward al controller dei messaggi dei droni
    for(int i = 0; i < DIM; i++) {
      if (udp.remoteIP() == tello_ips[i]) {
        sprintf(response_packet, "%d: %s", i, incoming_packet);

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
  }
}
