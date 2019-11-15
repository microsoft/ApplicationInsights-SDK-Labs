# JavaScript CORE

This project is an attempt to create a new Core API for JavaScript telemetry reporting. The main driver of this work is collaboration with internal Microsoft teams to build a single SDK for 3rd and 1st party customers. 

The reason we need a new API surface is that existing SDK public surface is not generic and extensible enough. Core SDK will allow to converge on data collection and delivery while enabling vendor-specific telemetry types and enrichments.

To make it more specific - there will be a single mechanism collecting page views. However for internal teams, page view telemetry will contain extra properties collected automatically. It may be properties like customer's account details or market region which are specific for Microsoft. There are also be internal-specific telemetry data types that do not have matching experience in Application Insights.

## Requirements and design principles

- Same API for node.js and JavaScript.
- Event produced by SDK should be valid from the endpoint perspective.
- Event produced by Vendor's wrapped Core SDK should be valid from the vendor's perspective. In other words - Application Insights wrapper on Core SDK should not produce events or properties that are not valid from Application Insights perspective.

Non goals:
- Support of a back-end other than Application Insights or MS internal is possible, but not a goal.
- Support telemetry multiplexing (sending one telemetry item into two channels).