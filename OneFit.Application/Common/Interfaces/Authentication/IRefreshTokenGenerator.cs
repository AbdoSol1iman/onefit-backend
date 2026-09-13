using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces.Authentication
{
    public interface IRefreshTokenGenerator
    {
        string GenerateToken();
    }
}
