function openAddUserModal() {
    document.getElementById('modalTitle').innerText = 'Add User';
    document.getElementById('editingRowIndex').value = '';

    document.getElementById('fullNameInput').value = '';
    document.getElementById('emailInput').value = '';
    document.getElementById('roleInput').value = 'User';
    document.getElementById('careerGoalInput').value = '';
    document.getElementById('statusInput').value = 'Active';
}

function openEditUserModal(button) {
    const row = button.closest('tr');
    const cells = row.querySelectorAll('td');

    document.getElementById('modalTitle').innerText = 'Edit User';
    document.getElementById('editingRowIndex').value = row.rowIndex - 1;

    document.getElementById('fullNameInput').value = cells[1].innerText.trim();
    document.getElementById('emailInput').value = cells[2].innerText.trim();
    document.getElementById('roleInput').value = cells[3].innerText.trim();
    document.getElementById('careerGoalInput').value = cells[4].innerText.trim();
    document.getElementById('statusInput').value = cells[5].innerText.trim();

    const modal = new bootstrap.Modal(document.getElementById('userModal'));
    modal.show();
}

function saveUser() {
    const fullName = document.getElementById('fullNameInput').value.trim();
    const email = document.getElementById('emailInput').value.trim();
    const role = document.getElementById('roleInput').value;
    const careerGoal = document.getElementById('careerGoalInput').value.trim();
    const status = document.getElementById('statusInput').value;
    const editingRowIndex = document.getElementById('editingRowIndex').value;

    if (!fullName || !email || !careerGoal) {
        alert('Vui lòng nhập đầy đủ thông tin');
        return;
    }

    let badgeClass = 'bg-green-lt';

    if (status === 'Pending')
        badgeClass = 'bg-yellow-lt';

    if (status === 'Blocked')
        badgeClass = 'bg-red-lt';

    const rowHtml = `
        <td><input type="checkbox" class="form-check-input"></td>
        <td>${fullName}</td>
        <td>${email}</td>
        <td>${role}</td>
        <td>${careerGoal}</td>
        <td><span class="badge ${badgeClass}">${status}</span></td>
        <td class="table-actions">
            <button class="btn btn-sm btn-outline-primary"
                    onclick="openEditUserModal(this)">
                Edit
            </button>

            <button class="btn btn-sm btn-outline-danger"
                    onclick="deleteUser(this)">
                Delete
            </button>
        </td>
    `;

    const tbody = document.getElementById('userTableBody');

    if (editingRowIndex === '') {
        const newRow = document.createElement('tr');
        newRow.innerHTML = rowHtml;
        tbody.appendChild(newRow);
    } else {
        tbody.rows[editingRowIndex].innerHTML = rowHtml;
    }

    bootstrap.Modal.getInstance(
        document.getElementById('userModal')
    ).hide();

    updateEntryText();
    filterUsers();
}

function deleteUser(button) {
    if (confirm('Bạn có chắc muốn xoá user này không?')) {
        button.closest('tr').remove();
        updateEntryText();
    }
}

function filterUsers() {
    const searchValue =
        document.getElementById('searchInput').value.toLowerCase();

    const roleValue =
        document.getElementById('roleFilter').value;

    const statusValue =
        document.getElementById('statusFilter').value;

    const rows =
        document.querySelectorAll('#userTableBody tr');

    rows.forEach(row => {

        const name =
            row.children[1].innerText.toLowerCase();

        const email =
            row.children[2].innerText.toLowerCase();

        const role =
            row.children[3].innerText.trim();

        const status =
            row.children[5].innerText.trim();

        const matchSearch =
            name.includes(searchValue) ||
            email.includes(searchValue);

        const matchRole =
            roleValue === '' || role === roleValue;

        const matchStatus =
            statusValue === '' || status === statusValue;

        row.style.display =
            matchSearch && matchRole && matchStatus
                ? ''
                : 'none';
    });
}

function updateEntryText() {
    const total =
        document.querySelectorAll('#userTableBody tr').length;

    document.getElementById('entryText').innerText =
        `Showing 1 to ${total} entries`;
}

document.addEventListener('DOMContentLoaded', function () {
    updateEntryText();
});