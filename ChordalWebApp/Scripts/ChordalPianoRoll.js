class ChordalPianoRoll {
    constructor(containerId, options = {}) {
        this.container = document.getElementById(containerId);
        if (!this.container) {
            throw new Error(`Container element '${containerId}' not found`);
        }

        // Configuration
        this.options = {
            startOctave: options.startOctave || 3,
            octaveCount: options.octaveCount || 2,
            keyWidth: options.keyWidth || 40,
            whiteKeyHeight: options.whiteKeyHeight || 120,
            blackKeyHeight: options.blackKeyHeight || 80,
            interactive: options.interactive !== false,
            showNoteNames: options.showNoteNames !== false,
            sustainDuration: options.sustainDuration || 1.0, // seconds
            volume: options.volume || 0.3,
            ...options
        };

        // State
        this.activeNotes = new Set();
        this.pressedKeys = new Map(); // For mouse/touch tracking

        // Audio context
        this.initAudioContext();

        // Note data
        this.noteNames = ['C', 'C#', 'D', 'D#', 'E', 'F', 'F#', 'G', 'G#', 'A', 'A#', 'B'];
        this.whiteKeyIndices = [0, 2, 4, 5, 7, 9, 11]; // C, D, E, F, G, A, B
        this.blackKeyIndices = [1, 3, 6, 8, 10]; // C#, D#, F#, G#, A#

        // Build piano
        this.buildPiano();

        // Setup event listeners
        if (this.options.interactive) {
            this.setupInteraction();
        }
    }

    initAudioContext() {
        try {
            const AudioContext = window.AudioContext || window.webkitAudioContext;
            this.audioContext = new AudioContext();
            this.masterGain = this.audioContext.createGain();
            this.masterGain.gain.value = this.options.volume;
            this.masterGain.connect(this.audioContext.destination);
        } catch (e) {
            console.error('Web Audio API not supported', e);
        }
    }

    buildPiano() {
        // Create piano container
        this.pianoContainer = document.createElement('div');
        this.pianoContainer.className = 'chordal-piano-container';
        this.pianoContainer.style.cssText = `
            position: relative;
            display: inline-block;
            background: #333;
            padding: 10px;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.3);
            user-select: none;
        `;

        this.keysContainer = document.createElement('div');
        this.keysContainer.className = 'piano-keys';
        this.keysContainer.style.cssText = `
            position: relative;
            display: flex;
        `;

        this.keys = [];

        // Build white keys first
        let whiteKeyIndex = 0;
        for (let octave = 0; octave < this.options.octaveCount; octave++) {
            for (let noteIndex of this.whiteKeyIndices) {
                const key = this.createKey(octave, noteIndex, whiteKeyIndex, 'white');
                this.keys.push(key);
                this.keysContainer.appendChild(key.element);
                whiteKeyIndex++;
            }
        }

        // Add black keys on top
        whiteKeyIndex = 0;
        for (let octave = 0; octave < this.options.octaveCount; octave++) {
            for (let i = 0; i < this.noteNames.length; i++) {
                if (this.whiteKeyIndices.includes(i)) {
                    whiteKeyIndex++;
                } else {
                    // This is a black key
                    const key = this.createKey(octave, i, whiteKeyIndex - 0.7, 'black');
                    this.keys.push(key);
                    this.keysContainer.appendChild(key.element);
                }
            }
        }

        this.pianoContainer.appendChild(this.keysContainer);
        this.container.appendChild(this.pianoContainer);
    }

    createKey(octave, noteIndex, position, color) {
        const noteName = this.noteNames[noteIndex];
        const midiNote = (this.options.startOctave + octave) * 12 + noteIndex;

        const keyElement = document.createElement('div');
        keyElement.className = `piano-key piano-key-${color}`;
        keyElement.dataset.midiNote = midiNote;
        keyElement.dataset.noteName = noteName;

        const isWhite = color === 'white';
        const width = isWhite ? this.options.keyWidth : this.options.keyWidth * 0.6;
        const height = isWhite ? this.options.whiteKeyHeight : this.options.blackKeyHeight;
        const left = position * this.options.keyWidth;

        keyElement.style.cssText = `
            position: ${isWhite ? 'relative' : 'absolute'};
            width: ${width}px;
            height: ${height}px;
            ${!isWhite ? `left: ${left}px; z-index: 2;` : ''}
            background: ${isWhite ? 'linear-gradient(to bottom, #fff 0%, #f5f5f5 100%)' : 'linear-gradient(to bottom, #222 0%, #000 100%)'};
            border: 1px solid ${isWhite ? '#ccc' : '#000'};
            border-bottom: 2px solid ${isWhite ? '#aaa' : '#000'};
            cursor: ${this.options.interactive ? 'pointer' : 'default'};
            transition: all 0.05s ease;
            display: flex;
            flex-direction: column;
            justify-content: flex-end;
            align-items: center;
            padding-bottom: 8px;
            box-sizing: border-box;
        `;

        // Add note name label
        if (this.options.showNoteNames && isWhite) {
            const label = document.createElement('span');
            label.textContent = `${noteName}${this.options.startOctave + octave}`;
            label.style.cssText = `
                font-size: 11px;
                color: #999;
                font-family: Arial, sans-serif;
                pointer-events: none;
            `;
            keyElement.appendChild(label);
        }

        return {
            element: keyElement,
            midiNote: midiNote,
            noteName: noteName,
            octave: this.options.startOctave + octave,
            color: color
        };
    }

    setupInteraction() {
        // Mouse events
        this.keysContainer.addEventListener('mousedown', (e) => this.handleKeyPress(e));
        document.addEventListener('mouseup', (e) => this.handleKeyRelease(e));

        // Touch events
        this.keysContainer.addEventListener('touchstart', (e) => {
            e.preventDefault();
            this.handleKeyPress(e);
        });
        document.addEventListener('touchend', (e) => {
            e.preventDefault();
            this.handleKeyRelease(e);
        });

        // Keyboard events (optional)
        if (this.options.keyboardControl) {
            this.setupKeyboardControl();
        }
    }

    handleKeyPress(e) {
        let target = e.target;
        if (!target.classList.contains('piano-key')) {
            target = target.closest('.piano-key');
        }

        if (target && target.dataset.midiNote) {
            const midiNote = parseInt(target.dataset.midiNote);
            this.pressKey(midiNote);
            this.pressedKeys.set(target, midiNote);

            // Emit custom event
            this.container.dispatchEvent(new CustomEvent('noteOn', {
                detail: { midiNote, noteName: target.dataset.noteName }
            }));
        }
    }

    handleKeyRelease(e) {
        let target = e.target;
        if (!target.classList.contains('piano-key')) {
            target = target.closest('.piano-key');
        }

        // Release all pressed keys on mouse up
        this.pressedKeys.forEach((midiNote, keyElement) => {
            this.releaseKey(midiNote);

            // Emit custom event
            this.container.dispatchEvent(new CustomEvent('noteOff', {
                detail: { midiNote, noteName: keyElement.dataset.noteName }
            }));
        });
        this.pressedKeys.clear();
    }

    pressKey(midiNote) {
        const key = this.keys.find(k => k.midiNote === midiNote);
        if (!key) return;

        // Visual feedback
        const isWhite = key.color === 'white';
        key.element.style.background = isWhite
            ? 'linear-gradient(to bottom, #e8e8e8 0%, #d8d8d8 100%)'
            : 'linear-gradient(to bottom, #444 0%, #222 100%)';
        key.element.style.transform = 'translateY(2px)';

        // Play sound
        this.playNote(midiNote);

        this.activeNotes.add(midiNote);
    }

    releaseKey(midiNote) {
        const key = this.keys.find(k => k.midiNote === midiNote);
        if (!key) return;

        // Reset visual
        const isWhite = key.color === 'white';
        key.element.style.background = isWhite
            ? 'linear-gradient(to bottom, #fff 0%, #f5f5f5 100%)'
            : 'linear-gradient(to bottom, #222 0%, #000 100%)';
        key.element.style.transform = 'translateY(0)';

        this.activeNotes.delete(midiNote);
    }

    playNote(midiNote, duration = this.options.sustainDuration) {
        if (!this.audioContext) return;

        // Resume audio context if suspended (browser autoplay policy)
        if (this.audioContext.state === 'suspended') {
            this.audioContext.resume();
        }

        const frequency = this.midiToFrequency(midiNote);

        // Create oscillator
        const oscillator = this.audioContext.createOscillator();
        oscillator.type = 'triangle'; // Piano-like timbre
        oscillator.frequency.value = frequency;

        // Create envelope
        const gainNode = this.audioContext.createGain();
        gainNode.gain.value = 0;

        // ADSR envelope
        const now = this.audioContext.currentTime;
        const attackTime = 0.01;
        const decayTime = 0.2;
        const sustainLevel = 0.7;
        const releaseTime = 0.3;

        gainNode.gain.setValueAtTime(0, now);
        gainNode.gain.linearRampToValueAtTime(1, now + attackTime);
        gainNode.gain.linearRampToValueAtTime(sustainLevel, now + attackTime + decayTime);
        gainNode.gain.setValueAtTime(sustainLevel, now + duration);
        gainNode.gain.linearRampToValueAtTime(0, now + duration + releaseTime);

        // Connect nodes
        oscillator.connect(gainNode);
        gainNode.connect(this.masterGain);

        // Start and stop
        oscillator.start(now);
        oscillator.stop(now + duration + releaseTime);
    }

    playChord(midiNotes, duration = this.options.sustainDuration) {
        // Visual feedback
        midiNotes.forEach(note => {
            const key = this.keys.find(k => k.midiNote === note);
            if (key) {
                this.pressKey(note);
                // Release after duration
                setTimeout(() => this.releaseKey(note), duration * 1000);
            }
        });
    }

    highlightNotes(midiNotes, color = '#77aaff') {
        // Highlight specific notes without playing
        this.clearHighlights();

        midiNotes.forEach(note => {
            const key = this.keys.find(k => k.midiNote === note);
            if (key) {
                key.element.style.background = color;
                key.element.dataset.highlighted = 'true';
            }
        });
    }

    clearHighlights() {
        this.keys.forEach(key => {
            if (key.element.dataset.highlighted) {
                const isWhite = key.color === 'white';
                key.element.style.background = isWhite
                    ? 'linear-gradient(to bottom, #fff 0%, #f5f5f5 100%)'
                    : 'linear-gradient(to bottom, #222 0%, #000 100%)';
                delete key.element.dataset.highlighted;
            }
        });
    }

    getActiveNotes() {
        return Array.from(this.activeNotes);
    }

    reset() {
        this.activeNotes.forEach(note => this.releaseKey(note));
        this.activeNotes.clear();
        this.pressedKeys.clear();
        this.clearHighlights();
    }

    midiToFrequency(midiNote) {
        // A4 (MIDI note 69) = 440 Hz
        return 440 * Math.pow(2, (midiNote - 69) / 12);
    }

    destroy() {
        if (this.audioContext) {
            this.audioContext.close();
        }
        this.container.innerHTML = '';
    }
}

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = ChordalPianoRoll;
}