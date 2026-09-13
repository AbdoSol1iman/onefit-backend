using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Common.Interfaces
{
    public interface IIdentityService
    {
        Task<(bool Succeeded, string[] Errors, string UserId)> CreateUserAsync(
            string firstName,
            string lastName,
            string email,
            string password);

        Task<(bool Succeeded, string[] Errors, string UserId)> CreateBrandAsync(
            string firstName,
            string lastName,
            string email,
            string password);
    }
}
