using Microsoft.EntityFrameworkCore;
using System.Text;
using UploadProjectDemo.Data;
using UploadProjectDemo.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProductDbContext>(
            options => options.UseSqlServer(
                builder.Configuration.GetConnectionString("ProductConnectionString")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUploadFileRepository, UploadFileRepository>();
builder.Services.AddScoped<IExportFileRepository, ExportFileRepository>();

// Register the encoding provider before running the app
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
