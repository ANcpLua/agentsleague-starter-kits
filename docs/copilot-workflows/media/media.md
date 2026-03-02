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
