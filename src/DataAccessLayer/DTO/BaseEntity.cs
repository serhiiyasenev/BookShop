using System;
using System.ComponentModel.DataAnnotations;

namespace DataAccessLayer.DTO
{
    public abstract class BaseEntity
    {
        [Key]
        public Guid Id { get; set; }
    }
}
