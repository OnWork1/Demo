using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMW.IntegrationService.CommonClassesAndEnums.Classes.Data_Storage
{
    class Helper
    {
        /// <summary>
        /// CreateContainer
        /// </summary>
        /// <param name="client"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public CloudBlobContainer CreateContainer(CloudBlobClient client, string container)
        {
            CloudBlobContainer blobContainer = client.GetContainerReference(container);
            if (blobContainer.CreateIfNotExists())
            {
                blobContainer.SetPermissions(
                    new BlobContainerPermissions
                    {
                        PublicAccess = BlobContainerPublicAccessType.Blob
                    }
                );
            }
            return blobContainer;
        }

        /// <summary>
        /// GetBinaryFile
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public byte[] GetBinaryFile(FileStream file)
        {
            byte[] bytes;

            bytes = new byte[file.Length];
            file.Read(bytes, 0, (int)file.Length);

            return bytes;
        }
    }
}
