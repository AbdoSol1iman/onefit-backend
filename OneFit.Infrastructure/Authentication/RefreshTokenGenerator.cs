using OneFit.Application.Common.Interfaces.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Infrastructure.Authentication
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        public string GenerateToken()
        {
            var randomNumber = new byte[32];
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
