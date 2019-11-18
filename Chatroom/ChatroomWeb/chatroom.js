var client = null;
var username = null;

$(document).ready(() => {
    let host = "broker.hivemq.com";
    let port = 8000;

    client = new Paho.MQTT.Client(host, port, "broker");
    client.connect({
        onSuccess: () => {
            client.subscribe("messori/fermi/chatroom");
            client.onMessageArrived = (message) => {
                let data = JSON.parse(message.payloadString);

                let position = data.username == username ? "right" : "left";
                $("#messagesContainer").html(
                    $("#messagesContainer").html() +
                    `<div class="row">
                        <div class="col ${position}" style="max-width: 60%; min-width: 20%;">
                            <div class="card blue-grey darken-1">
                                <div class="white-text" style="padding: 10px">
                                    <span class="card-title">${data.username}</span>
                                    <p style="word-break: break-all;">${data.contents.replace(/\n/gi, "<br>")}</p>
                                    <span class="right" style="font-size:11px; margin-top: -8px;">
                                        ${data.timestamp}
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
    let date = new Date();
    let message = $("#messageSendField").val();

    let json = {
        "username"  : username,
        "contents"  : message,
        "timestamp" : `${date.getHours()}:${date.getMinutes()}`
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

    if(name)
    {
        username = name;
        $("#usernameModal").modal('close');
        $("body").css("overflow", "");
    }
}