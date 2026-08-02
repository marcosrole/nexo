using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nexo.Server.Data;
using Nexo.Shared.Models;

namespace Nexo.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<NexoDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
                {
                    // Equipo chico (2-3 personas): password simple, sin exigencias de complejidad.
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequiredLength = 6;
                })
                .AddEntityFrameworkStores<NexoDbContext>()
                .AddDefaultTokenProviders();

            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";

                // Las llamadas del cliente Blazor a /api deben recibir 401/403 planos,
                // no una redirección HTML a la página de login.
                options.Events.OnRedirectToLogin = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 401;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    if (context.Request.Path.StartsWithSegments("/api"))
                    {
                        context.Response.StatusCode = 403;
                        return Task.CompletedTask;
                    }
                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = Configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
                });

            services.AddControllersWithViews();
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<NexoDbContext>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<Startup>>();
                db.Database.Migrate();
                SeedRolesAsync(scope.ServiceProvider).GetAwaiter().GetResult();
                SeedAdminAsync(scope.ServiceProvider, Configuration, logger).GetAwaiter().GetResult();
                SeedAdminsAsync(scope.ServiceProvider, Configuration, logger).GetAwaiter().GetResult();
                SeedCatalogosAsync(db).GetAwaiter().GetResult();
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        private static async Task SeedRolesAsync(System.IServiceProvider services)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();

            foreach (var role in new[] { "SuperAdministrador", "Administrador" })
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int>(role));
                }
            }

            // El rol Operador se dio de baja (2026-08-02): nunca se le asigno a ningun usuario,
            // se elimina si quedo de un deploy anterior.
            var operador = await roleManager.FindByNameAsync("Operador");
            if (operador != null)
            {
                await roleManager.DeleteAsync(operador);
            }
        }

        // Bootstrap del primer usuario, con el rol mas alto (SuperAdministrador: acceso total).
        // Las credenciales NUNCA viven en appsettings.json: se cargan con
        // `dotnet user-secrets set "SeedAdmin:UserName" ...` / "SeedAdmin:Password" en desarrollo,
        // o como variables de entorno en el hosting real. Si no están configuradas, no hace nada.
        private static async Task SeedAdminAsync(System.IServiceProvider services, IConfiguration configuration, ILogger logger)
        {
            var userName = configuration["SeedAdmin:UserName"];
            var password = configuration["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                return;

            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            var existente = await userManager.FindByNameAsync(userName);
            if (existente != null)
            {
                // Ya existe de un deploy anterior: solo nos aseguramos de que tenga el rol mas alto
                // (por si se creo antes de que existiera SuperAdministrador).
                if (!await userManager.IsInRoleAsync(existente, "SuperAdministrador"))
                {
                    await userManager.AddToRoleAsync(existente, "SuperAdministrador");
                }

                return;
            }

            var admin = new ApplicationUser
            {
                UserName = userName,
                NombreCompleto = "Administrador",
                Activo = true,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(admin, password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "SuperAdministrador");
            }
            else
            {
                logger.LogWarning("No se pudo crear el SeedAdmin '{UserName}': {Errores}",
                    userName, string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }

        // Siembra de administradores adicionales (además del de SeedAdmin arriba). Igual que ese,
        // las credenciales NUNCA viven en appsettings.json: se cargan con
        // `dotnet user-secrets set "SeedAdmins:0:UserName" ...` en desarrollo, o como variables de
        // entorno (SeedAdmins__0__UserName, etc.) en el hosting real. Si no hay ninguno configurado, no hace nada.
        private static async Task SeedAdminsAsync(System.IServiceProvider services, IConfiguration configuration, ILogger logger)
        {
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            foreach (var seccion in configuration.GetSection("SeedAdmins").GetChildren())
            {
                var userName = seccion["UserName"];
                var nombreCompleto = seccion["NombreCompleto"];
                var password = seccion["Password"];

                if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
                    continue;

                if (await userManager.FindByNameAsync(userName) != null)
                    continue;

                var admin = new ApplicationUser
                {
                    UserName = userName,
                    NombreCompleto = string.IsNullOrWhiteSpace(nombreCompleto) ? userName : nombreCompleto,
                    Activo = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Administrador");
                }
                else
                {
                    logger.LogWarning("No se pudo crear el SeedAdmin adicional '{UserName}': {Errores}",
                        userName, string.Join("; ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        // Catálogo de tareas y estudios tomado directamente del relevamiento (docs/requerimientos-funcionales.md,
        // Módulo 8). Solo se siembra si la tabla está vacía, para no pisar nada si el equipo ya lo editó.
        private static async Task SeedCatalogosAsync(NexoDbContext db)
        {
            if (!await db.TareasCatalogo.AnyAsync())
            {
                db.TareasCatalogo.AddRange(
                    new TareaCatalogo { Nombre = "Grabación de voces", TipoTrabajo = TipoTrabajo.Grabacion },
                    new TareaCatalogo { Nombre = "Grabación de batería", TipoTrabajo = TipoTrabajo.Grabacion },
                    new TareaCatalogo { Nombre = "Grabación de guitarras", TipoTrabajo = TipoTrabajo.Grabacion },
                    new TareaCatalogo { Nombre = "Grabación de bajos", TipoTrabajo = TipoTrabajo.Grabacion },
                    new TareaCatalogo { Nombre = "Grabación de teclados", TipoTrabajo = TipoTrabajo.Grabacion },
                    new TareaCatalogo { Nombre = "Grabación de acordeón", TipoTrabajo = TipoTrabajo.Grabacion },
                    new TareaCatalogo { Nombre = "Grabación de bandoneón", TipoTrabajo = TipoTrabajo.Grabacion },
                    new TareaCatalogo { Nombre = "Grabación de percusión", TipoTrabajo = TipoTrabajo.Grabacion },
                    new TareaCatalogo { Nombre = "Corrección de afinación", TipoTrabajo = TipoTrabajo.Edicion },
                    new TareaCatalogo { Nombre = "Edición multimedia", TipoTrabajo = TipoTrabajo.Edicion },
                    new TareaCatalogo { Nombre = "Edición de voces", TipoTrabajo = TipoTrabajo.Edicion },
                    new TareaCatalogo { Nombre = "Edición de batería", TipoTrabajo = TipoTrabajo.Edicion },
                    new TareaCatalogo { Nombre = "Edición de acordeón", TipoTrabajo = TipoTrabajo.Edicion },
                    new TareaCatalogo { Nombre = "Edición de bandoneón", TipoTrabajo = TipoTrabajo.Edicion },
                    new TareaCatalogo { Nombre = "Filmación", TipoTrabajo = TipoTrabajo.Grabacion },
                    new TareaCatalogo { Nombre = "Mezcla tema 1", TipoTrabajo = TipoTrabajo.Mezcla },
                    new TareaCatalogo { Nombre = "Mezcla tema 2", TipoTrabajo = TipoTrabajo.Mezcla },
                    new TareaCatalogo { Nombre = "Exportación de stems", TipoTrabajo = TipoTrabajo.Mezcla },
                    new TareaCatalogo { Nombre = "Mastering", TipoTrabajo = TipoTrabajo.Mastering },
                    new TareaCatalogo { Nombre = "Revisión con cliente", TipoTrabajo = TipoTrabajo.Otro },
                    new TareaCatalogo { Nombre = "Cambios solicitados por cliente", TipoTrabajo = TipoTrabajo.Otro },
                    new TareaCatalogo { Nombre = "Backup del proyecto", TipoTrabajo = TipoTrabajo.Otro },
                    new TareaCatalogo { Nombre = "Otro", TipoTrabajo = TipoTrabajo.Otro }
                );
            }

            // Altas puntuales (2026-08-01): el estudio también cotiza ensayos y clases de música,
            // que no estaban en la siembra original. Se agregan solo si todavía no existen, para
            // no reinsertar ni pisar nada en bases que ya tenían el catálogo cargado.
            var nombresExistentes = await db.TareasCatalogo.Select(t => t.Nombre).ToListAsync();
            var tareasNuevas = new[]
            {
                new TareaCatalogo { Nombre = "Ensayo", TipoTrabajo = TipoTrabajo.Ensayo },
                new TareaCatalogo { Nombre = "Clases de música", TipoTrabajo = TipoTrabajo.Clases }
            };

            foreach (var tarea in tareasNuevas)
            {
                if (!nombresExistentes.Contains(tarea.Nombre))
                {
                    db.TareasCatalogo.Add(tarea);
                }
            }

            if (!await db.Estudios.AnyAsync())
            {
                db.Estudios.Add(new Estudio { Nombre = "Estudio principal", EsLocacionExterna = false });
            }

            await db.SaveChangesAsync();
        }
    }
}
