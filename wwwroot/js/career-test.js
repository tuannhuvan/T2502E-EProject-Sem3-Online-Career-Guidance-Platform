// career-test.js
// This script supports two modes:
// - server-driven: serverQuestions + serverTestId injected by Razor
// - fallback: static local `questions` (your previous array); you already had static data earlier

// If you have a local static 'questions' array above in file, keep it as fallback.
// If serverQuestions is present, override questions.

let questions = [];

if (typeof serverQuestions !== 'undefined' && Array.isArray(serverQuestions) && serverQuestions.length > 0) {
    // Normalize serverQuestions items to the shape we use below: { group, title, answers: [{ id, text, desc? }] }
    questions = serverQuestions.map(q => ({
        group: q.group ?? q.Group ?? '',
        title: q.content ?? q.Content ?? '',
        questionId: q.questionId ?? q.QuestionId ?? 0,
        answers: (q.options ?? q.Options ?? []).map(opt => ({
            id: opt.optionId ?? opt.OptionId ?? 0,
            text: opt.content ?? opt.Content ?? '',
            desc: opt.description ?? opt.Description ?? opt.desc ?? opt.Desc ?? ''
        }))
    }));
} else {
    // If you have an inline `questions` array defined earlier in this file, keep it.
    // Otherwise, ensure `questions` is defined above with the required shape.
}

// UI elements
let currentQuestion = 0;
let answers = new Array(questions.length).fill(null);

const questionCounter = document.getElementById("questionCounter");
const questionGroup = document.getElementById("questionGroup");
const questionTitle = document.getElementById("questionTitle");
const answerList = document.getElementById("answerList");
const progressText = document.getElementById("progressText");
const progressBar = document.getElementById("progressBar");

const prevBtn = document.getElementById("prevBtn");
const nextBtn = document.getElementById("nextBtn");
const restartBtn = document.getElementById("restartBtn");

const questionCard = document.getElementById("questionCard");
const resultBox = document.getElementById("resultBox");
const resultGrid = document.getElementById("resultGrid");

function renderQuestion() {
    const question = questions[currentQuestion];

    questionCounter.textContent = `Câu hỏi ${currentQuestion + 1}/${questions.length}`;
    questionGroup.textContent = `Nhóm: ${question.group || ''}`;
    questionTitle.textContent = question.title || '';

    answerList.innerHTML = "";

    question.answers.forEach((answer, index) => {
        const label = document.createElement("label");
        label.className = "answer-item";

        // build radio with option id as value
        label.innerHTML = `
            <input type="radio" name="answer" value="${answer.id}" ${answers[currentQuestion] === answer.id ? "checked" : ""}>
            <div>
                <strong>${answer.text}</strong>
                ${answer.desc ? `<p>${answer.desc}</p>` : ''}
            </div>
        `;

        label.addEventListener("click", () => {
            answers[currentQuestion] = answer.id;
            updateProgress();
        });

        answerList.appendChild(label);
    });

    prevBtn.disabled = currentQuestion === 0;
    prevBtn.style.opacity = currentQuestion === 0 ? "0.5" : "1";

    nextBtn.textContent = currentQuestion === questions.length - 1 ? "Hoàn thành bài test" : "Tiếp tục";

    updateProgress();
}

function updateProgress() {
    const answeredCount = answers.filter(a => a !== null).length;
    const percent = Math.round((answeredCount / questions.length) * 100);

    if (progressText) progressText.textContent = `Hoàn thành ${percent}%`;
    if (progressBar) progressBar.style.width = `${percent}%`;
}

function calculateResult() {
    const scores = {};
    questions.forEach((q, idx) => {
        const ans = answers[idx];
        if (!ans) return;
        scores[ans] = (scores[ans] || 0) + 1;
    });

    const result = Object.keys(scores).map(k => ({ id: k, score: scores[k] }));
    return result;
}

function renderResult() {
    questionCard.classList.add("hidden");
    resultBox.classList.remove("hidden");

    // Hide result grid
    resultGrid.innerHTML = "";
    resultGrid.style.display = "none";

    // Set title and description
    const sectionTitle = resultBox.querySelector('.section-title');
    if (sectionTitle) {
        sectionTitle.querySelector('span').textContent = "📝 Hoàn thành bài đánh giá";
        sectionTitle.querySelector('h3').textContent = "Chúc mừng bạn đã hoàn thành!";
    }

    // Set description to prompt submission
    const desc = resultBox.querySelector('.question-desc') || document.createElement('p');
    desc.className = 'question-desc';
    desc.style.textAlign = 'center';
    desc.style.marginBottom = '24px';
    desc.style.color = '#64748b';
    desc.textContent = "Bạn đã hoàn thành 20 câu hỏi hướng nghiệp. Hãy nhấn nút 'Nộp bài' bên dưới để hệ thống xử lý và phân tích lộ trình sự nghiệp phù hợp nhất cho bạn.";
    if (!resultBox.querySelector('.question-desc')) {
        resultBox.insertBefore(desc, resultBox.querySelector('.result-actions') || resultGrid);
    }

    // Replace action buttons to display ONLY "Nộp bài"
    const resultActions = resultBox.querySelector('.result-actions');
    if (resultActions) {
        resultActions.innerHTML = "";
        
        const submitBtn = document.createElement('button');
        submitBtn.id = 'serverSubmitBtn';
        submitBtn.className = 'primary-btn';
        submitBtn.type = 'button';
        submitBtn.style.padding = '14px 48px';
        submitBtn.style.fontSize = '16px';
        submitBtn.style.fontWeight = '700';
        submitBtn.textContent = 'Nộp bài';
        submitBtn.addEventListener('click', submitAnswersToServer);
        
        resultActions.appendChild(submitBtn);
    }

    updateProgress();
}

nextBtn.addEventListener("click", () => {
    if (!answers[currentQuestion]) {
        alert("Bạn cần chọn một đáp án trước khi tiếp tục.");
        return;
    }

    if (currentQuestion < questions.length - 1) {
        currentQuestion++;
        renderQuestion();
    } else {
        renderResult();
    }
});

prevBtn.addEventListener("click", () => {
    if (currentQuestion > 0) {
        currentQuestion--;
        renderQuestion();
    }
});

restartBtn.addEventListener("click", () => {
    currentQuestion = 0;
    answers = new Array(questions.length).fill(null);
    resultBox.classList.add("hidden");
    questionCard.classList.remove("hidden");
    renderQuestion();
});

function getAntiforgeryTokenValue() {
    const tokenInput = document.querySelector('#__antiForgeryForm input[name="__RequestVerificationToken"]');
    return tokenInput ? tokenInput.value : null;
}

function submitAnswersToServer() {
    const totalQuestions = questions.length;
    const answeredCount = answers.filter(a => a != null).length;

    if (answeredCount < totalQuestions) {
        alert(`Bạn mới làm được ${answeredCount}/${totalQuestions} câu. Vui lòng hoàn thành tất cả câu hỏi trước khi nộp!`);
        return;
    }

    // Build form for MVC model binding
    const form = document.createElement('form');
    form.method = 'post';
    form.action = '/CareerTest/SubmitAnswers';

    // antiforgery token
    const token = getAntiforgeryTokenValue();
    if (token) {
        const t = document.createElement('input');
        t.type = 'hidden';
        t.name = '__RequestVerificationToken';
        t.value = token;
        form.appendChild(t);
    } else {
        console.warn('CSRF token not found. Make sure @Html.AntiForgeryToken() is present.');
    }

    // TestId
    const testIdInput = document.createElement('input');
    testIdInput.type = 'hidden';
    testIdInput.name = 'TestId';
    testIdInput.value = (typeof serverTestId !== 'undefined') ? serverTestId.toString() : '';
    form.appendChild(testIdInput);

    // Answers[i].QuestionId and Answers[i].OptionId
    for (let i = 0; i < questions.length; i++) {
        const q = questions[i];
        const qId = q.questionId ?? q.QuestionId ?? (i + 1);
        const optionId = answers[i] ?? '';

        const qInput = document.createElement('input');
        qInput.type = 'hidden';
        qInput.name = `Answers[${i}].QuestionId`;
        qInput.value = qId;
        form.appendChild(qInput);

        const oInput = document.createElement('input');
        oInput.type = 'hidden';
        oInput.name = `Answers[${i}].OptionId`;
        oInput.value = optionId;
        form.appendChild(oInput);
    }

    document.body.appendChild(form);
    form.submit();
}

// initial render
renderQuestion();