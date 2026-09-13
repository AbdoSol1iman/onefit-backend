using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Authentication.DTOs
{
    public class LoginRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
