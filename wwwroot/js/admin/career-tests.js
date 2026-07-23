function openAddModal() {
    document.getElementById("modalTitle").innerText = "Create Question";
    document.getElementById("editingQuestionId").value = "";
    
    // Set default values for first item in dropdowns or fallback
    const testSelect = document.getElementById("testInput");
    if (testSelect.options.length > 0) {
        testSelect.selectedIndex = 0;
    }
    document.getElementById("groupInput").value = "Interests";
    document.getElementById("questionInput").value = "";
    document.getElementById("statusInput").value = "Active";

    clearOptions();
}

function openEditModal(btn) {
    const row = btn.closest("tr");
    if (!row) return;

    const questionId = row.getAttribute("data-id");
    const question = row.querySelector(".question-content")?.innerText.trim() || "";
    
    const optionCell = row.querySelector(".question-options");
    const testId = optionCell.getAttribute("data-test-id");
    const testType = optionCell.getAttribute("data-test-type") || "Interests";
    const status = optionCell.getAttribute("data-status") || "Active";

    document.getElementById("modalTitle").innerText = "Edit Question";
    document.getElementById("editingQuestionId").value = questionId;
    document.getElementById("testInput").value = testId;
    document.getElementById("questionInput").value = question;
    document.getElementById("groupInput").value = testType;
    document.getElementById("statusInput").value = status;

    const optionsData = JSON.parse(optionCell.getAttribute("data-options") || "[]");

    const optionInputs = document.querySelectorAll(".option-input");
    const careerInputs = document.querySelectorAll(".career-path-input");
    const weightInputs = document.querySelectorAll(".weight-input");

    clearOptions();

    for (let i = 0; i < 4; i++) {
        if (optionsData[i]) {
            optionInputs[i].value = optionsData[i].content || "";
            careerInputs[i].value = optionsData[i].career || "";
            weightInputs[i].value = optionsData[i].weight || "";
        }
    }
}

function saveQuestion() {
    const content = document.getElementById("questionInput").value.trim();
    const testId = document.getElementById("testInput").value;
    const testType = document.getElementById("groupInput").value;
    const status = document.getElementById("statusInput").value;
    const editingId = document.getElementById("editingQuestionId").value;

    if (!content) {
        alert("Please enter the question content.");
        return;
    }

    const optionInputs = document.querySelectorAll(".option-input");
    const careerInputs = document.querySelectorAll(".career-path-input");
    const weightInputs = document.querySelectorAll(".weight-input");
    const options = [];

    let hasEmptyOptionContent = false;
    for (let i = 0; i < 4; i++) {
        const text = optionInputs[i].value.trim();
        if (text) {
            const pathVal = careerInputs[i].value;
            const weightVal = weightInputs[i].value;
            
            options.push({
                content: text,
                careerPathId: pathVal ? parseInt(pathVal) : null,
                weight: weightVal ? parseInt(weightVal) : 1
            });
        }
    }

    if (options.length === 0) {
        alert("Please provide at least one answer option.");
        return;
    }

    const payload = {
        testId: parseInt(testId),
        content: content,
        testType: testType,
        status: status,
        options: options
    };

    if (editingId) {
        payload.id = parseInt(editingId);
    }

    fetch('/Admin/SaveQuestion', {
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
        alert('Failed to save question.');
    });
}

function deleteQuestion(id) {
    if (confirm("Are you sure you want to delete this question? This action will permanently remove all linked answers and options.")) {
        fetch('/Admin/DeleteQuestion?id=' + id, {
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
            alert('Failed to delete question.');
        });
    }
}

function clearOptions() {
    document.querySelectorAll(".option-input").forEach(input => input.value = "");
    document.querySelectorAll(".career-path-input").forEach(select => select.value = "");
    document.querySelectorAll(".weight-input").forEach(input => input.value = "");
}

function applyFilters() {
    const search = document.getElementById("searchInput").value.trim();
    const type = document.getElementById("filterType").value;
    window.location.href = `/Admin/CareerTests?page=1&search=${encodeURIComponent(search)}&type=${encodeURIComponent(type)}`;
}