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
