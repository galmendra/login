
// Controlador que maneja las acciones relacionadas con autenticación
// Usa el mismo patrón que HomeController para conectar con Supabase
using System.Text.Json;
using login.Models;
using Microsoft.AspNetCore.Mvc;
namespace login1.Controllers
{
    public class AuthController : Controller
    {
        // Variables privadas (igual que en HomeController)
        private readonly HttpClient _httpClient;
        private readonly string _supabaseUrl;
        private readonly string _supabaseKey;

        // Constructor: inyección de dependencia (IGUAL que HomeController)
        public AuthController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            // Crear HttpClient usando el factory (mismo patrón)
            _httpClient = httpClientFactory.CreateClient();

            // Obtener configuraciones de Supabase desde appsettings.json (mismo patrón)
            _supabaseUrl = configuration["Supabase:Url"] ?? string.Empty;
            _supabaseKey = configuration["Supabase:Key"] ?? string.Empty;

            // Configurar headers para Supabase (IGUAL que HomeController)
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseKey);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_supabaseKey}");
        }

        // GET: /Auth/Login
        // Muestra el formulario de login
        [HttpGet]
        public IActionResult Login()
        {
            // Crear un modelo vacío para el formulario
            var model = new LoginViewModel();
            return View(model);
        }

        // POST: /Auth/Login
        // Procesa el formulario de login enviado
        [HttpPost]
        [ValidateAntiForgeryToken]  // Protección contra ataques CSRF
        public async Task<IActionResult> login(LoginViewModel model)
        {
            // Verificar si el modelo es válido (validaciones de DataAnnotations)
            if (!ModelState.IsValid)
            {
                // Si hay errores de validación, mostrar el formulario nuevamente
                return View(model);
            }

            try
            {
                // Intentar validar las credenciales
                var user = await ValidateUserAsync(model);

                if (user != null)
                {
                    // ✅ Credenciales correctas: redirigir al dashboard
                    // Pasamos el nombre de usuario al dashboard
                    TempData["Username"] = user.Username;
                    TempData["WelcomeMessage"] = $"¡Bienvenido, {user.Username}!";

                    return RedirectToAction("Dashboard");
                }
                else
                {
                    // ❌ Credenciales incorrectas: mostrar error
                    model.ErrorMessage = "Usuario o contraseña incorrectos";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                // Error del sistema: mostrar mensaje genérico
                Console.WriteLine($"Error en Login: {ex.Message}");
                model.ErrorMessage = "Error del sistema. Intenta nuevamente.";
                return View(model);
            }
        }

        // GET: /Auth/Dashboard
        // Página de bienvenida después del login exitoso
        [HttpGet]
        public IActionResult Dashboard()
        {
            // Verificar si tenemos datos del usuario logueado
            if (TempData["Username"] == null)
            {
                // Si no hay datos, redirigir al login
                return RedirectToAction("Login");
            }

            // Mantener los datos para la vista
            ViewBag.Username = TempData["Username"];
            ViewBag.WelcomeMessage = TempData["WelcomeMessage"];

            return View();
        }

        // GET: /Auth/Logout
        // Cerrar sesión (simple redirección por ahora)
        [HttpGet]
        public IActionResult Logout()
        {
            // Limpiar datos temporales
            TempData.Clear();

            // Redirigir al home o login
            return RedirectToAction("Index", "Home");
        }

        // MÉTODO PRIVADO: Validar credenciales (toda la lógica de Supabase aquí)
        private async Task<UserModel?> ValidateUserAsync(LoginViewModel model)
        {
            try
            {
                // 1. Buscar el usuario por username en Supabase
                var user = await GetUserByUsernameAsync(model.Username);

                // 2. Si no se encuentra el usuario, retornar null
                if (user == null)
                {
                    return null;
                }

                // 3. Verificar la contraseña usando BCrypt
                // BCrypt.Verify compara la contraseña en texto plano con el hash guardado
                // para encriptar el password,
                //var pass = BCrypt.Net.BCrypt.HashPassword(model.Password);
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);


                // 4. Si la contraseña es correcta, retornar el usuario
                if (isPasswordValid)
                {
                    return user;
                }

                // 5. Si la contraseña es incorrecta, retornar null
                return null;
            }
            catch (Exception ex)
            {
                // En caso de error, loguear y retornar null
                Console.WriteLine($"Error en ValidateUserAsync: {ex.Message}");
                return null;
            }
        }

        // MÉTODO PRIVADO: Buscar usuario por username (usando HttpClient como HomeController)
        private async Task<UserModel?> GetUserByUsernameAsync(string username)
        {
            try
            {
                // Construir URL para filtrar por username
                // eq significa "equal" (igual) en la API de Supabase
                string url = $"{_supabaseUrl}/rest/v1/users?username=eq.{username}";

                // Hacer petición GET a Supabase (igual que en HomeController)
                var response = await _httpClient.GetAsync(url);

                // Verificar si la petición fue exitosa
                if (response.IsSuccessStatusCode)
                {
                    // Leer el contenido de la respuesta
                    var responseContent = await response.Content.ReadAsStringAsync();

                    // Deserializar JSON a array de UserModel
                    var users = JsonSerializer.Deserialize<UserModel[]>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower // Supabase usa snake_case
                    });

                    // Retornar el primer usuario encontrado (debería ser único)
                    return users?.FirstOrDefault();
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en GetUserByUsernameAsync: {ex.Message}");
                return null;
            }
        }
    }
}
