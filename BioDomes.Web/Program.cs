using BioDomes.Infrastructures;
using BioDomes.Web.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddTransient<ReverseProxyLinksMiddleware>();
builder.Services.AddSingleton<ISpeciesRepository, InMemorySpeciesRepository>();
builder.Services.AddWebOptimizer(pipeline =>
{
    pipeline.AddCssBundle("/css/bundle.css",
        "/css/paragraph-custom-style.css",
        "site.css",
        "title-custom-style.css");
});

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