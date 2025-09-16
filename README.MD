# ACME - Envío de Pedidos (Prueba Técnica .NET)

La compañía **ACME** requiere implementar un servicio a través de una **API REST** para el ciclo de abastecimiento.  
La tienda *Carrera 70* debe enviar la información de los pedidos en **JSON**, y este servicio debe transformar la solicitud a **SOAP (XML)** y retornar la respuesta en JSON.

---

## requisitos de la prueba

1. Exponer una API REST con mensajería JSON.  
2. Transformar la petición de **JSON → XML** según el mapeo definido.  
3. Consumir el endpoint SOAP y transformar la respuesta **XML → JSON**.  
4. Documentar y ejecutar el proyecto en **Docker**.

---

## Tecnologías usadas

- .NET 8 (Minimal API)  
- C#  
- Docker  
- HttpClient (para llamada SOAP)  

---

## Estructura del proyecto
acme-envio-pedidos/
├── Models/ # Clases DTO de entrada y salida
├── Services/ # Servicio SoapClientService (transformación JSON <-> XML)
├── Program.cs # Minimal API
├── Dockerfile # Imagen Docker
├── acme-envio-pedidos.csproj
└── README.md


---

## ▶️ Ejecución local con Docker

### 1. Construir la imagen

docker build -t acme-envio-pedidos .
### 2. Levantar el contenedor
docker run -d -p 5050:8080 --name acme-envio-pedidos acme-envio-pedidos

	La API quedará escuchando en:
		👉 http://localhost:5050
### 3.Uso de la api
POST /api/pedidos/enviar

### 4.Request json
{
  "enviarPedido": {
    "numPedido": "75630275",
    "cantidadPedido": "1",
    "codigoEAN": "00110000765191002104587",
    "nombreProducto": "Armario INVAL",
    "numDocumento": "1113987400",
    "direccion": "CR 72B 45 12 APT 301"
  }
}


### 5.Respuesta esperada
{
  "enviarPedidoRespuesta": {
    "codigoEnvio": "80375472",
    "estado": "Entregado exitosamente al cliente"
  }
}
### 6. Prueba Con Postman

Importa el endpoint POST http://localhost:5050/api/pedidos/enviar.

Envía el JSON de prueba.

Deberías recibir el JSON de respuesta mapeado.

# Autor

👨‍💻 Desarrollado por Oscar Espejo