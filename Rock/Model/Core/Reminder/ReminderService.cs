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
using System.Linq;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data access class for <see cref="Rock.Model.Reminder"/> entity objects.
    /// </summary>
    public partial class ReminderService
    {
        public void RecalculateReminderCount( int personId )
        {
            var rockContext = this.Context as RockContext;
            var person = new PersonService( rockContext ).Get( personId );
            int reminderCount = GetByPerson( person.Id ).Count();
            person.ReminderCount = reminderCount;
            rockContext.SaveChanges();
        }

        /// <summary>
        /// Gets reminders by person.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Reminder> GetByPerson( int personId )
        {
            var currentDate = RockDateTime.Now;
            return this.Queryable()
                .Where( f => f.PersonAlias.PersonId == personId
                        && f.ReminderDate <= currentDate
                        && !f.IsComplete );
        }

        /// <summary>
        /// Gets reminders by entity type and person.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Reminder> GetByEntityTypeAndPerson( int entityTypeId, int personId )
        {
            return this.Queryable()
                .Where( f => f.ReminderType.EntityTypeId == entityTypeId
                    && f.PersonAlias.PersonId == personId );
        }

        /// <summary>
        /// Gets reminders by entity and person.
        /// </summary>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="personId">The person identifier.</param>
        /// <returns></returns>
        public IQueryable<Reminder> GetByEntityAndPerson( int entityTypeId, int entityId, int personId )
        {
            return this.Queryable()
                .Where( f => f.ReminderType.EntityTypeId == entityTypeId
                    && f.PersonAlias.PersonId == personId
                    && f.EntityId == entityId );
        }
    }
}
