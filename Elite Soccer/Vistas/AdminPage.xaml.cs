using Firebase.Database;
using Firebase.Database.Query;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms;
using Elite_Soccer.Modelo;
using System.Linq;
using System.Globalization;
using static Elite_Soccer.Vistas.CalendarioPage;
using Xamarin.Essentials;

namespace Elite_Soccer.Vistas
{

    public partial class AdminPage : ContentPage
    {

        private const string FirebaseDatabaseUrl = "https://clubeliteapp-default-rtdb.firebaseio.com";
        private FirebaseClient firebase;
        public AdminPage()
        {
            InitializeComponent();
            CargarPickerJornadas();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CargarTablaExistente();
            await CargarGoleadores();
            firebase = new FirebaseClient(
                FirebaseDatabaseUrl,
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(MainPage.IdTokenUsuario)
                });
        }




        /************TABLA DE POSICIONES *********/



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
        /************TABLA DE POSICIONES FIN*********/


   
        
        private List<Partido> partidosTemp = new List<Partido>();

        public class Partido
        {
            public string Local { get; set; }
            public string Visitante { get; set; }
            public string Fecha { get; set; }
            public string Hora { get; set; }
            public int GolesLocal { get; set; }
            public int GolesVisitante { get; set; }
            public string Categoria { get; set; }

            public string DescripcionVisible => $"{Local} vs {Visitante} ({Fecha})"; // Usado en el Picker

            public override string ToString() => DescripcionVisible;

        }


        public class Jornada
        {
            public int Numero { get; set; }
            public string Categoria { get; set; }
            public List<Partido> Partidos { get; set; } = new List<Partido>();
        }



        private void CargarPickerJornadas()
        {
            for (int i = 1; i <= 20; i++)
            {
                pickerNumeroJornada.Items.Add(i.ToString());
            }

            pickerCategoria.SelectedIndexChanged += pickerCategoria_SelectedIndexChanged;
        }

        private void pickerCategoria_SelectedIndexChanged(object sender, EventArgs e)
        {
            string categoriaSeleccionada = pickerCategoria.SelectedItem as string;

            if (categoriaSeleccionada == "Varonil")
            {
                pickerLocal.ItemsSource = equiposVaronil;
                pickerVisitante.ItemsSource = equiposVaronil;
            }
            else if (categoriaSeleccionada == "Femenil")
            {
                pickerLocal.ItemsSource = equiposFemenil;
                pickerVisitante.ItemsSource = equiposFemenil;
            }
        }

        private void AgregarPartido_Clicked(object sender, EventArgs e)
        {
            if (pickerLocal.SelectedIndex == -1 || pickerVisitante.SelectedIndex == -1)
            {
                DisplayAlert("Error", "Seleccione equipos local y visitante.", "OK");
                return;
            }

            var partido = new Partido
            {
                Categoria = pickerCategoria.SelectedItem?.ToString() ?? "Varonil",
                Local = pickerLocal.SelectedItem.ToString(),
                Visitante = pickerVisitante.SelectedItem.ToString(),
                Fecha = datePickerPartido.Date.ToString("yyyy-MM-dd"),
                Hora = timePickerPartido.Time.ToString(@"hh\:mm")
            };

            partidosTemp.Add(partido);
            MostrarPartidosAgregados();

            // Limpiar pickers
            pickerLocal.SelectedIndex = -1;
            pickerVisitante.SelectedIndex = -1;
            datePickerPartido.Date = DateTime.Today;
            timePickerPartido.Time = new TimeSpan(0, 0, 0);
        }

        private void MostrarPartidosAgregados()
        {
            stackPartidosAgregados.Children.Clear();
            foreach (var partido in partidosTemp)
            {
                stackPartidosAgregados.Children.Add(new Label
                {
                    Text = $"{partido.Local} vs {partido.Visitante} - {partido.Fecha} {partido.Hora}",
                    TextColor = Color.White,
                    FontSize = 14
                });
            }
        }

        private async void GuardarJornada_Clicked(object sender, EventArgs e)
        {
            if (pickerNumeroJornada.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Seleccione un número de jornada.", "OK");
                return;
            }

            if (partidosTemp.Count == 0)
            {
                await DisplayAlert("Error", "Agregue al menos un partido.", "OK");
                return;
            }

            int numeroJornada = int.Parse(pickerNumeroJornada.SelectedItem.ToString());

            Jornada jornada = new Jornada
            {
                Numero = numeroJornada,
                Categoria = pickerCategoria.SelectedItem?.ToString() ?? "Varonil",
                Partidos = partidosTemp ?? new List<Partido>()
            };


            try
            {
                string categoriaSeleccionada = pickerCategoria.SelectedItem?.ToString() ?? "varonil";
                string ruta = categoriaSeleccionada.ToLower() == "femenil" ? "jornadas_femenil" : "jornadas_varonil";
                string url = $"{FirebaseDatabaseUrl}/{ruta}/{numeroJornada - 1}.json?auth={MainPage.IdTokenUsuario}";

                var json = JsonConvert.SerializeObject(jornada);
                using (var cliente = new HttpClient())
                {
                    var respuesta = await cliente.PutAsync(url, new StringContent(json));
                    if (respuesta.IsSuccessStatusCode)
                    {
                        await DisplayAlert("Éxito", "Jornada guardada correctamente.", "OK");
                        pickerNumeroJornada.SelectedIndex = -1;
                        pickerLocal.SelectedIndex = -1;
                        pickerVisitante.SelectedIndex = -1;
                        pickerCategoria.SelectedIndex = -1;
                        partidosTemp.Clear();
                        MostrarPartidosAgregados();
                    }
                    else
                    {
                        await DisplayAlert("Error", "No se pudo guardar la jornada.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void pickerCategoriaEditar_SelectedIndexChanged(object sender, EventArgs e)
        {
            pickerJornadaEditar.Items.Clear();
            stackPartidosEditar.Children.Clear();

            string categoria = pickerCategoriaEditar.SelectedItem as string;
            if (string.IsNullOrEmpty(categoria))
                return;

            string ruta = categoria.ToLower() == "femenil" ? "jornadas_femenil" : "jornadas_varonil";
            string url = $"{FirebaseDatabaseUrl}/{ruta}.json?auth={MainPage.IdTokenUsuario}";

            using var cliente = new HttpClient();
            var respuesta = await cliente.GetAsync(url);
            if (!respuesta.IsSuccessStatusCode) return;

            string json = await respuesta.Content.ReadAsStringAsync();

            List<Jornada> jornadas = new List<Jornada>();

            try
            {
                // Intenta primero como diccionario
                var dict = JsonConvert.DeserializeObject<Dictionary<string, Jornada>>(json);
                if (dict != null)
                {
                    jornadas = dict.OrderBy(j => int.Parse(j.Key)).Select(j => j.Value).ToList();
                }
            }
            catch
            {
                try
                {
                    // Si falla, intenta como array
                    jornadas = JsonConvert.DeserializeObject<List<Jornada>>(json);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo leer las jornadas: {ex.Message}", "OK");
                    return;
                }
            }

            for (int i = 0; i < jornadas.Count; i++)
            {
                pickerJornadaEditar.Items.Add((i + 1).ToString());
            }
        }

        private Jornada jornadaActual;
        private int partidoActualIndex = 0;
        private async void pickerJornadaEditar_SelectedIndexChanged(object sender, EventArgs e)
        {
            stackPartidosEditar.Children.Clear();
            btnGuardarResultado.IsVisible = false;
            partidoActualIndex = 0;

            string categoria = pickerCategoriaEditar.SelectedItem as string;
            if (pickerJornadaEditar.SelectedIndex == -1 || string.IsNullOrEmpty(categoria))
                return;

            int numJornada = int.Parse(pickerJornadaEditar.SelectedItem.ToString());
            string ruta = categoria.ToLower() == "femenil" ? "jornadas_femenil" : "jornadas_varonil";
            string url = $"{FirebaseDatabaseUrl}/{ruta}/{numJornada - 1}.json?auth={MainPage.IdTokenUsuario}";

            using var cliente = new HttpClient();
            var respuesta = await cliente.GetAsync(url);
            if (!respuesta.IsSuccessStatusCode) return;

            string json = await respuesta.Content.ReadAsStringAsync();
            jornadaActual = JsonConvert.DeserializeObject<Jornada>(json);

            MostrarPartidoActual();
        }
        private async void GuardarCambiosJornada_Clicked(object sender, EventArgs e)
        {
            if (pickerCategoriaEditar.SelectedIndex == -1 || pickerJornadaEditar.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Selecciona categoría y jornada.", "OK");
                return;
            }

            var categoria = pickerCategoriaEditar.SelectedItem.ToString().ToLower();
            int numJornada = int.Parse(pickerJornadaEditar.SelectedItem.ToString());

            var partidos = new List<Partido>();
            foreach (var vista in stackPartidosEditar.Children)
            {
                if (vista is Grid grid && grid.BindingContext is Tuple<Partido, Entry, Entry> datos)
                {
                    var partido = datos.Item1;
                    int.TryParse(datos.Item2.Text, out int golesLocal);
                    int.TryParse(datos.Item3.Text, out int golesVisitante);

                    partido.GolesLocal = golesLocal;
                    partido.GolesVisitante = golesVisitante;
                    partidos.Add(partido);
                }
            }

            Jornada jornada = new Jornada
            {
                Numero = numJornada,
                Categoria = categoria.Equals("femenil") ? "Femenil" : "Varonil",
                Partidos = partidos
            };


            string ruta = categoria == "femenil" ? "jornadas_femenil" : "jornadas_varonil";
            string url = $"{FirebaseDatabaseUrl}/{ruta}/{numJornada - 1}.json?auth={MainPage.IdTokenUsuario}";
            string json = JsonConvert.SerializeObject(jornada);

            using var cliente = new HttpClient();
            var respuesta = await cliente.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

            if (respuesta.IsSuccessStatusCode)
            {
                await DisplayAlert("Éxito", "Resultados actualizados.", "OK");
                // Aquí puedes llamar tu función para actualizar la tabla de posiciones automáticamente
                ActualizarTablaDePosiciones(categoria); // Asegúrate de tener esta función implementada
            }
            else
            {
                await DisplayAlert("Error", "No se pudieron guardar los cambios.", "OK");
            }
        }


        private async void ActualizarTablaDePosiciones(string categoria)
        {
            string nombreCategoria = categoria.Equals("femenil", StringComparison.OrdinalIgnoreCase) ? "Femenil" : "Varonil";
            var tabla = _tablasPorCategoria[nombreCategoria];
            tabla.Clear();

            // 🔁 Cargar jornadas directamente por HttpClient
            string rutaJornadas = categoria.ToLower() == "femenil" ? "jornadas_femenil" : "jornadas_varonil";
            string urlJornadas = $"{FirebaseDatabaseUrl}/{rutaJornadas}.json?auth={MainPage.IdTokenUsuario}";

            using var http = new HttpClient();
            var response = await http.GetAsync(urlJornadas);

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert("Error", $"No se pudieron cargar jornadas de {categoria}", "OK");
                return;
            }

            string jsonJornadas = await response.Content.ReadAsStringAsync();
            List<Jornada> jornadas = JsonConvert.DeserializeObject<List<Jornada>>(jsonJornadas) ?? new List<Jornada>();

            // 🔁 Reunir todos los partidos con resultados válidos
            var partidosTotales = new List<Partido>();
            foreach (var jornada in jornadas)
            {
                if (jornada?.Partidos == null) continue;

                foreach (var partido in jornada.Partidos)
                {
                    if (partido.GolesLocal >= 0 && partido.GolesVisitante >= 0)
                        partidosTotales.Add(partido);
                }
            }

            // 🔁 Generar tabla temporal
            var equiposTemp = new Dictionary<string, EquipoTabla>();

            foreach (var partido in partidosTotales)
            {
                if (!equiposTemp.ContainsKey(partido.Local))
                    equiposTemp[partido.Local] = new EquipoTabla { Equipo = partido.Local };
                if (!equiposTemp.ContainsKey(partido.Visitante))
                    equiposTemp[partido.Visitante] = new EquipoTabla { Equipo = partido.Visitante };

                var local = equiposTemp[partido.Local];
                var visitante = equiposTemp[partido.Visitante];

                local.JJ++;
                visitante.JJ++;

                local.GF += partido.GolesLocal;
                local.GC += partido.GolesVisitante;

                visitante.GF += partido.GolesVisitante;
                visitante.GC += partido.GolesLocal;

                if (partido.GolesLocal > partido.GolesVisitante)
                {
                    local.JG++;
                    visitante.JP++;
                }
                else if (partido.GolesLocal < partido.GolesVisitante)
                {
                    visitante.JG++;
                    local.JP++;
                }
                else
                {
                    local.JE++;
                    visitante.JE++;
                }
            }

            tabla.AddRange(equiposTemp.Values);

            var tablaOrdenada = tabla
                .OrderByDescending(x => x.PTS)
                .ThenByDescending(x => x.DIF)
                .ThenByDescending(x => x.GF)
                .ToList();

            for (int i = 0; i < tablaOrdenada.Count; i++)
                tablaOrdenada[i].Posicion = i + 1;

            _tablasPorCategoria[nombreCategoria] = tablaOrdenada;

            // 🔁 Guardar en Firebase
            try
            {
                string rutaGuardar = $"tabla_{nombreCategoria.ToLower()}";
                string urlGuardar = $"{FirebaseDatabaseUrl}/{rutaGuardar}.json?auth={MainPage.IdTokenUsuario}";
                string jsonTabla = JsonConvert.SerializeObject(tablaOrdenada);

                var guardar = await http.PutAsync(urlGuardar, new StringContent(jsonTabla, Encoding.UTF8, "application/json"));

                if (!guardar.IsSuccessStatusCode)
                    await DisplayAlert("Error", $"No se pudo guardar la tabla de posiciones de {nombreCategoria}.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Error al guardar tabla: {ex.Message}", "OK");
            }

            // 🔁 Refrescar vista
            if (_categoriaTablaActual == nombreCategoria)
                ActualizarVistaTabla();
        }

        private async void btnGuardarResultado_Clicked(object sender, EventArgs e)
        {
            if (jornadaActual == null || partidoActualIndex >= jornadaActual.Partidos.Count)
                return;

            var grid = stackPartidosEditar.Children.FirstOrDefault() as Grid;
            if (grid?.BindingContext is Tuple<Partido, Entry, Entry> datos)
            {
                int.TryParse(datos.Item2.Text, out int golesLocal);
                int.TryParse(datos.Item3.Text, out int golesVisitante);

                datos.Item1.GolesLocal = golesLocal;
                datos.Item1.GolesVisitante = golesVisitante;

                string categoria = pickerCategoriaEditar.SelectedItem.ToString().ToLower();
                int numJornada = int.Parse(pickerJornadaEditar.SelectedItem.ToString());
                string ruta = categoria == "femenil" ? "jornadas_femenil" : "jornadas_varonil";
                string url = $"{FirebaseDatabaseUrl}/{ruta}/{numJornada - 1}.json?auth={MainPage.IdTokenUsuario}";
                string json = JsonConvert.SerializeObject(jornadaActual);

                using var cliente = new HttpClient();
                var respuesta = await cliente.PutAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

                if (respuesta.IsSuccessStatusCode)
                {
                    await DisplayAlert("Éxito", "Resultado guardado correctamente.", "OK");

                    // Actualiza tabla automáticamente
                    ActualizarTablaDePosiciones(categoria);
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo guardar el resultado.", "OK");
                }
            }
        }
        //BOTONES DE ATRAS Y ADELANTE
        private void MostrarPartidoActual()
        {
            stackPartidosEditar.Children.Clear();
            btnGuardarResultado.IsVisible = false;
            btnAnteriorPartido.IsVisible = false;
            btnSiguientePartido.IsVisible = false;

            if (jornadaActual?.Partidos == null || jornadaActual.Partidos.Count == 0 || partidoActualIndex >= jornadaActual.Partidos.Count)
                return;

            var partido = jornadaActual.Partidos[partidoActualIndex];

            var grid = new Grid { ColumnSpacing = 5 };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var entryGolesLocal = new Entry { Text = partido.GolesLocal.ToString(), Keyboard = Keyboard.Numeric };
            var entryGolesVisitante = new Entry { Text = partido.GolesVisitante.ToString(), Keyboard = Keyboard.Numeric };

            grid.Children.Add(new Label { Text = partido.Local }, 0, 0);
            grid.Children.Add(entryGolesLocal, 1, 0);
            grid.Children.Add(entryGolesVisitante, 2, 0);
            grid.Children.Add(new Label { Text = partido.Visitante }, 3, 0);

            grid.BindingContext = new Tuple<Partido, Entry, Entry>(partido, entryGolesLocal, entryGolesVisitante);

            stackPartidosEditar.Children.Add(grid);

            btnGuardarResultado.IsVisible = true;
            btnAnteriorPartido.IsVisible = partidoActualIndex > 0;
            btnSiguientePartido.IsVisible = partidoActualIndex < jornadaActual.Partidos.Count - 1;
        }

        private void btnAnteriorPartido_Clicked(object sender, EventArgs e)
        {
            if (partidoActualIndex > 0)
            {
                partidoActualIndex--;
                MostrarPartidoActual();
            }
        }

        private void btnSiguientePartido_Clicked(object sender, EventArgs e)
        {
            if (jornadaActual != null && partidoActualIndex < jornadaActual.Partidos.Count - 1)
            {
                partidoActualIndex++;
                MostrarPartidoActual();
            }
        }




        


        private async void Volver_Clicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
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

        /*********************************GOLEADORES fiiiiin********************************/




        


    }
}

