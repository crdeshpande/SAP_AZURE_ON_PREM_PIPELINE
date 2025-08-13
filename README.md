# Integration Architecture: Azure Logic Apps with SAP HANA and Azure SQL

This project provides a sample implementation and scaffold for the architecture described in the "Integration Scenario: Azure Logic Apps with SAP HANA on Private Cloud and Azure SQL Database" document. It demonstrates how to connect Azure Logic Apps to a classic SAP system using a custom API layer (Azure Functions) and how to integrate with Azure SQL Database.

## 1. Solution Components

The solution is comprised of two main parts:

1.  **`SapBridgeFunction/`**: A C# Azure Function App that acts as a secure bridge for making BAPI/RFC calls to an SAP ABAP system.
2.  **`LogicApp/`**: A sample JSON definition for an Azure Logic App workflow that orchestrates the integration.

---

## 2. Azure Function - SAP Bridge (`SapBridgeFunction`)

As recommended in the architecture, Logic Apps do not have a native connector for BAPI/RFC calls. This Azure Function acts as the "Custom API Layer" to bridge that gap.

### Key Features:

-   **HTTP Trigger**: Exposes an endpoint that Logic Apps can call easily (`/api/bapi/{bapiName}`).
-   **Mock SAP Connector (`SapConnector.cs`)**: Simulates a connection to SAP using the SAP .NET Connector (NCo). In a real implementation, you would replace the mock logic with actual calls to your SAP system using the NCo libraries.
-   **Secure by Design**:
    -   It reads the `SAP_CONNECTION_STRING` from environment variables, which should be configured in the Function App's settings and backed by Azure Key Vault.
    -   The HTTP trigger is set to `AuthorizationLevel.Function`, requiring an API key for access.

### How to Deploy (Conceptual):

1.  Publish the Function App to Azure.
2.  In the Azure Portal, configure the Function App's Application Settings:
    -   `SAP_CONNECTION_STRING`: The actual connection string for your SAP system.
    -   `KEY_VAULT_URI`: The URI of your Azure Key Vault for storing secrets.
    -   Use Key Vault references for sensitive settings.
3.  Enable Application Insights for logging and monitoring, as per the `host.json` configuration.

---

## 3. Azure Logic App (`LogicApp/workflow.json`)

This JSON file is a template for the Logic App workflow. It demonstrates the orchestration pattern.

### Workflow Steps:

1.  **HTTP Trigger**: The workflow starts when it receives a POST request with customer and product data.
2.  **Call SAP Bridge Function**: It calls the deployed `SapBridgeFunction` to create a sales order in SAP (simulated).
    -   It securely passes the function's API key.
    -   The URL for the function is parameterized and should be set in the Logic App's parameters.
3.  **Get Product Details from SQL**: It connects to an Azure SQL Database using the built-in SQL Server connector to retrieve additional product information.
    -   The connection string is parameterized and should be stored securely.
4.  **Response**: It combines the results from the SAP call and the SQL query and returns them to the original caller.

### Best Practices Followed:

-   **Parameterization**: Connection strings and endpoint URLs are parameterized, not hard-coded.
-   **Secure Inputs/Outputs**: For a production workload, you would enable Secure Inputs/Outputs on each action to prevent sensitive data from appearing in run history.
-   **Managed Identity**: The connection to Azure SQL should be configured to use the Logic App's Managed Identity for passwordless authentication.

---

## 4. Overall Architecture Alignment

This sample aligns with the provided document's recommendations:

-   **Gateway Pattern**: The Azure Function serves a similar purpose to the On-Premises Data Gateway (OPDG) but for the application layer (BAPI/RFC) instead of the database layer. For connecting to the **SAP HANA DB directly**, you would still use the OPDG as outlined in the document.
-   **Security**: Emphasizes the use of Key Vault, managed identities, and secure configuration.
-   **Audit Logging**: The Azure Function is configured to send logs to Application Insights. The Logic App, by default, sends its run history to Azure Monitor. Both SAP HANA and Azure SQL should have their respective auditing enabled.
-   **Decoupling**: The architecture is decoupled. The Logic App doesn't need to know the complexities of connecting to SAP; it just calls a simple REST API.

To make this a complete, runnable solution, you would need to deploy these components to Azure and configure the connections with your live SAP and SQL environments.
