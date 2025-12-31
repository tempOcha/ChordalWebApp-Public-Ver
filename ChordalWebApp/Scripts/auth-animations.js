// ==========================================
// CHORDAL AUTHENTICATION ANIMATIONS
// Wave Border & Success Transition
// ==========================================

// ==========================================
// 1. AUTH WAVE BORDER ANIMATION
// ==========================================
let authWaveSketch = function (p) {
    let waves = [];
    let numWaves = 4;
    let isSuccess = false;
    let fillProgress = 0;

    p.setup = function () {
        let container = document.getElementById('auth-wave-canvas');
        if (container) {
            let canvas = p.createCanvas(container.offsetWidth, container.offsetHeight);
            canvas.parent('auth-wave-canvas');

            // Create waves
            for (let i = 0; i < numWaves; i++) {
                waves.push(new Wave(p, i));
            }
        }
    };

    p.draw = function () {
        p.clear();

        if (isSuccess) {
            // Fill screen animation
            drawSuccessFill();
        } else {
            // Normal wave animation
            for (let wave of waves) {
                wave.update();
                wave.display();
            }
        }
    };

    function drawSuccessFill() {
        // Animated wave fill effect
        fillProgress += 0.02;

        let fillHeight = p.map(fillProgress, 0, 1, p.height, 0);

        // Check if this is admin page (red waves)
        let isAdminPage = document.querySelector('.auth-visual-panel.admin') !== null;

        // Colors: Teal for normal, Red/Pink for admin
        let color1 = isAdminPage ? [220, 53, 69] : [23, 115, 100]; // Red or Teal
        let color2 = isAdminPage ? [251, 68, 102] : [23, 115, 100]; // Pink or Teal

        // Draw multiple waves for smooth effect
        p.fill(color1[0], color1[1], color1[2], 200);
        p.noStroke();

        p.beginShape();
        p.vertex(0, p.height);

        for (let x = 0; x <= p.width; x += 10) {
            let waveOffset = p.sin(x * 0.01 + fillProgress * 5) * 20;
            let y = fillHeight + waveOffset;
            p.vertex(x, y);
        }

        p.vertex(p.width, p.height);
        p.endShape(p.CLOSE);

        // Additional wave layers for depth (slightly pink-tinted for admin)
        p.fill(color2[0], color2[1], color2[2], 150);
        p.beginShape();
        p.vertex(0, p.height);

        for (let x = 0; x <= p.width; x += 10) {
            let waveOffset = p.sin(x * 0.008 + fillProgress * 6) * 25;
            let y = fillHeight + waveOffset + 20;
            p.vertex(x, y);
        }

        p.vertex(p.width, p.height);
        p.endShape(p.CLOSE);

        if (fillProgress >= 1.2) {
            // Transition complete - fade to white or redirect
            document.body.style.transition = 'opacity 0.5s';
            document.body.style.opacity = '0';
        }
    }

    // Function to trigger success animation
    window.triggerAuthSuccess = function (redirectUrl) {
        isSuccess = true;
        fillProgress = 0;

        // Redirect after animation
        setTimeout(function () {
            if (redirectUrl) {
                window.location.href = redirectUrl;
            }
        }, 2000);
    };

    p.windowResized = function () {
        let container = document.getElementById('auth-wave-canvas');
        if (container) {
            p.resizeCanvas(container.offsetWidth, container.offsetHeight);
        }
    };

    class Wave {
        constructor(p, index) {
            this.p = p;
            this.index = index;
            this.yOffset = index * 60;
            this.amplitude = 40 + index * 15;
            this.frequency = 0.008 - index * 0.001;
            this.speed = 0.025 + index * 0.008;
            this.time = index * 100; // Offset start time
            this.alpha = 180 - index * 35;
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

// ==========================================
// 2. SUCCESS WAVE FULL SCREEN
// ==========================================
let successWaveSketch = function (p) {
    let fillProgress = 0;
    let isActive = false;

    p.setup = function () {
        let canvas = p.createCanvas(p.windowWidth, p.windowHeight);
        canvas.parent('auth-success-wave');
    };

    p.draw = function () {
        if (!isActive) {
            return;
        }

        p.clear();

        fillProgress += 0.015;

        let fillHeight = p.map(fillProgress, 0, 1, p.height, 0);

        // Check if this is admin page (red waves)
        let isAdminPage = document.querySelector('.auth-visual-panel.admin') !== null;

        // Colors: Teal for normal, Red/Pink for admin
        let baseColor = isAdminPage ? [220, 53, 69] : [23, 115, 100]; // Red or Teal
        let accentColor = isAdminPage ? [251, 68, 102] : [31, 142, 111]; // Pink or Medium Teal

        // Multiple wave layers
        for (let i = 0; i < 3; i++) {
            // Interpolate between base and accent color
            let r = baseColor[0] + (accentColor[0] - baseColor[0]) * (i / 3);
            let g = baseColor[1] + (accentColor[1] - baseColor[1]) * (i / 3);
            let b = baseColor[2] + (accentColor[2] - baseColor[2]) * (i / 3);
            let alpha = 180 - i * 40;

            p.fill(r, g, b, alpha);
            p.noStroke();

            p.beginShape();
            p.vertex(0, p.height);

            for (let x = 0; x <= p.width; x += 10) {
                let waveOffset = p.sin(x * 0.008 + fillProgress * (5 + i)) * (25 + i * 5);
                let y = fillHeight + waveOffset + (i * 20);
                p.vertex(x, y);
            }

            p.vertex(p.width, p.height);
            p.endShape(p.CLOSE);
        }

        if (fillProgress >= 1.1) {
            isActive = false;
        }
    };

    window.triggerSuccessWave = function (redirectUrl) {
        isActive = true;
        fillProgress = 0;
        document.getElementById('auth-success-wave').classList.add('active');

        setTimeout(function () {
            document.body.style.transition = 'opacity 0.5s';
            document.body.style.opacity = '0';

            setTimeout(function () {
                if (redirectUrl) {
                    window.location.href = redirectUrl;
                }
            }, 500);
        }, 1800);
    };

    p.windowResized = function () {
        p.resizeCanvas(p.windowWidth, p.windowHeight);
    };
};

// ==========================================
// 3. FORM INTERACTIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    // Initialize wave sketches
    setTimeout(function () {
        if (document.getElementById('auth-wave-canvas')) {
            new p5(authWaveSketch);
        }

        if (document.getElementById('auth-success-wave')) {
            new p5(successWaveSketch);
        }
    }, 100);

    // Password toggle functionality
    const passwordToggles = document.querySelectorAll('.password-toggle');
    passwordToggles.forEach(toggle => {
        toggle.addEventListener('click', function () {
            const input = this.previousElementSibling;
            if (input && input.type === 'password') {
                input.type = 'text';
                this.textContent = '👁️';
            } else if (input) {
                input.type = 'password';
                this.textContent = '👁️';
            }
        });
    });

    // Input focus animations
    const inputs = document.querySelectorAll('.auth-form-input');
    inputs.forEach(input => {
        input.addEventListener('focus', function () {
            this.parentElement.style.transform = 'translateY(-2px)';
            this.parentElement.style.transition = 'transform 0.2s ease';
        });

        input.addEventListener('blur', function () {
            this.parentElement.style.transform = 'translateY(0)';
        });
    });

    // Button hover effect with anime.js
    const authButtons = document.querySelectorAll('.auth-submit-btn');
    authButtons.forEach(button => {
        button.addEventListener('mouseenter', function () {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: 1.02,
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

    // Form entrance animation
    const authFormPanel = document.querySelector('.auth-form-panel');
    if (authFormPanel && typeof anime !== 'undefined') {
        anime({
            targets: '.auth-form-panel',
            translateX: [50, 0],
            opacity: [0, 1],
            duration: 600,
            easing: 'easeOutQuad',
            delay: 200
        });

        anime({
            targets: '.auth-visual-panel',
            translateX: [-50, 0],
            opacity: [0, 1],
            duration: 600,
            easing: 'easeOutQuad',
            delay: 100
        });
    }

    // Form group stagger animation
    const formGroups = document.querySelectorAll('.auth-form-group');
    if (formGroups.length > 0 && typeof anime !== 'undefined') {
        anime({
            targets: formGroups,
            translateY: [20, 0],
            opacity: [0, 1],
            duration: 500,
            delay: anime.stagger(100, { start: 400 }),
            easing: 'easeOutQuad'
        });
    }
});

// ==========================================
// 4. SUCCESS HANDLER FOR ASP.NET
// ==========================================
// This function can be called from ASP.NET code-behind
window.handleAuthSuccess = function (redirectUrl, delay) {
    delay = delay || 2000;

    // Trigger the wave animation
    if (typeof triggerSuccessWave !== 'undefined') {
        triggerSuccessWave(redirectUrl);
    } else if (typeof triggerAuthSuccess !== 'undefined') {
        triggerAuthSuccess(redirectUrl);
    } else {
        // Fallback if animations aren't loaded
        setTimeout(function () {
            window.location.href = redirectUrl;
        }, delay);
    }
};

// ==========================================
// 5. VALIDATION HELPERS
// ==========================================
function showValidationError(inputId, message) {
    const input = document.getElementById(inputId);
    if (input) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'auth-validation';
        errorDiv.textContent = message;

        // Remove existing error
        const existingError = input.parentElement.querySelector('.auth-validation');
        if (existingError) {
            existingError.remove();
        }

        input.parentElement.appendChild(errorDiv);
        input.style.borderColor = 'var(--color-accent-pink)';

        // Shake animation
        if (typeof anime !== 'undefined') {
            anime({
                targets: input,
                translateX: [
                    { value: -10, duration: 100 },
                    { value: 10, duration: 100 },
                    { value: -10, duration: 100 },
                    { value: 10, duration: 100 },
                    { value: 0, duration: 100 }
                ],
                easing: 'easeInOutQuad'
            });
        }
    }
}

function clearValidationError(inputId) {
    const input = document.getElementById(inputId);
    if (input) {
        const errorDiv = input.parentElement.querySelector('.auth-validation');
        if (errorDiv) {
            errorDiv.remove();
        }
        input.style.borderColor = '';
    }
}