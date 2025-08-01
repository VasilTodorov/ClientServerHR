using Microsoft.AspNetCore.DataProtection;

namespace ClientServerHR.Services
{
    public class BankDataProtectorService : IBankDataProtectorService
    {
        private readonly IDataProtector _protector;

        public BankDataProtectorService(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("Employee.IBAN");
        }

        public string? EncryptIban(string? plainIban)
        {
            return string.IsNullOrWhiteSpace(plainIban) ? string.Empty : _protector.Protect(plainIban);
        }

        public string? DecryptIban(string? encryptedIban)
        {
            if (string.IsNullOrWhiteSpace(encryptedIban))
                return string.Empty;

            try
            {
                return _protector.Unprotect(encryptedIban);
            }
            catch
            {                
                return string.Empty;
            }
        }
    }
}
