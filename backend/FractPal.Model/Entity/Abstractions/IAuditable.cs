namespace FractPal.Model.Entity.Abstractions;

using System;

public interface IAuditable
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
