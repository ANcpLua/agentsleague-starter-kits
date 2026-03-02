# Enterprise Support Agent — Combined Knowledge Base

---

# ═══════════════════════════════════════════════════════════════
# FILE: adhd/adhd.md
# ═══════════════════════════════════════════════════════════════

---
name: "Day in the Life: Person with ADHD"
description: |
  Jaclyn works with teams across eight countries and uses Microsoft 365 Copilot
  tools throughout her day to manage email, translations, meetings, document
  search, drafting, and catching up on team communications.
track: day-in-the-life
persona: "Cross-Country Team Member"
persona-name: "Jaclyn"
industry: "General"
source: Microsoft-Copilot-day-in-the-life_Person-who-has-ADHD.pptx

imports:
  - shared/tools/copilot-chat.md
  - shared/tools/copilot-outlook.md
  - shared/tools/copilot-teams.md
  - shared/tools/copilot-word.md
  - shared/patterns/email-summarization.md
  - shared/patterns/meeting-recap.md
  - shared/patterns/document-creation.md

tools:
  - copilot-chat
  - copilot-outlook
  - copilot-teams
  - copilot-word

schedule:
  - time: "9:00 am"
    action: "Summarize email"
    tool: copilot-outlook
    pattern: email-summarization
  - time: "10:15 am"
    action: "Translate text"
    tool: copilot-chat
  - time: "11:00 am"
    action: "Recap a meeting"
    tool: copilot-teams
    pattern: meeting-recap
  - time: "1:30 pm"
    action: "Find documents"
    tool: copilot-chat
  - time: "2:00 pm"
    action: "Draft from a template"
    tool: copilot-word
    pattern: document-creation
  - time: "3:00 pm"
    action: "Catch up on the day"
    tool: copilot-teams
---
9:00 am Summarize email
Jaclyn works with teams across eight countries. On Monday morning, she uses using Copilot in Outlook.to summarize her emails by country. She then asks Copilot to create her weekly schedule.

Outlook icon
Copilot in Outlook

Sample Prompt: Summarize my emails by country and create my weekly schedule.

10:15 am Translate text
She receives content in Polish, so she asks Copilot to translate the text into English.

Microsoft Copilot icon
Microsoft 365 Copilot Chat1

Sample Prompt: Summarize [text] into English.

11:00 am Recap a meeting
Next, Jaclyn participates in a Teams call. She often struggles to recall action items from meetings, so she relies on Teams Meeting Recap to document them.

Microsoft Teams icon
Copilot in Teams

Sample Prompt: Following the meeting, Jaclyn selects View Recap to review meeting notes and follow-up tasks.

1:30 pm Find documents
Jaclyn receives content from the eight countries she works with along with outside vendors. She uses Microsoft 365 Copilot to sort through and find specific content both internal and external.

Microsoft Copilot icon
Microsoft 365 Copilot Chat2

Sample Prompt: Find all content about [topic].

2:00 pm Draft from a template
Jaclyn is responsible for creating best practice documents for internal teams and vendors. She uses Office Agent to create a draft in Word based on previous templates she has built.

Word icon
Copilot in Word

Sample Prompt: Create a draft for [outline] using /[file] as a template.

3:00 pm Catch up on the day
As Jaclyn wraps up her day, she asks Copilot in Teams to summarize any mentions in her teams chats that she might've missed during the day.

Microsoft Teams icon
Copilot in Teams

Sample Prompt: Find where I have been mentioned over the past eight hours.

---

# ═══════════════════════════════════════════════════════════════
# FILE: hr/hr.md
# ═══════════════════════════════════════════════════════════════

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

Identify key metrics which can provide insights into manager capabilities and opportunities. Use Copilot to access HR system data using a Copilot Studio agent.

**Tool:** Microsoft Copilot Studio — AI Agent

**Action:** Use Copilot to rapidly locate relevant manager metrics (employee survey, attrition, training consumption) which impact team health.

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
> I am a human resources consultant. Create a planning document based on the manager report and best in class manager practices.

---

## 4. Communicate Opportunities

Draft and send email to managers sharing the data insights and best practices with them for consideration.

**Tool:** Copilot in Outlook

**Example Prompt:**
> Draft with Copilot: an email to managers sharing opportunities based on best practices and their manager insights. Tone is Direct and Length is Medium.

---

## 5. Align and Track

Conduct a meeting to present the opportunities, align and track the action items, referencing insights and notes from Copilot in Teams.

**Tool:** Copilot in Teams

**Action:** Use Copilot during the meeting to "list main ideas we discussed" and then review the AI notes "Follow-up tasks" after the meeting to finalize the plan and track.

---

## 6. Adjust and Iterate

Based on the impact of the actions, adjust approach and continue to iterate using Copilot in Loop.

**Tool:** Copilot in Loop

**Example Prompt:**
> Brainstorm activities to empower and improve effectiveness of people managers in an organization. Invite colleagues to iterate on ideas.

---

# ═══════════════════════════════════════════════════════════════
# FILE: it-devices/it-devices.md
# ═══════════════════════════════════════════════════════════════

---
name: "Agent: Assisted Device Acquisition"
description: |
  IT professionals can use agents to create a self-service workflow for
  device acquisitions by end-users that assists with selection and approvals.
track: agent
persona: "IT Professional"
industry: "IT"
primary-tool: "Microsoft Copilot Studio — AI Agent"
source: Microsoft-Copilot-scenario-for-IT_Assisted-device-acquisition.pptx

imports:
  - shared/tools/copilot-studio.md

tools:
  - copilot-studio

steps: 6
---
# Using Copilot for Assisted Device Acquisition

IT professionals can use agents to create a self-service workflow for device acquisitions by end-users that assists with selection and approvals.

**Tool:** Microsoft Copilot Studio — AI Agent

---

## 1. Access Device Purchase App

Using a Teams app, access a custom Copilot agent built with Copilot Studio that assists users in selecting and ordering a new device.

**Activity:** In Teams, open the Device Selection and Purchase app.

---

## 2. Answer Usage Questions

Answer a series of questions about typical usage patterns. The bot determines follow-up questions based on the responses given.

**Activity:** Answer questions from the bot about applications used, mobility requirements, meeting requirements, etc.

---

## 3. Review Recommendations

The app interprets the responses and uses additional information like the person's role to provide a selection of applicable devices from the pre-approved vendor list with their capabilities and pricing.

**Activity:** Review the suggested devices and select the most preferred one.

---

## 4. Place Order

After the selection is made, the app initiates the ordering process, which includes an approval process.

**Activity:** Place the order in the app.

---

## 5. Manager Approval

The app sends an email to the user's manager with the order information and a link to approve.

**Activity:** The manager uses the link to open the Teams app and approve the order.

---

## 6. Tracking Shipment

The app places the order with the device supplier and confirms the order and delivery date with the user via email. The user can track progress on the order via the app.

---

# ═══════════════════════════════════════════════════════════════
# FILE: it-prompts/it-prompts.md
# ═══════════════════════════════════════════════════════════════

---
name: "Scenario: General IT Prompts"
description: |
  Copilot Chat can assist IT professionals throughout their day with coding,
  research, and documentation. A collection of standalone prompt templates.
track: scenario
persona: "IT Professional"
industry: "IT"
source: Microsoft-Copilot-scenario-for-IT_General-IT-prompts.pptx

imports:
  - shared/tools/copilot-chat.md
  - shared/patterns/research-synthesis.md

tools:
  - copilot-chat

steps: 6
---
# Using Copilot for General IT Prompts

Copilot Chat can assist IT professionals throughout their day with coding, research, and documentation.

**Tool:** Microsoft 365 Copilot Chat

---

## 1. Research

Use Copilot Chat to stay up to date with the latest technologies and best practices through continued learning.

**Sample Prompt:**
> I want to learn more about how to implement [tool, service]. Provide a high-level outline with guidance on how to implement, best practice set ups, how to get leader buy-in, etc.

---

## 2. Create IT Documentation

Use Copilot Chat in Word to create and update documentation for IT processes, configurations, and troubleshooting guides.

**Sample Prompt:**
> Create a 2-page document detailing the architecture of [system] in non-technical language. Be sure to incorporate key features, technologies, and a process diagram.

---

## 3. User Training

When you implement a new service or product to your organization, use Copilot to streamline the user training process.

**Sample Prompt:**
> I am training new users on our [software, system, tool]. Create an outline for a 30-minute training including key concepts, demos, and best practices.

---

## 4. IT Inventory Management

Use Copilot to help manage and regularly update your IT inventory.

**Sample Prompt:**
> Outline the key information needed for an IT asset inventory system and some suggestions for how to maintain these logs.

---

## 5. Backup and Recovery

Copilot can help you stay on top of your backup and recovery processes.

**Sample Prompt:**
> Provide an example of a PowerShell script that takes daily snapshot backups of Azure Storage volumes, organizes logically, and removes snapshots older than 30 days for compliance.

---

## 6. Code Review

Ask Copilot to help fix issues that arise from a code review.

**Sample Prompt:**
> Rewrite this code so that it will no longer have an error when the user picks an invalid product type. Also add comments and provide a summary of what it does: `<code>`

---

# ═══════════════════════════════════════════════════════════════
# FILE: it-solution/it-solution.md
# ═══════════════════════════════════════════════════════════════

---
name: "Scenario: Evaluate and Purchase IT Solution"
description: |
  IT professionals can use Copilot for assistance when evaluating, purchasing,
  and deploying new solutions. Copilot helps speed the process and reduce costs
  through research, pricing modeling, and RFP creation.
track: scenario
persona: "IT Professional"
industry: "IT"
source: Microsoft-Copilot-scenario-for-IT_evaluate-and-purchase-a-new-IT-solution.pptx

imports:
  - shared/tools/copilot-chat.md
  - shared/tools/copilot-excel.md
  - shared/tools/copilot-word.md
  - shared/tools/copilot-outlook.md
  - shared/patterns/research-synthesis.md
  - shared/patterns/document-creation.md
  - shared/patterns/data-analysis.md
  - shared/patterns/stakeholder-communication.md

tools:
  - copilot-chat
  - copilot-excel
  - copilot-word
  - copilot-outlook

steps: 6
---
# Using Copilot to Evaluate and Purchase a New IT Solution

IT professionals can use Copilot for assistance when evaluating, purchasing, and deploying new solutions. Copilot helps speed the process and reduce costs through research, pricing modeling, and RFP creation.

---

## 1. Gather Business Requirements

Use Microsoft Copilot to aggregate multiple threads of conversations and create a holistic view of all essential business requirements for the new solution.

**Tool:** Microsoft 365 Copilot Chat

**Sample Prompt:**
> Summarize my conversations about [project name].

---

## 2. Research Solutions

Quickly and accurately compare available solutions on the market using Copilot.

**Tool:** Microsoft 365 Copilot Chat

**Sample Prompt:**
> Prepare a summary of information gathered from [website].

---

## 3. Create a Build vs. Buy Analysis

Use Agent Mode in Excel to compare quotes from vendors with internal cost proposals and analyze key differences.

**Tool:** Copilot in Excel

**Sample Prompt:**
> Create a build vs buy analysis for our new image hosting platform and use the data from these documents to create a table of key differences.

---

## 4. Create Solution RFP

Use Office Agent to draft an RFP in Word to the selected vendors, pulling in information from your emails, meeting notes, and presentations.

**Tool:** Copilot in Word

**Sample Prompt:**
> Draft a "request for proposal" using the attached files as reference: [email], [meeting recap], [presentation].

---

## 5. Create a Comparison Chart

Use Copilot to create a comparison chart across all the vendor proposals you received.

**Tool:** Microsoft 365 Copilot Chat

**Sample Prompt:**
> Create a comparison chart with each company's proposal, with the company at the top of each column, and rows that compare cost, services provided, and timeline.

---

## 6. Create Launch Communication

Use Copilot in Outlook to easily draft an email announcing the new solution and thanking contributors.

**Tool:** Copilot in Outlook

**Action:** Use Coaching by Copilot to ensure the message is clear, concise, and impactful, and receive coaching tips.

---

# ═══════════════════════════════════════════════════════════════
# FILE: manager/manager.md
# ═══════════════════════════════════════════════════════════════

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

---

# ═══════════════════════════════════════════════════════════════
# FILE: media/media.md
# ═══════════════════════════════════════════════════════════════

---
name: "Scenario: AI Visual Creation"
description: |
  Copilot can assist media teams with preparation for an interview such as
  research, potential questions and answers, and reviews. Covers the full
  interview lifecycle from research to post-interview analysis.
track: scenario
persona: "Media Professional"
industry: "Media & Entertainment"
source: Microsoft-Copilot-Scenario-for-Media-and-Entertainment_AI-visual-creation.pptx

imports:
  - shared/tools/copilot-chat.md
  - shared/tools/copilot-word.md
  - shared/tools/copilot-teams.md
  - shared/patterns/research-synthesis.md
  - shared/patterns/meeting-recap.md
  - shared/patterns/document-creation.md

tools:
  - copilot-chat
  - copilot-word
  - copilot-teams

steps: 6
---
# Scenario — Media & Entertainment: AI Visual Creation

Copilot can assist media teams with the preparation for an interview such as research, potential questions and answers, and reviews.

---

## 1. Research the Interviewer

Ask Copilot to research the interviewer and their publication to understand their previous work and approach to the topic.

**Tool:** Microsoft 365 Copilot Chat

**Benefit:** Having context from similar conversations can help you anticipate tough questions that may arise. Search for articles, interviews, and other content they have produced.

---

## 2. Research the Audience

Use Copilot to research the demographic of the publication the interviewer works for. This will help tailor the FAQs and responses to the audience's interests and knowledge level.

**Tool:** Microsoft 365 Copilot Chat

**Benefit:** Understand the publication's readership, allowing the spokesperson to connect more effectively with the audience's interests and expectations.

---

## 3. Generate Questions

Prompt Copilot to generate anticipated questions. The questions should be informed by the research conducted in the previous steps and crafted to be conversational yet concise.

**Tool:** Microsoft 365 Copilot Chat

**Benefit:** Proactively prepare for the types of questions that are likely to be asked, enabling the spokesperson to respond confidently and thoughtfully.

---

## 4. Generate Answers

Ask Copilot to generate answers for each anticipated question generated in the previous step, using related organization documents to source answers.

**Tool:** Microsoft 365 Copilot Chat

**Benefit:** Craft answers that align with the organization's messaging and the spokesperson's personal voice — all with the context of the interviewer's approach.

---

## 5. Create an FAQ Document

Use Office Agent to create an FAQ Word document using the Copilot-generated questions and answers from the previous steps.

**Tool:** Copilot in Word

**Benefit:** Ensure consistency and accuracy in the spokesperson's responses by aligning with the organization's official stance and create a single source of truth to reference later.

---

## 6. Conduct the Interview

Conduct the interview through Microsoft Teams and query the transcript with Copilot in Teams to judge your spokesperson's answers against the FAQ Document.

**Tool:** Copilot in Teams

**Benefit:** Maintain integrity and consistency of messaging and prepare for future interviews.

---

# ═══════════════════════════════════════════════════════════════
# FILE: operations-pm/operations-pm.md
# ═══════════════════════════════════════════════════════════════

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

---

# ═══════════════════════════════════════════════════════════════
# FILE: project-manager/project-manager.md
# ═══════════════════════════════════════════════════════════════

---
name: "Agent: Project Manager"
description: |
  Automate standardized BRD and PRD creation for clarity, consistency,
  and stakeholder alignment using a Copilot Studio AI Agent.
track: agent
persona: "Project Manager"
industry: "General"
primary-tool: "Microsoft Copilot Studio — AI Agent"
source: Microsoft-AI-Agent-for-IT_Project-Manager-Agent.pptx

imports:
  - shared/tools/copilot-studio.md

tools:
  - copilot-studio

steps: 6
---
# Using AI Agents to Create a Project Manager Agent

Automate standardized BRD and PRD creation for clarity, consistency, and stakeholder alignment.

**Tool:** Microsoft Copilot Studio — AI Agent

---

## 1. Gathering Business Needs from Stakeholders

Gathers business needs and objectives from stakeholder interviews, workshops, and existing documentation.

> Reduces time spent on manual stakeholder interviews by consolidating and structuring inputs automatically.

---

## 2. Analyzing User Stories & Market Research

Analyzes user stories, market research, and competitive analysis to identify product/project/business requirements.

> Improves requirement quality by identifying trends, gaps, and user needs from historical and external sources.

---

## 3. Structuring Information into BRD/PRD Templates

Structures information into a standardized BRD/PRD/etc. template, ensuring clarity and consistency.

> Ensures consistency and completeness in documentation using standardized formats.

---

## 4. Automated Generation of Scope, Features & Specs

Automatically generates sections for scope, features, user flows, acceptance criteria, and technical specifications.

> Accelerates documentation by instantly generating core requirement components, reducing manual drafting.

---

## 5. Facilitating Collaboration & Feedback

Facilitates collaboration and feedback from stakeholders through version control and commenting features.

> Streamlines versioning and feedback collection for better stakeholder alignment and fewer revisions.

---

## 6. Finalize and Share Documents

Prepares polished, ready-to-share documentation.

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/patterns/data-analysis.md
# ═══════════════════════════════════════════════════════════════

---
pattern:
  name: Data Analysis & Reporting
  slug: data-analysis
  description: >
    Analyze structured data, generate insights, create comparison charts, and produce reports using Copilot in Excel (Agent Mode) or Copilot Chat.
  typical-tools:
    - copilot-excel
    - copilot-chat
    - analyst
  used-in:
    - hr
    - it-solution
    - operations-pm
---

# Data Analysis & Reporting

Analyze structured data, generate insights, create comparison charts, and produce reports using Copilot in Excel (Agent Mode) or Copilot Chat. This pattern transforms raw data into meaningful visualizations and actionable recommendations.

## Example Prompts

- "Produce a report for each manager showing strengths and weaknesses based on the metrics"
- "Create a build vs buy analysis for our new image hosting platform"
- "Suggest formulas for this column. Show insights in charts"
- "Create a comparison chart with each company's proposal, with the company at the top of each column"

## Typical Flow

1. Gather data sources (spreadsheets, metrics, vendor proposals)
2. Use Agent Mode in Excel or Copilot Chat for analysis
3. Request specific outputs (comparison tables, charts, formulas)
4. Review insights and identify key findings
5. Share analysis with stakeholders

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/patterns/document-creation.md
# ═══════════════════════════════════════════════════════════════

---
pattern:
  name: Document Creation
  slug: document-creation
  description: >
    Create formal documents (RFPs, FAQs, best practice guides, IT documentation) using Copilot in Word or Office Agent, often referencing existing templates and multiple source files.
  typical-tools:
    - copilot-word
    - copilot-chat
  used-in:
    - adhd
    - media
    - it-solution
    - manager
---

# Document Creation

Create formal documents (RFPs, FAQs, best practice guides, IT documentation) using Copilot in Word or Office Agent, often referencing existing templates and multiple source files. This pattern accelerates the drafting process by combining multiple inputs into a cohesive document.

## Example Prompts

- "Create a draft for [outline] using /[file] as a template"
- "Draft a 'request for proposal' using the attached files as reference: [email], [meeting recap], [presentation]"
- "Create a 2-page document detailing the architecture of [system] in non-technical language"
- "Create an FAQ Word document using the Copilot-generated questions and answers"

## Typical Flow

1. Identify source materials (templates, emails, meeting recaps, presentations)
2. Use Copilot in Word or Office Agent with /[file] references
3. Specify document type, structure, and tone
4. Review and refine generated draft
5. Finalize and distribute

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/patterns/email-summarization.md
# ═══════════════════════════════════════════════════════════════

---
pattern:
  name: Email Summarization
  slug: email-summarization
  description: >
    Summarize and triage email threads, highlighting urgent items, action items, and key mentions. Especially useful for catching up on overnight activity or cross-timezone communication.
  typical-tools:
    - copilot-chat
    - copilot-outlook
  used-in:
    - adhd
    - manager
    - operations-pm
---

# Email Summarization

Summarize and triage email threads, highlighting urgent items, action items, and key mentions. This pattern is especially useful for catching up on overnight activity or cross-timezone communication. It helps users quickly identify what needs attention without reading every message.

## Example Prompts

- "Summarize my emails by country and create my weekly schedule"
- "Summarize this thread calling out where my name was mentioned and any action items for me"
- "Summarize all the emails and Teams chats in the past day, highlighting the primary asks and open items"

## Typical Flow

1. Open Copilot Chat or Copilot in Outlook
2. Ask for a summary with specific criteria (by sender, topic, urgency, country)
3. Review highlighted action items
4. Use drafting features to respond to urgent items

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/patterns/meeting-recap.md
# ═══════════════════════════════════════════════════════════════

---
pattern:
  name: Meeting Recap
  slug: meeting-recap
  description: >
    Capture meeting summaries, key discussion points, action items, and follow-up tasks using Copilot in Teams. Eliminates manual note-taking.
  typical-tools:
    - copilot-teams
  used-in:
    - adhd
    - manager
    - operations-pm
    - media
---

# Meeting Recap

Capture meeting summaries, key discussion points, action items, and follow-up tasks using Copilot in Teams. This pattern eliminates manual note-taking and ensures nothing falls through the cracks after a meeting.

## Example Prompts

- "Summarize this meeting and provide the key points and action items"
- "Summarize key discussion points. Identify agreed-upon next steps"
- "List the main ideas we discussed"
- "Find where I have been mentioned over the past eight hours"

## Typical Flow

1. Enable transcription in the Teams meeting
2. During or after the meeting, activate Copilot in Teams
3. Request summary of key points and action items
4. Review AI-generated "Follow-up tasks"
5. Share recap with attendees via Outlook

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/patterns/research-synthesis.md
# ═══════════════════════════════════════════════════════════════

---
pattern:
  name: Research & Synthesis
  slug: research-synthesis
  description: >
    Use Copilot to research topics, gather information across multiple sources (documents, web, emails, meetings), and synthesize findings into actionable summaries.
  typical-tools:
    - copilot-chat
  used-in:
    - manager
    - media
    - it-prompts
    - it-solution
---

# Research & Synthesis

Use Copilot to research topics, gather information across multiple sources (documents, web, emails, meetings), and synthesize findings into actionable summaries. This pattern turns scattered information into structured, decision-ready outputs.

## Example Prompts

- "Find information on [project x]"
- "Research the interviewer and their publication to understand their previous work"
- "I want to learn more about how to implement [tool, service]. Provide a high-level outline"
- "Prepare a summary of information gathered from [website]"
- "Summarize my conversations about [project name]"

## Typical Flow

1. Define the research question or topic
2. Ask Copilot to search across relevant sources (documents, web, emails)
3. Request structured output (outline, comparison, summary)
4. Refine and iterate based on initial findings
5. Compile into actionable document or decision framework

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/patterns/stakeholder-communication.md
# ═══════════════════════════════════════════════════════════════

---
pattern:
  name: Stakeholder Communication
  slug: stakeholder-communication
  description: >
    Draft and send targeted communications to stakeholders (managers, teams, executives) with specified tone, sharing insights, recommendations, or announcements.
  typical-tools:
    - copilot-outlook
    - copilot-chat
  used-in:
    - hr
    - manager
    - it-solution
---

# Stakeholder Communication

Draft and send targeted communications to stakeholders (managers, teams, executives) with specified tone, sharing insights, recommendations, or announcements. This pattern ensures professional, consistent messaging across organizational levels.

## Example Prompts

- "Draft with Copilot: an email to managers sharing opportunities based on best practices and their manager insights. Tone is Direct and Length is Medium"
- "Draft an email announcing the new solution and thanking contributors"
- "Use Coaching by Copilot to ensure the message is clear, concise, and impactful"

## Typical Flow

1. Prepare insights or recommendations to share
2. Use Copilot in Outlook to draft the communication
3. Specify tone (Direct, Formal, Casual), length, and sentiment
4. Use Coaching by Copilot for refinement
5. Review and send to target audience

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/tools/analyst.md
# ═══════════════════════════════════════════════════════════════

---
tool:
  name: Analyst
  slug: analyst
  category: data-analysis
  m365-product: Microsoft 365 Copilot
  description: >
    Copilot's data analysis capability that produces detailed reports with insights from structured data and metrics.
  capabilities:
    - report-generation
    - strength-weakness-analysis
    - metric-interpretation
---

# Analyst

Copilot's data analysis capability that produces detailed reports with insights from structured data and metrics.

## Common Actions

- Produce reports showing strengths and weaknesses
- Analyze metrics such as survey results, attrition data, and training completion
- Generate per-entity analyses for detailed breakdowns

## Best Practices

- Provide clear metric definitions so the analysis is grounded in accurate context.
- Request specific report formats to get output that matches your needs.

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/tools/copilot-chat.md
# ═══════════════════════════════════════════════════════════════

---
tool:
  name: Microsoft 365 Copilot Chat
  slug: copilot-chat
  category: chat
  m365-product: Microsoft 365 Copilot
  description: >
    General-purpose AI assistant accessed via microsoft365.com/copilot or the M365 Copilot Chat app. Searches across emails, meetings, files, and the web. Supports work and web mode.
  capabilities:
    - email-summarization
    - research
    - document-search
    - translation
    - calendar-analysis
    - content-generation
    - comparison-creation
---

# Microsoft 365 Copilot Chat

General-purpose AI assistant accessed via microsoft365.com/copilot or the M365 Copilot Chat app. Searches across emails, meetings, files, and the web. Supports work and web mode.

## Common Actions

- Summarize emails and email threads
- Research topics using web and organizational data
- Translate content between languages
- Analyze calendar for scheduling insights
- Generate content such as comparisons, plans, and outlines
- Find information across files in your organization

## Best Practices

- Use Work mode for organizational data such as emails, files, and calendar events.
- Use Web mode for external research and publicly available information.
- Reference files directly using the /[filename] syntax to ground responses in specific documents.

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/tools/copilot-excel.md
# ═══════════════════════════════════════════════════════════════

---
tool:
  name: Copilot in Excel
  slug: copilot-excel
  category: data-analysis
  m365-product: Microsoft Excel
  description: >
    AI assistant in Excel for data analysis, formula suggestions, chart generation, and build-vs-buy analysis using Agent Mode.
  capabilities:
    - data-analysis
    - formula-suggestions
    - chart-generation
    - comparison-tables
---

# Copilot in Excel

AI assistant in Excel for data analysis, formula suggestions, chart generation, and build-vs-buy analysis using Agent Mode.

## Common Actions

- Analyze data and suggest formulas
- Generate insights as charts
- Create build-vs-buy analyses
- Compare vendor proposals in tables

## Best Practices

- Use Agent Mode for complex analysis that requires multiple steps.
- Provide clear data context so Copilot can generate accurate formulas and insights.
- Ask for specific chart types to get the visualization you need.

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/tools/copilot-loop.md
# ═══════════════════════════════════════════════════════════════

---
tool:
  name: Copilot in Loop
  slug: copilot-loop
  category: collaboration
  m365-product: Microsoft Loop
  description: >
    AI assistant in Loop for iterative collaboration, brainstorming, and team-based content refinement.
  capabilities:
    - iterative-brainstorming
    - collaborative-editing
    - activity-planning
---

# Copilot in Loop

AI assistant in Loop for iterative collaboration, brainstorming, and team-based content refinement.

## Common Actions

- Brainstorm activities and improvements
- Invite colleagues to iterate on ideas
- Track iterative changes across sessions

## Best Practices

- Use Copilot in Loop for ongoing iteration where content evolves over time.
- Invite team members for collaborative refinement to leverage diverse perspectives.

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/tools/copilot-outlook.md
# ═══════════════════════════════════════════════════════════════

---
tool:
  name: Copilot in Outlook
  slug: copilot-outlook
  category: communication
  m365-product: Microsoft Outlook
  description: >
    AI assistant in Outlook for email drafting, summarization, and coaching on tone and clarity.
  capabilities:
    - email-drafting
    - email-summarization
    - tone-adjustment
    - coaching
---

# Copilot in Outlook

AI assistant in Outlook for email drafting, summarization, and coaching on tone and clarity.

## Common Actions

- Draft emails with specified tone, length, and sentiment
- Summarize email threads to quickly understand conversations
- Use Coaching by Copilot for clarity and impact improvements
- Draft follow-up emails after meetings

## Best Practices

- Use Draft with Copilot for composing new emails efficiently.
- Specify tone (Direct, Casual, Formal) and length (Short, Medium, Long) to get the desired output.
- Use coaching tips to craft more impactful and clear messages.

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/tools/copilot-powerpoint.md
# ═══════════════════════════════════════════════════════════════

---
tool:
  name: Copilot in PowerPoint
  slug: copilot-powerpoint
  category: presentation
  m365-product: Microsoft PowerPoint
  description: >
    AI assistant in PowerPoint for creating and editing presentations from documents and outlines.
  capabilities:
    - presentation-creation
    - slide-layout-changes
    - content-from-documents
---

# Copilot in PowerPoint

AI assistant in PowerPoint for creating and editing presentations from documents and outlines.

## Common Actions

- Create presentations from Word files
- Change slide layouts to match desired style
- Generate content from outlines

## Best Practices

- Start from existing Word or Loop content to produce well-structured presentations.
- Use Office Agent for cross-document content when building comprehensive decks.

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/tools/copilot-studio.md
# ═══════════════════════════════════════════════════════════════

---
tool:
  name: Microsoft Copilot Studio
  slug: copilot-studio
  category: agent-builder
  m365-product: Microsoft Copilot Studio
  description: >
    Platform for building custom AI agents that automate multi-step workflows, questionnaires, and approval processes within Microsoft 365.
  capabilities:
    - custom-agent-creation
    - conversational-workflows
    - approval-processes
    - data-aggregation
    - document-generation
---

# Microsoft Copilot Studio

Platform for building custom AI agents that automate multi-step workflows, questionnaires, and approval processes within Microsoft 365.

## Common Actions

- Build self-service bots for device ordering, BRD/PRD generation, and more
- Create conversational questionnaires that guide users through processes
- Automate approval workflows
- Access HR and business system data via custom agents

## Best Practices

- Deploy agents via Teams apps for easy access across the organization.
- Design conversational flows with follow-up logic to handle complex scenarios.
- Integrate with existing business systems to leverage organizational data.

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/tools/copilot-teams.md
# ═══════════════════════════════════════════════════════════════

---
tool:
  name: Copilot in Teams
  slug: copilot-teams
  category: collaboration
  m365-product: Microsoft Teams
  description: >
    AI assistant within Microsoft Teams for meeting recaps, chat summaries, and real-time meeting assistance.
  capabilities:
    - meeting-recap
    - chat-summarization
    - action-item-extraction
    - mention-tracking
---

# Copilot in Teams

AI assistant within Microsoft Teams for meeting recaps, chat summaries, and real-time meeting assistance.

## Common Actions

- Meeting Recap (View Recap) to review what was discussed
- Summarize discussions with action items
- Track mentions of specific people or topics
- Extract follow-up tasks from conversations

## Best Practices

- Enable transcription before meetings to allow Copilot to generate accurate recaps.
- Use Copilot during meetings for real-time summaries and key point tracking.
- Review AI Notes Follow-up tasks after meetings to ensure nothing is missed.

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/tools/copilot-whiteboard.md
# ═══════════════════════════════════════════════════════════════

---
tool:
  name: Copilot in Whiteboard
  slug: copilot-whiteboard
  category: ideation
  m365-product: Microsoft Whiteboard
  description: >
    AI assistant in Whiteboard for brainstorming, idea organization, and collaborative planning.
  capabilities:
    - brainstorming
    - idea-categorization
    - project-simplification
---

# Copilot in Whiteboard

AI assistant in Whiteboard for brainstorming, idea organization, and collaborative planning.

## Common Actions

- Brainstorm ideas and organize them into categories
- Simplify complex projects for collaboration
- Capture key project needs visually

## Best Practices

- Use Copilot in Whiteboard for team brainstorming sessions to capture ideas quickly.
- Organize thoughts into logical categories to structure discussions effectively.

---

# ═══════════════════════════════════════════════════════════════
# FILE: shared/tools/copilot-word.md
# ═══════════════════════════════════════════════════════════════

---
tool:
  name: Copilot in Word
  slug: copilot-word
  category: document-creation
  m365-product: Microsoft Word
  description: >
    AI assistant in Word for document drafting, template-based creation, and content generation using Office Agent.
  capabilities:
    - document-drafting
    - template-based-creation
    - content-structuring
    - reference-integration
---

# Copilot in Word

AI assistant in Word for document drafting, template-based creation, and content generation using Office Agent.

## Common Actions

- Create drafts using templates with /[file] syntax to reference existing documents
- Generate documentation from outlines
- Draft RFPs, FAQs, and best practice documents
- Create documents from multiple reference files

## Best Practices

- Reference existing templates to maintain consistency with organizational standards.
- Use Office Agent for cross-document content aggregation and generation.
- Specify document structure in prompts to get well-organized output.
