using Azure.Communication;
using Azure.Communication.CallAutomation;
using Azure.Messaging;
using Microsoft.AspNetCore.Mvc;

var welcomeMessage = new Uri("https://ignite2022storage.blob.core.windows.net/ivr/AdatumAirlines_KAI_FlightOptions.wav");
var optionToChangeEmail = new Uri("https://ignite2022storage.blob.core.windows.net/ivr/AdatumAirlines_KAI_OptionalEmailChange.wav");
var wrapUpWithSurvey = new Uri("https://ignite2022storage.blob.core.windows.net/ivr/AdatumAirlines_KAI_WrapUpWithSurvey.wav");

var activeCalls = new Dictionary<string, string>();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new CallAutomationClient(builder.Configuration["ConnectionString"]));
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

app.MapPost("/api/calls/{contextId}", async (
    HttpRequest httpRequest,
    [FromRoute] string contextId,
    CallAutomationClient callAutomationClient,
    ILogger<Program> logger,
    IConfiguration configuration) =>
{
    var cloudEvents = await httpRequest.ReadFromJsonAsync<CloudEvent[]>();
    foreach (var cloudEvent in cloudEvents)
    {
        var @event = CallAutomationEventParser.Parse(cloudEvent);
        logger.LogInformation($"CorrelationId: {@event.CorrelationId} | CallConnectionId: {@event.CallConnectionId}");

        if (@event is CallConnected)
        {
            if (activeCalls.ContainsKey(@event.ServerCallId)) return Results.Ok();
            activeCalls.Add(@event.ServerCallId, @event.CorrelationId);

            await callAutomationClient
                .GetCallRecording()
                .StartRecordingAsync(new StartRecordingOptions(new ServerCallLocator(@event.ServerCallId)));

            // top level menu
            var recognizeOptions = new CallMediaRecognizeDtmfOptions(new PhoneNumberIdentifier(configuration["CustomerPhoneNumber"]), 1)
            {
                InitialSilenceTimeout = TimeSpan.FromSeconds(10),
                InterruptPrompt = true,
                InterruptCallMediaOperation = true,
                Prompt = new FileSource(welcomeMessage),
                OperationContext = "OutboundFlightChange"
            };

            await callAutomationClient
                .GetCallConnection(@event.CallConnectionId)
                .GetCallMedia()
                .StartRecognizingAsync(recognizeOptions);
        }

        if (@event is RecognizeCompleted recognizeCompleted)
        {
            var tone = recognizeCompleted.CollectTonesResult.Tones.FirstOrDefault();
            if (tone == DtmfTone.One && recognizeCompleted.OperationContext == "OutboundFlightChange")
            {
                // 1 = Flight change options
                var recognizeOptions = new CallMediaRecognizeDtmfOptions(new PhoneNumberIdentifier(configuration["CustomerPhoneNumber"]), 1)
                {
                    InitialSilenceTimeout = TimeSpan.FromSeconds(2),
                    InterruptPrompt = true,
                    InterruptCallMediaOperation = true,
                    Prompt = new FileSource(optionToChangeEmail),
                    OperationContext = "AlternateEmailAddress"
                };

                await callAutomationClient
                    .GetCallConnection(@event.CallConnectionId)
                    .GetCallMedia()
                    .StartRecognizingAsync(recognizeOptions);
            }
        }

        if (@event is RecognizeFailed recognizeFailed)
        {
            if (recognizeFailed.OperationContext == "AlternateEmailAddress" && recognizeFailed.ResultInformation.SubCode is 8510)
            {
                await callAutomationClient
                    .GetCallConnection(@event.CallConnectionId)
                    .GetCallMedia()
                    .PlayToAllAsync(new FileSource(wrapUpWithSurvey));
            }
        }

        if (@event is PlayCompleted)
        {
            await callAutomationClient
                .GetCallConnection(@event.CallConnectionId)
                .HangUpAsync(true);
        }
    }

    return Results.Ok();
}).Produces(StatusCodes.Status200OK);

app.Run();
