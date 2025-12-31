// ==========================================
// CHORDAL MY PROGRESSIONS ANIMATIONS
// Using p5.js for wave background
// ==========================================

// ==========================================
// MY PROGRESSIONS HEADER WAVE ANIMATION
// ==========================================
let myprogWaveSketch = function (p) {
    let waves = [];
    let numWaves = 3;

    p.setup = function () {
        let container = document.getElementById('myprog-wave-canvas');
        if (container) {
            // Get parent section height
            let headerSection = container.closest('.myprog-header-section');
            let canvasHeight = headerSection ? headerSection.offsetHeight : 300;

            let canvas = p.createCanvas(p.windowWidth, canvasHeight);
            canvas.id('myprog-wave-canvas');
            canvas.parent(container.parentElement);

            // Create waves
            for (let i = 0; i < numWaves; i++) {
                waves.push(new Wave(p, i));
            }
        }
    };

    p.draw = function () {
        p.clear();

        // Draw waves
        for (let wave of waves) {
            wave.update();
            wave.display();
        }
    };

    p.windowResized = function () {
        let container = document.getElementById('myprog-wave-canvas');
        if (container) {
            let headerSection = container.closest('.myprog-header-section');
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

// Initialize wave animation if canvas exists
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(() => {
        if (document.getElementById('myprog-wave-canvas')) {
            new p5(myprogWaveSketch);
        }
    }, 100);
});

// ==========================================
// CARD ANIMATIONS ON SCROLL
// ==========================================
function initCardAnimations() {
    const cards = document.querySelectorAll('.progression-card');

    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                setTimeout(() => {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                }, index * 50);
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    cards.forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.5s ease, transform 0.5s ease';
        observer.observe(card);
    });
}

// Initialize animations when DOM is loaded
document.addEventListener('DOMContentLoaded', function () {
    initCardAnimations();
});

// ==========================================
// FILTER CHIP ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const filterChips = document.querySelectorAll('.filter-chip');

    filterChips.forEach(chip => {
        chip.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.05)';
        });

        chip.addEventListener('mouseleave', function () {
            this.style.transform = 'scale(1)';
        });
    });
});

// ==========================================
// SEARCH INPUT ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.querySelector('.search-bar input[type="text"]');

    if (searchInput) {
        searchInput.addEventListener('focus', function () {
            this.style.transform = 'scale(1.01)';
        });

        searchInput.addEventListener('blur', function () {
            this.style.transform = 'scale(1)';
        });
    }
});

// ==========================================
// CATEGORY COLOR APPLICATION
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const coloredHeaders = document.querySelectorAll('.card-header-colored[data-category-color]');

    coloredHeaders.forEach(header => {
        const color = header.getAttribute('data-category-color');
        if (color) {
            header.style.backgroundColor = color;
        }
    });
});

// ==========================================
// SMOOTH SCROLL TO RESULTS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const searchButton = document.querySelector('.search-bar .btn');

    if (searchButton) {
        searchButton.addEventListener('click', function () {
            setTimeout(() => {
                const resultsGrid = document.querySelector('.progression-grid');
                if (resultsGrid) {
                    resultsGrid.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }, 100);
        });
    }
});

// ==========================================
// CATEGORY MODAL ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const modal = document.querySelector('.category-modal-overlay');

    if (modal) {
        // Add animation classes
        modal.classList.add('fade-in');

        // Close on overlay click
        modal.addEventListener('click', function (e) {
            if (e.target === this) {
                const cancelButton = modal.querySelector('.btn-modal-secondary');
                if (cancelButton) {
                    cancelButton.click();
                }
            }
        });
    }
});
