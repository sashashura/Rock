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
using Rock.Model;
using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a Segment
    /// </summary>
    [Serializable]
    [DataContract]
    public class SegmentCache : ModelCache<SegmentCache, PersonalizationSegment>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the segment key.
        /// </summary>
        /// <value>
        /// The segment key.
        /// </value>
        [DataMember]
        public string SegmentKey { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <summary>
        /// Gets or sets the filter data view identifier.
        /// </summary>
        /// <value>
        /// The filter data view identifier.
        /// </value>
        [DataMember]
        public int? FilterDataViewId { get; private set; }

        /// <summary>
        /// Gets or sets the additional filter json.
        /// </summary>
        /// <value>
        /// The additional filter json.
        /// </value>
        [DataMember]
        public string AdditionalFilterJson { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            if ( entity is PersonalizationSegment segment )
            {
                Name = segment.Name;
                SegmentKey = segment.SegmentKey;
                IsActive = segment.IsActive;
                FilterDataViewId = segment.FilterDataViewId;
                AdditionalFilterJson = segment.AdditionalFilterJson;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion Public Methods
    }
}
