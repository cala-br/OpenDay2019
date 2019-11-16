# Chatroom

The chatroom will use a local MQTT server (mosquitto) in order to handle the messages.
Messages will be transported using JSON, as a way to keep them platform independent.

# Topics

The MQTT topics used for the communication.

| Topic | Description |
| ----- | ----------- |
| chatroom | The topic that will contain all the messages |


# Messages format

```json
{
    "username"  : "",
    "contents"  : "",
    "timestamp" : ""
}
```

### Example:

```js
/**
 * Publishing
 */ 
mqttClient.publish(
    'chatroom',
    {
        'username' : 'Stephen',
        'contents' : 'hello',
        'timestamp': new Date()
    });

/**
 * On publish received
 *
 * @type {string} topic The chatroom.
 * @type {{
 *      'username' : string,
 *      'contents' : string,
 *      'timestamp': string
 *  }} message
 */
function onPublishReceived(topic, message)
{
    display(message);
}
``` 