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
