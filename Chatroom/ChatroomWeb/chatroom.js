const host = "broker.hivemq.com";
const port = 8000;

var client       = null;
var username     = null;
var lastReceived = null;

$(document).ready(() => {
    client = new Paho.MQTT.Client(host, port, "");

    client.connect({
        onSuccess: () => {
            client.subscribe("messori/fermi/chatroom");
            client.onMessageArrived = (message) => {
                
                let data = JSON.parse(message.payloadString);
                let date = new Date(data.timestamp);

                let sender = "";
                if(lastReceived != data.username)
                    sender = `<span class="card-title">${data.username}</span>`
                
                lastReceived = data.username
                let position = data.username == username ? "right" : "left";

                $("#messagesContainer").html(
                    $("#messagesContainer").html() +
                    `<div class="row" style="margin-bottom: 0px">
                        <div class="col ${position}" style="max-width: 80%; min-width: 40%;">
                            <div class="card blue-grey darken-1">
                                <div class="white-text" style="padding: 10px">
                                    ${sender}
                                    <p style="word-break: break-all; margin-top: 0px;">${data.contents.replace(/\n/gi, "<br>")}</p>
                                    <span class="right" style="font-size:11px; margin-top: -8px;">
                                        ${date.getHours()}:${date.getMinutes()}
                                        <i class="material-icons" style="font-size:11px;">check</i>
                                        <i class="material-icons" style="font-size:11px;">check</i>
                                    </span>
                                </div>
                            </div>
                        </div>
                    </div>`
                );

                window.scrollTo(0, document.body.scrollHeight);
            };
        }
    }); 

    $("#usernameModal").modal();
    $("#usernameModal").modal('open');

    let modal = document.querySelectorAll('#usernameModal');
    let instance = M.Modal.init(modal, { dismissible : false })[0];
    instance.open();

    $('.sidenav').sidenav();
});

function doSendMessage()
{
    let message = $("#messageSendField").val();
    if(!message.match(/^(\n)*$/))
        if(event.keyCode == 13 && event.shiftKey == false)
            sendMessage();
        
}

function sendMessage()
{
    let message = $("#messageSendField").val();

    let json = {
        "username"  : username,
        "contents"  : message,
        "timestamp" : new Date()
    }

    if(message)
    {
        client.send("messori/fermi/chatroom", JSON.stringify(json));
        setTimeout(() => {
            $("#messageSendField").val("");
            $("#messageSendField").css("height", "")
        }, 100);
    }
    else
        M.toast({html: 'Non puoi inviare il messaggio vuoto', displayLength: 800})
}

function selectUsername()
{
    let name = $("#usernameField").val();

    let c = new Paho.MQTT.Client(host, port, "");
    let usedUsernames = [];

    c.connect({
        onSuccess: () => {
            c.subscribe("messori/fermi/chatroom/usernames");
            c.onMessageArrived = (message) => {
                usedUsernames = JSON.parse(message.payloadString);

                if(name && !usedUsernames.includes(name))
                {
                    username = name;
                    $("#usernameModal").modal('close');
                    $("body").css("overflow", "");
                    
                    usedUsernames.push(name); 
                    client.send("messori/fermi/chatroom/usernames", JSON.stringify(usedUsernames), 1, true);
                    
                    c.unsubscribe();
                }
                else
                    $("#usernameErrorLabel").css("display", "");
            }
        }
    });
}

// This will free the username when the page is closed or refreshed
window.addEventListener("unload", (e) => 
{    
    if(username)
    {
        navigator.sendBeacon(
            'http://localhost:40000', 
            username)
    }
    e.returnValue = '';

    return null;
    return false;
})