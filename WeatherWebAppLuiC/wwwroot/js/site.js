// JS Code will be placed here.
// JS code the for the popup when the user enters the web app.
document.addEventListener('DOMContentLoaded', function () {
    var modal = document.getElementById('popupModal');
    var closeMsgBtn = modal.querySelector('button.close-button');
    modal.style.display = 'block';
    if (closeMsgBtn) {
        closeMsgBtn.onclick = function () {
            modal.style.display = 'none';
        };
    }
});