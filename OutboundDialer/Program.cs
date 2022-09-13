using Azure.Communication;
using Azure.Communication.CallingServer;

var callAutomationClient =
    new CallAutomationClient(
        "endpoint = https://jassha.communication.azure.com/;accesskey=ZLjtIBwWyDhWRoMguG+YZScz5YugDV7prAwzzi6QyK14YXtOiGqXEM4hgKyyq7Vj2xY8XHURX1q0MWMoNDDJnA==");

var callbackUri = new Uri($"https://e196-75-155-253-232.ngrok.io/api/calls/{Guid.NewGuid()}");
var source = new CommunicationUserIdentifier("8:acs:eba32226-8a75-47dc-afa3-cbbe8e84bc95_5bb7b304-4582-47d4-a0c1-1f763b4b9ecd");
var callSource = new CallSource(source)
{
    CallerId = new PhoneNumberIdentifier("+18336392154")
};
var target = new List<CommunicationIdentifier> { new PhoneNumberIdentifier("+17809669598") };

await callAutomationClient.CreateCallAsync(new CreateCallOptions(callSource, target, callbackUri));
