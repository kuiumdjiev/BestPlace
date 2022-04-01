﻿using System.ComponentModel.DataAnnotations;

namespace BestPlace.Core.Models;

public class QuestionnaireAddViewModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; }

}