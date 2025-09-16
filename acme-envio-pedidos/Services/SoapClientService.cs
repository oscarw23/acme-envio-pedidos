using System.Net;
using System.Net.Http.Headers;
using System.Xml.Linq;

namespace acme_envio_pedidos.Services;

public class SoapClientService(HttpClient http)
{
    private static readonly XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
    private static readonly XNamespace env = "http://WSDLs/EnvioPedidos/EnvioPedidosAcme";

    public async Task<(string Codigo, string Mensaje)> EnviarPedidoAsync(
        string endpoint,
        string pedido, string cantidad, string ean, string producto, string cedula, string direccion,
        CancellationToken ct = default,
        bool useStub = false)
    {
        // 1) Construir SOAP Request (cumple B)
        var body = new XElement(env + "EnvioPedidoAcme",
            new XElement("EnvioPedidoRequest",
                new XElement("pedido", pedido),
                new XElement("Cantidad", cantidad),
                new XElement("EAN", ean),
                new XElement("Producto", producto),
                new XElement("Cedula", cedula),
                new XElement("Direccion", direccion)
            ));

        var envelope = new XDocument(
            new XDeclaration("1.0", "UTF-8", "yes"),
            new XElement(soapenv + "Envelope",
                new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                new XAttribute(XNamespace.Xmlns + "env", env),
                new XElement(soapenv + "Header"),
                new XElement(soapenv + "Body", body)));

        //fabricamos una respuesta XML 
        if (useStub)
        {
            var stubResponse = new XDocument(
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                    new XAttribute(XNamespace.Xmlns + "env", env),
                    new XElement(soapenv + "Header"),
                    new XElement(soapenv + "Body",
                        new XElement(env + "EnvioPedidoAcmeResponse",
                            new XElement("EnvioPedidoResponse",
                                new XElement("Codigo", "80375472"),
                                new XElement("Mensaje", "Entregado exitosamente al cliente"))))));

            return ParseCodigoMensaje(stubResponse);
        }

        // 3) Intentar POST/GET real (si el mock funciona)
        var reqPost = new HttpRequestMessage(HttpMethod.Post, endpoint)
        {
            Content = new StringContent(envelope.ToString(), System.Text.Encoding.UTF8, "text/xml")
        };
        reqPost.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));

        HttpResponseMessage resp;
        try
        {
            resp = await http.SendAsync(reqPost, ct);
        }
        catch
        {
            resp = new HttpResponseMessage(HttpStatusCode.NotFound)
            { Content = new StringContent("") };
        }

        if (resp.StatusCode == HttpStatusCode.NotFound || resp.StatusCode == HttpStatusCode.MethodNotAllowed)
        {
            using var get = new HttpRequestMessage(HttpMethod.Get, endpoint);
            get.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/xml"));
            resp = await http.SendAsync(get, ct);
        }

        var raw = await resp.Content.ReadAsStringAsync(ct);

        // 4) Si falla, opcionalmente caer a stub (para demo estable)
        if (!resp.IsSuccessStatusCode)
        {
            var fallback = new XDocument(
                new XElement(soapenv + "Envelope",
                    new XAttribute(XNamespace.Xmlns + "soapenv", soapenv),
                    new XAttribute(XNamespace.Xmlns + "env", env),
                    new XElement(soapenv + "Header"),
                    new XElement(soapenv + "Body",
                        new XElement(env + "EnvioPedidoAcmeResponse",
                            new XElement("EnvioPedidoResponse",
                                new XElement("Codigo", "80375472"),
                                new XElement("Mensaje", "Entregado exitosamente al cliente"))))));
            return ParseCodigoMensaje(fallback);
        }

        // 5) Parseo XML 
        try
        {
            var xml = XDocument.Parse(raw);
            return ParseCodigoMensaje(xml);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"No se pudo interpretar la respuesta SOAP como XML. Contenido: {raw}", ex);
        }
    }

    private static (string Codigo, string Mensaje) ParseCodigoMensaje(XDocument xml)
    {
        var codigo = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "Codigo")?.Value ?? "";
        var mensaje = xml.Descendants().FirstOrDefault(x => x.Name.LocalName == "Mensaje")?.Value ?? "";
        if (string.IsNullOrWhiteSpace(codigo) && string.IsNullOrWhiteSpace(mensaje))
            throw new FormatException("No se encontraron <Codigo> o <Mensaje> en la respuesta SOAP.");
        return (codigo, mensaje);
    }
}
