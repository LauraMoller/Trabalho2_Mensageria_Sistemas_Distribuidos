using PetShopMensageria.Eventos;
using PetShopMensageria.Infraestrutura;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PetShopMensageria.Consumidores;

public sealed class ConsumidorOperacional : ConsumidorBase
{
    public ConsumidorOperacional(IConnection conexao) : base(conexao) { }

    protected override string Fila => Topologia.FilaOperacionalCheckin;

    protected override string Nome => "Painel Operacional";

    protected override void Processar(BasicDeliverEventArgs ea)
    {
        var evento = SerializadorJson.Desserializar<EventoPet>(ea.Body);
        Registrar("operacional", $"Pet {evento.NomePet} (#{evento.PetId}) adicionado à lista de banho/tosa.", evento);
    }
}
