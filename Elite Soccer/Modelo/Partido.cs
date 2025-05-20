using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Elite_Soccer.Modelo
{

    public class Partido
    {

        public string equipoLocal { get; set; }
        public string equipoVisitante { get; set; }
        public string categoria { get; set; }
        public string fecha { get; set; }      // Formato: yyyy-MM-dd
        public string hora { get; set; }       // Formato: HH:mm
        [JsonIgnore]
        public string IdFirebase { get; set; }

        public string descripcion =>
            $"{categoria}: {equipoLocal} vs {equipoVisitante} - {fecha} {hora}";
    }
}
