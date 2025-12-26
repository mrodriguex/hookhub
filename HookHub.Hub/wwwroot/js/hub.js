"use strict";

var connection; // The connection

(function (event) {
    try {
        Connect(event);
    } catch (error) {
        console.error(error);
    }
})();

/*********************************************************************/
/*** EVENT LISTENERS *************************************************/
/*********************************************************************/
function OnClientReceiveBroadcast(hookNameFrom, message) {
    var finalMessage = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var displayMsg = hookNameFrom + " -> HookHubNet : " + finalMessage;
    var li = document.createElement("li");
    li.textContent = displayMsg;
    var listMessage = document.getElementById("listMessage");
    listMessage.insertBefore(li, listMessage.firstChild);
    GetAllHookConnections(event);
}

function OnClientReceiveMessage(hookNameFrom, toUsername, message) {
    var finalMessage = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var displayMsg = hookNameFrom + " -> " + toUsername + " : " + finalMessage;
    var li = document.createElement("li");
    li.textContent = displayMsg;
    //document.getElementById("listMessage").appendChild(li);
    var listMessage = document.getElementById("listMessage");
    listMessage.insertBefore(li, listMessage.firstChild);
    GetAllHookConnections(event);
}

function OnStart() {
    try {
        var connectionId = connection.connectionId;
        GetAllHookConnections(event);
        connection.on("OnClientReceiveBroadcast", OnClientReceiveBroadcast);
        connection.on("OnClientReceiveMessage", OnClientReceiveMessage);
    } catch (err) {
        console.error(err);
    }
}
/*********************************************************************/

/*********************************************************************/
/*** ACTION METHODS **************************************************/
/*********************************************************************/
function Connect(event) {
    var url = document.getElementById("url").value;
    var hookNameFrom = document.getElementById("hookNameFrom").value;

    // Creating a connection to SignalR Hub
    connection = new signalR.HubConnectionBuilder().withUrl(url + "?hookName=" + hookNameFrom).build();

    // Starting the connection with server
    connection.start().then(OnStart).catch(function (err) {
        return console.error(err.toString());
    });
    //event.preventDefault();
}

function SendMessage(event) {
    var hookNameFrom = document.getElementById("hookNameFrom").value;
    var hookNameTo = document.getElementById("hookNameTo").value;
    var message = document.getElementById("message").value;
    connection.invoke("SendMessage", hookNameFrom, hookNameTo, message).catch(function (err) {
        return console.error(err.toString());
    });
    //event.preventDefault();
}

function GetAllHookConnections(event) {
    connection.invoke("GetAllHookConnections")
        .then(function (hookConnections) {
            var numHookConnections = hookConnections.length;
            $('#listUsers').empty();
            var listUsers = document.getElementById("listUsers");
            for (var i = 0; i < numHookConnections; i++) {
                var hookConnection = hookConnections[i];
                var li = document.createElement("li");
                li.textContent = hookConnection.hookName + " :: " + hookConnection.connectionId;
                listUsers.prepend(li);
            }
            sortList(listUsers);
        })
        .catch(function (err) {
            return console.error(err.toString());
        });

    //event.preventDefault();
}
/*********************************************************************/

/*********************************************************************/
/*** OBJECT EVENT LISTENER ADDS **************************************/
/*********************************************************************/
// Connecting the Client to the Hub
document.getElementById("btnConnect").addEventListener("click", Connect);
// Sending the message from Client
document.getElementById("btnSend").addEventListener("click", SendMessage);
// Refres the Users list
document.getElementById("btnRefreshUsers").addEventListener("click", GetAllHookConnections);
/*********************************************************************/



function sortList(ul) {
    Array.from(ul.getElementsByTagName("LI"))
        .sort((a, b) => a.textContent.localeCompare(b.textContent))
        .forEach(li => ul.appendChild(li));
}