using System;

namespace Evento.Infrastructure.DTO
{
    public class TicketDto
    {
         public Guid EventId { get;  set; }
        public int Seating { get;  set; }
        public decimal Price { get;  set; }
        public Guid? UserId { get;  set; }
        public string UserName { get;  set; }
        public DateTime? PurchasedAt {get;  set;}
        public bool Purchased => UserId.HasValue;
    }
}