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
