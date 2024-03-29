﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace BestPlace.Infrastructure.Data.Identity;

public class ApplicationUser : IdentityUser
{

    [StringLength(50)]
    public string FirstName { get; set; }


    [StringLength(50)]
    public string LastName { get; set; }


    public Guid? ImageId { get; set; }

    [ForeignKey(nameof(ImageId))]
    public Image? Image { get; set; }


    [Required]
    [MaxLength(250)]
    public string Address { get; set; }

    [Required]
    [RegularExpression("[+]{1}359 [0-9]{3} [0-9]{4}")]
    public string Phone { get; set; }


    public string Facebook { get; set; }

    public string Instagram { get; set; }

    public ICollection<Item> MyItems { get; set; } = new List<Item>();
}