using Azure.Communication;
using Azure.Communication.CallingServer;

#region Secrets
var connectionString = "[INSERT_CONNECTION_STRING_HERE]";
var callerIdNumber = "[INSERT_ACS_NUMBER_HERE]";
var targetPhoneNumber = "[INSERT_DESTINATION_PSTN_NUMBER_HERE]";
var callbackHost = "[INSERT_HOST_HERE]";
#endregion

var callAutomationClient = new CallAutomationClient(connectionString);
var callbackUri = new Uri($"{callbackHost}/api/calls/{Guid.NewGuid()}");
var sourceUserId = new CommunicationUserIdentifier("8:acs:cb089178-54ed-4d9e-9763-49510601f789_5bb7b304-4582-47d4-a0c1-1f763b4b9ecd");
var callSource = new CallSource(sourceUserId)
{
    CallerId = new PhoneNumberIdentifier(callerIdNumber)
};
var target = new List<CommunicationIdentifier> { new PhoneNumberIdentifier(targetPhoneNumber) };

await callAutomationClient.CreateCallAsync(new CreateCallOptions(callSource, target, callbackUri));
