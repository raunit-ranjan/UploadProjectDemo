using CsvHelper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Data;
using System.Globalization;
using UploadProjectDemo.Data;
using UploadProjectDemo.Models.Domain;
using UploadProjectDemo.Models.Dto;

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

        public async Task<ResponseDto> ExportProductsToPdf()
        {
            try
            {
                List<Product> products = await _productRepository.GetAllAsync();
                DataTable dataTable = ConvertToDataTable(products);
                string fileName = "products_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".pdf";
                string directoryPath = _configuration.GetValue<string>("ExportPath");
                string filePath = Path.Combine(directoryPath, fileName);
                EnsureDirectoryExists(directoryPath);

                ExportToPdf(dataTable, filePath);

                return new ResponseDto
                {
                    IsSuccess = true,
                    Message = "File generated successfully! " + directoryPath + "\\" + fileName
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

        public async Task<ResponseDto> ExportProductsToCsv()
        {
            try
            {
                List<Product> products = await _productRepository.GetAllAsync();
                // DataTable dataTable = ConvertToDataTable(products);
                string fileName = "products_" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".csv";
                string directoryPath = _configuration.GetValue<string>("ExportPath");
                string filePath = Path.Combine(directoryPath, fileName);
                EnsureDirectoryExists(directoryPath);

                ExportToCsv(products, filePath);

                return new ResponseDto
                {
                    IsSuccess = true,
                    Message = "File generated successfully! " + directoryPath + "\\" + fileName
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

        private List<Product> ConvertToObject(DataTable products)
        {
            var productList = new List<Product>();

            foreach (DataRow row in products.Rows)
            {
                var product = new Product()
                {
                    Id = (int)row["Id"],
                    Name = (string)row["Name"],
                    Description = (string)row["Description"],
                    UploadedOn = (DateTime)row["UploadedOn"]
                };

                productList.Add(product);
            }
            return productList;
        }

        private DataTable ConvertToDataTable(List<Product> products)
        {
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

            return dataTable;
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private void ExportToPdf(DataTable dataTable, string filePath)
        {
            Document document = new Document();
            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
            document.Open();

            PdfPTable pdfTable = new PdfPTable(dataTable.Columns.Count);
            pdfTable.WidthPercentage = 100;

            AddTableHeaders(pdfTable, dataTable);

            AddTableRows(pdfTable, dataTable);

            document.Add(pdfTable);
            document.Close();
            document.Dispose();
        }

        public void ExportToCsv(List<Product> products, string filePath)
        {
            try
            {
                using (var writer = new StreamWriter(filePath))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteHeader<Product>();
                    csv.NextRecord();
                    foreach (var product in products)
                    {
                        csv.WriteRecord(product);
                        csv.NextRecord();
                    }
                }

                /*
                var csvBuilder = new StringBuilder();

                // Add header row
                csvBuilder.AppendLine("Id,Name,Description,UploadedOn");

                // Add product rows
                var productList = ConvertToObject(products);
                foreach (var product in productList)
                {
                    csvBuilder.AppendLine($"{product.Id},{product.Name},{product.Description},{product.UploadedOn}");
                }

                File.WriteAllText(filePath, csvBuilder.ToString());
                */
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error exporting products: {ex.Message}");
            }
        }

        private void AddTableHeaders(PdfPTable pdfTable, DataTable dataTable)
        {
            foreach (DataColumn column in dataTable.Columns)
            {
                PdfPCell cell = new PdfPCell(new Phrase(column.ColumnName));
                cell.BackgroundColor = new BaseColor(240, 240, 240);
                pdfTable.AddCell(cell);
            }
        }

        private void AddTableRows(PdfPTable pdfTable, DataTable dataTable)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                foreach (object item in row.ItemArray)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(item.ToString()));
                    pdfTable.AddCell(cell);
                }
            }
        }

    }
}
