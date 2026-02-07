namespace FractPal.Service.Interface;

using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

public interface IJwtService
{
    public Task<string> GenerateJwt(IdentityUser user);
}
