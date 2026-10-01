using PetShopMensageria.Eventos;
using PetShopMensageria.Infraestrutura;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PetShopMensageria.Consumidores;

public sealed class ConsumidorRecepcao : ConsumidorBase
{
    public ConsumidorRecepcao(IConnection conexao) : base(conexao) { }

    protected override string Fila => Topologia.FilaRecepcaoStatus;

    protected override string Nome => "Painel Recepção";

    protected override void Processar(BasicDeliverEventArgs ea)
    {
        var evento = SerializadorJson.Desserializar<EventoPet>(ea.Body);
        Registrar("recepcao", $"Status atualizado: {evento.Evento} | pet {evento.NomePet} (#{evento.PetId}) | RK '{ea.RoutingKey}'", evento);
    }
}
