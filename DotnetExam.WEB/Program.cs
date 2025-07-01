
using DotnetExam.Data;
using DotnetExam.GraphQL;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataConfigured(builder.Configuration.GetConnectionString("ClickHouse")!);
builder.Services.AddGraphQlConfigured();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGraphQL();
app.Run();
