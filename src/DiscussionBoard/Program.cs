using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DiscussionBoard.Data;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Serilog;
using Serilog.Formatting.Compact;
using DiscussionBoard.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace DiscussionBoard
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            try
            {
                 var builder = WebApplication.CreateBuilder(args);

            // Get the location of the Logs folder for SeriLog
            string logPath = Path.Combine(builder.Environment.ContentRootPath, "Logs");
            string logFilePath = Path.Combine(logPath, "discussionboard-.json");

            builder.Configuration["Serilog:WriteTo:1:Args:configure:0:Args:path"] = logFilePath;

            builder.Services.AddSerilog((service, lc) => lc
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(service));

            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.LoginPath = "/Identity/Account/Login";
                    options.ReturnUrlParameter = "returnUrl";
                    options.SlidingExpiration = true;
                });

            builder.Services.AddSingleton<IAuthorizationHandler, IsOwnerOrAdminHandler>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(ApplicationAuthorizationPolicies.IsOwnerOrAdmin,
                    policy => policy.Requirements.Add(new IsOwnerOrAdminRequirement()));
            });


            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                                   throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
                    options.SignIn.RequireConfirmedAccount = !builder.Environment.IsDevelopment())
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            builder.Services.AddRazorPages();

            builder.Services.AddSingleton<ILogReader, JsonLogReader>();
            builder.Services.AddScoped<IForumService, ForumService>();
            builder.Services.AddScoped<IDiscussionThreadService, DiscussionThreadService>();
            builder.Services.AddScoped<IPostService, PostService>();
            builder.Services.AddScoped<IUserProfileService, UserProfileService>();
            builder.Services.AddScoped<IApplicationAuthorizationService, ApplicationAuthorizationService>();

            var app = builder.Build();

            // if (args.Contains("--bootstrap-admin", StringComparer.OrdinalIgnoreCase))
            // {
            //     if (!app.Environment.IsDevelopment())
            //     {
            //         throw new InvalidOperationException("This bootstrap tool is Development-only.");
            //     }
            //     await AdminBootstrapper.BootstrapAsync(app.Services,
            //         app.Configuration["BootstrapAdmin:Email"]);
            //     return;
            // }


            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days.
                // You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCsp(options => options
                .DefaultSources(s => s.Self())
                .ScriptSources(s => s.Self().CustomSources("https://cdnjs.cloudflare.com"))
                .StyleSources(s => s.Self().CustomSources("https://fonts.googleapis.com"))
                .FontSources(s => s.Self().CustomSources("https://fonts.gstatic.com"))
                .ImageSources(s => s.Self().CustomSources("data:"))
                .ObjectSources(s => s.None())
                .FrameAncestors(s => s.None())
                .UpgradeInsecureRequests()

            );
            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append(
                    "Content-Security-Policy",
                    "default-src 'self'; frame-ancestors 'none'; form-action 'self';"
                );
                await next();
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();

            app.MapRazorPages()
                .WithStaticAssets();

            app.Run();
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
