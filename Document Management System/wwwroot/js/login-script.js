function togglePassword() {
    let passwordInput = document.getElementById("password");
    let eyeIcon = document.querySelector(".password-toggle");

    if (passwordInput.type === "password") {
        passwordInput.type = "text";
        eyeIcon.textContent = "👁"; // Open eye
    } else {
        passwordInput.type = "password";
        eyeIcon.textContent = "🔒"; // Closed eye
    }
}

// Gallery functionality
let currentSlide = 0;
const slides = document.querySelectorAll('.gallery-item');
const dots = document.querySelectorAll('.dot');
const gallery = document.querySelector('.gallery');
const slideText = document.getElementById('slide-text');

// Define text content for each slide
const slideTexts = [
    {
        title: "Mission:",
        description: "To nurture a vibrant culture of academic wellness responsive to the challenges of technology and the global community."
    },
    {
        title: "Vision:",
        description: "An eminent center of excellent higher education for societal advancement."
    },
    {
        title: "Philosophy:",
        description: "Social transformation for a caring community and an ecologically-balanced country."
    },
];

function showSlide(index) { // Show slide at given index
    if (index >= slides.length) {
        currentSlide = 0;
    } else if (index < 0) {
        currentSlide = slides.length - 1;
    } else {
        currentSlide = index;
    }

    // Update gallery position
    gallery.style.transform = `translateX(-${currentSlide * 100}%)`;

    // Fade out text, update content, fade in text
    slideText.style.opacity = 0;

    setTimeout(() => {
        // Update text content
        slideText.querySelector('h1').textContent = slideTexts[currentSlide].title;
        slideText.querySelector('p').textContent = slideTexts[currentSlide].description;

        // Fade text back in
        slideText.style.opacity = 1;
    }, 300);

    // Update dots
    dots.forEach((dot, i) => {
        dot.classList.toggle('active', i === currentSlide);
    });
}

// Auto-rotate gallery
let slideInterval = setInterval(() => {
    showSlide(currentSlide + 1);
}, 5000);

// Stop auto-rotation when user interacts with dots
dots.forEach(dot => {
    dot.addEventListener('click', () => {
        clearInterval(slideInterval);
        slideInterval = setInterval(() => {
            showSlide(currentSlide + 1);
        }, 5000);
    });
});

// Initialize gallery
document.addEventListener('DOMContentLoaded', () => {
    showSlide(0);
});