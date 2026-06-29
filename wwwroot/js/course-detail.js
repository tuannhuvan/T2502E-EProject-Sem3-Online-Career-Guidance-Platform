document.addEventListener("DOMContentLoaded", function () {

    const countdownElement = document.getElementById("countdown");

    if (!countdownElement) return;

    const deadlineText = countdownElement.dataset.deadline;
    const deadline = new Date(deadlineText);

    function updateCountdown() {

        const now = new Date();
        const diff = deadline - now;

        if (diff <= 0) {
            countdownElement.innerHTML = "Đã hết thời gian";
            countdownElement.style.color = "#dc2626";
            return;
        }

        const days = Math.floor(diff / (1000 * 60 * 60 * 24));
        const hours = Math.floor((diff / (1000 * 60 * 60)) % 24);
        const minutes = Math.floor((diff / (1000 * 60)) % 60);
        const seconds = Math.floor((diff / 1000) % 60);

        countdownElement.innerHTML =
            `${days} ngày ${hours} giờ ${minutes} phút ${seconds} giây`;
    }

    updateCountdown();

    setInterval(updateCountdown, 1000);

});