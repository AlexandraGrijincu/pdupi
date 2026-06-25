namespace Gym.Models;

public class GymClass
{
    public int Id { get; set; }
    public int TrainerId { get; set; }
    public int RoomId { get; set; }
    public int SportTypeId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int MaxParticipants { get; set; }

    // Proprietăți de navigare pentru expand
    public virtual User? Trainer { get; set; }
    public virtual GymRoom? Room { get; set; }
    public virtual SportType? SportType { get; set; }
    
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public int AvailableSlots => MaxParticipants - Bookings.Count;

    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
}