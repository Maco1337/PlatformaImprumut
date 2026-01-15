using System;

namespace PlatformaImprumut
{
    public class LoanRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ItemId { get; set; }
        public string ItemName { get; set; }
        public string RequesterUsername { get; set; }
        public string OwnerUsername { get; set; }
        public string Status { get; set; } = "Pending"; 
    }
}
