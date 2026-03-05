using PTabuF2.Data; // Senin SqlHelper burada

var builder = WebApplication.CreateBuilder(args);

// 1. MVC Servislerini Ekle
builder.Services.AddControllersWithViews();

// 2. SESSION SERVÝSÝNÝ EKLE (Unutulan Kýsým Burasýydý)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 30 dk iþlem yapmazsa oturum kapansýn
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3. SqlHelper'ý Tanýt
builder.Services.AddScoped<SqlHelper>();

var app = builder.Build();

// Hata ayýklama modlarý
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 4. SESSION'I AKTÝF ET (Bu da Unutulmuþ Olabilir)
app.UseSession(); // <--- KRÝTÝK NOKTA

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();