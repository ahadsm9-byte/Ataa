namespace Ataa.Models
{
    public class ProfileViewModel
    {
        public Volunteers UserProfile { get; set; }

        public List<Skills> AllSkills { get; set; }
        public List<Interests> AllInterests { get; set; }

        public List<int> SelectedSkillIds { get; set; }
        public List<int> SelectedInterestIds { get; set; }

        public List<EventRegistrations> VolunteerHistory { get; set; }
    }
}
