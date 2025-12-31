// ==========================================
// CHORDAL LANDING PAGE ANIMATIONS
// Using p5.js and anime.js
// ==========================================

// ==========================================
// 1. HERO PARTICLE BACKGROUND
// ==========================================
let heroSketch = function (p) {
    let particles = [];
    let numParticles = 80;

    p.setup = function () {
        let canvas = p.createCanvas(p.windowWidth, p.windowHeight);
        canvas.parent('hero-canvas-container');

        // Create particles
        for (let i = 0; i < numParticles; i++) {
            particles.push(new Particle(p));
        }
    };

    p.draw = function () {
        p.clear();

        // Update and display particles
        for (let particle of particles) {
            particle.update();
            particle.display();
        }

        // Connect nearby particles with stronger teal lines
        for (let i = 0; i < particles.length; i++) {
            for (let j = i + 1; j < particles.length; j++) {
                let d = p.dist(particles[i].x, particles[i].y, particles[j].x, particles[j].y);
                if (d < 150) {
                    let alpha = p.map(d, 0, 255, 200, 0); // Increased from 30
                    p.stroke(0, 115, 0, alpha); // Teal color
                    p.strokeWeight(1.5); // Slightly thicker
                    p.line(particles[i].x, particles[i].y, particles[j].x, particles[j].y);
                }
            }
        }
    };

    p.windowResized = function () {
        p.resizeCanvas(p.windowWidth, p.windowHeight);
    };

    class Particle {
        constructor(p) {
            this.p = p;
            this.x = p.random(p.width);
            this.y = p.random(p.height);
            this.vx = p.random(-0.5, 0.5);
            this.vy = p.random(-0.5, 0.5);
            this.size = p.random(4, 8); // Increased from 2-6
            this.alpha = p.random(200, 255); // Increased from 100-200
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
            this.p.fill(23, 115, 100, this.alpha); // Teal particles
            this.p.circle(this.x, this.y, this.size);
        }
    }
};

// ==========================================
// 2. MIDI ROLL VISUALIZATION
// ==========================================
let midiSketch = function (p) {
    let notes = [];
    let currentTime = 0;
    let scrollSpeed = 2;
    let noteHeight = 8;

    p.setup = function () {
        let container = document.getElementById('midi-roll-container');
        let canvas = p.createCanvas(container.offsetWidth, 400);
        canvas.parent('midi-roll-container');

        // Create a sample chord progression (I-V-vi-IV in C major)
        createSampleProgression();
    };

    function createSampleProgression() {
        //let measures = 8;
        //let measureDuration = 6; // seconds

        // I chord (C major) - C E G
        addChord(0, [60, 64, 67], 3.5);

        // V chord (G major) - G B D
        addChord(4, [67, 71, 74], 3.5);

        // vi chord (A minor) - A C E
        addChord(8, [69, 72, 76], 3.5);

        // IV chord (F major) - F A C
        addChord(12, [65, 69, 72], 3.5);

        // Repeat progression
        addChord(16, [60, 64, 67], 3.5);
        addChord(20, [67, 71, 74], 3.5);
        addChord(24, [69, 72, 76], 3.5);
        addChord(28, [65, 69, 72], 3.5);
    }

    function addChord(startTime, noteValues, duration) {
        for (let noteValue of noteValues) {
            notes.push({
                note: noteValue,
                startTime: startTime,
                duration: duration,
                color: p.color(251, 68, 102)
            });
        }
    }

    p.draw = function () {
        p.background(245, 248, 250);

        // Update time
        currentTime += scrollSpeed * 0.016; // Assuming ~60fps

        // Draw grid lines (measures)
        p.stroke(220);
        p.strokeWeight(1);
        for (let i = 0; i < 32; i += 4) {
            let x = p.map(i - currentTime, 0, 32, 0, p.width);
            if (x >= 0 && x <= p.width) {
                p.line(x, 0, x, p.height);
            }
        }

        // Draw center playhead
        p.stroke(251, 68, 102);
        p.strokeWeight(2);
        let playheadX = p.width * 0.25;
        p.line(playheadX, 0, playheadX, p.height);

        // Draw notes
        p.noStroke();
        for (let note of notes) {
            let xStart = p.map(note.startTime - currentTime, 0, 32, 0, p.width);
            let xEnd = p.map(note.startTime + note.duration - currentTime, 0, 32, 0, p.width);
            let y = p.map(note.note, 55, 80, p.height - 40, 40);

            // Only draw if visible
            if (xEnd > 0 && xStart < p.width) {
                let noteWidth = xEnd - xStart;
                p.fill(note.color);
                p.rect(xStart, y, noteWidth, noteHeight, 4);
            }
        }

        // Loop the progression
        if (currentTime > 32) {
            currentTime = 0;
        }

        // Draw labels
        p.fill(100);
        p.noStroke();
        p.textAlign(p.CENTER, p.CENTER);
        p.textSize(14);
        p.text('I - V - vi - IV Progression', p.width / 2, 20);
    };

    p.windowResized = function () {
        let container = document.getElementById('midi-roll-container');
        if (container) {
            p.resizeCanvas(container.offsetWidth, 400);
        }
    };
};

// ==========================================
// 3. CHORD PROGRESSION ANIMATION
// ==========================================
let chordSketch = function (p) {
    let chords = ['I', 'V', 'vi', 'IV'];
    let currentChordIndex = 0;
    let transitionProgress = 0;
    let holdTime = 0;
    let maxHoldTime = 120; // frames

    p.setup = function () {
        let container = document.getElementById('chord-progression-container');
        if (container) {
            let canvas = p.createCanvas(container.offsetWidth, 300);
            canvas.parent('chord-progression-container');
        }
    };

    p.draw = function () {
        // Clean white background
        p.background(255);

        p.textAlign(p.CENTER, p.CENTER);
        p.textSize(120);
        p.textStyle(p.BOLD);

        // Draw current chord
        let currentChord = chords[currentChordIndex];
        let nextChord = chords[(currentChordIndex + 1) % chords.length];

        // Animate transition
        if (transitionProgress > 0) {
            let alpha = p.map(transitionProgress, 0, 1, 255, 0);
            p.fill(23, 115, 100, alpha); // Teal color
            p.text(currentChord, p.width / 2, p.height / 2 - transitionProgress * 50);

            alpha = p.map(transitionProgress, 0, 1, 0, 255);
            p.fill(23, 115, 100, alpha); // Teal color
            p.text(nextChord, p.width / 2, p.height / 2 + (1 - transitionProgress) * 50);

            transitionProgress += 0.02;

            if (transitionProgress >= 1) {
                currentChordIndex = (currentChordIndex + 1) % chords.length;
                transitionProgress = 0;
                holdTime = 0;
            }
        } else {
            // Hold current chord
            p.fill(23, 115, 100); // Teal color
            p.text(currentChord, p.width / 2, p.height / 2);

            holdTime++;
            if (holdTime >= maxHoldTime) {
                transitionProgress = 0.01;
            }
        }

        // Draw progression label
        p.textSize(16);
        p.fill(34, 31, 86, 180); // Dark blue for label
        p.text('Common Progression: I → V → vi → IV', p.width / 2, p.height - 30);
    };

    p.windowResized = function () {
        let container = document.getElementById('chord-progression-container');
        if (container) {
            p.resizeCanvas(container.offsetWidth, 300);
        }
    };
};

// ==========================================
// 4. WAVE ANIMATION (CTA SECTION)
// ==========================================
let waveSketch = function (p) {
    let waves = [];
    let numWaves = 3;

    p.setup = function () {
        let container = document.getElementById('wave-canvas-container');
        if (container) {
            let canvas = p.createCanvas(p.windowWidth, container.offsetHeight || 400);
            canvas.parent('wave-canvas-container');

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
        let container = document.getElementById('wave-canvas-container');
        if (container) {
            p.resizeCanvas(p.windowWidth, container.offsetHeight || 400);
        }
    };

    class Wave {
        constructor(p, index) {
            this.p = p;
            this.yOffset = index * 50;
            this.amplitude = 30 + index * 20;
            this.frequency = 0.01 - index * 0.002;
            this.speed = 0.02 + index * 0.005;
            this.time = 0;
            this.alpha = 100 - index * 30;
        }

        update() {
            this.time += this.speed;
        }

        display() {
            this.p.noFill();
            this.p.stroke(23, 115, 100, this.alpha); // Teal waves
            this.p.strokeWeight(2);

            this.p.beginShape();
            for (let x = 0; x <= this.p.width; x += 10) {
                let y = this.p.height / 2 + this.yOffset +
                    this.p.sin(x * this.frequency + this.time) * this.amplitude;
                this.p.vertex(x, y);
            }
            this.p.endShape();
        }
    }
};

// ==========================================
// INITIALIZE ALL SKETCHES
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    // Wait a bit to ensure containers are rendered
    setTimeout(function () {
        // Initialize hero particles
        if (document.getElementById('hero-canvas-container')) {
            new p5(heroSketch);
        }

        // Initialize MIDI roll
        if (document.getElementById('midi-roll-container')) {
            new p5(midiSketch);
        }

        // Initialize chord progression
        if (document.getElementById('chord-progression-container')) {
            new p5(chordSketch);
        }

        // Initialize waves
        if (document.getElementById('wave-canvas-container')) {
            new p5(waveSketch);
        }
    }, 100);
});

// ==========================================
// ANIME.JS SCROLL ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    // Animate feature cards on scroll
    const featureCards = document.querySelectorAll('.feature-card');
    const stepCards = document.querySelectorAll('.step-card');

    const observerOptions = {
        threshold: 0.2,
        rootMargin: '0px 0px -100px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                anime({
                    targets: entry.target,
                    translateY: [50, 0],
                    opacity: [0, 1],
                    duration: 800,
                    easing: 'easeOutQuad'
                });
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    featureCards.forEach(card => observer.observe(card));
    stepCards.forEach(card => observer.observe(card));

    // Button hover animations
    const buttons = document.querySelectorAll('.btn:not(.btn-pulse)');
    buttons.forEach(button => {
        button.addEventListener('mouseenter', function () {
            anime({
                targets: this,
                scale: 1.05,
                duration: 200,
                easing: 'easeOutQuad'
            });
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

    // Navbar animation on scroll
    let lastScroll = 0;
    const navbar = document.querySelector('.chordal-navbar');

    window.addEventListener('scroll', function () {
        const currentScroll = window.scrollY;

        if (currentScroll > 100) {
            navbar.style.boxShadow = '0 4px 20px rgba(0, 0, 0, 0.15)';
        } else {
            navbar.style.boxShadow = '0 4px 16px rgba(0, 0, 0, 0.08)';
        }

        lastScroll = currentScroll;
    });
});