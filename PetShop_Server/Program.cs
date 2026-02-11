using Microsoft.EntityFrameworkCore;
using PetShop_Server.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. KẾT NỐI DATABASE ---
// Dòng này báo cho Server biết dùng chuỗi kết nối nào
builder.Services.AddDbContext<PetShopDbContext>();

// --- 2. CẤU HÌNH CORS (Cho phép Blazor gọi) ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()  // Chấp nhận mọi nguồn (Blazor chạy port nào cũng được)
              .AllowAnyMethod()  // Chấp nhận GET, POST, PUT, DELETE
              .AllowAnyHeader();
    });
});

// --- 3. ĐĂNG KÝ DỊCH VỤ ---// Tìm dòng này trong Program.cs
builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

builder.Services.AddControllers(options => options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);
// ^^^ Thêm dòng .AddJsonOptions này vào để chặn lỗi vòng lặp
// Các dòng cấu hình Swagger (Giao diện test API)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 4. CẤU HÌNH HTTP REQUEST PIPELINE ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll"); // <--- KÍCH HOẠT CORS (Quan trọng)

app.UseAuthorization();

app.MapControllers();

app.Run();