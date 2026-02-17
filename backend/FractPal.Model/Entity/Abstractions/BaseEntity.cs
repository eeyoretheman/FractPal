namespace FractPal.Model.Entities.Abstractions;

using System.ComponentModel.DataAnnotations;

public abstract class BaseEntity
{
    [Key]
    public Guid Id { get; set; }
}
