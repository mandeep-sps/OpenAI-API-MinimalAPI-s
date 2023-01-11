using Microsoft.Extensions.DependencyInjection;
using OpenAI.GPT3.Extensions;
using OpenAI.GPT3.Interfaces;
using OpenAI.GPT3.ObjectModels.RequestModels;
using OpenAI.GPT3.ObjectModels;
using static OpenAI.GPT3.ObjectModels.SharedModels.IOpenAiModels;
using System.Drawing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenAIGPT_3;
using OpenAI.GPT3.Managers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var serviceCollection = new ServiceCollection();
// Open AI API keys/secret //
serviceCollection.AddOpenAIService(settings =>
{
    settings.ApiKey = "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";
});
#pragma warning disable ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
var serviceProvider = serviceCollection.BuildServiceProvider();
#pragma warning restore ASP0000 // Do not call 'IServiceCollection.BuildServiceProvider' in 'ConfigureServices'
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Create Completion Sample Data//
app.MapPost("/CreateCompletionAsync", async (CreateCompletionModel model) =>
{
    try
    {
        var openAiService = serviceProvider.GetRequiredService<IOpenAIService>();
        var completionResult = await openAiService.Completions.CreateCompletion(new CompletionCreateRequest()
        {
            Prompt = model.Text,
            Temperature = 1,
            MaxTokens = 5,
            LogProbs = 1
        }, Models.Davinci);

        if (completionResult.Successful)
        {
            var Data = completionResult.Choices.ToList();
            return Results.Ok(completionResult.Choices.FirstOrDefault());
        }
        else
        {
            if (completionResult.Error == null)
            {
                throw new Exception("Unknown Error");
            }
            return Results.Ok($"{completionResult.Error.Code}: {completionResult.Error.Message}");
        }
    }
    catch (Exception ex)
    {

        return Results.BadRequest(ex.Message);
    }
    


})
.WithName("CreateCompletionAsync")
.WithOpenApi();

// Create Image from Text//
app.MapPost("/CreateImageFromText", async (BestMatchRequestModel model) =>
{
    var openAiService = serviceProvider.GetRequiredService<IOpenAIService>();
#pragma warning disable CS8601 // Possible null reference assignment.
    var completionResult = await openAiService.Image.CreateImage(new ImageCreateRequest
    {
        Prompt = model.Text,
        N = 2,
        Size = StaticValues.ImageStatics.Size.Size256,
        ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
        User = model.Username
    });
#pragma warning restore CS8601 // Possible null reference assignment.
    if (completionResult.Successful)
    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var Data = completionResult.Results.FirstOrDefault().Url;
#pragma warning restore CS8602 // Dereference of a possibly null reference.
        return Results.Ok(Data);
    }
    else
    {
        if (completionResult.Error == null)
        {
            throw new Exception("Unknown Error");
        }
        return Results.Ok($"{completionResult.Error.Code}: {completionResult.Error.Message}");
    }


})
.WithName("CreateImageFromText")
.WithOpenApi();

// Fetch all the Models//
app.MapGet("/FetchModels", async () =>
{
    List<FetchModelData> MainList = new List<FetchModelData>();
    FetchModelData ObjModel;
    var openAiService = serviceProvider.GetRequiredService<IOpenAIService>();
    var engineList = await openAiService.Models.ListModel();
    if (engineList == null)
    {
        return Results.NoContent();
    }
    foreach (var engineItem in engineList.Models)
    {
        ObjModel = new FetchModelData();
        var retrieveEngineResponse = await openAiService.Models.RetrieveModel(engineItem.Id);
        if (retrieveEngineResponse.Successful)
        {
            ObjModel.ID = retrieveEngineResponse.Id;
            ObjModel.Owner = retrieveEngineResponse.Owner;
            ObjModel.Text = retrieveEngineResponse.Root;
            MainList.Add(ObjModel);
        }
        else
        {
            return Results.BadRequest($"Retrieving {engineItem.Id} Model failed");
        }
    }
    return Results.Ok(MainList);
    
})
.WithName("FetchModels")
.WithOpenApi();

await app.RunAsync();
