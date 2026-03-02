# Copilot Workflow System — Manual

Dieses Verzeichnis enthält Microsoft 365 Copilot Workflow-Szenarien, organisiert als modulares Fragment-System. Workflows beschreiben wie Menschen M365 Copilot-Tools im Arbeitsalltag einsetzen.

## Verzeichnisstruktur

```
MyFunctions/
├── shared/                        # Wiederverwendbare Bausteine
│   ├── tools/                     # M365-Tool-Definitionen (10 Dateien)
│   ├── patterns/                  # Aktionsmuster mit Beispiel-Prompts (6 Dateien)
│   └── mcp/                      # MCP-Server-Konfigurationen (1 Datei)
│
├── adhd/adhd.md                   # Track 1: Day in the Life
├── manager/manager.md             # Track 1: Day in the Life
├── operations-pm/operations-pm.md # Track 1: Day in the Life
├── hr/hr.md                       # Track 2: Scenario
├── it-prompts/it-prompts.md       # Track 2: Scenario
├── it-solution/it-solution.md     # Track 2: Scenario
├── media/media.md                 # Track 2: Scenario
├── it-devices/it-devices.md       # Track 3: Agent
├── project-manager/project-manager.md # Track 3: Agent
└── manual.md                      # Diese Datei
```

Jeder Workflow-Ordner enthält zusaetzlich die Original-PowerPoint-Quelle (`.pptx`).

## Die drei Tracks

| Track | Typ | Struktur | Beispiel |
|-------|-----|----------|----------|
| **1** | Day in the Life | Chronologisch mit Uhrzeiten (8am, 9am, ...) | Daichi, Business Manager |
| **2** | Scenario | Nummerierte Schritte (1. → 2. → ... → 6.) | IT Solution Evaluation |
| **3** | Agent | Copilot Studio Agent-gesteuert | Device Acquisition Bot |

## YAML Frontmatter Schema

Jede Workflow-Datei beginnt mit YAML-Frontmatter zwischen `---` Begrenzern.

### Pflichtfelder (alle Tracks)

```yaml
---
name: "Day in the Life: Business Manager"   # Menschenlesbarer Titel
description: |                               # Was dieser Workflow macht
  Daichi is a Business Manager...
track: day-in-the-life                       # day-in-the-life | scenario | agent
persona: "Business Manager"                  # Zielrolle
source: filename.pptx                        # PowerPoint-Quelle

imports:                                     # Shared Fragments
  - shared/tools/copilot-chat.md
  - shared/patterns/email-summarization.md

tools:                                       # Tool-Slugs (Kurzreferenz)
  - copilot-chat
  - copilot-teams
---
```

### Track-spezifische Felder

**Track 1 — Day in the Life:**

```yaml
persona-name: "Daichi"                       # Name der Persona
schedule:                                    # Tagesablauf
  - time: "8:00 am"
    action: "Catch Up"
    tool: copilot-chat
    pattern: email-summarization             # Optional: verknuepftes Pattern
```

**Track 2 — Scenario:**

```yaml
industry: "IT"                               # Branche
steps: 6                                     # Anzahl der Schritte
```

**Track 3 — Agent:**

```yaml
primary-tool: "Microsoft Copilot Studio"     # Haupt-Agent-Tool
steps: 6
```

## Shared Fragments

### Tools (`shared/tools/`)

Tool-Fragmente definieren ein M365-Produkt mit seinen Faehigkeiten.

| Datei | Tool | Verwendet in |
|-------|------|-------------|
| `copilot-chat.md` | Microsoft 365 Copilot Chat | 8 von 9 Workflows |
| `copilot-teams.md` | Copilot in Teams | 5 von 9 |
| `copilot-outlook.md` | Copilot in Outlook | 3 von 9 |
| `copilot-word.md` | Copilot in Word | 3 von 9 |
| `copilot-excel.md` | Copilot in Excel | 2 von 9 |
| `copilot-studio.md` | Microsoft Copilot Studio | 3 von 9 |
| `copilot-powerpoint.md` | Copilot in PowerPoint | 1 von 9 |
| `copilot-whiteboard.md` | Copilot in Whiteboard | 1 von 9 |
| `copilot-loop.md` | Copilot in Loop | 1 von 9 |
| `analyst.md` | Analyst | 1 von 9 |

**Fragment-Schema:**

```yaml
---
tool:
  name: Microsoft 365 Copilot Chat
  slug: copilot-chat
  category: chat
  m365-product: Microsoft 365 Copilot
  description: >
    General-purpose AI assistant...
  capabilities:
    - email-summarization
    - research
---
```

### Patterns (`shared/patterns/`)

Pattern-Fragmente beschreiben wiederkehrende Aktionen mit Beispiel-Prompts.

| Datei | Pattern | Verwendet in |
|-------|---------|-------------|
| `email-summarization.md` | Email Triage & Summary | adhd, manager, operations-pm |
| `meeting-recap.md` | Teams Meeting Recap | adhd, manager, operations-pm, media |
| `research-synthesis.md` | Research & Info Gathering | manager, media, it-prompts, it-solution |
| `document-creation.md` | Document Drafting | adhd, media, it-solution, manager |
| `data-analysis.md` | Data Analysis & Reporting | hr, it-solution, operations-pm |
| `stakeholder-communication.md` | Stakeholder Emails | hr, manager, it-solution |

**Fragment-Schema:**

```yaml
---
pattern:
  name: Email Summarization
  slug: email-summarization
  description: >
    Summarize and triage email threads...
  typical-tools:
    - copilot-chat
    - copilot-outlook
  used-in:
    - adhd
    - manager
    - operations-pm
---
```

### MCP (`shared/mcp/`)

MCP-Server-Konfigurationen fuer externe API-Anbindungen.

| Datei | Server | API |
|-------|--------|-----|
| `microsoft-docs.md` | Microsoft Learn | `https://learn.microsoft.com/api/mcp` |

## Neuen Workflow erstellen

### 1. Ordner und Datei anlegen

```
MyFunctions/sales/sales.md
```

### 2. Track waehlen und Frontmatter schreiben

```yaml
---
name: "Day in the Life: Sales Engineer"
description: |
  Alex is a Sales Engineer who uses Copilot to prepare for customer calls,
  research competitors, and draft proposals.
track: day-in-the-life
persona: "Sales Engineer"
persona-name: "Alex"
industry: "Sales"
source: Copilot-day-in-the-life_Sales-Engineer.pptx

imports:
  - shared/tools/copilot-chat.md
  - shared/tools/copilot-outlook.md
  - shared/tools/copilot-word.md
  - shared/patterns/email-summarization.md
  - shared/patterns/research-synthesis.md
  - shared/patterns/document-creation.md

tools:
  - copilot-chat
  - copilot-outlook
  - copilot-word

schedule:
  - time: "8:00 am"
    action: "Email Triage"
    tool: copilot-chat
    pattern: email-summarization
  - time: "9:30 am"
    action: "Customer Research"
    tool: copilot-chat
    pattern: research-synthesis
  - time: "2:00 pm"
    action: "Draft Proposal"
    tool: copilot-word
    pattern: document-creation
---
```

### 3. Markdown-Body schreiben

Unter dem schliessenden `---` den Workflow-Inhalt als Markdown dokumentieren.
Bestehende Workflows als Vorlage verwenden:

- Track 1: `manager/manager.md`
- Track 2: `it-solution/it-solution.md`
- Track 3: `it-devices/it-devices.md`

### 4. Checkliste

- [ ] Jeder Eintrag in `tools:` hat einen passenden Import in `imports:`
- [ ] Jedes Pattern in `imports:` existiert in `shared/patterns/`
- [ ] `source:` verweist auf eine vorhandene `.pptx` Datei
- [ ] `track:` ist einer der drei Werte: `day-in-the-life`, `scenario`, `agent`
- [ ] Track 1 hat `schedule:`, Track 2/3 hat `steps:`

## Neues Shared Fragment erstellen

### Neues Tool

Wenn ein Workflow ein M365-Produkt verwendet, das noch kein Fragment hat:

1. Datei in `shared/tools/` erstellen
2. YAML mit `tool:` Block (name, slug, category, m365-product, description, capabilities)
3. Markdown-Body mit Common Actions und Best Practices

### Neues Pattern

Wenn eine Aktion in **mindestens 2 Workflows** vorkommt:

1. Datei in `shared/patterns/` erstellen
2. YAML mit `pattern:` Block (name, slug, description, typical-tools, used-in)
3. Markdown-Body mit Example Prompts und Typical Flow
4. `used-in:` Liste aktuell halten wenn neue Workflows das Pattern importieren

## Design-Prinzipien

**Composition over Inheritance** — Tools und Patterns sind unabhaengig kombinierbar. Ein Workflow importiert genau die Bausteine die er braucht.

**Drei Abstraktionsebenen:**
- `tools/` = **Was** (welches M365-Produkt)
- `patterns/` = **Wie** (welche wiederkehrende Aktion)
- `mcp/` = **Womit** (welche externe API)

**DRY (Don't Repeat Yourself)** — Wenn sich ein Prompt oder eine Tool-Beschreibung aendert, wird sie an einer Stelle (im shared Fragment) aktualisiert und alle importierenden Workflows profitieren automatisch.

**Konvention: Dateiname = Ordnername** — `manager/manager.md`, nicht `manager/Business Manager.md`. Der menschenlesbare Titel steht im YAML `name:` Feld.
