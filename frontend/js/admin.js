// ─────────────────────────────────────────
// VIS ADMIN SEKTION
// ─────────────────────────────────────────
function showAdminSection() {
    document.getElementById('authSection').classList.add('hidden');
    document.getElementById('adminSection').classList.remove('hidden');
    loadAdminUsers();
}

// ─────────────────────────────────────────
// HENT ALLE BRUGERE
// ─────────────────────────────────────────
async function loadAdminUsers() {
    try {
        const response = await apiFetch('/admin/users');
        const users = await response.json();
        renderAdminUsers(users);
    } catch {
        showMessage('adminMessage', 'Kunne ikke hente brugere!', 'error');
    }
}

function renderAdminUsers(users) {
    document.getElementById('adminUserDetail').classList.add('hidden');
    document.getElementById('adminUserList').classList.remove('hidden');
    const list = document.getElementById('adminUserList');

    if (users.length === 0) {
        list.innerHTML = '<p style="text-align:center;color:#64748b">Ingen brugere fundet!</p>';
        return;
    }

    list.innerHTML = users.map(user => `
        <div class="user-card">
            <div class="user-card-header">
                <h3>
                    ${user.username}
                    ${user.role === 'admin' ? '<span class="admin-badge">admin</span>' : ''}
                </h3>
                <div class="user-card-buttons">
                    <button class="btn-info" onclick="loadUserDetail(${user.id}, '${user.username}')">
                        👁 Se opgaver
                    </button>
                    ${user.role !== 'admin' ? `
                    <button class="btn-danger" onclick="showConfirm(
                        '⚠️ Slet bruger',
                        'Er du sikker på du vil slette ${user.username}?',
                        () => deleteUser(${user.id})
                    )">🗑</button>` : ''}
                </div>
            </div>
            <div class="user-stats">
                <span>📋 Total: <strong>${user.totalTasks}</strong></span>
                <span>✅ Færdige: <strong style="color:#16a34a">${user.completedTasks}</strong></span>
                <span>⏳ Tilbage: <strong style="color:#f59e0b">${user.pendingTasks}</strong></span>
            </div>
        </div>
    `).join('');
}

// ─────────────────────────────────────────
// SE BRUGER DETALJER
// ─────────────────────────────────────────
async function loadUserDetail(userId, username) {
    try {
        const response = await apiFetch(`/admin/users/${userId}`);
        const user = await response.json();

        document.getElementById('adminUserList').classList.add('hidden');
        document.getElementById('adminUserDetail').classList.remove('hidden');
        document.getElementById('detailUsername').textContent = `${username}'s opgaver`;

        const taskList = document.getElementById('detailTaskList');

        if (user.tasks.length === 0) {
            taskList.innerHTML = '<p style="text-align:center;color:#64748b;padding:1rem">Ingen opgaver!</p>';
            return;
        }

        taskList.innerHTML = user.tasks.map(task => `
            <div class="task-item ${task.isCompleted ? 'completed' : ''}">
                <span class="task-title ${task.isCompleted ? 'completed' : ''}">${task.title}</span>
                <span style="font-size:0.85rem;color:${task.isCompleted ? '#16a34a' : '#f59e0b'}">
                    ${task.isCompleted ? '✓ Færdig' : '⏳ Afventer'}
                </span>
            </div>
        `).join('');

    } catch {
        showMessage('adminMessage', 'Kunne ikke hente bruger detaljer!', 'error');
    }
}

function showUserList() {
    document.getElementById('adminUserList').classList.remove('hidden');
    document.getElementById('adminUserDetail').classList.add('hidden');
}

// ─────────────────────────────────────────
// SLET BRUGER
// ─────────────────────────────────────────
async function deleteUser(id) {
    try {
        await apiFetch(`/admin/users/${id}`, 'DELETE');
        loadAdminUsers();
    } catch {
        showMessage('adminMessage', 'Kunne ikke slette bruger!', 'error');
    }
}