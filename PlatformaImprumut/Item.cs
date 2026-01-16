using System;

namespace PlatformaImprumut
{
    public class Item
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
        public string Description { get; set; } 
        public string Category { get; set; }
        public string OwnerUsername { get; set; }
        public bool IsAvailable { get; set; } = true;

        public override string ToString()
        {
            return $"{Name} - {Description}  Stare: {(IsAvailable ? "Disponibil" : "Imprumutat")}";
        }
    }
}
