<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalizationSegmentDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.PersonalizationSegmentDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfSegmentId" runat="server" />
            <div class="panel-heading">
                <h1 class="panel-title">

                    <i class="fa fa-star"></i>
                    <asp:Literal ID="lActionTitle" runat="server" Text="something" />


                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlStatus" runat="server" LabelType="Success" Text="Active/Inactive" />
                </div>
            </div>

            <div class="panel-body">
                <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <%-- Segment Name --%>
                <div class="row">
                    <div class="col-md-6">
                        <Rock:HiddenFieldWithClass ID="hfExistingSegmentKeyNames" runat="server" CssClass="js-existing-key-names" />
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Segment, Rock" PropertyName="Name" Required="true" onblur="populateSegmentKey()" />
                        <Rock:DataTextBox ID="tbSegmentKey" runat="server" SourceTypeName="Rock.Model.Segment, Rock" PropertyName="SegmentKey" Label="Key" Required="true" />
                    </div>

                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                    </div>
                </div>

                <%-- Person Filters --%>
                <asp:Panel ID="pnlPersonFilters" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <h1 class="panel-title">Person Filters</h1>
                    </div>
                    <div class="panel-body">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataViewItemPicker ID="dvpFilterDataView" runat="server" Label="Filter Data View" OnSelectItem="dvpFilterDataView_SelectItem" />
                                <Rock:NotificationBox ID="nbFilterDataViewWarning" runat="server" NotificationBoxType="Danger"
                                    Text="Segments only support data views that have been configured to persist. Please update the configuration of the selected dataview." />
                            </div>

                            <div class="col-md-6">
                                Adding a data view to the segment will exclude anonymous visitors.
                            </div>
                        </div>
                    </div>
                </asp:Panel>

                <span class="segment-and">--[AND]--</span>

                <%-- Session Filters --%>
                <asp:Panel ID="pnlSessionCountFilters" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">Session Filters</div>
                        <Rock:Toggle ID="tglSessionCountFiltersAllAny" runat="server" OnText="All" OffText="Any" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" />
                    </div>
                    <div class="panel-panel-body">
                        <Rock:Grid ID="gSessionCountFilters" runat="server" DisplayType="Light" RowItemText="Session Filter">
                            <Columns>
                                <Rock:RockLiteralField ID="lSessionCountFilterDescription" HeaderText="Description" OnDataBound="lSessionCountFilterDescription_DataBound" />
                                <Rock:EditField OnClick="gSessionCountFilters_EditClick" />
                                <Rock:DeleteField OnClick="gSessionCountFilters_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <span class="segment-and">--[AND]--</span>

                <%-- Page View Filters --%>
                <asp:Panel ID="pnlPageViewFilters" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">Page View Filters</div>
                        <Rock:Toggle ID="tglPageViewFiltersAllAny" runat="server" OnText="All" OffText="Any" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" />
                    </div>
                    <div class="panel-panel-body">
                        <Rock:Grid ID="gPageViewFilters" runat="server" DisplayType="Light" RowItemText="Page View Filter">
                            <Columns>
                                <Rock:RockLiteralField ID="lPageViewFilterDescription" HeaderText="Description" OnDataBound="lPageViewFilterDescription_DataBound" />
                                <Rock:EditField OnClick="gPageViewFilters_EditClick" />
                                <Rock:DeleteField OnClick="gPageViewFilters_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>

                </asp:Panel>

                <span class="segment-and">--[AND]--</span>

                <%-- Interaction Filters --%>
                <asp:Panel ID="pnlInteractionFilters" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">Interaction Filters</div>
                        <Rock:Toggle ID="tglInteractionFiltersAllAny" runat="server" OnText="All" OffText="Any" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" />
                    </div>
                    <div class="panel-body">
                    </div>
                </asp:Panel>

                <div class="actions">
                    <asp:LinkButton ID="btnTestSessionCountFilter" runat="server" Text="## Test Filter ##" CausesValidation="false" CssClass="btn btn-primary" OnClick="btnTestSessionCountFilter_Click" />

                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>

            <%-- Modal for Session Count Filter --%>
            <Rock:ModalDialog ID="mdSessionCountFilterConfiguration" runat="server" OnSaveClick="mdSessionCountFilterConfiguration_SaveClick" ValidationGroup="vgSessionCountFilterConfiguration">
                <Content>
                    <div class="panel-body">
                        <asp:HiddenField ID="hfSessionCountFilterGuid" runat="server" />

                        <asp:ValidationSummary ID="vsSessionCountFilterConfiguration" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgSessionCountFilterConfiguration" />

                        <div class="field-criteria">
                            <Rock:RockDropDownList ID="ddlSessionCountFilterComparisonType" CssClass="js-filter-compare" runat="server" ValidationGroup="vgSessionCountFilterConfiguration" />
                            <Rock:NumberBox ID="nbSessionCountFilterCompareValue" runat="server" Required="true" CssClass="js-filter-control" ValidationGroup="vgSessionCountFilterConfiguration" />
                        </div>

                        <span>sessions on the</span>
                        <Rock:RockListBox ID="lstSessionCountFilterWebSites" runat="server" Required="true" ValidationGroup="vgSessionCountFilterConfiguration" RequiredErrorMessage="Website is required." />
                        <span>website(s)</span>

                        <br />

                        <span>In the following date range</span>
                        <Rock:SlidingDateRangePicker ID="drpSessionCountFilterSlidingDateRange" runat="server" Label="" PreviewLocation="Right" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" ValidationGroup="vgSessionCountFilterConfiguration" />
                    </div>


                </Content>
            </Rock:ModalDialog>

            <%-- Modal for Page Views Filter --%>
            <Rock:ModalDialog ID="mdPageViewFilterConfiguration" runat="server" OnSaveClick="mdPageViewFilterConfiguration_SaveClick" ValidationGroup="vgPageViewFilterConfiguration">
                <Content>
                    <div class="panel-body">
                        <asp:HiddenField ID="hfPageViewFilterGuid" runat="server" />

                        <asp:ValidationSummary ID="vsPageViewFilterConfiguration" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgPageViewFilterConfiguration" />

                        <div class="field-criteria">
                            <Rock:RockDropDownList ID="ddlPageViewFilterComparisonType" CssClass="js-filter-compare" runat="server" ValidationGroup="vgPageViewFilterConfiguration" />
                            <Rock:NumberBox ID="nbPageViewFilterCompareValue" runat="server" Required="true" CssClass="js-filter-control" ValidationGroup="vgPageViewFilterConfiguration" />
                        </div>

                        <span>page views on the</span>
                        <Rock:RockListBox ID="lstPageViewFilterWebSites" runat="server" Required="true" ValidationGroup="vgPageViewFilterConfiguration" RequiredErrorMessage="Web Site is required." />
                        <span>website(s)</span>

                        <br />

                        <span>In the following date range</span>
                        <Rock:SlidingDateRangePicker ID="drpPageViewFilterSlidingDateRange" runat="server" Label="" PreviewLocation="Right" EnabledSlidingDateRangeTypes="Previous, Last, Current, DateRange" ValidationGroup="vgPageViewFilterConfiguration" ToolTip="<div class='js-slidingdaterange-info'></>" />

                        <br />

                        <span>optionally limited to the following pages</span>

                        <Rock:PagePicker ID="ppPageViewFilterPages" runat="server" AllowMultiSelect="true" ValidationGroup="vgPageViewFilterConfiguration"  Label="Page Picker Instead?"/>
                        <Rock:RockListBox ID="lstPageViewFilterPages" runat="server" Required="false" ValidationGroup="vgPageViewFilterConfiguration" Label="##TODO## Select Page Views using this?" />
                    </div>


                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

        <script>
            function populateSegmentKey() {
                // if the segment key hasn't been filled in yet, populate it with the segment name minus whitespace and special chars
                var $keyControl = $('#<%=tbSegmentKey.ClientID%>');
                var keyValue = $keyControl.val();

                var reservedKeyJson = $('#<%=hfExistingSegmentKeyNames.ClientID%>').val();
                var reservedKeyNames = eval('(' + reservedKeyJson + ')');

                if ($keyControl.length && (keyValue == '')) {

                    keyValue = $('#<%=tbName.ClientID%>').val().replace(/[^a-zA-Z0-9_.\-]/g, '');
                    var newKeyValue = keyValue;

                    var i = 1;
                    while ($.inArray(newKeyValue, reservedKeyNames) >= 0) {
                        newKeyValue = keyValue + i++;
                    }

                    $keyControl.val(newKeyValue);
                }
            }

        </script>

    </ContentTemplate>
</asp:UpdatePanel>
