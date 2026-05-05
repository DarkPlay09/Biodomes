using BioDomes.Domains.Repositories;
using BioDomes.Domains.Services;
using BioDomes.Infrastructures;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.Files;
using BioDomes.Infrastructures.Identity;
using BioDomes.Infrastructures.Mappers.Biome;
using BioDomes.Infrastructures.Mappers.Equipment;
using BioDomes.Infrastructures.Mappers.Species;
using BioDomes.Infrastructures.Repositories;
using BioDomes.Infrastructures.Services.Identity;
using BioDomes.Infrastructures.Services.Slug;
using BioDomes.Web.Areas.Identity;
using BioDomes.Web.Middlewares;
using BioDomes.Web.Routing;
using BioDomes.Web.Services;
using BioDomes.Web.Services.Stats;
using BioDomes.Web.Transformers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<ReverseProxyLinksMiddleware>();
builder.Services.AddScoped<ISpeciesRepository, EfSpeciesRepository>();
builder.Services.AddScoped<IEquipmentRepository, EfEquipmentRepository>();
builder.Services.AddScoped<IBiomeRepository, EfBiomeRepository>();
builder.Services.AddScoped<ISpeciesMapper, SpeciesMapper>();
builder.Services.AddScoped<IEquipmentMapper, EquipmentMapper>();
builder.Services.AddScoped<IBiomeMapper, BiomeMapper>();
builder.Services.AddScoped<IBiomeSpeciesFoodSnapshotMapper, BiomeSpeciesFoodSnapshotMapper>();
builder.Services.AddScoped<IUserResolver, UserResolver>();
builder.Services.AddScoped<ISlugService, SlugService>();
builder.Services.AddScoped<ISpeciesImageStorage, SpeciesImageStorage>();
builder.Services.AddScoped<IEquipmentImageStorage, EquipmentImageStorage>();
builder.Services.AddScoped<IStatsDashboardService, StatsDashboardService>();
builder.Services.AddScoped<IBiomeStabilityService, BiomeStabilityService>();
builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css",
        "site.css"); // TODO : inutile de faire un bundle pour un seul fichier
});

builder.Services.Configure<RouteOptions>(o =>
{
    o.ConstraintMap["speciesSlug"] = typeof(SpeciesSlugConstraint); 
    o.ConstraintMap["kebab"] = typeof(SlugTransformer);         
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Dashboard");
    options.Conventions.AuthorizeFolder("/Species");
    options.Conventions.AuthorizeFolder("/Equipment");
    options.Conventions.AuthorizeFolder("/Biome");
});

builder.Services.AddDbContext<BioDomesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BioDomesDb")));

builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("Smtp"));

builder.Services.AddTransient<SmtpEmailSender>();

builder.Services.AddTransient<IEmailSender>(serviceProvider =>
    serviceProvider.GetRequiredService<SmtpEmailSender>());

builder.Services
    .AddDefaultIdentity<UserEntity>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequiredUniqueChars = 4;
        options.User.RequireUniqueEmail = true;
    })
    .AddErrorDescriber<FrenchIdentityErrorDescriber>()
    .AddEntityFrameworkStores<BioDomesDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Error/403";
});

var app = builder.Build();

await IdentityDataSeeder.SeedAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseExceptionHandler("/Error");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.UseForwardedHeaders(new ForwardedHeadersOptions());
app.UseHttpsRedirection();
app.UseReverseProxyLinks();
app.UseWebOptimizer();
app.UseStaticFiles();

var uploadsPath = Path.Combine(builder.Environment.WebRootPath, "uploads");

Directory.CreateDirectory(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});


app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
