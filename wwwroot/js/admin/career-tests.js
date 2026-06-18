let currentRow = null;
console.log("career-tests.js loaded");
document.addEventListener("DOMContentLoaded", function () {
    const btnAdd = document.getElementById("btnAddQuestion");
    const btnSave = document.getElementById("btnSaveQuestion");
    const searchInput = document.getElementById("searchInput");
    const filterType = document.getElementById("filterType");

    btnAdd?.addEventListener("click", openAddModal);
    btnSave?.addEventListener("click", saveQuestion);
    searchInput?.addEventListener("keyup", applyFilters);
    filterType?.addEventListener("change", applyFilters);

    document.addEventListener("click", function (e) {
        const editBtn = e.target.closest(".btn-edit-question");
        const deleteBtn = e.target.closest(".btn-delete-question");

        if (editBtn) {
            openEditModal(editBtn);
        }

        if (deleteBtn) {
            deleteQuestion(deleteBtn);
        }
    });
});

function openAddModal() {
    currentRow = null;

    document.getElementById("modalTitle").innerText = "Add Question";
    document.getElementById("questionInput").value = "";
    document.getElementById("groupInput").value = "Interest";
    document.getElementById("statusInput").value = "Active";

    document.querySelectorAll(".option-input")
        .forEach(input => input.value = "");
}

function openEditModal(btn) {
    currentRow = btn.closest("tr");

    const cells = currentRow.querySelectorAll("td");
    const optionCell = currentRow.querySelector(".question-options");
    const inputs = document.querySelectorAll(".option-input");

    document.getElementById("modalTitle").innerText = "Edit Question";
    document.getElementById("questionInput").value =
        currentRow.querySelector(".question-content").innerText.trim();

    document.getElementById("groupInput").value =
        currentRow.querySelector(".question-type").innerText.trim();

    document.getElementById("statusInput").value =
        currentRow.querySelector(".question-status").innerText.trim();

    inputs[0].value = optionCell.dataset.option1 || "";
    inputs[1].value = optionCell.dataset.option2 || "";
    inputs[2].value = optionCell.dataset.option3 || "";
    inputs[3].value = optionCell.dataset.option4 || "";

    const modalEl = document.getElementById("questionModal");

    const modal = new bootstrap.Modal(modalEl);
    modal.show();
}

function saveQuestion() {
    const question =
        document.getElementById("questionInput").value.trim();

    const type =
        document.getElementById("groupInput").value;

    const status =
        document.getElementById("statusInput").value;

    if (!question) {
        alert("Please enter question.");
        return;
    }

    const options = [];

    document.querySelectorAll(".option-input").forEach(input => {
        options.push(input.value.trim());
    });

    const optionCount =
        options.filter(x => x !== "").length;

    const badgeClass =
        status === "Active"
            ? "bg-green-lt"
            : "bg-red-lt";

    const rowHtml = `
        <td class="question-content">${escapeHtml(question)}</td>

        <td class="question-type">${escapeHtml(type)}</td>

        <td class="question-options"
            data-option1="${escapeAttr(options[0])}"
            data-option2="${escapeAttr(options[1])}"
            data-option3="${escapeAttr(options[2])}"
            data-option4="${escapeAttr(options[3])}">
            ${optionCount} Options
        </td>

        <td class="question-status">
            <span class="badge ${badgeClass}">
                ${escapeHtml(status)}
            </span>
        </td>

        <td class="text-end">
            <div class="btn-list justify-content-end">
                <button class="btn btn-sm btn-outline-primary btn-edit-question"
                        type="button">
                    Edit
                </button>

                <button class="btn btn-sm btn-outline-danger btn-delete-question"
                        type="button">
                    Delete
                </button>
            </div>
        </td>
    `;

    if (currentRow) {
        currentRow.innerHTML = rowHtml;
    } else {
        const tr = document.createElement("tr");
        tr.innerHTML = rowHtml;

        document
            .getElementById("questionTableBody")
            .appendChild(tr);
    }

    bootstrap.Modal
        .getOrCreateInstance(document.getElementById("questionModal"))
        .hide();

    applyFilters();
}

function deleteQuestion(btn) {
    if (confirm("Delete this question?")) {
        btn.closest("tr")?.remove();
    }
}

function applyFilters() {
    const keyword =
        document.getElementById("searchInput").value.toLowerCase();

    const typeFilter =
        document.getElementById("filterType").value;

    document
        .querySelectorAll("#questionTableBody tr")
        .forEach(row => {
            const rowText =
                row.innerText.toLowerCase();

            const rowType =
                row.querySelector(".question-type")?.innerText.trim();

            const matchSearch =
                rowText.includes(keyword);

            const matchType =
                typeFilter === "" || rowType === typeFilter;

            row.style.display =
                matchSearch && matchType ? "" : "none";
        });
}

function escapeHtml(value) {
    if (!value) return "";

    return value
        .replaceAll("&", "&amp;")
        .replaceAll("<", "&lt;")
        .replaceAll(">", "&gt;")
        .replaceAll('"', "&quot;")
        .replaceAll("'", "&#039;");
}

function escapeAttr(value) {
    return escapeHtml(value);
}