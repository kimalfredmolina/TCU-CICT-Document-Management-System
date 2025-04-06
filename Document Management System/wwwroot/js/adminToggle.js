document.addEventListener("DOMContentLoaded", function () {
    // Sidebar toggle
    const menuToggle = document.querySelector('.menu-toggle');
    const dashboardNav = document.querySelector('.dashboard-nav');

    menuToggle.addEventListener('click', () => {
        dashboardNav.classList.toggle('hidden');
    });

    // Dropdown toggle (for both label and arrow)
    const dropdownToggles = document.querySelectorAll('.dashboard-nav-dropdown-toggle');

    dropdownToggles.forEach(toggle => {
        toggle.addEventListener('click', function (e) {
            e.preventDefault();
            const parentDropdown = this.closest('.dashboard-nav-dropdown');
            parentDropdown.classList.toggle('open');
        });
    });

    // Auto open dropdown if current URL matches a submenu item
    const currentPath = window.location.pathname;
    document.querySelectorAll('.dashboard-nav-dropdown-menu .dashboard-nav-dropdown-item').forEach(item => {
        if (item.getAttribute('href') === currentPath) {
            const dropdown = item.closest('.dashboard-nav-dropdown');
            dropdown.classList.add('open');
        }
    });
});
