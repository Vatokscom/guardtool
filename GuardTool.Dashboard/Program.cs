using GuardTool.Core;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();

// ✅ config’i build’den önce oku
var configuredRoot = builder.Configuration["GUARDTOOL_ROOT"];
var configuredOut  = builder.Configuration["GUARDTOOL_OUTDIR"]; // optional

var app = builder.Build();
app.UseStaticFiles();
app.MapRazorPages();

// ✅ root çöz
var root = string.IsNullOrWhiteSpace(configuredRoot)
    ? Directory.GetCurrentDirectory()
    : Path.GetFullPath(configuredRoot);

// ✅ tek yerden rapor dizini
string ReportsDirResolved() =>
    string.IsNullOrWhiteSpace(configuredOut)
        ? ReportWriters.ReportsDir(root)
        : ReportWriters.ResolveOutDir(root, configuredOut);

// ✅ rapor var mı?
bool HasAnyReports()
{
    try
    {
        var dir = ReportsDirResolved();
        return Directory.Exists(dir) && Directory.EnumerateFiles(dir, "*_report.json").Any();
    }
    catch { return false; }
}

// ✅ Guard: rapor yoksa Reports/Downloads/ReportFiles kilit
app.Use(async (ctxHttp, next) =>
{
    var path = ctxHttp.Request.Path;

    if (!HasAnyReports() &&
        (path.StartsWithSegments("/Reports", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWithSegments("/download", StringComparison.OrdinalIgnoreCase) ||
         path.StartsWithSegments("/reports", StringComparison.OrdinalIgnoreCase)))
    {
        ctxHttp.Response.Redirect("/?noreports=1");
        return;
    }

    await next();
});

app.MapGet("/api/reports", () =>
{
    var dir = ReportsDirResolved();
    if (!Directory.Exists(dir)) return Results.Ok(Array.Empty<object>());

    var files = Directory.GetFiles(dir, "*_report.json")
        .OrderByDescending(f => f)
        .Take(200)
        .ToArray();

    var list = new List<object>();
    foreach (var f in files)
    {
        try
        {
            var rpt = System.Text.Json.JsonSerializer.Deserialize<Report>(System.IO.File.ReadAllText(f), JsonConfig.Options);
            if (rpt is null) continue;

            var name = Path.GetFileName(f);
            var sarifName = name.Replace("_report.json", "_report.sarif.json", StringComparison.OrdinalIgnoreCase);
            var sarifPath = Path.Combine(dir, sarifName);

            list.Add(new
            {
                name,
                timestamp = rpt.TimestampUtc,
                grade = rpt.Grade,
                score = rpt.Score,
                status = rpt.Status,
                critical = rpt.Critical,
                high = rpt.High,
                medium = rpt.Medium,
                low = rpt.Low,
                newCritical = rpt.NewCritical,
                hasSarif = System.IO.File.Exists(sarifPath)
            });
        }
        catch { }
    }

    return Results.Ok(list);
});

app.MapGet("/reports/{file}", (string file) =>
{
    var dir = ReportsDirResolved();
    var path = Path.Combine(dir, file);
    if (!System.IO.File.Exists(path)) return Results.NotFound();

    var ext = Path.GetExtension(path).ToLowerInvariant();
    var ct = ext switch
    {
        ".html" => "text/html; charset=utf-8",
        ".json" => "application/json; charset=utf-8",
        _ => "text/plain; charset=utf-8"
    };

    return Results.File(path, ct);
});

static string? FindLatestReportJson(string reportsDir)
{
    if (!Directory.Exists(reportsDir)) return null;

    return Directory.GetFiles(reportsDir, "*_report.json")
        .OrderByDescending(f => f)
        .FirstOrDefault();
}

app.MapGet("/download/latest/json", () =>
{
    var dir = ReportsDirResolved();
    var latest = FindLatestReportJson(dir);
    if (latest is null) return Results.NotFound();

    return Results.File(latest, "application/json; charset=utf-8", Path.GetFileName(latest));
});

app.MapGet("/download/latest/html", () =>
{
    var dir = ReportsDirResolved();
    var latest = FindLatestReportJson(dir);
    if (latest is null) return Results.NotFound();

    var html = latest.Replace("_report.json", "_report.html", StringComparison.OrdinalIgnoreCase);
    if (!System.IO.File.Exists(html)) return Results.NotFound();

    return Results.File(html, "text/html; charset=utf-8", Path.GetFileName(html));
});

app.MapGet("/download/latest/sarif", () =>
{
    var dir = ReportsDirResolved();
    var latest = FindLatestReportJson(dir);
    if (latest is null) return Results.NotFound();

    var sarif = latest.Replace("_report.json", "_report.sarif.json", StringComparison.OrdinalIgnoreCase);
    if (!System.IO.File.Exists(sarif)) return Results.NotFound();

    return Results.File(sarif, "application/json; charset=utf-8", Path.GetFileName(sarif));
});

app.MapGet("/download/reports.zip", ([FromQuery] int? take) =>
{
    var dir = ReportsDirResolved();
    if (!Directory.Exists(dir)) return Results.NotFound();

    int n = take.GetValueOrDefault(200);
    if (n < 1) n = 1;
    if (n > 2000) n = 2000;

    var files = Directory.GetFiles(dir, "*_report.json")
        .OrderByDescending(f => f)
        .Take(n)
        .ToArray();

    var tmp = Path.Combine(Path.GetTempPath(), $"guardtool_reports_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip");

    try
    {
        if (System.IO.File.Exists(tmp)) System.IO.File.Delete(tmp);

        using (var zip = ZipFile.Open(tmp, ZipArchiveMode.Create))
        {
            foreach (var json in files)
            {
                var html = json.Replace("_report.json", "_report.html", StringComparison.OrdinalIgnoreCase);
                var sarif = json.Replace("_report.json", "_report.sarif.json", StringComparison.OrdinalIgnoreCase);

                if (System.IO.File.Exists(json)) zip.CreateEntryFromFile(json, Path.GetFileName(json), CompressionLevel.Fastest);
                if (System.IO.File.Exists(html)) zip.CreateEntryFromFile(html, Path.GetFileName(html), CompressionLevel.Fastest);
                if (System.IO.File.Exists(sarif)) zip.CreateEntryFromFile(sarif, Path.GetFileName(sarif), CompressionLevel.Fastest);
            }
        }

        return Results.File(tmp, "application/zip", $"GuardTool.Reports.latest{n}.zip");
    }
    catch
    {
        return Results.Problem("Failed to create zip.");
    }
});

app.Run();