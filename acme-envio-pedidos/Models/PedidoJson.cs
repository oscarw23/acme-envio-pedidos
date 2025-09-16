namespace acme_envio_pedidos.Models
{
    public record PedidoEntrada(PedidoDto enviarPedido);
    public record PedidoDto(
        string numPedido,
        string cantidadPedido,
        string codigoEAN,
        string nombreProducto,
        string numDocumento,
        string direccion
    );
}
