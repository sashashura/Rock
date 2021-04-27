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
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.Composition;

namespace Rock.Storage.Provider
{
    /// <summary>
    /// Storage provider for Azure Blob storage
    /// </summary>
    [Description( "Azure Blob Storage" )]
    [Export( typeof( ProviderComponent ) )]
    [ExportMetadata( "ComponentName", "Azure Blob Storage" )]

    #region Storage Provider Attributes

    [TextField( "Account Name",
        Description = "The Azure account name.",
        IsRequired = true,
        Order = 1 )]

    [TextField("Account Key",
        Description = "The Azure account key.",
        IsRequired = true,
        Order = 2 )]

    [UrlLinkField( "Custom Domain",
        Description = "If you have configured the Azure container with a custom domain name that you'd like to use, set that value here (e.g. 'http://storage.yourorganization.com').",
        IsRequired = false,
        Order = 3 )]

    [TextField( "Default Container Name",
        Description = "The default Azure blob container to use for file types that do not provide their own.",
        IsRequired = true,
        Order = 4 )]

    #endregion Storage Provider Attributes

    public class AzureBlobStorage : ProviderComponent
    {
        private readonly object __lockObj = new object();

        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        public override void SaveContent( BinaryFile binaryFile )
        {
            SaveContent( binaryFile, out _ );
        }

        /// <summary>
        /// Saves the file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        /// <param name="fileSize">Size of the file.</param>
        public override void SaveContent( BinaryFile binaryFile, out long? fileSize )
        {
            fileSize = 0;

            var blob = GetBlob( binaryFile );
            if ( blob != null )
            {
                blob.StreamWriteSizeInBytes = 256 * 1024;
                blob.Properties.ContentType = binaryFile.MimeType;

                TimeSpan backOffPeriod = TimeSpan.FromSeconds( 2 );
                int retryCount = 1;
                var options = new BlobRequestOptions
                {
                    SingleBlobUploadThresholdInBytes = 1024 * 1024,
                    ParallelOperationThreadCount = 1,
                    RetryPolicy = new ExponentialRetry( backOffPeriod, retryCount )
                };

                using ( var stream = binaryFile.ContentStream )
                {
                    var bytes = stream.ReadBytesToEnd();
                    fileSize = bytes.Length;
                    blob.UploadFromByteArray( bytes, 0, bytes.Length, null, options );
                }
            }
        }

        /// <summary>
        /// Removes the file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        public override void DeleteContent( BinaryFile binaryFile )
        {
            var blob = GetBlob( binaryFile );
            if ( blob != null && !blob.IsDeleted && blob.Exists() )
            {
                blob.Delete();
            }
        }

        /// <summary>
        /// Gets the content stream of a file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        /// <returns></returns>
        public override System.IO.Stream GetContentStream( BinaryFile binaryFile )
        {
            var blob = GetBlob( binaryFile );
            if ( blob == null || !blob.Exists() )
            {
                return null;
            }

            return blob.OpenRead();
        }

        /// <summary>
        /// Gets the path of a file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        /// <returns></returns>
        public override string GetPath( BinaryFile binaryFile )
        {
            if ( binaryFile != null && ( binaryFile.BinaryFileType == null || !binaryFile.BinaryFileType.RequiresViewSecurity ) )
            {
                var blob = GetBlob( binaryFile );
                if ( blob != null )
                {
                    return blob.Uri.AbsoluteUri;
                }
            }

            return base.GetPath( binaryFile );
        }

        /// <summary>
        /// Gets the URL of a file.
        /// </summary>
        /// <param name="binaryFile">The file.</param>
        /// <returns></returns>
        public override string GetUrl( BinaryFile binaryFile )
        {
            return binaryFile.Path;
        }

        /// <summary>
        /// Gets the BLOB.
        /// </summary>
        /// <param name="binaryFile">The file.</param>
        /// <returns></returns>
        private CloudBlockBlob GetBlob( BinaryFile binaryFile )
        {
            if ( binaryFile == null )
            {
                return null;
            }

            FileTypeSettings settings;
            if ( binaryFile.StorageSettings != null && binaryFile.StorageSettings.ContainsKey( "AzureBlobContainerName" ) && binaryFile.StorageSettings.ContainsKey( "AzureBlobContainerFolderPath" ) )
            {
                settings = new FileTypeSettings
                {
                    ContainerName = binaryFile.StorageSettings["AzureBlobContainerName"],
                    Folder = binaryFile.StorageSettings["AzureBlobContainerFolderPath"]
                };
            }
            else
            {
                settings = GetSettingsFromFileType( binaryFile );
            }

            string rawGuid = binaryFile.Guid.ToString().Replace( "-", "" );
            string fileName = $"{rawGuid}_{binaryFile.FileName}";
            string blobName = string.IsNullOrWhiteSpace( settings.Folder ) ? fileName : $"{settings.Folder}/{fileName}";

            var container = GetContainer( settings.ContainerName );
            if ( container == null )
            {
                return null;
            }

            return container.GetBlockBlobReference( blobName );
        }

        /// <summary>
        /// Gets the container.
        /// </summary>
        /// <param name="containerName">Name of the container.</param>
        /// <returns></returns>
        private CloudBlobContainer GetContainer( string containerName )
        {
            if ( string.IsNullOrWhiteSpace( containerName ) )
            {
                containerName = GetAttributeValue( "DefaultContainerName" );
            }

            if ( string.IsNullOrWhiteSpace( containerName ) )
            {
                ExceptionLogService.LogException( "Invalid Blob container name (Azure Blob Storage Provider)." );
                return null;
            }

            lock ( __lockObj )
            {
                var accountName = GetAttributeValue( "AccountName" );
                var accountKey = GetAttributeValue( "AccountKey" );
                var customDomain = GetAttributeValue( "CustomDomain" );

                var connectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey}";
                if ( !string.IsNullOrWhiteSpace( customDomain ) )
                {
                    connectionString = $"{connectionString};BlobEndpoint={customDomain}";
                }

                var storageAccount = CloudStorageAccount.Parse( connectionString );
                if ( storageAccount == null )
                {
                    ExceptionLogService.LogException( "Unable to parse Azure Storage account connection string (Azure Blob Storage Provider)." );
                    return null;
                }

                var blobClient = storageAccount.CreateCloudBlobClient();
                if ( blobClient == null )
                {
                    ExceptionLogService.LogException( "Unable to create Azure Blob client (Azure Blob Storage Provider)." );
                    return null;
                }

                var container = blobClient.GetContainerReference( containerName );
                if ( container == null )
                {
                    ExceptionLogService.LogException( $"Unable to locate Blob container: {containerName} (Azure Blob Storage Provider)." );
                    return null;
                }

                if ( !container.Exists() )
                {
                    container.CreateIfNotExists();
                    container.SetPermissions( new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob } );
                }

                return container;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileTypeSettings"/> from attributes of the <see cref="BinaryFileType"/> associated with a file.
        /// </summary>
        /// <param name="binaryFile">The <see cref="BinaryFile"/>.</param>
        /// <returns></returns>
        private FileTypeSettings GetSettingsFromFileType( BinaryFile binaryFile )
        {
            var settings = new FileTypeSettings();
            if ( binaryFile == null || !binaryFile.BinaryFileTypeId.HasValue )
            {
                return settings;
            }

            var binaryFileType = binaryFile.BinaryFileType;
            if ( binaryFileType == null && binaryFile.BinaryFileTypeId.HasValue )
            {
                binaryFileType = new BinaryFileTypeService( new RockContext() ).Get( binaryFile.BinaryFileTypeId.Value );
            }
            if ( binaryFileType == null )
            {
                return settings;
            }

            if ( binaryFileType.Attributes == null )
            {
                binaryFileType.LoadAttributes();
            }

            settings.ContainerName = binaryFileType.GetAttributeValue( "AzureBlobContainerName" );
            settings.Folder = ( binaryFileType.GetAttributeValue( "AzureBlobContainerFolderPath" ) ?? string.Empty )
                .Replace( @"\", "/" )
                .TrimEnd( "/".ToCharArray() );

            return settings;
        }

        /// <summary>
        /// File Type Settings POCO.
        /// </summary>
        private class FileTypeSettings
        {
            public string ContainerName;
            public string Folder;
        }
    }

}
