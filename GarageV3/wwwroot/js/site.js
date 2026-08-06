// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function updateClock() {
    const now = new Date();

    const weekdays = [
        "Sunday", "Monday", "Tuesday", "Wednesday",
        "Thursday", "Friday", "Saturday"
    ];

    const weekday = weekdays[now.getDay()];
    const year = now.getFullYear();
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    const hours = String(now.getHours()).padStart(2, '0');
    const minutes = String(now.getMinutes()).padStart(2, '0');
    const seconds = String(now.getSeconds()).padStart(2, '0');

    const timeString = `${weekday} ${year}-${month}-${day} ${hours}:${minutes}:${seconds}`;

    document.getElementById('liveClock').textContent = timeString;
}

function getParkedDuration(arrivalTimeString) {
    const arrival = new Date(arrivalTimeString);
    const now = new Date();

    const duration = now - arrival; // milliseconds

    const days = Math.floor(duration / (1000 * 60 * 60 * 24));
    const hours = Math.floor((duration / (1000 * 60 * 60)) % 24);
    const minutes = Math.floor((duration / (1000 * 60)) % 60);

    return `${days} d ${hours} h ${minutes} m`;
}

function updateParkedDurations() {
    const cells = document.querySelectorAll(".duration-cell");

    cells.forEach(cell => {
        const display = getParkedDuration(cell.dataset.arrivalTime);

        cell.textContent = display;
    });
}

function fadeOutSuccessAlert() {
    const alertBox = document.getElementById("successAlert");
    if (!alertBox) return;

    setTimeout(() => {
        alertBox.style.transition = "opacity 0.5s";
        alertBox.style.opacity = "0";

        setTimeout(() => alertBox.remove(), 500);
    }, 3000);
}