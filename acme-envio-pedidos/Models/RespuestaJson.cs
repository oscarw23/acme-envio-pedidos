namespace acme_envio_pedidos.Models
{
    public record RespuestaSalida(RespuestaDto enviarPedidoRespuesta);
    public record RespuestaDto(string codigoEnvio, string estado);
}
