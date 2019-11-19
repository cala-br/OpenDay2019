import json
from typing import List, Any
from paho.mqtt.client import \
    Client as MQTTClient, MQTTMessage


#region Unsubscriber
class Unsubscriber:

    #region Static fields

    BROKER_HOSTNAME = ''
    USERNAMES_TOPIC = 'chatroom/usernames'

    #endregion

    #region Constructor
    def __init__(self):
        """
        Initializes the MQTT client
        """
        self.client = MQTTClient()

        self.client.connect(Unsubscriber.BROKER_HOSTNAME)
        self.client.on_message = self.storeUsernames
        self.client.subscribe(Unsubscriber.USERNAMES_TOPIC)

        self.usernames : List[str] = []

        self.client.loop_start()
    #endregion

    #region Store usernames
    def storeUsernames(self,
        client : MQTTClient, 
        data   : Any,
        msg    : MQTTMessage) -> None:
        """
        Stores the usernames. 
        Bound to the client's on_message event.

        Parameters
        ----------
            msg : MQTTMessage
        """    
        if msg.topic != Unsubscriber.USERNAMES_TOPIC:
            return

        self.usernames = \
            json.loads(msg.payload.decode())
    #endregion

    #region Deregister client
    def deregisterClient(self, username : str) -> None:
        """
        Deregisters a client.

        Parameters
        ----------
            username : str
        """
        self.usernames\
            .remove(username)

        self.client\
            .publish(
                topic   = Unsubscriber.USERNAMES_TOPIC,
                payload = json.dumps(self.usernames),
                retain  = True)
    #endregion

    #region Client exists
    def clientExists(self, username : str) -> bool:
        """
        Checks if a client exists.

        Parameters
        ----------
            username : str

        Returns
        -------
            bool
        """
    #endregion

#endregion


#region Main
def main():
    unsub = Unsubscriber()

#endregion


if __name__ == "__main__":
    main()