namespace Gym.Models;

public class Booking
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public int GymClassId { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.Now;
    public bool Status { get; set; } = true;

    
    public virtual User? Member { get; set; }
    public virtual GymClass? GymClass { get; set; }

    public int? CompanyId { get; set; }
    public Company? Company { get; set; }

}