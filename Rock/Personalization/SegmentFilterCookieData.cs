using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Personalization
{
    /// <summary>
    /// SegmentIdKeys for the current person/visitor session stored in the <seealso cref="Rock.Personalization.RequestCookieKey.ROCK_SEGMENT_FILTERS"/> cookie
    /// </summary>
    public class SegmentFilterCookieData
    {
        /// <summary>
        /// Gets or sets the person alias identifier.
        /// </summary>
        /// <value>The person alias identifier.</value>
        public int PersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Rock.Data.IEntity.IdKey" >IdKeys</see> of <see cref="Rock.Model.PersonalizationSegment">PersonalizationSegment</see> for
        /// the <see cref="Rock.Web.UI.RockPage.CurrentPerson" /> or <see cref="Rock.Web.UI.RockPage.CurrentVisitor" /> 
        /// </summary>
        /// <value>The segment identifier keys.</value>
        public string[] SegmentIdKeys { get; set; }
    }
}
