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
}0