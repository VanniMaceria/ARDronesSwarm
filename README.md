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
3. **Application layer: Meta Quest application and Nodered interface**
   - Provides a user-friendly interface to control the drone swarm;
   - Transmits commands to the ESP8266 module via a wireless connection.
   - Visualize all the logs.
   - Store locally relevant info in csv about drones status.
<p align="center">
    <img src="https://github.com/user-attachments/assets/f2e8bfa1-41a5-4a8c-be10-136be7a79af1" width=500 height=500>
</p>

## Useful links
- [XR Interaction Toolkit](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.0/manual/index.html)
- [AR development in Unity](https://docs.unity3d.com/6000.0/Documentation/Manual/AROverview.html)
- [Tello SDK](https://dl-cdn.ryzerobotics.com/downloads/Tello/Tello%20SDK%202.0%20User%20Guide.pdf)

## Final result
https://github.com/user-attachments/assets/6fe79fb4-08f0-4afd-966f-d33ef1c3f7a9

<br>

> [!WARNING] 
> **It's mandatory to change the MAC addresses of the devices in esp_master_ap.ino** <br>
> **Unity project must have minimum android sdk version 29**

## Contributors
<a href="https://github.com/VanniMaceria/Progetto-LabIoT/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=VanniMaceria/Progetto-LabIoT" />
</a>

Made with [contrib.rocks](https://contrib.rocks).
