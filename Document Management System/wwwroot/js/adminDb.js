// JavaScript to handle the expandable navigation items
document.addEventListener('DOMContentLoaded', function() {
    // Get all expandable navigation items
    const navItems = document.querySelectorAll('.nav-item.expandable');

    // Add click event listener to each expandable item
    navItems.forEach(item => {
        const navContent = item.querySelector('.nav-content');

        navContent.addEventListener('click', function() {
            // Toggle the active class on the clicked item
            const isActive = item.classList.contains('active');

            // lagay // pag gusto na marami ma-open sa nav items
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

    menuToggle.addEventListener('click', function() {
        sidebar.style.display = sidebar.style.display === 'none' ? 'block' : 'none';
    });

    // Make sub-items clickable
    const subItems = document.querySelectorAll('.sub-item');

    subItems.forEach(subItem => {
        subItem.addEventListener('click', function() {
            // Here you would typically handle navigation to the specific section
            // For this example, we'll just log the clicked item
            console.log('Navigating to:', this.textContent);

            // Remove the active class from all sub-items
            subItems.forEach(item => item.classList.remove('sub-active'));

            // Add active class to the clicked sub-item
            this.classList.add('sub-active');
        });
    });
});
