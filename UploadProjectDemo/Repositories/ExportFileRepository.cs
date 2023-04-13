using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Data;
using Microsoft.AspNetCore.Mvc;
using UploadProjectDemo.Models.Domain;
using UploadProjectDemo.Data;
using UploadProjectDemo.Models.Dto;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders.Composite;

namespace UploadProjectDemo.Repositories
{
    public class ExportFileRepository : IExportFileRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ProductDbContext _dbContext;
        private readonly IProductRepository _productRepository;
        public ExportFileRepository(
            IConfiguration configuration,
            ProductDbContext dbContext,
            IProductRepository productRepository)
        {
            _configuration = configuration;
            _dbContext = dbContext;
            _productRepository = productRepository;
        }

        public async Task<ResponseDto> ExportProducts()
        {
            try
            {
                // Get the products data from your data source
                List<Product> products = await _productRepository.GetAllAsync();

                // Convert the data to a DataTable object
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("Id", typeof(int));
                dataTable.Columns.Add("Code", typeof(string));
                dataTable.Columns.Add("Name", typeof(string));
                dataTable.Columns.Add("Description", typeof(string));
                dataTable.Columns.Add("UploadedOn", typeof(DateTime));
                foreach (Product product in products)
                {
                    dataTable.Rows.Add(
                        product.Id, 
                        product.Code, 
                        product.Name, 
                        product.Description,
                        product.UploadedOn);
                }

                // Export the data to PDF and return the file
                string fileName = "products_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".pdf";
                string directoryPath = _configuration.GetValue<string>("ExportPath");
                string filePath = Path.Combine(directoryPath, fileName);

                // Create the directory if it doesn't exist
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                ExportToPdf(dataTable, filePath);

                //byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                //var result = new FileStreamResult(new MemoryStream(fileBytes), "application/pdf");
                //result.FileDownloadName = fileName;

                return new ResponseDto
                {
                    IsSuccess = true,
                    Message = "File generated successfully! "  + directoryPath + "\\"  + fileName
                };
            }
            catch (Exception ex)
            {
                return new ResponseDto
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

        private void ExportToPdf(DataTable dataTable, string filePath)
        {
            // Create the PDF document and writer
            Document document = new Document();
            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));

            // Open the document
            document.Open();

            // Create the PDF table
            PdfPTable pdfTable = new PdfPTable(dataTable.Columns.Count);
            pdfTable.WidthPercentage = 100;

            // Add the table headers
            foreach (DataColumn column in dataTable.Columns)
            {
                PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName));
                cell.BackgroundColor = new BaseColor(240, 240, 240);
                pdfTable.AddCell(cell);
            }

            // Add the table rows
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (object item in row.ItemArray)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(item.ToString()));
                    pdfTable.AddCell(cell);
                }
            }

            // Add the PDF table to the document
            document.Add(pdfTable);

            // Close the document and writer
            document.Close();
            document.Dispose();
        }
    }
}
