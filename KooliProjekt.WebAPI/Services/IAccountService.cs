using System.Threading;
using System.Threading.Tasks;
using KooliProjekt.WebAPI.Contracts;

namespace KooliProjekt.WebAPI.Services
{
    public interface IAccountService
    {
        Task<AccountResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
        Task<AccountResponse> ValidateLoginAsync(LoginRequest request, CancellationToken cancellationToken);
    }
}
