using PetShop.Components;
using PetShop.Services;
using Microsoft.AspNetCore.Components;
using PetShop.DTO;
using Dapper;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<AppointmentService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<PetService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<ServiceService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<EmployeeService>();
builder.Services.AddScoped<WorkShiftService>();
builder.Services.AddScoped<CommissionService>();
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient();
    try
    {
        var navManager = sp.GetService<NavigationManager>();
        if (navManager != null)
        {


            httpClient.BaseAddress = new Uri(navManager.BaseUri);
        }
    }
    catch {  }
    return httpClient;
});
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddScoped<PetShop.Services.PaymentService>();

var app = builder.Build();



app.MapPost("/api/payos/webhook", async (Microsoft.AspNetCore.Http.HttpContext context, PetShop.Services.OrderService orderService, PetShop.Services.AppointmentService apptService, PetShop.Services.CommissionService settings) => {
    using var reader = new System.IO.StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    


    
    using var doc = System.Text.Json.JsonDocument.Parse(body);
    var data = doc.RootElement.GetProperty("data");
    var orderCode = data.GetProperty("orderCode").GetInt64();
    var desc = data.GetProperty("description").GetString() ?? "";


    if (desc.Contains("Don hang"))
    {
        await orderService.UpdateOrderStatusAsync((int)orderCode, "Completed");
    }
    else if (desc.Contains("Lich hen"))
    {
        await apptService.UpdateAppointmentPaymentAsync((int)orderCode, "Completed");
        await apptService.UpdateAppointmentStatusAsync((int)orderCode, "Completed");
    }

    return Results.Ok(new { success = true });
});

app.MapPost("/api/payment/create", async (PaymentRequest req, PetShop.Services.PaymentService payService, Microsoft.AspNetCore.Http.HttpContext context) => {
    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}";
    var url = await payService.CreatePaymentLinkAsync(req.OrderCode, req.Amount, req.Description, baseUrl);
    return Results.Ok(new { checkoutUrl = url });
});



if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseDeveloperExceptionPage();
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(PetShop.Client._Imports).Assembly);

app.MapGet("/api/admin/reseed", async (IConfiguration config) => {
    using var db = new Microsoft.Data.SqlClient.SqlConnection(config.GetConnectionString("DefaultConnection"));
    await db.OpenAsync();
    try {
        await db.ExecuteAsync("DBCC CHECKIDENT ('ORDERS', RESEED)");
        await db.ExecuteAsync("DBCC CHECKIDENT ('ORDER_DETAIL', RESEED)");
        await db.ExecuteAsync("DBCC CHECKIDENT ('APPOINTMENT', RESEED)");
        await db.ExecuteAsync("DBCC CHECKIDENT ('APPOINTMENT_DETAIL', RESEED)");
        await db.ExecuteAsync("DBCC CHECKIDENT ('COMMISSION_HISTORY', RESEED)");
        return Results.Ok("Đã đồng bộ lại mã ID tự động cho tất cả các bảng thành công!");
    } catch (Exception ex) { return Results.BadRequest(ex.Message); }
});

app.MapPost("/api/roles", async (RoleDisplay role, EmployeeService service) => {
    role.role_id = await service.AddRoleAsync(role);
    return Results.Ok(role);
});
app.MapGet("/api/commissions", async (CommissionService service) => await service.GetCommissionsAsync());
app.MapPost("/api/commissions", async (CommissionDisplay c, CommissionService service) => {
    await service.AddCommissionAsync(c);
    return Results.Ok();
});
app.MapPut("/api/commissions/{id}", async (int id, CommissionDisplay c, CommissionService service) => {
    c.commission_id = id;
    await service.UpdateCommissionAsync(c);
    return Results.Ok();
});
app.MapDelete("/api/commissions/{id}", async (int id, CommissionService service) => {
    await service.DeleteCommissionAsync(id);
    return Results.Ok();
});
app.MapGet("/api/commissions/rate/{key}", async (string key, CommissionService service) => Results.Ok(await service.GetGlobalRateAsync(key)));
app.MapPost("/api/commissions/rate/{key}", async (string key, [Microsoft.AspNetCore.Mvc.FromBody] decimal value, CommissionService service) => {
    await service.SetGlobalRateAsync(key, value);
    return Results.Ok();
});
app.MapGet("/api/settings/{key}", async (string key, CommissionService service) => Results.Ok(await service.GetSettingAsync(key)));
app.MapPost("/api/settings/{key}", async (string key, [Microsoft.AspNetCore.Mvc.FromBody] string value, CommissionService service) => {
    await service.SetSettingAsync(key, value);
    return Results.Ok();
});
app.MapGet("/api/workshifts", async (WorkShiftService service) => await service.GetAllWorkShiftsAsync());
app.MapPost("/api/workshifts", async (WorkShiftDisplay ws, WorkShiftService service) => {
    await service.AddWorkShiftAsync(ws);
    return Results.Ok();
});
app.MapPut("/api/workshifts/{id}", async (int id, WorkShiftDisplay ws, WorkShiftService service) => {
    ws.workshift_id = id;
    await service.UpdateWorkShiftAsync(ws);
    return Results.Ok();
});
app.MapDelete("/api/workshifts/{id}", async (int id, WorkShiftService service) => {
    await service.DeleteWorkShiftAsync(id);
    return Results.Ok();
});
app.MapGet("/api/roles", async (EmployeeService service) => await service.GetRolesAsync());
app.MapGet("/api/employees", async (EmployeeService service) => await service.GetEmployeesAsync());
app.MapPost("/api/employees", async (EmployeeDisplay emp, EmployeeService service) => { await service.AddEmployeeAsync(emp); return Results.Ok(); });
app.MapPut("/api/employees/{id}", async (int id, EmployeeDisplay emp, EmployeeService service) => { emp.employee_id = id; await service.UpdateEmployeeAsync(emp); return Results.Ok(); });
app.MapDelete("/api/employees/{id}", async (int id, EmployeeService service) => { await service.DeleteEmployeeAsync(id); return Results.Ok(); });
app.MapPost("/api/accounts/login", async (LoginRequest request, AccountService service) => {
    var account = await service.LoginAsync(request);
    if (account == null) return Results.BadRequest("Sai tài khoản hoặc mật khẩu!");
    if (!account.status) return Results.BadRequest("Tài khoản này đã bị khóa!");
    if (!account.is_active) return Results.BadRequest("Tài khoản thuộc về nhân viên đã nghỉ việc!");
    return Results.Ok(account);
});
app.MapPost("/api/accounts", async (AccountCreateRequest req, AccountService service) => {
    try {
        int newId = await service.CreateAccountAsync(req);
        return Results.Ok(new { account_id = newId });
    }
    catch (Exception ex) { return Results.BadRequest(ex.Message); }
});

app.MapGet("/api/customers", async (CustomerService service) => Results.Ok(await service.GetAllCustomersAsync()));
app.MapGet("/api/customers/performance", async (CustomerService service) => {
    var dynamicResult = await service.GetCustomersDynamicAsync();
    var storedResult = await service.GetCustomersStoredAsync();
    return Results.Ok(new { dynamic = dynamicResult, stored = storedResult });
});

app.MapPost("/api/customers", async (CustomerDisplay customer, CustomerService service) => { await service.AddCustomerAsync(customer); return Results.Ok(); });
app.MapPut("/api/customers/{id}", async (int id, CustomerDisplay customer, CustomerService service) => {
    customer.customer_id = id;
    await service.UpdateCustomerAsync(customer);
    return Results.Ok();
});
app.MapDelete("/api/customers/{id}", async (int id, CustomerService service) => { await service.DeleteCustomerAsync(id); return Results.Ok(); });
app.MapGet("/api/orders", async (OrderService service) => Results.Ok(await service.GetOrdersAsync()));
app.MapGet("/api/orders/{id}", async (int id, OrderService service) => {
    var ord = await service.GetOrderByIdAsync(id);
    return ord != null ? Results.Ok(ord) : Results.NotFound();
});
app.MapPost("/api/orders", async (OrderCreateRequest req, OrderService service) => {
    try {
        int newId = await service.AddOrderAsync(req);
        return Results.Ok(new { order_id = newId });
    } catch (Exception ex) { return Results.BadRequest(ex.Message); }
});
app.MapPost("/api/orders/{id}/status", async (int id, string status, OrderService service) => { await service.UpdateOrderStatusAsync(id, status); return Results.Ok(); });
app.MapPut("/api/orders/{id}", async (int id, OrderFullDisplay order, OrderService service) => {
    order.order_id = id;
    await service.UpdateOrderAsync(order);
    return Results.Ok();
});
app.MapDelete("/api/orders/{id}", async (int id, OrderService service) => { await service.DeleteOrderAsync(id); return Results.Ok(); });

app.MapGet("/api/appointments", async (AppointmentService service) => Results.Ok(await service.GetAppointmentsForDisplayAsync()));
app.MapGet("/api/appointments/{id}", async (int id, AppointmentService service) => {
    var appt = await service.GetAppointmentByIdAsync(id);
    return appt != null ? Results.Ok(appt) : Results.NotFound();
});
app.MapPost("/api/appointments", async (AppointmentCreateRequest req, AppointmentService service) => {
    try {
        int newId = await service.AddAppointmentAsync(req);
        return Results.Ok(new { appointment_id = newId });
    } catch (Exception ex) { return Results.BadRequest(ex.Message); }
});
app.MapPost("/api/appointments/{id}/status", async (int id, string status, AppointmentService service) => { await service.UpdateAppointmentStatusAsync(id, status); return Results.Ok(); });
app.MapPost("/api/appointments/{id}/payment", async (int id, string status, AppointmentService service) => { await service.UpdateAppointmentPaymentAsync(id, status); return Results.Ok(); });
app.MapPut("/api/appointments/{id}", async (int id, AppointmentDisplay appt, AppointmentService service) => {
    try {
        appt.appointment_id = id;
        await service.UpdateAppointmentAsync(appt);
        return Results.Ok();
    } catch (Exception ex) { return Results.BadRequest(ex.Message); }
});
app.MapDelete("/api/appointments/{id}", async (int id, AppointmentService service) => { await service.DeleteAppointmentAsync(id); return Results.Ok(); });

app.MapGet("/api/pets", async (PetService service) => await service.GetAllPetsAsync());
app.MapPost("/api/pets", async (PetDisplay pet, PetService service) => { await service.AddPetAsync(pet); return Results.Ok(); });
app.MapPut("/api/pets/{id}", async (int id, PetDisplay pet, PetService service) => { pet.pet_id = id; await service.UpdatePetAsync(pet); return Results.Ok(); });
app.MapDelete("/api/pets/{id}", async (int id, PetService service) => {
    try {
        await service.DeletePetAsync(id);
        return Results.Ok();
    } catch (Exception ex) {
        return Results.BadRequest(ex.Message);
    }
});
app.MapGet("/api/products", async (ProductService service) => await service.GetAllProductsAsync());
app.MapPost("/api/products", async (ProductDisplay product, ProductService service) => { await service.AddProductAsync(product); return Results.Ok(); });
app.MapPut("/api/products/{id}", async (int id, ProductDisplay product, ProductService service) => { product.product_id = id; await service.UpdateProductAsync(product); return Results.Ok(); });
app.MapPut("/api/products/{id}/stock", async (int id, int stockQuantity, ProductService service) => { await service.UpdateStockAsync(id, stockQuantity); return Results.Ok(); });
app.MapDelete("/api/products/{id}", async (int id, ProductService service) => { await service.DeleteProductAsync(id); return Results.Ok(); });
app.MapGet("/api/services", async (ServiceService service) => await service.GetAllServicesAsync());
app.MapPost("/api/services", async (ServiceDisplay svc, ServiceService service) => { await service.AddServiceAsync(svc); return Results.Ok(); });
app.MapPut("/api/services/{id}", async (int id, ServiceDisplay svc, ServiceService service) => { svc.service_id = id; await service.UpdateServiceAsync(svc); return Results.Ok(); });
app.MapDelete("/api/services/{id}", async (int id, ServiceService service) => { await service.DeleteServiceAsync(id); return Results.Ok(); });

app.MapGet("/api/workshifts/qr-secret", async (WorkShiftService service) => Results.Ok(await service.GetQRSecretAsync()));
app.MapPost("/api/workshifts/qr-secret", async (string secret, WorkShiftService service) => { await service.SetQRSecretAsync(secret); return Results.Ok(); });
app.MapGet("/api/workshifts/active/{employeeId}", async (int employeeId, WorkShiftService service) => Results.Ok(await service.IsShiftActiveAsync(employeeId)));
app.MapPost("/api/workshifts/close/{employeeId}", async (int employeeId, WorkShiftService service) => { await service.CloseShiftAsync(employeeId); return Results.Ok(); });

app.MapGet("/api/customers/account/{id}", async (int id, CustomerService service) => Results.Ok(await service.GetCustomerByAccountIdAsync(id)));
app.MapPost("/api/portal/register", async (CustomerRegisterRequest req, CustomerService service) => {
    await service.RegisterCustomerAsync(req);
    return Results.Ok();
});
app.MapGet("/api/pets/customer/{id}", async (int id, PetService service) => Results.Ok(await service.GetPetsByCustomerIdAsync(id)));

app.MapGet("/api/appointments/customer/{id}", async (int id, AppointmentService service) => Results.Ok(await service.GetAppointmentsByCustomerIdAsync(id)));

app.Run();

public class PaymentRequest { public long OrderCode { get; set; } public int Amount { get; set; } public string Description { get; set; } }
