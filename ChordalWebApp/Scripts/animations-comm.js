// ==========================================
// CHORDAL COMMUNITY WAVE ANIMATIONS
// Using p5.js for subtle wave background
// ==========================================

// ==========================================
// COMMUNITY HEADER WAVE ANIMATION
// ==========================================
let communityWaveSketch = function (p) {
    let waves = [];
    let numWaves = 4;

    p.setup = function () {
        let container = document.getElementById('community-wave-canvas');
        if (container) {
            // Get parent section height
            let headerSection = container.closest('.community-header-section');
            let canvasHeight = headerSection ? headerSection.offsetHeight : 300;

            let canvas = p.createCanvas(p.windowWidth, canvasHeight);
            canvas.id('community-wave-canvas');
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
        let container = document.getElementById('community-wave-canvas');
        if (container) {
            let headerSection = container.closest('.community-header-section');
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
            this.alpha = 120 - index * 25; // More visible waves
        }

        update() {
            this.time += this.speed;
        }

        display() {
            this.p.noFill();
            this.p.stroke(255, 255, 255, this.alpha); // White waves on teal background
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

// Initialize community wave animation if canvas exists
document.addEventListener('DOMContentLoaded', function () {
    // Small delay to ensure the header is rendered
    setTimeout(() => {
        if (document.getElementById('community-wave-canvas')) {
            new p5(communityWaveSketch);
        }
    }, 100);
});

// ==========================================
// CARD ANIMATION ON SCROLL
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
                }, index * 50); // Stagger animation
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
// FILTER INTERACTIONS
// ==========================================
document.addEventListener('DOMContentLoaded', function () {
    // Smooth scroll to results when applying filters
    const searchButton = document.querySelector('.btn-search');
    if (searchButton) {
        searchButton.addEventListener('click', function () {
            setTimeout(() => {
                const resultsBar = document.querySelector('.results-bar');
                if (resultsBar) {
                    resultsBar.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
                }
            }, 100);
        });
    }

    // Add animation to filter inputs on focus
    const filterInputs = document.querySelectorAll('.filter-input, .filter-select');
    filterInputs.forEach(input => {
        input.addEventListener('focus', function () {
            this.style.transform = 'scale(1.02)';
        });
        input.addEventListener('blur', function () {
            this.style.transform = 'scale(1)';
        });
    });
});

// ==========================================
// MIDI VISUALIZATION & AUDIO PLAYBACK
// For CommunityProgressionView.aspx
// ==========================================

// P5.js sketch for MIDI-style visualization
let midiVisualizationSketch = function (p) {
    let progressionData = [];
    let currentTime = 0;
    let isPlaying = false;
    let startTime = 0;
    let tempo = 120; // BPM
    let barHeight = 1;
    let noteColor = '#177364';
    let showPlayhead = true;
    let synth = null;
    let waveformType = 'sine';

    // Audio context
    let audioInitialized = false;

    p.setup = function () {
        const container = document.getElementById('progression-canvas');
        if (container) {
            const canvas = p.createCanvas(container.offsetWidth, 400);
            canvas.parent('midi-visualization-container');

            // Load progression data from hidden field
            loadProgressionData();

            // Set up controls
            setupMidiControls(p);
        }
    };

    function loadProgressionData() {
        // Use the ClientID passed from inline script, fallback to direct ID
        const fieldId = window.chordalProgressionJSONFieldID || 'hdnProgressionJSON';
        const jsonField = document.getElementById(fieldId);

        console.log('=== MIDI VISUALIZATION DEBUG ===');
        console.log('1. Looking for field ID:', fieldId);
        console.log('2. Hidden field element:', jsonField);
        console.log('3. Hidden field value:', jsonField ? jsonField.value : 'NOT FOUND');

        if (jsonField && jsonField.value) {
            try {
                const chords = JSON.parse(jsonField.value);
                console.log('4. Parsed JSON successfully:', chords);
                console.log('5. Number of chords:', chords.length);

                progressionData = chords.map((chord) => ({
                    name: chord.ChordName || chord.name || 'Unknown',
                    startTime: chord.StartTime || 0,
                    duration: chord.Duration || 2.0,
                    notes: chord.Notes || [], // Use actual MIDI notes from database
                    color: noteColor,
                    romanNumeral: chord.RomanNumeral || '',
                    chordFunction: chord.ChordFunction || ''
                }));
                console.log('6. Loaded progression data:', progressionData);
                console.log('7. First chord notes:', progressionData[0] ? progressionData[0].notes : 'NO CHORDS');
            } catch (e) {
                console.error('ERROR parsing progression data:', e);
                console.log('Creating sample data instead...');
                createSampleData();
            }
        } else {
            console.warn('No progression JSON found, using sample data');
            createSampleData();
        }
    }

    function createSampleData() {
        // Create sample progression for display
        const sampleChords = [
            { name: 'C', notes: [60, 64, 67] },
            { name: 'Am', notes: [57, 60, 64] },
            { name: 'F', notes: [53, 57, 60] },
            { name: 'G', notes: [55, 59, 62] }
        ];

        progressionData = sampleChords.map((chord, index) => ({
            name: chord.name,
            startTime: index * 2,
            duration: 2,
            notes: chord.notes,
            color: noteColor,
            romanNumeral: '',
            chordFunction: ''
        }));
    }

    function setupMidiControls(p) {
        // Play button
        const playBtn = document.getElementById('play-button');
        if (playBtn) {
            playBtn.disabled = false;
            playBtn.addEventListener('click', (e) => {
                e.preventDefault(); // Prevent form submission
                startPlayback(p);
            });
        }

        // Stop button
        const stopBtn = document.getElementById('stop-button');
        if (stopBtn) {
            stopBtn.addEventListener('click', (e) => {
                e.preventDefault(); // Prevent form submission
                stopPlayback(p);
            });
        }

        // Tempo slider
        const tempoSlider = document.getElementById('tempo-slider');
        const tempoValue = document.getElementById('tempo-value');
        if (tempoSlider && tempoValue) {
            tempoSlider.addEventListener('input', (e) => {
                tempo = parseInt(e.target.value);
                tempoValue.textContent = tempo;
                console.log('Tempo changed to:', tempo);

                // Update Tone.js Transport BPM if audio is initialized
                if (audioInitialized) {
                    Tone.Transport.bpm.value = tempo;
                    console.log('Transport BPM updated to:', tempo);
                }
            });
        }

        // Bar height slider
        const heightSlider = document.getElementById('bar-height-slider');
        const heightValue = document.getElementById('height-value');
        if (heightSlider && heightValue) {
            heightSlider.addEventListener('input', (e) => {
                barHeight = parseInt(e.target.value);
                heightValue.textContent = barHeight;
            });
        }

        // Note color picker
        const colorPicker = document.getElementById('note-color-picker');
        if (colorPicker) {
            colorPicker.addEventListener('input', (e) => {
                noteColor = e.target.value;
                // Update all chord colors
                progressionData.forEach(chord => chord.color = noteColor);
            });
        }

        // Playhead toggle
        const playheadToggle = document.getElementById('show-playhead');
        if (playheadToggle) {
            playheadToggle.addEventListener('change', (e) => {
                showPlayhead = e.target.checked;
            });
        }

        // Waveform selector
        const waveformSelect = document.getElementById('waveform-select');
        if (waveformSelect) {
            waveformSelect.addEventListener('change', (e) => {
                waveformType = e.target.value;
                console.log('Waveform changed to:', waveformType);

                // Need to recreate the synth with new waveform type
                if (synth) {
                    synth.dispose(); // Clean up old synth
                    synth = new Tone.PolySynth(Tone.Synth, {
                        oscillator: { type: waveformType },
                        envelope: {
                            attack: 0.05,
                            decay: 0.1,
                            sustain: 0.3,
                            release: 1
                        },
                        volume: -10
                    }).toDestination();
                    console.log('Synth recreated with new waveform');

                    // If currently playing, restart with new synth
                    if (isPlaying) {
                        stopPlayback(p);
                        setTimeout(() => startPlayback(p), 100);
                    }
                }
            });
        }
    }

    async function startPlayback(p) {
        console.log('=== START PLAYBACK ===');

        // Initialize Tone.js on first interaction
        if (!audioInitialized) {
            console.log('Initializing Tone.js audio context...');
            await Tone.start();
            console.log('Tone.js started, audio context state:', Tone.context.state);
            audioInitialized = true;

            // Create synth with volume control
            synth = new Tone.PolySynth(Tone.Synth, {
                oscillator: { type: waveformType },
                envelope: {
                    attack: 0.05,
                    decay: 0.1,
                    sustain: 0.3,
                    release: 1
                },
                volume: -10
            }).toDestination();

            console.log('Synth created:', synth);
        }

        if (!progressionData || progressionData.length === 0) {
            console.error('No progression data to play!');
            alert('No chord progression data loaded. Please ensure the progression has chords.');
            return;
        }

        console.log('Starting playback with', progressionData.length, 'chords');

        isPlaying = true;
        startTime = p.millis();
        currentTime = 0;

        // Enable/disable buttons
        document.getElementById('play-button').disabled = true;
        document.getElementById('stop-button').disabled = false;

        // Schedule all chords
        scheduleChords();
    }

    function scheduleChords() {
        if (!synth) return;

        // Clear any scheduled events
        Tone.Transport.cancel();
        Tone.Transport.stop();

        // Set tempo
        Tone.Transport.bpm.value = tempo;

        console.log('=== SCHEDULING AUDIO ===');
        console.log('Tempo:', tempo);
        console.log('Number of chords to schedule:', progressionData.length);

        progressionData.forEach((chord, index) => {
            console.log(`Chord ${index + 1}: ${chord.name}, Notes:`, chord.notes, 'Start:', chord.startTime, 'Duration:', chord.duration);

            // Make sure we have notes
            if (!chord.notes || chord.notes.length === 0) {
                console.warn(`Chord ${chord.name} has no notes!`);
                return;
            }

            // Schedule this chord at its start time
            Tone.Transport.schedule((time) => {
                console.log(`Playing chord: ${chord.name} at time ${time}`);

                // Convert MIDI notes to note names and play them
                const noteNames = chord.notes.map(midiNote =>
                    Tone.Frequency(midiNote, "midi").toNote()
                );

                console.log('Playing notes:', noteNames);

                // Trigger all notes in the chord together
                synth.triggerAttackRelease(noteNames, chord.duration, time);

            }, chord.startTime);
        });

        // Start transport
        console.log('Starting Tone.js Transport...');
        Tone.Transport.start();
    }

    function stopPlayback(p) {
        console.log('=== STOP PLAYBACK ===');
        isPlaying = false;
        currentTime = 0;

        // Stop audio
        if (synth) {
            console.log('Stopping Tone.js Transport...');
            Tone.Transport.stop();
            Tone.Transport.cancel();
            synth.releaseAll(); // Release all currently playing notes
            console.log('Transport stopped and notes released');
        }

        // Enable/disable buttons
        document.getElementById('play-button').disabled = false;
        document.getElementById('stop-button').disabled = true;
    }

    p.draw = function () {
        p.background(245, 250, 248); // Soft green background

        // Update time if playing
        if (isPlaying) {
            currentTime = (p.millis() - startTime) / 1000; // Convert to seconds

            // Stop at the end
            const totalDuration = progressionData.length > 0
                ? progressionData[progressionData.length - 1].startTime +
                progressionData[progressionData.length - 1].duration
                : 0;

            if (currentTime >= totalDuration) {
                stopPlayback(p);
            }
        }

        // Calculate visible range
        const padding = 80;
        const visualWidth = p.width - padding * 2;
        const visualHeight = p.height - padding * 2;

        // Find total duration
        const totalDuration = progressionData.length > 0
            ? progressionData[progressionData.length - 1].startTime +
            progressionData[progressionData.length - 1].duration
            : 10;

        // Draw grid lines (measures)
        p.stroke(220);
        p.strokeWeight(2);
        for (let i = 0; i <= totalDuration; i += 2) {
            const x = p.map(i, 0, totalDuration, padding, padding + visualWidth);
            p.line(x, padding, x, padding + visualHeight);

            // Measure numbers
            p.noStroke();
            p.fill(150);
            p.textAlign(p.CENTER, p.TOP);
            p.textSize(10);
            p.text(Math.floor(i / 2) + 1, x , padding + visualHeight + 30);
        }

        // Draw chords as horizontal bars
        progressionData.forEach(chord => {
            const x1 = p.map(chord.startTime, 0, totalDuration, padding, padding + visualWidth);
            const x2 = p.map(chord.startTime + chord.duration, 0, totalDuration, padding, padding + visualWidth);
            const barWidth = x2 - x1;

            // Draw each note in the chord as a bar
            chord.notes.forEach((note, index) => {
                const noteY = p.map(note, 48, 84, padding + visualHeight - 80, padding + 20);

                // Highlight if currently playing
                const isCurrentlyPlaying = isPlaying &&
                    currentTime >= chord.startTime &&
                    currentTime < chord.startTime + chord.duration;

                p.noStroke();
                if (isCurrentlyPlaying) {
                    p.fill(p.color(chord.color));
                    p.rect(x1, noteY, barWidth, barHeight, 3);

                    // Add glow effect
                    p.fill(p.color(chord.color + '40'));
                    p.rect(x1 - 2, noteY - 2, barWidth + 4, barHeight + 4, 3);
                } else {
                    p.fill(p.color(chord.color + '80'));
                    p.rect(x1, noteY, barWidth, barHeight, 3);
                }
            });

            // Draw chord name
            p.noStroke();
            p.fill(100);
            p.textAlign(p.CENTER, p.BOTTOM);
            p.textSize(16);
            p.text(chord.romanNumeral, x1 + barWidth / 2, padding - 5);

            p.noStroke();
            p.fill(100);
            p.textAlign(p.CENTER, p.BOTTOM);
            p.textSize(10);
            p.text(chord.name, x1 + barWidth / 2, padding + 10);
        });

        // Draw playhead
        if (showPlayhead && isPlaying) {
            const playheadX = p.map(currentTime, 0, totalDuration, padding, padding + visualWidth);
            p.stroke(p.color(noteColor));
            p.strokeWeight(2);
            p.line(playheadX, padding, playheadX, padding + visualHeight);

            // Playhead indicator
            p.noStroke();
            p.fill(p.color(noteColor));
            p.triangle(
                playheadX, padding - 10,
                playheadX - 5, padding,
                playheadX + 5, padding
            );
        }
    };

    p.windowResized = function () {
        const container = document.getElementById('progression-canvas');
        if (container) {
            p.resizeCanvas(container.offsetWidth, 400);
        }
    };
};

// Initialize MIDI visualization if on progression view page
document.addEventListener('DOMContentLoaded', function () {
    setTimeout(() => {
        if (document.getElementById('progression-canvas')) {
            new p5(midiVisualizationSketch);
        }
    }, 100);
});