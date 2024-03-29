﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BestPlace.Infrastructure.Data;

public class ItemImages
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ImageId { get; set; }

    [ForeignKey(nameof(ImageId))]
    public Image Image { get; set; }

  
    public Guid? ItemId { get; set; }

    [ForeignKey(nameof(ItemId))]
    public Item? Item { get; set; }
}