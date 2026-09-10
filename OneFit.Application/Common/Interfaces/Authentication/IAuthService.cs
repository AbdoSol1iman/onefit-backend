using OneFit.Application.Features.Authentication.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces.Authentication
{
    public interface IAuthService
    {
        /// <summary>
        /// Login user with email and password
        /// </summary>
        Task<LoginResponseDTO> LoginAsync(LoginRequestDTO request);

        
    }
}
