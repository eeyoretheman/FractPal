using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FractPal.Business.Services.Interfaces
{
    public interface IJwtService
    {
        public Task<string> GenerateJwt(IdentityUser user);
    }
}
