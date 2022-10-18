using Azure.Communication;
using Azure.Communication.CallAutomation;
using JasonShave.Azure.Communication.Service.EventHandler.CallAutomation;

namespace EnhancedCallHandler;

public class CallHandlerService : BackgroundService
{
    private readonly ICallAutomationEventSubscriber _subscriber;
    private readonly CallAutomationClient _callAutomationClient;
    private readonly IConfiguration _configuration;
    private readonly Dictionary<string, string> _activeCalls = new();

    private readonly Uri _welcomeMessage = new("YOUR_WELCOME_FILE");
    private readonly Uri _optionToChangeEmail = new("YOUR_EMAIL_CHANGE_FILE");
    private readonly Uri _wrapUpWithSurvey = new("YOUR_WRAP_UP_SURVEY_FILE");

    public CallHandlerService(
        ICallAutomationEventSubscriber subscriber,
        CallAutomationClient callAutomationClient,
        IConfiguration configuration)
    {
        _subscriber = subscriber;
        _callAutomationClient = callAutomationClient;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _subscriber.OnCallConnected += HandleCallConnected;
        _subscriber.OnPlayCompleted += HandlePlayCompleted;
        _subscriber.OnRecognizeCompleted += HandleRecognizeCompleted;
        _subscriber.OnRecognizeFailed += SubscriberOnOnRecognizeFailed;

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }
    
    private async ValueTask HandleCallConnected(CallConnected callConnected, string? contextId)
    {
        if (_activeCalls.ContainsKey(callConnected.ServerCallId)) return;
        _activeCalls.Add(callConnected.ServerCallId, callConnected.CorrelationId);

        // top level menu
        var recognizeOptions = new CallMediaRecognizeDtmfOptions(
            new PhoneNumberIdentifier(_configuration["CustomerPhoneNumber"]), 1)
        {
            InitialSilenceTimeout = TimeSpan.FromSeconds(10),
            InterruptPrompt = true,
            InterruptCallMediaOperation = true,
            Prompt = new FileSource(_welcomeMessage),
            OperationContext = "OutboundFlightChange"
        };

        await _callAutomationClient
            .GetCallConnection(callConnected.CallConnectionId)
            .GetCallMedia()
            .StartRecognizingAsync(recognizeOptions);
    }

    private async ValueTask HandleRecognizeCompleted(RecognizeCompleted recognizeCompleted, string? contextId)
    {
        var tone = recognizeCompleted.CollectTonesResult.Tones.FirstOrDefault();
        if (tone == DtmfTone.One && recognizeCompleted.OperationContext == "OutboundFlightChange")
        {
            // 1 = Flight change options
            var recognizeOptions = new CallMediaRecognizeDtmfOptions(new PhoneNumberIdentifier(_configuration["CustomerPhoneNumber"]), 1)
            {
                InitialSilenceTimeout = TimeSpan.FromSeconds(2),
                InterruptPrompt = true,
                InterruptCallMediaOperation = true,
                Prompt = new FileSource(_optionToChangeEmail),
                OperationContext = "AlternateEmailAddress"
            };

            await _callAutomationClient
                .GetCallConnection(recognizeCompleted.CallConnectionId)
                .GetCallMedia()
                .StartRecognizingAsync(recognizeOptions);
        }
    }

    private async ValueTask SubscriberOnOnRecognizeFailed(RecognizeFailed recognizeFailed, string? contextId)
    {
        if (recognizeFailed.OperationContext == "AlternateEmailAddress" && recognizeFailed.ResultInformation.SubCode is 8510)
        {
            // timeout on choice to change email address
            await _callAutomationClient
                .GetCallConnection(recognizeFailed.CallConnectionId)
                .GetCallMedia()
                .PlayToAllAsync(new FileSource(_wrapUpWithSurvey));
        }
    }

    private async ValueTask HandlePlayCompleted(PlayCompleted playCompleted, string? contextId)
    {
        await _callAutomationClient
            .GetCallConnection(playCompleted.CallConnectionId)
            .HangUpAsync(true);
    }
}