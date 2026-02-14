namespace FractPal.Service.Interface;

using FractPal.Model.Entities;

public interface IJwtService
{
    public string GenerateToken(User user);
}
