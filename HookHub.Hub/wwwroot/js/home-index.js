// 1️⃣ Read config injected by Razor
const API_URL = window.hookHubConfig.apiUrl;
const HUB_URL = window.hookHubConfig.hubUrl;
const HOOK_NAME = window.hookHubConfig.hookName;

// 2️⃣ Page bootstrap
document.addEventListener("DOMContentLoaded", () => {
    loadHookInfo();
    Connect(HUB_URL, HOOK_NAME);
});

// 3️⃣ Load hook info (GET /hook/index)
async function loadHookInfo() {
    try {
        const response = await fetch(API_URL, {
            credentials: "include"
        });

        if (!response.ok) {
            throw new Error("Failed to load hook info");
        }

        const data = await response.json();
        renderHookInfo(data);

    } catch (err) {
        renderError(err.message);
    }
}

// 4️⃣ 🔁 THIS IS WHAT YOU ASKED ABOUT
// Async Start / Stop / Restart + refresh
async function callHookAction(url) {
    try {
        showLoading();
debugger;
        const response = await fetch(url, {
            method: "GET",
            credentials: "include"
        });

        if (!response.ok) {
            throw new Error("Action failed");
        }

        // 🔄 refresh data instead of reloading page
        await loadHookInfo();        

    } catch (err) {
        alert(err.message);
    } finally {
        hideLoading();
    }
}

// 5️⃣ Render helpers
function renderHookInfo(data) {
    const tbody = document.getElementById("hookInfoTable");

    const stateBadge =
        data.hubConnectionState === 1
            ? "bg-success"
            : "bg-danger";

    tbody.innerHTML = `
        <tr>
            <td><strong>Hook Name</strong></td>
            <td class="d-flex justify-content-between align-items-center">
            ${data.hookConnection.hookName} 
                <span class="badge ${stateBadge}">
                    ${data.hubConnectionState === 1 ? "Connected" : "Disconnected"}
                </span></td>
        </tr>
        <tr>
            <td><strong>Connection ID</strong></td>
            <td class="text-break">${data.connection.connectionId ?? ""}</td>
        </tr>
        <tr>
            <td><strong>Hub URL</strong></td>
            <td class="text-break">${data.hookConnection.hookHubNetURL}</td>
        </tr>
        <tr>
            <td><strong>Keep Alive</strong></td>
            <td class="d-flex justify-content-between align-items-center">
                ${new Date(data.hookConnection.lastKeepAlive).toISOString().replace(/\.\d{3}Z$/, 'Z')}              
                <span>${data.hookConnection.timeIntervals_KeepAlive} (ms) interval<span>
            </td>
        </tr>
    `;
}

function renderError(message) {
    document.getElementById("hookInfoTable").innerHTML = `
        <tr>
            <td colspan="2">
                <div class="alert alert-warning mb-0">${message}</div>
            </td>
        </tr>`;
}
