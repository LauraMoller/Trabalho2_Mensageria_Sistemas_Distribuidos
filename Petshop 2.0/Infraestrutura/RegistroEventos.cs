using PetShopMensageria.Eventos;

namespace PetShopMensageria.Infraestrutura;

public record EntradaLog(long Id, string Hora, string Origem, string Tipo, string Mensagem,
                         string? Evento, string? PetId, string? NomePet, string? EmailTutor);

public static class RegistroEventos
{
    private static readonly List<EntradaLog> Historico = new();
    private static readonly object Trava = new();
    private static event Action<EntradaLog>? Nova;
    private static long _proximoId;

    public static void Registrar(string origem, string tipo, string mensagem, EventoPet? evento = null)
    {
        lock (Trava)
        {
            var entrada = new EntradaLog(++_proximoId, DateTime.Now.ToString("HH:mm:ss"), origem, tipo, mensagem,
                                         evento?.Evento, evento?.PetId, evento?.NomePet, evento?.EmailTutor);
            Historico.Add(entrada);
            if (Historico.Count > 300) Historico.RemoveAt(0);
            Console.WriteLine($"[{origem}] {mensagem}");
            Nova?.Invoke(entrada);
        }
    }

    public static void Assinar(Action<EntradaLog> tratador)
    {
        lock (Trava)
        {
            foreach (var entrada in Historico) tratador(entrada);
            Nova += tratador;
        }
    }

    public static void CancelarAssinatura(Action<EntradaLog> tratador)
    {
        lock (Trava) Nova -= tratador;
    }
}
