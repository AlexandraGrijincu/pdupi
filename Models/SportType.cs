using System;
using System.Collections.Generic;

namespace Gym.Models;

public class SportType
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // Navigation Property: Aceasta este "perechea" pentru proprietatea SportType din GymClass
    public virtual ICollection<GymClass> GymClasses { get; set; } = new List<GymClass>();

    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
}