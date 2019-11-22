#!/bin/bash
# ITIS Enrico Fermi Open Day 2019
# Lab05 MQTT Chatroom
# Bash client by Giulio Corradini. MIT License

mqttHost="test.mosquitto.org"
mqttPort=1883
globalChat="chatroom"

registerServer="register-server.fermi.mo.it"#"localhost:40000"

function registerUsername {
	uname=$1
	response=$(curl -X POST -d "$uname" "$registerServer/register" -w %{http_code} 2> /dev/null)
	if [ "$response" == 200 ]; then
		echo $uname
	else
		uname="$uname$(date +%s)"
		curl -X POST -d "$uname" "$registerServer/register" &> /dev/null
		echo $uname
	fi
}

clear

echo -n "Come ti chiami? "
tput smul
read username
username=$(registerUsername $username)
clear

function printUsername {
	tput cup 1 1
	tput rmul
	tput bold
	echo -n "Nickname: $username"
	tput sgr0
}

function newMessage {
	tput cup $(($(tput lines) - 1)) 0
	echo -n '>> '
	read text
	if [[ "$text" == '' ]]; then
		tput setaf 1
		echo "Non puoi inviare un messaggio vuoto..."		
		tput sgr0
	else
		message="{\"username\":\"$username\",\"contents\":\"$text\",\"timestamp\":$(date +%s)}"
		mosquitto_pub -h "$mqttHost" -p "$mqttPort" -t "$globalChat" -m "$message"
		echo "Messaggio inviato"
	fi
	echo -en "\n\n"
}

function unregisterUsername {
	curl -X POST -d "$uname" "$registerServer/deregister"
	tput sgr0
	exit
}

trap unregisterUsername SIGINT

#TODO reset from chatroom controller

while true; do
	printUsername
	newMessage
done
