using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Elite_Soccer.Modelo;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Elite_Soccer.Vistas
{
    public partial class TablaPosicionesPage : ContentPage
    {
        private const string FirebaseUrl = "https://clubeliteapp-default-rtdb.firebaseio.com";
        private List<EquipoTabla> tablaVaronil = new List<EquipoTabla>();
        private List<EquipoTabla> tablaFemenil = new List<EquipoTabla>();
        private string categoriaActual = "Varonil";

        public class EquipoTabla
        {
            public string Equipo { get; set; }
            public int JJ { get; set; }
            public int JG { get; set; }
            public int JE { get; set; }
            public int JP { get; set; }
            public int GF { get; set; }
            public int GC { get; set; }
            public int DIF => GF - GC;
            public int PTS => (JG * 3) + (JE * 1);
            public int Posicion { get; set; }
        }

        public TablaPosicionesPage()
        {
            InitializeComponent();
           // pickerCategoria.SelectedIndex = 0; // Mostrar Varonil por defecto
        }
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarTablas();
            MostrarTabla(tablaVaronil); // Muestra por defecto la varonil
        }

        private async Task CargarTablas()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string token = MainPage.IdTokenUsuario;
                    string urlVaronil = $"https://clubeliteapp-default-rtdb.firebaseio.com/tabla_varonil.json?auth={token}";
                    string urlFemenil = $"https://clubeliteapp-default-rtdb.firebaseio.com/tabla_femenil.json?auth={token}";

                    var jsonVaronil = await client.GetStringAsync(urlVaronil);
                    tablaVaronil = JsonConvert.DeserializeObject<List<EquipoTabla>>(jsonVaronil) ?? new List<EquipoTabla>();

                    var jsonFemenil = await client.GetStringAsync(urlFemenil);
                    tablaFemenil = JsonConvert.DeserializeObject<List<EquipoTabla>>(jsonFemenil) ?? new List<EquipoTabla>();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"No se pudieron cargar las tablas: {ex.Message}", "OK");
            }
        }


        

        private void BtnVaronil_Clicked(object sender, EventArgs e)
        {
            categoriaActual = "Varonil";
            ActualizarBotonesCategoria();
            MostrarTabla(tablaVaronil);
        }

        private void BtnFemenil_Clicked(object sender, EventArgs e)
        {
            categoriaActual = "Femenil";
            ActualizarBotonesCategoria();
            MostrarTabla(tablaFemenil);
        }

        private void ActualizarBotonesCategoria()
        {
            btnVaronil.BackgroundColor = categoriaActual == "Varonil" ? Color.FromHex("#252525") : Color.FromHex("#1E1E1E");
            btnVaronil.TextColor = categoriaActual == "Varonil" ? Color.Gold : Color.White;

            btnFemenil.BackgroundColor = categoriaActual == "Femenil" ? Color.FromHex("#252525") : Color.FromHex("#1E1E1E");
            btnFemenil.TextColor = categoriaActual == "Femenil" ? Color.Gold : Color.White;
        }


        /*    private async void PickerCategoria_SelectedIndexChanged(object sender, EventArgs e)
            {
                string categoria = pickerCategoria.SelectedItem.ToString();
                await CargarTablaPosiciones(categoria);
            }
        */
        private async Task CargarTablaPosiciones(string categoria)
        {
            try
            {
                string url = $"{FirebaseUrl}/tabla_{categoria.ToLower()}.json?auth={MainPage.IdTokenUsuario}";

                using var cliente = new HttpClient();
                var respuesta = await cliente.GetAsync(url);

                if (respuesta.IsSuccessStatusCode)
                {
                    string json = await respuesta.Content.ReadAsStringAsync();
                    var equipos = JsonConvert.DeserializeObject<List<EquipoTabla>>(json) ?? new List<EquipoTabla>();

                    equipos = equipos.OrderByDescending(x => x.PTS)
                                     .ThenByDescending(x => x.DIF)
                                     .ToList();

                    for (int i = 0; i < equipos.Count; i++)
                        equipos[i].Posicion = i + 1;

                    MostrarTabla(equipos);
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo cargar la tabla", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void MostrarTabla(List<EquipoTabla> lista)
        {
            contenedorPosiciones.Children.Clear();

            foreach (var equipo in lista)
            {
                var fila = new Grid
                {
                    ColumnDefinitions =
            {
                new ColumnDefinition { Width = 40 },
                new ColumnDefinition { Width = 120 },
                new ColumnDefinition { Width = 50 },
                new ColumnDefinition { Width = 50 },
                new ColumnDefinition { Width = 50 },
                new ColumnDefinition { Width = 50 },
                new ColumnDefinition { Width = 50 },
                new ColumnDefinition { Width = 50 },
                new ColumnDefinition { Width = 50 },
                new ColumnDefinition { Width = 60 }
            },
                    Padding = new Thickness(5),
                    BackgroundColor = Color.FromHex("#1E1E1E")
                };

                // Crear cada Label
                var lblPos = new Label { Text = equipo.Posicion.ToString(), TextColor = Color.White, FontSize = 11 };
                var lblEquipo = new Label { Text = equipo.Equipo, TextColor = Color.Gold, FontSize = 11, FontAttributes = FontAttributes.Bold };
                var lblJJ = new Label { Text = equipo.JJ.ToString(), TextColor = Color.White, FontSize = 11 };
                var lblJG = new Label { Text = equipo.JG.ToString(), TextColor = Color.White, FontSize = 11 };
                var lblJE = new Label { Text = equipo.JE.ToString(), TextColor = Color.White, FontSize = 11 };
                var lblJP = new Label { Text = equipo.JP.ToString(), TextColor = Color.White, FontSize = 11 };
                var lblGF = new Label { Text = equipo.GF.ToString(), TextColor = Color.White, FontSize = 11 };
                var lblGC = new Label { Text = equipo.GC.ToString(), TextColor = Color.White, FontSize = 11 };
                var lblDIF = new Label { Text = equipo.DIF.ToString(), TextColor = Color.White, FontSize = 11 };
                var lblPTS = new Label { Text = equipo.PTS.ToString(), TextColor = Color.Gold, FontSize = 11, FontAttributes = FontAttributes.Bold };

                // Asignar columna a cada Label
                Grid.SetColumn(lblPos, 0);
                Grid.SetColumn(lblEquipo, 1);
                Grid.SetColumn(lblJJ, 2);
                Grid.SetColumn(lblJG, 3);
                Grid.SetColumn(lblJE, 4);
                Grid.SetColumn(lblJP, 5);
                Grid.SetColumn(lblGF, 6);
                Grid.SetColumn(lblGC, 7);
                Grid.SetColumn(lblDIF, 8);
                Grid.SetColumn(lblPTS, 9);

                // Agregar al grid
                fila.Children.Add(lblPos);
                fila.Children.Add(lblEquipo);
                fila.Children.Add(lblJJ);
                fila.Children.Add(lblJG);
                fila.Children.Add(lblJE);
                fila.Children.Add(lblJP);
                fila.Children.Add(lblGF);
                fila.Children.Add(lblGC);
                fila.Children.Add(lblDIF);
                fila.Children.Add(lblPTS);

                contenedorPosiciones.Children.Add(fila);
            }
        }



        private async void VerGoleadores_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new GoleadoresPage());
        }

        
        private async void VerCalendario_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new CalendarioPage());
        }
        private async void Volver_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}
