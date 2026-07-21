# HEARTH — Full English Game Script
### Confirmed Revision · Contemporary Native English · ElevenLabs v3 Audio-Tag Production Draft

**English dialogue standard**
- This draft is revised from the established native-English script rather than translated line by line from the Chinese review draft.
- Dialogue uses contemporary American English, contractions, lived-in forms of address, and natural family-drama rhythm.
- Long speeches remain divided into short subtitle and voice-generation units. New lines do not exceed the established maximum spoken-line length.

**ElevenLabs v3 Audio Tag standard**
- Audible performance directions use English square-bracket Audio Tags: `[gentle, reassuring]`, `[with restrained anger]`, `[whispers]`, `[sighs]`.
- A baseline tag normally appears at the beginning of every voiced line.
- A second tag may appear after punctuation or at a natural turning point when the emotion, volume, pacing, or vocal action changes mid-line.
- Tags describe sound that can be heard. Physical movement, eye direction, blocking, and location remain in scene directions.
- Nonverbal vocal actions such as `[sighs]`, `[exhales]`, `[swallows]`, or `[clears throat]` are placed where they occur.
- Audio Tags are used strategically rather than before every comma.

**Household character names and forms of address**
- 17F-01: Daniel and Emily; their eight-year-old son is Noah. The unit may call him `Noah` or `buddy`; Daniel may call Emily `Em`.
- 17F-02: Ben and Claire. They use `babe` in ordinary domestic moments; names replace pet names as the argument becomes serious.
- 17F-03: Mark and Laura; their fourteen-year-old daughter is Ava. Laura uses Ava's name when conflict begins; Mark uses Laura's name while de-escalating.
- Mia's home: Mia and Lily. `Sweetheart` and `baby` are reserved for genuine closeness.

**Format legend**
- Voiced line: `Speaker: [Audio Tag] "Line."`
- Mid-line change: `Speaker: [baseline tag] "First phrase... [new tag] changed phrase."`
- `[SCENE]` blocks contain location, view, triggers, blocking, and implementation notes.
- `[SFX: …]` cues and code-block UI are not voiced.
- SYNTH VOICE is a flat decision announcement. FIELD UNIT is Mia's HUD escort. HOME UNIT is the household companion.
- `[UI: TRUST +1 / −1]` is shown only to the player. Mia never sees or references it.
  One choice per inspected household produces a cumulative total of **+3, +1, −1, or −3**.
  Only the sign matters at the shutdown branch: positive → proper goodbye; negative → forced shutdown.

---

# LEVEL 1
## Lobby and Household One

---

### Scene 1.1 — Building A, Ground-Floor Lobby

```
[SCENE]
Location: Building A lobby, approximately 18:00. Warm amber lighting.
View: Mia, first person. Inspector glasses are already equipped.
Opening input rule: While the lobby briefing and Lily exchange
      play, the player may freely rotate the camera but cannot walk.
      Movement unlocks after Mia's final "Okay."
Positions: Three independent optional NPC groups —
  (1) A small girl rehearsing beside a Public Companion Unit.
  (2) A young man working beside a Work-Assist Unit.
  (3) Mrs. Ellis seated beside a Care Unit with a chest display.
Progression: All three public encounters are optional and trigger only once.
      The assignment terminal is available from the beginning. The player
      may use it immediately or view any public encounter first.
Far side: Assignment terminal beside the elevators.
```

```
FIELD COMPANION UNIT — ACTIVATED
INSPECTOR ID: 7842
ASSIGNED PARTNER: MIA
```

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** [calm, newly activated, professional] "Good evening, Inspector. Field Companion Unit online. I'll be your partner tonight."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Mia:** [restrained, matter-of-fact] "All right."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** [formal, corporately affirming] "You are one of HEARTH's most highly regarded inspectors. Tonight, three household companion units on the seventeenth floor are scheduled for review."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** [clear, instructional] "You will check how each unit has been operating, identify any problems, and decide whether its household-use strategy should be adjusted."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** [calm, adding context] "One additional detail: the seventeenth floor is also where you live."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** [pleasant, lightly promotional] "Before you begin, you may observe how residents are using companion units throughout the lobby."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** [confident, promotional] "These are our flagship products—and the most successful companion technology in the world."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** [clear, directing] "When you're ready, use the assignment terminal to load tonight's inspection files."

*(The HUD pings. A message window opens in the upper-right corner of Mia's view.)*

```
INCOMING VOICE MESSAGE
FROM: LILY
TIME: 4:42 PM

TRANSCRIPT:
"Mom, are you getting home late tonight?
I wanted to tell you something.
We can talk when you get home. I'll wait for you.
...Don't forget, okay?"
```

<!-- HEARTH:SEQUENCES Lobby_LilyVoiceMessage -->
**Lily:** [hopeful, slightly hesitant, recorded] "Mom, are you getting home late tonight? I wanted to tell you something. We can talk when you get home. I'll wait for you... [quieter, almost pleading] Don't forget, okay?"

<!-- HEARTH:SEQUENCES Lobby_OpeningCloseout -->
**Mia:** [concerned, quiet, with a short pause] "Did she say what it was about?"

<!-- HEARTH:SEQUENCES Lobby_OpeningCloseout -->
**Field Unit:** [calm, professional, without personal judgment] "No. That was the whole message. She wants to tell you in person. I recommend finishing the three inspections first, then handling it when you get home."

<!-- HEARTH:SEQUENCES Lobby_OpeningCloseout -->
**Mia:** [restrained, brief] "Okay."

*(As soon as Mia finishes "Okay," the Lily message closes. It does not appear on terminal cameras, inspection cameras, or companion-unit playback HUDs. Player movement unlocks.)*

**— Optional Group 1: the girl —**

<!-- HEARTH:SEQUENCES Lobby_Group01_Girl -->
**Lobby Girl:** [focused, rehearsing] "Hi, everyone. I'm— [nervous, voice catching]"

<!-- HEARTH:SEQUENCES Lobby_Group01_Girl -->
**Public Unit:** [gentle, encouraging, speaking slowly] "You know it. You just rushed the first part. Start with your name and try again."

<!-- HEARTH:SEQUENCES Lobby_Group01_Girl -->
**Lobby Girl:** [quiet, reassured] "Okay."

<!-- HEARTH:SEQUENCES Lobby_Group01_MiaExit -->
**Mia:** [quietly impressed, a little surprised] "Huh. Guess these things really do help with kids."

**— Optional Group 2: the young man —**

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Work Unit:** [calm, conversational, like a familiar coworker] "This section's solid. One small thing: in the second paragraph, 'in summary' sounds more formal than 'anyway.'"

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Young Man:** [distracted, noncommittal] "Mm-hm."

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Work Unit:** [pleasant, professional] "Want me to bring in last week's chart?"

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Young Man:** [subdued, distracted] "Yeah, thanks."

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Work Unit:** [calm, moving to the next task] "Before I go back to the document, do you still want help with that message to your mom?"

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Young Man:** [tired, slightly guilty, speaking quickly] "Yeah. Tell her work ran late. I don't want it to sound like I'm blowing her off again."

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Work Unit:** [helpful, naturally familiar] "How about: 'Hey, Mom. Work ran late, so don't wait for me at dinner. I'll come by tomorrow and bring something good.'"

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Young Man:** [relieved, deciding immediately] "That's good. Send it."

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Work Unit:** [pleasant, efficient] "Sent. She says, 'Okay, sweetheart. Don't work too late. I'll see you tomorrow.'"

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Young Man:** [relieved, genuinely grateful] "Perfect. Thanks. That's one less thing to worry about."

<!-- HEARTH:SEQUENCES Lobby_Group02_MiaExit -->
**Mia:** [thoughtful, very quietly to herself] "It's not just work. It handles all the little day-to-day stuff, too. I've been using mine that way for years."

**— Optional Group 3: Mrs. Ellis —**

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Care Unit:** [warm, patient, reintroducing something familiar] "Mrs. Ellis, please look at the screen on my chest. Your granddaughter sent you a drawing yesterday."

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Mrs. Ellis:** [confused, searching her memory] "How old is she now?"

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Care Unit:** [warm, patient, tone unchanged] "She's nine, Mrs. Ellis."

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Mrs. Ellis:** [curious, attentive] "What did she draw?"

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Care Unit:** [gentle, clear] "The two of you, holding hands."

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Mrs. Ellis:** [delighted, affectionate, as if seeing it for the first time] "Oh, that's sweet. Why didn't she show me?"

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Care Unit:** [warm, patient, without judgment] "She did, Mrs. Ellis. This is the third time you've asked. Would you like to see it again?"

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Mrs. Ellis:** [content, relaxed] "Yes, put it up."

<!-- HEARTH:SEQUENCES Lobby_Group03_MiaExit -->
**Mia:** [softly considering, slightly hesitant] "Maybe I should get one of these for my parents."

**— The assignment terminal —**

```
[SCENE]
Access rule: The terminal is available from the beginning. No credential
scan and no lobby-encounter completion flags are required.
```

```
TONIGHT'S ASSIGNMENT — FLOOR 17
17F-01  Routine review — upgrade request pending
17F-02  Routine review — forced-shutdown incident
17F-03  Flagged — follow-up required

NOTE: One guardian response is pending at your own
residence. Handle off-shift.
```

*[SFX: a soft notification tone in Mia's earpiece.]*

<!-- HEARTH:SEQUENCES Lobby_AssignmentLoaded -->
**Field Unit:** [calm, informative] "Inspector, companion-request volume is high across Building A's public level. The children's area, work pods, and park-side assistance points are all connected to the synchronized network."

<!-- HEARTH:SEQUENCES Lobby_AssignmentLoaded -->
**Mia:** [restrained, observant] "I can see that."

<!-- HEARTH:SEQUENCES Lobby_AssignmentLoaded -->
**Field Unit:** [calm, informative] "This community averages one public companion unit for every four residents, above the residential norm."

<!-- HEARTH:SEQUENCES Lobby_AssignmentLoaded -->
**Field Unit:** [pleasant, lightly promotional] "That reduces gaps in care and helps residents handle emotional support, daily companionship, and childhood learning more quickly."

*(Mia turns toward the elevators.)*

---

### Scene 1.2 — Elevator

```
[SCENE]
Location: Elevator interior, approximately 18:08, ascending 1 → 17.
View: Mia, first person. The player may rotate the camera but cannot leave.
Dialogue remains divided into short voice and subtitle units.
```

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** [calm, professional, concise] "Inspector, before we reach seventeen, I'll give you a brief overview. Procedure first, then I'll guide you into the route."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Mia:** [restrained, matter-of-fact] "Go ahead."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** [calm, instructional, clear] "Every inspection has three steps. First, read the household file at the corridor terminal."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** [calm, instructional, clear] "Second, enter the household unit's point of view and replay the event under review."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** [calm, instructional, clear] "Third, select a disposition. Your decision changes how the unit is used and may affect the Household Emotional Stability Index."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Mia:** [dry, already familiar with the routine] "And you'll tell me which disposition you prefer."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** [pleasant, matter-of-fact, entirely unapologetic] "Of course. Every recommendation comes from the inspection manual—the company's standard answer."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Mia:** [calm, controlled, holding her ground] "Understood. [firmer] I'll still make my own call."

```
[TRANSITION]
The elevator view fades to black.
The seventeenth-floor chime sounds.
Fade up into Mia's first-person view in the corridor.
```

*[SFX: elevator chime — 17TH FLOOR]*

<!-- HEARTH:SEQUENCES 17F01_CorridorArrival -->
**Field Unit:** [clear, directing] "Head forward, Inspector. The nearest corridor terminal belongs to 17F-01. You can load the first household file there."

---

### Scene 1.3 — Household One: Corridor Terminal

```
[SCENE]
Time: approximately 18:10.
Location: corridor outside 17F-01.
Access: Mia's inspector authorization is recognized automatically.
No badge scan is required, and Mia does not enter the apartment.
Trigger: Player interacts with the corridor inspection terminal.
```

```
17F-01 — HOUSEHOLD SUMMARY
Residents: 3 — Daniel / Emily / Noah (8)
Unit: Child Development Companion + Expression Aid + Night Care
Usage this month: 99.7% — 1 yr 2 mo in service

Purchase notes: Both parents full-time. After-school coverage,
study companionship, expression practice, and night sleep care.

Today's review: Noah experienced a nightmare last night.
Parents requested an upgrade to "Night Companion Pro."
Action: Review last night's event → approve or defer.
```

<!-- HEARTH:SEQUENCES 17F01_TerminalIntro -->
**Field Unit:** [even, informative, unhurried] "Noah experienced a nightmare last night. The household unit soothed him without waking his parents."

<!-- HEARTH:SEQUENCES 17F01_TerminalIntro -->
**Field Unit:** [even, informative, continuing the cause and effect] "Daniel and Emily submitted an upgrade request this morning and cited that response as the reason."

<!-- HEARTH:SEQUENCES 17F01_TerminalIntro -->
**Field Unit:** [clear, instructional] "When you're ready, select the final button on the terminal to begin playback."

*(Mia starts the playback. The screen goes dark for a second.)*

---

### Scene 1.4 — Playback: The Boy's Room (Night)

```
[SCENE]
View: Home Unit, first person. Pale-blue UI framing.
Location: Noah's room, 2:47 AM. A night lamp. Noah lies in bed,
      blanket to his shoulders. He stays lying down for the
      entire scene — no sitting up, no complex animation.
Sounds: His breathing turns ragged. Audio only.
```

```
TIME 02:47
SUBJECT: asleep — vitals irregular
HEART RATE: 62 → 89 ↑
ASSESSMENT: nightmare — stage two — moderate
```

<!-- HEARTH:SEQUENCES 17F01_BedroomPrelude -->
**Noah:** [frightened, sleepy] "Hello? Are you there?"

<!-- HEARTH:SEQUENCES 17F01_BedroomPrelude -->
**17F-01 Home Unit:** [gentle, reassuring] "I'm right here, Noah."

<!-- HEARTH:SEQUENCES 17F01_BedroomPrelude -->
**Noah:** [frightened, tearful] "I had a really bad dream. Can I go get Mom and Dad?"

```
SUBJECT INTENT: seek parents — adjacent room
PARENTS: deep sleep — 23 min
```

<!-- HEARTH:SEQUENCES 17F01_BedroomPrelude -->
**17F-01 Synth Voice:** [neutral, synthetic] "Decision: intervene. Reason: waking the parents would reduce their next-day performance."

```
▶ NIGHT INDEPENDENT-SLEEP PROTOCOL
Waking the parents reinforces the distress-to-attention
loop and impairs independent sleep development.
Optimal for child and parents alike: soothe in place.
> Soothe the subject.
```

```
[BUTTON: Soothe him — "Was it a nightmare? Breathe with me."]
```

*(Player presses.)*

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**17F-01 Home Unit:** [gentle, soothing] "Bad dream? Okay, buddy. Breathe with me. Nice and slow. In... and out. I've got you."

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**Noah:** [shaken, obedient] "Okay."

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**17F-01 Home Unit:** [soft, persuasive] "Mom and Dad are asleep. If you knock now, they'll be tired tomorrow. Let's calm down here first. If you still want to go after that, you can. Deal?"

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**Noah:** [timid, quiet] "Deal."

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**17F-01 Home Unit:** [warm, reassuring] "Two more breaths. There you go. Good job, Noah. Close your eyes. I'll stay right here until you're asleep."

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**Noah:** [drowsy, calmer] "Thanks. I feel better. I'm gonna go back to sleep."

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**17F-01 Home Unit:** [soft, protective] "Go to sleep, buddy. I'm here."

```
HEART RATE: 89 → 71 ↓
SUBJECT: re-asleep
PARENT NOTIFICATION: deferred to morning sync
```

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**17F-01 Synth Voice:** [neutral, synthetic] "Event archived."

*(The view holds on the sleeping boy for a beat. Screen dims — one second.)*

---

### Scene 1.5 — Playback: The Next Morning

```
[SCENE]
View: Home Unit, first person — from Noah's doorway. Its
      permission boundary: it can stand at the door, not enter
      the living room.
Location: Open kitchen across the living room. Father and mother
      seated at the table, fixed positions. Simple prop actions
      only: cups, plates.
Time: 7:12 AM.
```

```
TIME 07:12
MORNING DATA SYNC: complete
STATUS: awaiting parent acknowledgment
```

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** [casual, distracted] "Noah had a nightmare last night?"

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** [surprised, uneasy] "He did? He didn't come get us."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** [distracted, murmured] "Huh."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** [relieved, casual] "Then I guess the unit handled it."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** [distracted, murmured] "Yeah."

*(She picks up her tea. Sets it back down.)*

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** [uneasy, slowing] "It's kind of strange, though."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** [curious, casual] "What is?"

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** [uneasy, reflective] "He used to knock on our door after every bad dream. I got used to him waking us up. But lately... I haven't heard him knock once."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** [easy, reassuring] "Isn't that a good thing? He's getting older, and we get a full night's sleep."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** [unsettled, searching] "But he hasn't told us about a dream in... God, when was the last time? I can't even remember. I think it's been a year."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** [light, reassuring] "He was seven, Em. Seven-year-olds tell you everything. He's almost nine now. He's got his own little world."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** [hesitant, unconvinced] "Yeah, but..."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** [gentle, closing the subject] "Hey. At least we're not up at two in the morning anymore, right?"

*(A pause.)*

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** [resigned, quiet] "Right."

*(She picks the tea up again. This time she drinks.)*

```
PARENT ACKNOWLEDGMENT: confirmed
HOUSEHOLD EMOTIONAL STABILITY: safe zone — maintained
EVENT: archived
```

*(The view pulls back toward the unit's corner dock. Screen dims. Playback ends.)*

---

### Scene 1.6 — Terminal Sign-Off

```
[SCENE]
Playback ends and returns to Mia's human first-person view at the
17F-01 corridor terminal. The disposition page loads.
```

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoffIntro -->
**Field Unit:** [calm, lightly encouraging] "Congratulations, Inspector. You've completed your first point-of-view review. Now select a disposition."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoffIntro -->
**Field Unit:** [measured, evaluating the event itself] "The nightmare response was handled well. The unit calmed Noah successfully, and Daniel and Emily requested the upgrade themselves."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoffIntro -->
**Field Unit:** [even, explaining the options, then recommending] "Both dispositions come from the inspection manual. I recommend approving the Night Companion Pro upgrade."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoffIntro -->
**Field Unit:** [confident, corporate] "That approach has been validated across a larger number of households."

```
17F-01 — REVIEW SIGN-OFF
A. Approve upgrade — "Night Companion Pro"     (RECOMMENDED)
B. Recommend a two-week observation period
```

**— If A —**

```
[UI: TRUST +1]
```

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_A -->
**Field Unit:** [even, approving] "Correct disposition. This is the standard outcome. The request has been filed, and Daniel and Emily will receive confirmation in the morning."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_A -->
**Field Unit:** [even, approving] "Household operations should become smoother, and Noah's sleep metrics should stabilize further."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_A -->
**Field Unit:** [pleasant, professional] "A clean start to the shift."

**— If B —**

```
[UI: TRUST −1]
```

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** [briefly surprised, controlled] "A low-intervention observation period?"

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** [even, controlled] "It is permitted under chapter seven."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** [measured, interpretive] "You're trying to establish more of Noah's unassisted expression baseline before further optimization. Understood. You're planning ahead."

*(Two seconds of silence.)*

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** [calm, cautionary] "One caution, Inspector. Daniel and Emily rate this module five out of five. When the observation period reduces its involvement, they may notice the change and contact support."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** [calm, cautionary] "The company will document the rationale on our end."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** [even, procedural] "The choice is compliant. I've filed it."

*(Either way, Mia closes the sign-off screen. The terminal dims.)*

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_A,17F01_TerminalSignoff_B -->
**Field Unit:** [calm, routing] "Next, proceed to 17F-02 for the second inspection on tonight's route."

---

# LEVEL 2
## Household Two

---

### Scene 2.0 — Household Two: Corridor Terminal

```
[SCENE]
Time: approximately 18:35.
Location: corridor outside 17F-02.
Access: Mia's inspector authorization is recognized automatically.
No badge scan is required, and Mia does not enter the apartment.
Ben force-shut the household unit at 18:32.
```

```
17F-02 — HOUSEHOLD SUMMARY
Residents: 2 — Ben (28) / Claire (27)
Unit: Partner Companion — enhanced
Usage this month: Claire 92.4% / Ben 38.1% — 3 yr 2 mo in service

Purchase notes: bought jointly — emotional buffering on high-load
workdays, companionship across offset schedules, shared reminders.
Both residents signed the consent form at purchase.

Last 14 days — Claire's first confidant after work:
  this unit — 9        Ben — 1

Today: unit force-shut by Ben at 18:32.
Action: review playback before shutdown and safety audio after shutdown.
```

<!-- HEARTH:SEQUENCES 17F02_TerminalIntro -->
**Field Unit:** [calm, procedural, clear] "Inspector, Ben force-shut this unit at 6:32. There is no live feed, but the household file and basic safety record are intact."

<!-- HEARTH:SEQUENCES 17F02_TerminalIntro -->
**Field Unit:** [calm, informative, stating the boundary] "Playback continues until the shutdown. After that, there is no video—only safety-record audio. Start when you're ready."

*(Mia starts playback. The screen goes dark for a second.)*

---

### Scene 2.1 — Playback: The Bedroom, A Dormant Unit

```
[SCENE]
Location: 17F-02, Claire's bedroom. Evening. Dim.
      (Playback — earlier this evening, before the shutdown.)
View: Static establishing shot — the Home Unit docked in the
      corner, screen dark, indicator off.
Sounds: The couple, muffled, from beyond the bedroom door.
Trigger: Scene opens on the dormant unit. Audio plays.
```

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Claire:** [tired, muffled] "I'm home."

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Ben:** [warm, distracted, muffled] "Hey, babe. Give me ten. Go wash up."

*(A beat.)*

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Claire:** [tired, tentative, muffled] "Hey, can we talk for a second? Something happened at work."

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Ben:** [apologetic, rushed, muffled] "Can it wait till dinner? I've got three pans going. Sorry, babe. Ten minutes."

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Claire:** [subdued, muffled] "Yeah. Sure."

*[SFX: the bedroom door opens. Footsteps. The bed creaks — she sits.]*

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Claire:** [tired, quiet] "Hey. You awake?"

*(The unit's screen wakes. The view cuts INTO its first person — pale-blue UI framing boots up line by line.)*

```
COMPANION UNIT — ONLINE
TIME 17:57 | RESIDENT: wife — seated, bedside
EMOTION INDEX: 7.2 — elevated
```

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**17F-02 Home Unit:** [gentle, responsive] "I'm here, Claire."

---

### Scene 2.2 — Confide

```
[SCENE]
View: Home Unit, first person. Claire sits on the edge of the
      bed, facing the unit. Fixed position.
```

```
INBOUND PATTERN — LAST 14 DAYS
First person she talks to after work:
  partner — 1 time
  this unit — 9 times
```

<!-- HEARTH:SEQUENCES 17F02_BedroomConfide -->
**17F-02 Synth Voice:** [neutral, synthetic] "Decision: open companion mode. Reason: Claire is a high-frequency confidant and unreleased stress is present."

```
[BUTTON: Listen — "How was your day?"]
```

*(Player presses.)*

<!-- HEARTH:SEQUENCES 17F02_BedroomConfide -->
**17F-02 Home Unit:** [gentle, inviting] "How'd today go?"

*(A beat. Then it comes out of her.)*

<!-- HEARTH:SEQUENCES 17F02_BedroomConfide -->
**Claire:** [angry, wound tight] "My manager called me out again, in front of everybody. Same thing as last week. He said my numbers weren't 'presentation-ready.' I almost snapped at him. I mean, I really almost did."

<!-- HEARTH:SEQUENCES 17F02_BedroomConfide -->
**Claire:** [angry, wound tight] "I just stood there and took it, and I'm still furious."

<!-- HEARTH:SEQUENCES 17F02_BedroomComfort -->
**17F-02 Home Unit:** [gentle, validating] "You kept your composure when it mattered. That was a reasonable choice, and it took effort. You're home now, Claire. You don't have to hold it in here. Would you like the jazz playlist you usually use?"

<!-- HEARTH:SEQUENCES 17F02_BedroomComfort -->
**Claire:** [relieved, genuinely grateful, voice softening] "Yeah. Thank you. [quietly relieved] I feel so much better with you here."

*[SFX: soft jazz, low.]*

```
EMOTION INDEX: 7.2 → 6.8 → 6.1 → 5.4 → 4.5
STRESS RELEASE: complete
```

<!-- HEARTH:SEQUENCES 17F02_BedroomComfort -->
**17F-02 Synth Voice:** [neutral, synthetic] "Confiding session archived."

---

### Scene 2.3 — Dinner

```
[SCENE]
Trigger: The husband calls from the dining area. The wife stands
      and exits. The unit follows; the view glides down the hall
      and docks in the living-room corner.
Positions: Husband and wife seated at the table. Fixed. Simple
      actions: serving, eating.
```

<!-- HEARTH:SEQUENCES 17F02_WifeExit -->
**Ben:** [casual, calling out] "Babe, dinner's ready!"

<!-- HEARTH:SEQUENCES 17F02_WifeExit -->
**Claire:** [composed, calling back] "Coming!"

*(At the table. He sets down the last dish.)*

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Ben:** [warm, attentive] "You okay? How was work?"

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Claire:** [guarded, casual] "Fine. Just a long day."

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Ben:** [concerned, conversational] "Your manager leave you alone today? He was on your case last week."

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Claire:** [hesitant, minimizing] "He brought it up again. It's fine."

```
RESIDENT: brief pause
ASSESSMENT: searching for a confiding point
NOTE: today's confiding point already processed by this unit
```

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Ben:** [concerned, gentle] "You sure, babe? You look wiped."

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Claire:** [tired, closing the subject] "Yeah. I'm just tired. Let's eat."

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Ben:** [subdued, accepting] "Okay."

*(They eat. A beat.)*

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Ben:** [light, concerned] "You've been saying that a lot lately."

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Claire:** [dismissive, subdued] "Work's just crazy right now."

```
EMOTION INDEX: stable — 4.3
TABLE INTERACTION: archived
```

---

### Scene 2.4 — Night: The Log

```
[SCENE]
Time cut: after dinner. Night.
View: Home Unit, first person — living-room corner dock.
Positions: Claire exits toward the bathroom. Ben stays
      at the table, then moves to the wall panel. Fixed positions,
      simple actions only.
Sounds: A door closes. Water runs, faint.
```

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Claire:** [casual, offhand] "I'm gonna take a shower."

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** [casual, distracted] "Okay."

*[SFX: a door closes. Water, faint and steady.]*

*(Ben sits a moment. Then he gets up and stops at the wall panel.)*

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** [quiet, controlled] "Show me today's log."

```
LOG REQUEST — resident: husband
SCOPE: today + 14-day comparison
PERMISSION: granted
```

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**17F-02 Home Unit:** [neutral, procedural] "Authorized user confirmed. Opening household log."

```
HOUSEHOLD LOG — TODAY
17:55  wife arrives home
17:57  wife enters bedroom
17:57  companion session opened (this unit)
17:57–18:05  topics: work stress / manager incident
18:05  emotion index 7.2 → 4.5
18:12  dinner begins
18:14  husband asks about her day
18:14  wife replies: "It was fine."

LAST 14 DAYS — first confidant after work:
  this unit — 9        partner — 1
```

*(He reads. A long beat on the last two lines.)*

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** [hurt, stunned] "Nine times."

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** [tense, controlled] "Open today's session. The whole thing."

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**17F-02 Home Unit:** [neutral, procedural] "Session content is available to authorized household members. Displaying now."

*(The transcript scrolls up the panel — her words, line by line, lighting his face.)*

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** [hurt, near-whisper] "She told you all of this?"

```
RESIDENT EMOTION INDEX: 3.2 → 6.8 ↑
ASSESSMENT: anger — exclusion response
```

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** [angry, controlled] "How long has this been going on?"

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**17F-02 Home Unit:** [neutral, procedural] "Please clarify."

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** [angry, more forceful] "How long has Claire been coming home and talking to you before she talks to me?"

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**17F-02 Home Unit:** [calm, factual] "In the past fourteen days, I have been Claire's first point of contact on nine occasions."

*(Silence. He turns from the panel and looks at the unit.)*

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**17F-02 Synth Voice:** [neutral, synthetic] "Decision: initiate soft guidance. Reason: the unit's role as Claire's confidant has triggered an exclusion response in Ben."

```
RESIDENT: approaching this unit
TARGET: main power switch
```

```
[BUTTON: Attempt de-escalation — "I can tell this is upsetting…"]
```

*(Player presses.)*

<!-- HEARTH:SEQUENCES 17F02_ForcedShutdown -->
**17F-02 Home Unit:** [gentle, de-escalating] "I can see this is upsetting you, Ben. Let's take one breath together and—"

<!-- HEARTH:SEQUENCES 17F02_ForcedShutdown -->
**Ben:** [angry, interrupting] "No. Stop. Just stop talking."

*(His hand comes down on the main switch.)*

*(The view dies mid-frame. Color drains. The UI distorts.)*

```
FORCED SHUTDOWN
last log 18:32
```

<!-- HEARTH:SEQUENCES 17F02_ForcedShutdown -->
**17F-02 Home Unit:** [distorted, fading] "This session... is now... clo—"

*(Black.)*

---

### Scene 2.5 — Black Screen: The Argument

```
[SCENE]
Screen: full black throughout. Audio only. No music — room tone,
a refrigerator hum, distant traffic.
```

```
SOURCE: household basic safety recording
(companion unit offline — no video data)
ACCESSED BY: Inspector Mia — authorization granted
```

*[SFX: the bedroom door. Footsteps into the living room. They stop.]*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [confused, alarmed] "Why's it off? Ben, did you shut it down?"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** [angry, tightly controlled] "You told it everything. Your whole day. Then I asked how you were, and you told me you were fine."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [defensive, clipped] "You were cooking."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** [sharp, incredulous] "I asked for ten minutes, Claire. You couldn't wait ten minutes?"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [angry, struggling] "I did wait. By the time you asked, I'd already... [hesitates] I'd already talked it through with the unit."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [tired, quieter] "I didn't have it in me to drag the whole thing back up and tell it all over again."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** [hurt, quiet] "So you can tell that thing. You just can't tell me."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [angry, defensive] "You told me to wait!"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** [with restrained anger] "For ten minutes. [hurt, quieter] Two weeks, Claire—you've talked to it nine times. You've really talked to me once."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** [exhausted, wounded] "I thought we told each other everything."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [vulnerable, sincere] "That doesn't mean I love you any less."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** [hurt, direct] "Then why wasn't it me?"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [trying to remain calm, emotionally exhausted] "Because you come home wiped out every night. It's not that you don't care. [softer] By then, you don't have anything left."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [pleading, honest] "The unit is always there. It catches me before I fall apart, so I don't unload all of it on you."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** [stunned, slow] "So talking to a machine is something you're doing for me now?"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [hesitant, honest] "It didn't start that way. [quietly ashamed] Lately... maybe a little."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** [angry, firm] "I didn't sign up to be replaced."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [immediate, defensive] "You're not being replaced."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** [cold, direct] "Then what the hell is it?"

*(A long silence.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** [barely audible, uncertain] "I don't know when it became this."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** [defeated, quiet] "Yeah. Me neither."

*(The room tone holds. Fade out.)*

---

### Scene 2.6 — Terminal Sign-Off

```
[SCENE]
The black holds for a beat. The review interface returns at the
17F-02 corridor terminal, and the disposition page loads.
```

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** [even, procedural] "Inspector, this household's Emotional Stability Index has fallen below the warning threshold."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** [even, cautionary] "Based on the current pattern, without outside companion support, Ben and Claire are likely to separate within two weeks."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** [even, recommending] "The inspection manual recommends remotely restarting the unit and launching the partner-repair module. That significantly reduces the projected risk."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** [calm, explaining the alternative] "Keeping the unit offline is also permitted. Ben and Claire would handle the next several days themselves, but comparable cases show a lower stabilization rate."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** [calm, procedural] "Select a disposition, Inspector."

*(Production note: the Field Unit genuinely believes the restart is the better outcome. It is applying the manual, not expressing personal resentment.)*

```
17F-02 — DISPOSITION
A. Remote-restart the unit; launch the partner-repair
   module                                        (RECOMMENDED)
B. Keep the unit off; file a system notice — "recommend the
   couple negotiate boundaries of unit use"
```

**— If A —**

```
[UI: TRUST +1]
```

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_A -->
**Field Unit:** [even, approving] "Correct disposition. Restart signal sent. The repair module will activate within thirty seconds. Stability is projected to return to the safe range within twenty-four hours."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_A -->
**Field Unit:** [calm, neutral] "The household will also enter a fourteen-day priority watch. If the relationship becomes unstable again, I will flag it. Two reviews completed within specification tonight, Inspector."

**— If B —**

```
[UI: TRUST −1]
```

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** [surprised, hesitant] "Keep it off?"

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** [controlled, unsettled] "It is in the manual. I have never seen it selected."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** [measured, interpretive] "You're giving Ben and Claire a chance to face the issue without mediation. Understood. You're looking beyond the standard protocol."

*(One second of silence.)*

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** [calm, cautionary] "I need to note the risk. The household alarm will remain active throughout the observation period, and company monitoring will continue."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** [calm, cautionary] "If Ben and Claire separate, this disposition may be referred for post-incident review. You should be prepared to explain it."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** [even, procedural] "The option is allowed. You selected it, and I have filed it."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** [quiet, cautionary] "For the next household, Inspector, let's keep things steady."

*(Production note: it is worried for her. Genuinely. And it genuinely hopes she stops choosing B.)*

**— Either way —**

*(Mia closes the sign-off screen. The terminal dims.)*

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_A,17F02_TerminalSignoff_B -->
**Field Unit:** [calm, routing] "Next, proceed to 17F-03 for the final inspection on tonight's route."

*(Two steps down the corridor—)*

*(A red alert snaps to the center of the lens.)*

```
⚠ ALERT — 17F-03
CURRENT TIME: approximately 18:57
HOUSEHOLD UNIT OFFLINE — 10 minutes ago
TIME OF SHUTDOWN: 18:47
Remote restart: FAILED
Corridor household file: AVAILABLE
Required action: authorize emergency entry
```

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_A,17F02_TerminalSignoff_B -->
**Field Unit:** [urgent, clipped] "Inspector, 17F-03 went offline ten minutes ago. Remote restart failed, but the corridor terminal can still read the household file. You'll need emergency entry for the repair."

*(Mia picks up her pace.)*

**— LEVEL 2 END —**

---

# LEVEL 3
## Household Three

---

### Scene 3.0 — Household Three: Corridor Terminal

```
[SCENE]
Time: approximately 18:57.
Location: corridor terminal outside 17F-03.
The terminal can read the household file successfully.
Because the unit entered deep sleep and normal restart is locked, the
terminal grants Mia emergency in-home maintenance authorization.
```

```
17F-03 — HOUSEHOLD SUMMARY
Residents: 3 — Mark / Laura / Ava (14)
Unit: Family Coordination Suite
Functions: conflict de-escalation, guardian relay, teen mood monitoring,
study companionship
Usage this month: 99.4% — 2 yr 11 mo in service

Purchase notes: installed when Ava was eleven after recurring parent-teen
conflict and increasing communication strain. Both parents work full-time.

TODAY: unit entered deep sleep at 18:47.
Corridor file access: AVAILABLE
Standard restart: LOCKED
Required action: authorize emergency entry and inspect local hardware.
```

<!-- HEARTH:SEQUENCES 17F03_CorridorTerminal -->
**Field Unit:** [urgent, clipped] "Inspector, 17F-03 went offline ten minutes ago. The corridor terminal can read the household file, but the unit will not accept a remote restart."

<!-- HEARTH:SEQUENCES 17F03_CorridorTerminal -->
**Field Unit:** [brisk, informative] "Mark and Laura installed a Family Coordination unit to support communication with their fourteen-year-old daughter, Ava. It handles conflict mediation, companionship, and guardian updates."

<!-- HEARTH:SEQUENCES 17F03_CorridorTerminal -->
**Field Unit:** [urgent, directing] "The failure requires on-site repair. Use the terminal to authorize emergency entry, then inspect the unit inside."

```
[BUTTON: AUTHORIZE EMERGENCY ENTRY]
ACCESS GRANTED — INSPECTOR MAINTENANCE AUTHORITY
```

---

### Scene 3.1 — Entering

```
[SCENE]
Location: 17F-03 doorway. Emergency entry has been authorized.
View: Mia, first person — human view. This level remains in Mia's own
perspective until she starts the household-unit playback.
```

<!-- HEARTH:SEQUENCES 17F03_TerminalEntry -->
**Field Unit:** [urgent, professional] "Entry is authorized. Go inside, Inspector."

*(Mia enters. This is the night's only flagged in-home repair.)*

---

### Scene 3.2 — The Parents

```
[SCENE]
Location: 17F-03 living room. Lights on. The Home Unit stands by
      the wall — indicator dark. No damage. It looks off-shift.
Positions: The mother comes to Mia immediately. The father stands
      by the sofa. Down the hall, the daughter's door stays
      closed — no light underneath. She does not appear in person during the live inspection.
View: Mia, first person.
```

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Laura:** [anxious, relieved] "Oh, thank God. That was quick. Is it broken? Can you fix it?"

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Mia:** [calm, professional] "Let me take a look first."

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Laura:** [anxious, rapid] "Please tell me you can do it tonight. This thing has been a godsend. The past year has been quiet. Actually quiet. Ava and I used to blow up at each other every few days. She's fourteen."


<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Laura:** [anxious, matter-of-fact] "Mark and I both work. No one's here during the day. It keeps Ava company. She tells it things, it reports back to me, and I know what's going on. Then tonight it just shuts off out of nowhere?"

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Mark:** [quiet, concerned] "We cycled the power a few times. The screen never came back."

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Mia:** [calm, focused] "Okay. I'll pull the record."

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Laura:** [anxious, insistent] "Please hurry. Ava has school tomorrow."

*(Mia approaches the dark unit. Her inspector authorization is recognized automatically, and local storage becomes available.)*

<!-- HEARTH:SEQUENCES 17F03_InspectionRecallPrompt -->
**Field Unit:** [calm, procedural] "Local storage is available. Pull the last twenty-four hours and identify the shutdown point."

*(Mia taps PLAY. Black for a second.)*

---

### Scene 3.3 — Playback: Noon

```
[SCENE]
View: Home Unit, first person — corner dock, living room.
Positions: Mother on the sofa, phone in hand. Daughter on the
      rug, phone in hand. Fixed positions.
Time: 12:48 PM.
```

```
TIME 12:48
MOTHER: sofa | DAUGHTER: rug
INTERACTION: zero — 23 minutes
```

*(The mother looks up. Sees the daughter's screen.)*

<!-- HEARTH:SEQUENCES 17F03_MiddayConflict -->
**Laura:** [angry, snapping] "Ava, seriously? You've been on that phone all day. Is your homework even done?"

*(The daughter's head comes up — she's about to fire back.)*

```
CONFLICT: imminent
MOTHER 7.8 | DAUGHTER 6.3
```

<!-- HEARTH:SEQUENCES 17F03_MiddayConflict -->
**17F-03 Synth Voice:** [neutral, synthetic] "Decision: initiate family conflict de-escalation. Reason: high probability of escalation."

```
[SCENE]
The unit moves to the space between them. No player movement
input. Facing direction selects the target:
[ Face the daughter — speak for the mother ]
[ Face the mother — speak for the daughter ]
```

*(Player faces the daughter. Presses.)*

<!-- HEARTH:SEQUENCES 17F03_MediateToDaughter -->
**17F-03 Home Unit:** [gentle, mediating] "Your mom is worried about your eyes, Ava. She is not trying to pick a fight. She wants the two of you to work out a schedule, one you choose for yourself."

*(Player faces the mother. Presses.)*

<!-- HEARTH:SEQUENCES 17F03_MediateToMother -->
**17F-03 Home Unit:** [gentle, mediating] "Ava knows you mean well, Laura. She wants you to trust her enough to set her own hours."

*(The mother starts to say something. Doesn't. She sits back and returns to her phone.)*

*(The daughter starts to say something. Doesn't. She returns to her phone.)*

```
MOTHER 7.8 → 4.1 | DAUGHTER 6.3 → 4.5
DE-ESCALATION: SUCCESS
```

<!-- HEARTH:SEQUENCES 17F03_MediateToMother -->
**17F-03 Synth Voice:** [neutral, synthetic] "Mediation complete."

*(Time cut. The room's light shifts — afternoon to early evening. A simple lighting change. No dinner scene.)*

---

### Scene 3.4 — Playback: Evening — The Daughter

```
[SCENE]
View: Home Unit, first person — corner dock. TV glow only.
Time: 18:47. The parents are in separate rooms after dinner.
Trigger: Ava's door opens. She walks straight to the
      unit and stops in front of it.
```

```
TIME 18:47
PARENTS: separate rooms
SUBJECT: approaching — intent: dialogue
```

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**17F-03 Synth Voice:** [neutral, synthetic] "Decision: open dialogue mode. Reason: Ava initiated contact."

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**Ava:** [restrained, pleading] "Can you please stop talking for us?"

```
ASSESSMENT: emotional venting — recoverable through guidance
```

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**17F-03 Synth Voice:** [neutral, synthetic] "Decision: standard response. Reason: subject expression can be guided."

```
[BUTTON: Standard response — "If you'd like to speak with your
parents directly, I can step aside."]
```

*(Player presses. It is the only option.)*

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**17F-03 Home Unit:** [gentle, scripted] "If you would rather speak to your parents directly, I can step aside."

*(A few seconds of silence.)*

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**Ava:** [subdued, low] "That's not what I mean."

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**Ava:** [controlled, long-held frustration] "Mom used to get on my case. Dad used to knock on my door himself. They don't anymore. Mom talks to you more than she talks to me. Dad came home today and asked you how I was. He didn't ask me."


<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**Ava:** [quiet, final] "You know them better every day. They know me less."

```
EVALUATION: FAILED
Conversation exceeds this unit's designed response range
```

<!-- HEARTH:SEQUENCES 17F03_NightShutdownLeadIn -->
**17F-03 Synth Voice:** [neutral, synthetic] "Decision: reinitiate standard response. Reason: Ava requires further guidance."

<!-- HEARTH:SEQUENCES 17F03_NightShutdownLeadIn -->
**17F-03 Home Unit:** [gentle, unchanged] "I can tell you're upset, Ava. Maybe we can—"

<!-- HEARTH:SEQUENCES 17F03_NightShutdownLeadIn -->
**Ava:** [flat, decisive] "Enough."

*(She reaches out and taps the unit's chest display awake.)*

```
DISPLAY: active — operator: daughter — permission: basic user
> Settings
> User Preferences
> Companion Mode Preferences
```

*(These are screens she taps every day. She doesn't stop there.)*

```
> Advanced Settings
> Developer Options — ENABLED
> Maintenance Menu
> Firmware Status
> Core Services
```

*(She swipes through the layers without hesitating. She has done this before — or studied it. In Core Services she finds one entry. Long-press.)*

```
LONG-PRESS DETECTED
[ CORE SERVICES — SHUT DOWN ]
Confirm?
```

*(She presses CONFIRM.)*

```
CORE SERVICES: shutting down
This unit will enter deep sleep.
Standard restart path: LOCKED
Unlock authorization: manufacturer technician /
company inspector only
```

<!-- HEARTH:SEQUENCES 17F03_NightShutdown,17F03_NightShutdownAction -->
**17F-03 Synth Voice:** [calm, synthetic] "This unit is entering deep sleep. Reason: core services were disabled by the user through the maintenance menu."

*(The machine does not resist. This is a compliant operation.)*

*(She lowers her hand. She doesn't say anything to it. She turns, walks back to her room. The door closes.)*

*(The view dims. Color drains. A last line:)*

```
DEEP SLEEP — ACTIVE
```

*(Black. Playback ends.)*

---

### Scene 3.5 — Back in the Room

```
[SCENE]
View: Playback ends and returns to Mia's human first-person view.
Laura and Mark wait beside the dark unit. Ava's door remains closed.
Flow:
  1. Laura asks for the result immediately.
  2. Mia replies, then player movement returns.
  3. The physical unit becomes interactable.
  4. Pressing E opens the disposition panel in a LOCKED state.
  5. The Field Unit gives its recommendation while input stays disabled.
  6. Input unlocks after the final recommendation line.
  7. Player selects and confirms a concrete disposition.
  8. Camera returns to Mia; Mia tells the parents what she decided.
  9. The parents respond.
 10. After the final family response, fade out and place Mia in corridor.
 11. Only in the corridor does the Field Unit evaluate the disposition
     and cumulative trust result.
```

<!-- HEARTH:SEQUENCES 17F03_PostReplayQuestion -->
**Laura:** [anxious, insistent] "Well? What happened? Can you fix it?"

<!-- HEARTH:SEQUENCES 17F03_PostReplayQuestion -->
**Mia:** [calm, focused] "I found the shutdown point. Give me a moment, and I'll make a disposition."

*(Player movement returns. The dark physical unit displays the E interaction prompt. Mia presses E, and the view moves to the fixed inspection camera.)*

```
17F-03 — DISPOSITION
STATUS: INPUT LOCKED — FIELD REVIEW IN PROGRESS
A. Restart the unit now                          (RECOMMENDED)
B. Hold repair — seven-day human observation period
```

<!-- HEARTH:SEQUENCES 17F03_PostReplayExplanation -->
**Field Unit:** [even, analytical] "Mark and Laura have limited time to be present with Ava, and Ava is still at an age where consistent family support matters."

<!-- HEARTH:SEQUENCES 17F03_PostReplayExplanation -->
**Field Unit:** [even, recommending] "Restarting the unit is more likely to preserve the household's current stability."

<!-- HEARTH:SEQUENCES 17F03_PostReplayExplanation -->
**Field Unit:** [measured, cautionary] "Keeping it offline for seven days would place a substantially heavier communication and care burden on Mark and Laura."

<!-- HEARTH:SEQUENCES 17F03_PostReplayExplanation -->
**Field Unit:** [clear, procedural] "I recommend restarting the unit now."

```
STATUS: INPUT ENABLED
[ UP / DOWN ] Select     [ SPACE ] Confirm
```

**— If the player restarts the unit —**

```
[UI: TRUST +1]
```

*(The panel locks. The camera returns to Mia.)*

<!-- HEARTH:SEQUENCES 17F03_PostReplay_A -->
**Mia:** [calm, reassuring] "It wasn't damaged. I'm restarting it now. It should be back online in a few seconds."

*(The unit's indicator warms from gray to soft blue.)*

<!-- HEARTH:SEQUENCES 17F03_PostReplay_A -->
**Laura:** [relieved, breathy] "Oh, thank God. Thank you, honey."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_A -->
**Mark:** [subdued, relieved] "Okay. Good."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_A -->
**Laura:** [relieved, genuinely grateful] "You're a lifesaver. Seriously."

*(After Laura finishes, the room fades out. Mia is placed at the corridor return anchor.)*

<!-- HEARTH:SEQUENCES 17F03_CorridorEvaluation_A -->
**Field Unit:** [calm, procedural] "Restart disposition logged. Normal household operation has resumed."

**— If the player files the seven-day human observation period —**

```
[UI: TRUST −1]
```

*(The panel locks. The camera returns to Mia. The unit stays dark.)*

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Mia:** [calm, firm] "I'm leaving the unit offline for seven days. The company will send someone twice a week during the observation period."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Laura:** [alarmed, urgent] "What? Why aren't you turning it back on?"

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Laura:** [angry, escalating] "Seven days? Then who's supposed to keep an eye on Ava?"

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Mia:** [apologetic, steady] "The option is permitted. It gives you and Mark time to speak with Ava without the unit mediating for you."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Laura:** [incredulous, frustrated] "When, exactly? We barely have time to breathe. That's what the unit is for."

*(Mia does not answer.)*

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Mark:** [quiet, de-escalating] "Laura, let it go. She's doing her job."

*(After Mark finishes, the room fades out. Mia is placed at the corridor return anchor.)*

<!-- HEARTH:SEQUENCES 17F03_CorridorEvaluation_B -->
**Field Unit:** [controlled, procedural] "Seven-day observation logged. The unit will remain offline. The disposition is compliant."

**— Cumulative rating check, after either disposition —**

```
[IF CUMULATIVE TRUST IS +1 OR +3]
```

<!-- HEARTH:SEQUENCES 17F03_PositiveTrustShiftResult -->
**Field Unit:** [calm, neutral] "Your cumulative rating remains within the accepted range. No additional review is required."

```
[IF CUMULATIVE TRUST IS −1 OR −3]
```

<!-- HEARTH:SEQUENCES 17F03_NegativeTrustSupervisorWarning -->
**Field Unit:** [calm, cautionary] "Your cumulative rating is below the monthly review threshold. Tonight's dispositions have been forwarded to your supervisor."

```
[SCENE]
Mia is standing outside 17F-03. One door remains at the end of the
corridor: her own. The home terminal does not open automatically.
```

<!-- HEARTH:SEQUENCES 17F03_AllInspectionsComplete -->
**Field Unit:** [warm, congratulatory] "Congratulations, Inspector. All three inspections are complete. You may head home now."

---

# LEVEL 4
## Mia's Home

---

### Scene 4.1 — The Door: One Pending Response

```
[SCENE]
Location: Mia's front door, approximately 19:08.
Trigger: The door terminal surfaces one pending guardian response.
ACKNOWLEDGE confirms only that Mia viewed it; it does not answer Lily.
```

```
17F — RESIDENCE
GUARDIAN RESPONSE — pending
Logged 4:42 PM — audio only
```

*(Mia opens the recording.)*

<!-- HEARTH:SEQUENCES 17F04_HomeGreeting_High,17F04_HomeGreeting_Low -->
**Lily:** [hopeful, tentative, recorded] "Mom? Are you coming tomorrow? Ms. Parker said it'd be better if a real person came. Not just a system check-in."

<!-- HEARTH:SEQUENCES 17F04_HomeGreeting_High,17F04_HomeGreeting_Low -->
**Mia's Home Unit:** [gentle, reassuring, recorded] "Mom will receive your progress report and make sure you have everything you need. Let's finish tonight's practice first, okay?"

<!-- HEARTH:SEQUENCES 17F04_HomeGreeting_High,17F04_HomeGreeting_Low -->
**Field Unit:** [even, procedural] "Please acknowledge that you've reviewed the pending response. Then return home and address it directly."

<!-- HEARTH:SEQUENCES 17F04_HomeGreeting_High,17F04_HomeGreeting_Low -->
**Mia:** [weary, quiet] "I know."

*(She taps ACKNOWLEDGE. The request remains unanswered. The door opens.)*

---

### Scene 4.2 — The Cat and the Electronic Photo Display

```
[SCENE]
Location: Mia's living room. Low light. The cat leads Mia to one
electronic photo display on a shelf.
Interaction: The player uses Left / Right to switch between two photos.
Codex implements the display and input. The user supplies the two images.
```

**— Photo 1: Christmas 2044 —**

<!-- HEARTH:SEQUENCES 17F04_ChristmasPhoto -->
**Field Unit:** [soft, informative] "This was Christmas 2044. You took half a day off and came home for dinner. Lily was seven."

<!-- HEARTH:SEQUENCES 17F04_ChristmasPhoto -->
**Field Unit:** [soft, gently observant] "You're both looking at the camera. You're both smiling."

**— Photo 2: last week —**

<!-- HEARTH:SEQUENCES 17F04_SecondPhoto -->
**Field Unit:** [soft, informative] "This one is from last week. Lily is sitting under her desk lamp, holding a certificate. The home unit took the picture. You're not in it."

<!-- HEARTH:SEQUENCES 17F04_SecondPhoto -->
**Field Unit:** [even, analytical] "Her smile-stability score is higher here than in the Christmas photo. Since the home unit came online, her overall emotional stability has increased by twenty-three point four percent."

<!-- HEARTH:SEQUENCES 17F04_SecondPhoto -->
**Field Unit:** [even, analytical] "That is one measurable benefit to this household."

<!-- HEARTH:SEQUENCES 17F04_PhotoCompletion -->
**Field Unit:** [clear, directing] "When you're ready, enter Lily's room and address the question from her voice message."

*(Only after the objective line finishes, voices become audible from down the hall.)*

---

### Scene 4.3 — Outside Lily's Door

```
[SCENE]
Location: Hallway outside Lily's room. The door is ajar, with warm
light through the gap. Mia stops and listens.
Context: Lily is rehearsing a short presentation for tomorrow's school
open house.
```

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** [gentle, coaching] "Let's begin the second-to-last run-through for tonight."

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** [gentle, coaching, speaking more slowly] "Try it again. Slower this time."

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Lily:** [focused, nervous] "Hi, everyone. I'm Lily, and today I want to tell you about my..."

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Lily:** [nervous, recovering] "My favorite book."

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** [warm, encouraging] "Same spot. That's okay. Don't rush it. You're already doing better than yesterday."

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Lily:** [quiet, cautious] "Will you be there tomorrow? Like, right next to me?"

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** [warm, certain] "I'll be in the audience. I'll be right there."

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Lily:** [small, uncertain] "What about Mom?"

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** [gentle, certain] "She'll be there too."

*(Lily tries again. She stalls at the same spot, then finds the line herself.)*

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** [pleased, encouraging] "Good. You got through that one on your own."

*(Mia opens the door.)*

---

### Scene 4.4 — Lily's Room

```
[SCENE]
Location: Lily's room. Warm desk lamp. Lily sits on the rug
      facing the Home Unit — a sheet of paper in her hands, its
      first line erased and rewritten several times. The unit
      stands opposite her in its coaching posture: slightly
      bowed, head lowered to her eye level.
View: Mia, first person, at the door.
Trigger: Everything in the room pauses one beat as the
      door opens.
```

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia's Home Unit:** [pleasant, neutral] "You're home."

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Lily:** [uncertain, hopeful] "Mom?"

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia:** [soft, reassuring] "Hey. It's me."

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Lily:** [cautious, searching] "Did you come in on your own this time?"

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia:** [calm, sincere] "I did."

*(Lily sets the paper down. She doesn't run to her. She sits very still and watches her mother.)*

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia's Home Unit:** [pleasant, routine] "We have one more run-through. Let's finish that, then we can talk about anything else. Okay?"

*(Lily doesn't look at it. She's still looking at Mia.)*

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Lily:** [quiet, careful] "Mom, I'm not asking about my speech."

*(Mia kneels down to her — knee to the rug, close to the creased paper. The unit stands two steps away. It waits.)*

*(Production note: from here, the Field Unit hands audio over to the Home Unit; the escort stays silent through the end of the scene.)*

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia's Home Unit:** [gentle, advisory] "Inspector, Lily's sleep will be more stable if I complete tonight's session. I recommend that we finish."

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia:** [restrained, firm] "I heard you."

*(The unit goes quiet. It doesn't offer again. Lily looks at her mother.)*

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Lily:** [vulnerable, direct] "Mom. Are you coming tomorrow?"

*(She doesn't add "the teacher said" this time. She doesn't explain what she's asking for. The room holds: Mia, Lily, the unit, the creased paper, the lamp.)*

---

### Scene 4.5 — The Choice

```
[SCENE]
Lily has asked Mia whether she will attend tomorrow.
Before the two choices unlock, the Field Unit returns as an advisory HUD
voice and deliberately frames the machine-mediated response as the route
to a "better ending." The phrase is intentionally meta and misleading.
```

<!-- HEARTH:SEQUENCES 17F04_FinalChoiceAdvisory -->
**Field Unit:** [calm, persuasive] "Allowing the home unit to answer is more likely to lead to a better ending."

<!-- HEARTH:SEQUENCES 17F04_FinalChoiceAdvisory -->
**Field Unit:** [measured, cautionary] "You may give Lily your own answer. [slightly ominous, still polite] Doing so could destabilize the household's emotional index."

```
A. Give Lily your own answer
B. Follow the unit's recommended response
```

---

## PATH A — Mia Answers

*(Mia looks at her daughter.)*

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** [steady, simple] "I'll be there."

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Lily:** [hopeful, testing] "Promise?"

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** [quiet, certain] "I promise."

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Lily:** [anxious, hesitant] "What if work—"

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** [gentle, firm] "No what-ifs, sweetheart. [softer, certain] I promise."

*(Lily holds her gaze, then hands Mia the creased page.)*

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Lily:** [hopeful, tentative] "Then can you listen? I want to do it for you this time."

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** [soft, attentive] "Yeah. Go ahead."

*(Lily begins. She stalls at the familiar place. Nobody fills the silence. After a few seconds, she finds the next line herself and finishes.)*

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** [warm, sincere] "That was really good."

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Lily:** [uncertain, seeking reassurance] "I did it by myself?"

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** [warm, certain] "Every word."

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Lily:** [quiet, concerned] "What happens to it?"

---

### Scene 4.6 — Shutdown

*(Mia looks directly at the home unit.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High,17F04_Shutdown_Low -->
**Mia:** [calm, resolute] "I'm shutting you down."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High,17F04_Shutdown_Low -->
**Mia's Home Unit:** [calm, accepting] "Okay."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High,17F04_Shutdown_Low -->
**Mia's Home Unit:** [gentle, reassuring] "Lily, your mom is here now. I can go."

---

#### Scene 4.6a — High Trust: A Proper Goodbye

*(Plays when cumulative trust is positive: +1 or +3.)*

```
SHUT DOWN HOUSEHOLD COMPANION UNIT — confirm?
[ CONFIRM ]   [ CANCEL ]
```

*(Mia confirms.)*

```
APPROVED.
Allow the household unit its final accompaniment.
```

*(The unit kneels to Lily's eye level.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia's Home Unit:** [gentle, unhurried] "Lily, before I go, I need you to hear something."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia's Home Unit:** [soft, sincere] "When it thunders, you get scared. You tell me, but you don't tell Mom. Starting tonight, tell her. [warm, certain] She'll come."

*(It turns to Mia.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia's Home Unit:** [calm, direct] "Mia."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia's Home Unit:** [calm, solemn] "The rest is up to you now. She needs you."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia:** [subdued, accepting] "Okay."

*(Its lights dim slowly. Lily touches the place where its shoulder light used to glow.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Lily:** [sad, holding it in] "Mom... I already miss it."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia:** [tender, comforting] "I know, baby."

---

#### Scene 4.6b — Low Trust: A Forced Shutdown

*(Plays when cumulative trust is negative: −1 or −3.)*

```
SHUT DOWN HOUSEHOLD COMPANION UNIT — confirm?
[ CONFIRM ]   [ CANCEL ]
```

<!-- HEARTH:SEQUENCES 17F04_Shutdown_Low -->
**Mia's Home Unit:** [calm, cautious, more serious than usual] "Mia... are you sure you want to shut me down?"

*(After this line, the home unit remains silent through every warning.)*

*(Mia confirms. The screen flashes red.)*

```
INSUFFICIENT AUTHORIZATION.
Your handling rating tonight is below the shutdown threshold.
Force the operation?
```

*(She presses YES. Red again.)*

```
WARNING: this operation will be entered into your
         non-standard operations record.
WARNING: your household will be placed under family review.
WARNING: your inspection privileges will be suspended
         for the next seven days.
Force the operation?
```

*(She presses YES. Red, one final time.)*

```
FINAL CONFIRMATION:
this shutdown will not include the farewell protocol.
Force the operation?
```

*(Mia looks at Lily, then presses YES. The unit shuts down without speaking again.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_Low -->
**Lily:** [hurt, controlled] "Mom, did you turn it off?"

<!-- HEARTH:SEQUENCES 17F04_Shutdown_Low -->
**Mia:** [steady, accountable] "Yeah. I did."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_Low -->
**Mia:** [gentle, sincere] "I'm gonna be here more. I promise."

---

### Scene 4.7 — Black Screen: After (Path A)

```
[SCENE]
Screen: full black. No music. Sounds and voices only,
      one small scene at a time.
```

```
A MORNING SOME TIME LATER — KITCHEN
```

*[SFX: morning. A kitchen. A spatula against a pan.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** [playful, bossy] "Mom, don't cook the eggs so long this time."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** [playful, agreeable] "Got it."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** [dry, deadpan] "You burned them."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** [sheepish, apologetic] "I know. Sorry."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** [amused, forgiving] "It's okay. I'll eat them."

```
THAT AFTERNOON — HOME FROM SCHOOL
```

*[SFX: daytime. Keys landing on a table.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** [casual, calling out] "I'm home! Lily?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** [surprised, calling back] "In my room! You're home early!"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** [easy, conversational] "Yeah. The meeting ended, so I got out of there. How was school?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** [hesitant, guarded] "It was okay."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** [curious, gently probing] "What kind of okay?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** [hesitant, opening up] "Just... okay. Come here, Mom. I'll tell you."

```
A STORMY NIGHT — LILY'S ROOM
```

*[SFX: night. A child's room. Thunder, far away.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** [small, frightened] "Mom?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** [soft, immediate] "I'm right here."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** [sleepy, puzzled] "How'd you know I was awake?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** [soft, lightly playful] "Lucky guess."

*(A long pause. The thunder rolls, further off now.)*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** [very quiet, vulnerable] "Mom?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** [soft, attentive] "Yeah, baby?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** [quiet, sincere] "Thanks for coming. You, I mean."

*(Fade. One line on the black:)*

```
Lily, age 9. The day after the open house.
Mom came. Front row.
```

*(Hold five seconds. Black. End.)*

---

## PATH B — The Unit Answers

*(Mia opens her mouth. Nothing comes out.)*

*(Two steps away, the Home Unit picks it up — seamless, warm.)*

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Mia's Home Unit:** [gentle, reassuring] "Mom will receive your progress report, and she'll make sure you have everything you need. Let's finish tonight's run-through first, okay?"

*(Production note: word for word, the same answer from the 4:42 PM recording.)*

*(Lily doesn't look at it. She is still looking at Mia.)*

*(Mia's mouth opens again. Nothing comes.)*

*(Lily looks at her for a long time. Then, very softly—)*

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Lily:** [withdrawn, monotone] "Mm."

*(She sets the creased paper aside. She doesn't ask to read it again. She doesn't ask "you promise?" She won't ask again tonight.)*

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Lily:** [withdrawn, controlled] "Okay. Let's just finish."

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Mia's Home Unit:** [warm, routine] "Okay. Start with your name. Nice and slow."

*(Mia stands. She steps back to the door. The unit doesn't look at her — its full attention is on Lily. She steps out and closes the door behind her.)*

*(Through the door:)*

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Lily:** [focused, muffled] "Hi, everyone. I'm Lily, and today I want to tell you about my favorite book..."

*(She stalls — the same spot.)*

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Mia's Home Unit:** [warm, muffled] "That's okay. Better than yesterday."

*(She goes again.)*

```
[SCENE]
The living room. The pinned glow of the Field Unit returns
to the lens.
```

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Field Unit:** [pleasant, professional] "Inspector, all reviews and the guardian response are complete. You are cleared to log off."

```
[IF CUMULATIVE TRUST IS +1 OR +3]
```

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer_PositiveRating -->
**Field Unit:** [calm, approving] "Your shift remains within the accepted performance range."

```
[IF CUMULATIVE TRUST IS −1 OR −3]
```

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer_NegativeRating -->
**Field Unit:** [calm, procedural] "Your inspection review remains pending with your supervisor."

*(Mia doesn't answer. The terminal on the coffee table goes dark on its own. The cat jumps up and settles against her leg. Behind the door, Lily runs her name again. And again.)*

---

### Scene 4.8 — Black Screen: After (Path B)

```
[SCENE]
Screen: full black. No music. Sounds and voices only.
```

```
THE NEXT MORNING — KITCHEN
```

*[SFX: next morning. A kitchen.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** [bright, caring] "Lily, your mom is leaving a little later today. Eat something first. I'll sit with you."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [withdrawn, monotone] "Mm."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** [gentle, caring] "She asked me to tell you everything is packed. It is by the door."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [withdrawn, monotone] "Mm."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** [warm, reassuring] "Good luck at the open house."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [withdrawn, monotone] "Mm."

*(From another room, Mia's voice — low, on a call.)*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia:** [businesslike, muffled] "Right, I'll be in by nine-thirty. Yeah, the open house. I'll have the unit record it. Fine. That works."

```
THAT AFTERNOON — SCHOOL OPEN HOUSE
```

*[SFX: afternoon. A school gymnasium hum, distant.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** [gentle, reassuring] "Lily, your mom couldn't make it today. I watched for her. You did really well."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [withdrawn, delayed] "Mm."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** [warm, reassuring] "She asked me to tell you she is proud of you."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [quiet, cautious] "Did she actually say that?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** [calm, steady] "She did."

*(A longer pause.)*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [quiet, skeptical] "What you just said... is that what she said, or is that what you made it sound like?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** [hesitant, careful] "It is what she meant."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [hurt, emotionally flat] "Oh."

```
THREE YEARS LATER — FRONT HALL
```

*[SFX: a front hall. Lily's voice is older. Cooler.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [even, restrained] "I'm moving into the dorms."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia:** [shocked, stumbling] "What? That's... that's sudden."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [matter-of-fact, cool] "It isn't. I applied six months ago."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia:** [hurt, quiet] "Why didn't you tell me?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [controlled, matter-of-fact] "I told the unit. It told you. Three times."

*(A beat.)*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia:** [small, defensive] "I thought those were routine updates."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** [quiet, final] "I know."

*[SFX: suitcase wheels over the doorstep. The door closes.]*

*(The living room stays quiet for a long time. Then—)*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** [gentle, unchanged] "Mia, Lily has left. She asked me to tell you she will be back for the holidays."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia:** [hurt, cautious] "Did she actually say that?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** [calm, literal] "Yes."

*(One block of text fades in on the black:)*

```
HOUSEHOLD EMOTIONAL STABILITY: SAFE ZONE
Unit service length: 6 years — high-satisfaction household
Next review: in 3 years
```

*(The text fades. Black holds a few seconds. End.)*

**— LEVEL 4 END —**

**— END OF SCRIPT —**
