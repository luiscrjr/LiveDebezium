namespace Infra.Shared
{
    public interface IMensagemRoteavel
    {
        string EntityType { get; set; }

        string IdEntidade { get; set; }

        string Action { get; set; }

        string Data { get; set; }
    }
}