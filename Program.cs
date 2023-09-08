using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBlog.Data;
using MyBlog.Models;
using MyBlog.Services.Interfaces;
using MyBlog.Services;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = DataUtility.GetConnectionString(builder.Configuration) ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString, o => o.MigrationsHistoryTable(tableName: "BlogMigrationHistory", schema: "blog")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<BlogUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddDefaultUI()
    .AddDefaultTokenProviders()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Register Custom Services
builder.Services.AddScoped<IImageService, ImageService>();

builder.Services.AddScoped<IBlogService, BlogService>();

builder.Services.AddScoped<IEmailSender, EmailService>();

//Bind the email settings to the EmailSettings object
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddMvc();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Adam's Blog",
        Version = "v1",
        Description = "A public facing API to fetch the latest blog posts",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Adam Berry",
            Email = "aberry2288@gmail.com",
            Url = new Uri("https://adamberrysportfolio.netlify.app")
        }

    });

    string xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"; 
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));

});

builder.Services.AddCors(cors =>
{
    cors.AddPolicy("DefaultPolicy", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});



var app = builder.Build();

app.UseCors("DefaultPolicy");

var scope = app.Services.CreateScope();

await DataUtility.ManageDataAsync(scope.ServiceProvider);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PublicAPI v1");
    c.InjectStylesheet("/css/swagger.css");
    c.InjectJavascript("/js/swagger.js");
    c.DocumentTitle = "My Blog Documentation";
});


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//Custom BlogPost details route
app.MapControllerRoute(
    name: "custom",
    pattern: "Content/{slug}",
    defaults: new { controller = "BlogPosts", action = "Details" }
    );

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BlogPosts}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
