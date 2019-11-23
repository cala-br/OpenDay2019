import json
from typing import List, Any
from paho.mqtt.client import \
    Client as MQTTClient, MQTTMessage


#region SubscriptionManager
class SubscriptionManager:

    #region Static fields

    BROKER_HOSTNAME = 'broker.hivemq.com'
    USERNAMES_TOPIC = 'messori/fermi/chatroom/usernames'

    #endregion

    #region Constructor
    def __init__(self):
        """
        Initializes the MQTT client
        """
        self.client = MQTTClient()

        self.client.connect(SubscriptionManager.BROKER_HOSTNAME)
        self.client.on_message = self.storeUsernames
        self.client.subscribe(SubscriptionManager.USERNAMES_TOPIC)

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
        if msg.topic != SubscriptionManager.USERNAMES_TOPIC:
            return

        self.usernames = \
            json.loads(msg.payload.decode())

        print(self.usernames)
    #endregion

    #region Register client
    def registerClient(self, username : str) -> bool:
        """
        Registers a client.

        Parameters
        ----------
            username : str

        Returns
        -------
            bool
        """
        if not self.clientExists(username):
            self.usernames\
                .append(username)
        else:
            return False

        self.client\
            .publish(
                topic   = SubscriptionManager.USERNAMES_TOPIC,
                payload = json.dumps(self.usernames),
                retain  = True)

        return True
    #endregion

    #region Deregister client
    def deregisterClient(self, username : str) -> None:
        """
        Deregisters a client.

        Parameters
        ----------
            username : str
        """
        if self.clientExists(username):
            self.usernames\
                .remove(username)

        self.client\
            .publish(
                topic   = SubscriptionManager.USERNAMES_TOPIC,
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
        return username in self.usernames
    #endregion

#endregion
