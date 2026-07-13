function openAddUserModal() {
    document.getElementById('modalTitle').innerText = 'Add User';
    document.getElementById('editingUserId').value = '';

    document.getElementById('fullNameInput').value = '';
    document.getElementById('emailInput').value = '';
    document.getElementById('roleInput').value = 'User';
    document.getElementById('statusInput').value = 'Active';
    document.getElementById('passwordInput').value = '';
    document.getElementById('passwordGroup').style.display = 'block';
}

function openEditUserModal(button) {
    const userId = button.getAttribute('data-id');
    const fullName = button.getAttribute('data-fullname');
    const email = button.getAttribute('data-email');
    const role = button.getAttribute('data-role');
    const status = button.getAttribute('data-status');

    document.getElementById('modalTitle').innerText = 'Edit User';
    document.getElementById('editingUserId').value = userId;

    document.getElementById('fullNameInput').value = fullName;
    document.getElementById('emailInput').value = email;
    document.getElementById('roleInput').value = role;
    document.getElementById('statusInput').value = status;
    
    // Hide password group during editing
    document.getElementById('passwordGroup').style.display = 'none';

}

function saveUser() {
    const fullName = document.getElementById('fullNameInput').value.trim();
    const email = document.getElementById('emailInput').value.trim();
    const role = document.getElementById('roleInput').value;
    const status = document.getElementById('statusInput').value;
    const password = document.getElementById('passwordInput').value;
    const userId = document.getElementById('editingUserId').value;

    if (!fullName || !email) {
        alert('Please fill out all required fields.');
        return;
    }

    const payload = {
        fullName: fullName,
        email: email,
        role: role,
        status: status
    };

    let url = '/Admin/CreateUser';
    if (userId !== '') {
        payload.id = parseInt(userId);
        url = '/Admin/EditUser';
    } else {
        payload.password = password;
    }

    fetch(url, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(payload)
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            window.location.reload();
        } else {
            alert('Error: ' + data.message);
        }
    })
    .catch(error => {
        console.error('Error:', error);
        alert('Failed to save user.');
    });
}

function deleteUser(id) {
    if (confirm('Are you sure you want to delete this user?')) {
        fetch('/Admin/DeleteUser?id=' + id, {
            method: 'POST'
        })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                window.location.reload();
            } else {
                alert('Error: ' + data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Failed to delete user.');
        });
    }
}

function filterUsers() {
    const searchValue = document.getElementById('searchInput').value.toLowerCase();
    const roleValue = document.getElementById('roleFilter').value;
    const statusValue = document.getElementById('statusFilter').value;
    const rows = document.querySelectorAll('#userTableBody tr.user-row');

    rows.forEach(row => {
        const name = row.querySelector('.user-name').innerText.toLowerCase();
        const email = row.querySelector('.user-email').innerText.toLowerCase();
        const role = row.querySelector('.user-role').innerText.trim();
        const status = row.querySelector('.badge').innerText.trim();

        const matchSearch = name.includes(searchValue) || email.includes(searchValue);
        const matchRole = roleValue === '' || role === roleValue;
        const matchStatus = statusValue === '' || status === statusValue;

        if (matchSearch && matchRole && matchStatus) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}