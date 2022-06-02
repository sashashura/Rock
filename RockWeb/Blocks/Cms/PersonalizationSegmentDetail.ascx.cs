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
using System.Web.UI;

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

        #region PageParameter Keys

        private static class PageParameterKey
        {
            public const string SegmentId = "SegmentId";
        }

        #endregion PageParameter Keys

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

        private void LoadDropDowns()
        {
            ComparisonHelper.PopulateComparisonControl( ddlPageViewFilterComparisonType, ComparisonHelper.NumericFilterComparisonTypes | ComparisonType.Between );
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

            /* Person Filters */
            dvpFilterDataView.SetValue( segment.FilterDataViewId );

            /* Session Filters */
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
            // TODO
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {

        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {

        }

        #endregion

        protected void btnTestPageViewFilter_Click( object sender, EventArgs e )
        {
            var pageViewSegmentFilter = new PageViewSegmentFilter();
            pageViewSegmentFilter.ComparisonType = ddlPageViewFilterComparisonType.SelectedValueAsEnumOrNull<ComparisonType>() ?? ComparisonType.GreaterThanOrEqualTo;
            pageViewSegmentFilter.ComparisonValue = nbPageViewFilterCompareValue.Text.AsInteger();
            pageViewSegmentFilter.ComparisonValueBetweenUpper = nbPageViewFilterCompareValueUpper.Text.AsIntegerOrNull();
            pageViewSegmentFilter.SlidingDateRangeDelimitedValues = drpPageViewFilterSlidingDateRange.DelimitedValues;
            pageViewSegmentFilter.SiteGuids.Add( Rock.SystemGuid.Site.EXTERNAL_SITE.AsGuid(), true );
            pageViewSegmentFilter.SiteGuids.Add( Rock.SystemGuid.Site.SITE_ROCK_INTERNAL.AsGuid(), true );

            var rockContext = new RockContext();
            rockContext.SqlLogging( true );
            var personAliasService = new PersonAliasService( rockContext );
            var parameterExpression = personAliasService.ParameterExpression;
            var whereExpression = pageViewSegmentFilter.GetWherePersonAliasExpression( personAliasService, parameterExpression );
            var results = personAliasService.Get( parameterExpression, whereExpression ).ToList();
            rockContext.SqlLogging( false );

            // 
            Debug.WriteLine($"\n\nPerson Alias Count: {results.Count}" );
            //pageViewSegmentFilter.PageGuids = 
        }

        protected void mdSessionFilterConfiguration_SaveClick( object sender, EventArgs e )
        {

        }

        protected void mdTestShowConfiguration_Click( object sender, EventArgs e )
        {
            mdSessionFilterConfiguration.Show();
        }
    }
}