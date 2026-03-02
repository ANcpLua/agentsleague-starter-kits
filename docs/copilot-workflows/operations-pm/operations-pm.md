---
name: "Day in the Life: Operations PM"
description: |
  Cassandra is an Operations PM who uses Microsoft 365 Copilot throughout her
  day for email triage, meeting preparation, meeting recaps, data analysis,
  brainstorming, and presentation creation.
track: day-in-the-life
persona: "Operations PM"
persona-name: "Cassandra"
industry: "Operations"
source: Microsoft-Copilot-day-in-the-life_Operations-PM.pptx

imports:
  - shared/tools/copilot-chat.md
  - shared/tools/copilot-teams.md
  - shared/tools/copilot-excel.md
  - shared/tools/copilot-whiteboard.md
  - shared/tools/copilot-powerpoint.md
  - shared/patterns/email-summarization.md
  - shared/patterns/meeting-recap.md
  - shared/patterns/data-analysis.md

tools:
  - copilot-chat
  - copilot-teams
  - copilot-excel
  - copilot-whiteboard
  - copilot-powerpoint

schedule:
  - time: "8:00 am"
    action: "Catch Up"
    tool: copilot-chat
    pattern: email-summarization
  - time: "8:15 am"
    action: "Prepare for a Meeting"
    tool: copilot-chat
  - time: "9:00 am"
    action: "Recap a Meeting"
    tool: copilot-teams
    pattern: meeting-recap
  - time: "11:00 am"
    action: "Analyze Data"
    tool: copilot-excel
    pattern: data-analysis
  - time: "2:00 pm"
    action: "Get Creative"
    tool: copilot-whiteboard
  - time: "4:00 pm"
    action: "Create a Presentation"
    tool: copilot-powerpoint
---
# Day in the Life — Operations PM

*Persona: Cassandra, Operations PM*

---

## 8:00 am — Catch Up

Cassandra uses Copilot to summarize her emails and Teams messages from overnight and draft responses to urgent items leveraging the drafting feature.

**Tool:** Microsoft 365 Copilot Chat

**Sample Prompt:**
> Summarize all the emails and Teams chats in the past day, highlighting the primary asks and open items.

---

## 8:15 am — Prepare for a Meeting

To make sure she is ready for her meeting, she uses Copilot to search for information on the project looking across past emails, meetings, files, internal and external articles.

**Tool:** Microsoft 365 Copilot Chat

**Sample Prompt:**
> Find information on [project x].

---

## 9:00 am — Recap a Meeting

To focus on the meeting instead of taking notes, Cassandra uses the transcribe feature during the project team meeting to capture a summary of the conversation and key actions.

**Tool:** Copilot in Teams

**Sample Prompt:**
> Summarize key discussion points. Identify agreed-upon next steps.

---

## 11:00 am — Analyze Data

Following the meeting, she utilizes Agent Mode in Excel to review and analyze the project data and generate reports. She shares key insights and meeting actions to all the attendees.

**Tool:** Copilot in Excel

**Sample Prompt:**
> Suggest formulas for this column. Show insights in charts.

---

## 2:00 pm — Get Creative

Cassandra joins her colleagues to brainstorm the change management and readiness plan for an upcoming project using Copilot in Whiteboard to capture key project needs.

**Tool:** Copilot in Whiteboard

**Sample Prompt:**
> Organize thoughts into logical categories and simplify complex projects for better collaboration.

---

## 4:00 pm — Create a Presentation

Cassandra prepares a PowerPoint presentation on the readiness plan, using Office Agent to generate the content based on the whiteboarding session.

**Tool:** Copilot in PowerPoint

**Sample Prompt:**
> Create a new presentation from this Word file. Change the layout of this slide.
