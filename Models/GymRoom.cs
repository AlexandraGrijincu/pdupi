namespace Gym.Models;

public class GymRoom
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }

    // Clasele care au loc în această sală
    public virtual ICollection<GymClass> Classes { get; set; } = new List<GymClass>();

   
}