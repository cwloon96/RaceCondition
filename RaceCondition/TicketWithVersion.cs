using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaceCondition
{
    public class TicketWithVersion
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public bool Available { get; set; }

        [Timestamp]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public byte[] Timestamp { get; set; }
    }
}