using PetShopMensageria.Eventos;
using PetShopMensageria.Infraestrutura;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PetShopMensageria.Consumidores;

public abstract class ConsumidorBase : IDisposable
{
    protected IModel Canal { get; }
    protected abstract string Fila { get; }
    protected abstract string Nome { get; }

    protected ConsumidorBase(IConnection conexao, ushort prefetch = 1)
    {
        Canal = conexao.CreateModel();
        Canal.BasicQos(prefetchSize: 0, prefetchCount: prefetch, global: false);
    }

    public void Iniciar()
    {
        var consumidor = new EventingBasicConsumer(Canal);
        consumidor.Received += (_, ea) =>
        {
            try
            {
                Processar(ea);
                Canal.BasicAck(ea.DeliveryTag, multiple: false);
            }
            catch (Exception ex)
            {
                try { AoFalhar(ea, ex); } 
                catch (Exception erroFatal) { Log($"Erro ao tratar falha: {erroFatal.Message}"); }
            }
        };

        Canal.BasicConsume(queue: Fila, autoAck: false, consumer: consumidor);
        Log($"Consumindo a fila '{Fila}'.");
    }

    protected abstract void Processar(BasicDeliverEventArgs ea);

    protected virtual void AoFalhar(BasicDeliverEventArgs ea, Exception ex)
    {
        Log($"Falha: {ex.Message}. Rejeitando mensagem (NACK, sem requeue).");
        Canal.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
    }

    protected void Log(string mensagem, string tipo = "log") => Registrar(tipo, mensagem);

    protected void Registrar(string tipo, string mensagem, EventoPet? evento = null) =>
        RegistroEventos.Registrar(Nome, tipo, mensagem, evento);

    public void Dispose()
    {
        if (Canal.IsOpen) Canal.Close();
        Canal.Dispose();
    }
}
