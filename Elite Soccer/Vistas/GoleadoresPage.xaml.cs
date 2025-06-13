using Elite_Soccer.Modelo;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Xamarin.Forms;

namespace Elite_Soccer.Vistas
{
    public partial class GoleadoresPage : ContentPage
    {
        private const string FirebaseUrl = "https://clubeliteapp-default-rtdb.firebaseio.com/goleadores.json";
        private string categoriaActual = "Varonil"; // Mostrar esta por defecto

        public GoleadoresPage()
        {
            InitializeComponent();

            // Cargar automáticamente la categoría varonil al iniciar
            Device.BeginInvokeOnMainThread(() =>
            {
                MostrarCategoria("Varonil");
            });
        }

        private async void MostrarCategoria(string categoria)
        {
            btnFemenil.BackgroundColor = categoria == "Femenil" ? Color.FromHex("#252525") : Color.FromHex("#1E1E1E");
            btnFemenil.TextColor = categoria == "Femenil" ? Color.Gold : Color.White;
            btnVaronil.BackgroundColor = categoria == "Varonil" ? Color.FromHex("#252525") : Color.FromHex("#1E1E1E");
            btnVaronil.TextColor = categoria == "Varonil" ? Color.Gold : Color.White;

            categoriaActual = categoria;
            contenedorGoleadores.Children.Clear();

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
                        var lista = dictGoleadores.Values
                            .Where(g => g.categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase))
                            .OrderByDescending(g => g.goles)
                            .ToList();

                        for (int i = 0; i < lista.Count; i++)
                        {
                            var goleador = lista[i];
                            string icono = goleador.categoria.Equals("Femenil", StringComparison.OrdinalIgnoreCase)
                                ? "futbolistafem.png"
                                : "futbolistamen.png";

                            // Medalla
                            string medalla = i == 0 ? "medalla_oro.png" :
                                             i == 1 ? "medalla_plata.png" :
                                             i == 2 ? "medalla_bronce.png" : null;

                            var grid = new Grid
                            {
                                ColumnDefinitions =
                        {
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = GridLength.Auto },
                            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
                        },
                                VerticalOptions = LayoutOptions.Center
                            };

                            // Agregar imagen de medalla si aplica
                            if (medalla != null)
                            {
                                grid.Children.Add(new Image
                                {
                                    Source = medalla,
                                    HeightRequest = 35,
                                    WidthRequest = 35,
                                    VerticalOptions = LayoutOptions.Center
                                }, 0, 0);
                            }

                            // Imagen del jugador
                            grid.Children.Add(new Image
                            {
                                Source = icono,
                                HeightRequest = 50,
                                WidthRequest = 50,
                                Margin = new Thickness(5, 0),
                                VerticalOptions = LayoutOptions.Center
                            }, 1, 0);

                            // Info del jugador
                            grid.Children.Add(new StackLayout
                            {
                                VerticalOptions = LayoutOptions.Center,
                                Spacing = 2,
                                Children =
                        {
                            new Label
                            {
                                Text = goleador.nombre,
                                FontSize = 18,
                                FontAttributes = FontAttributes.Bold,
                                TextColor = Color.White
                            },
                            new Label
                            {
                                Text = goleador.equipo,
                                FontSize = 14,
                                TextColor = Color.Gray
                            },
                            new Label
                            {
                                Text = $"Goles: {goleador.goles}",
                                FontSize = 14,
                                TextColor = Color.Gold
                            }
                        }
                            }, 2, 0);

                            contenedorGoleadores.Children.Add(new Frame
                            {
                                BackgroundColor = Color.FromHex("#1A1A1A"),
                                CornerRadius = 20,
                                Padding = new Thickness(15, 10),
                                Margin = new Thickness(0, 0, 0, 10),
                                HasShadow = true,
                                Content = grid
                            });
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

        private void BtnCategoria_Clicked(object sender, EventArgs e)
        {
            if (sender is Button btn)
            {
                MostrarCategoria(btn.Text);
            }
        }

        private async void Volver_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
