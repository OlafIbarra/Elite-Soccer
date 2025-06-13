using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using Elite_Soccer.Vistas;



namespace Elite_Soccer
{
    public partial class RegistroUsuario : ContentPage
    {
        private static readonly HttpClient clienteHttp = new HttpClient();
        private const string ApiKey = "AIzaSyABVSBLEnEWNa5EggbWaUqynwTqoe1IZm4";

        public RegistroUsuario() => InitializeComponent();

        class RespuestaFirebase { public string localId { get; set; } }

        class RespuestaGoogle
        {
            public string localId { get; set; }
            public string email { get; set; }
            public string displayName { get; set; }
        }

        private async void BtnRegistrarse_Clicked(object sender, EventArgs e)
        {
            string nombre = txtNombre.Text?.Trim();
            string correo = txtCorreo.Text?.Trim();
            string contrasena = txtContrasena.Text;

            if (string.IsNullOrEmpty(nombre) || !correo.Contains("@") || string.IsNullOrWhiteSpace(contrasena) || contrasena.Length < 6)
            {
                await DisplayAlert("Error", "Nombre, correo válido y contraseña (mínimo 6 caracteres) son obligatorios", "OK");
                return;
            }

            var datos = new { email = correo, password = contrasena, returnSecureToken = true };
            var contenido = new StringContent(JsonConvert.SerializeObject(datos), Encoding.UTF8, "application/json");

            var resp = await clienteHttp.PostAsync(
              $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={ApiKey}",
              contenido
            );

            if (resp.IsSuccessStatusCode)
            {
                var res = JsonConvert.DeserializeObject<RespuestaFirebase>(await resp.Content.ReadAsStringAsync());

                var usuarioInfo = new { nombre, correo, rol = "Usuario" };
                await clienteHttp.PutAsync(
                  $"https://clubeliteapp-default-rtdb.firebaseio.com/usuarios/{res.localId}.json",
                  new StringContent(JsonConvert.SerializeObject(usuarioInfo), Encoding.UTF8, "application/json")
                );

                await DisplayAlert("Éxito", "Usuario registrado correctamente", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                var error = await resp.Content.ReadAsStringAsync();
                await DisplayAlert("Error", $"No se pudo registrar: {error}", "OK");
            }
        }
    private async void InicioSesion_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }
    }
}
