namespace LEAPBot.Domain.Contracts
{
    public interface ISettingsReader
    {
        string this[string index] { get; }
    }
}
