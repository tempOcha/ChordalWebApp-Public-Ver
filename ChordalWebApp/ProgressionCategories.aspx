<%@ Page Title="Manage Categories - Chordal" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="ProgressionCategories.aspx.cs" Inherits="ChordalWebApp.ProgressionCategories" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Manage Categories - Chordal
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="HeadContent" runat="server">
    <link href="Styles/categories-styles.css" rel="stylesheet" />
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
    <div class="categories-page-container">
        <!-- Page Header -->
        <div class="categories-page-header">
            <h1 class="categories-page-title">Manage Progression Categories</h1>
            <p class="categories-page-subtitle">Organize your chord progressions by creating custom categories with unique colors and descriptions</p>
        </div>

        <!-- Status Message -->
        <asp:Label ID="lblStatus" runat="server" Text="" CssClass="status-message"></asp:Label>

        <!-- Add/Edit Category Form -->
        <div class="category-form-section">
            <h2 class="category-form-title">
                <asp:Literal ID="litFormTitle" runat="server" Text="Create New Category"></asp:Literal>
            </h2>
            <asp:HiddenField ID="hfEditCategoryId" runat="server" Value="" />

            <!-- Category Name -->
            <div class="form-group">
                <label for="txtCategoryName">Category Name *</label>
                <asp:TextBox ID="txtCategoryName" runat="server" CssClass="form-control" 
                    placeholder="e.g., Jazz Standards, Rock Ballads, Lo-fi Beats" 
                    MaxLength="50"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvCategoryName" runat="server" 
                    ControlToValidate="txtCategoryName" 
                    ErrorMessage="Category name is required" 
                    CssClass="text-danger" 
                    Display="Dynamic">
                </asp:RequiredFieldValidator>
            </div>

            <!-- Category Description -->
            <div class="form-group">
                <label for="txtCategoryDescription">Description (Optional)</label>
                <asp:TextBox ID="txtCategoryDescription" runat="server" CssClass="form-control" 
                    TextMode="MultiLine" Rows="3" 
                    placeholder="Describe the types of progressions in this category..." 
                    MaxLength="200"></asp:TextBox>
            </div>

            <!-- Enhanced Color Picker -->
            <div class="form-group">
                <label>Category Color *</label>
                <div class="color-picker-section">
                    <asp:HiddenField ID="hfSelectedColor" runat="server" Value="#177364" />
                    
                    <!-- Color Preview -->
                    <div class="color-picker-header">
                        <div class="color-picker-title">Select a color for your category</div>
                        <div class="color-preview-current">
                            <div class="color-preview-swatch"></div>
                            <span class="color-preview-hex">#177364</span>
                        </div>
                    </div>

                    <!-- Preset Colors -->
                    <label class="color-presets-label">Preset Colors</label>
                    <div class="color-picker-presets">
                        <div class="color-option" style="background-color: #177364;"></div>
                        <div class="color-option" style="background-color: #FB4466;"></div>
                        <div class="color-option" style="background-color: #60AFA3;"></div>
                        <div class="color-option" style="background-color: #FF6B6B;"></div>
                        <div class="color-option" style="background-color: #4ECDC4;"></div>
                        <div class="color-option" style="background-color: #45B7D1;"></div>
                        <div class="color-option" style="background-color: #96CEB4;"></div>
                        <div class="color-option" style="background-color: #FFEAA7;"></div>
                        <div class="color-option" style="background-color: #DFE6E9;"></div>
                        <div class="color-option" style="background-color: #74B9FF;"></div>
                        <div class="color-option" style="background-color: #A29BFE;"></div>
                        <div class="color-option" style="background-color: #FD79A8;"></div>
                        <div class="color-option" style="background-color: #FDCB6E;"></div>
                        <div class="color-option" style="background-color: #6C5CE7;"></div>
                        <div class="color-option" style="background-color: #00B894;"></div>
                        <div class="color-option" style="background-color: #E17055;"></div>
                    </div>

                    <!-- Custom Color Input -->
                    <div class="color-custom-input-wrapper">
                        <label class="color-custom-label">Custom Color:</label>
                        <input type="color" class="color-input-native" value="#177364" />
                        <input type="text" class="color-input-text" placeholder="#177364" maxlength="7" />
                    </div>
                </div>
            </div>

            <!-- Form Actions -->
            <div class="form-actions">
                <asp:Button ID="btnSaveCategory" runat="server" Text="Create Category" 
                    CssClass="btn btn-primary" OnClick="btnSaveCategory_Click" />
                <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" 
                    CssClass="btn btn-secondary" 
                    OnClick="btnCancelEdit_Click" 
                    CausesValidation="false" 
                    Visible="false" />
            </div>
        </div>

        <!-- Existing Categories List -->
        <div class="categories-list-section">
            <div class="categories-list-header">
                <h2 class="categories-list-title">Your Categories</h2>
            </div>

            <!-- No Categories State -->
            <asp:PlaceHolder ID="phNoCategories" runat="server" Visible="false">
                <div class="categories-empty-state">
                    <div class="categories-empty-icon">📁</div>
                    <h3>No categories yet</h3>
                    <p>Create your first category above to start organizing your progressions!</p>
                </div>
            </asp:PlaceHolder>

            <!-- Categories Repeater -->
            <asp:Repeater ID="rptCategories" runat="server" OnItemCommand="rptCategories_ItemCommand">
                <ItemTemplate>
                    <div class="category-card">
                        <div class="category-card-content">
                            <div class="category-color-badge" style="background-color: <%# Eval("Color") %>;"></div>
                            <div class="category-info">
                                <div class="category-name"><%# Server.HtmlEncode(Eval("CategoryName").ToString()) %></div>
                                <div class="category-description">
                                    <%# string.IsNullOrEmpty(Eval("Description").ToString()) ? 
                                        "<em>No description</em>" : 
                                        Server.HtmlEncode(Eval("Description").ToString()) %>
                                </div>
                                <div class="category-count">
                                    <%# Eval("ProgressionCount") %> progression(s)
                                </div>
                            </div>
                        </div>
                        <div class="category-actions">
                            <asp:LinkButton ID="btnEdit" runat="server" 
                                CommandName="EditCategory" 
                                CommandArgument='<%# Eval("CategoryID") %>'
                                CssClass="btn btn-sm btn-edit"
                                CausesValidation="false">
                                ✏️ Edit
                            </asp:LinkButton>
                            <asp:LinkButton ID="btnDelete" runat="server" 
                                CommandName="DeleteCategory" 
                                CommandArgument='<%# Eval("CategoryID") %>'
                                CssClass="btn btn-sm btn-delete"
                                CausesValidation="false"
                                OnClientClick="return confirm('Are you sure you want to delete this category? Progressions will not be deleted, only uncategorized.');">
                                🗑️ Delete
                            </asp:LinkButton>
                        </div>
                    </div>
                </ItemTemplate>
            </asp:Repeater>
        </div>
    </div>

    <!-- Scripts -->
    <script src="Scripts/animations-categories.js"></script>
    
    <script type="text/javascript">
        // Handle color selection after page load or postback (for edit mode)
        window.addEventListener('load', function () {
            // Wait for animations-categories.js to initialize
            setTimeout(function () {
                var hiddenColorField = document.querySelector('[id*="hfSelectedColor"]');
                if (hiddenColorField && window.selectColorGlobal) {
                    var currentColor = hiddenColorField.value || '#177364';
                    console.log('Page load - setting color to:', currentColor);
                    window.selectColorGlobal(currentColor);
                }
            }, 200);
        });
    </script>
</asp:Content>
