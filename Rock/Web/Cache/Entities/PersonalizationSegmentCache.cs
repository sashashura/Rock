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
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;
using Rock.Personalization;
using System.Linq.Expressions;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a Segment
    /// </summary>
    [Serializable]
    [DataContract]
    public class PersonalizationSegmentCache : ModelCache<PersonalizationSegmentCache, PersonalizationSegment>
    {
        #region Properties

        /// <inheritdoc cref="PersonalizationSegment.Name"/>
        [DataMember]
        public string Name { get; private set; }

        /// <inheritdoc cref="PersonalizationSegment.SegmentKey"/>
        [DataMember]
        public string SegmentKey { get; private set; }

        /// <inheritdoc cref="PersonalizationSegment.IsActive"/>
        [DataMember]
        public bool IsActive { get; private set; }

        /// <inheritdoc cref="PersonalizationSegment.FilterDataViewId"/>
        [DataMember]
        public int? FilterDataViewId { get; private set; }

        /// <inheritdoc cref="PersonalizationSegment.AdditionalFilterJson"/>
        [DataMember]
        public string AdditionalFilterJson
        {
            get => AdditionalFilterConfiguration?.ToJson();
            private set => AdditionalFilterConfiguration = value?.FromJsonOrNull<PersonalizationSegmentAdditionalFilterConfiguration>();
        }

        /// <inheritdoc cref="PersonalizationSegment.AdditionalFilterConfiguration"/>
        public PersonalizationSegmentAdditionalFilterConfiguration AdditionalFilterConfiguration { get; private set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the person alias filters where expression.
        /// </summary>
        /// <param name="personAliasService">The person alias service.</param>
        /// <param name="parameterExpression">The parameter expression.</param>
        /// <returns>Expression.</returns>
        public Expression GetPersonAliasFiltersWhereExpression( PersonAliasService personAliasService, ParameterExpression parameterExpression )
        {
            return PersonalizationSegment.GetPersonAliasFiltersWhereExpression( this.Id, personAliasService, parameterExpression );
        }

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
