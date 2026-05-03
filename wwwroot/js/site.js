// Auto-dismiss alerts after 4s
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        document.querySelectorAll('.alert').forEach(function (a) {
            var bsAlert = new bootstrap.Alert(a);
            bsAlert.close();
        });
    }, 4000);
});
