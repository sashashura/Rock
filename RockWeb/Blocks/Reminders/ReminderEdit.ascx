<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderEdit.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderEdit" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
        
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-bell"></i> 
                    Reminder for <asp:Literal ID="lEntity" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblTest" runat="server" LabelType="Info" Text="Label" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <div>
                    <Rock:DatePicker ID="rdpReminderDate" runat="server" Label="Reminder Date" Required="true" AllowPastDateSelection="false" />
                </div>

                <div>
                    <Rock:RockTextBox ID="rtbNote" runat="server" Label="Note" TextMode="MultiLine" />
                </div>

                <div>
                    <Rock:RockDropDownList ID="rddlReminderType" runat="server" Label="Reminder Type" Required="true" />
                </div>

                <div>
                    <Rock:PersonPicker ID="rppPerson" runat="server" Label="Send Reminder To" Required="true" EnableSelfSelection="true" />
                </div>

                <div>
                    <div class="col-md-6">
                        Repeat Every
                    </div>
                    <div class="col-md-6">
                        Number of Times to Repeat
                    </div>
                </div>
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>