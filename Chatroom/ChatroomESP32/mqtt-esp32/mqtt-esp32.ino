#include <WiFi.h>
#include <PubSubClient.h>

WiFiClient wifiSTA;
PubSubClient client;

void setup() {
  //initialize WiFi client --> wifiSTA
  client.setClient(wifiSTA);
  client.setServer("test.mosquitto.org", 1883);
  while(!client.connect("esp-di-mecat")) {
    Serial.println("Non riesco a connettermi al broker, riprovo in 5 secondi");
    delay(5000);
  }
}

void loop() {
  char msg[1025];
  //ask for message
  publishMessage(msg);
}

int publishMessage(char* payload) {
  char message[1080] = "";
  sprintf(message, "{\"username\":\"mecatesp\",\"contents\":\"%1024s\",\"timestamp\":\"-1\"}", payload);
  return client.publish("chatroom", payload);
}

