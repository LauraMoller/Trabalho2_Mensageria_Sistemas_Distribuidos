using PetShopMensageria.Eventos;
using PetShopMensageria.Infraestrutura;
using PetShopMensageria.Publicadores;

namespace PetShopMensageria.Modulos;

public sealed class ModuloRecepcao
{
    private readonly PublicadorEventos _publicador;

    public ModuloRecepcao(PublicadorEventos publicador) => _publicador = publicador;

    public void RealizarCheckIn(string petId, string nomePet, string emailTutor) =>
        _publicador.Publicar(Topologia.RkCheckin,
            new CheckInRealizado { PetId = petId, NomePet = nomePet, EmailTutor = emailTutor });

    public void RealizarCheckOut(string petId, string nomePet, string emailTutor) =>
        _publicador.Publicar(Topologia.RkCheckout,
            new CheckOutRealizado { PetId = petId, NomePet = nomePet, EmailTutor = emailTutor });
}
