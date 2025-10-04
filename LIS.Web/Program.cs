using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

//تفعيل صلاحيات الوصول عالميه
builder.Services.AddControllersWithViews(options =>
{
    //فرض تسجيل دخول اجباري لكل المستخدمين
    options.Filters.Add(new AuthorizeFilter());
});



builder.Services.AddHttpClient();
// تفعيل الجلسات
builder.Services.AddSession();

// تفعيل المصادقة باستخدام الكوكيز
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.LoginPath = "/Account/Login";               // صفحة تسجيل الدخول
        options.AccessDeniedPath = "/Account/AccessDenied"; // صفحة رفض الدخول هذ الصفحة ينتقل لها اذا حصل ولم يتم قبول المستخدم اول قام السمتخدم بادخال بيانات خاطئة
    });

builder.Services.AddAuthorization();

var app = builder.Build();
//تفعيل صلاحيات الوصول للصفحات مثلا لا يقدر اي مستخدم الوصول الى اي صفحة اذا لم يسجل الدخول
app.Use(async (context, next) =>
{
    context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
    context.Response.Headers["Pragma"] = "no-cache";
    context.Response.Headers["Expires"] = "0";
    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();           // تفعيل الجلسات
app.UseAuthentication();    // تفعيل المصادقة
app.UseAuthorization();     // تفعيل الصلاحيات

// تحديد المسار الابتدائي اقلاع التطبيق
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
//مسوله عن تشغيل التطبيق
app.Run();
