using System;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Elite_Soccer
{
    public partial class RegistroUsuario : ContentPage
    {
        private static readonly HttpClient clienteHttp = new HttpClient();
        private const string ApiKey = "AIzaSyABVSBLEnEWNa5EggbWaUqynwTqoe1IZm4"; 

        public RegistroUsuario()
        {
            InitializeComponent();
        }

        private async void BtnRegistrarse_Clicked(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text;
            string correo = txtCorreo.Text;
            string contrasena = txtContrasena.Text;
            string rol = "Usuario"; // ← Rol fijo, ya no se escoge

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contrasena))
            {
                await DisplayAlert("Error", "Todos los campos son obligatorios", "OK");
                return;
            }

            // Crear cuenta en Firebase Authentication
            var datosRegistro = new
            {
                email = correo,
                password = contrasena,
                returnSecureToken = true
            };

            string json = JsonConvert.SerializeObject(datosRegistro);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");

            string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={ApiKey}";
            HttpResponseMessage respuesta = await clienteHttp.PostAsync(url, contenido);

            if (respuesta.IsSuccessStatusCode)
            {
                // Obtener el UID del usuario creado
                var respuestaJson = await respuesta.Content.ReadAsStringAsync();
                var datosRespuesta = JsonConvert.DeserializeObject<RespuestaFirebase>(respuestaJson);

                // Guardar nombre y rol en Realtime Database
                var datosExtra = new
                {
                    nombre = nombre,
                    correo = correo,
                    rol = rol
                };

                string dbUrl = $"https://clubeliteapp-default-rtdb.firebaseio.com/usuarios/{datosRespuesta.localId}.json";
                string jsonDatosExtra = JsonConvert.SerializeObject(datosExtra);
                await clienteHttp.PutAsync(dbUrl, new StringContent(jsonDatosExtra, Encoding.UTF8, "application/json"));

                await DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");
                await Navigation.PopAsync(); // Volver atrás
            }
            else
            {
                await DisplayAlert("Error", "No se pudo registrar el usuario", "OK");
            }
        }

        public class RespuestaFirebase
        {
            public string localId { get; set; }
        }
    }
}
