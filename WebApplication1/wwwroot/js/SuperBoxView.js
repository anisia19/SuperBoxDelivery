document.getElementById('selectAll').addEventListener('change', function () {
    var checkboxes = document.querySelectorAll('.orderCheckbox');
    for (var checkbox of checkboxes) {
        checkbox.checked = this.checked;
    }
    setTimeout(function() {
        location.reload();
    }, 1000);
});
