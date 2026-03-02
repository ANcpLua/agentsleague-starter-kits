---
name: "Day in the Life: Business Manager"
description: |
  Daichi is a Business Manager in a global team. This workflow follows his
  typical day using Microsoft 365 Copilot tools to manage email triage,
  meeting preparation, meeting recaps, research, and stakeholder communication.
track: day-in-the-life
persona: "Business Manager"
persona-name: "Daichi"
industry: "General"
source: Microsoft-Copilot-day-in-the-life_Business-Manager.pptx

imports:
  - shared/tools/copilot-chat.md
  - shared/tools/copilot-teams.md
  - shared/tools/copilot-outlook.md
  - shared/patterns/email-summarization.md
  - shared/patterns/meeting-recap.md
  - shared/patterns/research-synthesis.md
  - shared/patterns/document-creation.md
  - shared/patterns/stakeholder-communication.md

tools:
  - copilot-chat
  - copilot-teams
  - copilot-outlook

schedule:
  - time: "8:00 am"
    action: "Catch Up"
    tool: copilot-chat
    pattern: email-summarization
  - time: "8:15 am"
    action: "Analyze Agenda"
    tool: copilot-chat
  - time: "9:00 am"
    action: "Prepare for a Meeting"
    tool: copilot-chat
    pattern: research-synthesis
  - time: "11:00 am"
    action: "Recap a Meeting"
    tool: copilot-teams
    pattern: meeting-recap
  - time: "2:00 pm"
    action: "Conduct Research"
    tool: copilot-chat
    pattern: research-synthesis
  - time: "4:00 pm"
    action: "Draft an Email"
    tool: copilot-outlook
    pattern: stakeholder-communication
---
# Day in the Life — Business Manager

*Persona: Daichi, Business Manager in a global team*

---

## 8:00 am — Catch Up

Daichi works within a global team. He catches up on activity from overnight, specifically urgent items from stakeholders. He uses Copilot to summarize emails and alert him to any actions.

**Tool:** Microsoft 365 Copilot Chat

**Action:**
> Summarize this thread calling out where my name was mentioned and any action items for me.

---

## 8:15 am — Analyze Agenda

He optimizes his calendar for the week ahead. He asks Copilot to categorize his meetings. This quickly allows him to see if his time allocation aligns with this week's priorities and adjust if needed.

**Tool:** Microsoft 365 Copilot Chat

**Sample Prompt:**
> Review upcoming calendar events and make sure they align with your core priorities.

---

## 9:00 am — Prepare for a Meeting

Daichi prepares for his first meeting. He asks Copilot to summarize the document attached to the invite and look across past emails, meetings, and files.

**Tool:** Microsoft 365 Copilot Chat

**Action:**
> Summarize documents and past activities to fully prepare for upcoming meetings.

---

## 11:00 am — Recap a Meeting

Daichi joins the meeting via Teams. After the meeting he uses Copilot in Teams to recap, identify risks, and define next steps. He then uses Copilot in Outlook to draft a follow-up email to the attendees.

**Tool:** Copilot in Teams

**Action:**
> Summarize this meeting and provide the key points and action items.

---

## 2:00 pm — Conduct Research

Daichi puts together a briefing document based on customer and project information. He uses Copilot to research the most recent presentations by the organization CEO and cross-reference with competitors.

**Tool:** Microsoft 365 Copilot Chat

> Identifies key data points across multiple documents, external web content, and financial records quickly and accurately.

---

## 4:00 pm — Draft an Email

Daichi responds to emails and Teams messages using Copilot in Outlook to help him draft content tailored to a specified tone, length, and sentiment.

**Tool:** Copilot in Outlook

> Automates drafting of emails and uses Copilot to revise the tone based on the customer situation.
