// ─────────────────────────────────────────
// API KONFIGURATION OG GLOBAL STATE
// ─────────────────────────────────────────
const API_URL = 'http://localhost:5274';

// Token og rolle lever i hukommelsen
// Lukker du browseren — er de væk!
let token = null;
let currentRole = null;
let pendingAction = null;

// ─────────────────────────────────────────
// HJÆLPEFUNKTIONER
// ─────────────────────────────────────────

// Lav et API kald med JWT token automatisk
async function apiFetch(endpoint, method = 'GET', body = null) {
    const headers = { 'Content-Type': 'application/json' };
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const response = await fetch(`${API_URL}${endpoint}`, {
        method,
        headers,
        body: body ? JSON.stringify(body) : null
    });

    return response;
}

// Vis en besked til brugeren
function showMessage(id, text, type) {
    const el = document.getElementById(id);
    el.textContent = text;
    el.className = `message ${type}`;
}

// Ryd besked
function clearMessage(id) {
    document.getElementById(id).className = 'message';
}

// Vis loading spinner på en knap
function setLoading(btnId, loading, label) {
    const btn = document.getElementById(btnId);
    if (!btn) return;
    btn.disabled = loading;
    btn.innerHTML = loading
        ? `<span class="spinner"></span> Vent...`
        : label;
}

// Læs rolle fra JWT token
function getRoleFromToken(jwt) {
    try {
        const payload = JSON.parse(atob(jwt.split('.')[1]));
        return payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || 'user';
    } catch {
        return 'user';
    }
}

// Bekræftelsesdialog
function showConfirm(title, text, action) {
    document.getElementById('dialogTitle').textContent = title;
    document.getElementById('dialogText').textContent = text;
    pendingAction = action;
    document.getElementById('confirmOverlay').classList.add('visible');
}

function cancelAction() {
    pendingAction = null;
    document.getElementById('confirmOverlay').classList.remove('visible');
}

async function executeAction() {
    document.getElementById('confirmOverlay').classList.remove('visible');
    if (pendingAction) await pendingAction();
    pendingAction = null;
}