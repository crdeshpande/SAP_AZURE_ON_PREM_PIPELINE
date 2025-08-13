using System.Text.Json;

namespace SapIntegration;

/// <summary>
/// Represents a mock SAP Connector to simulate BAPI/RFC calls.
/// In a real-world scenario, this class would use the SAP .NET Connector (NCo)
/// to establish a connection to an SAP ABAP system.
/// </summary>
public interface ISapConnector
{
    Task<string> ExecuteBapiAsync(string bapiName, Dictionary<string, object> parameters);
}

public class SapConnector : ISapConnector
{
    private readonly string _connectionString;
    private readonly ILogger<SapConnector> _logger;

    public SapConnector(string connectionString, ILogger<SapConnector> logger)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger;

        if (_connectionString.Contains("PLACEHOLDER"))
        {
            _logger.LogWarning("SAP Connector is using a placeholder connection string.");
        }
    }

    /// <summary>
    /// Simulates executing a BAPI call to SAP.
    /// </summary>
    /// <param name="bapiName">The name of the BAPI to execute.</param>
    /// <param name="parameters">The input parameters for the BAPI.</param>
    /// <returns>A JSON string representing the BAPI result.</returns>
    public Task<string> ExecuteBapiAsync(string bapiName, Dictionary<string, object> parameters)
    {
        _logger.LogInformation("Attempting to execute BAPI: {BapiName}", bapiName);

        // Simulate a real BAPI call.
        // Here you would use the SAP NCo library to connect to SAP,
        // create a function object, set parameters, and execute it.

        // For this mock, we'll just return a simulated success response.
        var mockResult = new
        {
            Success = true,
            TransactionId = Guid.NewGuid().ToString(),
            Message = $"Successfully executed BAPI '{bapiName}' (simulated).",
            Data = new
            {
                SalesOrderNumber = $"SO-{new Random().Next(1000, 9999)}",
                Customer = parameters.GetValueOrDefault("Customer", "Unknown"),
                Amount = parameters.GetValueOrDefault("Amount", 0)
            }
        };

        _logger.LogInformation("BAPI {BapiName} executed successfully (simulated).", bapiName);

        string jsonResult = JsonSerializer.Serialize(mockResult);
        return Task.FromResult(jsonResult);
    }
}
