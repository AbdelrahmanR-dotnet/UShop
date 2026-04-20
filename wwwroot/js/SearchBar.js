document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.querySelector(".search-bar input");

    if (searchInput) {
        searchInput.addEventListener("input", () => {
            searchInput.style.borderColor = searchInput.value.length > 0 ? "#28a745" : "#ddd";
        });
    }
});
