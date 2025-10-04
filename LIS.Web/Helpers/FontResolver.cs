using PdfSharp.Fonts;
using System.IO;

public class ProjectFontResolver : IFontResolver
{
    public byte[] GetFont(string faceName)
    {
        // المسار الافتراضي للمشروع
        var path = Path.Combine(Directory.GetCurrentDirectory(), "fonts", "arial.ttf");
        return File.ReadAllBytes(path);
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        // جميع الأشكال تستخدم نفس الخط 
        return new FontResolverInfo("Arial#");
    }
}
