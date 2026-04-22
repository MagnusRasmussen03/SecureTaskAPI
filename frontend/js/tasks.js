// ─────────────────────────────────────────
// VIS TASK SEKTION
// ─────────────────────────────────────────
function showTaskSection() {
    document.getElementById('authSection').classList.add('hidden');
    document.getElementById('taskSection').classList.remove('hidden');
    loadTasks();
}

// Opdater statistik banner
function updateStats(tasks) {
    const total = tasks.length;
    const completed = tasks.filter(t => t.isCompleted).length;
    document.getElementById('statTotal').textContent = total;
    document.getElementById('statCompleted').textContent = completed;
    document.getElementById('statPending').textContent = total - completed;
}

// ─────────────────────────────────────────
// HENT ALLE OPGAVER
// ─────────────────────────────────────────
async function loadTasks() {
    try {
        const response = await apiFetch('/tasks');
        const tasks = await response.json();
        renderTasks(tasks);
        updateStats(tasks);
    } catch {
        showMessage('taskMessage', 'Kunne ikke hente opgaver!', 'error');
    }
}

// Vis opgaver i DOM
function renderTasks(tasks) {
    const list = document.getElementById('taskList');

    if (tasks.length === 0) {
        list.innerHTML = '<p style="text-align:center;color:#64748b;padding:1rem">Ingen opgaver endnu!</p>';
        return;
    }

    list.innerHTML = tasks.map(task => `
        <div class="task-item ${task.isCompleted ? 'completed' : ''}">
            <span class="task-title ${task.isCompleted ? 'completed' : ''}">${task.title}</span>
            <div class="task-buttons">
                <button class="btn-success" onclick="toggleTask(${task.id}, ${task.isCompleted})">
                    ${task.isCompleted ? '↩' : '✓'}
                </button>
                <button class="btn-danger" onclick="showConfirm(
                    '⚠️ Slet opgave',
                    'Er du sikker på du vil slette denne opgave?',
                    () => deleteTask(${task.id})
                )">🗑</button>
            </div>
        </div>
    `).join('');
}

// ─────────────────────────────────────────
// OPRET OPGAVE
// ─────────────────────────────────────────
async function createTask() {
    const title = document.getElementById('newTaskTitle').value.trim();

    if (!title) {
        showMessage('taskMessage', 'Skriv en titel først!', 'error');
        return;
    }

    setLoading('addTaskBtn', true, 'Tilføj opgave');

    try {
        const response = await apiFetch('/tasks', 'POST', { title });
        if (response.ok) {
            document.getElementById('newTaskTitle').value = '';
            clearMessage('taskMessage');
            loadTasks();
        }
    } catch {
        showMessage('taskMessage', 'Kunne ikke oprette opgave!', 'error');
    } finally {
        setLoading('addTaskBtn', false, 'Tilføj opgave');
    }
}

// ─────────────────────────────────────────
// SKIFT STATUS PÅ OPGAVE
// ─────────────────────────────────────────
async function toggleTask(id, isCompleted) {
    try {
        const response = await apiFetch(`/tasks/${id}`);
        const task = await response.json();
        await apiFetch(`/tasks/${id}`, 'PUT', {
            title: task.title,
            isCompleted: !isCompleted
        });
        loadTasks();
    } catch {
        showMessage('taskMessage', 'Kunne ikke opdatere opgave!', 'error');
    }
}

// ─────────────────────────────────────────
// SLET OPGAVE
// ─────────────────────────────────────────
async function deleteTask(id) {
    try {
        await apiFetch(`/tasks/${id}`, 'DELETE');
        loadTasks();
    } catch {
        showMessage('taskMessage', 'Kunne ikke slette opgave!', 'error');
    }
}

// Enter-tast til at oprette opgave
document.addEventListener('DOMContentLoaded', () => {
    document.getElementById('newTaskTitle')?.addEventListener('keypress', (e) => {
        if (e.key === 'Enter') createTask();
    });
});