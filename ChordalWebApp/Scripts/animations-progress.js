// ==========================================
// CHORDAL MY PROGRESS ANIMATIONS
// Train Journey Theme with p5.js and anime.js
// ==========================================

// ==========================================
// WAVE BACKGROUND FOR HERO
// ==========================================
let progressWaveSketch = function (p) {
    let waves = [];
    let numWaves = 3;

    p.setup = function () {
        let canvas = p.createCanvas(p.windowWidth, p.windowHeight);
        canvas.parent('progress-wave-canvas');

        for (let i = 0; i < numWaves; i++) {
            waves.push(new Wave(p, i));
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
            this.p.stroke(23, 115, 100, this.alpha);
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
        if (document.getElementById('progress-wave-canvas')) {
            new p5(progressWaveSketch);
        }
    }, 100);
});

// ==========================================
// ANIME.JS ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    // Hero content fade-in
    const heroContent = document.querySelector('.progress-hero-content');
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

    // Stat cards entrance animation
    const statCards = document.querySelectorAll('.progress-stat-card');
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
        }, 300);
    });

    // Train station animations on scroll
    const trainStations = document.querySelectorAll('.train-station');

    const observerOptions = {
        threshold: 0.2,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting && typeof anime !== 'undefined') {
                const isEven = Array.from(trainStations).indexOf(entry.target) % 2 === 0;

                // Animate the station card
                const card = entry.target.querySelector('.station-card');
                const marker = entry.target.querySelector('.station-dot');

                anime({
                    targets: card,
                    opacity: [0, 1],
                    translateX: isEven ? [50, 0] : [-50, 0],
                    duration: 700,
                    easing: 'easeOutQuad'
                });

                // Animate the station marker
                anime({
                    targets: marker,
                    scale: [0, 1],
                    duration: 500,
                    easing: 'easeOutElastic(1, .6)',
                    delay: 200
                });

                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    trainStations.forEach(station => {
        const card = station.querySelector('.station-card');
        if (card) {
            card.style.opacity = '0';
        }
        observer.observe(station);
    });

    // Animate progress bars
    const progressBars = document.querySelectorAll('.station-progress-fill');
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

    // Track line drawing animation
    const trackLine = document.querySelector('.track-line');
    if (trackLine && typeof anime !== 'undefined') {
        trackLine.style.height = '0';

        setTimeout(() => {
            anime({
                targets: trackLine,
                height: '100%',
                duration: 1500,
                easing: 'easeInOutQuad'
            });
        }, 500);
    }

    // Category card animations
    const categoryCards = document.querySelectorAll('.category-summary-card');
    const categoryObserver = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting && typeof anime !== 'undefined') {
                anime({
                    targets: entry.target,
                    opacity: [0, 1],
                    translateY: [30, 0],
                    duration: 600,
                    easing: 'easeOutQuad',
                    delay: parseInt(entry.target.dataset.index || 0) * 80
                });
                categoryObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.2 });

    categoryCards.forEach((card, index) => {
        card.style.opacity = '0';
        card.dataset.index = index;
        categoryObserver.observe(card);
    });

    // Achievement badges animation
    const achievementBadges = document.querySelectorAll('.achievement-badge');
    achievementBadges.forEach((badge, index) => {
        badge.style.opacity = '0';
        setTimeout(() => {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: badge,
                    opacity: [0, 1],
                    scale: [0.8, 1],
                    duration: 400,
                    easing: 'easeOutElastic(1, .6)',
                    delay: index * 80
                });
            }
        }, 300);
    });

    // Recommended lesson cards animation
    const recommendedCards = document.querySelectorAll('.recommended-lesson-card');
    const recommendedObserver = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting && typeof anime !== 'undefined') {
                anime({
                    targets: entry.target,
                    opacity: [0, 1],
                    translateY: [30, 0],
                    duration: 600,
                    easing: 'easeOutQuad',
                    delay: parseInt(entry.target.dataset.index || 0) * 100
                });
                recommendedObserver.unobserve(entry.target);
            }
        });
    }, { threshold: 0.2 });

    recommendedCards.forEach((card, index) => {
        card.style.opacity = '0';
        card.dataset.index = index;
        recommendedObserver.observe(card);
    });

    // Station marker hover effects
    const stationDots = document.querySelectorAll('.station-dot');
    stationDots.forEach(dot => {
        dot.addEventListener('mouseenter', function () {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: [1, 1.15, 1],
                    duration: 400,
                    easing: 'easeInOutQuad'
                });
            }
        });
    });

    // Station card click animation
    const stationCards = document.querySelectorAll('.station-card');
    stationCards.forEach(card => {
        card.addEventListener('click', function () {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: [1, 0.98, 1],
                    duration: 300,
                    easing: 'easeInOutQuad'
                });
            }
        });
    });

    // Number counter animation for stats
    const statNumbers = document.querySelectorAll('.progress-stat-number');
    statNumbers.forEach(statNumber => {
        const targetValue = parseInt(statNumber.textContent);
        if (!isNaN(targetValue)) {
            const obj = { value: 0 };

            if (typeof anime !== 'undefined') {
                anime({
                    targets: obj,
                    value: targetValue,
                    duration: 2000,
                    easing: 'easeOutQuad',
                    round: 1,
                    delay: 500,
                    update: function () {
                        statNumber.textContent = obj.value;
                    }
                });
            }
        }
    });
});
