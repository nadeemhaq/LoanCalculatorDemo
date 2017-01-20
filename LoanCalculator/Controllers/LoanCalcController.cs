using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Mvc;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
// Namespace for CloudConfigurationManager
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Blob; // Namespace for Blob storage types

namespace LoanCalculator.Controllers
{
    public class LoanCalcController : ApiController
    {

        // GET api/<controller>/5
        public double Get(double principal, int numberOfPayments, double interestRate)
        {
            var ratePerPeriod = interestRate/12f;
            // Calculate the Payment Amount here
            var paymentAmount = CalculatePaymentAmount(principal, numberOfPayments, ratePerPeriod);

            return paymentAmount;
        }

        public static double CalculatePaymentAmount(double principal, int numberOfPayments, double ratePerPeriod)
        {
            var paymentAmount = principal*
                                ((ratePerPeriod*Math.Pow((1 + ratePerPeriod), numberOfPayments))/
                                 (Math.Pow((1 + ratePerPeriod), numberOfPayments) - 1));
            return paymentAmount;
        }

        public async Task<IHttpActionResult> Post()
        { 
            //pick file with name: file
            HttpPostedFile uploadedFile = HttpContext.Current.Request.Files[0];

            if (uploadedFile == null)
            {
                throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            // Parse the connection string and return a reference to the storage account.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference("loancontainer");

            // Create the container if it doesn't already exist.
            container.CreateIfNotExists();

            container.SetPermissions(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });

            // Retrieve reference to a blob named "myblob".
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(uploadedFile.FileName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            await blockBlob.UploadFromStreamAsync(uploadedFile.InputStream);

            // Post a message on the service bus
            var client = QueueClient.CreateFromConnectionString(ConfigurationManager.ConnectionStrings["AzureWebJobsServiceBus"].ConnectionString, "imagesqueue");            
            var message = new BrokeredMessage(uploadedFile.FileName);
            client.Send(message);

            return Created("loanimage.jpg", uploadedFile.FileName);
        }

    }
}
