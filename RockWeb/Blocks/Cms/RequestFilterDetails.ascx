<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RequestFilterDetails.ascx.cs" Inherits="RockWeb.Blocks.Cms.RequestFilterDetails" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">
            <asp:HiddenField ID="hfRequestFilterId" runat="server" />
            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-user-tag"></i>
                    <asp:Literal ID="lPanelTitle" runat="server" Text="" />

                </h1>
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
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
                        <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.RequestFilter, Rock" PropertyName="Name" onblur="populateSegmentKey()" />
                        <Rock:DataTextBox ID="tbSegmentKey" runat="server" SourceTypeName="Rock.Model.RequestFilter, Rock" PropertyName="SiteId" Label="Site" Help="Site - Optional site to limit the request filter to."/>
                    </div>

                    <div class="col-md-6">
                        <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                    </div>
                </div>

                <%-- Person Filters --%>
                <asp:Panel ID="pnlPersonFilters" runat="server" CssClass="panel panel-section" Visible="false">
                    <div class="panel-heading">
                        <h1 class="panel-title">Previous Activity</h1>
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
                
                <%-- Previous Activity --%>
                <asp:Panel ID="pnlPreviousActivity" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">Previous Activity</div>
                    </div>
                    <div class="panel-body">
                        <Rock:RockCheckBoxList ID="cblPreviousActivity" runat="server" Label="Vistor Type" RepeatDirection="Horizontal" />
                    </div>
                </asp:Panel>

                <span class="segment-and">--[AND]--</span>

                <%-- Device Types --%>
                <asp:Panel ID="pnlDeviceType" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">Device Types</div>
                    </div>
                    <div class="panel-body">
                        <Rock:RockCheckBoxList ID="cblDeviceTypes" runat="server" Label="Device Type" RepeatDirection="Horizontal" />
                    </div>
                </asp:Panel>

                <span class="segment-and">--[AND]--</span>

                <%-- Query String Filter --%>
                <asp:Panel ID="pnlQueryStringFilter" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title pull-left">Query String Filter</div>
                        <Rock:Toggle ID="tglQueryStringFiltersAllAny" runat="server" OnText="All" OffText="Any" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" CssClass="panel-title pull-right" />
                        <div class="clearfix"></div>
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gQueryStringFilter" runat="server" DisplayType="Light" RowItemText="Query String Filter">
                            <Columns>
                                <Rock:RockBoundField DataField="Key" HeaderText="Key" />
                                <Rock:RockBoundField DataField="ComparisonType" HeaderText="Match Type"  />
                                <Rock:RockBoundField DataField="ComparisonValue" HeaderText="Value"  />
                                <Rock:EditField OnClick="gQueryStringFilter_EditClick" />
                                <Rock:DeleteField OnClick="gQueryStringFilter_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <span class="segment-and">--[AND]--</span>

                <%-- Cookie --%>
                <asp:Panel ID="pnlCookie" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title pull-left">Cookie</div>
                        <Rock:Toggle ID="tglCookiesAllAny" runat="server" OnText="All" OffText="Any" ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" CssClass="panel-title pull-right" />
                        <div class="clearfix"></div>                        
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gCookie" runat="server" DisplayType="Light" RowItemText="Cookie">
                            <Columns>
                                <Rock:RockBoundField DataField="Key" HeaderText="Key" />
                                <Rock:RockBoundField DataField="ComparisonType" HeaderText="Match Type"  />
                                <Rock:RockBoundField DataField="ComparisonValue" HeaderText="Value"  />
                                <Rock:EditField OnClick="gCookie_EditClick" />
                                <Rock:DeleteField OnClick="gCookie_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <span class="segment-and">--[AND]--</span>

                <%-- Browser --%>
                <asp:Panel ID="pnlBrowser" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">Browser</div>
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gBrowser" runat="server" DisplayType="Light" RowItemText="Browser">
                            <Columns>
                                <Rock:RockBoundField DataField="BrowserFamily" HeaderText="Browser Family" />
                                <Rock:RockBoundField DataField="VersionComparisonType" HeaderText="Match Type"  />
                                <Rock:RockBoundField DataField="MajorVersion" HeaderText="Major Version"  />
                                <Rock:EditField OnClick="gBrowser_EditClick" />
                                <Rock:DeleteField OnClick="gBrowser_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <span class="segment-and">--[AND]--</span>

                <%-- IP Addresses --%>
                <asp:Panel ID="pnlIpAddress" runat="server" CssClass="panel panel-section">
                    <div class="panel-heading">
                        <div class="panel-title">IP Addresses</div>
                    </div>
                    <div class="panel-body">
                        <Rock:Grid ID="gIPAddress" runat="server" DisplayType="Light" RowItemText="IP Address">
                            <Columns>
                                <Rock:RockBoundField DataField="BeginningIPAddress" HeaderText="Beginning Address" />
                                <Rock:RockBoundField DataField="EndingIPAddress" HeaderText="Ending Address" />
                                <Rock:RockBoundField DataField="MatchType" HeaderText="Match Type"  />
                                <Rock:EditField OnClick="gIpAddress_EditClick" />
                                <Rock:DeleteField OnClick="gIpAddress_DeleteClick" />
                            </Columns>
                        </Rock:Grid>
                    </div>
                </asp:Panel>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
            </div>

            <%-- Modal for Query String Filter --%>
            <Rock:ModalDialog ID="mdQueryStringFilter" runat="server" OnSaveClick="mdQueryStringFilter_SaveClick" ValidationGroup="vgQueryStringFilter">
                <Content>
                    <div class="panel-body">
                        <asp:HiddenField ID="hfQueryStringFilterGuid" runat="server" />

                        <asp:ValidationSummary ID="vsQueryFilterString" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgQueryFilterString" />

                        <div class="container">
                            <div class="row">
                                <span class="col-sm-2">Where the parameter </span>
                                <Rock:RockTextBox ID="tbQueryStringFilterParameter" runat="server" CssClass="col-sm-4"/>
                                <Rock:RockDropDownList ID="ddlQueryStringFilterMatchOptions" runat="server" CssClass="col-sm-2" />
                            </div>
                        </div>


                        <span>the value</span>
                        
                        <Rock:RockTextBox ID="tbQueryStringFilterValue" runat="server" />

                    </div>


                </Content>
            </Rock:ModalDialog>

            <%-- Modal for Cookie --%>
            <Rock:ModalDialog ID="mdCookie" runat="server" OnSaveClick="mdCookie_SaveClick" ValidationGroup="vgCookie">
                <Content>
                    <div class="panel-body">
                        <asp:HiddenField ID="hfCookieFilterGuid" runat="server" />

                        <asp:ValidationSummary ID="vsCookie" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgCookie" />

                        <div class="container">
                            <div class="row">
                                <span class="col-sm-2">Where the parameter </span>
                                <Rock:RockTextBox ID="tbCookieParameter" runat="server" CssClass="col-sm-4"/>
                                <Rock:RockDropDownList ID="ddlCookieMatchOptions" runat="server" CssClass="col-sm-2" />
                            </div>
                        </div>


                        <span>the value</span>
                        
                        <Rock:RockTextBox ID="tbCookieValue" runat="server" />

                    </div>


                </Content>
            </Rock:ModalDialog>

            <%-- Modal  for Browser Filter --%>
            <Rock:ModalDialog ID="mdBrowser" runat="server" OnSaveClick="mdBrowser_SaveClick" ValidationGroup="vgBrowser">
                <Content>
                    <div class="panel-body">
                        <asp:HiddenField ID="hfBrowserFilterGuid" runat="server" />

                        <asp:ValidationSummary ID="vsBrowser" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgBrowser" />

                        <div class="container">
                            <div class="row">
                                <span class="col-sm-2">Where </span>
                                <Rock:RockDropDownList ID="ddlBrowserFamily" runat="server" CssClass="col-sm-2" />
                                <span class="col-sm-2"> version is </span>
                                <Rock:RockDropDownList ID="ddlBrowserMatchOptions" runat="server" CssClass="col-sm-2" />
                                <Rock:RockTextBox ID="tbBrowserVersion" runat="server" CssClass="col-sm-4"/>
                            </div>
                        </div>
                    </div>


                </Content>
            </Rock:ModalDialog>

            <%-- Modal for IP Address Filter --%>
            <Rock:ModalDialog ID="mdIPAddress" runat="server" OnSaveClick="mdIpAddress_SaveClick" ValidationGroup="vgIPAddress">
                <Content>
                    <div class="panel-body">
                        <asp:HiddenField ID="hfIPAddressFilterGuid" runat="server" />

                        <asp:ValidationSummary ID="vsIPAddress" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgIPAddress" />

                        <div class="container">
                            <div class="row">
                                <span class="col-sm-2">Where the client IP is </span>
                                <Rock:Toggle ID="tglIPAddressRange" runat="server" OnText="Not in Range" OffText="In Range"
                                    ActiveButtonCssClass="btn-info" ButtonSizeCssClass="btn-xs" />
                                <Rock:RockTextBox ID="tbIPAddressStartRange" runat="server" CssClass="col-sm-4"/>
                                <Rock:RockTextBox ID="tbIPAddressEndRange" runat="server" CssClass="col-sm-4"/>
                            </div>
                        </div>
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
