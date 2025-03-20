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

    const ctx = document.getElementById('documentChart').getContext('2d');

    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: ['Software Engineering', 'HR Policies', 'SOP Production', 'Govt Forms', 'Resume', 'Terms & Conditions', 'International'],
            datasets: [{
                label: 'Document Categories',
                data: [2, 6, 12, 14, 16, 18, 32],
                backgroundColor: ['#FF6384', '#36A2EB', '#FFCE56', '#4CAF50', '#F44336', '#9C27B0', '#FF9800']
            }]
        }
    });

});
