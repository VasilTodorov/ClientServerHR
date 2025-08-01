namespace ClientServerHR.Services
{
    public interface IBankDataProtectorService
    {
        string? EncryptIban(string? plainIban);
        string? DecryptIban(string? encryptedIban);
    }
}
