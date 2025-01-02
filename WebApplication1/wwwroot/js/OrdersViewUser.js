document.addEventListener("DOMContentLoaded", function () {
    const checkboxes = document.querySelectorAll(".orderCheckbox");
    const cancelButton = document.querySelector(".btn-danger");

    function toggleButton() {
        cancelButton.disabled = !Array.from(checkboxes).some(checkbox => checkbox.checked);
    }

    checkboxes.forEach(checkbox => checkbox.addEventListener("change", toggleButton));
    toggleButton();
});