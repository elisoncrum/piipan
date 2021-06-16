# Log streaming

In keeping with [NIST SP 800-53 control AU-3](https://csrc.nist.gov/Projects/risk-management/sp800-53-controls/release-search#!/control?version=4.0&number=AU-3), resource logs are streamed to a central location where they can be accessed by a SIEM tool such as Splunk. This is accomplished in Azure using a combination of [Event Hub](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-about), resource [diagnostic settings](https://docs.microsoft.com/en-us/azure/azure-monitor/essentials/diagnostic-settings?tabs=CMD), and an [application registration](https://docs.microsoft.com/en-us/azure/event-hubs/authenticate-application) for accessing and reading logs.

## Event Hub configuration

All resource logs are streamed to a central Event Hub. The IaC establishes the following configuration in each deployment environment:

- A single Event Hub namespace (`evh-monitoring`), with the default `RootManageSharedAccessKey` shared access policy
- Within the namespace, a single event hub named `logs`, with the default `$Default` consumer group

## Resource configuration

Each resource (Function App, database cluster, etc) is configured with a diagnostic setting that sends all logs to the event hub. For each resource, the IaC establishes a single diagnostic setting named `stream-logs-to-event-hub` that is configured to:

- Stream all desired logging categories
- Stream logs to the `logs` event hub within the `evh-monitoring` namespace
- Use the default `RootManageSharedAccessKey` as the event hub policy

Resources have two categories of logging: "logs" and "metrics". Our default practice is to stream all logs categories and no metrics categories. Categories can be enabled/disabled as required — i.e., if the team analyzing audit logs determines a certain log category to produce too much noise and be unnecessary.

## Application logging

Some details required for NIST compliance are not built into default resource logging. For example, AU-3 requires logging the "identity of any individuals or subjects associated with the event." However, when a Function app that sits behind EasyAuth is called via a managed identity the identity information is not included in any built-in logging.

In these cases, log messages are explicitly written at the application level:

```
log.LogInformation("Executing request from user {User}", req.HttpContext?.User.Identity.Name);
```

## Accessing and reading logs

The IaC creates an application registration that can be used by an external SIEM tool to access and read logs. The application registration is explicitly granted the `Azure Event Hubs Data Receive` role on the `logs` event hub.
