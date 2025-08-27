using LetterAPI.Models;
using LetterAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<SenderSettings>(
    builder.Configuration.GetSection("Sender"));
builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<Microsoft.Extensions.Options.IOptions<SenderSettings>>().Value);
builder.Services.AddSingleton<IHtmlTemplateSetup, HtmlTemplateSetup>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("LetterAppCors",
        b => b.WithOrigins("https://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseCors("LetterAppCors");
app.Run();
