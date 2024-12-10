# Progetto-LabIoT
This is an IoT project developed for the Lab of Internet of Things course at the University of Salerno. This project implements an IoT system for controlling a swarm of drones using a MetaQuest application. Commands issued through the application interface are transmitted to an ESP8266 module configured as an Access Point (AP). The ESP8266 processes and forwards these commands to the drones, enabling simultaneous and coordinated movements.

## Architecture Overview
The system is designed following the three-layer IoT architecture: Sensor Layer, Gateway Layer, and Application Layer, as detailed below:
1. **Sensor layer: DJI Tello drones**
    - The drones receive commands from the ESP8266;
    - After that, they respond and transmit informations to the ESP8266.
2. **Gateway layer: ESP8266 module**
   - Acts as an Access Point to establish communication between the MetaQuest application and the drones;
   - Receives user commands from the application and relays them to the drones.
3. **Application layer: Meta Quest application**
   - Provides a user-friendly interface to control the drone swarm;
   - Transmits commands to the ESP8266 module via a wireless connection.
   
