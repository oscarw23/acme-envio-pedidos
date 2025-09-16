
using acme_envio_pedidos.Models;
using acme_envio_pedidos.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHttpClient<SoapClientService>()
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });

var app = builder.Build();
var useStub = builder.Configuration["USE_STUB"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;


app.UseDeveloperExceptionPage(); 

app.MapPost("/api/pedidos/enviar", async (PedidoEntrada? entrada, SoapClientService soap, IConfiguration cfg, CancellationToken ct) =>
{
    if (entrada?.enviarPedido is null)
        return Results.BadRequest(new { message = "Body inválido. Se esperaba { \"enviarPedido\": { ... } }" });

    var ep = cfg["SOAP_ENDPOINT"] ?? "https://run.mocky.io/v3/19217075-6d4e-4818-98bc-416d1feb7b84";


    if (cfg["USE_STUB"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
    {
        return Results.Ok(new RespuestaSalida(
            new RespuestaDto("80375472", "Entregado exitosamente al cliente")
        ));
    }

    var p = entrada.enviarPedido;

    try
    {
        var (codigo, mensaje) = await soap.EnviarPedidoAsync(
            ep, p.numPedido, p.cantidadPedido, p.codigoEAN, p.nombreProducto, p.numDocumento, p.direccion, ct);

        return Results.Ok(new RespuestaSalida(new RespuestaDto(codigo, mensaje)));
    }
    catch (Exception ex)
    {
        return Results.Problem(title: "Error SOAP", detail: ex.Message, statusCode: 502);
    }
});

app.Run();
