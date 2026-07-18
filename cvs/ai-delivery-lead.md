# Vincent van den Braken
## AI-First Delivery Lead | Production Systems Engineer

---

## Profile

**AI-first operational delivery**: Automate terugkerende informatieprobleem; niet symptoom-gaten plakken.

**Production discipline**: MVP's AI-only mag zijn. Schaalbare systemen niet. Review-slop-risico: AI-code ziet er klaar uit, faalt onder load of edge-case. Bewuste onderscheid tussen "proof concept" en "draait morgen in productie".

**Praktijk-bewijs**: Niet consultant die vertelt hoe het zou moeten. Eigen systemd-service (Bolwerker) triageert mijn dagelijkse leven: Telegram/Discord-inbox, PR-backlog, CI-fouten, operationele e-mail. Multi-agent orchestratie, review-gates, escalatielogica. Draait drie maanden zonder aanpassing. Dat is schaal.

---

## Key Achievement: Bolwerker

**Probleem**: Vijftig Slack/Discord/Telegram-meldingen per dag. Handmatig triage = context-switchen, fouten, wasted cycles.

**Oplossing**: C#-service (systemd). Vier gespecialiseerde agents: 
- **Sherlock**: Git/CLI diagnostiek (CI-failures, build-logs, repo-state)
- **LiRS**: Long-term info retrieval (vooruitplanningsvragen, historisch)
- **Florence**: Context-gevoelige escalatie (gezondheid, persoonlijke priorities)
- **Bolwerker**: Meta-orchestratie (welke agent, wanneer, hoe escaleren)

Elk agent eigen system-prompt + specialized tools. Inbox-triage: agent sluit af, of gooit naar Telegram voor manueel escalatie.

**Outcome**:
- 70% autonome afhandeling (geen handmatige triage)
- Drie maanden runtime: stabiel, reproduceerbaar
- Productie-grade: logging, config hot-reload, session-persistence
- Bewijs voor klant: AI is niet "tools draaien", maar "orchestratie op schaal"

---

## Key Achievement: Hermes / Bolwerker Retrieval Subsystem

**Probleem**: Gemeente wil inzicht in lokale (Stein, Urmond) beleidsveranderingen. Maar: informatie zit in persmeldingen, PDF's, websites. Geen centraal register.

**Use Case**: Terugkerend informatieprobleem = RAG-kandidaat.

**Oplossing**: 
- Dagelijks/wekelijks: Scrape gemeente-pages + Natuurmonumenten-feeds
- Chunk + index (vector-DB)
- Agent voert natuurlijke vragen uit: "Wat is de stand van zaken rond bosuitbreiding?" → contextueel antwoord
- Geen handmatige updates. Geen "ik ben hoe-het-was"

**Outcome**:
- Bewijs dat "elke terugkerende informatiebehoefte agent-materiaal is"
- Reproduceerbare RAG-pipeline (Docker-ready, één command-start)
- Niet aangeboden als "proof of concept", aangeboden als productie-onderhoud

---

## Distinctive Capabilities

### Use Case Identification (Root Cause, niet Symptoom)

Drie zzp-engagements illustreren de methode:

1. **Klant X**: "Releases lopen uit de hand."  
   → Triage 48h: développeurs wachten op code-review, niet op build.  
   → Oplossing: review-agent, niet meer CI. Impact: deployment-tijd -60%.

2. **Klant Y**: "Vertrokken dev = kennislek."  
   → Triage 72h: Dev wasn't knowledge bottleneck; design-proces was (no ADRs, monoliet-logic-jump).  
   → Oplossing: design-docs + playbook, niet "onboarding". New dev: self-service 72h.

3. **Klant Z**: "Data inzicht = maanden rapportage."  
   → Triage 48h: SQL-reports kloppen niet, dashboards staan droog omdat dashboarder handmatig bijwerkt.  
   → Oplossing: automated lineage-check, niet meer dashboards.

**Patroon**: Symptoom (langzaam) → oorzaak (proces) → solution (struktureel). Risicoloos escaleren als ik fout zit.

### AI Adoption at Scale (Één persoon, Drie jaar)

Geen "consultancy pilot op team-van-twintig". Wel: volledige eigenaar-to-production pad:

- Claude Code + custom agents
- Hooks (pre/post-tooluse, stop)
- Skills (herbruikbare prompts)
- Knowledge-graph pipeline (eigen context-management)
- Review-gates (code-reviewer, security-reviewer, in-policy voor alles)
- Token-tracking (cost-per-session, trend)

Elke mislukking (AI-output die leek goed maar niet draaide, review false-neg's, hallucination-cycles) is voeten-in-de-grond. Kan ik vertellen.

### Production vs MVP Discipline

**MVP** (AI-only mag):
- Proof of concept draait in notebook.
- Agent gaat "laten zien" zonder audit.
- Review: "Ziet er goed uit."

**Productie** (waarschuwing):
- AI-code die review passeert: vaak fragiel. Ziet eruit als "klaar", faalt op:
  - Edge cases (onverwachte input-format)
  - Concurrency (multi-agent contention)
  - Grensgevallen (lege result-set, timeout, null)
  - Schaal (performance onder load)

**Review-fagitude-risico**: Code review ziet "AI wrote it, looks clean", maar misses:
- "Dit breken bij 100 requests/sec"
- "Dit assume string is ASCII, falls apart op UTF-8"
- "Dit swallow error, log niets"

**Onderscheid dat ik maak**:
- AI: snel concept. 
- Productie: expliciete test-coverage, load-test, edge-case-list, monitoring.
- Anders: "AI slop" onder het mom van "klaar".

---

## Experience Snapshot

**30+ jaren**: Developer, architect, delivery manager, operations. Alle drie rollen in parallelle projecten.

**Delivery Context** (laatste 5 jaren):
- Root-cause analysis: probleem → symptoom-list → oorzaak (3-5 dagen)
- Adoption plans: Ik bouw ze MÉT het team (niet "ik schrijf advies, jij voert uit")
- Stakeholder trust: Ik zeg ook wat je niet wilt horen → vertrouwen van board EN engineers
- Schaal: Één klant tegelijk, volledig eigendom. (Groei = parallel capacity, niet context-split)

**AI/Automation**:
- Bolwerker (3 maanden continuous, systemd-service)
- Hermes (RAG, pipeline, dagelijks/wekelijks automation)
- Claude Code + agent-orchestratie (eigen produktiviteitssysteem)
- Bewerkingen: token-tracking, session-analysis, memory-system

**Data** (Power BI, R, reproduceerbare pipelines):
- Gemeentelijke onderwijsdata (student-flow, docentbezetting)
- CBS + gemeente (PC6-to-buurt mapping, multi-level joins)
- Voetbalclub-data (ledenverloop + CBS sociale samenstelling; anoniem)

**Tools**: 
- Azure DevOps, Jira (gebruikt)
- Power BI (echte pipelines, niet drag-drop templates)
- ServiceNow (nee)
- Azure AI, OpenAI (concept-equivalent aan Claude)
- Git, CI/CD (dertig jaar)

---

## Why This Matters for AI Delivery Lead

1. **Geen papieren-consult**. Bolwerker live tonen: "Dit is wat 'AI structureel in delivery' betekent."

2. **Anti-slop discipline**. Kan risico-waarschuwing geven: "Dit review-passeert, maar breken onder productie-last." (En waarom.)

3. **Root cause, niet symptoom-patch**. Opdrachtgever ziet: "Dit is hoe je een informatieprobleem echt oplost."

4. **Eigenaar-houding**. Bolwerker, Hermes, CV-pipeline: alles op mijn eigen machines, mijn eigen schaal. Kan ik schalen naar team-context.

5. **Eerlijke schaal-assessment**. Delivery Lead op één team tegelijk is precies de groei ik zoek. Niet "tien engagements parallel" → geen van allen diep.

---

## Questions for Your Team

1. **Bouwruimte**: Mag Delivery Lead zelf proof-of-concepts en reference-implementations bouwen? (Kracht: laten zien, niet vertellen. Bolwerker-niveau.)

2. **Verhouding**: Percentage klantwerk declarabel vs intern capability-building?

3. **Schaal**: Band is B/C. Met mijn ervaring: inschaling vroeg op tafel, anders verrassing later.

---

## Technical Notes

- **Language**: Nederlands moedertaal, Engelstalige technical work
- **Papieren**: HBO/WO-grens al doorgebroken (eigen precedent); ervaring meet het standaard
- **Schaal-realistisch**: Dertig jaar werk, tien jaar in één functionele context. Dat is groei, niet sprong.

---

## Niet Op CV, Wel Relevant

- Hermes draait ook op vacatureplatforms. Maar Natuurmonumenten/Stein/Urmond: het verhaal.
- Voetbalclub-werk: anoniem (NDA). Zeggen wat mag, niet meer.
- Nevenwerkzaamheden: Eigen stack + zzp-praktijk blijven hands-on onderhoud. Contractueel voor de eerste keer checken.
