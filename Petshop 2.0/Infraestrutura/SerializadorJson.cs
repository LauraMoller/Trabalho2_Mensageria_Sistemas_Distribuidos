using System.Text.Json;

namespace PetShopMensageria.Infraestrutura;

public static class SerializadorJson
{
    private static readonly JsonSerializerOptions Opcoes = new() { PropertyNameCaseInsensitive = true };

    public static byte[] Serializar(object evento) =>
        JsonSerializer.SerializeToUtf8Bytes(evento, evento.GetType(), Opcoes);

    public static T Desserializar<T>(ReadOnlyMemory<byte> corpo) =>
        JsonSerializer.Deserialize<T>(corpo.Span, Opcoes) ?? throw new JsonException("Mensagem vazia.");
}
