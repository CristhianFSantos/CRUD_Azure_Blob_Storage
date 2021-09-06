using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Azure_Blob_Storage
{
    class Program
    {
        const string StorageAccountName = "MyAccountName";
        const string StorageAccountKey = "MyAccountKey";
        const string Container = "b7620fa1-b268-4134-960c-fl76da90845c";
        const string WorkingDirectory = @"E:\Azure\";

        #region EXECUTION
        static async Task Main(string[] args)
        {

            await DownloadFiles("contrato.pdf");

        }

        #endregion


        #region METHODS
        static async Task DownloadFiles(string image)
        {
            CloudBlobContainer container = GetContainer();
            CloudBlockBlob blobs = container.GetBlockBlobReference(image);
            try
            {
                if (!Directory.Exists(WorkingDirectory)) Directory.CreateDirectory(WorkingDirectory);
                await blobs.DownloadToFileAsync(Path.Combine(WorkingDirectory, image), FileMode.Create);
            }
            catch (Exception)
            {
                throw;
            }
        }
        static async Task DeleteFiles(string file)
        {
            CloudBlobContainer container = GetContainer();
            CloudBlockBlob blobs = container.GetBlockBlobReference(file);
            try
            {
                await blobs.DeleteAsync();
                using StreamWriter writer = File.AppendText(WorkingDirectory + @"\DELETED FILES.txt");
                writer.WriteLine(file + " DELETED");
            }
            catch (Exception error)
            {
                using StreamWriter writer = File.AppendText(WorkingDirectory + @"\FILES NOT DELETED.txt");
                writer.WriteLine(file + " : " + error.Message.ToUpper());
            }
        }
        static async Task ListFiles()
        {
            CloudBlobContainer container = GetContainer();
            BlobContinuationToken blobContinuationToken = null;
            do
            {
                var results = await container.ListBlobsSegmentedAsync(null, blobContinuationToken);
                blobContinuationToken = results.ContinuationToken;
                foreach (IListBlobItem blob in results.Results)
                {
                    var v = blob.Uri.Scheme;
                    using StreamWriter writer = File.AppendText(WorkingDirectory + @"\ALL EXISTING FILES.txt");
                    writer.WriteLine(blob.Uri.Segments.Last().Replace("%20", " "));
                }
            } while (blobContinuationToken != null);
        }
        static async Task UploadPDFFile(string file, string fileName)
        {
            CloudBlobContainer container = GetContainer();
            CloudBlockBlob blobs = container.GetBlockBlobReference(fileName);
            try
            {
                string extension = new FileInfo(file).Extension.Substring(1).ToLower();
                blobs.Properties.ContentType = $"application/{extension}";
                await blobs.UploadFromFileAsync(file);
            }
            catch (Exception)
            {
                throw;
            }
        }
        static async Task RenameFiles(string oldFile, string newFile)
        {
            CloudBlobContainer container = GetContainer();
            CloudBlockBlob blobs = container.GetBlockBlobReference(oldFile);
            try
            {
                string blobURI = blobs.Uri.ToString();

                CloudBlockBlob newBlob = container.GetBlockBlobReference(blobURI.Replace(oldFile, newFile));
                await blobs.StartCopyAsync(blobs);
                await blobs.DeleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        static CloudBlobContainer GetContainer()
        {
            CloudStorageAccount storageAccount = new(new StorageCredentials(StorageAccountName, StorageAccountKey), true);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference(Container);
            return container;
        }
        #endregion
    }
}