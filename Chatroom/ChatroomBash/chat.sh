#!/bin/bash
# ITIS Enrico Fermi Open Day 2019
# Lab05 MQTT Chatroom
# Bash client by Giulio Corradini. MIT License

mqttHost="test.mosquitto.org"
mqttPort=1883
globalChat="fermi/od19/global"

username=$USER
#TODO ask for name on launch

#TODO reset from remote location

while true; do
	echo -n '>> '
	read text
	if [[ "$text" == '' ]]; then
		echo "Non puoi inviare un messaggio vuoto..."		
	else
		message="{\"username\":\"$username\",\"contents\":\"$text\",\"timestamp\":$(date +%s)}"
		mosquitto_pub -h "$mqttHost" -p "$mqttPort" -t "$globalChat" -m "$message"
		echo "Messaggio inviato"
	fi

done
