namespace Elite_Soccer.Modelo
{
    public class Goleador
    {
        public string IdFirebase { get; set; } 
        public string nombre { get; set; }
        public string categoria { get; set; }
        public string equipo { get; set; }
        public int goles { get; set; }

        public string descripcion => $"{nombre} ({equipo}) - {goles} gol(es)";
    }
}
