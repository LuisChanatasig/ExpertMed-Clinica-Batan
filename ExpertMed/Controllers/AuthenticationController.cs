using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ExpertMed.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly AuthenticationService _userService;

        public AuthenticationController(AuthenticationService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> SignInBasic(string username, string password)
        {
            // Verificación de campos vacíos
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new { success = false, errorMessage = "El nombre de usuario y la contraseña son obligatorios." });
            }

            try
            {
                // Validar credenciales
                var user = await _userService.ValidateCredentialsAsync(username, password);

                if (user == null)
                {
                    return Json(new { success = false, errorMessage = "Credenciales no válidas." });
                }

                // Configurar los datos de sesión
                var session = HttpContext.Session;
                session.SetString("UsuarioNombre", user.UsersNames ?? "No Name");
                session.SetInt32("UsuarioId", user.UsersId);
                session.SetInt32("UsuarioEspecialidadId", user.UsersSpecialityid ?? 0);
                session.SetString("UsuarioApellido", user.UsersSurcenames ?? "No Surname");
                session.SetString("UsuarioEmail", user.UsersEmail ?? "No Email");
                session.SetString("UsuarioPerfil", user.UsersProfile?.ProfileDescription ?? "No Profile");
                session.SetString("UsuarioEspecialidad", user.UsersSpeciality?.SpecialityName ?? "No Specialty");
                session.SetString("UsuarioEstablecimiento", user.UserEstablishment.EstablishmentAddress ?? "No Establishment");
                session.SetString("UsuarioFotoPerfil", ConvertToBase64(user.UsersProfilephoto)); // Guardar imagen en Base64

                // Retornar el éxito y la URL de redirección
                return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Json(new { success = false, errorMessage = $"Acceso no autorizado: {ex.Message}" });
            }
            catch (ArgumentNullException ex)
            {
                return Json(new { success = false, errorMessage = $"Error de argumento nulo: {ex.Message}" });
            }
            catch (SqlException ex)
            {
                return Json(new { success = false, errorMessage = $"Error de base de datos: {ex.Message}" });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, errorMessage = $"Operación no válida: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = $"Ocurrió un error inesperado: {ex.Message}" });
            }
        }

        private static string ConvertToBase64(byte[] data)
        {
            return data != null && data.Length > 0 ? Convert.ToBase64String(data) : string.Empty;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return View(); // Muestra la vista de inicio de sesión
        }

        public IActionResult SignOut()
        {
            // Limpiar la sesión
            HttpContext.Session.Clear();
            return RedirectToAction("SignOutBasic");
        }
        public IActionResult SignOutBasic()
        {
            return View();
        }

        //[HttpPost]
        //public async Task<IActionResult> SignInBasic()
        //{


        //    return View();
        //}


    }
}
