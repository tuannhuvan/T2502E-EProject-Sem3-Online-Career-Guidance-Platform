function switchTab(tabId) {
    document.querySelectorAll(".tab-pane").forEach(pane => {
        pane.classList.remove("active");
    });

    document.querySelectorAll(".tab-nav-btn").forEach(btn => {
        btn.classList.remove("active");
    });

    document.getElementById(tabId)?.classList.add("active");
    document.getElementById("btn-" + tabId)?.classList.add("active");
}

function getSelectedSkills() {
    return Array.from(document.querySelectorAll(".cv-skill-checkbox:checked"))
        .map(cb => cb.value)
        .filter(v => v && v.trim() !== "");
}

function submitUpdateCV(event) {
    const skills = getSelectedSkills();

    if (skills.length === 0) {
        alert("Vui lòng chọn ít nhất một kỹ năng.");
        event.preventDefault();
        return;
    }

    const container = document.getElementById("hiddenSkillsContainerUpdate");
    container.innerHTML = "";

    skills.forEach(skill => {
        const input = document.createElement("input");
        input.type = "hidden";
        input.name = "selectedSkills";
        input.value = skill;
        container.appendChild(input);
    });
}

function submitCreateCV(event) {
    const skills = getSelectedSkills();

    if (skills.length === 0) {
        alert("Vui lòng chọn ít nhất một kỹ năng.");
        event.preventDefault();
        return;
    }

    const container = document.getElementById("hiddenSkillsContainer");
    container.innerHTML = "";

    skills.forEach(skill => {
        const input = document.createElement("input");
        input.type = "hidden";
        input.name = "selectedSkills";
        input.value = skill;
        container.appendChild(input);
    });
}

document.addEventListener("DOMContentLoaded", function () {
    initSkillPagination();
});

function initSkillPagination() {
    const items = document.querySelectorAll(".skill-inventory-item");
    const paginationContainer = document.getElementById("skill-inventory-pagination");
    if (!paginationContainer || items.length === 0) return;

    const pageSize = 6;
    let currentPage = 1;
    const totalPages = Math.ceil(items.length / pageSize);

    function showPage(page) {
        currentPage = page;
        const start = (page - 1) * pageSize;
        const end = start + pageSize;

        items.forEach((item, index) => {
            if (index >= start && index < end) {
                item.style.display = "flex";
            } else {
                item.style.display = "none";
            }
        });

        renderControls();
    }

    function renderControls() {
        paginationContainer.innerHTML = "";

        // Prev Button
        const prevBtn = document.createElement("button");
        prevBtn.innerText = "Trước";
        prevBtn.disabled = currentPage === 1;
        prevBtn.addEventListener("click", () => showPage(currentPage - 1));
        paginationContainer.appendChild(prevBtn);

        // Page Numbers
        for (let i = 1; i <= totalPages; i++) {
            const pageBtn = document.createElement("button");
            pageBtn.innerText = i;
            if (i === currentPage) {
                pageBtn.classList.add("active");
            }
            pageBtn.addEventListener("click", () => showPage(i));
            paginationContainer.appendChild(pageBtn);
        }

        // Next Button
        const nextBtn = document.createElement("button");
        nextBtn.innerText = "Sau";
        nextBtn.disabled = currentPage === totalPages;
        nextBtn.addEventListener("click", () => showPage(currentPage + 1));
        paginationContainer.appendChild(nextBtn);
    }

    showPage(1);
}