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

builder.Services.AddDbContext<BioDomesDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BioDomesDb")));

var app = builder.Build();

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

app.UseAuthorization();

app.MapRazorPages();

app.Run();
