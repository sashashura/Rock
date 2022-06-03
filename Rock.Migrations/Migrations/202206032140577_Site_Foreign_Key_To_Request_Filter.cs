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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class Site_Foreign_Key_To_Request_Filter : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateIndex("dbo.PersonAliasPersonalization", "PersonAliasId");
            CreateIndex("dbo.RequestFilter", "SiteId");
            AddForeignKey("dbo.PersonAliasPersonalization", "PersonAliasId", "dbo.PersonAlias", "Id");
            AddForeignKey("dbo.RequestFilter", "SiteId", "dbo.Site", "Id");
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.RequestFilter", "SiteId", "dbo.Site");
            DropForeignKey("dbo.PersonAliasPersonalization", "PersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.RequestFilter", new[] { "SiteId" });
            DropIndex("dbo.PersonAliasPersonalization", new[] { "PersonAliasId" });
        }
    }
}
