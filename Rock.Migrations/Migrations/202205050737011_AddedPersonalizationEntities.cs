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

            Sql( @"
               WITH CTE AS
                (SELECT
	                    [Id], [IsPrimaryAlias], ROW_NUMBER() OVER (PARTITION BY [PersonId] ORDER BY [Id], case when AliasPersonId = PersonId then 0 else 1 end) as RecordNumber 
                    FROM 
	                [PersonAlias] )

                UPDATE CTE
                SET [IsPrimaryAlias] = 1
                WHERE RecordNumber = 1
                " );

            RockMigrationHelper.UpdateDefinedValue( "26BE73A6-A9C5-4E94-AE00-3AFDCF8C9275", "Anonymous Visitor", "An Anonymous Visitor", "80007453-30A7-453C-BF0B-C82AAFE2BA12", true );

            Sql( @"
DECLARE @personRecordType INT = ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '36CF10D6-C695-413D-8E7C-4546EFEF385E' ),
		@connectionStatusValueId INT = ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '8EBC0CEB-474D-4C1B-A6BA-734C3A9AB061'),
		@recordStatusValueId INT = (select [Id] from [DefinedValue] where [Guid] = '618F906C-C33D-4FA3-8AEF-E58CB7B63F1E'),
		@personId INT,
		@groupId INT,
		@personGuid UNIQUEIDENTIFIER = '7EBC167B-512D-4683-9D80-98B6BB02E1B9',
		@familyGroupType INT = ( SELECT [Id] FROM [GroupType] WHERE [Guid] = '790E3215-3B10-442B-AF69-616C0DCB998E' ),
		@adultRole INT = ( SELECT [Id] FROM [GroupTypeRole] WHERE [Guid] = '2639F9A5-2AAE-4E48-A8C3-4FFE86681E42' )

INSERT INTO [Person] (
    [IsSystem]
    ,[FirstName]
    ,[NickName]
    ,[LastName]
    ,[Gender]
    ,[AgeClassification]
    ,[IsEmailActive]
    ,[EmailPreference]
    ,[Guid]
    ,[RecordTypeValueId]
    ,[RecordStatusValueId]
    ,[ConnectionStatusValueId]
	,[IsDeceased]
    ,[CreatedDateTime]
    )
VALUES (
    1
    ,'Anonymous'
    ,'Anonymous'
    ,'Visitor'
    ,0
    ,1
    ,1
    ,0
    ,@personGuid
    ,@personRecordType
    ,@recordStatusValueId
    ,@connectionStatusValueId
	,0
    ,SYSDATETIME()
    )

SET @personId = SCOPE_IDENTITY()

INSERT INTO [PersonAlias] (
    PersonId
    ,AliasPersonId
    ,AliasPersonGuid
    ,[Guid]
    )
VALUES (
    @personId
    ,@personId
    ,@personGuid
    ,NEWID()
    );

declare @randomCampusId int = (select top 1 Id from Campus order by newid())

-- create family
INSERT INTO [Group] (
    IsSystem
    ,GroupTypeId
    ,NAME
    ,IsSecurityRole
    ,IsActive
    ,CampusId
    ,[Guid]
    ,[Order]
    )
VALUES (
    0
    ,@familyGroupType
    ,'Anonymous Visitor Family'
    ,0
    ,1
    ,@randomCampusId
    ,NEWID()
    ,0
    )

SET @groupId = SCOPE_IDENTITY()

INSERT INTO [GroupMember] (
    IsSystem
    ,GroupId
    ,PersonId
    ,GroupRoleId
    ,[Guid]
    ,GroupMemberStatus
	,DateTimeAdded
    )
VALUES (
    0
    ,@groupId
    ,@personId
    ,@adultRole
    ,newid()
    ,1
	,SYSDATETIME()
    )


	UPDATE Person
        SET GivingId = (
		        CASE 
			        WHEN [GivingGroupId] IS NOT NULL
				        THEN 'G' + CONVERT([varchar], [GivingGroupId])
			        ELSE 'P' + CONVERT([varchar], [Id])
			        END
		        )
        WHERE GivingId IS NULL OR GivingId != (
		        CASE 
			        WHEN [GivingGroupId] IS NOT NULL
				        THEN 'G' + CONVERT([varchar], [GivingGroupId])
			        ELSE 'P' + CONVERT([varchar], [Id])
			        END
		        )

        UPDATE x
        SET x.PrimaryFamilyId = x.CalculatedPrimaryFamilyId
            ,x.PrimaryCampusId = x.CalculatedPrimaryCampusId
        FROM (
            SELECT p.Id
                ,p.NickName
                ,p.LastName
                ,p.PrimaryFamilyId
                ,p.PrimaryCampusId
                ,pf.CalculatedPrimaryFamilyId
                ,pf.CalculatedPrimaryCampusId
            FROM Person p
            OUTER APPLY (
                SELECT TOP 1
                    g.Id [CalculatedPrimaryFamilyId]
                    ,g.CampusId [CalculatedPrimaryCampusId]
                FROM GroupMember gm
                JOIN [Group] g ON g.Id = gm.GroupId
                WHERE g.GroupTypeId = @familyGroupType
                    AND gm.PersonId = p.Id
                ORDER BY gm.GroupOrder
                    ,gm.GroupId
                ) pf
            WHERE (
                    (ISNULL(p.PrimaryFamilyId, 0) != ISNULL(pf.CalculatedPrimaryFamilyId, 0))
                    OR (ISNULL(p.PrimaryCampusId, 0) != ISNULL(pf.CalculatedPrimaryCampusId, 0))
                    ) ) x

            UPDATE x
            SET x.GivingLeaderId = x.CalculatedGivingLeaderId
            FROM (
	            SELECT p.Id
		            ,p.NickName
		            ,p.LastName
		            ,p.GivingLeaderId
		            ,isnull(pf.CalculatedGivingLeaderId, p.Id) CalculatedGivingLeaderId
	            FROM Person p
	            OUTER APPLY (
		            SELECT TOP 1 p2.[Id] CalculatedGivingLeaderId
		            FROM [GroupMember] gm
		            INNER JOIN [GroupTypeRole] r ON r.[Id] = gm.[GroupRoleId]
		            INNER JOIN [Person] p2 ON p2.[Id] = gm.[PersonId]
		            WHERE gm.[GroupId] = p.GivingGroupId
			            AND p2.[IsDeceased] = 0
			            AND p2.[GivingGroupId] = p.GivingGroupId
		            ORDER BY r.[Order]
			            ,p2.[Gender]
			            ,p2.[BirthYear]
			            ,p2.[BirthMonth]
			            ,p2.[BirthDay]
		            ) pf
	            WHERE (
			            p.GivingLeaderId IS NULL
			            OR (p.GivingLeaderId != pf.CalculatedGivingLeaderId)
			            )) x

" );

            Sql( MigrationSQL._202205050737011_spCrm_PersonMerge );
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
