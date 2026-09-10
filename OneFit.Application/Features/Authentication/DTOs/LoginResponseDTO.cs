using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Authentication.DTOs
{
    public class LoginResponseDTO
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public UserProfileDto User { get; set; }
    }
}
