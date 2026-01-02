"use strict";

var connection; // The connection

/*********************************************************************/
/*** EVENT LISTENERS *************************************************/
/*********************************************************************/
function OnClientReceiveBroadcast(hookNameFrom, message) {
    var finalMessage = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var displayMsg = `${new Date().toISOString()} | [${hookNameFrom}] -> [HookHubNet] | [OnClientReceiveBroadcast]: ${finalMessage}`;
    var li = document.createElement("li");
    li.textContent = displayMsg;
    var listMessage = document.getElementById("listMessage");
    listMessage.insertBefore(li, listMessage.firstChild);     
    loadHookInfo();
}

function OnClientReceiveMessage(hookNameFrom, toUsername, message) {
    var finalMessage = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var displayMsg = `${new Date().toISOString()} | [${hookNameFrom}] -> [${toUsername}] | [OnClientReceiveMessage]: ${finalMessage}`;
    var li = document.createElement("li");
    li.textContent = displayMsg;
    //document.getElementById("listMessage").appendChild(li);
    var listMessage = document.getElementById("listMessage");
    listMessage.insertBefore(li, listMessage.firstChild);
    loadHookInfo();    
}

function OnStart() {
    try {     
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
function Connect(url,hookNameFrom) {
    // Creating a connection to SignalR Hub
    connection = new signalR.HubConnectionBuilder().withUrl(url + "?hookName=" + hookNameFrom).build();

    // Starting the connection with server
    connection.start().then(OnStart).catch(function (err) {
        return console.error(err.toString());
    });
    //event.preventDefault();
}

/*********************************************************************/

function sortList(ul) {
    Array.from(ul.getElementsByTagName("LI"))
        .sort((a, b) => a.textContent.localeCompare(b.textContent))
        .forEach(li => ul.appendChild(li));
}