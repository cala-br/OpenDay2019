# Chatroom

The chatroom will use a local MQTT server (mosquitto) in order to handle the messages.
Messages will be transported using JSON, as a way to keep them platform independent.

# Topics

The MQTT topics used for the communication.

| Topic | Description |
| ----- | ----------- |
| rooms/room_name | [room_name]() will represent each separate room, and each room will contain its own messages |


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
    'rooms/OpenDay',
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
    // Displaying/pushing the message
    // into the right chatroom
    chatrooms[topic].push(message);
}
``` 