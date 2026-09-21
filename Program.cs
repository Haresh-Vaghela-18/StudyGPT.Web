using StudyGPT.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSession();
builder.Services.AddScoped<JwtService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();


// =========================================================
// PREVENT BROWSER CACHING OF PAGES
// =========================================================

app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["Cache-Control"] =
            "no-store, no-cache, must-revalidate";

        context.Response.Headers["Pragma"] = "no-cache";

        context.Response.Headers["Expires"] = "0";

        return Task.CompletedTask;
    });

    await next();
});


app.UseRouting();

app.UseSession();

app.UseAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();