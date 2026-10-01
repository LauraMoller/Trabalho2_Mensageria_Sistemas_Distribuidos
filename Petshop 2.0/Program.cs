using System.Text.Json;
using System.Threading.Channels;
using PetShopMensageria.Api;
using PetShopMensageria.Configuracao;
using PetShopMensageria.Consumidores;
using PetShopMensageria.Infraestrutura;
using PetShopMensageria.Modulos;
using PetShopMensageria.Publicadores;
using PetShopMensageria.Servicos;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5080");



var configuracaoEmail = ConfiguracaoEmail.DeVariaveisDeAmbiente();
var problemaEmail = configuracaoEmail.Validar();
if (problemaEmail is not null)
{
    Console.WriteLine($"[ERRO] {problemaEmail} Configure SMTP_HOST, SMTP_USER, SMTP_PASS e SMTP_FROM.");
    return;
}

var configuracao = ConfiguracaoRabbitMq.DeVariaveisDeAmbiente();
var conexao = configuracao.CriarFabrica().CreateConnection("petshop-web");
using (var canalTopologia = conexao.CreateModel())
    ConfiguradorTopologia.Configurar(canalTopologia);


var publicador = new PublicadorEventos(conexao);
IServicoEmail email = new ServicoEmailSmtp(configuracaoEmail);
var esperaRetry = TimeSpan.FromSeconds(
    int.TryParse(Environment.GetEnvironmentVariable("RETRY_ESPERA_SEGUNDOS"), out var seg) ? seg : 2);
Console.WriteLine($"[Infra] E-mail real via {configuracaoEmail.Host}:{configuracaoEmail.Porta} (remetente {configuracaoEmail.Remetente}).");
var consumidores = new ConsumidorBase[]
{
    new ConsumidorOperacional(conexao),
    new ConsumidorRecepcao(conexao),
    new ConsumidorNotificacao(conexao, email, publicador, espera: esperaRetry)
};
foreach (var consumidor in consumidores) consumidor.Iniciar();

var recepcao = new ModuloRecepcao(publicador);
var operacao = new ModuloOperacao(publicador);


var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();


app.MapPost("/api/checkin", (PetDto p) =>
{
    if (!ValidadorEmail.EhValido(p.EmailTutor))
        return Results.BadRequest("Informe um e-mail válido para o tutor.");

    recepcao.RealizarCheckIn(p.PetId, p.NomePet, p.EmailTutor);
    return Results.Ok();
});
app.MapPost("/api/checkout",  (PetDto p) => { recepcao.RealizarCheckOut(p.PetId, p.NomePet, p.EmailTutor);  return Results.Ok(); });
app.MapPost("/api/iniciar",   (PetDto p) => { operacao.IniciarServico(p.PetId, p.NomePet, p.EmailTutor);    return Results.Ok(); });
app.MapPost("/api/finalizar", (PetDto p) => { operacao.FinalizarServico(p.PetId, p.NomePet, p.EmailTutor);  return Results.Ok(); });

var jsonWeb = new JsonSerializerOptions(JsonSerializerDefaults.Web);
app.MapGet("/api/eventos", async (HttpContext ctx, CancellationToken ct) =>
{
    ctx.Response.ContentType = "text/event-stream";
    ctx.Response.Headers.CacheControl = "no-cache";

    var fila = Channel.CreateUnbounded<EntradaLog>();
    Action<EntradaLog> tratador = e => fila.Writer.TryWrite(e);
    RegistroEventos.Assinar(tratador);
    try
    {
        await foreach (var entrada in fila.Reader.ReadAllAsync(ct))
        {
            await ctx.Response.WriteAsync($"data: {JsonSerializer.Serialize(entrada, jsonWeb)}\n\n", ct);
            await ctx.Response.Body.FlushAsync(ct);
        }
    }
    catch (OperationCanceledException) { }
    finally { RegistroEventos.CancelarAssinatura(tratador); }
});

app.Lifetime.ApplicationStopping.Register(() =>
{
    foreach (var consumidor in consumidores) consumidor.Dispose();
    publicador.Dispose();
    conexao.Dispose();
});

Console.WriteLine("Pet Shop no ar: http://localhost:5080  (Recepção)  |  http://localhost:5080/operacao.html  (Operação)");
app.Run();