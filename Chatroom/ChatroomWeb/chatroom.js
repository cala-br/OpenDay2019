const host = "broker.hivemq.com";
const port = 8000;

var client       = null;
var privClient   = null;
var username     = null;
var lastReceived = {};
var openedDirect = [];

var currentContainer = "#globalContainer";

$(document).ready(() => {
    client = new Paho.MQTT.Client(host, port, "");

    client.connect({
        onSuccess: () => {
            client.subscribe("messori/fermi/chatroom");
            client.onMessageArrived = (message) => {
                
                let data = JSON.parse(message.payloadString);
                let date = new Date(data.timestamp);

                let sender = "";
                if(lastReceived[currentContainer] != data.username)
                    sender = `<span class="card-title">${data.username}</span>`
                
                lastReceived[currentContainer] = data.username
                let position = data.username == username ? "right" : "left";

                if($("#globalContainer").is(":hidden"))
                {
                    $("#globalContainerNewIcon").show();
                    $("#newMessageIcon").show();
                }
                $("#globalContainer").html(
                    $("#globalContainer").html() +
                    `<div class="row" style="margin-bottom: 0px">
                        <div class="col ${position}" style="max-width: 80%; min-width: 40%;">
                            <div class="card blue-grey darken-1">
                                <div class="white-text" style="padding: 10px">
                                    ${sender}
                                    <p style="word-break: break-all; margin-top: 0px;">${data.contents.replace(/\n/gi, "<br>")}</p>
                                    <span class="right" style="font-size:11px; margin-top: -8px;">
                                        ${date.toTimeString().slice(0,5)}
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

    $('select').formSelect();
    $(".modal").modal();
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
        let topic = "";
        if(currentContainer == '#globalContainer')
            topic = "messori/fermi/chatroom";
        else
        {
            topic = `messori/fermi/chatroom/private-messages/${currentContainer.replace("DirectContainer", "").replace("#", "")}`
            
            let data = json;
            let date = new Date(data.timestamp);

            let sender = "";
            if(lastReceived[currentContainer] != data.username)
                sender = `<span class="card-title">${data.username}</span>`
            
            lastReceived[currentContainer] = data.username
            let position = data.username == username ? "right" : "left";

            $(currentContainer).html(
                $(currentContainer).html() +
                `<div class="row" style="margin-bottom: 0px">
                    <div class="col ${position}" style="max-width: 80%; min-width: 40%;">
                        <div class="card blue-grey darken-1">
                            <div class="white-text" style="padding: 10px">
                                ${sender}
                                <p style="word-break: break-all; margin-top: 0px;">${data.contents.replace(/\n/gi, "<br>")}</p>
                                <span class="right" style="font-size:11px; margin-top: -8px;">
                                    ${date.toTimeString().slice(0,5)}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>`);

            window.scrollTo(0, document.body.scrollHeight);
        }
        
        client.send(topic, JSON.stringify(json));

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

    $.ajax(
    {
        method: 'POST',
        url   : 'http://localhost:40000/register',
        data  : name
    })
    .done(_ => {
        username = name;
        $("#usernameModal").modal('close');
        $("body").css("overflow", "");

        privClient = new Paho.MQTT.Client(host, port, "");

        privClient.connect({
            onSuccess: () => {
                privClient.subscribe("messori/fermi/chatroom/private-messages/" + username);
                privClient.onMessageArrived = (message) => {

                    let data = JSON.parse(message.payloadString);
                    let date = new Date(data.timestamp);

                    if(!openedDirect.includes(data.username))
                    {
                        $("#slide-out").html(
                            $("#slide-out").html() +
                            `
                            <li>
                                <a id="${data.username}Direct" class="waves-effect" href="#!" 
                                    style="margin-top: 10px;" onclick="selectChat('#${data.username}Direct')">
                                    <i class="material-icons">face</i>
                                    ${data.username}
                                    <i class="material-icons right" 
                                        style="font-size: 24px; color: #3b8880;"
                                        id="${data.username + "DirectContainerNewIcon"}">
                                        new_releases
                                    </i>
                                </a>
                            </li>
                            `
                        );
                        openedDirect.push(data.username);
                        $("#messagesContainers").html(
                            $("#messagesContainers").html() +
                            `<div class="row genericDivContainer" 
                                style="display: none"
                                id="${data.username}DirectContainer"></div>`
                        );

                        $("#newMessageIcon").show();
                    }

                    let sender = "";
                    if(lastReceived[currentContainer] != data.username)
                        sender = `<span class="card-title">${data.username}</span>`
                    
                    lastReceived[currentContainer] = data.username
                    let position = data.username == username ? "right" : "left";

                    if($("#" + data.username + "DirectContainer").is(":hidden"))
                    {
                        $("#" + data.username + "DirectContainerNewIcon").show();
                        $("#newMessageIcon").show();
                    }
                    $("#" + data.username + "DirectContainer").html(
                        $("#" + data.username + "DirectContainer").html() +
                        `<div class="row" style="margin-bottom: 0px">
                            <div class="col ${position}" style="max-width: 80%; min-width: 40%;">
                                <div class="card blue-grey darken-1">
                                    <div class="white-text" style="padding: 10px">
                                        ${sender}
                                        <p style="word-break: break-all; margin-top: 0px;">${data.contents.replace(/\n/gi, "<br>")}</p>
                                        <span class="right" style="font-size:11px; margin-top: -8px;">
                                            ${date.toTimeString().slice(0,5)}
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
    })
    .fail(_ => $("#usernameErrorLabel").css("display", ""))
}

// This will free the username when the page is closed or refreshed
window.addEventListener("unload", (e) => 
{    
    if(username)
    {
        navigator.sendBeacon(
            'http://localhost:40000/deregister', 
            username
        );
    }
    e.returnValue = '';

    return null;
})

function getUsernames()
{
    $.ajax(
    {
        method: 'GET',
        url   : 'http://localhost:40000/getNames'
    })
    .done((msg) => { 

        $("#directMessageSelect").html("<option disabled selected>Scegli il destinatario</option>");
        $('#newDirectModal').modal('open');

        for(el of JSON.parse(msg))
        {
            if(el != username)
            $("#directMessageSelect").html(
                $("#directMessageSelect").html() +
                `<option>${el}</option>`
            )
        }
        $('select').formSelect();
    })
}

function createNewDirect()
{
    let selected = $("#directMessageSelect").val();

    if(!openedDirect.includes(selected))
    {
        $("#slide-out .selected").removeClass("selected")
        $("#slide-out").html(
            $("#slide-out").html() +
            `
            <li>
                <a id="${selected}Direct" class="waves-effect selected" href="#!" 
                    style="margin-top: 10px;" onclick="selectChat('#${selected}Direct')">
                    <i class="material-icons">face</i>
                    ${selected}
                </a>
            </li>
            `
        );
        openedDirect.push(selected);
        $("#messagesContainers").html(
            $("#messagesContainers").html() +
            `<div class="row genericDivContainer" id="${selected}DirectContainer"></div>`
        );
    }
    $(currentContainer).hide();
    currentContainer = `#${selected}DirectContainer`
    $(currentContainer).show();

    $('.sidenav').sidenav('close');

    $("#currentChatLabel").html(selected);
}

function selectChat(username)
{
    $("#slide-out .selected").removeClass("selected");
    $(username).addClass("selected");

    $(currentContainer).hide();
    currentContainer = `${username}Container`
    $(currentContainer).show();

    $(currentContainer + "NewIcon").hide();

    $('.sidenav').sidenav('close');

    $("#currentChatLabel").html((username).replace("Direct","").replace("#",""));
}