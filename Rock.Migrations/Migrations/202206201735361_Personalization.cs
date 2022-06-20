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
    public partial class Personalization : Rock.Migrations.RockMigration
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
                    PersonAliasId = c.Int( nullable: false ),
                    PersonalizationType = c.Int( nullable: false ),
                    PersonalizationTypeId = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.PersonAliasId, t.PersonalizationType, t.PersonalizationTypeId } )
                .ForeignKey( "dbo.PersonAlias", t => t.PersonAliasId )
                .Index( t => t.PersonAliasId );

            CreateTable(
                "dbo.PersonalizedEntity",
                c => new
                {
                    EntityTypeId = c.Int( nullable: false ),
                    EntityId = c.Int( nullable: false ),
                    PersonalizationType = c.Int( nullable: false ),
                    PersonalizationTypeId = c.Int( nullable: false ),
                } )
                .PrimaryKey( t => new { t.EntityTypeId, t.EntityId, t.PersonalizationType, t.PersonalizationTypeId } )
                .ForeignKey( "dbo.EntityType", t => t.EntityTypeId )
                .Index( t => t.EntityTypeId );

            CreateTable(
                "dbo.RequestFilter",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( maxLength: 100 ),
                    RequestFilterKey = c.String(),
                    SiteId = c.Int(),
                    IsActive = c.Boolean( nullable: false ),
                    FilterJson = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .ForeignKey( "dbo.Site", t => t.SiteId )
                .Index( t => t.SiteId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            CreateTable(
                "dbo.PersonalizationSegment",
                c => new
                {
                    Id = c.Int( nullable: false, identity: true ),
                    Name = c.String( maxLength: 100 ),
                    SegmentKey = c.String(),
                    IsActive = c.Boolean( nullable: false ),
                    FilterDataViewId = c.Int(),
                    AdditionalFilterJson = c.String(),
                    CreatedDateTime = c.DateTime(),
                    ModifiedDateTime = c.DateTime(),
                    CreatedByPersonAliasId = c.Int(),
                    ModifiedByPersonAliasId = c.Int(),
                    Guid = c.Guid( nullable: false ),
                    ForeignId = c.Int(),
                    ForeignGuid = c.Guid(),
                    ForeignKey = c.String( maxLength: 100 ),
                } )
                .PrimaryKey( t => t.Id )
                .ForeignKey( "dbo.PersonAlias", t => t.CreatedByPersonAliasId )
                .ForeignKey( "dbo.DataView", t => t.FilterDataViewId )
                .ForeignKey( "dbo.PersonAlias", t => t.ModifiedByPersonAliasId )
                .Index( t => t.FilterDataViewId )
                .Index( t => t.CreatedByPersonAliasId )
                .Index( t => t.ModifiedByPersonAliasId )
                .Index( t => t.Guid, unique: true );

            AddColumn( "dbo.PersonAlias", "AliasedDateTime", c => c.DateTime() );
            AddColumn( "dbo.PersonAlias", "LastVisitDateTime", c => c.DateTime() );
            AddColumn( "dbo.ContentChannel", "EnablePersonalization", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Site", "EnableVisitorTracking", c => c.Boolean( nullable: false ) );
            AddColumn( "dbo.Site", "EnablePersonalization", c => c.Boolean( nullable: false ) );


            RockMigrationHelper.UpdateDefinedValue( "26BE73A6-A9C5-4E94-AE00-3AFDCF8C9275", "Anonymous Visitor", "An Anonymous Visitor", "80007453-30A7-453C-BF0B-C82AAFE2BA12", true );

            AddAnonymousVisitor_Up();

            UpdatePersonAliasAliasPersonIdIndex_Up();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropForeignKey( "dbo.PersonalizationSegment", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalizationSegment", "FilterDataViewId", "dbo.DataView" );
            DropForeignKey( "dbo.PersonalizationSegment", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.RequestFilter", "SiteId", "dbo.Site" );
            DropForeignKey( "dbo.RequestFilter", "ModifiedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.RequestFilter", "CreatedByPersonAliasId", "dbo.PersonAlias" );
            DropForeignKey( "dbo.PersonalizedEntity", "EntityTypeId", "dbo.EntityType" );
            DropForeignKey( "dbo.PersonAliasPersonalization", "PersonAliasId", "dbo.PersonAlias" );
            DropIndex( "dbo.PersonalizationSegment", new[] { "Guid" } );
            DropIndex( "dbo.PersonalizationSegment", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.PersonalizationSegment", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.PersonalizationSegment", new[] { "FilterDataViewId" } );
            DropIndex( "dbo.RequestFilter", new[] { "Guid" } );
            DropIndex( "dbo.RequestFilter", new[] { "ModifiedByPersonAliasId" } );
            DropIndex( "dbo.RequestFilter", new[] { "CreatedByPersonAliasId" } );
            DropIndex( "dbo.RequestFilter", new[] { "SiteId" } );
            DropIndex( "dbo.PersonalizedEntity", new[] { "EntityTypeId" } );
            DropIndex( "dbo.PersonAliasPersonalization", new[] { "PersonAliasId" } );
            DropColumn( "dbo.Site", "EnablePersonalization" );
            DropColumn( "dbo.Site", "EnableVisitorTracking" );
            DropColumn( "dbo.ContentChannel", "EnablePersonalization" );
            DropColumn( "dbo.PersonAlias", "LastVisitDateTime" );
            DropColumn( "dbo.PersonAlias", "AliasedDateTime" );
            DropTable( "dbo.PersonalizationSegment" );
            DropTable( "dbo.RequestFilter" );
            DropTable( "dbo.PersonalizedEntity" );
            DropTable( "dbo.PersonAliasPersonalization" );

            UpdatePersonAliasAliasPersonIdIndex_Down();
            AddAnonymousVisitor_Down();
        }

        private void UpdatePersonAliasAliasPersonIdIndex_Up()
        {
            // Delete this index because it a unique constraint that includes the NULL value, so only one NULL allowed */
            RockMigrationHelper.DropIndexIfExists( "PersonAlias", "IX_AliasPersonId" );
            Sql( @"
/* This is a 'filtered' unique constraint that excludes NULL value, so we can have as many nulls as we want.*/
CREATE UNIQUE NONCLUSTERED INDEX[IX_AliasPersonId] ON[dbo].[PersonAlias]
(

    [AliasPersonId] ASC
) WHERE[AliasPersonId] IS NOT NULL" );
        }

        private void UpdatePersonAliasAliasPersonIdIndex_Down()
        {
            // Recreate index as it was before this migration
            RockMigrationHelper.DropIndexIfExists( "PersonAlias", "IX_AliasPersonId" );
            Sql( @"
/* This is a 'filtered' unique constraint that excludes NULL value, so we can have as many nulls as we want.*/
CREATE UNIQUE NONCLUSTERED INDEX[IX_AliasPersonId] ON[dbo].[PersonAlias]
(

    [AliasPersonId] ASC
)" );
        }

        private void AddAnonymousVisitor_Up()
        {
            Sql( $@"
DECLARE @personRecordType INT = ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_RECORD_TYPE_PERSON}' ),
		@connectionStatusValueId INT = ( SELECT [Id] FROM [DefinedValue] WHERE [Guid] = '{SystemGuid.DefinedValue.PERSON_CONNECTION_STATUS_PARTICIPANT}'),
		@recordStatusValueId INT = (select [Id] from [DefinedValue] where [Guid] = '{SystemGuid.DefinedValue.PERSON_RECORD_STATUS_ACTIVE}'),
		@personId INT,
		@groupId INT,
		@personGuid UNIQUEIDENTIFIER = '{SystemGuid.Person.ANONYMOUS_VISITOR}',
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
    ,[GroupTypeId]
    )
VALUES (
    0
    ,@groupId
    ,@personId
    ,@adultRole
    ,newid()
    ,1
	,SYSDATETIME()
    ,@familyGroupType
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
        }

        private void AddAnonymousVisitor_Down()
        {
            Sql( $@"UPDATE Person
SET PrimaryFamilyId = NULL
WHERE [Guid] = '{SystemGuid.Person.ANONYMOUS_VISITOR}'

DELETE
FROM [Group]
WHERE Id IN (
        SELECT GroupId
        FROM GroupMember
        WHERE Personid IN (
                SELECT Id
                FROM Person
                WHERE Guid = '{SystemGuid.Person.ANONYMOUS_VISITOR}'
                )
        )

DELETE
FROM PersonAlias
WHERE PersonId IN (
        SELECT Id
        FROM Person
        WHERE Guid = '{SystemGuid.Person.ANONYMOUS_VISITOR}'
        )

DELETE
FROM Person
WHERE [Guid] = '{SystemGuid.Person.ANONYMOUS_VISITOR}'
" );
        }
    }
}
