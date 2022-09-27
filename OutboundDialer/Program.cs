using Azure.Communication;
using Azure.Communication.CallingServer;

#region Secrets
var connectionString = "endpoint=https://ignite-2022.communication.azure.com/;accesskey=w1uXtPWsrkselQKpBIQ09avV1U0pig6ff78iCju+rKa032Lc4lAcw/Vgg95q2jjH69x5nZajy0aU4uZh3KHSiA==";
var callerIdNumber = "+18333245465";
var targetPhoneNumber = "+17809669598";
#endregion

var callAutomationClient = new CallAutomationClient(connectionString);
var callbackUri = new Uri($"https://3ws6tpp5-7154.usw2.rel.tunnels.api.visualstudio.com/api/calls/{Guid.NewGuid()}");
var sourceUserId = new CommunicationUserIdentifier("8:acs:cb089178-54ed-4d9e-9763-49510601f789_5bb7b304-4582-47d4-a0c1-1f763b4b9ecd");
var callSource = new CallSource(sourceUserId)
{
    CallerId = new PhoneNumberIdentifier(callerIdNumber)
};
var target = new List<CommunicationIdentifier> { new PhoneNumberIdentifier(targetPhoneNumber) };

await callAutomationClient.CreateCallAsync(new CreateCallOptions(callSource, target, callbackUri));
