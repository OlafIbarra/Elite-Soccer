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
            public int GolesLocal { get; set; }
            public int GolesVisitante { get; set; }
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

                if (jornada.Partidos != null)
                {
                    foreach (var partido in jornada.Partidos)
                    {
                        var partidoGrid = new Grid
                        {
                            ColumnDefinitions = new ColumnDefinitionCollection
                    {
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength(50) },
                        new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                    },
                            Margin = new Thickness(0, 5),
                            VerticalOptions = LayoutOptions.Center
                        };

                        var lblLocal = new Label
                        {
                            Text = partido.Local,
                            FontSize = 14,
                            TextColor = Color.White,
                            HorizontalOptions = LayoutOptions.End,
                            VerticalOptions = LayoutOptions.Center
                        };

                        var lblVisitante = new Label
                        {
                            Text = partido.Visitante,
                            FontSize = 14,
                            TextColor = Color.White,
                            HorizontalOptions = LayoutOptions.Start,
                            VerticalOptions = LayoutOptions.Center
                        };

                        Label marcadorLabel;

                        if (partido.GolesLocal == 0 && partido.GolesVisitante == 0)
                        {
                            marcadorLabel = new Label
                            {
                                Text = $"{partido.Fecha} {partido.Hora}",
                                FontSize = 10,
                                TextColor = Color.Gray,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            };
                        }
                        else
                        {
                            marcadorLabel = new Label
                            {
                                Text = $"{partido.GolesLocal} - {partido.GolesVisitante}",
                                FontSize = 16,
                                TextColor = Color.Gold,
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.Center,
                                VerticalOptions = LayoutOptions.Center
                            };
                        }


                        partidoGrid.Children.Add(lblLocal, 0, 0);
                        partidoGrid.Children.Add(marcadorLabel, 1, 0);
                        partidoGrid.Children.Add(lblVisitante, 2, 0);

                        stack.Children.Add(partidoGrid);
                    }
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
