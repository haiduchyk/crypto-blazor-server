namespace CryptoBlazorApp.Hashing
{
    using System;
    using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
    using Microsoft.AspNetCore.DataProtection.KeyManagement;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Data;

    public class UserPhoneNumberStore : UserOnlyStore<IdentityUser>
    {
        private readonly IKeyManager keyManager;

        private IAuthenticatedEncryptor? Encryptor => keyManager.GetAllKeys().First().CreateEncryptor();

        public UserPhoneNumberStore(IKeyManager keyManager, ApplicationDbContext context,
            IdentityErrorDescriber describer = null) : base(
            context, describer)
        {
            this.keyManager = keyManager;
        }

        public override async Task<string> GetPhoneNumberAsync(IdentityUser user,
            CancellationToken cancellationToken = new())
        {
            if (string.IsNullOrEmpty(user.PhoneNumber))
            {
                return string.Empty;
            }

            var phoneBytes = Convert.FromBase64String(user.PhoneNumber);
            var phoneNumberBytes = Encryptor.Decrypt(phoneBytes, ArraySegment<byte>.Empty);
            return Encoding.UTF8.GetString(phoneNumberBytes);
        }

        public override async Task SetPhoneNumberAsync(IdentityUser user, string phoneNumber,
            CancellationToken cancellationToken = new())
        {
            if (string.IsNullOrEmpty(phoneNumber))
            {
                user.PhoneNumber = string.Empty;
                return;
            }

            var data = Encoding.UTF8.GetBytes(phoneNumber);
            var phoneNumberBytes = Encryptor.Encrypt(data, ArraySegment<byte>.Empty);
            user.PhoneNumber = Convert.ToBase64String(phoneNumberBytes);
        }
    }
}