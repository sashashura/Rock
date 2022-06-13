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
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Personalization.SegmentFilters;
using Rock.Reporting;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
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
            public const string PersonalizationSegmentId = "PersonalizationSegmentId";
        }

        #endregion PageParameter Keys

        private Rock.Personalization.PersonalizationSegmentAdditionalFilterConfiguration AdditionalFilterConfiguration { get; set; }

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

            gSessionCountFilters.DataKeyNames = new string[] { "Guid" };
            gSessionCountFilters.Actions.ShowAdd = true;
            gSessionCountFilters.Actions.AddClick += gSessionCountFilters_AddClick;

            gPageViewFilters.DataKeyNames = new string[] { "Guid" };
            gPageViewFilters.Actions.ShowAdd = true;
            gPageViewFilters.Actions.AddClick += gPageViewFilters_AddClick;

            gInteractionFilters.DataKeyNames = new string[] { "Guid" };
            gInteractionFilters.Actions.ShowAdd = true;
            gInteractionFilters.Actions.AddClick += gInteractionFilters_AddClick;

            gIPAddress.Actions.ShowAdd = true;
            gBrowser.Actions.ShowAdd = true;

            gCookie.Actions.ShowAdd = true;
            gCookie.Actions.AddClick += gCookie_AddClick;

            gQueryStringFilter.Actions.ShowAdd = true;
            gQueryStringFilter.Actions.AddClick += gQueryStringFilter_AddClick;
        }

        #region Query String Filter
        private void gQueryStringFilter_AddClick( object sender, EventArgs e )
        {
            ShowQueryStringFilterDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gInteractionFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gQueryStringFilter_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var queryStringFilterGuid = ( Guid ) e.RowKeyValue;
            var queryStringFilter = this.AdditionalFilterConfiguration.InteractionSegmentFilters.Where( a => a.Guid == queryStringFilterGuid ).FirstOrDefault();
            ShowQueryStringFilterDialog( queryStringFilter );
        }

        /// <summary>
        /// Shows the query filter string dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The interaction filter segment filter.</param>
        private void ShowQueryStringFilterDialog( Rock.Personalization.SegmentFilters.InteractionSegmentFilter interactionSegmentFilter )
        {
            if ( interactionSegmentFilter == null )
            {
                interactionSegmentFilter = new InteractionSegmentFilter();
                interactionSegmentFilter.Guid = Guid.NewGuid();
                mdQueryStringFilter.Title = "Add Query String Filter";
            }
            else
            {
                mdQueryStringFilter.Title = "Edit Query String Filter";
            }

            ddlQueryStringFilterMatchOptions.BindToEnum<ComparisonType>( ignoreTypes: new ComparisonType []
            { ComparisonType.GreaterThan, ComparisonType.GreaterThanOrEqualTo, ComparisonType.LessThan, ComparisonType.LessThanOrEqualTo, ComparisonType.Between, ComparisonType.RegularExpression } );


            hfInteractionFilterGuid.Value = interactionSegmentFilter.Guid.ToString();

            ComparisonHelper.PopulateComparisonControl( ddlInteractionFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypes, true );
            ddlInteractionFilterComparisonType.SetValue( interactionSegmentFilter.ComparisonType.ConvertToInt() );
            nbInteractionFilterCompareValue.Text = interactionSegmentFilter.ComparisonValue.ToString();

            var interactionChannelId = InteractionChannelCache.GetId( interactionSegmentFilter.InteractionChannelGuid );

            pInteractionFilterInteractionChannel.SetValue( interactionChannelId );
            pInteractionFilterInteractionComponent.InteractionChannelId = interactionChannelId;

            var interactionComponentId = interactionSegmentFilter.InteractionComponentGuid.HasValue ? InteractionComponentCache.GetId( interactionSegmentFilter.InteractionComponentGuid.Value ) : null;
            pInteractionFilterInteractionComponent.SetValue( interactionComponentId );
            tbInteractionFilterOperation.Text = interactionSegmentFilter.Operation;

            drpInteractionFilterSlidingDateRange.DelimitedValues = interactionSegmentFilter.SlidingDateRangeDelimitedValues;

            mdQueryStringFilter.Show();
        }

        protected void mdQueryStringFilter_SaveClick( object sender, EventArgs e )
        {
            var queryStringFilterGuid = hfQueryStringFilter.Value.AsGuid();
            var queryStringFilter = this.AdditionalFilterConfiguration.InteractionSegmentFilters
                .Where( a => a.Guid == queryStringFilterGuid )
                .FirstOrDefault();

            if ( queryStringFilter == null )
            {
                queryStringFilter = new InteractionSegmentFilter();
                queryStringFilter.Guid = hfInteractionFilterGuid.Value.AsGuid();
                this.AdditionalFilterConfiguration.InteractionSegmentFilters.Add( queryStringFilter );
            }

            var interactionChannelId = pInteractionFilterInteractionChannel.SelectedValueAsId();
            if ( interactionChannelId == null )
            {
                return;
            }

            queryStringFilter.ComparisonType = ddlInteractionFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            queryStringFilter.ComparisonValue = nbInteractionFilterCompareValue.Text.AsInteger();
            queryStringFilter.InteractionChannelGuid = InteractionChannelCache.Get( interactionChannelId.Value ).Guid;

            var interactionComponentId = pInteractionFilterInteractionComponent.SelectedValueAsId();
            if ( interactionComponentId.HasValue )
            {
                queryStringFilter.InteractionComponentGuid = InteractionComponentCache.Get( interactionComponentId.Value )?.Guid;
            }
            else
            {
                queryStringFilter.InteractionComponentGuid = null;
            }

            queryStringFilter.SlidingDateRangeDelimitedValues = drpInteractionFilterSlidingDateRange.DelimitedValues;
            queryStringFilter.Operation = tbInteractionFilterOperation.Text;
            mdQueryStringFilter.Hide();
            BindInteractionFiltersGrid();
        }

        #endregion Query String Filter

        #region Cookie Filter
        private void gCookie_AddClick( object sender, EventArgs e )
        {
            ShowCookieDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gInteractionFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gCookie_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var cookieGuid = ( Guid ) e.RowKeyValue;
            var cookie = this.AdditionalFilterConfiguration.InteractionSegmentFilters.Where( a => a.Guid == cookieGuid ).FirstOrDefault();
            ShowCookieDialog( cookie );
        }

        /// <summary>
        /// Shows the query filter string dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The interaction filter segment filter.</param>
        private void ShowCookieDialog( Rock.Personalization.SegmentFilters.InteractionSegmentFilter interactionSegmentFilter )
        {
            if ( interactionSegmentFilter == null )
            {
                interactionSegmentFilter = new InteractionSegmentFilter();
                interactionSegmentFilter.Guid = Guid.NewGuid();
                mdCookie.Title = "Add Cookie Filter";
            }
            else
            {
                mdCookie.Title = "Edit Cookie Filter";
            }

            ddlCookieMatchOptions.BindToEnum<ComparisonType>( ignoreTypes: new ComparisonType[]
            { ComparisonType.GreaterThan, ComparisonType.GreaterThanOrEqualTo, ComparisonType.LessThan, ComparisonType.LessThanOrEqualTo, ComparisonType.Between, ComparisonType.RegularExpression } );


            hfInteractionFilterGuid.Value = interactionSegmentFilter.Guid.ToString();

            ComparisonHelper.PopulateComparisonControl( ddlInteractionFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypes, true );
            ddlInteractionFilterComparisonType.SetValue( interactionSegmentFilter.ComparisonType.ConvertToInt() );
            nbInteractionFilterCompareValue.Text = interactionSegmentFilter.ComparisonValue.ToString();

            var interactionChannelId = InteractionChannelCache.GetId( interactionSegmentFilter.InteractionChannelGuid );

            pInteractionFilterInteractionChannel.SetValue( interactionChannelId );
            pInteractionFilterInteractionComponent.InteractionChannelId = interactionChannelId;

            var interactionComponentId = interactionSegmentFilter.InteractionComponentGuid.HasValue ? InteractionComponentCache.GetId( interactionSegmentFilter.InteractionComponentGuid.Value ) : null;
            pInteractionFilterInteractionComponent.SetValue( interactionComponentId );
            tbInteractionFilterOperation.Text = interactionSegmentFilter.Operation;

            drpInteractionFilterSlidingDateRange.DelimitedValues = interactionSegmentFilter.SlidingDateRangeDelimitedValues;

            mdCookie.Show();
        }
        protected void mdCookie_SaveClick( object sender, EventArgs e )
        {
            var cookieGuid = hfQueryStringFilter.Value.AsGuid();
            var cookieFilter = this.AdditionalFilterConfiguration.InteractionSegmentFilters
                .Where( a => a.Guid == cookieGuid )
                .FirstOrDefault();

            if ( cookieFilter == null )
            {
                cookieFilter = new InteractionSegmentFilter();
                cookieFilter.Guid = hfCookie.Value.AsGuid();
                this.AdditionalFilterConfiguration.InteractionSegmentFilters.Add( cookieFilter );
            }

            var interactionChannelId = pInteractionFilterInteractionChannel.SelectedValueAsId();
            if ( interactionChannelId == null )
            {
                return;
            }

            cookieFilter.ComparisonType = ddlInteractionFilterComparisonType
                    .SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            cookieFilter.ComparisonValue = nbInteractionFilterCompareValue.Text.AsInteger();
            cookieFilter.InteractionChannelGuid = InteractionChannelCache.Get( interactionChannelId.Value ).Guid;

            var interactionComponentId = pInteractionFilterInteractionComponent.SelectedValueAsId();
            if ( interactionComponentId.HasValue )
            {
                cookieFilter.InteractionComponentGuid = InteractionComponentCache.Get( interactionComponentId.Value )?.Guid;
            }
            else
            {
                cookieFilter.InteractionComponentGuid = null;
            }

            cookieFilter.SlidingDateRangeDelimitedValues = drpInteractionFilterSlidingDateRange.DelimitedValues;
            cookieFilter.Operation = tbInteractionFilterOperation.Text;
            mdCookie.Hide();
            BindInteractionFiltersGrid();
        }

        #endregion Cookie Filter

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( PageParameterKey.PersonalizationSegmentId ).AsInteger() );
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

            this.AdditionalFilterConfiguration = additionalFilterConfigurationJson.FromJsonOrNull<Rock.Personalization.PersonalizationSegmentAdditionalFilterConfiguration>() ?? new Rock.Personalization.PersonalizationSegmentAdditionalFilterConfiguration();
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
        /// <param name="personalizationSegmentId">The segment identifier.</param>
        public void ShowDetail( int personalizationSegmentId )
        {
            var rockContext = new RockContext();

            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );
            PersonalizationSegment personalizationSegment = null;

            if ( personalizationSegmentId > 0 )
            {
                personalizationSegment = personalizationSegmentService.Get( personalizationSegmentId );
            }

            if ( personalizationSegment == null )
            {
                personalizationSegment = new PersonalizationSegment();
            }

            if ( personalizationSegment.Id == 0 )
            {
                lPanelTitle.Text = ActionTitle.Add( PersonalizationSegment.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lPanelTitle.Text = personalizationSegment.Name;
            }

            /* Name, etc */
            hfRequestFilterId.Value = personalizationSegment.Id.ToString();
            tbName.Text = personalizationSegment.Name;
            tbSegmentKey.Text = personalizationSegment.SegmentKey;
            hlInactive.Visible = !personalizationSegment.IsActive;
            cbIsActive.Checked = personalizationSegment.IsActive;
            hfExistingSegmentKeyNames.Value = personalizationSegmentService.Queryable().Where( a => a.Id != personalizationSegment.Id ).Select( a => a.SegmentKey ).ToList().ToJson();

            this.AdditionalFilterConfiguration = personalizationSegment.AdditionalFilterConfiguration ?? new Rock.Personalization.PersonalizationSegmentAdditionalFilterConfiguration();

            // Person Filters
            dvpFilterDataView.SetValue( personalizationSegment.FilterDataViewId );
            ShowDataViewWarningIfInvalid( personalizationSegment.FilterDataViewId );

            // Session Filters
            tglSessionCountFiltersAllAny.Checked = AdditionalFilterConfiguration.SessionFilterExpressionType == FilterExpressionType.GroupAll;
            BindSessionCountFiltersGrid();

            // Page View Filters
            tglPageViewFiltersAllAny.Checked = AdditionalFilterConfiguration.PageViewFilterExpressionType == FilterExpressionType.GroupAll;
            BindPageViewFiltersGrid();

            // Interaction Filters
            //tglInteractionFiltersAllAny.Checked = AdditionalFilterConfiguration.InteractionFilterExpressionType == FilterExpressionType.GroupAll;
            BindInteractionFiltersGrid();

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

            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );
            PersonalizationSegment personalizationSegment;

            var personalizationSegmentId = hfRequestFilterId.Value.AsInteger();

            if ( personalizationSegmentId == 0 )
            {
                personalizationSegment = new PersonalizationSegment();
                personalizationSegment.Id = personalizationSegmentId;
                personalizationSegmentService.Add( personalizationSegment );
            }
            else
            {
                personalizationSegment = personalizationSegmentService.Get( personalizationSegmentId );
            }

            if ( personalizationSegment == null )
            {
                return;
            }

            personalizationSegment.Name = tbName.Text;
            personalizationSegment.IsActive = cbIsActive.Checked;
            personalizationSegment.SegmentKey = tbSegmentKey.Text;
            personalizationSegment.FilterDataViewId = dvpFilterDataView.SelectedValueAsId();

            if ( tglSessionCountFiltersAllAny.Checked )
            {
                AdditionalFilterConfiguration.SessionFilterExpressionType = FilterExpressionType.GroupAll;
            }
            else
            {
                AdditionalFilterConfiguration.SessionFilterExpressionType = FilterExpressionType.GroupAny;
            }

            if ( tglPageViewFiltersAllAny.Checked )
            {
                AdditionalFilterConfiguration.PageViewFilterExpressionType = FilterExpressionType.GroupAll;
            }
            else
            {
                AdditionalFilterConfiguration.PageViewFilterExpressionType = FilterExpressionType.GroupAny;
            }

            //if ( tglInteractionFiltersAllAny.Checked )
            //{
            //    AdditionalFilterConfiguration.InteractionFilterExpressionType = FilterExpressionType.GroupAll;
            //}
            //else
            //{
            //    AdditionalFilterConfiguration.InteractionFilterExpressionType = FilterExpressionType.GroupAny;
            //}

            personalizationSegment.AdditionalFilterConfiguration = this.AdditionalFilterConfiguration;

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

        #region Session Filters Related

        /// <summary>
        /// Binds the session count filters grid.
        /// </summary>
        private void BindSessionCountFiltersGrid()
        {
            var sessionCountFilters = this.AdditionalFilterConfiguration.SessionSegmentFilters;
            gSessionCountFilters.DataSource = sessionCountFilters.OrderBy( a => a.GetDescription() );
            gSessionCountFilters.DataBind();
        }

        /// <summary>
        /// Handles the AddClick event of the gSessionCountFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gSessionCountFilters_AddClick( object sender, EventArgs e )
        {
            ShowSessionCountFilterDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gSessionCountFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gSessionCountFilters_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var sessionSegmentFilterGuid = ( Guid ) e.RowKeyValue;
            var sessionSegmentFilter = this.AdditionalFilterConfiguration.SessionSegmentFilters.FirstOrDefault( a => a.Guid == sessionSegmentFilterGuid );
            ShowSessionCountFilterDialog( sessionSegmentFilter );
        }

        /// <summary>
        /// Shows the session count filter dialog.
        /// </summary>
        /// <param name="sessionCountSegmentFilter">The session count segment filter.</param>
        private void ShowSessionCountFilterDialog( Rock.Personalization.SegmentFilters.SessionCountSegmentFilter sessionCountSegmentFilter )
        {
            if ( sessionCountSegmentFilter == null )
            {
                sessionCountSegmentFilter = new SessionCountSegmentFilter();
                sessionCountSegmentFilter.Guid = Guid.NewGuid();
                mdSessionCountFilterConfiguration.Title = "Add Session Filter";
            }
            else
            {
                mdSessionCountFilterConfiguration.Title = "Edit Session Filter";
            }

            hfSessionCountFilterGuid.Value = sessionCountSegmentFilter.Guid.ToString();

            lstSessionCountFilterWebSites.Items.Clear();
            foreach ( var site in SiteCache.All().Where( a => a.IsActive ) )
            {
                lstSessionCountFilterWebSites.Items.Add( new ListItem( site.Name, site.Guid.ToString() ) );
            }

            ComparisonHelper.PopulateComparisonControl( ddlSessionCountFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypes, true );
            ddlSessionCountFilterComparisonType.SetValue( sessionCountSegmentFilter.ComparisonType.ConvertToInt() );
            nbSessionCountFilterCompareValue.Text = sessionCountSegmentFilter.ComparisonValue.ToString();
            drpSessionCountFilterSlidingDateRange.DelimitedValues = sessionCountSegmentFilter.SlidingDateRangeDelimitedValues;
            lstSessionCountFilterWebSites.SetValues( sessionCountSegmentFilter.SiteGuids );

            mdSessionCountFilterConfiguration.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdSessionCountFilterConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdSessionCountFilterConfiguration_SaveClick( object sender, EventArgs e )
        {
            var sessionSegmentFilterGuid = hfSessionCountFilterGuid.Value.AsGuid();
            var sessionSegmentFilter = this.AdditionalFilterConfiguration.SessionSegmentFilters.Where( a => a.Guid == sessionSegmentFilterGuid ).FirstOrDefault();
            if ( sessionSegmentFilter == null )
            {
                sessionSegmentFilter = new SessionCountSegmentFilter();
                sessionSegmentFilter.Guid = hfSessionCountFilterGuid.Value.AsGuid();
                this.AdditionalFilterConfiguration.SessionSegmentFilters.Add( sessionSegmentFilter );
            }

            sessionSegmentFilter.ComparisonType = ddlSessionCountFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            sessionSegmentFilter.ComparisonValue = nbSessionCountFilterCompareValue.Text.AsInteger();
            sessionSegmentFilter.SiteGuids = lstSessionCountFilterWebSites.SelectedValuesAsGuid;

            sessionSegmentFilter.SlidingDateRangeDelimitedValues = drpSessionCountFilterSlidingDateRange.DelimitedValues;
            mdSessionCountFilterConfiguration.Hide();
            BindSessionCountFiltersGrid();
        }

        /// <summary>
        /// Handles the DeleteClick event of the gSessionCountFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gSessionCountFilters_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var sessionSegmentFilterGuid = ( Guid ) e.RowKeyValue;
            var sessionSegmentFilter = this.AdditionalFilterConfiguration.SessionSegmentFilters.Where( a => a.Guid == sessionSegmentFilterGuid ).FirstOrDefault();
            if ( sessionSegmentFilter != null )
            {
                this.AdditionalFilterConfiguration.SessionSegmentFilters.Remove( sessionSegmentFilter );
            }

            BindSessionCountFiltersGrid();
        }

        /// <summary>
        /// Handles the DataBound event of the lSessionCountFilterDescription control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void lSessionCountFilterDescription_DataBound( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            SessionCountSegmentFilter sessionCountSegmentFilter = e.Row.DataItem as SessionCountSegmentFilter;
            var lSessionCountFilter = sender as Literal;
            if ( sessionCountSegmentFilter == null || lSessionCountFilter == null )
            {
                return;
            }

            lSessionCountFilter.Text = sessionCountSegmentFilter.GetDescription();
        }

        #endregion Session Filters Related

        #region Page View Filters Related

        /// <summary>
        /// Binds the page views filters grid.
        /// </summary>
        private void BindPageViewFiltersGrid()
        {
            var pageViewFilters = this.AdditionalFilterConfiguration.PageViewSegmentFilters;
            gPageViewFilters.DataSource = pageViewFilters.OrderBy( a => a.GetDescription() );
            gPageViewFilters.DataBind();
        }

        /// <summary>
        /// Handles the AddClick event of the gPageViewFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPageViewFilters_AddClick( object sender, EventArgs e )
        {
            ShowPageViewFilterDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gPageViewFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gPageViewFilters_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var pageViewFilterGuid = ( Guid ) e.RowKeyValue;
            var pageViewFilter = this.AdditionalFilterConfiguration.PageViewSegmentFilters.Where( a => a.Guid == pageViewFilterGuid ).FirstOrDefault();
            ShowPageViewFilterDialog( pageViewFilter );
        }

        /// <summary>
        /// Shows the page view filter dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The page view filter segment filter.</param>
        private void ShowPageViewFilterDialog( Rock.Personalization.SegmentFilters.PageViewSegmentFilter pageViewFilterSegmentFilter )
        {
            if ( pageViewFilterSegmentFilter == null )
            {
                pageViewFilterSegmentFilter = new PageViewSegmentFilter();
                pageViewFilterSegmentFilter.Guid = Guid.NewGuid();
                mdPageViewFilterConfiguration.Title = "Add Page View Filter";
            }
            else
            {
                mdPageViewFilterConfiguration.Title = "Edit Page View Filter";
            }

            hfPageViewFilterGuid.Value = pageViewFilterSegmentFilter.Guid.ToString();

            lstPageViewFilterWebSites.Items.Clear();
            foreach ( var site in SiteCache.All().Where( a => a.IsActive ) )
            {
                lstPageViewFilterWebSites.Items.Add( new ListItem( site.Name, site.Guid.ToString() ) );
            }

            ComparisonHelper.PopulateComparisonControl( ddlPageViewFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypes, true );
            ddlPageViewFilterComparisonType.SetValue( pageViewFilterSegmentFilter.ComparisonType.ConvertToInt() );
            nbPageViewFilterCompareValue.Text = pageViewFilterSegmentFilter.ComparisonValue.ToString();
            drpPageViewFilterSlidingDateRange.DelimitedValues = pageViewFilterSegmentFilter.SlidingDateRangeDelimitedValues;
            lstPageViewFilterWebSites.SetValues( pageViewFilterSegmentFilter.SiteGuids );

            ppPageViewFilterPages.SetValues( pageViewFilterSegmentFilter.GetSelectedPages().Select( a => a.Id ) );

            mdPageViewFilterConfiguration.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdPageViewFilterConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdPageViewFilterConfiguration_SaveClick( object sender, EventArgs e )
        {
            var pageViewFilterGuid = hfPageViewFilterGuid.Value.AsGuid();
            var pageViewFilter = this.AdditionalFilterConfiguration.PageViewSegmentFilters.Where( a => a.Guid == pageViewFilterGuid ).FirstOrDefault();
            if ( pageViewFilter == null )
            {
                pageViewFilter = new PageViewSegmentFilter();
                pageViewFilter.Guid = hfPageViewFilterGuid.Value.AsGuid();
                this.AdditionalFilterConfiguration.PageViewSegmentFilters.Add( pageViewFilter );
            }

            pageViewFilter.ComparisonType = ddlPageViewFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            pageViewFilter.ComparisonValue = nbPageViewFilterCompareValue.Text.AsInteger();
            pageViewFilter.SiteGuids = lstPageViewFilterWebSites.SelectedValuesAsGuid;
            pageViewFilter.PageGuids = ppPageViewFilterPages.SelectedIds.Select( a => PageCache.Get( a )?.Guid ).Where( a => a.HasValue ).Select( a => a.Value ).ToList();

            pageViewFilter.SlidingDateRangeDelimitedValues = drpPageViewFilterSlidingDateRange.DelimitedValues;
            mdPageViewFilterConfiguration.Hide();
            BindPageViewFiltersGrid();
        }

        /// <summary>
        /// Handles the DeleteClick event of the gPageViewFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gPageViewFilters_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var pageViewFilterGuid = ( Guid ) e.RowKeyValue;
            var pageViewFilter = this.AdditionalFilterConfiguration.PageViewSegmentFilters.Where( a => a.Guid == pageViewFilterGuid ).FirstOrDefault();
            if ( pageViewFilter != null )
            {
                this.AdditionalFilterConfiguration.PageViewSegmentFilters.Remove( pageViewFilter );
            }

            BindPageViewFiltersGrid();
        }

        /// <summary>
        /// Handles the DataBound event of the lPageViewFilterDescription control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void lPageViewFilterDescription_DataBound( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var pageViewFilterSegmentFilter = e.Row.DataItem as PageViewSegmentFilter;
            var lPageViewFilter = sender as Literal;
            if ( pageViewFilterSegmentFilter == null || lPageViewFilter == null )
            {
                return;
            }

            lPageViewFilter.Text = pageViewFilterSegmentFilter.GetDescription();
        }

        #endregion Page View Filters Related

        #region Interaction Filter Related

        /// <summary>
        /// Binds the interactions views filters grid.
        /// </summary>
        private void BindInteractionFiltersGrid()
        {
            var interactionSegmentFilters = this.AdditionalFilterConfiguration.InteractionSegmentFilters;
            var interactionSegmentDataSource = interactionSegmentFilters.Select( a => new
            {
                a.Guid,
                InteractionChannelName = InteractionChannelCache.Get( a.InteractionChannelGuid )?.Name,
                InteractionComponentName = a.InteractionComponentGuid.HasValue ? InteractionComponentCache.Get( a.InteractionComponentGuid.Value )?.Name : "*",
                Operation = a.Operation.IfEmpty( "*" ),
                ComparisonText = $"{a.ComparisonType.ConvertToString()} {a.ComparisonValue}",
                DateRangeText = SlidingDateRangePicker.FormatDelimitedValues( a.SlidingDateRangeDelimitedValues )
            } );

            gInteractionFilters.DataSource = interactionSegmentDataSource.OrderBy( a => a.InteractionChannelName ).ThenBy( a => a.InteractionComponentName ).ToList();
            gInteractionFilters.DataBind();
        }

        /// <summary>
        /// Handles the AddClick event of the gInteractionFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gInteractionFilters_AddClick( object sender, EventArgs e )
        {
            ShowInteractionFilterDialog( null );
        }

        /// <summary>
        /// Handles the EditClick event of the gInteractionFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gInteractionFilters_EditClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var interactionSegmentFilterGuid = ( Guid ) e.RowKeyValue;
            var interactionSegmentFilter = this.AdditionalFilterConfiguration.InteractionSegmentFilters.Where( a => a.Guid == interactionSegmentFilterGuid ).FirstOrDefault();
            ShowInteractionFilterDialog( interactionSegmentFilter );
        }

        /// <summary>
        /// Shows the interactionfilter dialog.
        /// </summary>
        /// <param name="pageViewFilterSegmentFilter">The interaction filter segment filter.</param>
        private void ShowInteractionFilterDialog( Rock.Personalization.SegmentFilters.InteractionSegmentFilter interactionSegmentFilter )
        {
            if ( interactionSegmentFilter == null )
            {
                interactionSegmentFilter = new InteractionSegmentFilter();
                interactionSegmentFilter.Guid = Guid.NewGuid();
                mdInteractionFilterConfiguration.Title = "Add Interaction Filter";
            }
            else
            {
                mdInteractionFilterConfiguration.Title = "Edit Interaction Filter";
            }

            hfInteractionFilterGuid.Value = interactionSegmentFilter.Guid.ToString();

            ComparisonHelper.PopulateComparisonControl( ddlInteractionFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypes, true );
            ddlInteractionFilterComparisonType.SetValue( interactionSegmentFilter.ComparisonType.ConvertToInt() );
            nbInteractionFilterCompareValue.Text = interactionSegmentFilter.ComparisonValue.ToString();

            var interactionChannelId = InteractionChannelCache.GetId( interactionSegmentFilter.InteractionChannelGuid );

            pInteractionFilterInteractionChannel.SetValue( interactionChannelId );
            pInteractionFilterInteractionComponent.InteractionChannelId = interactionChannelId;

            var interactionComponentId = interactionSegmentFilter.InteractionComponentGuid.HasValue ? InteractionComponentCache.GetId( interactionSegmentFilter.InteractionComponentGuid.Value ) : null;
            pInteractionFilterInteractionComponent.SetValue( interactionComponentId );
            tbInteractionFilterOperation.Text = interactionSegmentFilter.Operation;

            drpInteractionFilterSlidingDateRange.DelimitedValues = interactionSegmentFilter.SlidingDateRangeDelimitedValues;

            mdInteractionFilterConfiguration.Show();
        }

        /// <summary>
        /// Handles the SaveClick event of the mdInteractionFilterConfiguration control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void mdInteractionFilterConfiguration_SaveClick( object sender, EventArgs e )
        {
            var interactionFilterGuid = hfInteractionFilterGuid.Value.AsGuid();
            var interactionFilter = this.AdditionalFilterConfiguration.InteractionSegmentFilters.Where( a => a.Guid == interactionFilterGuid ).FirstOrDefault();
            if ( interactionFilter == null )
            {
                interactionFilter = new InteractionSegmentFilter();
                interactionFilter.Guid = hfInteractionFilterGuid.Value.AsGuid();
                this.AdditionalFilterConfiguration.InteractionSegmentFilters.Add( interactionFilter );
            }

            var interactionChannelId = pInteractionFilterInteractionChannel.SelectedValueAsId();
            if ( interactionChannelId == null )
            {
                return;
            }

            interactionFilter.ComparisonType = ddlInteractionFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            interactionFilter.ComparisonValue = nbInteractionFilterCompareValue.Text.AsInteger();
            interactionFilter.InteractionChannelGuid = InteractionChannelCache.Get( interactionChannelId.Value ).Guid;

            var interactionComponentId = pInteractionFilterInteractionComponent.SelectedValueAsId();
            if ( interactionComponentId.HasValue )
            {
                interactionFilter.InteractionComponentGuid = InteractionComponentCache.Get( interactionComponentId.Value )?.Guid;
            }
            else
            {
                interactionFilter.InteractionComponentGuid = null;
            }

            interactionFilter.SlidingDateRangeDelimitedValues = drpInteractionFilterSlidingDateRange.DelimitedValues;
            interactionFilter.Operation = tbInteractionFilterOperation.Text;
            mdInteractionFilterConfiguration.Hide();
            BindInteractionFiltersGrid();
        }

        /// <summary>
        /// Handles the DeleteClick event of the gInteractionFilters control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gInteractionFilters_DeleteClick( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var interactionFilterGuid = ( Guid ) e.RowKeyValue;
            var interactionFilter = this.AdditionalFilterConfiguration.InteractionSegmentFilters.FirstOrDefault( a => a.Guid == interactionFilterGuid );
            if ( interactionFilter != null )
            {
                this.AdditionalFilterConfiguration.InteractionSegmentFilters.Remove( interactionFilter );
            }

            BindInteractionFiltersGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the pInteractionFilterInteractionChannel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void pInteractionFilterInteractionChannel_SelectedIndexChanged( object sender, EventArgs e )
        {
            pInteractionFilterInteractionComponent.InteractionChannelId = pInteractionFilterInteractionChannel.SelectedValueAsId();
        }

        #endregion Interaction Filter Related

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