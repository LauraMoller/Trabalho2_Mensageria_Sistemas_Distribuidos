namespace PetShopMensageria.Eventos;

public class ServicoIniciado : EventoPet
{
    public ServicoIniciado() => Evento = nameof(ServicoIniciado);
}
