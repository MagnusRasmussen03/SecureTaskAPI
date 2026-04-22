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
async function login() {
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    setLoading('loginBtn', true, 'Log ind');

    try {
        const response = await apiFetch('/auth/login', 'POST', { username, password });

        if (response.ok) {
            const data = await response.json();
            token = data.token;
            currentRole = getRoleFromToken(token);

            // Vis admin panel eller normal task sektion
            if (currentRole === 'admin') {
                showAdminSection();
            } else {
                showTaskSection();
            }
        } else {
            showMessage('authMessage', 'Forkert brugernavn eller password!', 'error');
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