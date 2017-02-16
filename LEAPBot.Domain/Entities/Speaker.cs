using System;

namespace LEAPBot.Domain.Entities
{
    [Serializable]
    public class Speaker
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string WorkTitle { get; set; }

        public string Email { get; set; }

        public string IntroHeading { get; set; }

        public string Bio { get; set; }

        public string Image { get; set; }
    }
}
