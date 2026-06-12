const questions = [
    {
        group: "Sở thích nghề nghiệp",
        title: "Bạn cảm thấy hứng thú nhất với hoạt động nào?",
        answers: [
            {
                text: "Lập trình và giải quyết vấn đề",
                desc: "Tôi thích xây dựng ứng dụng, làm việc với công nghệ và xử lý logic.",
                type: "software"
            },
            {
                text: "Thiết kế giao diện và trải nghiệm người dùng",
                desc: "Tôi thích màu sắc, bố cục, giao diện đẹp và dễ sử dụng.",
                type: "uiux"
            },
            {
                text: "Phân tích yêu cầu và làm việc với khách hàng",
                desc: "Tôi thích giao tiếp, hỏi yêu cầu và chuyển thành giải pháp.",
                type: "ba"
            },
            {
                text: "Phân tích số liệu và tìm xu hướng",
                desc: "Tôi thích dữ liệu, biểu đồ và đưa ra kết luận từ số liệu.",
                type: "data"
            }
        ]
    },
    {
        group: "Kỹ năng cá nhân",
        title: "Bạn tự tin nhất với kỹ năng nào?",
        answers: [
            {
                text: "Tư duy logic",
                desc: "Tôi có khả năng chia nhỏ vấn đề và xử lý từng bước.",
                type: "software"
            },
            {
                text: "Sáng tạo hình ảnh",
                desc: "Tôi có khả năng phối màu, chọn bố cục và thiết kế trực quan.",
                type: "uiux"
            },
            {
                text: "Giao tiếp và trình bày",
                desc: "Tôi có thể giải thích ý tưởng rõ ràng cho người khác.",
                type: "ba"
            },
            {
                text: "Xử lý bảng tính và báo cáo",
                desc: "Tôi thích Excel, SQL, biểu đồ và dashboard.",
                type: "data"
            }
        ]
    },
    {
        group: "Tính cách làm việc",
        title: "Bạn thích môi trường làm việc nào nhất?",
        answers: [
            {
                text: "Tập trung code và giải quyết bug",
                desc: "Tôi thích làm việc sâu, xử lý lỗi và tối ưu hệ thống.",
                type: "software"
            },
            {
                text: "Thử nghiệm nhiều ý tưởng thiết kế",
                desc: "Tôi thích tạo prototype và cải thiện trải nghiệm người dùng.",
                type: "uiux"
            },
            {
                text: "Làm việc với nhiều phòng ban",
                desc: "Tôi thích kết nối giữa khách hàng, dev và tester.",
                type: "ba"
            },
            {
                text: "Làm việc với dữ liệu và con số",
                desc: "Tôi thích phân tích và đưa ra insight từ dữ liệu.",
                type: "data"
            }
        ]
    },
    {
        group: "Giá trị nghề nghiệp",
        title: "Điều gì quan trọng nhất với bạn khi chọn nghề?",
        answers: [
            {
                text: "Tạo ra sản phẩm công nghệ thực tế",
                desc: "Tôi muốn xây dựng hệ thống có thể chạy và phục vụ người dùng.",
                type: "software"
            },
            {
                text: "Tạo ra trải nghiệm đẹp và dễ dùng",
                desc: "Tôi muốn sản phẩm trở nên trực quan và thân thiện.",
                type: "uiux"
            },
            {
                text: "Giúp doanh nghiệp giải quyết vấn đề",
                desc: "Tôi muốn hiểu vấn đề và đề xuất giải pháp phù hợp.",
                type: "ba"
            },
            {
                text: "Đưa ra quyết định dựa trên dữ liệu",
                desc: "Tôi muốn dùng dữ liệu để hỗ trợ chiến lược.",
                type: "data"
            }
        ]
    },
    {
        group: "Công cụ yêu thích",
        title: "Bạn muốn học công cụ nào nhất?",
        answers: [
            {
                text: "C#, ASP.NET Core, SQL Server",
                desc: "Tôi muốn phát triển website và hệ thống backend.",
                type: "software"
            },
            {
                text: "Figma, Photoshop, Design System",
                desc: "Tôi muốn thiết kế giao diện và prototype.",
                type: "uiux"
            },
            {
                text: "Draw.io, Jira, Requirement Document",
                desc: "Tôi muốn phân tích nghiệp vụ và viết tài liệu.",
                type: "ba"
            },
            {
                text: "Python, Power BI, Excel, SQL",
                desc: "Tôi muốn xử lý và trực quan hóa dữ liệu.",
                type: "data"
            }
        ]
    },
    {
        group: "Cách giải quyết vấn đề",
        title: "Khi gặp một vấn đề khó, bạn thường làm gì?",
        answers: [
            {
                text: "Tìm nguyên nhân và debug từng bước",
                desc: "Tôi thích tìm lỗi, kiểm tra logic và sửa vấn đề.",
                type: "software"
            },
            {
                text: "Phác thảo nhiều phương án giao diện",
                desc: "Tôi thích thử nhiều layout để chọn phương án tốt nhất.",
                type: "uiux"
            },
            {
                text: "Hỏi thêm thông tin từ người liên quan",
                desc: "Tôi muốn hiểu rõ nhu cầu trước khi đưa ra giải pháp.",
                type: "ba"
            },
            {
                text: "Thu thập dữ liệu để kiểm chứng",
                desc: "Tôi muốn có số liệu trước khi kết luận.",
                type: "data"
            }
        ]
    },
    {
        group: "Mục tiêu phát triển",
        title: "Bạn muốn trở thành kiểu chuyên gia nào?",
        answers: [
            {
                text: "Developer xây dựng sản phẩm",
                desc: "Tôi muốn tạo ra website, app và hệ thống phần mềm.",
                type: "software"
            },
            {
                text: "Designer tạo trải nghiệm người dùng",
                desc: "Tôi muốn thiết kế sản phẩm đẹp, tiện lợi và dễ hiểu.",
                type: "uiux"
            },
            {
                text: "Business Analyst kết nối nghiệp vụ và kỹ thuật",
                desc: "Tôi muốn phân tích yêu cầu và làm cầu nối cho team.",
                type: "ba"
            },
            {
                text: "Data Analyst phân tích dữ liệu",
                desc: "Tôi muốn biến dữ liệu thành báo cáo và insight.",
                type: "data"
            }
        ]
    },
    {
        group: "Sở thích học tập",
        title: "Bạn thích học theo cách nào?",
        answers: [
            {
                text: "Làm project thực tế",
                desc: "Tôi học tốt nhất khi tự xây dựng chức năng hoàn chỉnh.",
                type: "software"
            },
            {
                text: "Xem mẫu thiết kế và làm lại",
                desc: "Tôi học tốt qua giao diện mẫu, layout và case study.",
                type: "uiux"
            },
            {
                text: "Đọc tình huống nghiệp vụ",
                desc: "Tôi học tốt qua requirement, process và workflow.",
                type: "ba"
            },
            {
                text: "Thực hành với dataset",
                desc: "Tôi học tốt khi xử lý dữ liệu thật.",
                type: "data"
            }
        ]
    }
];

const careerInfo = {
    software: {
        title: "Software Engineer",
        icon: "💻",
        desc: "Phù hợp với người thích lập trình, tư duy logic và xây dựng sản phẩm công nghệ.",
        skills: ["C#", "ASP.NET Core", "SQL Server", "Git", "Problem Solving"]
    },
    uiux: {
        title: "UI/UX Designer",
        icon: "🎨",
        desc: "Phù hợp với người thích thiết kế, sáng tạo giao diện và tối ưu trải nghiệm người dùng.",
        skills: ["Figma", "User Research", "Wireframe", "Prototype", "Design System"]
    },
    ba: {
        title: "Business Analyst",
        icon: "📋",
        desc: "Phù hợp với người thích giao tiếp, phân tích yêu cầu và kết nối giữa business và technical team.",
        skills: ["Requirement Analysis", "Draw.io", "UML", "Communication", "Documentation"]
    },
    data: {
        title: "Data Analyst",
        icon: "📊",
        desc: "Phù hợp với người thích số liệu, phân tích dữ liệu và tạo báo cáo hỗ trợ quyết định.",
        skills: ["Excel", "SQL", "Power BI", "Python", "Data Visualization"]
    }
};

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
    questionGroup.textContent = `Nhóm: ${question.group}`;
    questionTitle.textContent = question.title;

    answerList.innerHTML = "";

    question.answers.forEach((answer, index) => {
        const label = document.createElement("label");

        label.innerHTML = `
            <input type="radio" name="answer" value="${answer.type}" ${answers[currentQuestion] === answer.type ? "checked" : ""}>
            <div>
                <strong>${answer.text}</strong>
                <p>${answer.desc}</p>
            </div>
        `;

        label.addEventListener("click", () => {
            answers[currentQuestion] = answer.type;
        });

        answerList.appendChild(label);
    });

    prevBtn.disabled = currentQuestion === 0;
    prevBtn.style.opacity = currentQuestion === 0 ? "0.5" : "1";

    nextBtn.textContent = currentQuestion === questions.length - 1 ? "Xem kết quả" : "Tiếp tục";

    updateProgress();
}

function updateProgress() {
    const answeredCount = answers.filter(answer => answer !== null).length;
    const percent = Math.round((answeredCount / questions.length) * 100);

    progressText.textContent = `Hoàn thành ${percent}%`;
    progressBar.style.width = `${percent}%`;
}

function calculateResult() {
    const scores = {
        software: 0,
        uiux: 0,
        ba: 0,
        data: 0
    };

    answers.forEach(answer => {
        if (answer) {
            scores[answer]++;
        }
    });

    const result = Object.keys(scores)
        .map(type => ({
            type,
            score: scores[type],
            percent: Math.round((scores[type] / questions.length) * 100)
        }))
        .sort((a, b) => b.score - a.score);

    return result;
}

function renderResult() {
    const results = calculateResult();

    questionCard.classList.add("hidden");
    resultBox.classList.remove("hidden");

    resultGrid.innerHTML = "";

    results.forEach(item => {
        const career = careerInfo[item.type];

        const card = document.createElement("div");
        card.className = "result-card career-result-card";

        card.innerHTML = `
            <div class="career-result-icon">${career.icon}</div>
            <h4>${career.title}</h4>
            <div class="score-bar">
                <span style="width:${item.percent}%"></span>
            </div>
            <p>${item.percent}% phù hợp</p>
            <small>${career.desc}</small>
            <div class="mini-skill-list">
                ${career.skills.map(skill => `<span>${skill}</span>`).join("")}
            </div>
        `;

        resultGrid.appendChild(card);
    });

    progressText.textContent = "Hoàn thành 100%";
    progressBar.style.width = "100%";
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

renderQuestion();