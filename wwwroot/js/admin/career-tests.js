let currentRow = null;

document.addEventListener("DOMContentLoaded", function () {
    document.getElementById("btnAddQuestion")?.addEventListener("click", openAddModal);
    document.getElementById("btnSaveQuestion")?.addEventListener("click", saveQuestion);
    document.getElementById("searchInput")?.addEventListener("keyup", applyFilters);
    document.getElementById("filterType")?.addEventListener("change", applyFilters);

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

    document.getElementById("modalTitle").innerText = "Create Question";
    document.getElementById("testInput").value = "Holland Test";
    document.getElementById("groupInput").value = "Interest";
    document.getElementById("questionInput").value = "";
    document.getElementById("statusInput").value = "Active";

    clearOptions();
}

function openEditModal(btn) {
    currentRow = btn.closest("tr");

    if (!currentRow) return;

    const test =
        currentRow.querySelector(".question-test")?.innerText.trim() || "Holland Test";

    const question =
        currentRow.querySelector(".question-content")?.innerText.trim() || "";

    const type =
        currentRow.querySelector(".question-type")?.innerText.trim() || "Interest";

    const status =
        currentRow.querySelector(".question-status")?.innerText.trim() || "Active";

    const optionCell =
        currentRow.querySelector(".question-options");

    document.getElementById("modalTitle").innerText = "Edit Question";
    document.getElementById("testInput").value = test;
    document.getElementById("questionInput").value = question;
    document.getElementById("groupInput").value = type;
    document.getElementById("statusInput").value = status;

    for (let i = 1; i <= 4; i++) {
        document.querySelectorAll(".option-input")[i - 1].value =
            optionCell?.dataset[`option${i}`] || "";

        document.querySelectorAll(".career-path-input")[i - 1].value =
            optionCell?.dataset[`career${i}`] || "";

        document.querySelectorAll(".weight-input")[i - 1].value =
            optionCell?.dataset[`weight${i}`] || "";
    }

    const modal = bootstrap.Modal.getOrCreateInstance(
        document.getElementById("questionModal")
    );

    modal.show();
}

function saveQuestion() {
    const test = document.getElementById("testInput").value;
    const question = document.getElementById("questionInput").value.trim();
    const type = document.getElementById("groupInput").value;
    const status = document.getElementById("statusInput").value;

    if (!question) {
        alert("Please enter question content.");
        return;
    }

    const options = collectOptions();
    const optionCount = options.filter(x => x.content !== "").length;

    const badgeClass =
        status === "Active"
            ? "bg-green-lt"
            : "bg-red-lt";

    const rowHtml = `
        <td class="question-test">${escapeHtml(test)}</td>

        <td class="question-content">${escapeHtml(question)}</td>

        <td class="question-type">${escapeHtml(type)}</td>

        <td class="question-options"
            data-option1="${escapeAttr(options[0].content)}"
            data-career1="${escapeAttr(options[0].career)}"
            data-weight1="${escapeAttr(options[0].weight)}"
            data-option2="${escapeAttr(options[1].content)}"
            data-career2="${escapeAttr(options[1].career)}"
            data-weight2="${escapeAttr(options[1].weight)}"
            data-option3="${escapeAttr(options[2].content)}"
            data-career3="${escapeAttr(options[2].career)}"
            data-weight3="${escapeAttr(options[2].weight)}"
            data-option4="${escapeAttr(options[3].content)}"
            data-career4="${escapeAttr(options[3].career)}"
            data-weight4="${escapeAttr(options[3].weight)}">
            ${optionCount} Options
        </td>

        <td class="question-status">
            <span class="badge ${badgeClass}">
                ${escapeHtml(status)}
            </span>
        </td>

        <td class="text-end">
            <div class="btn-list justify-content-end">
                <button class="btn btn-sm btn-outline-primary btn-edit-question" type="button">
                    Edit
                </button>

                <button class="btn btn-sm btn-outline-danger btn-delete-question" type="button">
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
        document.getElementById("questionTableBody").appendChild(tr);
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

function collectOptions() {
    const optionInputs = document.querySelectorAll(".option-input");
    const careerInputs = document.querySelectorAll(".career-path-input");
    const weightInputs = document.querySelectorAll(".weight-input");

    const options = [];

    for (let i = 0; i < 4; i++) {
        options.push({
            content: optionInputs[i]?.value.trim() || "",
            career: careerInputs[i]?.value || "",
            weight: weightInputs[i]?.value || ""
        });
    }

    return options;
}

function clearOptions() {
    document.querySelectorAll(".option-input")
        .forEach(input => input.value = "");

    document.querySelectorAll(".career-path-input")
        .forEach(select => select.value = "");

    document.querySelectorAll(".weight-input")
        .forEach(input => input.value = "");
}

function applyFilters() {
    const keyword =
        document.getElementById("searchInput").value.toLowerCase();

    const typeFilter =
        document.getElementById("filterType").value;

    document.querySelectorAll("#questionTableBody tr")
        .forEach(row => {
            const rowText = row.innerText.toLowerCase();
            const rowType = row.querySelector(".question-type")?.innerText.trim();

            const matchSearch = rowText.includes(keyword);
            const matchType = typeFilter === "" || rowType === typeFilter;

            row.style.display = matchSearch && matchType ? "" : "none";
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