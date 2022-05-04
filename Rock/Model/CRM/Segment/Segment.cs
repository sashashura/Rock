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
using Rock.Data;
using Rock.Web.Cache;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// Segment Entity
    /// </summary>
    /// <seealso cref="Data.Model{TEntity}" />
    /// <seealso cref="ICacheable" />
    [RockDomain( "CRM" )]
    [Table( "Segment" )]
    [DataContract]
    public class Segment : Model<Segment>
    {
        #region Entity Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        [MaxLength( 100 )]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the segment key.
        /// </summary>
        /// <value>
        /// The segment key.
        /// </value>
        [DataMember]
        public string SegmentKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the filter data view identifier.
        /// </summary>
        /// <value>
        /// The filter data view identifier.
        /// </value>
        [DataMember]
        public int? FilterDataViewId { get; set; }

        /// <summary>
        /// Gets or sets the additional filter json.
        /// </summary>
        /// <value>
        /// The additional filter json.
        /// </value>
        [DataMember]
        public string AdditionalFilterJson { get; set; }

        #endregion
    }
}
