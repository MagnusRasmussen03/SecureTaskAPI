// ─────────────────────────────────────────
// TAB SKIFT (Login / Register)
// ─────────────────────────────────────────
function showTab(tab, event) {
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
    event.target.classList.add('active');

    if (tab === 'login') {
        document.getElementById('loginBtn').classList.remove('hidden');
        document.getElementById('registerBtn').classList.add('hidden');
    } else {
        document.getElementById('loginBtn').classList.add('hidden');
        document.getElementById('registerBtn').classList.remove('hidden');
    }
    clearMessage('authMessage');
}

// ─────────────────────────────────────────
// LOG IND
// ─────────────────────────────────────────
// Hold styr på antal forsøg
let loginAttempts = 0;
const MAX_ATTEMPTS = 2;

async function login() {
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    setLoading('loginBtn', true, 'Log ind');

    try {
        const response = await apiFetch('/auth/login', 'POST', { username, password });

        if (response.ok) {
            // Nulstil forsøg ved success
            loginAttempts = 0;
            const data = await response.json();
            token = data.token;
            currentRole = getRoleFromToken(token);

            if (currentRole === 'admin') {
                showAdminSection();
            } else {
                showTaskSection();
            }

        } else if (response.status === 429) {
            // Rate limited!
            showMessage('authMessage', 
                '🔒 For mange forsøg! Prøv igen om en time.', 'error');
            document.getElementById('loginBtn').disabled = true;

        } else {
            // Forkert password - tæl forsøget
            loginAttempts++;
            const remaining = MAX_ATTEMPTS - loginAttempts;

            if (remaining <= 0) {
                showMessage('authMessage',
                    '⚠️ Sidste forsøg brugt! Næste fejl låser kontoen i en time.', 'error');
            } else {
                showMessage('authMessage',
                    `Forkert brugernavn eller password! Du har ${remaining} forsøg tilbage.`, 'error');
            }
        }
    } catch {
        showMessage('authMessage', 'Kunne ikke forbinde til API!', 'error');
    } finally {
        setLoading('loginBtn', false, 'Log ind');
    }
}

// ─────────────────────────────────────────
// OPRET BRUGER
// ─────────────────────────────────────────
async function register() {
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    setLoading('registerBtn', true, 'Opret bruger');

    try {
        const response = await apiFetch('/auth/register', 'POST', { username, password });

        if (response.ok) {
            showMessage('authMessage', 'Bruger oprettet! Log ind nu.', 'success');
        } else {
            const text = await response.text();
            showMessage('authMessage', text, 'error');
        }
    } catch {
        showMessage('authMessage', 'Kunne ikke forbinde til API!', 'error');
    } finally {
        setLoading('registerBtn', false, 'Opret bruger');
    }
}

// ─────────────────────────────────────────
// LOG UD
// ─────────────────────────────────────────
function logout() {
    token = null;
    currentRole = null;
    document.getElementById('authSection').classList.remove('hidden');
    document.getElementById('taskSection').classList.add('hidden');
    document.getElementById('adminSection').classList.add('hidden');
    document.getElementById('username').value = '';
    document.getElementById('password').value = '';
}