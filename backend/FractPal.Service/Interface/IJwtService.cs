namespace FractPal.Service.Interface;

using FractPal.Model.Entities;

public interface IJwtService
{
    public string GenerateToken(FractPalUser user);
}
