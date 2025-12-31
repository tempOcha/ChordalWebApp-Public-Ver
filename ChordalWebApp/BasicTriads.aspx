<%@ Page Title="Basic Triads - Chordal Learning" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="BasicTriads.aspx.cs" Inherits="ChordalWebApp.BasicTriads" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Basic Triads - Chordal Learning Centre
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        .lesson-container {
            max-width: 1000px;
            margin: 30px auto;
            padding: 20px;
        }
        
        .breadcrumb {
            font-size: 14px;
            color: #666;
            margin-bottom: 20px;
        }
        
        .breadcrumb a {
            color: #667eea;
            text-decoration: none;
        }
        
        .breadcrumb a:hover {
            text-decoration: underline;
        }
        
        .lesson-header {
            background: linear-gradient(135deg, var(--color-soft-green) 0%, var(--color-teal-medium) 100%);
            color: white;
            padding: 40px;
            border-radius: 12px;
            margin-bottom: 30px;
        }
        
        .lesson-title {
            font-size: 2.5em;
            margin: 0 0 15px 0;
        }
        
        .lesson-meta {
            display: flex;
            gap: 20px;
            font-size: 0.95em;
            opacity: 0.95;
            flex-wrap: wrap;
        }
        
        .meta-badge {
            padding: 6px 14px;
            background: cadetblue;
            border-radius: 20px;
        }
        
        .lesson-content {
            background: white;
            border-radius: 12px;
            padding: 50px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
            margin-bottom: 30px;
        }
        
        .triad-example {
            padding: 25px;
            border-radius: 8px;
            margin: 25px 0;
        }
        
        .triad-example h4 {
            margin-top: 0;
            font-size: 1.5em;
        }
        
        .triad-major { background: #f0f9ff; border-left: 4px solid #0ea5e9; }
        .triad-minor { background: #fef3c7; border-left: 4px solid #f59e0b; }
        .triad-diminished { background: #fee2e2; border-left: 4px solid #ef4444; }
        .triad-augmented { background: #f3e8ff; border-left: 4px solid #a855f7; }
        
        .piano-container {
            margin: 20px 0;
            padding: 15px;
            background: #f8f9fa;
            border-radius: 8px;
        }
        
        /* Piano Roll Visualization */
        .piano-roll {
            width: 100%;
            height: 180px;
            background: #fff;
            border: 2px solid #ddd;
            border-radius: 8px;
            position: relative;
            overflow: hidden;
            margin: 15px 0;
        }
        
        .piano-keys {
            position: absolute;
            left: 0;
            top: 0;
            width: 60px;
            height: 100%;
            background: #f5f5f5;
            border-right: 2px solid #ddd;
        }
        
        .piano-key {
            height: 15px;
            border-bottom: 1px solid #ddd;
            display: flex;
            align-items: center;
            padding-left: 8px;
            font-size: 11px;
            color: #666;
            position: absolute;
            width: 100%;
        }
        
        .piano-key.white {
            background: white;
        }
        
        .piano-key.black {
            background: #333;
            color: #fff;
        }
        
        .note-grid {
            position: absolute;
            left: 60px;
            top: 0;
            right: 0;
            height: 100%;
        }
        
        /* Grid background - alternating based on white/black keys */
        .grid-line {
            position: absolute;
            width: 100%;
            height: 15px;
            border-bottom: 1px solid #e5e5e5;
        }
        
        .grid-line.white-key {
            background: #fafafa;
        }
        
        .grid-line.black-key {
            background: #f0f0f0;
        }
        
        .note-bar {
            position: absolute;
            background: #667eea;
            border-radius: 4px;
            height: 13px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.2);
            opacity: 0.9;
            transition: all 0.3s;
        }
        
        .note-bar.playing {
            background: #4caf50;
            animation: pulse 0.3s ease-in-out;
        }
        
        @keyframes pulse {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.05); }
        }
        
        .play-button {
            background: #667eea;
            color: white;
            border: none;
            padding: 12px 24px;
            border-radius: 6px;
            font-size: 16px;
            cursor: pointer;
            margin-top: 15px;
            transition: all 0.3s;
        }
        
        .play-button:hover {
            background: #5568d3;
            transform: translateY(-2px);
        }
        
        .play-button:active {
            transform: translateY(0);
        }
        
        .play-button:disabled {
            background: #ccc;
            cursor: not-allowed;
        }
        
        .play-button.playing {
            background: #4caf50;
        }
        
        .quiz-section {
            margin-top: 50px;
            padding: 30px;
            background: #f8f9fa;
            border-radius: 12px;
        }
        
        .quiz-question {
            background: white;
            padding: 25px;
            border-radius: 8px;
            margin-bottom: 25px;
            box-shadow: 0 2px 8px rgba(0,0,0,0.05);
        }
        
        .quiz-question.completed {
            opacity: 0.7;
            pointer-events: none;
        }
        
        .quiz-question h4 {
            margin-top: 0;
            color: #333;
            font-size: 1.2em;
        }
        
        .quiz-options {
            display: grid;
            gap: 12px;
            margin-top: 20px;
        }
        
        .quiz-option {
            padding: 15px 20px;
            border: 2px solid #e5e7eb;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.3s;
            background: white;
            text-align: left;
            font-size: 16px;
        }
        
        .quiz-option:hover:not(:disabled) {
            border-color: #667eea;
            background: #f8f9ff;
        }
        
        .quiz-option.selected {
            border-color: #667eea;
            background: #f0f4ff;
        }
        
        .quiz-option.correct {
            border-color: #10b981;
            background: #d1fae5;
        }
        
        .quiz-option.incorrect {
            border-color: #ef4444;
            background: #fee2e2;
        }
        
        .quiz-option:disabled {
            cursor: not-allowed;
            opacity: 0.7;
        }
        
        .quiz-feedback {
            margin-top: 15px;
            padding: 15px;
            border-radius: 6px;
            font-weight: 500;
        }
        
        .quiz-feedback.correct {
            background: #d1fae5;
            color: #065f46;
        }
        
        .quiz-feedback.incorrect {
            background: #fee2e2;
            color: #991b1b;
        }
        
        .interactive-piano-quiz {
            margin-top: 20px;
        }
        
        .hint-button {
            background: #f59e0b;
            color: white;
            border: none;
            padding: 10px 20px;
            border-radius: 6px;
            font-size: 14px;
            cursor: pointer;
            margin-top: 10px;
        }
        
        .hint-button:hover {
            background: #d97706;
        }
        
        .hint-content {
            margin-top: 15px;
            padding: 15px;
            background: #fef3c7;
            border-left: 4px solid #f59e0b;
            border-radius: 6px;
            display: none;
        }
        
        .hint-content.visible {
            display: block;
        }
        
        .practice-tips {
            background: #dbeafe;
            padding: 25px;
            border-radius: 8px;
            margin-top: 30px;
        }
        
        .practice-tips h4 {
            margin-top: 0;
            color: #1e40af;
        }
        
        .practice-tips ul {
            margin: 15px 0;
            padding-left: 25px;
        }
        
        .practice-tips li {
            margin: 10px 0;
            line-height: 1.6;
        }
        
        .lesson-actions {
            background: white;
            padding: 25px;
            border-radius: 12px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.08);
            display: flex;
            justify-content: space-between;
            align-items: center;
            flex-wrap: wrap;
            gap: 15px;
        }
        
        .btn-complete {
            background: #4caf50;
            color: white;
            border: none;
            padding: 12px 30px;
            border-radius: 6px;
            font-size: 16px;
            cursor: pointer;
            transition: background 0.3s;
        }
        
        .btn-complete:hover {
            background: #45a049;
        }
        
        .btn-complete:disabled {
            background: #ccc;
            cursor: not-allowed;
        }
        
        .btn-secondary {
            background: #667eea;
            color: white;
            border: none;
            padding: 12px 30px;
            border-radius: 6px;
            font-size: 16px;
            cursor: pointer;
            text-decoration: none;
            display: inline-block;
        }
        
        .btn-secondary:hover {
            background: #5568d3;
            color: white;
        }
        
        .score-display {
            font-size: 1.2em;
            font-weight: 600;
            color: #667eea;
            margin: 20px 0;
        }
        
        /* Interactive Piano for Quiz */
        .interactive-piano {
            display: flex;
            justify-content: center;
            margin: 20px 0;
            position: relative;
            height: 220px;
        }
        
        .piano-white-keys {
            display: flex;
            gap: 2px;
            position: relative; /* Add this so black keys can position relative to white keys */
        }
        
        .piano-white-key {
            width: 50px;
            height: 200px;
            background: white;
            border: 2px solid #333;
            border-radius: 0 0 4px 4px;
            cursor: pointer;
            position: relative;
            display: flex;
            align-items: flex-end;
            justify-content: center;
            padding-bottom: 10px;
            font-size: 12px;
            color: #666;
            transition: all 0.2s;
        }
        
        .piano-white-key:hover {
            background: #f0f0f0;
        }
        
        .piano-white-key.active {
            background: #667eea;
            color: white;
        }
        
        .piano-black-keys {
            position: absolute;
            top: 0;
            left: 0; /* Now this left:0 is relative to .piano-white-keys */
            width: 100%;
            height: 120px;
            pointer-events: none;
        }
        
        .piano-black-key {
            width: 35px;
            height: 120px;
            background: #333;
            border: 2px solid #000;
            border-radius: 0 0 4px 4px;
            cursor: pointer;
            position: absolute;
            color: white;
            display: flex;
            align-items: flex-end;
            justify-content: center;
            padding-bottom: 8px;
            font-size: 11px;
            transition: all 0.2s;
            pointer-events: all;
        }
        
        .piano-black-key:hover {
            background: #444;
        }
        
        .piano-black-key.active {
            background: #667eea;
        }
        
        .tutorial-completed-overlay {
            background: rgba(255, 255, 255, 0.95);
            padding: 30px;
            border-radius: 8px;
            text-align: center;
            margin: 20px 0;
            border: 2px solid #4caf50;
        }
        
        .tutorial-completed-overlay h3 {
            color: #4caf50;
            margin-top: 0;
        }
    </style>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="lesson-container">
        
        <div class="breadcrumb">
            <a href="LearningCentre.aspx">← Learning Centre</a> / 
            <span>Basic Music Theory</span> /
            <span>Basic Triads</span>
        </div>
        
        <div class="lesson-header">
            <h1 class="lesson-title">Understanding Basic Triads</h1>
            <p style="font-size: 1.1em; opacity: 0.95; margin-bottom: 20px;">
                Learn the four fundamental types of triads that form the foundation of Western harmony
            </p>
            <div class="lesson-meta">
                <span class="meta-badge">Beginner</span>
                <span class="meta-badge">15 minutes</span>
                <span class="meta-badge">Interactive Tutorial</span>
            </div>
        </div>
        
        <!-- Lesson Content -->
        <div class="lesson-content">
            
            <h2>What Are Triads?</h2>
            <p style="font-size: 1.1em; line-height: 1.8;">
                Triads are three-note chords that form the foundation of Western harmony.
                They consist of a <strong>root note</strong>, a <strong>third</strong>, and a <strong>fifth</strong>. 
                The quality of these intervals determines whether the triad is major, minor, diminished, or augmented.
            </p>
            
            <h3 style="margin-top: 40px;">The Four Types of Triads</h3>
            
            <!-- Major Triad -->
            <div class="triad-example triad-major">
                <h4>1. Major Triad</h4>
                <p><strong>Formula:</strong> Root + Major 3rd + Perfect 5th</p>
                <p><strong>Example:</strong> C Major = C + E + G</p>
                <p><strong>Sound:</strong> Bright, happy, stable</p>
                
                <div class="piano-container">
                    <div class="piano-roll" id="roll-major">
                        <div class="piano-keys" id="keys-major"></div>
                        <div class="note-grid" id="grid-major"></div>
                    </div>
                    <button type="button" class="play-button" onclick="playTriad('major', [60, 64, 67], this)">
                        Play C Major Triad
                    </button>
                </div>
            </div>
            
            <!-- Minor Triad -->
            <div class="triad-example triad-minor">
                <h4>2. Minor Triad</h4>
                <p><strong>Formula:</strong> Root + Minor 3rd + Perfect 5th</p>
                <p><strong>Example:</strong> C Minor = C + E♭ + G</p>
                <p><strong>Sound:</strong> Sad, melancholic, introspective</p>
                
                <div class="piano-container">
                    <div class="piano-roll" id="roll-minor">
                        <div class="piano-keys" id="keys-minor"></div>
                        <div class="note-grid" id="grid-minor"></div>
                    </div>
                    <button type="button" class="play-button" onclick="playTriad('minor', [60, 63, 67], this)">
                        Play C Minor Triad
                    </button>
                </div>
            </div>
            
            <!-- Diminished Triad -->
            <div class="triad-example triad-diminished">
                <h4>3. Diminished Triad</h4>
                <p><strong>Formula:</strong> Root + Minor 3rd + Diminished 5th</p>
                <p><strong>Example:</strong> C Diminished = C + E♭ + G♭</p>
                <p><strong>Sound:</strong> Tense, unstable, dramatic</p>
                
                <div class="piano-container">
                    <div class="piano-roll" id="roll-diminished">
                        <div class="piano-keys" id="keys-diminished"></div>
                        <div class="note-grid" id="grid-diminished"></div>
                    </div>
                    <button type="button" class="play-button" onclick="playTriad('diminished', [60, 63, 66], this)">
                        Play C Diminished Triad
                    </button>
                </div>
            </div>
            
            <!-- Augmented Triad -->
            <div class="triad-example triad-augmented">
                <h4>4. Augmented Triad</h4>
                <p><strong>Formula:</strong> Root + Major 3rd + Augmented 5th</p>
                <p><strong>Example:</strong> C Augmented = C + E + G#</p>
                <p><strong>Sound:</strong> Dreamy, mysterious, floating</p>
                
                <div class="piano-container">
                    <div class="piano-roll" id="roll-augmented">
                        <div class="piano-keys" id="keys-augmented"></div>
                        <div class="note-grid" id="grid-augmented"></div>
                    </div>
                    <button type="button" class="play-button" onclick="playTriad('augmented', [60, 64, 68], this)">
                        Play C Augmented Triad
                    </button>
                </div>
            </div>
            
            <!-- Practice Tips -->
            <div class="practice-tips">
                <h4>Practice Tips</h4>
                <ul>
                    <li>Practice playing each triad type in different keys</li>
                    <li>Listen carefully to the emotional quality of each triad</li>
                    <li>Try building triads starting from different root notes</li>
                    <li>Experiment with inversions (rearranging the order of notes)</li>
                </ul>
            </div>
            
        </div>
        
        <!-- Interactive Tutorial Section -->
        <div class="quiz-section" id="tutorialSection">
            <h2 style="margin-top: 0;">Interactive Tutorial - Test Your Knowledge</h2>
            <p style="font-size: 1.1em; color: #666; margin-bottom: 30px;">
                Complete the exercises below to reinforce what you've learned about triads.
            </p>
            
            <div class="score-display">
                Score: <span id="score">0</span> / <span id="total">3</span>
            </div>
            
            <!-- Question 1 -->
            <div class="quiz-question" data-question-id="1">
                <h4>Question 1: Which triad has a minor 3rd and diminished 5th?</h4>
                <div class="quiz-options">
                    <button type="button" class="quiz-option" data-answer="false" onclick="checkAnswer(this, 1)">
                        A) Major Triad
                    </button>
                    <button type="button" class="quiz-option" data-answer="false" onclick="checkAnswer(this, 1)">
                        B) Minor Triad
                    </button>
                    <button type="button" class="quiz-option" data-answer="true" onclick="checkAnswer(this, 1)">
                        C) Diminished Triad
                    </button>
                    <button type="button" class="quiz-option" data-answer="false" onclick="checkAnswer(this, 1)">
                        D) Augmented Triad
                    </button>
                </div>
                <div class="quiz-feedback" style="display: none;"></div>
                <button type="button" class="hint-button" onclick="showHint(1)">Show Hint</button>
                <div class="hint-content" id="hint-1">
                    <strong>Hint:</strong> Look for the triad that has both intervals flattened (minor 3rd and diminished 5th). 
                    This creates the most tense and unstable sound.
                </div>
            </div>
            
            <!-- Question 2 -->
            <div class="quiz-question" data-question-id="2">
                <h4>Question 2: Which triad sounds bright and happy?</h4>
                <div class="quiz-options">
                    <button type="button" class="quiz-option" data-answer="true" onclick="checkAnswer(this, 2)">
                        A) Major Triad
                    </button>
                    <button type="button" class="quiz-option" data-answer="false" onclick="checkAnswer(this, 2)">
                        B) Minor Triad
                    </button>
                    <button type="button" class="quiz-option" data-answer="false" onclick="checkAnswer(this, 2)">
                        C) Diminished Triad
                    </button>
                    <button type="button" class="quiz-option" data-answer="false" onclick="checkAnswer(this, 2)">
                        D) Augmented Triad
                    </button>
                </div>
                <div class="quiz-feedback" style="display: none;"></div>
                <button type="button" class="hint-button" onclick="showHint(2)">Show Hint</button>
                <div class="hint-content" id="hint-2">
                    <strong>Hint:</strong> This is the most common triad in popular music and has a stable, consonant sound.
                </div>
            </div>
            
            <!-- Question 3: Interactive Piano -->
            <div class="quiz-question" data-question-id="3">
                <h4>Question 3: Build a C Major triad on the piano below</h4>
                <p style="color: #666; margin-bottom: 15px;">
                    Click on the piano keys to select: C, E, and G
                </p>
                <div class="interactive-piano-quiz">
                    <div class="interactive-piano" id="quiz-piano-container">
                        <div class="piano-white-keys" id="white-keys-container"></div>
                        <div class="piano-black-keys" id="black-keys-container"></div>
                    </div>
                    <button type="button" class="play-button" onclick="checkPianoAnswer()" style="margin-top: 15px;">
                        ✓ Check Answer
                    </button>
                </div>
                <div class="quiz-feedback" style="display: none;"></div>
                <button type="button" class="hint-button" onclick="showHint(3)">Show Hint</button>
                <div class="hint-content" id="hint-3">
                    <strong>Hint:</strong> C Major uses the white keys C, E, and G. Skip one white key between each note.
                </div>
            </div>
            
            <div id="completion-message" style="display: none; background: #d1fae5; padding: 25px; border-radius: 8px; margin-top: 30px; text-align: center;">
                <h3 style="color: #065f46; margin-top: 0;">Excellent Work!</h3>
                <p style="color: #065f46; font-size: 1.1em;">
                    You've completed all exercises. Your progress has been saved!
                </p>
            </div>
        </div>
        
        <!-- Lesson Actions -->
        <div class="lesson-actions">
            <div>
                <asp:Button ID="btnMarkComplete" runat="server" 
                    CssClass="btn-complete" 
                    Text="✓ Mark as Complete" 
                    OnClick="btnMarkComplete_Click" />
                <asp:Label ID="lblCompleteMessage" runat="server" 
                    Text="" 
                    Style="margin-left: 15px; color: #4caf50; font-weight: 500;"
                    Visible="false"></asp:Label>
            </div>
            <div>
                <a href="LearningCentre.aspx" class="btn-secondary">Back to Learning Centre</a>
            </div>
        </div>
        
    </div>
    
    <!-- Hidden fields for server-side data -->
    <asp:HiddenField ID="hdnLessonID" runat="server" />
    <asp:HiddenField ID="hdnExercise1ID" runat="server" />
    <asp:HiddenField ID="hdnExercise2ID" runat="server" />
    <asp:HiddenField ID="hdnExercise3ID" runat="server" />
    <asp:HiddenField ID="hdnTutorialCompleted" runat="server" Value="false" />
    
    <script>
        // Audio Context for Web Audio API
        let audioContext;
        let score = 0;
        let totalQuestions = 3;
        let answeredQuestions = new Set();
        let selectedNotes = new Set();
        let tutorialCompleted = false;

        // MIDI note names - correct order starting from C
        const noteNames = ['C', 'C#', 'D', 'D#', 'E', 'F', 'F#', 'G', 'G#', 'A', 'A#', 'B'];

        // Correct piano layout: C=0, C#=1, D=2, etc.
        const noteLayout = [
            { note: 60, name: 'C', isBlack: false },      // 0
            { note: 61, name: 'C#', isBlack: true },      // 1
            { note: 62, name: 'D', isBlack: false },      // 2
            { note: 63, name: 'D#', isBlack: true },      // 3
            { note: 64, name: 'E', isBlack: false },      // 4
            { note: 65, name: 'F', isBlack: false },      // 5
            { note: 66, name: 'F#', isBlack: true },      // 6
            { note: 67, name: 'G', isBlack: false },      // 7
            { note: 68, name: 'G#', isBlack: true },      // 8
            { note: 69, name: 'A', isBlack: false },      // 9
            { note: 70, name: 'A#', isBlack: true },      // 10
            { note: 71, name: 'B', isBlack: false }       // 11
        ];

        // Initialize on page load
        window.addEventListener('load', function () {
            initializeAudio();
            drawPianoRolls();
            createInteractivePiano();
            checkIfTutorialCompleted();
        });

        function initializeAudio() {
            try {
                audioContext = new (window.AudioContext || window.webkitAudioContext)();
            } catch (e) {
                console.error('Web Audio API not supported', e);
            }
        }

        function checkIfTutorialCompleted() {
            const completedField = document.getElementById('MainContent_hdnTutorialCompleted');
            if (completedField && completedField.value === 'true') {
                tutorialCompleted = true;
                showTutorialCompletedMessage();
            }
        }

        function showTutorialCompletedMessage() {
            const tutorialSection = document.getElementById('tutorialSection');
            const overlay = document.createElement('div');
            overlay.className = 'tutorial-completed-overlay';
            overlay.innerHTML = `
                <h3>✅ Tutorial Already Completed</h3>
                <p style="color: #666; font-size: 1.1em;">
                    You've already completed this tutorial with a perfect score!<br>
                    Your progress has been saved.
                </p>
            `;
            tutorialSection.innerHTML = '';
            tutorialSection.appendChild(overlay);
        }

        function drawPianoRolls() {
            drawPianoRoll('major', [60, 64, 67]);
            drawPianoRoll('minor', [60, 63, 67]);
            drawPianoRoll('diminished', [60, 63, 66]);
            drawPianoRoll('augmented', [60, 64, 68]);
        }

        function drawPianoRoll(type, midiNotes) {
            const keysContainer = document.getElementById(`keys-${type}`);
            const gridContainer = document.getElementById(`grid-${type}`);

            const startNote = 60; // C4

            // Draw piano keys AND grid backgrounds in correct order (C to B)
            for (let i = 0; i < 12; i++) {
                const note = startNote + i;
                const noteName = noteNames[i]; // Use index directly
                const isBlack = noteName.includes('#');

                // Create piano key
                const key = document.createElement('div');
                key.className = `piano-key ${isBlack ? 'black' : 'white'}`;
                key.textContent = noteName;
                key.style.top = `${(11 - i) * 15}px`; // Inverted for display (B at top, C at bottom)
                keysContainer.appendChild(key);

                // Create corresponding grid line
                const gridLine = document.createElement('div');
                gridLine.className = `grid-line ${isBlack ? 'black-key' : 'white-key'}`;
                gridLine.style.top = `${(11 - i) * 15}px`;
                gridContainer.appendChild(gridLine);
            }

            // Draw note bars - all start at same position
            midiNotes.forEach((note, index) => {
                const noteBar = document.createElement('div');
                noteBar.className = 'note-bar';
                noteBar.dataset.note = note;

                // Position based on MIDI note (inverted Y axis)
                const noteIndex = 11 - (note - startNote);
                noteBar.style.top = `${noteIndex * 15 + 1}px`;
                noteBar.style.left = '10px'; // Same start position
                noteBar.style.width = '150px'; // All same width

                gridContainer.appendChild(noteBar);
            });
        }

        function playTriad(type, midiNotes, button) {
            if (!audioContext) {
                initializeAudio();
            }

            if (audioContext.state === 'suspended') {
                audioContext.resume();
            }

            button.classList.add('playing');
            button.disabled = true;

            const gridContainer = document.getElementById(`grid-${type}`);
            const noteBars = gridContainer.querySelectorAll('.note-bar');
            noteBars.forEach(bar => bar.classList.add('playing'));

            midiNotes.forEach((midiNote, index) => {
                setTimeout(() => {
                    playNote(midiNote, 0.8);
                }, index * 50);
            });

            setTimeout(() => {
                button.classList.remove('playing');
                button.disabled = false;
                noteBars.forEach(bar => bar.classList.remove('playing'));
            }, 1000);
        }

        function playNote(midiNote, duration = 0.5) {
            if (!audioContext) return;

            const frequency = 440 * Math.pow(2, (midiNote - 69) / 12);
            const oscillator = audioContext.createOscillator();
            const gainNode = audioContext.createGain();

            oscillator.connect(gainNode);
            gainNode.connect(audioContext.destination);

            oscillator.frequency.value = frequency;
            oscillator.type = 'sine';

            gainNode.gain.setValueAtTime(0.3, audioContext.currentTime);
            gainNode.gain.exponentialRampToValueAtTime(0.01, audioContext.currentTime + duration);

            oscillator.start(audioContext.currentTime);
            oscillator.stop(audioContext.currentTime + duration);
        }

        function createInteractivePiano() {
            if (tutorialCompleted) return;

            const whiteKeysContainer = document.getElementById('white-keys-container');
            const blackKeysContainer = document.getElementById('black-keys-container');

            // Create white keys
            const whiteNotes = noteLayout.filter(n => !n.isBlack);
            whiteNotes.forEach(({ note, name }) => {
                const key = document.createElement('div');
                key.className = 'piano-white-key';
                key.dataset.note = note;
                key.textContent = name;
                key.onclick = () => toggleNote(note, key);
                whiteKeysContainer.appendChild(key);
            });

            // Create black keys with correct positioning
            const blackNotes = noteLayout.filter(n => n.isBlack);
            // White keys: 50px + 4px border = 54px total width
            // Gap between white keys: 2px
            // Spacing between left edges of consecutive white keys: 54 + 2 = 56px
            // Black keys: 35px wide
            // To center black key between two white keys:
            //   - Take position halfway between the two keys: 54px/2 + 2px/2 + 54px/2 = 55px
            //   - Subtract half the black key width: 55px - 17.5px = 37.5px
            // So each black key is 37.5px from the left edge of the white key before it
            const blackKeyPositions = {
                61: 275.5,   // C# - C is at 0, so C# at 0 + 37.5 = 37.5
                63: 332.5,   // D# - D is at 56, so D# at 56 + 37.5 = 93.5
                66: 435.5,  // F# - F is at 168 (56*3), so F# at 168 + 37.5 = 205.5
                68: 485.5,  // G# - G is at 224 (56*4), so G# at 224 + 37.5 = 261.5
                70: 535.5   // A# - A is at 280 (56*5), so A# at 280 + 37.5 = 317.5
            };

            blackNotes.forEach(({ note, name }) => {
                const key = document.createElement('div');
                key.className = 'piano-black-key';
                key.dataset.note = note;
                key.textContent = name;
                key.onclick = () => toggleNote(note, key);
                key.style.left = `${blackKeyPositions[note]}px`;
                blackKeysContainer.appendChild(key);
            });
        }

        function toggleNote(note, keyElement) {
            if (tutorialCompleted) return;
            if (answeredQuestions.has(3)) return; // Question 3 already answered

            if (selectedNotes.has(note)) {
                selectedNotes.delete(note);
                keyElement.classList.remove('active');
            } else {
                selectedNotes.add(note);
                keyElement.classList.add('active');
                playNote(note, 0.3);
            }
        }

        function checkAnswer(button, questionId) {
            if (tutorialCompleted) return;
            if (answeredQuestions.has(questionId)) return;

            const isCorrect = button.getAttribute('data-answer') === 'true';
            const question = document.querySelector(`[data-question-id="${questionId}"]`);
            const options = question.querySelectorAll('.quiz-option');
            const feedback = question.querySelector('.quiz-feedback');

            options.forEach(opt => opt.disabled = true);

            if (isCorrect) {
                button.classList.add('correct');
                feedback.className = 'quiz-feedback correct';
                feedback.textContent = '✓ Correct! Well done!';
                feedback.style.display = 'block';
                score++;
                answeredQuestions.add(questionId);
                question.classList.add('completed');
                recordAttempt(questionId, true);
            } else {
                button.classList.add('incorrect');
                options.forEach(opt => {
                    if (opt.getAttribute('data-answer') === 'true') {
                        opt.classList.add('correct');
                    }
                });
                feedback.className = 'quiz-feedback incorrect';
                feedback.textContent = '✗ Not quite. The correct answer is highlighted above.';
                feedback.style.display = 'block';
                answeredQuestions.add(questionId);
                question.classList.add('completed');
                recordAttempt(questionId, false);
            }

            updateScore();
            checkCompletion();
        }

        function checkPianoAnswer() {
            if (tutorialCompleted) return;

            const questionId = 3;
            if (answeredQuestions.has(questionId)) return;

            const question = document.querySelector(`[data-question-id="3"]`);
            const feedback = question.querySelector('.quiz-feedback');

            const correctNotes = new Set([60, 64, 67]);
            const isCorrect = selectedNotes.size === 3 &&
                [...selectedNotes].every(note => correctNotes.has(note));

            if (isCorrect) {
                feedback.className = 'quiz-feedback correct';
                feedback.textContent = '✓ Perfect! You built a C Major triad correctly!';
                feedback.style.display = 'block';
                score++;
                answeredQuestions.add(questionId);
                question.classList.add('completed');

                [60, 64, 67].forEach((note, i) => {
                    setTimeout(() => playNote(note, 0.8), i * 50);
                });

                recordAttempt(questionId, true);
            } else {
                feedback.className = 'quiz-feedback incorrect';
                feedback.textContent = '✗ Not quite right. You need C, E, and G. Try again!';
                feedback.style.display = 'block';
                recordAttempt(questionId, false);
            }

            updateScore();
            checkCompletion();
        }

        function showHint(questionId) {
            const hint = document.getElementById(`hint-${questionId}`);
            hint.classList.toggle('visible');
        }

        function updateScore() {
            document.getElementById('score').textContent = score;
        }

        function checkCompletion() {
            if (answeredQuestions.size === totalQuestions && score === totalQuestions) {
                tutorialCompleted = true;
                document.getElementById('completion-message').style.display = 'block';
                document.getElementById('completion-message').scrollIntoView({
                    behavior: 'smooth',
                    block: 'center'
                });

                // Disable all quiz interactions
                document.querySelectorAll('.quiz-question').forEach(q => {
                    q.classList.add('completed');
                });
            }
        }

        function recordAttempt(questionId, isCorrect) {
            let exerciseIdField = document.getElementById('MainContent_hdnExercise' + questionId + 'ID');
            if (!exerciseIdField || !exerciseIdField.value) {
                console.warn('Exercise ID not found for question ' + questionId);
                return;
            }

            let exerciseId = parseInt(exerciseIdField.value);
            let scorePercent = (score / totalQuestions) * 100;

            fetch('BasicTriads.aspx/RecordTutorialAttempt', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    exerciseId: exerciseId,
                    isCorrect: isCorrect,
                    score: scorePercent
                })
            })
                .then(response => {
                    if (!response.ok) {
                        console.error('Failed to record attempt');
                    }
                })
                .catch(err => console.error('Error recording attempt:', err));
        }
    </script>
</asp:Content>
