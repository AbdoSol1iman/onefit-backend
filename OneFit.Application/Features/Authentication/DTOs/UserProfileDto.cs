using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Authentication.DTOs
{
    public class UserProfileDto
    {
        public string Id { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }
}
