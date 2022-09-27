# Call Automation Ignite 2022 Demo

This project contains three applications which were used in the (upcoming) Ignite 2022 on-demand video recording and the recent Azure Communication Services TAP webcast.

The scenario involves a fictitious airline called "Adatum Airlines". They are trying to reach one of their customers to inform them about a flight change. The caller has the option of pressing 1 on their phone to have the flight options emailed to them with a sub-menu allowing them to press 1 to change their target email address.

## OutboundDialer project

This project performs the outbound call. Fill in the `Secrets` region with the necessary information to place an outbound call:

```csharp
#region Secrets
var connectionString = "[INSERT_CONNECTION_STRING_HERE]";
var callerIdNumber = "[INSERT_ACS_NUMBER_HERE]";
var targetPhoneNumber = "[INSERT_DESTINATION_PSTN_NUMBER_HERE]";
var callbackHost = "[INSERT_HOST_HERE]";
var applicationId = "[INSERT_ACS_ID_HERE]";
#endregion
```

## CallHandler project

This project contains the ASP.NET 6 web API project which will receive the Webhook callback events. This project has also been configured to use a [Visual Studio 2022 Preview feature called "Port Tunnelling"](https://devblogs.microsoft.com/visualstudio/introducing-private-preview-port-tunneling-visual-studio-for-asp-net-core-projects/) enabling Webhook callbacks without the need for NGROK. To configure the required configuration settings:

1. Right-click on the `CallHandler` project in Visual Studio 2022 and choose **Manage User Secrets**.
2. Enter the following JSON information to configure the application:

   ```JSON
   {
       "ConnectionString": "[INSERT_CONNECTION_STRING_HERE]",
       "CustomerPhoneNumber": "[INSERT_DESTINATION_PSTN_NUMBER_HERE]"
   }
   ```

## EnhancedCallHandler project

This project enhances event handling, deserialization, and casting by abstracting the business logic to a `CallHandlerService` class. This project is not required for the solution to function, but rather included to demonstrate an alternative approach.
