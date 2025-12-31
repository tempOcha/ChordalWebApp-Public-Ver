<%@ Page Title="" Language="C#" MasterPageFile="~/Chordal.Master" AutoEventWireup="true" CodeBehind="MyProgressionsList.aspx.cs" Inherits="ChordalWebApp.MyProgressionsList" %>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="progressions-container" style="margin-top: 20px;">
        <h2>My Chord Progressions</h2>
        <hr />

        <asp:PlaceHolder ID="phNoProgressions" runat="server" Visible="false">
            <p>You haven't uploaded any chord progressions yet. 
                <asp:HyperLink NavigateUrl="~/UploadProgression.aspx" runat="server">Upload your first one now!</asp:HyperLink>
            </p>
        </asp:PlaceHolder>

        <asp:GridView ID="gvProgressions" runat="server" AutoGenerateColumns="False"
            CssClass="table table-hover table-striped" Width="100%"
            DataKeyNames="ProgressionID"
            OnRowCommand="gvProgressions_RowCommand">
            <Columns>
                <asp:BoundField DataField="ProgressionTitle" HeaderText="Title" SortExpression="ProgressionTitle" >
                    <ItemStyle Width="30%" />
                </asp:BoundField>
                <asp:BoundField DataField="KeySignature" HeaderText="Key" SortExpression="KeySignature" >
                    <ItemStyle Width="15%" />
                </asp:BoundField>
                <asp:BoundField DataField="UploadDate" HeaderText="Uploaded On" SortExpression="UploadDate" DataFormatString="{0:yyyy-MM-dd HH:mm}" >
                    <ItemStyle Width="20%" />
                </asp:BoundField>
                 <asp:TemplateField HeaderText="Chords">
                    <ItemTemplate>
                        <asp:Label ID="lblChordCount" runat="server" Text='<%# Eval("ChordEventCount") + " chords" %>'></asp:Label>
                    </ItemTemplate>
                    <ItemStyle Width="15%" />
                </asp:TemplateField>
                <asp:TemplateField HeaderText="Actions">
                    <ItemTemplate>
                        <asp:LinkButton ID="lnkViewDetails" runat="server" Text="View Details"
                            CommandName="ViewDetails" CommandArgument='<%# Eval("ProgressionID") %>' CssClass="btn btn-info btn-sm" />
                        <%-- Temporary delete button I guess? Idk i think itll break --%>
                        <%-- <asp:LinkButton ID="lnkDelete" runat="server" Text="Delete"
                            CommandName="DeleteProgression" CommandArgument='<%# Eval("ProgressionID") %>' 
                            CssClass="btn btn-danger btn-sm" OnClientClick="return confirm('Are you sure you want to delete this progression?');" /> --%>
                    </ItemTemplate>
                    <ItemStyle Width="20%" HorizontalAlign="Center" />
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <p>No chord progressions found. 
                    <asp:HyperLink NavigateUrl="~/UploadProgression.aspx" runat="server">Upload one now!</asp:HyperLink>
                </p>
            </EmptyDataTemplate>
        </asp:GridView>
    </div>
</asp:Content>