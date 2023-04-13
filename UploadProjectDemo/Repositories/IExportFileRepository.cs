﻿using Microsoft.AspNetCore.Mvc;
using UploadProjectDemo.Models.Dto;

namespace UploadProjectDemo.Repositories
{
    public interface IExportFileRepository
    {
        public Task<ResponseDto> ExportProducts();
    }
}
