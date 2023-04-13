using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Reflection.Metadata;
using UploadProjectDemo.Models.Domain;
using UploadProjectDemo.Models.Dto;
using UploadProjectDemo.Repositories;

namespace UploadProjectDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IUploadFileRepository _uploadFileRepository;
        private readonly IExportFileRepository _exportFileRepository;

        public ProductsController(
            IProductRepository productRepository, 
            IUploadFileRepository uploadFileDL, 
            IExportFileRepository exportFileRepository)
        {
            this._productRepository = productRepository;
            this._uploadFileRepository = uploadFileDL;
            this._exportFileRepository = exportFileRepository;
        }



        // Get All Products
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                //Get data from database
                var products = await _productRepository.GetAllAsync();

                //Map Domain Model to Dto
                var productsDto = new List<ProductDto>();
                foreach (var product in products)
                {
                    productsDto.Add(new ProductDto()
                    {
                        Code = product.Code,
                        Name = product.Name,
                        Description = product.Description,
                        UploadedOn = product.UploadedOn,
                    });
                }

                //Map Domain Model to Dto
                // var productsDto = mapper.Map<List<ProductDto>>(products);

                //retrun the Dto
                return Ok(productsDto);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // Get Product by Id
        [HttpGet]
        [Route("{id:int}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            var product = await _productRepository.GetByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            var productDto = new ProductDto()
            {
                Code = product.Code,
                Name = product.Name,
                Description = product.Description,
                UploadedOn= product.UploadedOn,
            };

            // var regionDto = mapper.Map<RegionDto>(region);

            return Ok(productDto);
        }


        // Upload data from CSV or Excel File
        [HttpPost]
        [Route("Upload")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileRequest request)
        {
            try
            {
                string filePath = "UploadFileFolder/" + request.File.FileName;
                string directoryPath = Path.GetDirectoryName(filePath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                using (FileStream stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
                {
                    await request.File.CopyToAsync(stream);
                }

                ResponseDto response = null;
                switch (Path.GetExtension(request.File.FileName).ToLower())
                {
                    case ".csv":
                        response = await _uploadFileRepository.UploadCSVFile(request, filePath);
                        break;
                    case ".xlsx":
                    case ".xls":
                        response = await _uploadFileRepository.UploadExcelFile(request, filePath);
                        break;
                    default:
                        throw new Exception("Unsupported file format");
                }

                string[] files = Directory.GetFiles("UploadFileFolder/");
                foreach (string file in files)
                {
                    System.IO.File.Delete(file);
                    Console.WriteLine($"{file} is deleted.");
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseDto
                {
                    IsSuccess = false,
                    Message = ex.Message
                });
            }
        }


        // Read data from database and write in pdf
        [HttpGet]
        [Route("Export")]
        public async Task<IActionResult> Export()
        {
            try
            {
                return Ok(await _exportFileRepository.ExportProducts());
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
