# 🛡 GuardTool

## ASP.NET Production & Security Scanner
### Catch production-breaking misconfigurations before they ship.

***GuardTool is a developer-first CLI tool and dashboard that analyzes ASP.NET Core projects for real-world production and security risks — not just code smells.***

***Unlike generic static analyzers, GuardTool focuses on misconfiguration, middleware order, security hardening, and deployment risks.***

🚀 Why GuardTool?

Your ASP.NET project compiles.
Your tests pass.

But is it production-safe?

## GuardTool detects:

**🔓 Hardcoded secrets**

**🌍 Dangerous CORS configurations**

**🔐 JWT validation**

**🚫 Missing HTTPS redirection**

**📄 Swagger exposed in production**

**⚠ Missing rate limiting**

**🧨 EF Core raw SQL risks**

**🧱 Middleware ordering problems**

**📦 Missing production configs**

**🛑 Debug logging in production**

and many more...

✨ Features
🔍 Production-First Rules

### Focused on real ASP.NET production misconfiguration risks.

## 📊 Risk Scoring

Each scan generates:

Grade (A–F)

Score (0–100)

Severity breakdown

Status summary

## 📄 Multiple Output Formats

JSON

HTML

SARIF (CI/CD integration)

## 📈 Dashboard

Interactive web dashboard:

View historical reports

Download latest HTML / JSON / SARIF

Export ZIP bundles

Risk overview

## 🚦 CI/CD Ready

Fail builds automatically:

--fail-on=critical
--fail-on=high
## 🧠 Baseline Support

Track only new critical issues over time.

## 📦 Installation
Install as a .NET Tool
dotnet tool install GuardTool.Tool --add-source <path-to-nupkg>

Or update:

dotnet tool update GuardTool.Tool --add-source <path-to-nupkg>

## 🧪 Usage
🔍 Scan a project
guardtool scan --root .
With HTML + SARIF
guardtool scan --root . --html --sarif
Fail build if critical found
guardtool scan --root . --fail-on=critical
Custom output directory
guardtool scan --root . --out reports --html --sarif
📊 Open Dashboard
guardtool dashboard --root .

Optional:

guardtool dashboard --root . --out reports --port 5180

Dashboard will:

Launch local web server

Open browser automatically

Display reports

🗂 Report Structure

By default reports are stored in:

<root>/.guardtool/reports/

Each scan generates:

yyyyMMdd_HHmmss_report.json
yyyyMMdd_HHmmss_report.html
yyyyMMdd_HHmmss_report.sarif.json

🧱 Project Architecture
GuardTool.Core        → Rules engine, scanning logic
GuardTool.Cli         → .NET tool entry point
GuardTool.Dashboard   → Razor Pages dashboard
🛠 Example CI Integration
GitHub Actions
- name: Run GuardTool
  run: guardtool scan --root . --fail-on=critical
  
📌 Command Reference
scan
guardtool scan <rootPath>
    | --root <rootPath>
    [--out <dir>]
    [--only-new-critical]
    [--html]
    [--sarif]
    [--fail-on=critical|high|none]
baseline
guardtool baseline <rootPath> [--include-high]
dashboard
guardtool dashboard <rootPath>
    | --root <rootPath>
    [--out <dir>]
    [--port=5180]
    
## 🟢 Community Edition

The current version includes:

Core production readiness rules

Security misconfiguration checks

Basic dashboard

JSON / HTML / SARIF export

Baseline tracking

## 🔵 Pro Edition (Planned)

Advanced JWT validation analysis

Deep EF Core query inspection

Secret entropy scanning

Dependency vulnerability scanning

Historical trend analytics

Team dashboard

PR comment bot

Slack / Teams alerts

Policy configuration files

## 🎯 Roadmap

 Pro licensing system

 SaaS dashboard option

 GitHub App integration

 Azure DevOps extension

 Plugin rule system

 Custom rule authoring SDK

## 🛡 Philosophy

GuardTool is built on one principle:

“Production-ready by default.”

We believe backend security and production safety should be:

Lightweight

Developer-first

CI-friendly

Focused on real risks

📄 License

MIT (Community Edition)

🤝 Contributing

PRs are welcome.

If you'd like to:

Add new rules

Improve false-positive detection

Enhance dashboard UI

Improve performance

Open an issue first to discuss.

🌍 Future Vision

GuardTool aims to become:

The production-readiness standard for ASP.NET applications.
