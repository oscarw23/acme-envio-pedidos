using acme_envio_pedidos.Models;
using acme_envio_pedidos.Services;
using Microsoft.AspNetCore.Mvc;

namespace acme_envio_pedidos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly SoapClientService _soap;
        private readonly IConfiguration _cfg;

        public PedidosController(SoapClientService soap, IConfiguration cfg)
        {
            _soap = soap; _cfg = cfg;
        }

        [HttpPost("enviar")]
        public async Task<ActionResult<RespuestaSalida>> Enviar([FromBody] PedidoEntrada entrada, CancellationToken ct)
        {
            var ep = _cfg["SOAP_ENDPOINT"] ?? "https://run.mocky.io/v3/19217075-6d4e-4818-98bc-416d1feb7b84";
            var p = entrada.enviarPedido;

            var (codigo, mensaje) = await _soap.EnviarPedidoAsync(
                ep, p.numPedido, p.cantidadPedido, p.codigoEAN, p.nombreProducto, p.numDocumento, p.direccion, ct);

            return Ok(new RespuestaSalida(new RespuestaDto(codigo, mensaje)));
        }
    }
}
