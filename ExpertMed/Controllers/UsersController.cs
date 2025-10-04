using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.AspNetCore.Mvc;

namespace ExpertMed.Controllers
{
    public class UsersController : Controller
    {
        private readonly UserService _userService;
        private readonly SelectsService _selectsService;
        private readonly ILogger<UsersController> _logger;
        private readonly MedicalOfficeService _clinicaService;
        public UsersController(UserService userService, SelectsService selectsService, ILogger<UsersController> logger, MedicalOfficeService clinicaService)
        {
            _userService = userService;
            _selectsService = selectsService;
            _logger = logger;
            _clinicaService = clinicaService;
        }

        public IActionResult UserList()
        {
            try
            {
                int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
                int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;
                List<Users> users = _userService.GetAllUsers(usuarioId, perfilId);
                return View(users);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocurrió un error al obtener la lista de usuarios.";
                return View("UserList");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeUserStatus(int userId, int status)
        {
            var result = await _userService.DesactiveOrActiveUserAsync(userId, status);
            TempData[result.success ? "SuccessMessage" : "ErrorMessage"] = result.message;
            return RedirectToAction("UserList");
        }

        [HttpGet]
        public async Task<IActionResult> NewUser()
        {
            try
            {
                int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
                int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;

                var profiles = await _selectsService.GetAllProfilesAsync();
                var specialties = await _selectsService.GetAllSpecialtiesAsync();
                var medics = await _selectsService.GetAllMedicsAsync(perfilId, usuarioId);
                var countries = await _selectsService.GetAllCountriesAsync();
                var percentage = await _selectsService.GetAllVatPercentageAsync();
                var establishments = await _selectsService.GetAllEstablishmentAsync(perfilId, usuarioId);

                var consultorios = await _clinicaService.GetMedicalOfficesAsync(perfilId, usuarioId);

                var viewModel = new NewUserViewModel
                {
                    Profiles = profiles,
                    Specialties = specialties,
                    Users = medics,
                    Countries = countries,
                    VatBillings = percentage,
                    Establishments = establishments,
                    MedicalOfficeListDtos = consultorios

                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar datos para NewUser.");
                return View("Error");
            }
        }
        [HttpPost]
        public async Task<IActionResult> NewUser(
            UserViewModel usuario,
            IFormFile? ProfilePhoto,
            string? selectedDoctorIds,
            IFormFile? CompanyLogo,
            IFormFile? CertificateP12,
            IFormFile? CompanySignature,
            IFormFile? CompanyStamp)
        {
            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"Campo: {state.Key} → Error: {error.ErrorMessage}");
                    }
                }

                TempData["ErrorMessage"] = "Datos inválidos. Por favor, revisa los campos e intenta de nuevo.";

                int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
                int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;

                var viewModel = new NewUserViewModel
                {
                    Profiles = await _selectsService.GetAllProfilesAsync(),
                    Specialties = await _selectsService.GetAllSpecialtiesAsync(),
                    Users = await _selectsService.GetAllMedicsAsync(perfilId, usuarioId),
                    Countries = await _selectsService.GetAllCountriesAsync(),
                    VatBillings = await _selectsService.GetAllVatPercentageAsync(),
                    Establishments = await _selectsService.GetAllEstablishmentAsync(perfilId, usuarioId),
                    MedicalOfficeListDtos = await _clinicaService.GetMedicalOfficesAsync(perfilId, usuarioId)
                };

                return View(viewModel);
            }

            foreach (var file in Request.Form.Files)
            {
                Console.WriteLine($"Archivo recibido: {file.Name} - {file.FileName} - {file.ContentType}");
            }

            int usuarioCreadorId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            usuario.AssignedBy = usuarioCreadorId;

            // Foto de perfil
            usuario.UserProfilephoto = await ConvertFileToByteArray(ProfilePhoto);

            // Logo
            if (CompanyLogo is { Length: > 0 })
            {
                using var msLogo = new MemoryStream();
                await CompanyLogo.CopyToAsync(msLogo);
                usuario.CompanyLogoBytes = msLogo.ToArray();
                usuario.CompanyLogoFileName = Path.GetFileName(CompanyLogo.FileName);
                usuario.CompanyLogoContentType = CompanyLogo.ContentType;
            }
            else
            {
                usuario.CompanyLogoBytes = new byte[] { 0 }; // Binario mínimo
                usuario.CompanyLogoFileName = "default-logo.png";
                usuario.CompanyLogoContentType = "image/png";
            }

            // Firma digital
            if (CompanySignature is { Length: > 0 })
            {
                using var msSign = new MemoryStream();
                await CompanySignature.CopyToAsync(msSign);
                usuario.CompanySignatureBytes = msSign.ToArray();
                usuario.CompanySignatureFileName = Path.GetFileName(CompanySignature.FileName);
                usuario.CompanySignatureContentType = CompanySignature.ContentType;
            }
            else
            {
                usuario.CompanySignatureBytes = new byte[] { 0 };
                usuario.CompanySignatureFileName = "default-signature.png";
                usuario.CompanySignatureContentType = "image/png";
            }

            // Sello
            if (CompanyStamp is { Length: > 0 })
            {
                using var msStamp = new MemoryStream();
                await CompanyStamp.CopyToAsync(msStamp);
                usuario.CompanyStampBytes = msStamp.ToArray();
                usuario.CompanyStampFileName = Path.GetFileName(CompanyStamp.FileName);
                usuario.CompanyStampContentType = CompanyStamp.ContentType;
            }
            else
            {
                usuario.CompanyStampBytes = new byte[] { 0 };
                usuario.CompanyStampFileName = "default-stamp.png";
                usuario.CompanyStampContentType = "image/png";
            }

            // Certificado .p12
            if (CertificateP12 is { Length: > 0 })
            {
                using var msCert = new MemoryStream();
                await CertificateP12.CopyToAsync(msCert);
                usuario.CertificateP12Bytes = msCert.ToArray();
                usuario.CertificateP12FileName = Path.GetFileName(CertificateP12.FileName);
                usuario.CertificateP12ContentType = CertificateP12.ContentType;
            }
            else
            {
                usuario.CertificateP12Bytes = new byte[] { 0 };
                usuario.CertificateP12FileName = "default-cert.p12";
                usuario.CertificateP12ContentType = "application/x-pkcs12";
            }


            // Doctores asociados
            List<int>? associatedDoctorIds = selectedDoctorIds?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            try
            {
                int idUsuarioCreado = await _userService.CreateUserAsync(usuario, associatedDoctorIds);
                TempData["SuccessMessage"] = "Usuario creado satisfactoriamente.";
                return RedirectToAction("UserList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.Contains("CI ya existe") ? "El usuario con este CI ya existe."
                                        : ex.Message.Contains("nombre de usuario ya existe") ? "El nombre de usuario ya existe."
                                        : "Error inesperado: " + ex.Message;

                int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
                int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;

                var viewModel = new NewUserViewModel
                {
                    Profiles = await _selectsService.GetAllProfilesAsync(),
                    Specialties = await _selectsService.GetAllSpecialtiesAsync(),
                    Users = await _selectsService.GetAllMedicsAsync(perfilId, usuarioId),
                    Countries = await _selectsService.GetAllCountriesAsync(),
                    VatBillings = await _selectsService.GetAllVatPercentageAsync(),
                    Establishments = await _selectsService.GetAllEstablishmentAsync(perfilId, usuarioId),
                    MedicalOfficeListDtos = await _clinicaService.GetMedicalOfficesAsync(perfilId, usuarioId)
                };

                return View(viewModel);
            }
        }


        [HttpGet("Actualizar-Usuario")]
        public async Task<IActionResult> UpdateUser(int id)
        {
            var user = await _userService.GetUserDetailsAsync(id);
            if (user == null) return NotFound("User Not Found");

            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;

            var profiles = await _selectsService.GetAllProfilesAsync();
            var specialties = await _selectsService.GetAllSpecialtiesAsync();
            var countries = await _selectsService.GetAllCountriesAsync();
            var percentage = await _selectsService.GetAllVatPercentageAsync();
            var establishments = await _selectsService.GetAllEstablishmentAsync(perfilId, usuarioId);
            var medics = await _selectsService.GetAllMedicsAsync(perfilId, usuarioId);
            var consultorios = await _clinicaService.GetMedicalOfficesAsync(perfilId, usuarioId);

            var viewModel = new NewUserViewModel
            {
                User = user,
                Profiles = profiles,
                Specialties = specialties,
                Countries = countries,
                VatBillings = percentage,
                Establishments = establishments,
                Users = medics,
                AssociatedDoctors = user.Doctors,
                MedicalOfficeListDtos = consultorios

            };

            return View(viewModel);
        }



        [HttpGet("Actualizar_Datos_Personales")]
        public async Task<IActionResult> UpdateUserP(int id)
        {
            var user = await _userService.GetUserDetailsAsync(id);
            if (user == null) return NotFound("User Not Found");

            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;

            var profiles = await _selectsService.GetAllProfilesAsync();
            var specialties = await _selectsService.GetAllSpecialtiesAsync();
            var countries = await _selectsService.GetAllCountriesAsync();
            var percentage = await _selectsService.GetAllVatPercentageAsync();
            var establishments = await _selectsService.GetAllEstablishmentAsync(perfilId, usuarioId);
            var medics = await _selectsService.GetAllMedicsAsync(perfilId, usuarioId);
            var consultorios = await _clinicaService.GetMedicalOfficesAsync(perfilId, usuarioId);

            var viewModel = new NewUserViewModel
            {
                User = user,
                Profiles = profiles,
                Specialties = specialties,
                Countries = countries,
                VatBillings = percentage,
                Establishments = establishments,
                Users = medics,
                AssociatedDoctors = user.Doctors,
                MedicalOfficeListDtos = consultorios
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserMethod(
       UserViewModel usuario,
       IFormFile? ProfilePhoto,
       string? selectedDoctorIds,
       IFormFile? CompanyLogo,
       IFormFile? CompanySignature,
       IFormFile? CompanyStamp,
       IFormFile? CertificateP12,
       int id)
        {
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(kvp => kvp.Value.Errors.Any())
                    .Select(kvp =>
                        $"Campo: {kvp.Key} - Error: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}")
                    .ToList();

                TempData["ErrorMessage"] = "Datos inválidos:\n" + string.Join("\n", errores);

                return await ReloadUserEditView(id);
            }


            // Asignar usuario modificador
            usuario.AssignedBy = HttpContext.Session.GetInt32("UsuarioId") ?? 0;

            // Foto de perfil
            usuario.UserProfilephoto = await ConvertFileToByteArray(ProfilePhoto);

            // LOGO
if (CompanyLogo is { Length: > 0 })
{
    using var ms = new MemoryStream();
    await CompanyLogo.CopyToAsync(ms);
    usuario.CompanyLogoBytes = ms.ToArray();
    usuario.CompanyLogoFileName = "logotipo.png";
    usuario.CompanyLogoContentType = CompanyLogo.ContentType;
}
// No hay nuevo archivo, mantenemos el actual si existe
else if (!string.IsNullOrEmpty(usuario.CompanyLogoBase64))
{
    usuario.CompanyLogoBytes = Convert.FromBase64String(usuario.CompanyLogoBase64.Replace("data:image/png;base64,", ""));
    usuario.CompanyLogoFileName = "logotipo.png";
    usuario.CompanyLogoContentType = "image/png";
}
else
{
    usuario.CompanyLogoBytes = null;
    usuario.CompanyLogoFileName = null;
    usuario.CompanyLogoContentType = null;
}

// FIRMA
if (CompanySignature is { Length: > 0 })
{
    using var ms = new MemoryStream();
    await CompanySignature.CopyToAsync(ms);
    usuario.CompanySignatureBytes = ms.ToArray();
    usuario.CompanySignatureFileName = "firma.png";
    usuario.CompanySignatureContentType = CompanySignature.ContentType;
}
else if (!string.IsNullOrEmpty(usuario.CompanySignatureBase64))
{
    usuario.CompanySignatureBytes = Convert.FromBase64String(usuario.CompanySignatureBase64.Replace("data:image/png;base64,", ""));
    usuario.CompanySignatureFileName = "firma.png";
    usuario.CompanySignatureContentType = "image/png";
}
else
{
    usuario.CompanySignatureBytes = null;
    usuario.CompanySignatureFileName = null;
    usuario.CompanySignatureContentType = null;
}

// SELLO
if (CompanyStamp is { Length: > 0 })
{
    using var ms = new MemoryStream();
    await CompanyStamp.CopyToAsync(ms);
    usuario.CompanyStampBytes = ms.ToArray();
    usuario.CompanyStampFileName = "sello.png";
    usuario.CompanyStampContentType = CompanyStamp.ContentType;
}
else if (!string.IsNullOrEmpty(usuario.CompanyStampBase64))
{
    usuario.CompanyStampBytes = Convert.FromBase64String(usuario.CompanyStampBase64.Replace("data:image/png;base64,", ""));
    usuario.CompanyStampFileName = "sello.png";
    usuario.CompanyStampContentType = "image/png";
}
else
{
    usuario.CompanyStampBytes = null;
    usuario.CompanyStampFileName = null;
    usuario.CompanyStampContentType = null;
}

// CERTIFICADO
if (CertificateP12 is { Length: > 0 })
{
    using var ms = new MemoryStream();
    await CertificateP12.CopyToAsync(ms);
    usuario.CertificateP12Bytes = ms.ToArray();
    usuario.CertificateP12FileName = "certificado.p12";
    usuario.CertificateP12ContentType = CertificateP12.ContentType;
}
else if (!string.IsNullOrEmpty(usuario.CertificateP12Base64))
{
    usuario.CertificateP12Bytes = Convert.FromBase64String(usuario.CertificateP12Base64.Replace("data:application/x-pkcs12;base64,", ""));
    usuario.CertificateP12FileName = "certificado.p12";
    usuario.CertificateP12ContentType = "application/x-pkcs12";
}
else
{
    usuario.CertificateP12Bytes = null;
    usuario.CertificateP12FileName = null;
    usuario.CertificateP12ContentType = null;
}


            // Relación con médicos
            List<int>? associatedDoctorIds = selectedDoctorIds?
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            try
            {
                await _userService.UpdateUserAsync(id, usuario, associatedDoctorIds);
                TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                return RedirectToAction("UserList", "Users");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message.Contains("El usuario no existe.") ? "El usuario no existe." :
                    ex.Message.Contains("documento ya existe") ? "El número de documento ya existe." :
                    ex.Message.Contains("usuario ya está registrado") ? "El nombre de usuario ya está registrado." :
                    "Error inesperado: " + ex.Message;

                return await ReloadUserEditView(id);
            }
        }


        private async Task<IActionResult> ReloadUserEditView(int id)
        {
            var user = await _userService.GetUserDetailsAsync(id);
            if (user == null) return NotFound("Usuario no encontrado.");

            int usuarioId = HttpContext.Session.GetInt32("UsuarioId") ?? 0;
            int perfilId = HttpContext.Session.GetInt32("PerfilId") ?? 0;

            var profiles = await _selectsService.GetAllProfilesAsync();
            var specialties = await _selectsService.GetAllSpecialtiesAsync();
            var countries = await _selectsService.GetAllCountriesAsync();
            var percentage = await _selectsService.GetAllVatPercentageAsync();
            var establishments = await _selectsService.GetAllEstablishmentAsync(perfilId, usuarioId);
            var medics = await _selectsService.GetAllMedicsAsync(perfilId, usuarioId);

            var viewModel = new NewUserViewModel
            {
                User = user,
                Profiles = profiles,
                Specialties = specialties,
                Countries = countries,
                VatBillings = percentage,
                Establishments = establishments,
                Users = medics,
                AssociatedDoctors = user.Doctors
            };

            return View(viewModel);
        }

        public IActionResult ProfileSimple()
        {
            return View();
        }

        private async Task<byte[]> ConvertFileToByteArray(IFormFile file)
        {
            if (file != null && file.Length > 0)
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
            return null;
        }
    }
}
