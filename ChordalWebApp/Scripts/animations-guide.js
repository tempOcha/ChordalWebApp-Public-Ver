// ==========================================
// GUIDELINES PAGE PARTICLE ANIMATION
// Using p5.js
// ==========================================

let guidelinesParticleSketch = function (p) {
    let particles = [];
    let numParticles = 60;

    p.setup = function () {
        let container = document.getElementById('guidelines-particles-canvas');
        if (container) {
            let canvas = p.createCanvas(container.offsetWidth, container.offsetHeight);
            canvas.parent('guidelines-particles-canvas');

            // Create particles
            for (let i = 0; i < numParticles; i++) {
                particles.push(new Particle(p));
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

        // Connect nearby particles with lines
        for (let i = 0; i < particles.length; i++) {
            for (let j = i + 1; j < particles.length; j++) {
                let d = p.dist(particles[i].x, particles[i].y, particles[j].x, particles[j].y);
                if (d < 150) {
                    let alpha = p.map(d, 0, 150, 120, 0);
                    p.stroke(255, 255, 255, alpha); // White lines for teal background
                    p.strokeWeight(1.5);
                    p.line(particles[i].x, particles[i].y, particles[j].x, particles[j].y);
                }
            }
        }
    };

    p.windowResized = function () {
        let container = document.getElementById('guidelines-particles-canvas');
        if (container) {
            p.resizeCanvas(container.offsetWidth, container.offsetHeight);
        }
    };

    class Particle {
        constructor(p) {
            this.p = p;
            this.x = p.random(p.width);
            this.y = p.random(p.height);
            this.vx = p.random(-0.5, 0.5);
            this.vy = p.random(-0.5, 0.5);
            this.size = p.random(4, 8);
            this.alpha = p.random(180, 255);
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
            this.p.fill(255, 255, 255, this.alpha); // White particles
            this.p.circle(this.x, this.y, this.size);
        }
    }
};

// ==========================================
// INITIALIZE SKETCH
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        if (document.getElementById('guidelines-particles-canvas')) {
            new p5(guidelinesParticleSketch);
        }
    }, 100);
});

// ==========================================
// SCROLL ANIMATIONS WITH ANIME.JS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const guidelineSections = document.querySelectorAll('.guideline-section');

    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach((entry, index) => {
            if (entry.isIntersecting) {
                anime({
                    targets: entry.target,
                    translateY: [30, 0],
                    opacity: [0, 1],
                    duration: 600,
                    delay: index * 100,
                    easing: 'easeOutQuad'
                });
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    guidelineSections.forEach(section => observer.observe(section));
});