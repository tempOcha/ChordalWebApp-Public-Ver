<%@ Page Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="LandingPage.aspx.cs" Inherits="ChordalWebApp.LandingPage" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Hero Section with p5.js Background -->
    <section class="hero-section">
        <div id="hero-canvas-container"></div>
        <div class="hero-overlay"></div>
        <div class="container hero-content">
            <div class="row align-items-center min-vh-100">
                <div class="col-lg-7">
                    <div class="hero-text fade-in">
                        <h1 class="hero-title">
                            Compose with
                            <span class="gradient-text">Confidence</span>
                        </h1>
                        <p class="hero-subtitle">
                            Real-time chord analysis and music theory guidance for modern producers
                        </p>
                        <p class="hero-description">
                            Bridge the gap between inspiration and understanding. Chordal brings music theory 
                            directly into your DAW with intelligent chord recognition, harmonic analysis, 
                            and educational insights—all while you create.
                        </p>
                        <div class="hero-cta">
                            <asp:PlaceHolder ID="LandingPageAnonymousLinks" runat="server">
                                <asp:HyperLink NavigateUrl="~/Register.aspx" runat="server" CssClass="btn btn-primary btn-hero btn-pulse" Text="Get Started Free" />
                                <asp:HyperLink NavigateUrl="~/Login.aspx" runat="server" CssClass="btn btn-outline-light btn-hero" Text="Sign In" />
                                <a href="https://drive.google.com/uc?export=download&id=1PJspapefg1nybBrxVkTyypuGGp1auRiR" target="_blank" class="btn btn-outline-light btn-hero">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="vertical-align: middle; margin-right: 8px;">
                                        <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                                        <polyline points="7 10 12 15 17 10"></polyline>
                                        <line x1="12" y1="15" x2="12" y2="3"></line>
                                    </svg>
                                    Download VST
                                </a>
                            </asp:PlaceHolder>
                            <asp:PlaceHolder ID="LandingPageAuthenticatedLinks" runat="server" Visible="false">
                                <asp:HyperLink NavigateUrl="~/UploadProgression.aspx" runat="server" CssClass="btn btn-primary btn-hero" Text="Upload Progression" />
                                <asp:HyperLink NavigateUrl="~/MyProgressionsListEnhanced.aspx" runat="server" CssClass="btn btn-outline-light btn-hero" Text="My Progressions" />
                                <a href="https://drive.google.com/uc?export=download&id=1PJspapefg1nybBrxVkTyypuGGp1auRiR" target="_blank" class="btn btn-outline-light btn-hero">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" style="vertical-align: middle; margin-right: 8px;">
                                        <path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path>
                                        <polyline points="7 10 12 15 17 10"></polyline>
                                        <line x1="12" y1="15" x2="12" y2="3"></line>
                                    </svg>
                                    Download VST
                                </a>
                            </asp:PlaceHolder>
                        </div>
                        <div class="hero-stats">
                            <div class="stat-item">
                                <span class="stat-number">Real-time</span>
                                <span class="stat-label">Analysis</span>
                            </div>
                            <div class="stat-item">
                                <span class="stat-number">VST Plugin</span>
                                <span class="stat-label">DAW Integration</span>
                            </div>
                            <div class="stat-item">
                                <span class="stat-number">Learn & Create</span>
                                <span class="stat-label">Educational Tools</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- Features Section -->
    <section class="features-section">
        <div class="container">
            <div class="section-header text-center">
                <h2 class="section-title">Two Powerful Components, One Vision</h2>
                <p class="section-subtitle">A complete music theory ecosystem for producers and learners</p>
            </div>
            
            <div class="row g-4">
                <div class="col-lg-6">
                    <div class="feature-card feature-card-vst">
                        <div class="feature-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <path d="M9 18V5l12-2v13"></path>
                                <circle cx="6" cy="18" r="3"></circle>
                                <circle cx="18" cy="16" r="3"></circle>
                            </svg>
                        </div>
                        <h3 class="feature-title">VST Plugin</h3>
                        <p class="feature-description">
                            Seamlessly integrates with FL Studio, Ableton, Logic Pro, and other major DAWs. 
                            Analyze MIDI in real-time with Roman numeral notation, chord function charts, 
                            and instant theory tooltips right in your workflow.
                        </p>
                        <ul class="feature-list">
                            <li>Real-time chord identification</li>
                            <li>Roman numeral analysis</li>
                            <li>Harmonic function visualization</li>
                            <li>Export progressions as JSON</li>
                            <li>Zero-latency processing</li>
                        </ul>
                        <div class="feature-note">
                            <video width="500" height="360" loop autoplay muted playsinline>
                                <source src="Images/videovst.mp4" type="video/mp4">
                            </video>                  
                        </div>
                        <p class="text-muted text-center mt-2" style="font-size: 0.875rem;">
                            Real-time chord analysis directly in your DAW
                        </p>
                    </div>
                </div>
                
                <div class="col-lg-6">
                    <div class="feature-card feature-card-web">
                        <div class="feature-icon">
                            <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <rect x="2" y="3" width="20" height="14" rx="2" ry="2"></rect>
                                <line x1="8" y1="21" x2="16" y2="21"></line>
                                <line x1="12" y1="17" x2="12" y2="21"></line>
                            </svg>
                        </div>
                        <h3 class="feature-title">Web Platform</h3>
                        <p class="feature-description">
                            Your personal music theory hub. Upload progressions, track your learning journey, 
                            explore community creations, and dive deep into structured theory lessons with 
                            interactive visualizations.
                        </p>
                        <ul class="feature-list">
                            <li>Progression library & management</li>
                            <li>Interactive learning center</li>
                            <li>Progress tracking & analytics</li>
                            <li>Community sharing & discovery</li>
                            <li>Export to MIDI files</li>
                        </ul>
                        <div class="feature-note">
                            <video width="500" height="360" loop autoplay muted playsinline>
                                <source src="Images/videoweb.mp4" type="video/mp4">
                            </video>                  
                        </div>
                        <p class="text-muted text-center mt-2" style="font-size: 0.875rem;">
                            Explore, learn and share with other like-minded musicians 
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- MIDI Visualization Section -->
    <section class="midi-viz-section">
        <div class="container-fluid">
            <div class="section-header text-center">
                <h2 class="section-title">See Your Music Come to Life</h2>
                <p class="section-subtitle">Real-time visualization of chord progressions</p>
            </div>
            <div id="midi-roll-container"></div>
        </div>
    </section>

    <!-- Chord Progression Animation Section -->
    <section class="chord-animation-section">
        <div class="container">
            <div class="row align-items-center">
                <div class="col-lg-6">
                    <h2 class="section-title">Master Harmonic Theory</h2>
                    <p class="section-text">
                        Watch chord progressions unfold in real-time with animated Roman numeral notation. 
                        Understand the relationships between chords and how they create emotional movement 
                        in your music.
                    </p>
                    <ul class="benefit-list">
                        <li>Learn common progressions: I-V-vi-IV, ii-V-I, and more</li>
                        <li>Understand chord functions and substitutions</li>
                        <li>Explore modal interchange and borrowed chords</li>
                        <li>Build your harmonic vocabulary naturally</li>
                    </ul>
                </div>
                <div class="col-lg-6">
                    <div id="chord-progression-container"></div>
                </div>
            </div>
        </div>
    </section>

    <!-- How It Works Section -->
    <section class="how-it-works-section">
        <div class="container">
            <div class="section-header text-center">
                <h2 class="section-title">Simple, Powerful Workflow</h2>
                <p class="section-subtitle">From production to analysis in three easy steps</p>
            </div>
            
            <div class="row g-4">
                <div class="col-md-4">
                    <div class="step-card">
                        <div class="step-number">01</div>
                        <h3 class="step-title">Create in Your DAW</h3>
                        <p class="step-description">
                            Load Chordal VST in your favorite DAW. Play MIDI or route tracks through 
                            the plugin for instant chord analysis and theory insights.
                        </p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="step-card">
                        <div class="step-number">02</div>
                        <h3 class="step-title">Analyze & Learn</h3>
                        <p class="step-description">
                            See Roman numerals, chord functions, and theory explanations in real-time. 
                            Learn as you create with contextual tooltips and visual guides.
                        </p>
                    </div>
                </div>
                <div class="col-md-4">
                    <div class="step-card">
                        <div class="step-number">03</div>
                        <h3 class="step-title">Save & Share</h3>
                        <p class="step-description">
                            Export progressions to the web platform. Build your library, track progress, 
                            share with the community, and export to MIDI anytime.
                        </p>
                    </div>
                </div>
            </div>
        </div>
    </section>

    <!-- CTA Section with Wave Animation -->
    <section class="cta-section">
        <div id="wave-canvas-container"></div>
        <div class="container">
            <div class="cta-content text-center">
                <h2 class="cta-title">Ready to Elevate Your Music?</h2>
                <p class="cta-subtitle">
                    Join musicians, producers, and educators who are bridging the gap between 
                    creativity and theory with Chordal.
                </p>
                <asp:PlaceHolder ID="PlaceHolder1" runat="server">
                    <asp:HyperLink NavigateUrl="~/Register.aspx" runat="server" CssClass="btn btn-primary btn-lg btn-cta btn-pulse" Text="Start Learning Now" />
                </asp:PlaceHolder>
            </div>
        </div>
    </section>

    <!-- p5.js Sketch Scripts -->
    <script src='<%= ResolveUrl("Scripts/landing-animations.js") %>'></script>

</asp:Content>
