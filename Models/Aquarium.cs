using System;
using System.Collections.Generic;

namespace AquariumMonitor.Models
{
    public class Aquarium
    { 
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset Created { get; set; }
        public string Notes { get; set; }
        public User User { get; set; }
        public AquariumType Type { get; set; }
        public decimal? OfficalVolume { get; set; }
        public decimal? ActualVolume { get; set; }
        public Unit VolumeUnit { get; set; }
        public decimal? Width { get; set; }
        public decimal? Height { get; set; }
        public decimal? Length { get; set; }
        public Unit DimensionUnit { get; set; }
        public List<Measurement> Measurements { get; set; }
        public byte[] RowVersion { get; set; }
    }
}
