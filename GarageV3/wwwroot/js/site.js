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

// function calculateTotalPrice(arrivalTimeString, hourlyRate) {
//     const arrival = new Date(arrivalTimeString);
//     const now = new Date();

//     const durationMs = now - arrival;
//     const durationHours = durationMs / (1000 * 60 * 60);

//     const totalPrice = durationHours * hourlyRate;

//     return totalPrice.toFixed(2);
// }

// function updateTotalPrices() {
//     const cells = document.querySelectorAll(".total-price-cell");

//     cells.forEach(cell => {
//         const arrival = cell.dataset.arrivalTime;
//         const rate = parseFloat(cell.dataset.hourlyRate);

//         const price = calculateTotalPrice(arrival, rate);

//         cell.textContent = price + " kr";
//     });
// }

async function calculateTotalPrice(arrivalTimeString, hourlyRate, isPro) {
    try {
        const url = `/api/parking/calculate-fee?arrivalTime=${encodeURIComponent(arrivalTimeString)}&hourlyRate=${encodeURIComponent(hourlyRate)}&isPro=${encodeURIComponent(isPro)}`;
        const response = await fetch(url);

        if (!response.ok) {
            throw new Error("Cannot fetch fee");
        }

        const data = await response.json();
        return data.totalPrice.toFixed(2);
    } catch (error) {
        console.error("Error while calculating price:", error);
        return "0.00";
    }
}

async function updateTotalPrices() {
    const cells = document.querySelectorAll(".total-price-cell");
    if (!cells) return;

    await Promise.all(Array.from(cells).map(async (cell) => {
        const arrival = cell.dataset.arrivalTime;
        const hourlyRate = Number(cell.dataset.hourlyRate);
        const isPro = cell.dataset.isPro === 'true';

        const price = await calculateTotalPrice(arrival, hourlyRate, isPro);
        cell.textContent = price + " kr";
    }));
}

// 1. Convert initial UTC time labels to user's browser local time
document.querySelectorAll(".local-time-display").forEach(el => {
    const utcString = el.dataset.utc;
    if (utcString) {
        const localDate = new Date(utcString);

        el.textContent = localDate.toLocaleString([], {
            weekday: 'long',
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    }
});