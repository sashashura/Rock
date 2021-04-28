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
    public partial class AddAzureBlobStorageProvider : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            CreateAzureStorageProvider();
        }

        private void CreateAzureStorageProvider()
        {
            Sql( @"
INSERT [dbo].[EntityType] ([Name], [AssemblyName], [FriendlyName], [IsEntity], [IsSecured], [IsCommon], [Guid], [ForeignKey], [SingleValueFieldTypeId], [MultiValueFieldTypeId], [ForeignGuid], [ForeignId], [IsIndexingEnabled], [IndexResultTemplate], [IndexDocumentUrl], [LinkUrlLavaTemplate], [AttributesSupportPrePostHtml], [AttributesSupportShowOnBulk]) VALUES (N'Rock.Storage.Provider.AzureBlobStorage', N'Rock.Storage.Provider.AzureBlobStorage, Rock, Version=1.13.0.10, Culture=neutral, PublicKeyToken=null', N'FileSystem', 0, 1, 0, N'9925a20a-7262-4fc7-b86e-856f6d98be17', NULL, NULL, NULL, NULL, NULL, 0, NULL, NULL, NULL, 0, 0);

DECLARE @BinaryFileEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '62AF597F-F193-412B-94EA-291CF713327D');
DECLARE @AzureBlobStorageEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '9925A20A-7262-4FC7-B86E-856F6D98BE17');
DECLARE @FieldTypeId int = (SELECT TOP 1 [Id] FROM [FieldType] WHERE [Guid] = '9C204CD0-1233-41C5-818A-C5DA439445AA');

INSERT [dbo].[Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [IconCssClass], [AllowSearch], [ForeignGuid], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory], [IsActive], [EnableHistory], [PreHtml], [PostHtml], [AbbreviatedName], [ShowOnBulk], [IsPublic]) VALUES (1, @FieldTypeId, @BinaryFileEntityTypeId, N'StorageEntityTypeId', @AzureBlobStorageEntityTypeId, N'AzureBlobContainerName', N'Azure Blob Container Name', N'The Azure Blob Storage container name to use for files of this type.', 0, 0, N'', 0, 0, N'5d921dde-623a-4079-b987-25c74b4cdb7b', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0);
INSERT [dbo].[Attribute] ([IsSystem], [FieldTypeId], [EntityTypeId], [EntityTypeQualifierColumn], [EntityTypeQualifierValue], [Key], [Name], [Description], [Order], [IsGridColumn], [DefaultValue], [IsMultiValue], [IsRequired], [Guid], [CreatedDateTime], [ModifiedDateTime], [CreatedByPersonAliasId], [ModifiedByPersonAliasId], [ForeignKey], [IconCssClass], [AllowSearch], [ForeignGuid], [IsIndexEnabled], [IsAnalytic], [IsAnalyticHistory], [IsActive], [EnableHistory], [PreHtml], [PostHtml], [AbbreviatedName], [ShowOnBulk], [IsPublic]) VALUES (1, @FieldTypeId, @BinaryFileEntityTypeId, N'StorageEntityTypeId', @AzureBlobStorageEntityTypeId, N'AzureBlobContainerFolderPath', N'Azure Blob Container Folder Path', N'An optional folder path inside the container to use for files of this type.', 0, 0, N'', 0, 0, N'ba7c28e6-b45e-4983-8a8d-96985e2c4ef4', NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL, 0, 0, 0, 1, 0, NULL, NULL, NULL, 0, 0);

" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
