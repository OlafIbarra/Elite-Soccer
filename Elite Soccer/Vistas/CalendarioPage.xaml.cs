using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Elite_Soccer.Modelo;
using Newtonsoft.Json;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Elite_Soccer.Vistas
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CalendarioPage : ContentPage
    {
        private const string FirebaseUrl = "https://clubeliteapp-default-rtdb.firebaseio.com";
        private string categoriaActual = "Varonil";

        public class PartidoJornada
        {
            public string Local { get; set; }
            public string Visitante { get; set; }
            public int GF_Local { get; set; }
            public int GF_Visitante { get; set; }
            public string Fecha { get; set; }
            public string Hora { get; set; }
        }

        public class Jornada
        {
            public int Numero { get; set; }
            public List<PartidoJornada> Partidos { get; set; }
        }

        public CalendarioPage()
        {
            InitializeComponent();
            CargarJornadas();
        }

        private async void CargarJornadas()
        {
            try
            {
                string ruta = $"jornadas_{categoriaActual.ToLower()}";
                string url = $"{FirebaseUrl}/{ruta}.json?auth={MainPage.IdTokenUsuario}";

                using (var cliente = new HttpClient())
                {
                    var respuesta = await cliente.GetAsync(url);
                    if (respuesta.IsSuccessStatusCode)
                    {
                        var json = await respuesta.Content.ReadAsStringAsync();
                        var jornadas = JsonConvert.DeserializeObject<List<Jornada>>(json);

                        MostrarJornadas(jornadas ?? new List<Jornada>());
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudieron obtener las jornadas", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void MostrarJornadas(List<Jornada> jornadas)
        {
            contenedorJornadas.Children.Clear();

            foreach (var jornada in jornadas)
            {
                var jornadaCard = new Frame
                {
                    BackgroundColor = Color.FromHex("#1E1E1E"),
                    CornerRadius = 20,
                    Padding = 10,
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var stack = new StackLayout();

                stack.Children.Add(new Label
                {
                    Text = $"JORNADA {jornada.Numero}",
                    FontSize = 18,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.Gold,
                    HorizontalOptions = LayoutOptions.Center
                });

                foreach (var partido in jornada.Partidos)
                {
                    string marcador = (partido.GF_Local + partido.GF_Visitante) == 0 ?
                        $"{partido.Fecha} {partido.Hora}" :
                        $"{partido.GF_Local} - {partido.GF_Visitante}";

                    stack.Children.Add(new Label
                    {
                        Text = $"{partido.Local} vs {partido.Visitante}    {marcador}",
                        FontSize = 14,
                        TextColor = Color.White
                    });
                }

                jornadaCard.Content = stack;
                contenedorJornadas.Children.Add(jornadaCard);
            }
        }

        private void BtnCategoria_Clicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            categoriaActual = btn.Text;

            // Estilo activo
            btnVaronil.BackgroundColor = categoriaActual == "VARONIL" ? Color.FromHex("#252525") : Color.FromHex("#1E1E1E");
            btnVaronil.TextColor = categoriaActual == "VARONIL" ? Color.Gold : Color.White;

            btnFemenil.BackgroundColor = categoriaActual == "FEMENIL" ? Color.FromHex("#252525") : Color.FromHex("#1E1E1E");
            btnFemenil.TextColor = categoriaActual == "FEMENIL" ? Color.Gold : Color.White;

            categoriaActual = categoriaActual == "VARONIL" ? "Varonil" : "Femenil";
            CargarJornadas();
        }

        private async void Volver_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
