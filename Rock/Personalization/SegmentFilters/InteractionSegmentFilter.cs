using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Rock.Model;

namespace Rock.Personalization.SegmentFilters
{
    /// <summary>
    /// Class InteractionSegmentFilter.
    /// Implements the <see cref="Rock.Personalization.SegmentFilter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.SegmentFilter" />
    public class InteractionSegmentFilter : Rock.Personalization.SegmentFilter
    {
        /// <summary>
        /// Gets the description based on how the filter is configured.
        /// </summary>
        /// <returns>System.String.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string GetDescription()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets Expression that will be used as one of the WHERE clauses for the PersonAlias query.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Expression GetWherePersonAliasExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            throw new NotImplementedException();
        }
    }
}
