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
    /// Implements the <see cref="Rock.Personalization.Filter" />
    /// </summary>
    /// <seealso cref="Rock.Personalization.Filter" />
    public class InteractionSegmentFilter : Rock.Personalization.Filter
    {
        public override Expression GetWherePersonAliasExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            throw new NotImplementedException();
        }
    }
}
