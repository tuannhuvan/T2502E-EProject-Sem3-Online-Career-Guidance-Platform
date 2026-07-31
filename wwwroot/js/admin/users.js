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

let currentPage = 1;
const pageSize = 9;

function filterUsers(resetPage = false) {
    if (resetPage === true || (resetPage && (resetPage instanceof Event || resetPage.target))) {
        currentPage = 1;
    }

    const searchValue = document.getElementById('searchInput').value.toLowerCase();
    const roleValue = document.getElementById('roleFilter').value;
    const statusValue = document.getElementById('statusFilter').value;
    const membershipValue = document.getElementById('membershipFilter').value;
    const rows = document.querySelectorAll('#userTableBody tr.user-row');

    const matchedRows = [];

    rows.forEach(row => {
        try {
            const nameEl = row.querySelector('.user-name');
            const emailEl = row.querySelector('.user-email');
            const roleEl = row.querySelector('.user-role');
            const statusEl = row.querySelector('.status-badge');

            const name = nameEl ? nameEl.innerText.toLowerCase() : '';
            const email = emailEl ? emailEl.innerText.toLowerCase() : '';
            const role = roleEl ? roleEl.innerText.trim() : '';
            const status = statusEl ? statusEl.innerText.trim() : '';
            const isPremium = row.getAttribute('data-premium') === 'true';

            const matchSearch = name.includes(searchValue) || email.includes(searchValue);
            const matchRole = roleValue === '' || role === roleValue;
            const matchStatus = statusValue === '' || status === statusValue;
            const matchMembership = membershipValue === '' || 
                                    (membershipValue === 'premium' && isPremium) || 
                                    (membershipValue === 'free' && !isPremium);

            if (matchSearch && matchRole && matchStatus && matchMembership) {
                matchedRows.push(row);
            } else {
                row.style.display = 'none';
            }
        } catch (e) {
            console.error("Error processing row:", e, row);
        }
    });

    const totalItems = matchedRows.length;
    const totalPages = Math.ceil(totalItems / pageSize) || 1;

    if (currentPage > totalPages) {
        currentPage = totalPages;
    }

    const startIdx = (currentPage - 1) * pageSize;
    const endIdx = startIdx + pageSize;

    matchedRows.forEach((row, index) => {
        if (index >= startIdx && index < endIdx) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });

    // Update entry info
    const startEntry = totalItems === 0 ? 0 : startIdx + 1;
    const endEntry = Math.min(endIdx, totalItems);
    document.getElementById('entryText').innerHTML = `Showing ${startEntry} to ${endEntry} of ${totalItems} entries`;

    // Render pagination links
    const paginationUl = document.getElementById('pagination-ul');
    if (paginationUl) {
        paginationUl.innerHTML = '';

        // Prev Button
        const prevLi = document.createElement('li');
        prevLi.className = `page-item ${currentPage === 1 ? 'disabled' : ''}`;
        prevLi.innerHTML = `<a class="page-link" href="#" tabindex="-1" aria-disabled="${currentPage === 1}"><i class="ti ti-chevron-left"></i> prev</a>`;
        if (currentPage > 1) {
            prevLi.querySelector('a').addEventListener('click', function(e) {
                e.preventDefault();
                currentPage--;
                filterUsers();
            });
        }
        paginationUl.appendChild(prevLi);

        // Page numbers
        const maxButtons = 5;
        let startPage = Math.max(1, currentPage - Math.floor(maxButtons / 2));
        let endPage = Math.min(totalPages, startPage + maxButtons - 1);
        if (endPage - startPage + 1 < maxButtons) {
            startPage = Math.max(1, endPage - maxButtons + 1);
        }

        for (let i = startPage; i <= endPage; i++) {
            const li = document.createElement('li');
            li.className = `page-item ${currentPage === i ? 'active' : ''}`;
            li.innerHTML = `<a class="page-link" href="#">${i}</a>`;
            li.querySelector('a').addEventListener('click', function(e) {
                e.preventDefault();
                currentPage = i;
                filterUsers();
            });
            paginationUl.appendChild(li);
        }

        // Next Button
        const nextLi = document.createElement('li');
        nextLi.className = `page-item ${currentPage === totalPages ? 'disabled' : ''}`;
        nextLi.innerHTML = `<a class="page-link" href="#">next <i class="ti ti-chevron-right"></i></a>`;
        if (currentPage < totalPages) {
            nextLi.querySelector('a').addEventListener('click', function(e) {
                e.preventDefault();
                currentPage++;
                filterUsers();
            });
        }
        paginationUl.appendChild(nextLi);
    }
}

// Initialise pagination and trigger search reset on input changes
function init() {
    const searchInput = document.getElementById('searchInput');
    const roleFilter = document.getElementById('roleFilter');
    const statusFilter = document.getElementById('statusFilter');
    const membershipFilter = document.getElementById('membershipFilter');

    // Parse URL query parameter for role
    const urlParams = new URLSearchParams(window.location.search);
    let roleParam = urlParams.get('role');
    if (roleParam) {
        if (roleParam === 'Student') {
            roleParam = 'User';
        }
        if (roleFilter) {
            roleFilter.value = roleParam;
        }
    }

    // Parse URL query parameter for accountType (Premium/Free) and sync to membership filter
    let accountTypeParam = urlParams.get('accountType');
    if (accountTypeParam && membershipFilter) {
        if (accountTypeParam === 'Premium') {
            membershipFilter.value = 'premium';
        } else if (accountTypeParam === 'Free') {
            membershipFilter.value = 'free';
        }
    }

    if (searchInput) searchInput.addEventListener('input', () => filterUsers(true));
    if (roleFilter) roleFilter.addEventListener('change', () => filterUsers(true));
    if (statusFilter) statusFilter.addEventListener('change', () => filterUsers(true));
    if (membershipFilter) membershipFilter.addEventListener('change', () => filterUsers(true));

    rebindActionButtons();

    // View user details popup modal logic via event delegation
    const tableBody = document.getElementById('userTableBody');
    if (tableBody) {
        tableBody.addEventListener('click', function(e) {
            const btn = e.target.closest('.view-details-btn');
            if (btn) {
                const fullName = btn.getAttribute('data-fullname') || 'N/A';
                const email = btn.getAttribute('data-email') || '';
                const role = btn.getAttribute('data-role') || 'User';
                const status = btn.getAttribute('data-status') || 'Active';
                const premium = btn.getAttribute('data-premium') === 'true';
                const created = btn.getAttribute('data-created') || 'N/A';
                const headline = btn.getAttribute('data-headline') || 'Chưa cung cấp';
                const school = btn.getAttribute('data-school') || 'Chưa cung cấp';
                const major = btn.getAttribute('data-major') || 'Chưa cung cấp';
                const experience = btn.getAttribute('data-experience') || 'Chưa cung cấp';

                document.getElementById('viewUserFullName').innerText = fullName;
                document.getElementById('viewUserEmail').innerText = email;
                document.getElementById('viewUserRole').innerText = role;
                document.getElementById('viewUserCreatedAt').innerText = created;
                
                const statusBadge = document.getElementById('viewUserStatus');
                statusBadge.innerText = status;
                statusBadge.className = 'badge ' + (status === 'Active' ? 'bg-green-lt' : (status === 'Pending' ? 'bg-yellow-lt' : 'bg-red-lt'));

                const membershipBadge = document.getElementById('viewUserMembership');
                membershipBadge.innerText = premium ? 'Premium' : 'Regular';
                membershipBadge.className = 'badge ' + (premium ? 'bg-purple-lt' : 'bg-secondary-lt');

                const avatar = document.getElementById('viewUserAvatar');
                avatar.innerText = fullName ? fullName.charAt(0).toUpperCase() : 'U';

                document.getElementById('viewUserHeadline').innerText = headline;
                document.getElementById('viewUserSchool').innerText = school;
                document.getElementById('viewUserMajor').innerText = major;
                document.getElementById('viewUserExperience').innerText = experience;

                const myModal = bootstrap.Modal.getOrCreateInstance(document.getElementById('userViewModal'));
                myModal.show();
            }
        });
    }

    filterUsers(true);
}

// Re-attach click handlers for Edit/Delete/View action buttons after the
// table body is replaced or updated (e.g. following an AJAX refresh).
// Uses event delegation on the table body, so it is safe to call multiple
// times without creating duplicate listeners.
function rebindActionButtons() {
    const tableBody = document.getElementById('userTableBody');
    if (!tableBody || tableBody.dataset.actionsBound === 'true') {
        return;
    }
    tableBody.dataset.actionsBound = 'true';

    tableBody.addEventListener('click', function (e) {
        const editBtn = e.target.closest('.btn-action-edit');
        if (editBtn && editBtn.tagName === 'BUTTON') {
            const userId = editBtn.getAttribute('data-id');
            if (userId) openEditUserModal(editBtn);
            return;
        }

        const deleteBtn = e.target.closest('.btn-action-delete');
        if (deleteBtn) {
            const userId = deleteBtn.getAttribute('data-id');
            if (userId) deleteUser(parseInt(userId, 10));
        }
    });
}

if (document.readyState !== 'loading') {
    init();
} else {
    document.addEventListener("DOMContentLoaded", init);
}