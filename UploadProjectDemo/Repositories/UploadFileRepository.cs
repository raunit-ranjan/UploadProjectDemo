using CsvHelper;
using CsvHelper.Configuration;
using ExcelDataReader;
using System.Globalization;
using UploadProjectDemo.Data;
using UploadProjectDemo.Models.Domain;
using UploadProjectDemo.Models.Dto;

namespace UploadProjectDemo.Repositories
{
    public class UploadFileRepository : IUploadFileRepository
    {
        private readonly ProductDbContext _dbContext;

        public UploadFileRepository(ProductDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResponseDto> UploadCSVFile(UploadFileRequest request, string Path)
        {
            ResponseDto response = new ResponseDto();
            List<Product> productsData = new List<Product>();
            response.IsSuccess = false;
            response.Message = "";

            try
            {
                if (request.File.FileName.ToLower().EndsWith(".csv"))
                {
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        HeaderValidated = null,
                        MissingFieldFound = null
                    };

                    using (var reader = new StreamReader(Path))
                    using (var csv = new CsvReader(reader, config))
                    {
                        var records = csv.GetRecords<UploadFileParameter>();

                        foreach (var record in records)
                        {
                            // check if code already exists in database
                            if (!_dbContext.Products.Any(x => x.Code == record.Code))
                            {
                                productsData.Add(new Product
                                {
                                    Code = record.Code,
                                    Name = record.Name,
                                    Description = record.Description,
                                    UploadedOn = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                });
                            }
                        }

                        if (productsData.Count > 0)
                        {
                            await _dbContext.Products.AddRangeAsync(productsData);
                            await _dbContext.SaveChangesAsync();
                        }

                        response.IsSuccess = true;
                        response.Message = "Successful";
                    }
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid File";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }


        public async Task<ResponseDto> UploadExcelFile(UploadFileRequest request, string path)
        {
            ResponseDto response = new ResponseDto();
            List<Product> productsData = new List<Product>();
            response.IsSuccess = false;
            response.Message = "";

            try
            {
                if (request.File.FileName.ToLower().EndsWith(".xlsx") || request.File.FileName.ToLower().EndsWith(".xls"))
                {
                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await request.File.CopyToAsync(stream);
                    }

                    using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                    {
                        using (var reader = ExcelReaderFactory.CreateReader(stream))
                        {
                            var config = new ExcelDataSetConfiguration
                            {
                                ConfigureDataTable = _ => new ExcelDataTableConfiguration
                                {
                                    UseHeaderRow = true,
                                }
                            };

                            var dataSet = reader.AsDataSet(config);

                            var dataTable = dataSet.Tables[0];

                            for (int i = 0; i < dataTable.Rows.Count; i++)
                            {
                                UploadFileParameter readData = new UploadFileParameter();
                                readData.Code = dataTable.Rows[i][0] != null ? Convert.ToString(dataTable.Rows[i][0]) : "";
                                readData.Name = dataTable.Rows[i][1] != null ? Convert.ToString(dataTable.Rows[i][1]) : "";
                                readData.Description = dataTable.Rows[i][2] != null ? Convert.ToString(dataTable.Rows[i][2]) : "";

                                // check if code already exists in database
                                if (!_dbContext.Products.Any(x => x.Code == readData.Code))
                                {
                                    productsData.Add(new Product
                                    {
                                        Code = readData.Code,
                                        Name = readData.Name,
                                        Description = readData.Description,
                                        UploadedOn = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                                    });
                                }

                            }




                            if (productsData.Count > 0)
                            {
                                await _dbContext.Products.AddRangeAsync(productsData);
                                await _dbContext.SaveChangesAsync();
                            }

                            response.IsSuccess = true;
                            response.Message = "Successful";
                        }
                    }

                    File.Delete(path);
                }
                else
                {
                    response.IsSuccess = false;
                    response.Message = "Invalid file format";
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
            }

            return response;
        }

    }
}

