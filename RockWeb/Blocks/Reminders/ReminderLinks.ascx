<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReminderLinks.ascx.cs" Inherits="RockWeb.Blocks.Reminders.ReminderLinks" %>
<%@ Import Namespace="Rock" %>

<script type="text/javascript">
    function unregisterReminderEvents() {
        $(document).off(".reminderLinks");
    }
    function registerReminderEvents() {
        var remindersButton = $('.js-rock-reminders');
        var remindersPanel = $('.js-reminders-popover');
        $(document).off("mouseup.reminderLinks").on("mouseup.reminderLinks", function (e) {
            var isRemindersButton = $(".js-rock-reminders").is(e.target) || $(".js-rock-reminders").has(e.target).length != 0;
            // 'js-rock-reminders' has it's own handler, so ignore if this is from js-rock-reminders
            if (isRemindersButton) {
                return;
            }

            // if the target of the click isn't the bookmarkPanel or a descendant of the bookmarkPanel
            var remindersPanel = $('.js-reminders-popover');
            if (!remindersPanel.is(e.target) && remindersPanel.has(e.target).length === 0) {
                hideReminderLinks();
            }
        });

        $(window).on("resize.reminderLinks", function (e) { positionReminderLinks(remindersButton, remindersPanel) });
    }

    function showReminderLinks($button, $panel) {
        registerReminderEvents();
        if (typeof $button !== "undefined" && $panel !== null && $panel.hasClass('d-none')) {
            positionReminderLinks($button, $panel);

            var contextEntityTypeId = $('#<%= hfContextEntityTypeId.ClientID %>').val();
            if (contextEntityTypeId != 0) {
                // use ajax to check if there are active reminder types for this entity type.
                var restUrl = Rock.settings.get('baseUrl') + 'api/ReminderTypes/ReminderTypesExistForEntityType';
                restUrl += '?entityTypeId=' + contextEntityTypeId;
                $.ajax({
                    url: restUrl,
                    dataType: 'json',
                    success: function (data, status, xhr) {
                        if (data) {
                            $('.js-add-reminder').removeClass('d-none');
                        } else {
                            $('.js-add-reminder').addClass('d-none');
                        }
                    },
                    error: function (xhr, status, error) {
                        console.log('ReminderTypesExistForEntityType status: ' + status + ' [' + error + ']: ' + xhr.reponseText);
                    }
                });
            }

            $panel.removeClass('d-none');
        } else {
            this.hideReminderLinks();
        }
    }

    function hideReminderLinks() {
        $('.js-reminders-popover').addClass('d-none');
        unregisterReminderEvents();
    }

    function positionReminderLinks($button, $panel) {
        var bottom = ($button.position().top + $button.outerHeight(true));
        var buttonRight = ($button.offset().left + $button.outerWidth());
        var buttonCenter = ($button.offset().left + ($button.outerWidth() / 2));

        var left = (buttonCenter - ($panel.outerWidth() / 2));
        var leftMax = $(window).width() - $panel.outerWidth();
        left = Math.max(0, Math.min(left, leftMax));

        $panel.css('top', bottom).css('left', left);
    }

    Sys.Application.add_load(function () {
        var remindersButton = $('.js-rock-reminders');
        var remindersPanel = $('.js-reminders-popover');

        $(remindersButton).off("click").on("click", function (e) {
            e.preventDefault();
            showReminderLinks(remindersButton, remindersPanel);
        });
    });
</script>

<asp:HiddenField ID="hfContextEntityTypeId" runat="server" Value="0" />

<asp:LinkButton runat="server" ID="lbReminders" Visible="false" CssClass="rock-bookmark js-rock-reminders"
    href="#" ><i class="fa fa-bell"></i></asp:LinkButton>

<asp:UpdatePanel ID="upnlReminders" runat="server" class="popover rock-popover styled-scroll js-reminders-popover position-fixed d-none" role="tooltip">
    <ContentTemplate>
        <asp:Panel ID="pnlReminders" runat="server" CssClass="rock-popover js-reminders-container">
            <div class="popover-panel">
                <div class="popover-content">
                    <asp:LinkButton runat="server" ID="lbViewReminders" href="/reminders" >View Reminders</asp:LinkButton>
                    <asp:LinkButton runat="server" ID="lbAddReminder" CssClass="js-add-reminder d-none" OnClick="lbAddReminder_Click">Add Reminder</asp:LinkButton>
                </div>
            </div>
        </asp:Panel>
    </ContentTemplate>

</asp:UpdatePanel>
