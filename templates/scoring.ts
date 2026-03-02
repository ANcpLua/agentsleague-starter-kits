// Agents League — Scoring Logic

// ─── Track 1: Creative Apps ───────────────────────────────────────────────────

type Track1Score = {
  accuracyRelevance: number;        // 0–100
  reasoningMultiStep: number;       // 0–100
  creativityOriginality: number;    // 0–100
  userExperiencePresentation: number; // 0–100
  reliabilitySafety: number;        // 0–100
  communityVote: number;            // 0–100
};

function scoreTrack1(s: Track1Score): number {
  return (
    s.accuracyRelevance          * 0.20 +
    s.reasoningMultiStep         * 0.20 +
    s.creativityOriginality      * 0.15 +
    s.userExperiencePresentation * 0.15 +
    s.reliabilitySafety          * 0.20 +
    s.communityVote              * 0.10
  );
}

// ─── Track 2: Reasoning Agents ────────────────────────────────────────────────

type Track2Score = {
  accuracyRelevance: number;        // 0–100
  reasoningMultiStep: number;       // 0–100
  creativityOriginality: number;    // 0–100
  userExperiencePresentation: number; // 0–100
  reliabilitySafety: number;        // 0–100
};

function scoreTrack2(s: Track2Score): number {
  return (
    s.accuracyRelevance          * 0.25 +
    s.reasoningMultiStep         * 0.25 +
    s.creativityOriginality      * 0.15 +
    s.userExperiencePresentation * 0.15 +
    s.reliabilitySafety          * 0.20
  );
}

// ─── Track 3: Enterprise Agents ───────────────────────────────────────────────

type Track3Features = {
  m365CopilotChatAgent: boolean;      // REQUIRED — must have
  externalMcpServer: boolean;         // +8 pts
  oauthSecurityForMcp: boolean;       // +5 pts
  adaptiveCardsUiUx: boolean;         // +5 pts
  connectedAgentsArchitecture: boolean; // +15 pts (highest bonus)
};

type Track3Result = {
  eligible: boolean;   // false if required feature missing
  points: number;
  maxPoints: number;
  breakdown: Record<string, number>;
};

function scoreTrack3(f: Track3Features): Track3Result {
  if (!f.m365CopilotChatAgent) {
    return { eligible: false, points: 0, maxPoints: 33, breakdown: {} };
  }

  const breakdown = {
    externalMcpServer:            f.externalMcpServer            ? 8  : 0,
    oauthSecurityForMcp:          f.oauthSecurityForMcp          ? 5  : 0,
    adaptiveCardsUiUx:            f.adaptiveCardsUiUx            ? 5  : 0,
    connectedAgentsArchitecture:  f.connectedAgentsArchitecture  ? 15 : 0,
  };

  const points = Object.values(breakdown).reduce((a, b) => a + b, 0);

  return { eligible: true, points, maxPoints: 33, breakdown };
}

// ─── Usage examples ───────────────────────────────────────────────────────────

const track1 = scoreTrack1({
  accuracyRelevance: 85,
  reasoningMultiStep: 90,
  creativityOriginality: 70,
  userExperiencePresentation: 80,
  reliabilitySafety: 88,
  communityVote: 75,
});
console.log(`Track 1 final score: ${track1.toFixed(1)} / 100`);

const track2 = scoreTrack2({
  accuracyRelevance: 90,
  reasoningMultiStep: 95,
  creativityOriginality: 70,
  userExperiencePresentation: 75,
  reliabilitySafety: 85,
});
console.log(`Track 2 final score: ${track2.toFixed(1)} / 100`);

const track3 = scoreTrack3({
  m365CopilotChatAgent: true,       // ✅ required
  externalMcpServer: true,          // ✅ +8
  oauthSecurityForMcp: true,        // ✅ +5
  adaptiveCardsUiUx: false,         // ❌
  connectedAgentsArchitecture: true, // ✅ +15
});
console.log(`Track 3 eligible: ${track3.eligible}`);
console.log(`Track 3 bonus points: ${track3.points} / ${track3.maxPoints}`);
console.log("Breakdown:", track3.breakdown);
