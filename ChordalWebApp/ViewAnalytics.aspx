<%@ Page Title="View Analytics" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="ViewAnalytics.aspx.cs" Inherits="ChordalWebApp.ViewAnalytics" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    View Analytics - Chordal Admin
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="<%= ResolveUrl("~/Styles/admin-styles.css") %>" rel="stylesheet" />
    
    <!-- Chart.js for analytics -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js@4.4.0/dist/chart.umd.min.js"></script>
    
    <!-- PDF Export Libraries -->
    <script src="https://cdn.jsdelivr.net/npm/jspdf@2.5.1/dist/jspdf.umd.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/html2canvas@1.4.1/dist/html2canvas.min.js"></script>
    
    <style>
        .chart-container {
            position: relative;
            height: 350px;
            margin: var(--spacing-lg) 0;
            padding: var(--spacing-lg);
            background: var(--color-white);
            border-radius: var(--radius-lg);
        }
        
        .chart-card {
            background: var(--color-white);
            border: var(--border-width) solid var(--border-color);
            border-radius: var(--radius-lg);
            padding: var(--spacing-xl);
            margin-bottom: var(--spacing-lg);
        }
        
        .chart-title {
            font-family: var(--font-heading);
            font-size: var(--text-xl);
            font-weight: 700;
            color: var(--color-text-primary);
            margin-bottom: var(--spacing-lg);
            padding-bottom: var(--spacing-sm);
            border-bottom: 2px solid var(--border-color);
        }
        
        .charts-grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(500px, 1fr));
            gap: var(--spacing-lg);
            margin-bottom: var(--spacing-2xl);
        }
        
        .export-section {
            background: var(--color-white);
            border: var(--border-width) solid var(--border-color);
            border-radius: var(--radius-lg);
            padding: var(--spacing-lg);
            margin: var(--spacing-2xl) 0;
            display: flex;
            gap: var(--spacing-md);
            flex-wrap: wrap;
        }
        
        .analytics-table {
            width: 100%;
            border-collapse: collapse;
        }
        
        .analytics-table th {
            background: var(--admin-gray-50);
            padding: var(--spacing-md);
            text-align: left;
            font-weight: 600;
            border-bottom: 2px solid var(--border-color);
            color: var(--color-text-primary);
        }
        
        .analytics-table td {
            padding: var(--spacing-md);
            border-bottom: 1px solid var(--border-color);
        }
        
        .analytics-table tr:hover {
            background: var(--admin-gray-50);
        }
        
        @keyframes chartFadeIn {
            from {
                opacity: 0;
                transform: translateY(20px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }
        
        .chart-card {
            animation: chartFadeIn 0.6s ease forwards;
        }
        
        @media (max-width: 768px) {
            .charts-grid {
                grid-template-columns: 1fr;
            }
            
            .chart-container {
                height: 250px;
            }
        }
    </style>
</asp:Content>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="admin-page">
        <!-- Admin Header with Particles -->
        <div class="admin-header">
            <div id="admin-particles-canvas"></div>
            <div class="admin-header-content">
                <div class="admin-header-top">
                    <div>
                        <h1 class="admin-title">
                            <span class="admin-title-icon">System Analytics</span>
                        </h1>
                        <p class="admin-subtitle">Platform performance and user insights</p>
                    </div>
                    <div class="admin-header-actions">
                        <asp:HyperLink ID="hlBackToDashboard" runat="server" NavigateUrl="~/AdminDashboard.aspx" CssClass="btn-admin-header">
                            ← Dashboard
                        </asp:HyperLink>
                    </div>
                </div>
            </div>
        </div>

        <div class="admin-container">
            <!-- Time Range Filter -->
            <div class="admin-filters">
                <div class="filters-row">
                    <div class="filter-group">
                        <label class="filter-label">Time Range:</label>
                        <asp:DropDownList ID="ddlTimeRange" runat="server" CssClass="filter-select">
                            <asp:ListItem Value="Today">Today</asp:ListItem>
                            <asp:ListItem Value="Week">Last 7 Days</asp:ListItem>
                            <asp:ListItem Value="Month">Last 30 Days</asp:ListItem>
                            <asp:ListItem Value="All" Selected="True">All Time</asp:ListItem>
                        </asp:DropDownList>
                    </div>
                    <div class="filter-group">
                        <label class="filter-label">&nbsp;</label>
                        <asp:Button ID="btnApplyFilter" runat="server" Text="Apply Filter" 
                            OnClick="btnApplyFilter_Click" CssClass="btn-admin btn-admin-primary" />
                    </div>
                </div>
            </div>

            <!-- User Metrics -->
            <h2 class="section-title-admin">
                User Statistics
            </h2>
            <div class="admin-stats-grid">
                <div class="stat-card info">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">Users</div>
                    </div>
                    <div class="stat-label">Total Users</div>
                    <div class="stat-number">
                        <asp:Label ID="lblTotalUsers" runat="server">0</asp:Label>
                    </div>
                    <div class="stat-change positive">
                        <span>Growing</span>
                    </div>
                </div>
                
                <div class="stat-card success">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">New</div>
                    </div>
                    <div class="stat-label">New Users</div>
                    <div class="stat-number">
                        <asp:Label ID="lblNewUsers" runat="server">0</asp:Label>
                    </div>
                    <div class="stat-change positive">
                        <span>This period</span>
                    </div>
                </div>
                
                <div class="stat-card info">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">Active</div>
                    </div>
                    <div class="stat-label">Active Users</div>
                    <div class="stat-number">
                        <asp:Label ID="lblActiveUsers" runat="server">0</asp:Label>
                    </div>
                    <div class="stat-change">
                        <span>Last 30 days</span>
                    </div>
                </div>
                
                <div class="stat-card warning">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">Admin</div>
                    </div>
                    <div class="stat-label">Admin Users</div>
                    <div class="stat-number">
                        <asp:Label ID="lblAdminUsers" runat="server">0</asp:Label>
                    </div>
                    <div class="stat-change">
                        <span>System admins</span>
                    </div>
                </div>
            </div>

            <!-- Content Metrics -->
            <h2 class="section-title-admin">
                Content Statistics
            </h2>
            <div class="admin-stats-grid">
                <div class="stat-card info">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">Total</div>
                    </div>
                    <div class="stat-label">Total Progressions</div>
                    <div class="stat-number">
                        <asp:Label ID="lblTotalProgressions" runat="server">0</asp:Label>
                    </div>
                </div>
                
                <div class="stat-card success">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">Shared</div>
                    </div>
                    <div class="stat-label">Shared Progressions</div>
                    <div class="stat-number">
                        <asp:Label ID="lblSharedProgressions" runat="server">0</asp:Label>
                    </div>
                </div>
                
                <div class="stat-card info">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">Published</div>
                    </div>
                    <div class="stat-label">Published</div>
                    <div class="stat-number">
                        <asp:Label ID="lblPublishedProgressions" runat="server">0</asp:Label>
                    </div>
                </div>
                
                <div class="stat-card warning">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">Review</div>
                    </div>
                    <div class="stat-label">Under Review</div>
                    <div class="stat-number">
                        <asp:Label ID="lblUnderReview" runat="server">0</asp:Label>
                    </div>
                </div>
            </div>

            <!-- Community Engagement -->
            <h2 class="section-title-admin">
                Community Engagement
            </h2>
            <div class="admin-stats-grid">
                <div class="stat-card info">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">Views</div>
                    </div>
                    <div class="stat-label">Total Views</div>
                    <div class="stat-number">
                        <asp:Label ID="lblTotalViews" runat="server">0</asp:Label>
                    </div>
                </div>
                
                <div class="stat-card success">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">Likes</div>
                    </div>
                    <div class="stat-label">Total Likes</div>
                    <div class="stat-number">
                        <asp:Label ID="lblTotalLikes" runat="server">0</asp:Label>
                    </div>
                </div>
                
                <div class="stat-card info">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">Comments</div>
                    </div>
                    <div class="stat-label">Total Comments</div>
                    <div class="stat-number">
                        <asp:Label ID="lblTotalComments" runat="server">0</asp:Label>
                    </div>
                </div>
                
                <div class="stat-card warning">
                    <div class="stat-card-header">
                        <div class="stat-card-icon">New</div>
                    </div>
                    <div class="stat-label">New Comments</div>
                    <div class="stat-number">
                        <asp:Label ID="lblNewComments" runat="server">0</asp:Label>
                    </div>
                </div>
            </div>

            <!-- Charts Section -->
            <h2 class="section-title-admin">
                Trend Analysis
            </h2>
            
            <div class="charts-grid">
                <!-- User Growth Chart -->
                <div class="chart-card">
                    <h3 class="chart-title">User Growth Over Time</h3>
                    <div class="chart-container">
                        <canvas id="userGrowthChart"></canvas>
                    </div>
                </div>
                
                <!-- Content Activity Chart -->
                <div class="chart-card">
                    <h3 class="chart-title">Content Activity</h3>
                    <div class="chart-container">
                        <canvas id="contentActivityChart"></canvas>
                    </div>
                </div>
            </div>
            
            <div class="charts-grid">
                <!-- Engagement Metrics Chart -->
                <div class="chart-card">
                    <h3 class="chart-title">Engagement Metrics</h3>
                    <div class="chart-container">
                        <canvas id="engagementChart"></canvas>
                    </div>
                </div>
                
                <!-- Top Categories Chart -->
                <div class="chart-card">
                    <h3 class="chart-title">Content Distribution</h3>
                    <div class="chart-container">
                        <canvas id="distributionChart"></canvas>
                    </div>
                </div>
            </div>

            <!-- Top Content Table -->
            <div class="admin-content-card">
                <div class="admin-card-header">
                    <h3 class="admin-card-title">Top Shared Progressions</h3>
                </div>
                <div style="overflow-x: auto;">
                    <asp:GridView ID="gvTopContent" runat="server" 
                        CssClass="analytics-table" 
                        AutoGenerateColumns="False"
                        EmptyDataText="No data available.">
                        <Columns>
                            <asp:BoundField DataField="ShareTitle" HeaderText="Title" />
                            <asp:BoundField DataField="Author" HeaderText="Author" />
                            <asp:BoundField DataField="ViewCount" HeaderText="Views" />
                            <asp:BoundField DataField="LikeCount" HeaderText="Likes" />
                            <asp:BoundField DataField="CommentCount" HeaderText="Comments" />
                            <asp:BoundField DataField="ShareDate" HeaderText="Shared" DataFormatString="{0:MMM dd, yyyy}" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <!-- Top Contributors Table -->
            <div class="admin-content-card">
                <div class="admin-card-header">
                    <h3 class="admin-card-title">Top Contributors</h3>
                </div>
                <div style="overflow-x: auto;">
                    <asp:GridView ID="gvTopContributors" runat="server" 
                        CssClass="analytics-table" 
                        AutoGenerateColumns="False"
                        EmptyDataText="No data available.">
                        <Columns>
                            <asp:BoundField DataField="Username" HeaderText="Username" />
                            <asp:BoundField DataField="SharedCount" HeaderText="Shared Progressions" />
                            <asp:BoundField DataField="TotalViews" HeaderText="Total Views" />
                            <asp:BoundField DataField="TotalLikes" HeaderText="Total Likes" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>

            <!-- Export Section -->
            <div class="export-section">
                <h3 style="width: 100%; margin: 0 0 var(--spacing-md) 0; color: var(--color-text-primary);">
                    Export Options
                </h3>
                <asp:Button ID="btnExportCSV" runat="server" Text="Export CSV" 
                    OnClick="btnExportCSV_Click" CssClass="btn-admin btn-admin-success" />
                <button type="button" onclick="exportPDF()" class="btn-admin btn-admin-primary">
                    Export PDF Report
                </button>
                <button type="button" onclick="printReport()" class="btn-admin btn-admin-outline">
                    Print Report
                </button>
            </div>

            <!-- Status Message -->
            <asp:Label ID="lblMessage" runat="server" CssClass="alert" Visible="false" 
                style="display:block; margin-top:var(--spacing-lg); padding:var(--spacing-md); border-radius:var(--radius-md);"></asp:Label>
        </div>
    </div>

    <!-- Hidden fields to pass data from server to JavaScript -->
    <asp:HiddenField ID="hfUserGrowthData" runat="server" />
    <asp:HiddenField ID="hfContentActivityData" runat="server" />
    <asp:HiddenField ID="hfEngagementData" runat="server" />

    <!-- Chart.js Initialization Script -->
    <script>
        // Chart.js Global Configuration
        Chart.defaults.font.family = "'Mona Sans', sans-serif";
        Chart.defaults.color = '#16181d';

        // Color Palette
        const chartColors = {
            primary: '#dc3545',
            primaryLight: '#FB4466',
            teal: '#177364',
            tealLight: '#60afa3',
            success: '#28a745',
            warning: '#ffc107',
            info: '#17a2b8',
            gray: '#6c757d'
        };

        // Initialize charts when page loads
        document.addEventListener('DOMContentLoaded', function () {
            initUserGrowthChart();
            initContentActivityChart();
            initEngagementChart();
            initDistributionChart();
        });

        // User Growth Chart - Using real database data
        function initUserGrowthChart() {
            const ctx = document.getElementById('userGrowthChart');
            if (!ctx) return;

            // Get data from hidden field (populated by C# backend)
            const userGrowthJson = document.getElementById('<%= hfUserGrowthData.ClientID %>').value;
            let chartData;
            
            if (userGrowthJson) {
                chartData = JSON.parse(userGrowthJson);
            } else {
                // Fallback sample data
                chartData = {
                    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                    newUsers: [12, 19, 25, 32, 45, 58, 72, 89, 105, 128, 145, 167],
                    cumulativeUsers: [12, 31, 56, 88, 133, 191, 263, 352, 457, 585, 730, 897]
                };
            }
            
            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: chartData.labels,
                    datasets: [{
                        label: 'Cumulative Users',
                        data: chartData.cumulativeUsers,
                        borderColor: chartColors.primary,
                        backgroundColor: 'rgba(220, 53, 69, 0.1)',
                        tension: 0.4,
                        fill: true
                    }, {
                        label: 'New Users',
                        data: chartData.newUsers,
                        borderColor: chartColors.teal,
                        backgroundColor: 'rgba(23, 115, 100, 0.1)',
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'bottom'
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    },
                    animation: {
                        duration: 2000,
                        easing: 'easeOutQuart'
                    }
                }
            });
        }

        // Content Activity Chart - Using real database data
        function initContentActivityChart() {
            const ctx = document.getElementById('contentActivityChart');
            if (!ctx) return;
            
            // Get data from hidden field (populated by C# backend)
            const contentJson = document.getElementById('<%= hfContentActivityData.ClientID %>').value;
            let chartData;
            
            if (contentJson) {
                chartData = JSON.parse(contentJson);
            } else {
                // Fallback sample data
                chartData = {
                    labels: ['Week 1', 'Week 2', 'Week 3', 'Week 4'],
                    created: [45, 52, 61, 58],
                    shared: [32, 38, 45, 42]
                };
            }
            
            new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: chartData.labels,
                    datasets: [{
                        label: 'Progressions Created',
                        data: chartData.created,
                        backgroundColor: chartColors.primary
                    }, {
                        label: 'Progressions Shared',
                        data: chartData.shared,
                        backgroundColor: chartColors.teal
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'bottom'
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    },
                    animation: {
                        duration: 1500,
                        easing: 'easeOutBounce'
                    }
                }
            });
        }

        // Engagement Chart - Using real database data
        function initEngagementChart() {
            const ctx = document.getElementById('engagementChart');
            if (!ctx) return;
            
            // Get data from hidden field (populated by C# backend)
            const engagementJson = document.getElementById('<%= hfEngagementData.ClientID %>').value;
            let chartData;

            if (engagementJson) {
                chartData = JSON.parse(engagementJson);
            } else {
                // Fallback sample data
                chartData = {
                    labels: ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'],
                    views: [320, 450, 380, 510, 420, 290, 250],
                    likes: [85, 120, 95, 135, 110, 75, 65],
                    comments: [42, 58, 48, 67, 55, 38, 32]
                };
            }

            new Chart(ctx, {
                type: 'line',
                data: {
                    labels: chartData.labels,
                    datasets: [{
                        label: 'Views',
                        data: chartData.views,
                        borderColor: chartColors.info,
                        backgroundColor: 'rgba(23, 162, 184, 0.1)',
                        tension: 0.4,
                        fill: true
                    }, {
                        label: 'Likes',
                        data: chartData.likes,
                        borderColor: chartColors.success,
                        backgroundColor: 'rgba(40, 167, 69, 0.1)',
                        tension: 0.4,
                        fill: true
                    }, {
                        label: 'Comments',
                        data: chartData.comments,
                        borderColor: chartColors.warning,
                        backgroundColor: 'rgba(255, 193, 7, 0.1)',
                        tension: 0.4,
                        fill: true
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'bottom'
                        }
                    },
                    scales: {
                        y: {
                            beginAtZero: true
                        }
                    },
                    animation: {
                        duration: 2000,
                        easing: 'easeInOutQuad'
                    }
                }
            });
        }

        // Distribution Chart (Doughnut)
        function initDistributionChart() {
            const ctx = document.getElementById('distributionChart');
            if (!ctx) return;

            new Chart(ctx, {
                type: 'doughnut',
                data: {
                    labels: ['Public', 'Unlisted', 'Private', 'Under Review'],
                    datasets: [{
                        data: [65, 20, 10, 5],
                        backgroundColor: [
                            chartColors.success,
                            chartColors.info,
                            chartColors.gray,
                            chartColors.warning
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            display: true,
                            position: 'bottom'
                        }
                    },
                    animation: {
                        animateRotate: true,
                        animateScale: true,
                        duration: 2000,
                        easing: 'easeOutQuart'
                    }
                }
            });
        }

        // Export to PDF
        async function exportPDF() {
            const loadingMsg = document.createElement('div');
            loadingMsg.textContent = 'Generating PDF...';
            loadingMsg.style.cssText = 'position:fixed;top:50%;left:50%;transform:translate(-50%,-50%);background:white;padding:2rem;border-radius:8px;box-shadow:0 4px 12px rgba(0,0,0,0.15);z-index:10000;';
            document.body.appendChild(loadingMsg);

            try {
                const { jsPDF } = window.jspdf;
                const doc = new jsPDF('p', 'mm', 'a4');

                doc.setFontSize(20);
                doc.text('Chordal Analytics Report', 105, 15, { align: 'center' });

                doc.setFontSize(12);
                doc.text('Generated: ' + new Date().toLocaleDateString(), 105, 22, { align: 'center' });

                const element = document.querySelector('.admin-container');
                const canvas = await html2canvas(element, {
                    scale: 2,
                    logging: false
                });

                const imgData = canvas.toDataURL('image/png');
                const imgWidth = 190;
                const imgHeight = (canvas.height * imgWidth) / canvas.width;

                doc.addImage(imgData, 'PNG', 10, 30, imgWidth, imgHeight);

                doc.save('chordal-analytics-' + Date.now() + '.pdf');
            } catch (error) {
                alert('Error generating PDF: ' + error.message);
            } finally {
                document.body.removeChild(loadingMsg);
            }
        }

        // Print Report
        function printReport() {
            window.print();
        }
    </script>

    <!-- Admin Animation Scripts -->
    <script src="<%= ResolveUrl("~/Scripts/animations-admin.js") %>"></script>
</asp:Content>
