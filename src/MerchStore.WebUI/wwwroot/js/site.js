// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// 🍔 Toggle dropdown menu with animation
const dropdownToggle = document.getElementById('dropdownToggleButton'); // Your toggle button's ID
const dropdownMenu = document.getElementById('dropdownNavbar');

if (dropdownToggle && dropdownMenu) {
    dropdownToggle.addEventListener('click', () => {
        dropdownMenu.classList.remove('hidden');
        dropdownMenu.classList.add('animate-in');

        // Optional: hide it again on second click (toggle mode)
        if (dropdownMenu.classList.contains('showing')) {
            dropdownMenu.classList.add('hidden');
            dropdownMenu.classList.remove('showing', 'animate-in');
        } else {
            dropdownMenu.classList.add('showing');
        }
    });
}

var bsModal = new bootstrap.Modal(modalEl, {
    backdrop: 'static',
    keyboard: true
});

