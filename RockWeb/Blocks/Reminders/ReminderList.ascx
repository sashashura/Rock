<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderList.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-star"></i> 
                    Blank List Block
                </h1>
            </div>
            <div class="panel-body">

                <asp:Repeater ID="rptReminders" runat="server" />

            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
