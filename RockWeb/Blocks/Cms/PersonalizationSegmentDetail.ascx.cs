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
using Rock.Data;
using Rock.Model;
using Rock.Personalization.SegmentFilters;
using Rock.Reporting;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Personalization Segment Detail" )]
    [Category( "Cms" )]
    [Description( "Displays the details of an audience segment." )]

    #region Block Attributes

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "1F0A0A57-952D-4774-8760-52C6D56B9DB5" )]
    public partial class PersonalizationSegmentDetail : Rock.Web.UI.RockBlock
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
            public const string SegmentId = "SegmentId";
        }

        #endregion PageParameter Keys

        private Rock.Personalization.SegmentAdditionalFilterConfiguration AdditionalFilterConfiguration { get; set; }

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
                LoadDropDowns();
                ShowDetail( PageParameter( PageParameterKey.SegmentId ).AsInteger() );
            }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var additionalFilterConfigurationJson = this.ViewState[ViewStateKey.AdditionalFilterConfigurationJson] as string;

            this.AdditionalFilterConfiguration = additionalFilterConfigurationJson.FromJsonOrNull<Rock.Personalization.SegmentAdditionalFilterConfiguration>() ?? new Rock.Personalization.SegmentAdditionalFilterConfiguration();
        }

        protected override object SaveViewState()
        {
            this.ViewState[ViewStateKey.AdditionalFilterConfigurationJson] = this.AdditionalFilterConfiguration?.ToJson();
            return base.SaveViewState();
        }

        private void LoadDropDowns()
        {

            //ComparisonHelper.PopulateComparisonControl( ddl, ComparisonHelper.NumericFilterComparisonTypes | ComparisonType.Between );
            //ComparisonHelper.PopulateComparisonControl( ddlPageViewFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypes | ComparisonType.Between );
        }

        #endregion Base Control Methods

        #region Methods

        public void ShowDetail( int segmentId )
        {
            var rockContext = new RockContext();

            var segmentService = new SegmentService( rockContext );
            Segment segment = null;

            if ( segmentId > 0 )
            {
                segment = segmentService.Get( segmentId );
            }

            if ( segment == null )
            {
                segment = new Segment();
            }

            /* Name, etc */
            hfSegmentId.Value = segment.Id.ToString();
            tbName.Text = segment.Name;
            tbSegmentKey.Text = segment.SegmentKey;
            cbIsActive.Checked = segment.IsActive;
            hfExistingSegmentKeyNames.Value = segmentService.Queryable().Where( a => a.Id != segment.Id ).Select( a => a.SegmentKey ).ToList().ToJson();

            this.AdditionalFilterConfiguration = segment.AdditionalFilterConfiguration;

            /* Person Filters */
            dvpFilterDataView.SetValue( segment.FilterDataViewId );
            ShowDataViewWarningIfInvalid( segment.FilterDataViewId );

            /* Session Filters */
            BindSessionCountFiltersGrid();
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

        protected void dvpFilterDataView_SelectItem( object sender, EventArgs e )
        {
            var selectedDataViewId = dvpFilterDataView.SelectedValueAsId();
            ShowDataViewWarningIfInvalid( selectedDataViewId );
        }

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

            var segmentService = new SegmentService( rockContext );
            Segment segment;

            var segmentId = hfSegmentId.Value.AsInteger();

            if ( segmentId == 0 )
            {
                segment = new Segment();
                segment.Id = segmentId;
                segmentService.Add( segment );
            }
            else
            {
                segment = segmentService.Get( segmentId );
            }

            if ( segment == null )
            {
                return;
            }

            segment.Name = tbName.Text;
            segment.IsActive = cbIsActive.Checked;
            segment.SegmentKey = tbSegmentKey.Text;
            segment.FilterDataViewId = dvpFilterDataView.SelectedValueAsId();
            segment.AdditionalFilterConfiguration = this.AdditionalFilterConfiguration;

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

        protected void btnTestSessionCountFilter_Click( object sender, EventArgs e )
        {
            var sessionCountSegmentFilter = new SessionCountSegmentFilter();
            sessionCountSegmentFilter.ComparisonType = ddlSessionCountFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            sessionCountSegmentFilter.ComparisonValue = nbSessionCountFilterCompareValue.Text.AsInteger();

            sessionCountSegmentFilter.SlidingDateRangeDelimitedValues = drpSessionCountFilterSlidingDateRange.DelimitedValues;
            sessionCountSegmentFilter.SiteGuids = lstSessionCountFilterWebSites.SelectedValuesAsGuid;
            //sessionCountSegmentFilter.SiteGuids.Add( Rock.SystemGuid.Site.EXTERNAL_SITE.AsGuid(), true );
            //sessionCountSegmentFilter.SiteGuids.Add( Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), true );

            var rockContext = new RockContext();
            rockContext.SqlLogging( true );
            var personAliasService = new PersonAliasService( rockContext );
            var parameterExpression = personAliasService.ParameterExpression;
            Expression allSegmentsWhereExpression = null;

            var segmentWhereExpression = sessionCountSegmentFilter.GetWherePersonAliasExpression( personAliasService, parameterExpression );
            if ( segmentWhereExpression != null )
            {
                if ( allSegmentsWhereExpression == null )
                {
                    allSegmentsWhereExpression = segmentWhereExpression;
                }
                else
                {
                    // todo OR/AND
                    allSegmentsWhereExpression = Expression.AndAlso( allSegmentsWhereExpression, segmentWhereExpression );
                }
            }

            if ( allSegmentsWhereExpression == null )
            {
                // if there aren't any where expressions, don't return any person aliases
                allSegmentsWhereExpression = Expression.Constant( false );
            }

            var results = personAliasService.Get( parameterExpression, allSegmentsWhereExpression ).ToList();
            rockContext.SqlLogging( false );

            // 
            Debug.WriteLine( $"\n\nPerson Alias Count: {results.Count}" );
            //pageViewSegmentFilter.PageGuids = 
        }

        #region Session Filters Related

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
            var sessionSegmentFilter = this.AdditionalFilterConfiguration.SessionSegmentFilters.Where( a => a.Guid == sessionSegmentFilterGuid ).FirstOrDefault();
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
            }

            hfSessionCountFilterGuid.Value = sessionCountSegmentFilter.Guid.ToString();

            lstSessionCountFilterWebSites.Items.Clear();
            foreach ( var site in SiteCache.All().Where( a => a.IsActive ) )
            {
                lstSessionCountFilterWebSites.Items.Add( new ListItem( site.Name, site.Guid.ToString() ) );
            }

            ComparisonHelper.PopulateComparisonControl( ddlSessionCountFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypes, true, true );
            ddlSessionCountFilterComparisonType.SetValue( sessionCountSegmentFilter.ComparisonType.ConvertToInt() );
            nbSessionCountFilterCompareValue.Text = sessionCountSegmentFilter.ComparisonValue.ToString();
            drpSessionCountFilterSlidingDateRange.DelimitedValues = sessionCountSegmentFilter.SlidingDateRangeDelimitedValues;
            lstSessionCountFilterWebSites.SetValues( sessionCountSegmentFilter.SiteGuids );

            mdSessionCountFilterConfiguration.Show();
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
        /// Binds the session count filters grid.
        /// </summary>
        private void BindSessionCountFiltersGrid()
        {
            var sessionCountFilters = this.AdditionalFilterConfiguration.SessionSegmentFilters;
            gSessionCountFilters.DataSource = sessionCountFilters.OrderBy( a => a.GetDescription() );
            gSessionCountFilters.DataBind();
        }

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


    }
}