document.addEventListener('DOMContentLoaded', function () {
    const navItems = document.querySelectorAll('.nav-item.expandable');
    navItems.forEach(item => {
        const navContent = item.querySelector('.nav-content');

        navContent.addEventListener('click', function () {
            const isActive = item.classList.contains('active');
            navItems.forEach(nav => nav.classList.remove('active'));
            if (isActive) {
                item.classList.remove('active');
            } else {
                item.classList.add('active');
            }
        });
    });

    // Toggle sidebar with menu button
    const menuToggle = document.querySelector('.menu-toggle');
    const sidebar = document.querySelector('.sidebar');

    menuToggle.addEventListener('click', function () {
        sidebar.classList.toggle('hidden');
    });
    const subItems = document.querySelectorAll('.sub-item');

    subItems.forEach(subItem => {
        subItem.addEventListener('click', function () {
            console.log('Navigating to:', this.textContent);
            subItems.forEach(item => item.classList.remove('sub-active'));
            this.classList.add('sub-active');
        });
    });
});