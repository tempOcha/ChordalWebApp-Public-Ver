// ==========================================
// CHORDAL ADMIN ANIMATIONS
// Using p5.js and anime.js
// ==========================================

// ==========================================
// 1. ADMIN HEADER PARTICLES
// ==========================================
let adminParticlesSketch = function (p) {
    let particles = [];
    let numParticles = 60;

    p.setup = function () {
        let container = document.getElementById('admin-particles-canvas');
        if (container) {
            let canvas = p.createCanvas(container.offsetWidth, container.offsetHeight);
            canvas.parent('admin-particles-canvas');

            // Create particles with pink/red tint
            for (let i = 0; i < numParticles; i++) {
                particles.push(new AdminParticle(p));
            }
        }
    };

    p.draw = function () {
        p.clear();

        // Update and display particles
        for (let particle of particles) {
            particle.update();
            particle.display();
        }

        // Connect nearby particles with pink-red gradient lines
        for (let i = 0; i < particles.length; i++) {
            for (let j = i + 1; j < particles.length; j++) {
                let d = p.dist(particles[i].x, particles[i].y, particles[j].x, particles[j].y);
                if (d < 140) {
                    let alpha = p.map(d, 0, 140, 70, 0);
                    // Pink-red with teal hint
                    p.stroke(251, 68, 102, alpha);
                    p.strokeWeight(1.5);
                    p.line(particles[i].x, particles[i].y, particles[j].x, particles[j].y);
                }
            }
        }
    };

    p.windowResized = function () {
        let container = document.getElementById('admin-particles-canvas');
        if (container) {
            p.resizeCanvas(container.offsetWidth, container.offsetHeight);
        }
    };

    class AdminParticle {
        constructor(p) {
            this.p = p;
            this.x = p.random(p.width);
            this.y = p.random(p.height);
            this.vx = p.random(-0.4, 0.4);
            this.vy = p.random(-0.4, 0.4);
            this.size = p.random(4, 9);
            this.alpha = p.random(140, 210);
            // Mix of pink, red, and occasional teal
            this.colorChoice = p.random(1);
            if (this.colorChoice < 0.7) {
                // Pink-red particles (70%)
                this.r = p.random(220, 251);
                this.g = p.random(53, 102);
                this.b = p.random(69, 102);
            } else {
                // Teal particles (30%)
                this.r = 23;
                this.g = 115;
                this.b = 100;
            }
        }

        update() {
            this.x += this.vx;
            this.y += this.vy;

            // Wrap around edges
            if (this.x < 0) this.x = this.p.width;
            if (this.x > this.p.width) this.x = 0;
            if (this.y < 0) this.y = this.p.height;
            if (this.y > this.p.height) this.y = 0;
        }

        display() {
            this.p.noStroke();
            this.p.fill(this.r, this.g, this.b, this.alpha);
            this.p.circle(this.x, this.y, this.size);
        }
    }
};

// ==========================================
// 2. STAT CARD CHART ANIMATIONS (Mini Charts)
// ==========================================
let statChartSketches = [];

function createStatChart(containerId, data, chartType = 'line') {
    let sketch = function (p) {
        let chartData = data || [65, 75, 60, 80, 70, 85, 90];
        let maxVal = Math.max(...chartData);

        p.setup = function () {
            let container = document.getElementById(containerId);
            if (container) {
                let canvas = p.createCanvas(container.offsetWidth, 60);
                canvas.parent(containerId);
            }
        };

        p.draw = function () {
            p.clear();

            if (chartType === 'line') {
                drawLineChart(p, chartData, maxVal);
            } else if (chartType === 'bar') {
                drawBarChart(p, chartData, maxVal);
            }
        };

        p.windowResized = function () {
            let container = document.getElementById(containerId);
            if (container) {
                p.resizeCanvas(container.offsetWidth, 60);
            }
        };
    };

    return new p5(sketch);
}

function drawLineChart(p, data, maxVal) {
    p.noFill();
    p.stroke(251, 68, 102, 180); // Pink-red
    p.strokeWeight(2.5);

    p.beginShape();
    for (let i = 0; i < data.length; i++) {
        let x = p.map(i, 0, data.length - 1, 10, p.width - 10);
        let y = p.map(data[i], 0, maxVal, p.height - 5, 5);
        p.vertex(x, y);
    }
    p.endShape();

    // Draw points
    for (let i = 0; i < data.length; i++) {
        let x = p.map(i, 0, data.length - 1, 10, p.width - 10);
        let y = p.map(data[i], 0, maxVal, p.height - 5, 5);
        p.fill(251, 68, 102, 220);
        p.noStroke();
        p.circle(x, y, 5);
    }
}

function drawBarChart(p, data, maxVal) {
    let barWidth = (p.width - 20) / data.length - 4;

    for (let i = 0; i < data.length; i++) {
        let x = 10 + i * (barWidth + 4);
        let barHeight = p.map(data[i], 0, maxVal, 0, p.height - 10);
        let y = p.height - barHeight - 5;

        // Gradient effect
        let alpha = p.map(i, 0, data.length - 1, 150, 220);
        p.fill(251, 68, 102, alpha);
        p.noStroke();
        p.rect(x, y, barWidth, barHeight, 2);
    }
}

// ==========================================
// 3. CARD ANIMATIONS ON SCROLL
// ==========================================
function initAdminCardAnimations() {
    const cards = document.querySelectorAll('.stat-card, .action-card, .admin-content-card, .data-item');

    const observerOptions = {
        threshold: 0.15,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                // Stagger animation
                setTimeout(() => {
                    if (typeof anime !== 'undefined') {
                        anime({
                            targets: entry.target,
                            translateY: [30, 0],
                            opacity: [0, 1],
                            duration: 600,
                            easing: 'easeOutQuad'
                        });
                    } else {
                        // Fallback CSS animation
                        entry.target.style.animation = 'fadeInUp 0.6s ease forwards';
                    }
                }, index * 50);
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    cards.forEach(card => observer.observe(card));
}

// ==========================================
// 4. BUTTON HOVER ANIMATIONS
// ==========================================
function initAdminButtonAnimations() {
    const buttons = document.querySelectorAll('.btn-admin, .action-card');

    buttons.forEach(button => {
        button.addEventListener('mouseenter', function () {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: 1.05,
                    duration: 200,
                    easing: 'easeOutQuad'
                });
            }
        });

        button.addEventListener('mouseleave', function () {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: 1,
                    duration: 200,
                    easing: 'easeOutQuad'
                });
            }
        });
    });

    // Special animation for stat cards
    const statCards = document.querySelectorAll('.stat-card');
    statCards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            const number = this.querySelector('.stat-number');
            if (number && typeof anime !== 'undefined') {
                anime({
                    targets: number,
                    scale: [1, 1.1, 1],
                    duration: 400,
                    easing: 'easeInOutQuad'
                });
            }
        });
    });
}

// ==========================================
// 5. MODAL ANIMATIONS
// ==========================================
function showAdminModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'flex';
        const content = modal.querySelector('.admin-modal-content');

        if (typeof anime !== 'undefined') {
            anime({
                targets: content,
                scale: [0.9, 1],
                opacity: [0, 1],
                duration: 300,
                easing: 'easeOutQuad'
            });
        }
    }
}

function closeAdminModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        const content = modal.querySelector('.admin-modal-content');

        if (typeof anime !== 'undefined') {
            anime({
                targets: content,
                scale: [1, 0.9],
                opacity: [1, 0],
                duration: 200,
                easing: 'easeInQuad',
                complete: function () {
                    modal.style.display = 'none';
                }
            });
        } else {
            modal.style.display = 'none';
        }
    }
}

// ==========================================
// 6. NUMBER COUNTER ANIMATION
// ==========================================
function animateStatNumbers() {
    const statNumbers = document.querySelectorAll('.stat-number');

    statNumbers.forEach(stat => {
        const target = parseInt(stat.textContent) || 0;

        if (typeof anime !== 'undefined') {
            const obj = { value: 0 };
            anime({
                targets: obj,
                value: target,
                duration: 1500,
                easing: 'easeOutQuad',
                round: 1,
                update: function () {
                    stat.textContent = obj.value;
                }
            });
        }
    });
}

// ==========================================
// 7. PROGRESS BAR ANIMATIONS
// ==========================================
function animateProgressBar(elementId, targetPercent) {
    const progressBar = document.getElementById(elementId);

    if (progressBar && typeof anime !== 'undefined') {
        anime({
            targets: progressBar,
            width: targetPercent + '%',
            duration: 1200,
            easing: 'easeOutQuad'
        });
    }
}

// ==========================================
// 8. DATA TABLE ROW ANIMATIONS
// ==========================================
function animateTableRows() {
    const rows = document.querySelectorAll('.data-item');

    rows.forEach((row, index) => {
        if (typeof anime !== 'undefined') {
            anime({
                targets: row,
                translateX: [-20, 0],
                opacity: [0, 1],
                duration: 400,
                delay: index * 50,
                easing: 'easeOutQuad'
            });
        }
    });
}

// ==========================================
// INITIALIZE ALL ADMIN ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    // Small delay to ensure elements are rendered
    setTimeout(function () {
        // Initialize admin header particles
        if (document.getElementById('admin-particles-canvas')) {
            new p5(adminParticlesSketch);
        }

        // Initialize card animations
        initAdminCardAnimations();

        // Initialize button animations
        initAdminButtonAnimations();

        // Animate stat numbers on page load
        animateStatNumbers();

        // Animate table rows if present
        if (document.querySelectorAll('.data-item').length > 0) {
            animateTableRows();
        }

        // Setup modal close on background click
        document.querySelectorAll('.admin-modal-overlay').forEach(modal => {
            modal.addEventListener('click', function (e) {
                if (e.target === this) {
                    closeAdminModal(this.id);
                }
            });
        });

        // Initialize any stat charts
        // Example: createStatChart('chart-container-1', [65, 75, 60, 80, 70, 85, 90], 'line');
    }, 100);
});

// ==========================================
// CSS FALLBACK ANIMATIONS
// ==========================================
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeInUp {
        from {
            opacity: 0;
            transform: translateY(30px);
        }
        to {
            opacity: 1;
            transform: translateY(0);
        }
    }

    @keyframes pulse {
        0%, 100% {
            transform: scale(1);
        }
        50% {
            transform: scale(1.05);
        }
    }

    @keyframes shimmer {
        0% {
            background-position: -1000px 0;
        }
        100% {
            background-position: 1000px 0;
        }
    }
`;
document.head.appendChild(style);

// ==========================================
// UTILITY FUNCTIONS
// ==========================================

// Flash animation for success messages
function flashSuccess(elementId) {
    const element = document.getElementById(elementId);
    if (element && typeof anime !== 'undefined') {
        anime({
            targets: element,
            backgroundColor: ['#d4edda', '#ffffff', '#d4edda'],
            duration: 1000,
            easing: 'easeInOutQuad'
        });
    }
}

// Shake animation for errors
function shakeElement(elementId) {
    const element = document.getElementById(elementId);
    if (element && typeof anime !== 'undefined') {
        anime({
            targets: element,
            translateX: [
                { value: -10, duration: 100 },
                { value: 10, duration: 100 },
                { value: -10, duration: 100 },
                { value: 10, duration: 100 },
                { value: 0, duration: 100 }
            ],
            easing: 'easeInOutSine'
        });
    }
}

// Export functions for use in other scripts
if (typeof window !== 'undefined') {
    window.adminAnimations = {
        showModal: showAdminModal,
        closeModal: closeAdminModal,
        animateProgressBar: animateProgressBar,
        flashSuccess: flashSuccess,
        shakeElement: shakeElement,
        createStatChart: createStatChart
    };
}
