// ==========================================
// CHORDAL SHARE PROGRESSION ANIMATIONS
// Using p5.js for wave background
// ==========================================

// ==========================================
// SHARE HEADER WAVE ANIMATION
// ==========================================
let shareWaveSketch = function (p) {
    let waves = [];
    let numWaves = 4;

    p.setup = function () {
        let container = document.getElementById('share-wave-canvas');
        if (container) {
            let headerSection = container.closest('.share-header-section');
            let canvasHeight = headerSection ? headerSection.offsetHeight : 300;

            let canvas = p.createCanvas(p.windowWidth, canvasHeight);
            canvas.id('share-wave-canvas');
            canvas.parent(container.parentElement);

            for (let i = 0; i < numWaves; i++) {
                waves.push(new Wave(p, i));
            }
        }
    };

    p.draw = function () {
        p.clear();

        for (let wave of waves) {
            wave.update();
            wave.display();
        }
    };

    p.windowResized = function () {
        let container = document.getElementById('share-wave-canvas');
        if (container) {
            let headerSection = container.closest('.share-header-section');
            let canvasHeight = headerSection ? headerSection.offsetHeight : 300;
            p.resizeCanvas(p.windowWidth, canvasHeight);
        }
    };

    class Wave {
        constructor(p, index) {
            this.p = p;
            this.yOffset = index * 40;
            this.amplitude = 35 + index * 15;
            this.frequency = 0.008 - index * 0.001;
            this.speed = 0.025 + index * 0.008;
            this.time = 0;
            this.alpha = 120 - index * 25;
        }

        update() {
            this.time += this.speed;
        }

        display() {
            this.p.noFill();
            this.p.stroke(255, 255, 255, this.alpha);
            this.p.strokeWeight(2.5);

            this.p.beginShape();
            for (let x = 0; x <= this.p.width; x += 8) {
                let y = this.p.height / 2 + this.yOffset +
                    this.p.sin(x * this.frequency + this.time) * this.amplitude;
                this.p.vertex(x, y);
            }
            this.p.endShape();
        }
    }
};

// Initialize wave animation
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(() => {
        if (document.getElementById('share-wave-canvas')) {
            new p5(shareWaveSketch);
        }
    }, 100);
});

// ==========================================
// CHARACTER COUNTER ANIMATION
// ==========================================
function updateCharCount(element, maxLength, counterId) {
    const currentLength = element.value.length;
    const counter = document.getElementById(counterId);

    if (counter) {
        counter.textContent = currentLength + '/' + maxLength;

        // Change color as approaching limit
        if (currentLength > maxLength * 0.9) {
            counter.style.color = '#fb4466'; // Pink accent for warning
        } else if (currentLength > maxLength * 0.75) {
            counter.style.color = '#f39c12'; // Orange for caution
        } else {
            counter.style.color = '#6c757d'; // Default muted
        }

        // Animate on update
        counter.style.transform = 'scale(1.1)';
        setTimeout(() => {
            counter.style.transform = 'scale(1)';
        }, 150);
    }
}

// ==========================================
// FORM INPUT ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const formInputs = document.querySelectorAll('.form-control');

    formInputs.forEach(input => {
        input.addEventListener('focus', function () {
            this.style.transform = 'scale(1.01)';
            this.parentElement.style.transition = 'all 0.2s ease';
        });

        input.addEventListener('blur', function () {
            this.style.transform = 'scale(1)';
        });

        // Add character count listeners
        if (input.hasAttribute('onkeyup')) {
            input.style.transition = 'all 0.2s ease';
        }
    });
});

// ==========================================
// PERMISSION OPTION ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const permissionOptions = document.querySelectorAll('.permission-option');

    permissionOptions.forEach(option => {
        option.addEventListener('click', function () {
            // Remove active state from all options
            permissionOptions.forEach(opt => {
                opt.style.borderColor = '#e1e8ed';
                opt.style.background = '#ffffff';
            });

            // Add active state to clicked option
            this.style.borderColor = '#177364';
            this.style.background = '#f5faf8';

            // Trigger the radio button
            const radio = this.querySelector('input[type="radio"]');
            if (radio) {
                radio.checked = true;
            }
        });

        // Set initial state for checked options
        const radio = option.querySelector('input[type="radio"]');
        if (radio && radio.checked) {
            option.style.borderColor = '#177364';
            option.style.background = '#f5faf8';
        }
    });
});

// ==========================================
// CHECKBOX OPTION ANIMATION
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const checkboxOptions = document.querySelectorAll('.checkbox-option');

    checkboxOptions.forEach(option => {
        const checkbox = option.querySelector('input[type="checkbox"]');

        if (checkbox) {
            checkbox.addEventListener('change', function () {
                if (this.checked) {
                    option.style.borderColor = '#60afa3';
                    option.style.background = '#f5faf8';
                } else {
                    option.style.borderColor = '#60afa3';
                    option.style.background = '#f5faf8';
                }

                // Animate
                option.style.transform = 'scale(1.02)';
                setTimeout(() => {
                    option.style.transform = 'scale(1)';
                }, 200);
            });
        }
    });
});

// ==========================================
// BUTTON HOVER ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const buttons = document.querySelectorAll('.btn-share-primary, .btn-share-secondary');

    buttons.forEach(button => {
        button.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-2px)';
        });

        button.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
});

// ==========================================
// PROGRESSIVE FORM REVEAL
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const formSections = document.querySelectorAll('.form-section, .progression-preview');

    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry) => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    formSections.forEach((section, index) => {
        section.style.opacity = '0';
        section.style.transform = 'translateY(20px)';
        section.style.transition = `opacity 0.5s ease ${index * 0.1}s, transform 0.5s ease ${index * 0.1}s`;
        observer.observe(section);
    });
});

// ==========================================
// TAG SUGGESTIONS (if implemented)
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const tagInput = document.querySelector('input[id*="txtTags"]');

    if (tagInput) {
        const commonTags = ['Jazz', 'Blues', 'Rock', 'Pop', 'Classical', 'Folk',
            'Ballad', 'Upbeat', 'Melancholic', 'Experimental'];

        tagInput.addEventListener('input', function () {
            // This is a placeholder for tag suggestions functionality
            // Could be expanded with actual autocomplete
        });
    }
});

// ==========================================
// FORM VALIDATION ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const form = document.querySelector('form');

    if (form) {
        form.addEventListener('submit', function (e) {
            const submitButton = form.querySelector('.btn-share-primary');
            if (submitButton) {
                submitButton.style.transform = 'scale(0.95)';
                setTimeout(() => {
                    submitButton.style.transform = 'scale(1)';
                }, 100);
            }
        });
    }
});

// ==========================================
// ALERT MESSAGE ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const alerts = document.querySelectorAll('.alert-modern');

    alerts.forEach(alert => {
        alert.style.opacity = '0';
        alert.style.transform = 'translateY(-10px)';
        alert.style.transition = 'opacity 0.3s ease, transform 0.3s ease';

        setTimeout(() => {
            alert.style.opacity = '1';
            alert.style.transform = 'translateY(0)';
        }, 100);
    });
});

// ==========================================
// SMOOTH SCROLL TO ERROR
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const errorMessages = document.querySelectorAll('.text-danger');

    if (errorMessages.length > 0) {
        errorMessages[0].scrollIntoView({
            behavior: 'smooth',
            block: 'center'
        });
    }
});
