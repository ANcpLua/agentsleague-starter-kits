---
name: "Scenario: Deliver Insights to Managers"
description: |
  HR professionals can use AI to deliver feedback to managers on employee
  performance and share best practices. Combines data analysis with
  stakeholder communication.
track: scenario
persona: "HR Professional"
industry: "HR"
source: Microsoft-Copilot-scenario-for-HR_Deliver-insights-to-managers.pptx

imports:
  - shared/tools/copilot-chat.md
  - shared/tools/copilot-outlook.md
  - shared/tools/copilot-teams.md
  - shared/tools/copilot-loop.md
  - shared/tools/copilot-studio.md
  - shared/tools/analyst.md
  - shared/patterns/data-analysis.md
  - shared/patterns/stakeholder-communication.md

tools:
  - copilot-chat
  - copilot-outlook
  - copilot-teams
  - copilot-loop
  - copilot-studio
  - analyst

steps: 6
---

# Scenario — HR: Deliver Insights to Managers

HR professionals can use AI to deliver feedback to managers on employee performance and share best practices.

---

## 1. Identifying Key Metrics

Identify key metrics which can provide insights into manager capabilities and opportunities. Use Copilot to access HR
system data using a Copilot Studio agent.

**Tool:** Microsoft Copilot Studio — AI Agent

**Action:** Use Copilot to rapidly locate relevant manager metrics (employee survey, attrition, training consumption)
which impact team health.

---

## 2. Gather Data Insights

Analyze data/metrics, summarize findings, provide insights, and create potential manager opportunities.

**Tool:** Analyst

**Example Prompt:**
> Produce a report for each manager showing strengths and weaknesses based on the metrics.

---

## 3. Identify Best Practices

Based on the data/insights, have Copilot locate best practices for managers to follow to improve upon the key metrics.

**Tool:** Microsoft 365 Copilot Chat

**Example Prompt:**
> I am a human resources consultant. Create a planning document based on the manager report and best in class manager
> practices.

---

## 4. Communicate Opportunities

Draft and send email to managers sharing the data insights and best practices with them for consideration.

**Tool:** Copilot in Outlook

**Example Prompt:**
> Draft with Copilot: an email to managers sharing opportunities based on best practices and their manager insights.
> Tone is Direct and Length is Medium.

---

## 5. Align and Track

Conduct a meeting to present the opportunities, align and track the action items, referencing insights and notes from
Copilot in Teams.

**Tool:** Copilot in Teams

**Action:** Use Copilot during the meeting to "list main ideas we discussed" and then review the AI notes "Follow-up
tasks" after the meeting to finalize the plan and track.

---

## 6. Adjust and Iterate

Based on the impact of the actions, adjust approach and continue to iterate using Copilot in Loop.

**Tool:** Copilot in Loop

**Example Prompt:**
> Brainstorm activities to empower and improve effectiveness of people managers in an organization. Invite colleagues to
> iterate on ideas.
