using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using server.Models;

namespace server.Interface
{
    public interface ILoginService
    {
        Task InsertLoginAsync(Login login);
        Task InsertLoginTableData();
        Task<string> ValidateLogin(string email, string password);
    }
}