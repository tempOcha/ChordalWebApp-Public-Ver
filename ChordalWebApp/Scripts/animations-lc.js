// ==========================================
// CHORDAL LEARNING CENTRE ANIMATIONS
// Using p5.js and anime.js
// ==========================================

// ==========================================
// WAVE BACKGROUND FOR HERO
// ==========================================
let lcWaveSketch = function (p) {
    let waves = [];
    let numWaves = 2;

    p.setup = function () {
        let canvas = p.createCanvas(p.windowWidth, p.windowHeight);
        canvas.parent('lc-wave-canvas');

        // Create multiple wave layers
        for (let i = 0; i < numWaves; i++) {
            waves.push(new Wave(p, i));
        }
    };

    p.draw = function () {
        p.clear();

        // Draw all waves
        for (let wave of waves) {
            wave.update();
            wave.display();
        }
    };

    p.windowResized = function () {
        p.resizeCanvas(p.windowWidth, p.windowHeight);
    };

    class Wave {
        constructor(p, index) {
            this.p = p;
            this.yOffset = (index * 20) - 80;
            this.amplitude = 100 + (index * 10);
            this.frequency = 0.002 + (index * 0.0015);
            this.speed = 0.02 + (index * 0.01);
            this.time = 0;
            this.alpha = 200 - (index * 10);
        }

        update() {
            this.time += this.speed;
        }

        display() {
            this.p.noFill();
            this.p.stroke(23, 130, 100, this.alpha);
            this.p.strokeWeight(2);
            this.p.beginShape();

            for (let x = 0; x <= this.p.width; x += 5) {
                let y = this.p.height / 2 + this.yOffset +
                    this.p.sin(x * this.frequency + this.time) * this.amplitude;
                this.p.vertex(x, y);
            }
            this.p.endShape();
        }
    }
};

// ==========================================
// INITIALIZE WAVE SKETCH
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        if (document.getElementById('lc-wave-canvas')) {
            new p5(lcWaveSketch);
        }
    }, 100);
});

// ==========================================
// ANIME.JS CARD ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    // Animate lesson cards on scroll
    const lessonCards = document.querySelectorAll('.lc-lesson-card');
    const categoryCards = document.querySelectorAll('.lc-category-section');

    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting && typeof anime !== 'undefined') {
                anime({
                    targets: entry.target,
                    opacity: [0, 1],
                    translateY: [30, 0],
                    duration: 600,
                    easing: 'easeOutQuad',
                    delay: parseInt(entry.target.dataset.index || 0) * 50
                });
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Observe lesson cards
    lessonCards.forEach((card, index) => {
        card.style.opacity = '0';
        card.dataset.index = index;
        observer.observe(card);
    });

    // Observe category cards
    categoryCards.forEach((card, index) => {
        card.style.opacity = '0';
        card.dataset.index = index;
        observer.observe(card);
    });

    // Animate stat cards
    const statCards = document.querySelectorAll('.lc-stat-card');
    statCards.forEach((card, index) => {
        card.style.opacity = '0';
        setTimeout(() => {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: card,
                    opacity: [0, 1],
                    scale: [0.9, 1],
                    duration: 500,
                    easing: 'easeOutElastic(1, .6)',
                    delay: index * 100
                });
            }
        }, 200);
    });

    // Animate progress bars
    const progressBars = document.querySelectorAll('.lc-progress-bar-fill');
    const progressObserver = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const targetWidth = entry.target.style.width || '0%';
                entry.target.style.width = '0%';

                setTimeout(() => {
                    if (typeof anime !== 'undefined') {
                        anime({
                            targets: entry.target,
                            width: targetWidth,
                            duration: 1200,
                            easing: 'easeOutQuad'
                        });
                    }
                }, 300);

                progressObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.5 });

    progressBars.forEach(bar => {
        progressObserver.observe(bar);
    });

    // Status icon pulse animation
    const statusIcons = document.querySelectorAll('.lc-lesson-status');
    statusIcons.forEach(icon => {
        icon.addEventListener('mouseenter', function () {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: [1, 1.2, 1],
                    duration: 400,
                    easing: 'easeInOutQuad'
                });
            }
        });
    });

    // Hero content fade-in
    const heroContent = document.querySelector('.lc-hero-content');
    if (heroContent && typeof anime !== 'undefined') {
        anime({
            targets: heroContent,
            opacity: [0, 1],
            translateY: [-20, 0],
            duration: 800,
            easing: 'easeOutQuad',
            delay: 200
        });
    }
});