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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Quartz.IJob" />
    [DisplayName( "Rock Update Helper v13.0 - Add Missing Indexes" )]
    [Description( "Add some missing indexes. A few PersonAliasId indexes, etc." )]

    [DisallowConcurrentExecution]
    [IntegerField(
        "Command Timeout",
        Description = "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the default for this job (3600). Note, it could take several minutes, so you might want to set it at 3600 (60 minutes) or higher",
        IsRequired = false,
        DefaultIntegerValue = AttributeDefaults.CommandTimeout,
        Category = "General",
        Order = 1,
        Key = AttributeKey.CommandTimeout )]
    public class PostV130AddMissingIndexes : IJob
    {
        #region Keys

        /// <summary>
        /// Attribute Keys
        /// </summary>
        private static class AttributeKey
        {
            /// <summary>
            /// The command timeout
            /// </summary>
            public const string CommandTimeout = "CommandTimeout";
        }

        /// <summary>
        /// Attribute value defaults
        /// </summary>
        private static class AttributeDefaults
        {
            /// <summary>
            /// The command timeout
            /// </summary>
            public const int CommandTimeout = 60 * 60;
        }

        #endregion Keys

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;

            // get the configured timeout, or default to 60 minutes if it is blank
            var commandTimeout = dataMap.GetString( AttributeKey.CommandTimeout ).AsIntegerOrNull() ?? AttributeDefaults.CommandTimeout;
            var jobMigration = new JobMigration( commandTimeout );
            var migrationHelper = new MigrationHelper( jobMigration );

            AddMissingUniqueGuidIndexOnTableIfNotExists( context, jobMigration, migrationHelper, "Attendance" );
            AddMissingUniqueGuidIndexOnTableIfNotExists( context, jobMigration, migrationHelper, "AuditDetail" );
            AddMissingUniqueGuidIndexOnTableIfNotExists( context, jobMigration, migrationHelper, "History" );
            AddMissingUniqueGuidIndexOnTableIfNotExists( context, jobMigration, migrationHelper, "PersonDuplicate" );


            /*
Attendance	            CreatedByPersonAliasId
Attendance	            ModifiedByPersonAliasId
History	                CreatedByPersonAliasId
History	                ModifiedByPersonAliasId
PersonDuplicate	        CreatedByPersonAliasId
PersonDuplicate	        ModifiedByPersonAliasId
             */

            migrationHelper.CreateIndexIfNotExists( "Attendance", new string[1] { "CreatedByPersonAliasId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "Attendance", new string[1] { "ModifiedByPersonAliasId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "History", new string[1] { "CreatedByPersonAliasId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "History", new string[1] { "ModifiedByPersonAliasId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "PersonDuplicate", new string[1] { "CreatedByPersonAliasId" }, new string[0] );
            migrationHelper.CreateIndexIfNotExists( "PersonDuplicate", new string[1] { "ModifiedByPersonAliasId" }, new string[0] );


            migrationHelper.CreateIndexIfNotExists( "PageShortLink", new string[1] { "Token " }, new string[0] );


            ServiceJobService.DeleteJob( context.GetJobId() );
        }

        private static void AddMissingUniqueGuidIndexOnTableIfNotExists( IJobExecutionContext context, JobMigration jobMigration, MigrationHelper migrationHelper, string tableName )
        {
            // fix up any duplicate Guids in the specific table
            context.UpdateLastStatusMessage( $"Ensuring {tableName} Guids are unique..." );
            jobMigration.Sql( $@"UPDATE [{tableName}]
SET [Guid] = newid()
WHERE [Guid] IN (
        SELECT [Guid]
        FROM (
            SELECT [Guid]
                , count(*) [DuplicateCount]
            FROM [{tableName}]
            GROUP BY [Guid]
            ) x
        WHERE [DuplicateCount] > 1
        )" );

            context.UpdateLastStatusMessage( $"Creating {tableName} IX_GUID index" );
            migrationHelper.CreateIndexIfNotExists( "Attendance", "IX_Guid", new string[1] { "Guid" }, new string[0], true );
        }

        
    }
}
