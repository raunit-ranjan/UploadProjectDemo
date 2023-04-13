using UploadProjectDemo.Models.Dto;
using static UploadProjectDemo.Models.Dto.UploadFileRequest;

namespace UploadProjectDemo.Repositories
{
    public interface IUploadFileRepository
    {
        public Task<ResponseDto> UploadCSVFile(UploadFileRequest request, string Path);
        public Task<ResponseDto> UploadExcelFile(UploadFileRequest request, string path);
    }
}
