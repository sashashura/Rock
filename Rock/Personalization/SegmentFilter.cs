using System;
using System.Linq.Expressions;

using Rock.Model;

namespace Rock.Personalization
{
    /// <summary>
    /// Class Filter.
    /// </summary>
    public abstract class SegmentFilter
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets the description based on how the filter is configured.
        /// </summary>
        /// <returns>System.String.</returns>
        public abstract string GetDescription();

        /// <summary>
        /// Gets Expression that will be used as one of the WHERE clauses for the PersonAlias query.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        public abstract Expression GetWherePersonAliasExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression );
    }
}
