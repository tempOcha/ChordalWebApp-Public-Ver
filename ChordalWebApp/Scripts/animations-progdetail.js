// ==========================================
// CHORDAL PROGRESSION DETAIL ANIMATIONS
// Using p5.js for wave background
// ==========================================

// ==========================================
// PROGRESSION DETAIL HEADER WAVE ANIMATION
// ==========================================
let progdetailWaveSketch = function (p) {
    let waves = [];
    let numWaves = 4;

    p.setup = function () {
        let container = document.getElementById('progdetail-wave-canvas');
        if (container) {
            let headerSection = container.closest('.progression-detail-header');
            let canvasHeight = headerSection ? headerSection.offsetHeight : 300;

            let canvas = p.createCanvas(p.windowWidth, canvasHeight);
            canvas.id('progdetail-wave-canvas');
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
        let container = document.getElementById('progdetail-wave-canvas');
        if (container) {
            let headerSection = container.closest('.progression-detail-header');
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
        if (document.getElementById('progdetail-wave-canvas')) {
            new p5(progdetailWaveSketch);
        }
    }, 100);
});

// ==========================================
// NOTES VISUALIZATION USING P5.JS
// Reusing the visualization logic from comm
// ==========================================
let notesSketch = function (p) {
    let chordData = [];
    let currentTime = 0;
    let totalDuration = 0;
    let barHeight = 40;
    let canvasWidth = 800;
    let canvasHeight = 300;

    p.setup = function () {
        let container = document.getElementById('notesCanvasContainer');
        if (container) {
            canvasWidth = container.offsetWidth - 40;
            let canvas = p.createCanvas(canvasWidth, canvasHeight);
            canvas.parent('notesCanvasContainer');

            // Load chord data from page
            loadChordData();
        }
    };

    p.draw = function () {
        p.background(255);

        if (chordData.length === 0) {
            drawEmptyState();
            return;
        }

        drawChordBars();
        drawTimeIndicator();
        drawLabels();
    };

    function loadChordData() {
        // This function would be populated by the server-side code
        // For now, it's a placeholder that can be filled via a global variable
        if (typeof window.progressionChordData !== 'undefined') {
            chordData = window.progressionChordData;
            totalDuration = chordData.reduce((sum, chord) => sum + chord.duration, 0);
        }
    }

    function drawChordBars() {
        let startX = 20;
        let startY = 50;
        let currentX = startX;

        chordData.forEach((chord, index) => {
            let barWidth = (chord.duration / totalDuration) * (canvasWidth - 40);

            // Alternate colors for visual separation
            let fillColor = index % 2 === 0 ?
                p.color(96, 175, 163, 100) : // Teal light
                p.color(23, 115, 100, 100);   // Teal

            p.fill(fillColor);
            p.stroke(23, 115, 100);
            p.strokeWeight(2);
            p.rect(currentX, startY, barWidth, barHeight);

            // Draw chord name
            p.fill(34, 31, 86);
            p.noStroke();
            p.textAlign(p.CENTER, p.CENTER);
            p.textSize(14);
            p.textStyle(p.BOLD);

            if (barWidth > 40) {
                p.text(chord.name, currentX + barWidth / 2, startY + barHeight / 2);
            }

            currentX += barWidth;
        });
    }

    function drawTimeIndicator() {
        if (currentTime > 0 && currentTime <= totalDuration) {
            let x = 20 + (currentTime / totalDuration) * (canvasWidth - 40);
            p.stroke(251, 68, 102);
            p.strokeWeight(2);
            p.line(x, 40, x, 100);
        }
    }

    function drawLabels() {
        p.fill(108, 117, 125);
        p.noStroke();
        p.textAlign(p.LEFT, p.TOP);
        p.textSize(12);
        p.text('0s', 20, 100);

        p.textAlign(p.RIGHT, p.TOP);
        p.text(totalDuration.toFixed(1) + 's', canvasWidth - 20, 100);

        p.textAlign(p.CENTER, p.TOP);
        p.textStyle(p.BOLD);
        p.text('Chord Progression Timeline', canvasWidth / 2, 15);
    }

    function drawEmptyState() {
        p.fill(108, 117, 125);
        p.noStroke();
        p.textAlign(p.CENTER, p.CENTER);
        p.textSize(16);
        p.text('No chord data available', canvasWidth / 2, canvasHeight / 2);
    }

    p.windowResized = function () {
        let container = document.getElementById('notesCanvasContainer');
        if (container) {
            canvasWidth = container.offsetWidth - 40;
            p.resizeCanvas(canvasWidth, canvasHeight);
        }
    };

    // Public method to update playback time
    p.updateTime = function (time) {
        currentTime = time;
    };
};

// Initialize notes visualization
let notesVisualization = null;
document.addEventListener('DOMContentLoaded', function () {
    if (document.getElementById('notesCanvasContainer')) {
        notesVisualization = new p5(notesSketch);
    }
});

// ==========================================
// CHORD CARD ANIMATIONS
// ==========================================
function initChordCardAnimations() {
    const cards = document.querySelectorAll('.chord-event-card');

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
                }, index * 30);
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    cards.forEach(card => {
        card.style.opacity = '0';
        card.style.transform = 'translateY(20px)';
        card.style.transition = 'opacity 0.4s ease, transform 0.4s ease';
        observer.observe(card);
    });
}

document.addEventListener('DOMContentLoaded', function () {
    initChordCardAnimations();
});

// ==========================================
// PLAYBACK SIMULATION
// (This would be replaced with actual audio playback)
// ==========================================
let isPlaying = false;
let playbackTimer = null;
let currentPlaybackTime = 0;

function initPlaybackControls() {
    const playButton = document.getElementById('btnPlayPause');
    const timeDisplay = document.getElementById('playbackTime');
    const statusDisplay = document.getElementById('playbackStatus');

    if (playButton) {
        playButton.addEventListener('click', function () {
            isPlaying = !isPlaying;

            if (isPlaying) {
                playButton.textContent = '⏸';
                startPlayback();
            } else {
                playButton.textContent = '▶';
                stopPlayback();
            }
        });
    }
}

function startPlayback() {
    const statusDisplay = document.getElementById('playbackStatus');
    if (statusDisplay) {
        statusDisplay.textContent = 'Playing...';
    }

    playbackTimer = setInterval(function () {
        currentPlaybackTime += 0.1;
        updatePlaybackDisplay();

        if (notesVisualization && notesVisualization.updateTime) {
            notesVisualization.updateTime(currentPlaybackTime);
        }
    }, 100);
}

function stopPlayback() {
    const statusDisplay = document.getElementById('playbackStatus');
    if (statusDisplay) {
        statusDisplay.textContent = 'Paused';
    }

    if (playbackTimer) {
        clearInterval(playbackTimer);
        playbackTimer = null;
    }
}

function updatePlaybackDisplay() {
    const timeDisplay = document.getElementById('playbackTime');
    if (timeDisplay) {
        const minutes = Math.floor(currentPlaybackTime / 60);
        const seconds = Math.floor(currentPlaybackTime % 60);
        timeDisplay.textContent = `${minutes}:${seconds.toString().padStart(2, '0')}`;
    }
}

document.addEventListener('DOMContentLoaded', function () {
    initPlaybackControls();
});

// ==========================================
// BUTTON HOVER EFFECTS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const actionButtons = document.querySelectorAll('.btn-action-primary, .btn-action-secondary');

    actionButtons.forEach(button => {
        button.addEventListener('mouseenter', function () {
            if (!this.disabled) {
                this.style.transform = 'translateY(-2px)';
            }
        });

        button.addEventListener('mouseleave', function () {
            if (!this.disabled) {
                this.style.transform = 'translateY(0)';
            }
        });
    });
});

// ==========================================
// SMOOTH SECTION ANIMATIONS
// ==========================================
function initSectionAnimations() {
    const sections = document.querySelectorAll('.midi-visualization-section, .chord-cards-section');

    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -100px 0px'
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

    sections.forEach(section => {
        section.style.opacity = '0';
        section.style.transform = 'translateY(30px)';
        section.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        observer.observe(section);
    });
}

document.addEventListener('DOMContentLoaded', function () {
    initSectionAnimations();
});
