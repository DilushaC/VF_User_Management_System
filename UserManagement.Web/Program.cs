using UserManagement.Business.BranchHandler;
using UserManagement.Business.ConnectionHandler;
using UserManagement.Business.DatatableHandler;
using UserManagement.Business.DepartmentHandler;
using UserManagement.Business.DesignationHandler;
using UserManagement.Business.Helpers;
using UserManagement.Business.PageHandler;
using UserManagement.Business.ProductHandler;
using UserManagement.Business.RoleHandler;
using UserManagement.Business.RolePagePermission;
using UserManagement.Business.UserHandler;
using UserManagement.Business.UserRoleHandler;
using UserManagement.Data.Context;
using UserManagement.Presentation.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<SessionCheckAttribute>();
});

// Register application services
builder.Services.AddScoped<DapperContext>();
builder.Services.AddScoped<_ConnectionService>(); 
builder.Services.AddScoped<PasswordHelper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IDesignationService, DesignationService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserRoleService, UserRoleService>();
builder.Services.AddScoped<IRolePagePermissionService, RolePagePermissionService>();
builder.Services.AddScoped<IDataTableService, DataTableService>();


// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

// Accessor for HttpContext in services
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session before authorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
