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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;

namespace Rock.Model
{
    /// <summary>
    /// The personalized entity
    /// </summary>
    [RockDomain( "CRM" )]
    [Table( "PersonalizedEntity" )]
    [DataContract]
    public class PersonalizedEntity
    {
        /// <summary>
        /// Gets or sets the entity type identifier.
        /// </summary>
        /// <value>
        /// The entity type identifier.
        /// </value>
        [DataMember]
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public int EntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        [DataMember]
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public int EntityId { get; set; }

        /// <summary>
        /// Gets or sets the type of the personalization.
        /// </summary>
        /// <value>
        /// The type of the personalization.
        /// </value>
        [DataMember]
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public PersonalizationType PersonalizationType { get; set; }

        /// <summary>
        /// Gets or sets the personalization type identifier.
        /// </summary>
        /// <value>
        /// The personalization type identifier.
        /// </value>
        [DataMember]
        [Key]
        [DatabaseGenerated( DatabaseGeneratedOption.None )]
        public int PersonalizationTypeId { get; set; }

        #region Entity Configuration

        /// <summary>
        /// PersonalizedEntity Configuration class.
        /// </summary>
        public partial class PersonalizedEntityConfiguration : EntityTypeConfiguration<PersonalizedEntity>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="PersonalizedEntityConfiguration"/> class.
            /// </summary>
            public PersonalizedEntityConfiguration()
            {
                HasKey( a => new { a.EntityTypeId, a.EntityId, a.PersonalizationType, a.PersonalizationTypeId } );
            }
        }

        #endregion
    }
}
