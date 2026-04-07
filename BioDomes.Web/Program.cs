using BioDomes.Domains.Repositories;
using BioDomes.Infrastructures;
using BioDomes.Infrastructures.EntityFramework;
using BioDomes.Infrastructures.Files;
using BioDomes.Infrastructures.Repositories;
using BioDomes.Web.Middlewares;
using BioDomes.Web.Routing;
using BioDomes.Web.Transformers;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddTransient<ReverseProxyLinksMiddleware>();
builder.Services.AddScoped<ISpeciesRepository, EfSpeciesRepository>();
builder.Services.AddScoped<ISpeciesImageStorage, SpeciesImageStorage>();
builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css",
        "site.css");
});
builder.Services.Configure<RouteOptions>(o =>
{
    o.ConstraintMap["speciesSlug"] = typeof(SpeciesSlugConstraint); 
    o.ConstraintMap["kebab"] = typeof(SlugTransformer);         
});

// Création du dossier Data dans BioDomes.Web
var dataFolder = Path.Combine(builder.Environment.ContentRootPath, "Data");
Directory.CreateDirectory(dataFolder);

// Chemin absolu vers la base SQLite
var dbPath = Path.Combine(dataFolder, "BioDomes.sqlite");

builder.Services.AddDbContext<BioDomesDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
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
app.UseAuthorization();
app.MapRazorPages();

app.Run();