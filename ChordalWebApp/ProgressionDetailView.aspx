<%@ Page Title="View Progression" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="ProgressionDetailView.aspx.cs" Inherits="ChordalWebApp.ProgressionDetailView" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    <asp:Literal ID="litPageTitle" runat="server">View Progression</asp:Literal> - Chordal
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Tone.js for audio synthesis -->
    <script src="https://cdn.jsdelivr.net/npm/tone@14.8.49/build/Tone.min.js"></script>
    
    <link rel="stylesheet" href="Styles/comm-styles.css">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Community Header with Wave Animation -->
    <div class="community-header-section progression-view-header">
        <canvas id="community-wave-canvas"></canvas>
        <div class="container">
            <div class="community-header-content">
                <h1 class="community-header-title">
                    <asp:Literal ID="litProgressionTitle" runat="server">Progression Title</asp:Literal>
                </h1>
                <div class="progression-header-meta">
                    <div class="author-section-header">
                        <div>
                            <div class="share-date-header">
                                Uploaded: <asp:Literal ID="litUploadDate" runat="server">Date</asp:Literal>
                            </div>
                            <div class="meta-item-header">
                                Key: <asp:Literal ID="litKeySignature" runat="server">Key</asp:Literal>
                            </div>
                        </div>
                    </div>
                    <div class="header-meta-items">
                        <asp:Panel ID="divTempo" runat="server" Visible="false" CssClass="meta-item-header">
                            🎼 <asp:Literal ID="litTempo" runat="server">120</asp:Literal> BPM
                        </asp:Panel>
                        <asp:Panel ID="divCategory" runat="server" Visible="false" CssClass="meta-item-header">
                            📁 <asp:Literal ID="litCategory" runat="server"></asp:Literal>
                        </asp:Panel>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <div class="container progression-view-container">
        <asp:Panel ID="pnlProgressionDetails" runat="server">
            <!-- Back Link -->
            <div style="margin-bottom: var(--spacing-lg);">
                <asp:HyperLink ID="lnkBackToList" NavigateUrl="~/MyProgressionsListEnhanced.aspx" runat="server" 
                              style="color: var(--color-teal); text-decoration: none; font-weight: 600;">
                    ← Back to My Progressions
                </asp:HyperLink>
            </div>
        
            <!-- MIDI Visualization Section -->
            <div class="midi-visualization-section">
                <div class="midi-viz-header">
                    <h2 class="section-title">Chord Progression Visualization</h2>
                    <div class="viz-controls-inline">
                        <button type="button" id="play-button" class="btn-viz-control btn-play" disabled>
                            <span class="btn-icon">▶</span> Play
                        </button>
                        <button type="button" id="stop-button" class="btn-viz-control btn-stop" disabled>
                            <span class="btn-icon">■</span> Stop
                        </button>
                    </div>
                </div>
                
                <!-- MIDI Display Canvas -->
                <div id="midi-visualization-container">
                    <canvas id="progression-canvas"></canvas>
                </div>
                
                <!-- Playback Controls Panel -->
                <div class="playback-controls-panel">
                    <div class="control-group">
                        <label class="control-label">
                            <span class="label-icon"></span>
                            Waveform
                        </label>
                        <select id="waveform-select" class="form-control-viz">
                            <option value="sine">Sine Wave</option>
                            <option value="square">Square Wave</option>
                            <option value="sawtooth">Sawtooth Wave</option>
                            <option value="triangle">Triangle Wave</option>
                        </select>
                    </div>

                    <div class="control-group">
                        <label class="control-label">
                            <span class="label-icon"></span>
                            Note Color
                        </label>
                        <input type="color" id="note-color-picker" class="color-picker-viz" 
                               value="#177364">
                    </div>
                    
                    <div class="control-group">
                        <label class="control-label">
                            <span class="label-icon"></span>
                            Bar Height: <span id="height-value">1</span>px
                        </label>
                        <div></div>
                        <input type="range" id="bar-height-slider" class="slider-viz" 
                               min="1" max="8" value="1">
                    </div>
                    
                    <div class="control-group">
                        <label class="checkbox-label">
                            <input type="checkbox" id="show-playhead" checked>
                            Show Playhead
                        </label>
                    </div>
                </div>
            </div>

            <!-- Chord Progression Cards Section -->
            <div class="chord-cards-section">
                <h2 class="section-title">Chord Sequence</h2>
                <div class="chord-sequence-grid">
                    <asp:Repeater ID="rptChordEvents" runat="server">
                        <ItemTemplate>
                            <div class="chord-card-modern">
                                <div class="chord-number"><%# Container.ItemIndex + 1 %></div>
                                <div class="chord-name-large"><%# Eval("ChordName") %></div>
                                <div class="chord-roman-modern"><%# Eval("RomanNumeral") %></div>
                                <div class="chord-function-modern"><%# Eval("ChordFunction") %></div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
                
                <asp:Label ID="lblNoChords" runat="server" Visible="false" style="text-align: center; color: var(--color-text-muted); padding: var(--spacing-xl);">
                    No chord events found for this progression.
                </asp:Label>
            </div>

            <!-- Action Buttons -->
            <div class="action-buttons-modern" style="margin-top: var(--spacing-xl); display: flex; gap: var(--spacing-md); justify-content: center;">
                <asp:Button ID="btnExportMIDI" runat="server" Text="Export MIDI" 
                           CssClass="btn-download-modern" OnClick="btnExportMIDI_Click" 
                           ToolTip="Download this progression as a MIDI file" />
                
                <asp:Button ID="btnShare" runat="server" Text="Share with Community" 
                           CssClass="btn-download-modern" OnClick="btnShare_Click" 
                           ToolTip="Share this progression with the Chordal community" />
            </div>
        </asp:Panel>
        
        <asp:Panel ID="pnlError" runat="server" Visible="false" CssClass="error-panel">
            <h2>Progression Not Found</h2>
            <p>The progression you're looking for doesn't exist or you don't have permission to view it.</p>
            <a href="MyProgressionsListEnhanced.aspx" class="btn btn-primary">Back to My Progressions</a>
        </asp:Panel>
        
        <asp:HiddenField ID="hdnProgressionJSON" runat="server" />
    </div>
    
    <!-- Debug Script -->
    <script type="text/javascript">
        window.chordalProgressionJSONFieldID = '<%= hdnProgressionJSON.ClientID %>';

        window.addEventListener('DOMContentLoaded', function () {
            const jsonField = document.getElementById('<%= hdnProgressionJSON.ClientID %>');
            if (jsonField && jsonField.value) {
                try {
                    const data = JSON.parse(jsonField.value);
                    console.log('✓ Progression data loaded:', data.length, 'chords');
                } catch (e) {
                    console.error('✗ JSON parse error:', e);
                }
            }
        });
    </script>
    
    <script src="Scripts/animations-comm.js"></script>
</asp:Content>
