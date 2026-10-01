using PetShopMensageria.Eventos;

namespace PetShopMensageria.Servicos;

public interface IServicoEmail
{
    void Enviar(EventoPet evento);
}
