using Azure.Communication.CallAutomation;
using Azure.Messaging;
using EnhancedCallHandler;
using JasonShave.Azure.Communication.Service.EventHandler;
using JasonShave.Azure.Communication.Service.EventHandler.CallAutomation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new CallAutomationClient(builder.Configuration["ConnectionString"]));
builder.Services.AddHostedService<CallHandlerService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapPost("/api/calls/{contextId}", (
    CloudEvent[] cloudEvents,
    [FromRoute] string contextId,
    IEventPublisher<Calling> publisher) =>
{
    publisher.Publish(cloudEvents);
    return Results.Ok();
}).Produces(StatusCodes.Status200OK);

app.Run();
