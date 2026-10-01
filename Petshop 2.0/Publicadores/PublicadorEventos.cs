using PetShopMensageria.Eventos;
using PetShopMensageria.Infraestrutura;
using RabbitMQ.Client;

namespace PetShopMensageria.Publicadores;

public sealed class PublicadorEventos : IDisposable
{
    private readonly IModel _canal;
    private readonly object _trava = new();

    public PublicadorEventos(IConnection conexao)
    {
        _canal = conexao.CreateModel();
        _canal.ConfirmSelect();
        _canal.BasicReturn += (_, ea) =>
            RegistroEventos.Registrar("Produtor", "erro", $"Mensagem NÃO roteada para nenhuma fila (RK '{ea.RoutingKey}').");
    }

    public void Publicar(string routingKey, EventoPet evento)
    {
        byte[] corpo = SerializadorJson.Serializar(evento);

        lock (_trava)
        {
            var props = _canal.CreateBasicProperties();
            props.Persistent = true;
            props.MessageId = Guid.NewGuid().ToString();
            props.ContentType = "application/json";
            props.Type = evento.Evento;

            _canal.BasicPublish(Topologia.ExchangePrincipal, routingKey, mandatory: true, props, corpo);
            _canal.WaitForConfirmsOrDie(TimeSpan.FromSeconds(5));
        }

        RegistroEventos.Registrar("Produtor", "log", $"{evento.Evento} publicado (RK '{routingKey}', pet {evento.NomePet} #{evento.PetId}).");
    }

    public void Dispose()
    {
        if (_canal.IsOpen) _canal.Close();
        _canal.Dispose();
    }
}
