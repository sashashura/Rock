// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Personalization;
using Rock.Personalization.SegmentFilters;
using Rock.Web.Cache;

using static Rock.Personalization.BrowserRequestFilter;
using static Rock.Personalization.DeviceTypeRequestFilter;
using static Rock.Personalization.PreviousActivityRequestFilter;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Request Filters" )]
    [Category( "Cms" )]
    [Description( "Block that lists the existing request details" )]

    #region Block Attributes

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "0CE221F6-EECE-46F9-A703-FCD09DEBC653" )]
    public partial class RequestFilterDetails : Rock.Web.UI.RockBlock
    {

        #region Attribute Keys

        private static class AttributeKey
        {
            //
        }

        #endregion Attribute Keys

        #region ViewState Keys

        private static class ViewStateKey
        {
            public const string AdditionalFilterConfigurationJson = "AdditionalFilterConfigurationJson";
        }

        #endregion ViewState Keys

        #region PageParameter Keys

        private static class PageParameterKey
        {
            public const string RequestFilterId = "RequestFilterId";
        }

        #endregion PageParameter Keys

        private Rock.Personalization.PersonalizationRequestFilterConfiguration AdditionalFilterConfiguration { get; set; }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            dvpFilterDataView.EntityTypeId = EntityTypeCache.GetId( Rock.SystemGuid.EntityType.PERSON.AsGuid() );

            // This event gets fired after block settings are updated. It's nice to repaint the screen if these settings would alter it.
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            gIPAddress.Actions.ShowAdd = true;
            gIPAddress.Actions.AddClick += gIpAddress_AddClick;

            gBrowser.Actions.ShowAdd = true;
            gBrowser.Actions.AddClick += gBrowser_AddClick;

            gCookie.Actions.ShowAdd = true;
            gCookie.Actions.AddClick += gCookie_AddClick;

            gQueryStringFilter.Actions.ShowAdd = true;
            gQueryStringFilter.Actions.AddClick += gQueryStringFilter_AddClick;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.RequestFilterId ).AsInteger() );
            }
        }

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var additionalFilterConfigurationJson = this.ViewState[ViewStateKey.AdditionalFilterConfigurationJson] as string;

            this.AdditionalFilterConfiguration = additionalFilterConfigurationJson.FromJsonOrNull<Rock.Personalization.PersonalizationRequestFilterConfiguration>() ?? new Rock.Personalization.PersonalizationRequestFilterConfiguration();
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>Returns the user control's current view state. If there is no view state associated with the control, it returns <see langword="null" />.</returns>
        protected override object SaveViewState()
        {
            this.ViewState[ViewStateKey.AdditionalFilterConfigurationJson] = this.AdditionalFilterConfiguration?.ToJson();
            return base.SaveViewState();
        }

        #endregion Base Control Methods

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="requestFilterId">The segment identifier.</param>
        public void ShowDetail( int requestFilterId )
        {
            var rockContext = new RockContext();

            var requestFilterService = new RequestFilterService( rockContext );
            RequestFilter requestFilter = null;

            if ( requestFilterId > 0 )
            {
                requestFilter = requestFilterService.Get( requestFilterId );
            }

            if ( requestFilter == null )
            {
                requestFilter = new RequestFilter();
            }

            if ( requestFilter.Id == 0 )
            {
                lPanelTitle.Text = ActionTitle.Add( RequestFilter.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lPanelTitle.Text = requestFilter.Name;
            }

            /* Name, etc */
            hfRequestFilterId.Value = requestFilter.Id.ToString();
            tbName.Text = requestFilter.Name;
            //tbSegmentKey.Text = requestFilter.SiteId;
            hlInactive.Visible = !requestFilter.IsActive;
            cbIsActive.Checked = requestFilter.IsActive;


            this.AdditionalFilterConfiguration = requestFilter.FilterConfiguration ?? new Rock.Personalization.PersonalizationRequestFilterConfiguration();

            gIPAddress.DataBind();
            gBrowser.DataBind();
            gCookie.DataBind();
            gQueryStringFilter.DataBind();


            cblDeviceTypes.BindToEnum<DeviceType>();
            cblPreviousActivity.BindToEnum<PreviousActivityType>();
        }

        #endregion Methods

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            NavigateToCurrentPageReference();
        }

        /// <summary>
        /// Handles the SelectItem event of the dvpFilterDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dvpFilterDataView_SelectItem( object sender, EventArgs e )
        {
            var selectedDataViewId = dvpFilterDataView.SelectedValueAsId();
            ShowDataViewWarningIfInvalid( selectedDataViewId );
        }

        /// <summary>
        /// Shows the data view warning if invalid.
        /// </summary>
        /// <param name="selectedDataViewId">The selected data view identifier.</param>
        private void ShowDataViewWarningIfInvalid( int? selectedDataViewId )
        {
            nbFilterDataViewWarning.Visible = false;
            DataView selectedDataView;
            var rockContext = new RockContext();
            if ( selectedDataViewId != null )
            {
                selectedDataView = new DataViewService( rockContext ).Get( selectedDataViewId.Value );
                if ( selectedDataView == null )
                {
                    return;
                }

                if ( !selectedDataView.IsPersisted() )
                {
                    nbFilterDataViewWarning.Visible = true;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();

            var filterDataViewId = dvpFilterDataView.SelectedValueAsId();
            ShowDataViewWarningIfInvalid( filterDataViewId );
            if ( nbFilterDataViewWarning.Visible )
            {
                return;
            }

            var requestFilterService = new RequestFilterService( rockContext );
            RequestFilter requestFilter;

            var requestFilterId = hfRequestFilterId.Value.AsInteger();

            if ( requestFilterId == 0 )
            {
                requestFilter = new RequestFilter();
                requestFilter.Id = requestFilterId;
                requestFilterService.Add( requestFilter );
            }
            else
            {
                requestFilter = requestFilterService.Get( requestFilterId );
            }

            if ( requestFilter == null )
            {
                return;
            }

            requestFilter.Name = tbName.Text;
            requestFilter.IsActive = cbIsActive.Checked;

            AdditionalFilterConfiguration.PreviousActivityRequestFilter.PreviousActivityTypes = cblPreviousActivity.SelectedValues
                .Select( v => v.ConvertToEnum<PreviousActivityType>() )
                .ToArray();

            AdditionalFilterConfiguration.DeviceTypeRequestFilter.DeviceTypes = cblDeviceTypes.SelectedValues
                .Select( v => v.ConvertToEnum<DeviceType>() )
                .ToArray();

            requestFilter.FilterConfiguration = AdditionalFilterConfiguration;

            rockContext.SaveChanges();
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        #endregion

        #region Query String Filter
        private void gQueryStringFilter_AddClick( object sender, EventArgs e )
        {
            ShowQueryStringFilterDialog( null );
        }
        /// <summary>
        /// The Edit Click event of the gQueryStringFilter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void gQueryStringFilter_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queryStringFilterGuid = ( Guid ) e.RowKeyValue;
            var queryStringFilter = AdditionalFilterConfiguration.QueryStringRequestFilters
                .Where( a => a.Guid == queryStringFilterGuid )
                .FirstOrDefault();

            ShowQueryStringFilterDialog( queryStringFilter );
        }

        /// <summary>
        /// Shows the query filter string dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The interaction filter segment filter.</param>
        private void ShowQueryStringFilterDialog( QueryStringRequestFilter queryStringRequestFilter )
        {
            if ( queryStringRequestFilter == null )
            {
                queryStringRequestFilter = new QueryStringRequestFilter
                {
                    Guid = Guid.NewGuid()
                };
                mdQueryStringFilter.Title = "Add Query String Filter";
            }
            else
            {
                mdQueryStringFilter.Title = "Edit Query String Filter";
            }

            hfQueryStringFilterGuid.Value = queryStringRequestFilter.Guid.ToString();

            ComparisonType[] ignoreTypes = new ComparisonType[] { ComparisonType.GreaterThan,
                ComparisonType.GreaterThanOrEqualTo,
                ComparisonType.LessThan,
                ComparisonType.LessThanOrEqualTo,
                ComparisonType.Between,
                ComparisonType.RegularExpression
            };

            ddlQueryStringFilterMatchOptions.BindToEnum( ignoreTypes: ignoreTypes );

            // populate the modal
            tbQueryStringFilterParameter.Text = queryStringRequestFilter.Key;
            ddlQueryStringFilterMatchOptions.SetValue( queryStringRequestFilter.ComparisonType.ConvertToInt() );
            tbQueryStringFilterValue.Text = queryStringRequestFilter.ComparisonValue;

            mdQueryStringFilter.Show();
        }

        protected void mdQueryStringFilter_SaveClick( object sender, EventArgs e )
        {
            var queryStringFilterGuid = hfQueryStringFilterGuid.Value.AsGuid();

            var queryStringFilter = this.AdditionalFilterConfiguration.QueryStringRequestFilters
                .Where( a => a.Guid == queryStringFilterGuid )
                .FirstOrDefault();

            if ( queryStringFilter == null )
            {
                queryStringFilter = new QueryStringRequestFilter
                {
                    Guid = hfQueryStringFilterGuid.Value.AsGuid()
                };
                this.AdditionalFilterConfiguration.QueryStringRequestFilters.Add( queryStringFilter );
            }

            queryStringFilter.Key = tbQueryStringFilterParameter.Text;
            queryStringFilter.ComparisonType = ddlQueryStringFilterMatchOptions.SelectedValue.ConvertToEnum<ComparisonType>();
            queryStringFilter.ComparisonValue = tbQueryStringFilterValue.Text;

            mdQueryStringFilter.Hide();
            BindQueryStringFilterToGrid();
        }

        private void BindQueryStringFilterToGrid()
        {
            var queryStringFilter = this.AdditionalFilterConfiguration.QueryStringRequestFilters;
            gQueryStringFilter.DataSource = queryStringFilter.OrderBy( q => q.Key );
        }

        #endregion Query String Filter

        #region Cookie Filter
        private void gCookie_AddClick( object sender, EventArgs e )
        {
            ShowCookieDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gCookie control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gCookie_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var cookieGuid = ( Guid ) e.RowKeyValue;
            var cookie = this.AdditionalFilterConfiguration.CookieRequestFilters
                .Where( a => a.Guid == cookieGuid )
                .FirstOrDefault();

            ShowCookieDialog( cookie );
        }

        /// <summary>
        /// Shows the Cookie Filter dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The interaction filter segment filter.</param>
        private void ShowCookieDialog( Rock.Personalization.CookieRequestFilter cookieRequestFilter )
        {
            if ( cookieRequestFilter == null )
            {
                cookieRequestFilter = new CookieRequestFilter();
                cookieRequestFilter.Guid = Guid.NewGuid();
                mdCookie.Title = "Add Cookie Filter";
            }
            else
            {
                mdCookie.Title = "Edit Cookie Filter";
            }

            ComparisonType[] ignoreTypes = new ComparisonType[] { ComparisonType.GreaterThan,
                ComparisonType.GreaterThanOrEqualTo,
                ComparisonType.LessThan,
                ComparisonType.LessThanOrEqualTo,
                ComparisonType.Between,
                ComparisonType.RegularExpression
            };

            ddlCookieMatchOptions.BindToEnum<ComparisonType>( ignoreTypes: ignoreTypes );

            tbCookieParameter.Text = cookieRequestFilter.Key;
            ddlCookieMatchOptions.SetValue( cookieRequestFilter.ComparisonType.ConvertToInt() );
            tbCookieValue.Text = cookieRequestFilter.ComparisonValue;

            mdCookie.Show();
        }
        protected void mdCookie_SaveClick( object sender, EventArgs e )
        {
            var cookieGuid = hfQueryStringFilterGuid.Value.AsGuid();
            var cookieFilter = this.AdditionalFilterConfiguration.CookieRequestFilters
                .Where( a => a.Guid == cookieGuid )
                .FirstOrDefault();

            if ( cookieFilter == null )
            {
                cookieFilter = new Rock.Personalization.CookieRequestFilter();
                cookieFilter.Guid = hfCookie.Value.AsGuid();
                this.AdditionalFilterConfiguration.CookieRequestFilters.Add( cookieFilter );
            }

            cookieFilter.Key = tbCookieParameter.Text;
            cookieFilter.ComparisonType = ddlCookieMatchOptions.SelectedValue.ConvertToEnum<ComparisonType>();
            cookieFilter.ComparisonValue = tbCookieValue.Text;

            mdCookie.Hide();
            BindCookieFilterToGrid();
        }

        private void BindCookieFilterToGrid()
        {
            var cookieFilter = this.AdditionalFilterConfiguration.CookieRequestFilters;
            gQueryStringFilter.DataSource = cookieFilter.OrderBy( c => c.Key );
        }

        #endregion Cookie Filter

        #region Browser Filter
        private void gBrowser_AddClick( object sender, EventArgs e )
        {
            ShowBrowserDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gCookie control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gBrowser_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var browserGuid = ( Guid ) e.RowKeyValue;
            var browser = this.AdditionalFilterConfiguration
                .BrowserRequestFilters
                .Where( a => a.Guid == browserGuid ).FirstOrDefault();
            ShowBrowserDialog( browser );
        }

        /// <summary>
        /// Shows the Cookie Filter dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The interaction filter segment filter.</param>
        private void ShowBrowserDialog( Rock.Personalization.BrowserRequestFilter browserRequestFilter )
        {
            if ( browserRequestFilter == null )
            {
                browserRequestFilter = new Rock.Personalization.BrowserRequestFilter();
                browserRequestFilter.Guid = Guid.NewGuid();
                mdBrowser.Title = "Add Browser Filter";
            }
            else
            {
                mdBrowser.Title = "Edit Browser Filter";
            }

            ddlBrowserFamily.BindToEnum<BrowserFamilyEnum>();
            ddlBrowserMatchOptions.BindToEnum( ignoreTypes: new ComparisonType[]
            { ComparisonType.GreaterThan, ComparisonType.GreaterThanOrEqualTo, ComparisonType.LessThan, ComparisonType.LessThanOrEqualTo, ComparisonType.Between, ComparisonType.RegularExpression } );


            //hfInteractionFilterGuid.Value = browserRequestFilter.Guid.ToString();

            //ComparisonHelper.PopulateComparisonControl( ddlInteractionFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypes, true );
            //ddlInteractionFilterComparisonType.SetValue( browserRequestFilter.ComparisonType.ConvertToInt() );
            //nbInteractionFilterCompareValue.Text = browserRequestFilter.ComparisonValue.ToString();

            //var interactionChannelId = InteractionChannelCache.GetId( browserRequestFilter.InteractionChannelGuid );

            //pInteractionFilterInteractionChannel.SetValue( interactionChannelId );
            //pInteractionFilterInteractionComponent.InteractionChannelId = interactionChannelId;

            //var interactionComponentId = browserRequestFilter.InteractionComponentGuid.HasValue ? InteractionComponentCache.GetId( browserRequestFilter.InteractionComponentGuid.Value ) : null;
            //pInteractionFilterInteractionComponent.SetValue( interactionComponentId );
            //tbInteractionFilterOperation.Text = browserRequestFilter.Operation;

            //drpInteractionFilterSlidingDateRange.DelimitedValues = browserRequestFilter.SlidingDateRangeDelimitedValues;

            mdBrowser.Show();
        }
        protected void mdBrowser_SaveClick( object sender, EventArgs e )
        {
            var browserGuid = hfQueryStringFilterGuid.Value.AsGuid();
            var browserFilter = this.AdditionalFilterConfiguration.BrowserRequestFilters
                .Where( a => a.Guid == browserGuid )
                .FirstOrDefault();

            if ( browserFilter == null )
            {
                browserFilter = new Rock.Personalization.BrowserRequestFilter();
                browserFilter.Guid = hfBrowser.Value.AsGuid();
                this.AdditionalFilterConfiguration.BrowserRequestFilters.Add( browserFilter );
            }

            //var interactionChannelId = pInteractionFilterInteractionChannel.SelectedValueAsId();
            //if ( interactionChannelId == null )
            //{
            //    return;
            //}

            //browserFilter.ComparisonType = ddlInteractionFilterComparisonType
            //        .SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            //browserFilter.ComparisonValue = nbInteractionFilterCompareValue.Text.AsInteger();
            //browserFilter.InteractionChannelGuid = InteractionChannelCache.Get( interactionChannelId.Value ).Guid;

            //var interactionComponentId = pInteractionFilterInteractionComponent.SelectedValueAsId();
            //if ( interactionComponentId.HasValue )
            //{
            //    browserFilter.InteractionComponentGuid = InteractionComponentCache.Get( interactionComponentId.Value )?.Guid;
            //}
            //else
            //{
            //    browserFilter.InteractionComponentGuid = null;
            //}

            //browserFilter.SlidingDateRangeDelimitedValues = drpInteractionFilterSlidingDateRange.DelimitedValues;
            //browserFilter.Operation = tbInteractionFilterOperation.Text;
            mdBrowser.Hide();
            //BindInteractionFiltersGrid();
        }

        #endregion Browser Filter

        #region IP Address Filter
        private void gIpAddress_AddClick( object sender, EventArgs e )
        {
            ShowIPAddressDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gCookie control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gIpAddress_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var ipAddressGuid = ( Guid ) e.RowKeyValue;
            var ipAddress = this.AdditionalFilterConfiguration.IPAddressRequestFilters
                .Where( a => a.Guid == ipAddressGuid )
                .FirstOrDefault();

            ShowIPAddressDialog( ipAddress );
        }

        /// <summary>
        /// Shows the Cookie Filter dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The interaction filter segment filter.</param>
        private void ShowIPAddressDialog( Rock.Personalization.IPAddressRequestFilter ipAddressRequestFilter )
        {
            if ( ipAddressRequestFilter == null )
            {
                ipAddressRequestFilter = new Rock.Personalization.IPAddressRequestFilter();
                ipAddressRequestFilter.Guid = Guid.NewGuid();
                mdBrowser.Title = "Add IP Address Filter";
            }
            else
            {
                mdBrowser.Title = "Edit IP Address Filter";
            }

            //hfInteractionFilterGuid.Value = ipAddressRequestFilter.Guid.ToString();

            //ComparisonHelper.PopulateComparisonControl( ddlInteractionFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypes, true );
            ////ddlInteractionFilterComparisonType.SetValue( ipAddressRequestFilter.ComparisonType.ConvertToInt() );
            //nbInteractionFilterCompareValue.Text = ipAddressRequestFilter.ComparisonValue.ToString();

            //var interactionChannelId = InteractionChannelCache.GetId( ipAddressRequestFilter.InteractionChannelGuid );

            //pInteractionFilterInteractionChannel.SetValue( interactionChannelId );
            //pInteractionFilterInteractionComponent.InteractionChannelId = interactionChannelId;

            //var interactionComponentId = ipAddressRequestFilter.InteractionComponentGuid.HasValue ? InteractionComponentCache.GetId( ipAddressRequestFilter.InteractionComponentGuid.Value ) : null;
            //pInteractionFilterInteractionComponent.SetValue( interactionComponentId );
            //tbInteractionFilterOperation.Text = ipAddressRequestFilter.Operation;

            //drpInteractionFilterSlidingDateRange.DelimitedValues = ipAddressRequestFilter.SlidingDateRangeDelimitedValues;

            mdIPAddress.Show();
        }
        protected void mdIpAddress_SaveClick( object sender, EventArgs e )
        {
            var ipAddressGuid = hfQueryStringFilterGuid.Value.AsGuid();
            var ipAddressFilter = this.AdditionalFilterConfiguration.IPAddressRequestFilters
                .Where( a => a.Guid == ipAddressGuid )
                .FirstOrDefault();

            if ( ipAddressFilter == null )
            {
                ipAddressFilter = new Rock.Personalization.IPAddressRequestFilter();
                ipAddressFilter.Guid = hfBrowser.Value.AsGuid();
                this.AdditionalFilterConfiguration.IPAddressRequestFilters.Add( ipAddressFilter );
            }

            //var interactionChannelId = pInteractionFilterInteractionChannel.SelectedValueAsId();
            //if ( interactionChannelId == null )
            //{
            //    return;
            //}

            //ipAddressFilter.ComparisonType = ddlInteractionFilterComparisonType
            //        .SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            //ipAddressFilter.ComparisonValue = nbInteractionFilterCompareValue.Text.AsInteger();
            //ipAddressFilter.InteractionChannelGuid = InteractionChannelCache.Get( interactionChannelId.Value ).Guid;

            //var interactionComponentId = pInteractionFilterInteractionComponent.SelectedValueAsId();
            //if ( interactionComponentId.HasValue )
            //{
            //    ipAddressFilter.InteractionComponentGuid = InteractionComponentCache.Get( interactionComponentId.Value )?.Guid;
            //}
            //else
            //{
            //    ipAddressFilter.InteractionComponentGuid = null;
            //}

            //ipAddressFilter.SlidingDateRangeDelimitedValues = drpInteractionFilterSlidingDateRange.DelimitedValues;
            //ipAddressFilter.Operation = tbInteractionFilterOperation.Text;
            mdIPAddress.Hide();
            //BindInteractionFiltersGrid();
        }

        #endregion IP Address Filter

        // ## TODO ## remove this

        /// <summary>
        /// Handles the Click event of the btnTestSegmentFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnTestSegmentFilters_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var personalizationSegmentId = hfRequestFilterId.Value.AsInteger();

            rockContext.SqlLogging( true );
            var personAliasService = new PersonAliasService( rockContext );
            var parameterExpression = personAliasService.ParameterExpression;
            var personalizationSegmentCache = PersonalizationSegmentCache.Get( personalizationSegmentId );
            Expression segmentFiltersWhereExpression = personalizationSegmentCache.GetPersonAliasFiltersWhereExpression( personAliasService, parameterExpression );

            var personAliasQuery = personAliasService.Get( parameterExpression, segmentFiltersWhereExpression );

            var dataViewFilterId = dvpFilterDataView.SelectedValueAsId();
            if ( dataViewFilterId.HasValue )
            {
                var args = new DataViewGetQueryArgs { DbContext = rockContext };
                var dataView = new DataViewService( rockContext ).Get( dataViewFilterId.Value );

                var personDataViewQuery = new PersonService( rockContext ).GetQueryUsingDataView( dataView );
                personAliasQuery = personAliasQuery.Where( pa => personDataViewQuery.Any( person => person.Aliases.Any( alias => alias.Id == pa.Id ) ) );
            }

            var results = personAliasQuery.ToList();

            rockContext.SqlLogging( false );

            // 
            Debug.WriteLine( $"\n\nPerson Alias Count: {results.Count}" );
        }
    }
}