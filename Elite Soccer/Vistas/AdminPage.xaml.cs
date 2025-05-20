using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Elite_Soccer.Modelo;
using System.Linq;

namespace Elite_Soccer.Vistas
{
    public partial class AdminPage : ContentPage
    {

        private const string FirebaseDatabaseUrl = "https://clubeliteapp-default-rtdb.firebaseio.com";

        // Modelo para tabla de posiciones
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

        private Dictionary<string, List<EquipoTabla>> _tablasPorCategoria = new Dictionary<string, List<EquipoTabla>>
        {
            ["Varonil"] = new List<EquipoTabla>(),
            ["Femenil"] = new List<EquipoTabla>()
        };
        private string _categoriaTablaActual = "Varonil";

        public AdminPage()
        {
            InitializeComponent();
            InicializarTablaPosiciones();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarTablaExistente();
            await CargarGoleadores();
            await CargarPartidos();

        }

        private void InicializarTablaPosiciones()
        {
            pickerEquipoTabla.ItemsSource = equiposVaronil;
        }

        private async Task CargarTablaExistente()
        {
            try
            {
                using (var cliente = new HttpClient())
                {
                    var urlVaronil = $"{FirebaseDatabaseUrl}/tabla_varonil.json?auth={MainPage.IdTokenUsuario}";
                    var urlFemenil = $"{FirebaseDatabaseUrl}/tabla_femenil.json?auth={MainPage.IdTokenUsuario}";

                    var respuestaVaronil = await cliente.GetAsync(urlVaronil);
                    if (respuestaVaronil.IsSuccessStatusCode)
                    {
                        var json = await respuestaVaronil.Content.ReadAsStringAsync();
                        _tablasPorCategoria["Varonil"] = JsonConvert.DeserializeObject<List<EquipoTabla>>(json) ?? new List<EquipoTabla>();
                    }

                    var respuestaFemenil = await cliente.GetAsync(urlFemenil);
                    if (respuestaFemenil.IsSuccessStatusCode)
                    {
                        var json = await respuestaFemenil.Content.ReadAsStringAsync();
                        _tablasPorCategoria["Femenil"] = JsonConvert.DeserializeObject<List<EquipoTabla>>(json) ?? new List<EquipoTabla>();
                    }

                    ActualizarVistaTabla();
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al cargar tablas: {ex.Message}", "OK");
            }
        }

        private void BtnTablaCategoria_Clicked(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            _categoriaTablaActual = btn.Text == "VARONIL" ? "Varonil" : "Femenil";

            btnTablaVaronil.BackgroundColor = _categoriaTablaActual == "Varonil" ? Color.FromHex("#252525") : Color.FromHex("#1E1E1E");
            btnTablaVaronil.TextColor = _categoriaTablaActual == "Varonil" ? Color.FromHex("Gold") : Color.White;

            btnTablaFemenil.BackgroundColor = _categoriaTablaActual == "Femenil" ? Color.FromHex("#252525") : Color.FromHex("#1E1E1E");
            btnTablaFemenil.TextColor = _categoriaTablaActual == "Femenil" ? Color.FromHex("Gold") : Color.White;

            pickerEquipoTabla.ItemsSource = _categoriaTablaActual == "Varonil" ? equiposVaronil : equiposFemenil;
            ActualizarVistaTabla();
        }

        private void ActualizarVistaTabla()
        {
            var tablaActual = _tablasPorCategoria[_categoriaTablaActual];
            var tablaOrdenada = tablaActual
                .OrderByDescending(x => x.PTS)
                .ThenByDescending(x => x.DIF)
                .ToList();

            for (int i = 0; i < tablaOrdenada.Count; i++)
            {
                tablaOrdenada[i].Posicion = i + 1;
            }

            collectionTabla.ItemsSource = tablaOrdenada;
        }

        private void CollectionTabla_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // El binding automático se encarga de mostrar el panel de edición
        }

        private async void BtnGuardarCambios_Clicked(object sender, EventArgs e)
        {
            if (collectionTabla.SelectedItem is EquipoTabla equipo)
            {
                if (!int.TryParse(entryEditarGF.Text, out int gf) || !int.TryParse(entryEditarGC.Text, out int gc))
                {
                    await DisplayAlert("Error", "Ingresa valores válidos", "OK");
                    return;
                }

                // Resetear los valores anteriores antes de recalcular
                equipo.JG = 0;
                equipo.JE = 0;
                equipo.JP = 0;
                equipo.JJ = 1; // Siempre se cuenta como 1 juego jugado
                equipo.GF = gf;
                equipo.GC = gc;

                if (gf > gc) equipo.JG = 1;
                else if (gf < gc) equipo.JP = 1;
                else equipo.JE = 1;

                // DIF y PTS se recalculan automáticamente desde las propiedades
                ActualizarVistaTabla();
                collectionTabla.SelectedItem = null;
            }
        }


        private async void BtnAgregarPartido_Clicked(object sender, EventArgs e)
        {
            if (pickerEquipoTabla.SelectedItem == null)
            {
                await DisplayAlert("Error", "Selecciona un equipo", "OK");
                return;
            }

            if (!int.TryParse(entryNuevoGF.Text, out int gf) || !int.TryParse(entryNuevoGC.Text, out int gc))
            {
                await DisplayAlert("Error", "Ingresa valores válidos para GF y GC", "OK");
                return;
            }

            var nombreEquipo = pickerEquipoTabla.SelectedItem.ToString();
            var tablaActual = _tablasPorCategoria[_categoriaTablaActual];

            var equipo = tablaActual.FirstOrDefault(x => x.Equipo == nombreEquipo);
            if (equipo == null)
            {
                equipo = new EquipoTabla { Equipo = nombreEquipo };
                tablaActual.Add(equipo);
            }

            equipo.JJ++;
            equipo.GF += gf;
            equipo.GC += gc;

            if (gf > gc) equipo.JG++;
            else if (gf < gc) equipo.JP++;
            else equipo.JE++;

            ActualizarVistaTabla();
            entryNuevoGF.Text = entryNuevoGC.Text = string.Empty;
        }

        private async void BtnGuardarTabla_Clicked(object sender, EventArgs e)
        {
            try
            {
                var ruta = $"tabla_{_categoriaTablaActual.ToLower()}";
                var url = $"{FirebaseDatabaseUrl}/{ruta}.json?auth={MainPage.IdTokenUsuario}";
                var json = JsonConvert.SerializeObject(_tablasPorCategoria[_categoriaTablaActual]);

                using (var cliente = new HttpClient())
                {
                    var respuesta = await cliente.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                    if (respuesta.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", "Tabla guardada correctamente", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo guardar la tabla", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
            }
        }


        private readonly List<string> equiposVaronil = new List<string>
        {
            "FÉNIX", "MACRO PLAZA", "VALENCIA", "REAL PRIMERA", "AJAX", "BRASIL",
            "TAZOS DORADOS", "PUEBLA", "TDR", "PURO CHACOTEO", "SAN PACHO", "UNAM",
            "DVO. TACHIRA", "SAN RAFA", "ALEMANIA", "CARACAS", "NEW PEOPLE"
        };

        private readonly List<string> equiposFemenil = new List<string>
        {
            "TEAM INFIERNO", "MAJESTIC", "MIAMI", "BRASIL", "CHELSEA",
            "BARCELONA", "FENIX", "PUMAS", "TBT"
        };

        private async Task CargarPartidos()
        {
            if (string.IsNullOrEmpty(MainPage.IdTokenUsuario))
            {
                await DisplayAlert("Error", "Sesión no válida. Por favor, inicia sesión nuevamente.", "OK");
                await Navigation.PopToRootAsync();
                return;
            }

            using (HttpClient cliente = new HttpClient())
            {
                string url = $"{FirebaseDatabaseUrl}/partidos.json?auth={MainPage.IdTokenUsuario}";
                HttpResponseMessage respuesta = await cliente.GetAsync(url);

                if (respuesta.IsSuccessStatusCode)
                {
                    string json = await respuesta.Content.ReadAsStringAsync();
                    var partidosDict = JsonConvert.DeserializeObject<Dictionary<string, Partido>>(json);

                    if (partidosDict != null)
                    {
                        var lista = new List<Partido>();

                        foreach (var item in partidosDict)
                        {
                            item.Value.IdFirebase = item.Key;
                            lista.Add(item.Value);
                        }


                        listaPartidos.ItemsSource = null;
                        listaPartidos.ItemsSource = lista.Count > 0 ? new List<Partido>(lista) : new List<Partido>();

                    }
                }
                else
                {
                    await DisplayAlert("Error", "No se pudieron cargar los partidos", "OK");
                }
            }
        }

        private void pickerCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            var categoriaSeleccionada = pickerCategoria.SelectedItem as string;

            if (categoriaSeleccionada == "Varonil")
            {
                pickerEquipoLocal.ItemsSource = equiposVaronil;
                pickerEquipoVisitante.ItemsSource = equiposVaronil;
            }
            else if (categoriaSeleccionada == "Femenil")
            {
                pickerEquipoLocal.ItemsSource = equiposFemenil;
                pickerEquipoVisitante.ItemsSource = equiposFemenil;
            }
        }

        private async void BtnRegistrarPartido_Clicked(object sender, EventArgs e)
        {
            string categoria = pickerCategoria.SelectedItem as string;
            string equipoLocal = pickerEquipoLocal.SelectedItem as string;
            string equipoVisitante = pickerEquipoVisitante.SelectedItem as string;
            DateTime fecha = fechaPartido.Date;
            TimeSpan hora = horaPartido.Time;

            if (string.IsNullOrWhiteSpace(categoria) ||
                string.IsNullOrWhiteSpace(equipoLocal) ||
                string.IsNullOrWhiteSpace(equipoVisitante))
            {
                await DisplayAlert("Error", "Todos los campos son obligatorios", "OK");
                return;
            }

            var partido = new Partido
            {
                equipoLocal = equipoLocal,
                equipoVisitante = equipoVisitante,
                categoria = categoria,
                fecha = fecha.ToString("yyyy-MM-dd"),
                hora = hora.ToString(@"hh\:mm")
            };

            string json = JsonConvert.SerializeObject(partido);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpClient cliente = new HttpClient())
            {
                string url = $"{FirebaseDatabaseUrl}/partidos.json?auth={MainPage.IdTokenUsuario}";
                HttpResponseMessage respuesta = await cliente.PostAsync(url, contenido);

                if (respuesta.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Partido registrado exitosamente", "OK");
                    LimpiarCampos();
                    await CargarPartidos();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo registrar el partido", "OK");
                }
            }
        }

        private async void ListaPartidos_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (e.SelectedItem is Partido partidoSeleccionado)
            {
                ((ListView)sender).SelectedItem = null;


                var paginaEditar = new EditarPartido(partidoSeleccionado.IdFirebase, partidoSeleccionado);
                await Navigation.PushAsync(paginaEditar);


                await CargarPartidos();
            }
        }


        private void LimpiarCampos()
        {
            pickerCategoria.SelectedIndex = -1;
            pickerEquipoLocal.ItemsSource = null;
            pickerEquipoVisitante.ItemsSource = null;
            fechaPartido.Date = DateTime.Today;
            horaPartido.Time = TimeSpan.Zero;
        }



        /*********************************GOLEADORES********************************/
        private const string RutaGoleadores = "https://clubeliteapp-default-rtdb.firebaseio.com/goleadores";

        private async void BtnGuardarGoleador_Clicked(object sender, EventArgs e)
        {
            string nombre = txtNombreJugador.Text;
            string categoria = pickerCategoriaG.SelectedItem as string;
            string equipo = pickerEquipoG.SelectedItem as string;

            bool golesConvertidos = int.TryParse(txtGoles.Text, out int goles);

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(categoria) ||
                string.IsNullOrWhiteSpace(equipo) || !golesConvertidos)
            {
                await DisplayAlert("Error", "Completa todos los campos correctamente", "OK");
                return;
            }

            var goleador = new Modelo.Goleador
            {
                nombre = nombre,
                categoria = categoria,
                equipo = equipo,
                goles = goles
            };

            string json = JsonConvert.SerializeObject(goleador);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpClient cliente = new HttpClient())
            {
                string url = $"{RutaGoleadores}.json?auth={MainPage.IdTokenUsuario}";
                HttpResponseMessage respuesta = await cliente.PostAsync(url, contenido);

                if (respuesta.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Goleador guardado", "OK");
                    LimpiarFormularioGoleador();
                    await CargarGoleadores();
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo guardar el goleador", "OK");
                }
            }
        }



        private async Task CargarGoleadores()
        {
            using (HttpClient cliente = new HttpClient())
            {
                string url = $"{RutaGoleadores}.json?auth={MainPage.IdTokenUsuario}";
                HttpResponseMessage respuesta = await cliente.GetAsync(url);

                if (respuesta.IsSuccessStatusCode)
                {
                    string json = await respuesta.Content.ReadAsStringAsync();
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, Modelo.Goleador>>(json);

                    if (dict != null)
                    {
                        var lista = new List<Modelo.Goleador>();

                        foreach (var item in dict)
                        {
                            item.Value.IdFirebase = item.Key;
                            lista.Add(item.Value);
                        }

                        // Ordenar por goles descendente
                        listaGoleadores.ItemsSource = lista
                            .OrderByDescending(g => g.goles)
                            .ToList();
                    }
                    else
                    {
                        listaGoleadores.ItemsSource = null;
                    }
                }
            }
        }

        private void LimpiarFormularioGoleador()
        {
            pickerCategoriaG.SelectedIndex = -1;
            txtNombreJugador.Text = string.Empty;
            pickerEquipoG.SelectedIndex = -1;
            txtGoles.Text = string.Empty;
        }

        private async void ListaGoleadores_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            if (e.Item is Goleador goleador)
            {
                ((ListView)sender).SelectedItem = null;
                await Navigation.PushAsync(new EditarGoleador(goleador));
            }
        }



        private void pickerCategoriaG_SelectedIndexChanged(object sender, EventArgs e)
        {
            string categoriaSeleccionada = pickerCategoriaG.SelectedItem as string;

            if (categoriaSeleccionada == "Varonil")
            {
                pickerEquipoG.ItemsSource = equiposVaronil;
            }
            else if (categoriaSeleccionada == "Femenil")
            {
                pickerEquipoG.ItemsSource = equiposFemenil;
            }
        }
        private async void Volver_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
}