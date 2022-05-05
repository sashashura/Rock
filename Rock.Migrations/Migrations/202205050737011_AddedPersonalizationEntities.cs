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
    public partial class AddedPersonalizationEntities : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateTable(
                "dbo.PersonAliasPersonalization",
                c => new
                    {
                        PersonAliasId = c.Int(nullable: false),
                        PersonalizationType = c.Int(nullable: false),
                        PersonalizationTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.PersonAliasId, t.PersonalizationType, t.PersonalizationTypeId });
            
            CreateTable(
                "dbo.PersonalizedEntity",
                c => new
                    {
                        EntityTypeId = c.Int(nullable: false),
                        EntityId = c.Int(nullable: false),
                        PersonalizationType = c.Int(nullable: false),
                        PersonalizationTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.EntityTypeId, t.EntityId, t.PersonalizationType, t.PersonalizationTypeId });
            
            CreateTable(
                "dbo.RequestFilter",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        RequestFilterKey = c.String(),
                        SiteId = c.Int(),
                        IsActive = c.Boolean(nullable: false),
                        FilterJson = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            CreateTable(
                "dbo.Segment",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 100),
                        SegmentKey = c.String(),
                        IsActive = c.Boolean(nullable: false),
                        FilterDataViewId = c.Int(),
                        AdditionalFilterJson = c.String(),
                        CreatedDateTime = c.DateTime(),
                        ModifiedDateTime = c.DateTime(),
                        CreatedByPersonAliasId = c.Int(),
                        ModifiedByPersonAliasId = c.Int(),
                        Guid = c.Guid(nullable: false),
                        ForeignId = c.Int(),
                        ForeignGuid = c.Guid(),
                        ForeignKey = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.PersonAlias", t => t.CreatedByPersonAliasId)
                .ForeignKey("dbo.PersonAlias", t => t.ModifiedByPersonAliasId)
                .Index(t => t.CreatedByPersonAliasId)
                .Index(t => t.ModifiedByPersonAliasId)
                .Index(t => t.Guid, unique: true);
            
            AddColumn("dbo.PersonAlias", "IsPrimaryAlias", c => c.Boolean(nullable: false));
            AddColumn("dbo.PersonAlias", "AliasedDateTime", c => c.DateTime());
            AddColumn("dbo.PersonAlias", "LastVisitDateTime", c => c.DateTime());
            AddColumn("dbo.ContentChannel", "EnablePersonalization", c => c.Boolean(nullable: false));
            AddColumn("dbo.Site", "EnableVisitorTracking", c => c.Boolean(nullable: false));
            AddColumn("dbo.Site", "EnablePersonalization", c => c.Boolean(nullable: false));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey("dbo.Segment", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.Segment", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RequestFilter", "ModifiedByPersonAliasId", "dbo.PersonAlias");
            DropForeignKey("dbo.RequestFilter", "CreatedByPersonAliasId", "dbo.PersonAlias");
            DropIndex("dbo.Segment", new[] { "Guid" });
            DropIndex("dbo.Segment", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.Segment", new[] { "CreatedByPersonAliasId" });
            DropIndex("dbo.RequestFilter", new[] { "Guid" });
            DropIndex("dbo.RequestFilter", new[] { "ModifiedByPersonAliasId" });
            DropIndex("dbo.RequestFilter", new[] { "CreatedByPersonAliasId" });
            DropColumn("dbo.Site", "EnablePersonalization");
            DropColumn("dbo.Site", "EnableVisitorTracking");
            DropColumn("dbo.ContentChannel", "EnablePersonalization");
            DropColumn("dbo.PersonAlias", "LastVisitDateTime");
            DropColumn("dbo.PersonAlias", "AliasedDateTime");
            DropColumn("dbo.PersonAlias", "IsPrimaryAlias");
            DropTable("dbo.Segment");
            DropTable("dbo.RequestFilter");
            DropTable("dbo.PersonalizedEntity");
            DropTable("dbo.PersonAliasPersonalization");
        }
    }
}
