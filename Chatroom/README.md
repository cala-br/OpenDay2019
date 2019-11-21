# Chatroom

The chatroom will use a local MQTT server (mosquitto) in order to handle the messages.
Messages will be transported using JSON, as a way to keep them platform independent.

# Topics

The MQTT topics used for the communication.

| Topic | Description |
| ----- | ----------- |
| [chatroom](###Normal-message) | The topic that will contain all the messages |
| [chatroom/usernames](###Username-registration-message) | The topic that will contain the list of used usernames. The message must be retained. This is managed by the subscription-manager server |
| [chatroom/private-messages/recipient](###Direct-messages) | The topic used when sending direct messages. **/recipient** is the recipient's username. |

# Server requests

The requests that the subscription-manager server supports.

| Request | Type | Returns | Data | Description |
| ------- | ---- | ------- | ---- | ----------- |
| [/deregister](###Registering username) | POST | 200 | `string` | Deregisters a client, removing it from the usernames list. |
| [/register](###Deregistering username) | POST | 200 Done <br> 400 Fail | `string` | **Planned**, registers a client by adding it into the usernames list. |
| [/getNames](###Getting usernames) | GET | \["n1", "n2"] |  | Get the list of all currently registered usernames |

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
```python
usernames = []

mqttClient.publish(
    "chatroom/usernames",
    json.dumps(usernames))
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

### Registering username

```js
/**
 * Checks if the username is taken or not.
 *
 * If it is, the client must show an error and ask
 * for another, otherwise it must publish back the usernames
 * with its own added to the list.
 */
function RegisterUsername(username)
{
    $.ajax(
    {
        method: 'POST',
        url   : 'http://subscription-manager.local:40000/register',
        data  : username
    })
    .done(_ => alert('Username ok'))
    .fail(_ => alert('Username taken'))
}
```

### Deregistering username

```js
/**
 * Deregisters the username.
 */
function deregisterUsername(username)
{
    /**
     * The DELETE method is not used as
     * the navigator.sendBeacon(...) method in javascript
     * can only send post requests.
     */ 

    navigator.sendBeacon(
        'http://subscription-manager.local:40000/deregister',
        username
    );
}
```

### Getting usernames

```js
/**
 * Gets the registered username.
 */
function getUsernames()
{
    $.ajax(
    {
        method: 'GET',
        url   : 'http://localhost:40000/getNames'
    })
    .done((usernameList) => {
        do_something(usernameList))
    }
}
