/**
 * LessonInteractivity.js
 * Handles interactive elements in lesson content, preventing unwanted postbacks
 * and managing piano/quiz functionality
 */

(function () {
    'use strict';

    // Wait for DOM to be fully loaded
    document.addEventListener('DOMContentLoaded', function () {
        initializeLessonInteractivity();
    });

    function initializeLessonInteractivity() {
        // Prevent all buttons inside lesson content from causing postbacks
        preventLessonButtonPostbacks();

        // Initialize piano play buttons if they exist
        initializePianoPlayButtons();

        // Initialize quiz buttons if they exist
        initializeQuizButtons();
    }

    /**
     * Prevents all button elements within lesson content from causing form submissions
     */
    function preventLessonButtonPostbacks() {
        // Find the lesson content container
        const lessonContent = document.querySelector('.lesson-content-body, #lessonContentBody, [data-lesson-content]');

        if (!lessonContent) {
            console.warn('Lesson content container not found');
            return;
        }

        // Get all buttons within lesson content
        const buttons = lessonContent.querySelectorAll('button');

        buttons.forEach(function (button) {
            // Set type to "button" to prevent form submission
            if (!button.hasAttribute('type')) {
                button.setAttribute('type', 'button');
            }

            // Add event listener to prevent default behavior
            button.addEventListener('click', function (e) {
                // Only prevent default if it's not explicitly a submit button
                if (button.getAttribute('type') !== 'submit') {
                    e.preventDefault();
                    e.stopPropagation();
                }
            }, true); // Use capture phase to ensure we catch it first
        });

        console.log(`Initialized ${buttons.length} lesson buttons to prevent postbacks`);
    }

    /**
     * Initialize piano play buttons (buttons that play chord notes)
     */
    function initializePianoPlayButtons() {
        const playButtons = document.querySelectorAll('.play-chord-btn, [data-play-notes]');

        playButtons.forEach(function (button) {
            button.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                // Get notes from data attribute
                const notes = button.getAttribute('data-notes') || button.getAttribute('data-play-notes');

                if (notes) {
                    playChordNotes(notes);
                } else {
                    console.error('No notes found for play button');
                }
            });
        });

        console.log(`Initialized ${playButtons.length} piano play buttons`);
    }

    /**
     * Initialize quiz answer buttons
     */
    function initializeQuizButtons() {
        const quizButtons = document.querySelectorAll('.quiz-answer-btn, [data-quiz-answer]');

        quizButtons.forEach(function (button) {
            button.addEventListener('click', function (e) {
                e.preventDefault();
                e.stopPropagation();

                handleQuizAnswer(button);
            });
        });

        console.log(`Initialized ${quizButtons.length} quiz answer buttons`);
    }

    /**
     * Play chord notes on the piano
     * @param {string} notesStr - Comma-separated MIDI note numbers or note names
     */
    function playChordNotes(notesStr) {
        if (!window.chordalPiano) {
            console.error('Piano not initialized. Make sure ChordalPianoRoll.js is loaded.');
            return;
        }

        try {
            // Parse notes (could be MIDI numbers or note names)
            const noteArray = notesStr.split(',').map(n => n.trim());

            // Clear any currently playing notes
            window.chordalPiano.releaseAll();

            // Play each note
            noteArray.forEach(function (note) {
                const midiNote = isNaN(note) ? noteNameToMidi(note) : parseInt(note);
                if (midiNote) {
                    window.chordalPiano.pressKey(midiNote);
                }
            });

            // Auto-release after 1 second
            setTimeout(function () {
                window.chordalPiano.releaseAll();
            }, 1000);

        } catch (error) {
            console.error('Error playing notes:', error);
        }
    }

    /**
     * Handle quiz answer selection
     * @param {HTMLElement} button - The clicked answer button
     */
    function handleQuizAnswer(button) {
        const isCorrect = button.getAttribute('data-correct') === 'true';
        const questionContainer = button.closest('.quiz-question, [data-quiz-question]');

        if (!questionContainer) {
            console.error('Quiz question container not found');
            return;
        }

        // Disable all buttons in this question
        const allButtons = questionContainer.querySelectorAll('.quiz-answer-btn, [data-quiz-answer]');
        allButtons.forEach(function (btn) {
            btn.disabled = true;
        });

        // Add visual feedback
        if (isCorrect) {
            button.classList.add('correct-answer');
            button.style.backgroundColor = '#10b981';
            button.style.color = 'white';
            showQuizFeedback(questionContainer, 'Correct! Well done.', true);
        } else {
            button.classList.add('incorrect-answer');
            button.style.backgroundColor = '#ef4444';
            button.style.color = 'white';

            // Highlight the correct answer
            allButtons.forEach(function (btn) {
                if (btn.getAttribute('data-correct') === 'true') {
                    btn.classList.add('correct-answer');
                    btn.style.backgroundColor = '#10b981';
                    btn.style.color = 'white';
                }
            });

            showQuizFeedback(questionContainer, 'Incorrect. The correct answer is highlighted.', false);
        }
    }

    /**
     * Show feedback message for quiz answer
     */
    function showQuizFeedback(container, message, isCorrect) {
        let feedbackDiv = container.querySelector('.quiz-feedback');

        if (!feedbackDiv) {
            feedbackDiv = document.createElement('div');
            feedbackDiv.className = 'quiz-feedback';
            feedbackDiv.style.marginTop = '10px';
            feedbackDiv.style.padding = '10px';
            feedbackDiv.style.borderRadius = '4px';
            feedbackDiv.style.fontWeight = 'bold';
            container.appendChild(feedbackDiv);
        }

        feedbackDiv.textContent = message;
        feedbackDiv.style.backgroundColor = isCorrect ? '#d1fae5' : '#fee2e2';
        feedbackDiv.style.color = isCorrect ? '#065f46' : '#991b1b';
        feedbackDiv.style.display = 'block';
    }

    /**
     * Convert note name to MIDI number
     * @param {string} noteName - e.g., "C4", "F#5", "Bb3"
     * @returns {number|null} - MIDI note number or null if invalid
     */
    function noteNameToMidi(noteName) {
        const noteMap = {
            'C': 0, 'C#': 1, 'Db': 1, 'D': 2, 'D#': 3, 'Eb': 3,
            'E': 4, 'F': 5, 'F#': 6, 'Gb': 6, 'G': 7, 'G#': 8,
            'Ab': 8, 'A': 9, 'A#': 10, 'Bb': 10, 'B': 11
        };

        // Extract note and octave
        const match = noteName.match(/^([A-G][#b]?)(-?\d+)$/i);
        if (!match) return null;

        const note = match[1].toUpperCase();
        const octave = parseInt(match[2]);

        if (!(note in noteMap)) return null;

        // MIDI note calculation: (octave + 1) * 12 + noteOffset
        return (octave + 1) * 12 + noteMap[note];
    }

    // Add a method to release all piano notes (useful for cleanup)
    window.releaseAllPianoNotes = function () {
        if (window.chordalPiano && typeof window.chordalPiano.releaseAll === 'function') {
            window.chordalPiano.releaseAll();
        }
    };

})();