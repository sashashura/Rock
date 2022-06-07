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
using System.Collections.Generic;
using System.Linq.Expressions;

using Rock.Model;
using Rock.Personalization.SegmentFilters;

namespace Rock.Personalization
{
    /// <summary>
    /// Configuration class for Additional Filters for Personalization Segments.
    /// </summary>
    public class SegmentAdditionalFilterConfiguration
    {
        /// <summary>
        /// Gets or sets <see cref="FilterExpressionType"/>
        /// </summary>
        /// <value>The type of the session filter expression.</value>
        public FilterExpressionType SessionFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// Gets or sets the session segment filters.
        /// </summary>
        /// <value>The session segment filters.</value>
        public List<SessionCountSegmentFilter> SessionSegmentFilters { get; set; } = new List<SessionCountSegmentFilter>();

        /// <summary>
        /// Gets or sets the type of the page view filter expression.
        /// </summary>
        /// <value>The type of the page view filter expression.</value>
        public FilterExpressionType PageViewFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// Gets or sets the page view segment filters.
        /// </summary>
        /// <value>The page view segment filters.</value>
        public List<PageViewSegmentFilter> PageViewSegmentFilters { get; set; } = new List<PageViewSegmentFilter>();

        /// <summary>
        /// Gets or sets the type of the interaction filter expression.
        /// </summary>
        /// <value>The type of the interaction filter expression.</value>
        public FilterExpressionType InteractionFilterExpressionType { get; set; } = FilterExpressionType.GroupAll;

        /// <summary>
        /// Gets or sets the interaction segment filters.
        /// </summary>
        /// <value>The interaction segment filters.</value>
        public List<InteractionSegmentFilter> InteractionSegmentFilters { get; set; } = new List<InteractionSegmentFilter>();

        /// <summary>
        /// Gets the page view segment filters where expression.
        /// </summary>
        /// <param name="segmentFilters">The segment filters.</param>
        /// <param name="filterExpressionType">Type of the filter expression.</param>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        private static Expression CombineSegmentFilters( IEnumerable<SegmentFilter> segmentFilters, FilterExpressionType filterExpressionType, PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            Expression allPageViewSegmentFiltersExpression = null;

            foreach ( var pageViewSegment in segmentFilters )
            {
                var segmentWhereExpression = pageViewSegment.GetWherePersonAliasExpression( personAliasService, parameterExpression );
                allPageViewSegmentFiltersExpression = AppendExpression( allPageViewSegmentFiltersExpression, segmentWhereExpression, filterExpressionType );
            }

            if ( allPageViewSegmentFiltersExpression == null )
            {
                // if there aren't any 'where' expressions, don't filter
                allPageViewSegmentFiltersExpression = Expression.Constant( true );
            }

            return allPageViewSegmentFiltersExpression;
        }

        /// <summary>
        /// Appends the expression.
        /// </summary>
        /// <param name="allSegmentFiltersExpression">All segment filters expression.</param>
        /// <param name="segmentWhereExpression">The segment where expression.</param>
        /// <param name="filterExpressionType">Type of the filter expression.</param>
        /// <returns>Expression.</returns>
        private static Expression AppendExpression( Expression allSegmentFiltersExpression, Expression segmentWhereExpression, FilterExpressionType filterExpressionType )
        {
            if ( segmentWhereExpression == null )
            {
                return allSegmentFiltersExpression;
            }

            if ( allSegmentFiltersExpression == null )
            {
                allSegmentFiltersExpression = segmentWhereExpression;
            }
            else
            {
                if ( filterExpressionType == FilterExpressionType.GroupAll )
                {
                    allSegmentFiltersExpression = Expression.AndAlso( allSegmentFiltersExpression, segmentWhereExpression );
                }
                else if ( filterExpressionType == FilterExpressionType.GroupAny )
                {
                    allSegmentFiltersExpression = Expression.Or( allSegmentFiltersExpression, segmentWhereExpression );
                }
            }

            return allSegmentFiltersExpression;
        }

        /// <summary>
        /// Gets the final person alias filters where expression.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>System.Linq.Expressions.Expression.</returns>
        public Expression GetPersonAliasFiltersWhereExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            Expression finalExpression = null;
            var sessionSegmentFiltersWhereExpression = CombineSegmentFilters( SessionSegmentFilters, SessionFilterExpressionType, personAliasService, parameterExpression );
            var pageViewSegmentFiltersWhereExpression = CombineSegmentFilters( PageViewSegmentFilters, PageViewFilterExpressionType, personAliasService, parameterExpression );
            var interactionSegmentFiltersWhereExpression = CombineSegmentFilters( InteractionSegmentFilters, InteractionFilterExpressionType, personAliasService, parameterExpression );

            finalExpression = AppendExpression( finalExpression, sessionSegmentFiltersWhereExpression, FilterExpressionType.GroupAll );
            finalExpression = AppendExpression( finalExpression, pageViewSegmentFiltersWhereExpression, FilterExpressionType.GroupAll );
            finalExpression = AppendExpression( finalExpression, interactionSegmentFiltersWhereExpression, FilterExpressionType.GroupAll );

            return finalExpression;
        }
    }
}
