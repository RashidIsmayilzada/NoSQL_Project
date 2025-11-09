using MongoDB.Driver;
using NoSQL_Project.Repositories;
using NoSQL_Project.Repositories.Interfaces;
using NoSQL_Project.Services;
using NoSQL_Project.Services.Interfaces;

DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// 1) Register MongoClient as a SINGLETON (one shared instance for the whole app)
// WHY: MongoClient is thread-safe and internally manages a connection pool.
// Reusing one instance is fast and efficient. Creating many clients would waste resources.
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    // Read the connection string from configuration (env var via .env)
    var conn = builder.Configuration["Mongo:ConnectionString"];
    if (string.IsNullOrWhiteSpace(conn))
        throw new InvalidOperationException("Mongo:ConnectionString is not configured. Did you set it in .env?");

    // Optional: tweak settings (timeouts, etc.)
    var settings = MongoClientSettings.FromConnectionString(conn);
    // settings.ServerSelectionTimeout = TimeSpan.FromSeconds(5);

    return new MongoClient(settings);
});

// 2) Register IMongoDatabase as SCOPED (new per HTTP request)
// WHY: Fits the ASP.NET request lifecycle and keeps each request cleanly separated.
builder.Services.AddScoped(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();

    var dbName = builder.Configuration["Mongo:Database"]; // from appsettings.json
    if (string.IsNullOrWhiteSpace(dbName))
        throw new InvalidOperationException("Mongo:Database is not configured in appsettings.json.");

    return client.GetDatabase(dbName);
});

// Add authentification for login
builder.Services.AddAuthentication("MyCookie")
    .AddCookie("MyCookie", options =>
    {
        options.LoginPath = "/Login/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<ITicketSearchService, TicketSearchService>(); //individual feature ticket search service Pariya Hallaji

// Enable session state  
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(120);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();
app.UseStaticFiles();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
