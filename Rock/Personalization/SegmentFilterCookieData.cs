using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Utility;

namespace Rock.Personalization
{
    /// <summary>
    /// SegmentIdKeys for the current person/visitor session stored in the <seealso cref="Rock.Personalization.RequestCookieKey.ROCK_SEGMENT_FILTERS"/> cookie
    /// </summary>
    public class SegmentFilterCookieData
    {
        /// <summary>
        /// Gets or sets the IdKey of the Person Alias.
        /// </summary>
        /// <value>The person alias identifier.</value>
        public string PersonAliasIdKey { get; set; }

        /// <summary>
        /// Determines whether the <paramref name="otherPersonAliasId"/> is the same PersonAliasId that is embedded in <see cref="PersonAliasIdKey"/>.
        /// </summary>
        /// <param name="otherPersonAliasId">The other person alias identifier.</param>
        /// <returns><c>true</c> if [is same person alias] [the specified other person alias identifier]; otherwise, <c>false</c>.</returns>
        public bool IsSamePersonAlias( int otherPersonAliasId )
        {
            var thisPersonAliasId = IdHasher.Instance.GetId( PersonAliasIdKey ?? string.Empty );
            return thisPersonAliasId.HasValue && thisPersonAliasId.Value == otherPersonAliasId;
        }

        /// <summary>
        /// Gets or sets the list of <see cref="Rock.Data.IEntity.IdKey" >IdKeys</see> of <see cref="Rock.Model.PersonalizationSegment">PersonalizationSegment</see> for
        /// the <see cref="Rock.Web.UI.RockPage.CurrentPerson" /> or <see cref="Rock.Web.UI.RockPage.CurrentVisitor" /> 
        /// </summary>
        /// <value>The segment identifier keys.</value>
        public string[] SegmentIdKeys { get; set; }

        /// <summary>
        /// Gets the segment ids converted from the stored <see cref="SegmentIdKeys"/>
        /// </summary>
        /// <returns>System.Int32[].</returns>
        public int[] GetSegmentIds()
        {
            return SegmentIdKeys.Select( s => IdHasher.Instance.GetId( s ) ).Where( a => a.HasValue ).Select( s => s.Value ).ToArray();
        }
    }
}
