# Chatroom

The chatroom will use a local MQTT server (mosquitto) in order to handle the messages.
Messages will be transported using JSON, as a way to keep them platform independent.

# Topics

The MQTT topics used for the communication.

| Topic | Description |
| ----- | ----------- |
| [chatroom/](###Normal-message) | The topic that will contain all the messages |
| [chatroom/usernames](###Username-registration-message) | The topic that will contain the list of used usernames. The message must be retained. |
| [chatroom/private-messages/recipient](###Direct-messages) | The topic used when sending direct messages. **/recipient** is the recipient's username. |


# Messages format

### Normal message 
```js
mqttClient.publish(
    "chatroom/",
    {
        "username"  : "",
        "contents"  : "",
        "timestamp" : ""
    });
```

### Username registration message 
```js
mqttClient.publish(
    "chatroom/usernames",
    [
        "username1", "usernameN"
    ]);
```

### Direct messages
```js
mqttClient.publish(
    `chatroom/private-messages/${recipient}`,
    {
        "username"  : "", 
        "contents"  : "",
        "timestamp" : ""
    });
```


# Examples

_*All the examples are written in pseudocode_

### Publishing message

```js
/**
 * Publishing
 * 
 * For direct messages, the topic will be 'chatroom/private-messages/recipient'.
 */ 
mqttClient.publish(
    'chatroom/',
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

### Direct chat
```js
/**
 * Subscribes the client to its own topic,
 * in order to receive direct messages.
 */
function initOwnTopic()
{
    const OWN_TOPIC = 
        `chatroom/private-messages/${ownUsername}`;

    mqttClient
        .subscribe(OWN_TOPIC);
        .onPublishReceived = (topic, msg) =>
        {
            if (topic == OWN_TOPIC)
            {
                if (!privateChatExists(msg.username))
                    createPrivateChat(msg.username);

                showPrivateMessage(msg);
            }
        };
}
```

### Checking for username

```js
const USERNAMES_TOPIC = "chatroom/usernames";

/**
 * Checks if the username is taken or not.
 * 
 * If it is, the client must show an error and ask
 * for another, otherwise it must publish back the usernames
 * with its own added to the list.
 */
function checkIfUsernameTaken()
{
    mqttClient.onPublishReceived = (topic, msg) =>
    {
        if (topic == USERNAMES_TOPIC)
        {
            let usernames = JSON.parse(msg);

            // If the username is already
            // registered
            let isUsernameTaken = 
                usernames.find(username)

            if (isUsernameTaken)
            {
                usernameTakenError();
                return;
            }

            usernames.push(username);

            // Unsubscribing to avoid
            // recursion
            mqttClient.unsubscribe(USERNAMES_TOPIC);

            // Publishing back the updated
            // usernames list
            mqttClient.publish(
            {
                topic  : USERNAMES_TOPIC,
                message: JSON.encode(usernames),
                retain : true
            });
        }
    }

    mqttClient.subscribe(USERNAMES_TOPIC);
}
```