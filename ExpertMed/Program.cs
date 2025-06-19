using ExpertMed.Models;
using ExpertMed.Services;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

namespace ExpertMed
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Establecer cultura por defecto a en-US para evitar errores con punto decimal
            var defaultCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
            CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

            builder.Services.Configure<RequestLocalizationOptions>(options =>
            {
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = new List<CultureInfo> { defaultCulture };
                options.SupportedUICultures = new List<CultureInfo> { defaultCulture };
            });

            // Registrar IHttpClientFactory
            builder.Services.AddHttpClient();

            // Configuración de la base de datos
            builder.Services.AddDbContext<DbExpertmedContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("conexion")));

            // Habilitar Razor Pages con recompilación en tiempo de ejecución
            builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

            // Registrar IHttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // Registrar servicios personalizados
            builder.Services.AddScoped<AuthenticationService>();
            builder.Services.AddScoped<UserService>();
            builder.Services.AddScoped<SelectsService>();
            builder.Services.AddScoped<PatientService>();
            builder.Services.AddScoped<AppointmentService>();
            builder.Services.AddScoped<ConsultationService>();
            builder.Services.AddScoped<BillingServices>();
            builder.Services.AddScoped<ChatGPTService>();
            builder.Services.AddScoped<LaboratoryService>();
            builder.Services.AddScoped<MedicalOfficeService>();
            builder.Services.AddScoped<ImagesService>();
            builder.Services.AddScoped<ReportService>();
            builder.Services.AddScoped<FavoriteMedicationService>();
            builder.Services.AddScoped<TherapyService>();
            builder.Services.AddScoped<TarifarioService>();
            builder.Services.AddScoped<EstablishmentService>();

            // Configuración de controladores y vistas
            builder.Services.AddControllersWithViews();

            // Configuración de la sesión
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración de la sesión
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Conversor personalizado para TimeOnly
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonTimeOnlyConverter());
            });

            // Licencia de QuestPDF
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var app = builder.Build();

            // Habilitar el uso de sesiones
            app.UseSession();

            // Configuración del pipeline HTTP
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Habilitar cultura para interpretación de decimales (punto)
            app.UseRequestLocalization();

            app.UseAuthorization();

            // Configuración de endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Authentication}/{action=SignIn}/{id?}");
            });

            // Configuración de Rotativa para PDFs
            IWebHostEnvironment env = app.Environment;
            RotativaConfiguration.Setup(env.WebRootPath, "Rotativa/Windows");

            app.Run();
        }
    }
}
