// Sidebar Toggle
const menuToggle = document.querySelector('.menu-toggle');
const dashboardNav = document.querySelector('.dashboard-nav');

menuToggle.addEventListener('click', () => {
    dashboardNav.classList.toggle('hidden');
});

// Dropdown Menu Toggle
document.querySelector('.dashboard-nav-list').addEventListener('click', (event) => {
    const arrowIcon = event.target.closest('.arrow-icon');
    if (!arrowIcon) return;

    const toggle = arrowIcon.closest('.dashboard-nav-dropdown-toggle');
    const dropdownMenu = toggle.nextElementSibling;
    const isExpanded = toggle.getAttribute('aria-expanded') === 'true';

    toggle.setAttribute('aria-expanded', !isExpanded);
    dropdownMenu.style.display = isExpanded ? 'none' : 'flex';
    arrowIcon.style.transform = isExpanded ? 'rotate(0deg)' : 'rotate(180deg)';
});

// Accordion Menu Toggle
const folders = document.querySelectorAll('.folder'); 

for (const folder of folders) {
    folder.onclick = () => {
        folder.classList.toggle('active');
        const target_list = folder.nextElementSibling;
        target_list.style.maxHeight = target_list.style.maxHeight ? null : '100vh';
    }
}