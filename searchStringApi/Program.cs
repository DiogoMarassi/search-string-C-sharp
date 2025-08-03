using MyApp.Services;
using MyApp.Utilities;
using MyApp.Services.pdf;
using MyApp.Services.String;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Singleton, Scoped e Transient são ciclos de vida (lifetimes) de dependências no Dependency Injection (DI) Container
// Eles controlam quanto tempo um objeto injetado vive e como ele é compartilhado.

//Singleton
//Cria UMA instância para toda a aplicação
//A mesma instância é usada em todas as requisições e por todos os controllers
//Só morre quando a aplicação fecha

//Scoped
//Cria UMA instância por requisição HTTP
//Cada request recebe uma instância nova,
//mas todos os controllers e serviços dentro da mesma requisição compartilham essa instância.

//Transient
//Cria uma nova instância TODA vez que é injetado ou chamado
//Vive por pouco tempo, morre quando não há mais referência

builder.Services.AddSingleton<CermineRunner>();
builder.Services.AddSingleton<XmlArticleExtractor>();
builder.Services.AddScoped<PdfReaderService>();

builder.Services.AddScoped<PdfReaderService>();
builder.Services.AddScoped<TextPreprocessing>();
builder.Services.AddScoped<NGrams>();
builder.Services.AddScoped<Embedding>(sp =>
{
    var apiKey = Environment.GetEnvironmentVariable("NOMIC_API_KEY");
    return new Embedding(apiKey ?? throw new Exception("Nomic API Key não encontrada"));
});

builder.Services.AddScoped<StringGeneratorService>();

// Adiciona serviços de controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(); 

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

