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
using System.Collections.Generic;
using System.Data.Entity.SqlServer;
using System.Linq;
using Rock.Data;
using Rock.Model;

namespace Rock.Tasks
{
    /// <summary>
    /// Updates a group members status. Will also recalculate group member requirements when a member is added
    /// </summary>
    public sealed class UpdateGroupMembersStatus : BusStartedTask<UpdateGroupMembersStatus.Message>
    {
        /// <summary>
        /// Executes this instance.
        /// </summary>
        /// <param name="message"></param>
        public override void Execute( Message message )
        {
            using ( var rockContext = new RockContext() )
            {
                var groupMemberQuery = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == message.GroupId );

                var groupMemberIds = new List<int>();
                GroupMemberStatus groupMemberStatus = GroupMemberStatus.Inactive;
                if ( message.GroupMemberStatus == GroupMemberStatus.Inactive )
                {
                    groupMemberIds = groupMemberQuery.Where( gm => gm.GroupMemberStatus != GroupMemberStatus.Inactive ).Select( gm => gm.Id ).ToList();
                }
                else if ( message.OriginalInactiveDateTime.HasValue )
                {
                    groupMemberIds = groupMemberQuery
                        .Where( a => a.GroupMemberStatus == GroupMemberStatus.Inactive
                        && a.InactiveDateTime.HasValue
                        && Math.Abs( SqlFunctions.DateDiff( "hour", a.InactiveDateTime.Value, message.OriginalInactiveDateTime.Value ).Value ) < 24 ).Select( gm => gm.Id ).ToList();
                    groupMemberStatus = GroupMemberStatus.Active;
                }

                foreach ( var groupMemberId in groupMemberIds )
                {
                    try
                    {
                        // Use a new context so any validation exception may not halt the whole process
                        using ( var groupMemberContext = new RockContext() )
                        {
                            // Delete the records for that person's group and role.
                            // NOTE: just in case there are duplicate records, delete all group member records for that person and role
                            var groupMember = new GroupMemberService( groupMemberContext ).Get( groupMemberId );
                            groupMember.GroupMemberStatus = groupMemberStatus;
                            groupMember.InactiveDateTime = message.NewInactiveDateTime;
                            groupMemberContext.SaveChanges();
                        }
                    }
                    catch ( Exception ex )
                    {
                        ExceptionLogService.LogException( ex );
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : BusStartedTaskMessage
        {
            /// <summary>
            /// Gets or sets the original inactive dateTime.
            /// </summary>
            /// <value>
            /// The original inactive dateTime.
            /// </value>
            public DateTime? OriginalInactiveDateTime { get; set; }

            /// <summary>
            /// Gets or sets the group identifier.
            /// </summary>
            /// <value>
            /// The group identifier.
            /// </value>
            public int GroupId { get; set; }

            /// <summary>
            /// Gets or sets the new inactive dateTime.
            /// </summary>
            /// <value>
            /// The new inactive dateTime.
            /// </value>
            public DateTime? NewInactiveDateTime { get; set; }

            /// <summary>
            /// Gets or sets the group member status.
            /// </summary>
            /// <value>
            /// The group member status.
            /// </value>
            public GroupMemberStatus GroupMemberStatus { get; set; }
        }
    }
}