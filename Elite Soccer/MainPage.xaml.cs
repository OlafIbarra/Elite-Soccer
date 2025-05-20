using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Elite_Soccer.Vistas;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Elite_Soccer
{
    public partial class MainPage : ContentPage
    {
        private const string FirebaseApiKey = "AIzaSyABVSBLEnEWNa5EggbWaUqynwTqoe1IZm4";

        public MainPage()
        {
            InitializeComponent();
        }

        private async void BtnIniciarSesion_Clicked(object sender, EventArgs e)
        {
            string correo = txtCorreo.Text;
            string contrasena = txtContrasena.Text;

            lblMensaje.IsVisible = false;
            indicadorCarga.IsVisible = true;
            indicadorCarga.IsRunning = true;

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                lblMensaje.Text = "Por favor, completa todos los campos.";
                lblMensaje.IsVisible = true;
                indicadorCarga.IsRunning = false;
                indicadorCarga.IsVisible = false;
                return;
            }

            bool exito = await IniciarSesionAsync(correo, contrasena);

            indicadorCarga.IsRunning = false;
            indicadorCarga.IsVisible = false;

            if (exito)
            {
                if (correo == "admin@elite.com" && contrasena == "admin123")
                {
                    await Navigation.PushAsync(new AdminPage());
                }
                else
                {
                    await Navigation.PushAsync(new PaginaUsuario());
                }
            }
            else
            {
                lblMensaje.Text = "Correo o contraseña incorrectos.";
                lblMensaje.IsVisible = true;
            }
        }


        public static string IdTokenUsuario { get; private set; } // Añádelo en tu clase MainPage

        public static async Task<bool> IniciarSesionAsync(string correo, string contrasena)
        {
            string url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}";
            var datos = new { email = correo, password = contrasena, returnSecureToken = true };
            string json = JsonConvert.SerializeObject(datos);
            var contenido = new StringContent(json, Encoding.UTF8, "application/json");

            using (HttpClient cliente = new HttpClient())
            {
                HttpResponseMessage respuesta = await cliente.PostAsync(url, contenido);
                if (respuesta.IsSuccessStatusCode)
                {
                    string resultado = await respuesta.Content.ReadAsStringAsync();
                    var datosRespuesta = JsonConvert.DeserializeObject<FirebaseAuthResponse>(resultado);
                    IdTokenUsuario = datosRespuesta.idToken; // Guardamos el idToken
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }



        private async void RegistroUsuario_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegistroUsuario());
        }
    }
    public class FirebaseAuthResponse
    {
        public string idToken { get; set; }
        public string email { get; set; }
        public string refreshToken { get; set; }
        public string expiresIn { get; set; }
        public string localId { get; set; }
    }

}
