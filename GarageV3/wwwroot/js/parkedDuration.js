updateParkedDurations();
updateTotalPrices()

setInterval(() => {
    updateParkedDurations();
    updateTotalPrices();
}, 60000);