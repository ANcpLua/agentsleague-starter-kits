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
