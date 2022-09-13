using Azure.Communication;
using Azure.Communication.CallingServer;
using Azure.Messaging;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new CallAutomationClient(builder.Configuration["ConnectionString"]));

var app = builder.Build();

app.MapPost("/api/calls/{contextId}", async (
    HttpRequest httpRequest,
    [FromRoute] string contextId,
    CallAutomationClient callAutomationClient,
    ILogger<Program> logger) =>
{
    var mayPhoneNumber = "+17809669598";
    try
    {
        var cloudEvents = await httpRequest.ReadFromJsonAsync<CloudEvent[]>();
        foreach (var cloudEvent in cloudEvents)
        {
            var @event = CallAutomationEventParser.Parse(cloudEvent);
            logger.LogInformation($"CorrelationId: {@event.CorrelationId} | CallConnectionId: {@event.CallConnectionId}");

            if (@event.GetType() == typeof(CallConnected))
            {
                // invoke recognize api
                var recognizeOptions = new CallMediaRecognizeDtmfOptions(new PhoneNumberIdentifier(mayPhoneNumber))
                {
                    MaxTonesToCollect = 1,
                    InitialSilenceTimeout = TimeSpan.FromSeconds(10),
                    InterToneTimeout = TimeSpan.FromSeconds(5),
                    InterruptPrompt = true,
                    InterruptCallMediaOperation = true,
                    Prompt = new FileSource(new Uri("https://callingstoragequeue.blob.core.windows.net/ivr/contoso-airlines-main-menu.wav")),
                    OperationContext = "OutboundFlightChange",
                    StopTones = new [] { DtmfTone.Pound }
                };

                await callAutomationClient.GetCallConnection(@event.CallConnectionId).GetCallMedia().StartRecognizingAsync(recognizeOptions);
            }

            if (@event.GetType() == typeof(RecognizeCompleted))
            {
                var recognizeCompleted = @event as RecognizeCompleted;
                var tone = recognizeCompleted.CollectTonesResult.Tones.FirstOrDefault();
                if (tone == DtmfTone.One && recognizeCompleted.OperationContext == "OutboundFlightChange")
                {
                    // play 
                }
            }
        }
    }
    catch (InvalidOperationException)
    {
        
    }

    return Results.Ok();
}).Produces(StatusCodes.Status200OK);

app.Run();
