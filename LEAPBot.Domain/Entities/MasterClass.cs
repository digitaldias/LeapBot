using System;

namespace LEAPBot.Domain.Entities
{    
    [Serializable]
    public class MasterClass
    {
        public Guid Id { get; set; }

        public int Number { get; set; }

        public int Year { get; set; }

        public DateTime Date { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public bool DateConfirmed { get; set; }
    }
}
