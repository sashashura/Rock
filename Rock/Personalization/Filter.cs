using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Rock.Model;

namespace Rock.Personalization
{
    /// <summary>
    /// Class Filter.
    /// </summary>
    public abstract class Filter
    {
        /// <summary>
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <value>The unique identifier.</value>
        public Guid Guid { get; set; }

        /// <summary>
        /// Gets the where person alias expression.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        public abstract Expression GetWherePersonAliasExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression );
    }
}
