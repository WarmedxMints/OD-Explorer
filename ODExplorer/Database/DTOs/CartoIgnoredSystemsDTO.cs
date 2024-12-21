using ODUtils.Database.DTOs;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ODExplorer.Database.DTOs
{
    [Index(nameof(Address))]
    public sealed class CartoIgnoredSystemsDTO
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long Address { get; set; }
        public required string Name { get; set; }
        public List<JournalCommanderDTO> Commanders { get; set; } = [];
    }
}
