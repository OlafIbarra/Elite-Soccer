using Elite_Soccer.Modelo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Xamarin.Forms;

namespace Elite_Soccer.Vistas
{
    public partial class GoleadoresPage : ContentPage
    {
        private const string FirebaseUrl = "https://clubeliteapp-default-rtdb.firebaseio.com/goleadores.json";

        public GoleadoresPage()
        {
            InitializeComponent();
            CargarGoleadores();
        }

        private async void CargarGoleadores()
        {
            using (HttpClient cliente = new HttpClient())
            {
                string url = FirebaseUrl + $"?auth={MainPage.IdTokenUsuario}";
                HttpResponseMessage respuesta = await cliente.GetAsync(url);

                if (respuesta.IsSuccessStatusCode)
                {
                    string json = await respuesta.Content.ReadAsStringAsync();
                    var dictGoleadores = JsonConvert.DeserializeObject<Dictionary<string, Goleador>>(json);

                    if (dictGoleadores != null)
                    {
                        contenedorFemenil.Children.Clear();
                        contenedorVaronil.Children.Clear();

                        foreach (var item in dictGoleadores.Values)
                        {
                            var tarjeta = new Frame
                            {
                                BackgroundColor = Color.FromHex("#1A1A1A"),
                                CornerRadius = 15,
                                Padding = 15,
                                HasShadow = false,
                                Content = new StackLayout
                                {
                                    Spacing = 5,
                                    Children =
                            {
                                new Label
                                {
                                    Text = item.nombre,
                                    FontAttributes = FontAttributes.Bold,
                                    FontSize = 18,
                                    TextColor = Color.White
                                },
                                new Label
                                {
                                    Text = $"{item.equipo} - {item.categoria}",
                                    FontSize = 14,
                                    TextColor = Color.Gray
                                },
                                new Label
                                {
                                    Text = $"Goles: {item.goles}",
                                    FontSize = 14,
                                    TextColor = Color.Gold
                                }
                            }
                                }
                            };

                            // Clasificar por categoría
                            if (item.categoria.ToLower() == "femenil")
                                contenedorFemenil.Children.Add(tarjeta);
                            else if (item.categoria.ToLower() == "varonil")
                                contenedorVaronil.Children.Add(tarjeta);
                        }
                    }
                    else
                    {
                        await DisplayAlert("Info", "No hay goleadores registrados aún", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se pudieron cargar los goleadores", "OK");
                }
            }
        }


        private async void Volver_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
