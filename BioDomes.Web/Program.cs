using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures;
using BioDomes.Infrastructures.EntityFramework.Entities;
using BioDomes.Infrastructures.Files;
using BioDomes.Infrastructures.Identity;
using BioDomes.Infrastructures.Repositories;
using BioDomes.Infrastructures.SpeciesMapper;
using BioDomes.Web.Middlewares;
using BioDomes.Web.Routing;
using BioDomes.Web.Services;
using BioDomes.Web.Transformers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<ReverseProxyLinksMiddleware>();
builder.Services.AddScoped<ISpeciesRepository, EfSpeciesRepository>();
builder.Services.AddScoped<ISpeciesMapper, SpeciesMapper>();
builder.Services.AddScoped<IUserResolver, UserResolver>();
builder.Services.AddScoped<ISpeciesSlugService, SpeciesSlugService>();
builder.Services.AddScoped<ISpeciesImageStorage, SpeciesImageStorage>();
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
});

builder.Services.AddDbContext<BioDomesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BioDomesDb")));

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

builder.Services
    .AddDefaultIdentity<UserEntity>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireNonAlphanumeric = false;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<BioDomesDbContext>();

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

app.UseForwardedHeaders(new ForwardedHeadersOptions());
app.UseHttpsRedirection();
app.UseReverseProxyLinks();
app.UseWebOptimizer();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
