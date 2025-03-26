using BlogA.DAL;
using BlogA.Services;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Env.Load();
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllers();
builder.Services.AddControllers()
    .AddJsonOptions(
        options => options.JsonSerializerOptions.PropertyNamingPolicy = null);

builder.Services.Configure<DatabaseConfig>(builder.Configuration.GetSection("ConnectionStrings"));
builder.Services.AddSingleton<DatabaseConnection>();
builder.Services.AddSingleton<ReaderDBConnection>();
builder.Services.AddSingleton<Sequence>();
builder.Services.AddSingleton<BookService>();
builder.Services.AddSingleton<ChapterService>();
builder.Services.AddSingleton<CommentService>();
builder.Services.AddSingleton<AuthorService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder => builder
            .AllowAnyOrigin()  
            .AllowAnyMethod()  
            .AllowAnyHeader()); 
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();

}
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
