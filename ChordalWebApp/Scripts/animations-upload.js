// ==========================================
// UPLOAD PROGRESSION WAVE ANIMATION
// Using p5.js - Similar to auth page transition
// ==========================================

let uploadWaveSketch = function (p) {
    let waves = [];
    let numWaves = 5;
    let animationActive = false;
    let animationDuration = 2000; // 2 seconds
    let animationStartTime = 0;

    p.setup = function () {
        let container = document.getElementById('upload-wave-canvas');
        if (container) {
            // Get the parent container's dimensions
            let parentContainer = container.parentElement;
            let canvas = p.createCanvas(parentContainer.offsetWidth, parentContainer.offsetHeight);
            canvas.parent(container); // Attach canvas to the canvas element itself

            // Create waves
            for (let i = 0; i < numWaves; i++) {
                waves.push(new Wave(p, i));
            }

            console.log('Wave canvas initialized:', parentContainer.offsetWidth, 'x', parentContainer.offsetHeight);
        }
    };

    p.draw = function () {
        if (!animationActive) return;

        p.clear();

        // Calculate animation progress (0 to 1)
        let elapsed = Date.now() - animationStartTime;
        let progress = Math.min(elapsed / animationDuration, 1);

        // Draw waves
        for (let wave of waves) {
            wave.update();
            wave.display(progress);
        }

        // End animation after duration
        if (progress >= 1) {
            animationActive = false;
            let canvasElement = document.getElementById('upload-wave-canvas');
            if (canvasElement) {
                canvasElement.classList.remove('active');
            }
        }
    };

    p.windowResized = function () {
        let container = document.getElementById('upload-wave-canvas');
        if (container && container.parentElement) {
            let parentContainer = container.parentElement;
            p.resizeCanvas(parentContainer.offsetWidth, parentContainer.offsetHeight);
        }
    };

    // Public method to trigger animation
    p.triggerAnimation = function () {
        console.log('Wave animation triggered!');
        animationActive = true;
        animationStartTime = Date.now();
        let canvasElement = document.getElementById('upload-wave-canvas');
        if (canvasElement) {
            canvasElement.classList.add('active');
            console.log('Canvas active class added');
        }

        // Reset waves
        waves = [];
        for (let i = 0; i < numWaves; i++) {
            waves.push(new Wave(p, i));
        }
    };

    class Wave {
        constructor(p, index) {
            this.p = p;
            this.yOffset = (index - 2) * 30; // Center the waves better
            this.amplitude = 50 + index * 20;
            this.frequency = 0.01 - index * 0.002;
            this.speed = 0.05 + index * 0.01;
            this.time = 0;
            this.baseAlpha = 150 - index * 25;
        }

        update() {
            this.time += this.speed;
        }

        display(progress) {
            // Alpha fades in then out
            let alpha = this.baseAlpha;
            if (progress < 0.3) {
                alpha = this.p.map(progress, 0, 0.3, 0, this.baseAlpha);
            } else if (progress > 0.7) {
                alpha = this.p.map(progress, 0.7, 1, this.baseAlpha, 0);
            }

            this.p.noFill();
            // Teal color: RGB(23, 115, 100)
            this.p.stroke(23, 115, 100, alpha);
            this.p.strokeWeight(4);

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
// INITIALIZE SKETCH
// ==========================================
let uploadWaveInstance;

document.addEventListener('DOMContentLoaded', function () {
    setTimeout(function () {
        let canvasElement = document.getElementById('upload-wave-canvas');
        if (canvasElement) {
            console.log('Initializing p5 wave sketch...');
            uploadWaveInstance = new p5(uploadWaveSketch);
        } else {
            console.error('Canvas element not found!');
        }
    }, 300);
});

// ==========================================
// DRAG AND DROP FUNCTIONALITY
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const uploadBox = document.querySelector('.upload-box-container');
    const fileInput = document.getElementById('jsonFileUpload');

    if (uploadBox && fileInput) {
        // Prevent default drag behaviors
        ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
            uploadBox.addEventListener(eventName, preventDefaults, false);
            document.body.addEventListener(eventName, preventDefaults, false);
        });

        // Highlight drop area when dragging over it
        ['dragenter', 'dragover'].forEach(eventName => {
            uploadBox.addEventListener(eventName, highlight, false);
        });

        ['dragleave', 'drop'].forEach(eventName => {
            uploadBox.addEventListener(eventName, unhighlight, false);
        });

        // Handle dropped files
        uploadBox.addEventListener('drop', handleDrop, false);

        function preventDefaults(e) {
            e.preventDefault();
            e.stopPropagation();
        }

        function highlight(e) {
            uploadBox.classList.add('drag-over');
        }

        function unhighlight(e) {
            uploadBox.classList.remove('drag-over');
        }

        function handleDrop(e) {
            const dt = e.dataTransfer;
            const files = dt.files;

            if (files.length > 0) {
                fileInput.files = files;
                updateFileName(files[0].name);
            }
        }
    }

    // Update file name display when file is selected
    if (fileInput) {
        fileInput.addEventListener('change', function (e) {
            if (this.files.length > 0) {
                updateFileName(this.files[0].name);
            }
        });
    }

    function updateFileName(fileName) {
        const fileNameDisplay = document.querySelector('.file-name-display');
        if (fileNameDisplay) {
            fileNameDisplay.textContent = '📄 ' + fileName;
            fileNameDisplay.style.display = 'block';
        }
    }
});

// ==========================================
// FUNCTION TO TRIGGER WAVE ANIMATION
// Call this from your ASP.NET code-behind after successful upload
// ==========================================
function triggerUploadSuccessAnimation() {
    console.log('triggerUploadSuccessAnimation called');

    if (uploadWaveInstance && typeof uploadWaveInstance.triggerAnimation === 'function') {
        uploadWaveInstance.triggerAnimation();
    } else {
        console.error('uploadWaveInstance not available or triggerAnimation method missing');
    }

    // Also animate the upload box
    if (typeof anime !== 'undefined') {
        anime({
            targets: '.upload-box-container',
            scale: [1, 1.02, 1],
            duration: 600,
            easing: 'easeInOutQuad'
        });
    } else {
        console.warn('anime.js not loaded');
    }
}

// ==========================================
// UPLOAD BUTTON ANIMATIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    const uploadButton = document.querySelector('.btn-upload');
    const browseButton = document.querySelector('.file-input-label');

    if (uploadButton) {
        uploadButton.addEventListener('mouseenter', function () {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: 1.05,
                    duration: 200,
                    easing: 'easeOutQuad'
                });
            }
        });

        uploadButton.addEventListener('mouseleave', function () {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: 1,
                    duration: 200,
                    easing: 'easeOutQuad'
                });
            }
        });
    }

    if (browseButton) {
        browseButton.addEventListener('mouseenter', function () {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: 1.05,
                    duration: 200,
                    easing: 'easeOutQuad'
                });
            }
        });

        browseButton.addEventListener('mouseleave', function () {
            if (typeof anime !== 'undefined') {
                anime({
                    targets: this,
                    scale: 1,
                    duration: 200,
                    easing: 'easeOutQuad'
                });
            }
        });
    }
});

// ==========================================
// SIMULATE PROGRESS BAR (if needed)
// ==========================================
function showUploadProgress() {
    const progressBar = document.querySelector('.upload-progress');
    const progressFill = document.querySelector('.upload-progress-bar');

    if (progressBar && progressFill) {
        progressBar.classList.add('active');

        if (typeof anime !== 'undefined') {
            anime({
                targets: progressFill,
                width: '100%',
                duration: 1500,
                easing: 'easeInOutQuad',
                complete: function () {
                    setTimeout(function () {
                        progressBar.classList.remove('active');
                        progressFill.style.width = '0%';
                    }, 500);
                }
            });
        }
    }
}