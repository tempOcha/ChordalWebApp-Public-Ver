// ==========================================
// PROGRESSION CATEGORIES ANIMATIONS
// Enhanced Color Picker with Anime.js
// ==========================================

// Store the select color function globally so ASP.NET can access it
window.selectColorGlobal = null;

document.addEventListener('DOMContentLoaded', function () {
    initializeColorPicker();
    initializeFormAnimations();
    initializeCategoryCardAnimations();
});

// ==========================================
// COLOR PICKER FUNCTIONALITY
// ==========================================
function initializeColorPicker() {
    const colorOptions = document.querySelectorAll('.color-option');
    const hiddenColorField = document.querySelector('[id*="hfSelectedColor"]');
    const colorPreviewSwatch = document.querySelector('.color-preview-swatch');
    const colorPreviewHex = document.querySelector('.color-preview-hex');
    const colorInputNative = document.querySelector('.color-input-native');
    const colorInputText = document.querySelector('.color-input-text');

    if (!hiddenColorField) {
        console.error('Hidden color field not found!');
        return;
    }

    console.log('Color picker initialized with field:', hiddenColorField.id);

    // Main selectColor function
    function selectColor(hexColor) {
        if (!hiddenColorField) return;

        console.log('Selecting color:', hexColor);

        // Ensure hex format
        if (!hexColor.startsWith('#')) {
            hexColor = '#' + hexColor;
        }

        // Update hidden field
        hiddenColorField.value = hexColor;
        console.log('Hidden field updated to:', hiddenColorField.value);

        // Update visual display
        updateColorDisplay(hexColor);

        // Update preset selections
        colorOptions.forEach(option => {
            option.classList.remove('selected');
            const optionColor = rgbToHex(option.style.backgroundColor);
            if (optionColor.toLowerCase() === hexColor.toLowerCase()) {
                option.classList.add('selected');
                console.log('Matched preset color:', optionColor);
            }
        });

        // Update native color picker
        if (colorInputNative) {
            colorInputNative.value = hexColor;
        }

        // Update text input
        if (colorInputText) {
            colorInputText.value = hexColor.toUpperCase();
        }

        // Animate preview swatch
        if (colorPreviewSwatch && typeof anime !== 'undefined') {
            anime({
                targets: colorPreviewSwatch,
                scale: [1.2, 1],
                duration: 400,
                easing: 'easeOutElastic(1, .5)'
            });
        }
    }

    // Make selectColor globally accessible
    window.selectColorGlobal = selectColor;

    function updateColorDisplay(hexColor) {
        if (colorPreviewSwatch) {
            colorPreviewSwatch.style.backgroundColor = hexColor;
        }
        if (colorPreviewHex) {
            colorPreviewHex.textContent = hexColor.toUpperCase();
        }
    }

    // Preset color selection with click handler
    colorOptions.forEach(option => {
        option.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const color = this.style.backgroundColor;
            const hexColor = rgbToHex(color);
            console.log('Color option clicked:', color, '->', hexColor);
            selectColor(hexColor);

            // Animate the selection
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: [1.2, 1.1],
                    duration: 300,
                    easing: 'easeOutElastic(1, .5)'
                });
            }
        });

        // Also add touch support for mobile
        option.addEventListener('touchend', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const color = this.style.backgroundColor;
            const hexColor = rgbToHex(color);
            selectColor(hexColor);
        });
    });

    // Native color picker
    if (colorInputNative) {
        colorInputNative.addEventListener('input', function (e) {
            selectColor(e.target.value);
        });
        colorInputNative.addEventListener('change', function (e) {
            selectColor(e.target.value);
        });
    }

    // Text input for hex color
    if (colorInputText) {
        colorInputText.addEventListener('input', function (e) {
            let value = e.target.value.trim();

            // Auto-add # if missing
            if (!value.startsWith('#') && value.length > 0) {
                value = '#' + value;
                e.target.value = value;
            }

            // Validate and update if valid hex
            if (/^#[0-9A-Fa-f]{6}$/.test(value)) {
                selectColor(value);
            }
        });

        colorInputText.addEventListener('blur', function (e) {
            // Revert to current color if invalid
            const currentColor = hiddenColorField ? hiddenColorField.value : '#177364';
            if (!/^#[0-9A-Fa-f]{6}$/.test(e.target.value)) {
                e.target.value = currentColor;
            }
        });
    }

    // Initialize with current color from hidden field
    setTimeout(function () {
        const currentColor = hiddenColorField.value || '#177364';
        console.log('Initializing with color:', currentColor);
        selectColor(currentColor);
    }, 100);
}

// ==========================================
// RGB TO HEX CONVERSION
// ==========================================
function rgbToHex(rgb) {
    // If already hex, return as is
    if (rgb.startsWith('#')) return rgb;

    const result = rgb.match(/\d+/g);
    if (!result || result.length < 3) return rgb;

    const r = parseInt(result[0]);
    const g = parseInt(result[1]);
    const b = parseInt(result[2]);

    return '#' + ((1 << 24) + (r << 16) + (g << 8) + b).toString(16).slice(1);
}

// ==========================================
// FORM ANIMATIONS
// ==========================================
function initializeFormAnimations() {
    const formInputs = document.querySelectorAll('.form-control');

    formInputs.forEach(input => {
        // Focus animation
        input.addEventListener('focus', function () {
            anime({
                targets: this,
                scale: [1, 1.02],
                duration: 200,
                easing: 'easeOutQuad'
            });
        });

        // Blur animation
        input.addEventListener('blur', function () {
            anime({
                targets: this,
                scale: [1.02, 1],
                duration: 200,
                easing: 'easeOutQuad'
            });
        });
    });

    // Button animations
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('mouseenter', function () {
            if (!this.disabled) {
                anime({
                    targets: this,
                    scale: 1.05,
                    duration: 200,
                    easing: 'easeOutQuad'
                });
            }
        });

        button.addEventListener('mouseleave', function () {
            anime({
                targets: this,
                scale: 1,
                duration: 200,
                easing: 'easeOutQuad'
            });
        });
    });
}

// ==========================================
// CATEGORY CARD ANIMATIONS
// ==========================================
function initializeCategoryCardAnimations() {
    const categoryCards = document.querySelectorAll('.category-card');

    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                anime({
                    targets: entry.target,
                    translateX: [-30, 0],
                    opacity: [0, 1],
                    duration: 600,
                    delay: index * 100,
                    easing: 'easeOutQuad'
                });
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    categoryCards.forEach(card => observer.observe(card));
}

// ==========================================
// RANDOM COLOR GENERATOR (OPTIONAL)
// ==========================================
function generateRandomColor() {
    const letters = '0123456789ABCDEF';
    let color = '#';
    for (let i = 0; i < 6; i++) {
        color += letters[Math.floor(Math.random() * 16)];
    }
    return color;
}

// ==========================================
// DELETE CONFIRMATION ANIMATION
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const deleteButtons = document.querySelectorAll('.btn-delete');

    deleteButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const card = this.closest('.category-card');
            if (card) {
                anime({
                    targets: card,
                    opacity: [1, 0.5],
                    scale: [1, 0.98],
                    duration: 200,
                    easing: 'easeOutQuad'
                });
            }
        });
    });
});

// ==========================================
// SMOOTH SCROLL TO FORM ON EDIT
// ==========================================
function scrollToForm() {
    const formSection = document.querySelector('.category-form-section');
    if (formSection) {
        formSection.scrollIntoView({ behavior: 'smooth', block: 'start' });

        // Highlight the form briefly
        anime({
            targets: formSection,
            backgroundColor: ['#f5faf8', '#ffffff'],
            duration: 1000,
            easing: 'easeInOutQuad'
        });
    }
}

// ==========================================
// STATUS MESSAGE ANIMATION
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const statusMessage = document.querySelector('.status-message');

    if (statusMessage) {
        anime({
            targets: statusMessage,
            translateY: [-20, 0],
            opacity: [0, 1],
            duration: 600,
            easing: 'easeOutQuad'
        });

        // Auto-hide after 5 seconds
        setTimeout(function () {
            anime({
                targets: statusMessage,
                opacity: [1, 0],
                translateY: [0, -20],
                duration: 400,
                easing: 'easeInQuad',
                complete: function () {
                    statusMessage.style.display = 'none';
                }
            });
        }, 5000);
    }
});
