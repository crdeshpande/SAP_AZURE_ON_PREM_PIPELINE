using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using SapIntegration;

public class SapBridgeFunction
{
    private readonly ILogger<SapBridgeFunction> _logger;
    private readonly ISapConnector _sapConnector;

    public SapBridgeFunction(ILogger<SapBridgeFunction> logger)
    {
        _logger = logger;

        // This demonstrates a mock setup. In a real application,
        // you would use dependency injection to provide a configured ISapConnector instance.
        // The connection string would be sourced from Azure Key Vault via application settings.
        var sapConnectionString = Environment.GetEnvironmentVariable("SAP_CONNECTION_STRING") ?? "mock_connection_string";
        _sapConnector = new SapConnector(sapConnectionString, new LoggerFactory().CreateLogger<SapConnector>());
    }

    [Function("ExecuteBapi")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "bapi/{bapiName}")] HttpRequest req,
        string bapiName)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request to execute BAPI: {BapiName}", bapiName);

        if (string.IsNullOrEmpty(bapiName))
        {
            return new BadRequestObjectResult("BAPI name must be provided in the route.");
        }

        string requestBody;
        using (var reader = new StreamReader(req.Body))
        {
            requestBody = await reader.ReadToEndAsync();
        }

        var parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(requestBody) ?? new Dictionary<string, object>();

        try
        {
            var result = await _sapConnector.ExecuteBapiAsync(bapiName, parameters);
            return new OkObjectResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing BAPI {BapiName}", bapiName);
            return new StatusCodeResult(StatusCodes.Status500InternalServerError);
        }
    }
}
