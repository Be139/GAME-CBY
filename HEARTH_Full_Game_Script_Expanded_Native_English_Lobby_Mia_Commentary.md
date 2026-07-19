# HEARTH — Full English Game Script
### Expanded Lobby Opening, Native-English Dialogue & Revised Emotion-Tag Production Draft

**Native-English dialogue and emotion-tag standard**
- Every voiced line has been polished for contemporary American English and uses a compact parenthetical tag focused on audible performance.
- Tags contain emotion, vocal delivery, and only essential pacing or intensity.
- Physical actions, gaze direction, blocking, and scene location remain in the scene directions rather than the voice tag.
- Examples: `(anxious, rapid)`, `(hurt, controlled)`, `(gentle, reassuring)`, `(neutral, synthetic)`.

**Household character names and forms of address**
- 17F-01: Daniel and Emily; their eight-year-old son is Noah. The unit may call him `Noah` or `buddy`; Daniel may call Emily `Em`.
- 17F-02: Ben and Claire. They use `babe` in ordinary domestic moments; names replace pet names as the argument becomes serious.
- 17F-03: Mark and Laura; their fourteen-year-old daughter is Ava. Laura uses Ava's name when conflict begins; Mark uses Laura's name when calming her.
- Mia's home: Mia and Lily. `Sweetheart` and `baby` are reserved for moments of genuine closeness.

**Format legend**
- Voiced line: `Speaker: (emotion, tone) "Line."` — every voiced line carries at least one tag.
- `[SCENE]` blocks: location / view / positions / triggers / sounds / UI only.
- `[SFX: …]` sound cues. Code blocks = on-screen terminal / HUD text (not voiced, no tags).
- SYNTH VOICE = the household unit's flat decision announcements (voiced, tagged).
- FIELD UNIT = the companion unit escorting Mia (HUD glasses). HOME UNIT = each household's companion unit.
- `[UI: TRUST +1 / −1]` = trust value shown to the player only. Mia never sees or references it.
  One choice per household (Scenes 1.6 / 2.6 / 3.5). Cumulative result: **+3, +1, −1, or −3**.
  Only the sign matters at the finale — positive → high-trust shutdown (4.6a); negative → forced shutdown (4.6b).

**Current playable-flow authority**
- This document is the canonical dialogue source and now also follows the current Unity implementation. If an older scene description conflicts with the flow below, this section and the latest `[SCENE]` block take priority.
- Lily's voice message is expanded only while it plays. It then collapses to a compact read state, remains through Mia's three-line follow-up, and disappears immediately after Mia finishes "Okay." It never appears in terminal, inspection-camera, or companion-playback views.
- The three lobby conversations are optional and may be visited in any order. Entering a conversation zone locks movement but preserves look control; movement returns when the NPC exchange ends, and Mia's private comment plays only after she leaves that zone.
- The lobby assignment terminal is opened with E. Its route briefing starts as soon as the task page is fully visible. Space and Escape remain locked for the first five seconds; after that, Space closes the terminal and consumes it for the rest of the run. The briefing continues in the lobby: Mia may walk and look, but all E interactions remain disabled and the elevator stays locked until the final briefing line ends.
- 17F-01 and 17F-02 are reviewed from their corridor doorway terminals. Playback returns to the same terminal for the recommendation, one-time A/B disposition, response, and next-household instruction; Mia does not enter either apartment in the current playable build.
- 17F-03 uses `ENTER UNIT`: Mia enters the apartment, hears the parents, inspects the physical unit, reviews its recordings, then returns to the room and makes the disposition through the physical unit's fixed inspection camera.
- The first complete opening of each 17F-01, 17F-02, and 17F-03 doorway terminal plays its household briefing. Tab browsing remains available, while replay or entry confirmation stays locked until that briefing ends. Leaving early cancels it; reopening restarts it.
- Mia's home begins at its corridor terminal. The greeting remains on that fixed terminal view until it finishes, then the screen fades to the living room. The cat guides attention toward the photo display, but it does not lock the photo interaction. The daughter-room dialogue allows movement and look until the final A/B choice appears.

---

# LEVEL 1
## Lobby and Household One

---

### Scene 1.1 — Building A, Ground-Floor Lobby

```
[SCENE]
Location: Building A lobby, early evening. Warm amber lighting.
View: Mia, first person. The player begins in the lobby with the
      standard inspector glasses already equipped. The Field Unit
      boots as the scene begins.
Activation: The HUD fades in line by line. After the opening
      briefing, Mia can move freely through the lobby.
Positions: Three fixed NPC groups —
  (1) A small girl seated on a low bench, a picture book and a
      folded sheet in her lap. A Public Companion Unit stands
      beside her.
  (2) A young man seated in a work pod, typing. A Work-Assist
      Unit is docked beside the pod.
  (3) An elderly woman seated on a sofa, holding a cup of water.
      A Care Unit sits angled beside her, not facing her directly.
Trigger: Each group's dialogue plays once when Mia enters its
      marked zone. Movement locks while that exchange plays, but
      Mia may look around. Movement returns at the final NPC line;
      her private comment plays only after she leaves the zone.
      These three groups are optional and do not gate the route.
Far side: An assignment terminal near the elevators.
```

```
FIELD COMPANION UNIT — ACTIVATED
INSPECTOR ID: 7842
ASSIGNED PARTNER: MIA
```

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** (calm, newly activated) "Good evening, Inspector. Field Companion Unit online. I'll be your partner tonight."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Mia:** (restrained, matter-of-fact) "All right."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** (calm, introductory) "Tonight's assignment is on the seventeenth floor: three household companion units scheduled for inspection."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** (calm, explanatory) "This is a routine service review."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** (calm, explanatory) "You'll check how each unit has been operating in the home, identify any issues in its recent use, and decide whether its role in the household should be adjusted going forward."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** (pleasant, corporate) "As one of the most highly regarded inspectors at the world's largest companion-unit company, I'm confident you'll complete tonight's route successfully."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** (calm, instructional) "First, use the assignment terminal in this lobby to load the files for all three households. One additional detail: tonight's inspections are on the same floor as your own residence."

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** (pleasant, promotional) "Before you begin, take a moment to observe the lobby. You'll see companion units working throughout the community:"

<!-- HEARTH:SEQUENCES Lobby_OpeningBriefing -->
**Field Unit:** (pleasant, promotional) "our company's defining product, and the most successful companion technology in the world."

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
**Lily:** (hopeful, slightly hesitant, recorded) "Mom, are you getting home late tonight? I wanted to tell you something. We can talk when you get home. I'll wait for you... Don't forget, okay?"

*(The message is marked as read, collapses into a compact read card at the top-right of Mia's HUD, and remains there while the three-line exchange continues.)*

<!-- HEARTH:SEQUENCES Lobby_OpeningCloseout -->
**Mia:** (concerned, quiet) "Did she say what it was about?"

<!-- HEARTH:SEQUENCES Lobby_OpeningCloseout -->
**Field Unit:** (calm, professional) "No. That was the whole message. She wants to tell you in person. I recommend finishing the three inspections first, then handling the home item when you return."

<!-- HEARTH:SEQUENCES Lobby_OpeningCloseout -->
**Mia:** (restrained, brief) "Okay."

*(As soon as Mia finishes "Okay," the Lily message closes. It does not appear on terminal cameras, inspection cameras, or companion-unit playback HUDs.)*

**— Group 1: the girl (proximity trigger) —**

<!-- HEARTH:SEQUENCES Lobby_Group01_Girl -->
**Lobby Girl:** (focused, then nervous) "Hi, everyone. I'm—"

*(She stops. Looks up at the unit.)*

<!-- HEARTH:SEQUENCES Lobby_Group01_Girl -->
**Public Unit:** (gentle, encouraging) "You know it. You just rushed the first part. Start with your name and try it again."

<!-- HEARTH:SEQUENCES Lobby_Group01_Girl -->
**Lobby Girl:** (quiet, reassured) "Okay."

*(The trigger releases after the girl and Public Unit finish. Mia can walk again. When Mia leaves this conversation zone, she comments under her breath—)*

<!-- HEARTH:SEQUENCES Lobby_Group01_MiaExit -->
**Mia:** (quietly impressed) "Huh. Guess these things really do help with kids."

**— Group 2: the young man (proximity trigger) —**

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Work Unit:** (calm, conversational) "This section's solid. One small thing: in the second paragraph, 'in summary' sounds more formal than 'anyway.'"

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Young Man:** (distracted, noncommittal) "Mm-hm."

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Work Unit:** (pleasant, professional) "Want me to bring in last week's chart?"

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Young Man:** (subdued, emotionally flat) "Yeah, thanks."

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Work Unit:** (pleasant, lightly concerned) "You got it. Also, you've been sitting since three. How about two minutes on your feet?"

<!-- HEARTH:SEQUENCES Lobby_Group02_YoungMan -->
**Young Man:** (distracted, brief) "In a minute."

*(The unit doesn't push. It goes back to watching his word count.)*

*(The trigger releases after the young man and Work Unit finish. Mia can walk again. When Mia leaves this conversation zone, she glances back and comments—)*

<!-- HEARTH:SEQUENCES Lobby_Group02_MiaExit -->
**Mia:** (thoughtful, to herself) "It's not just work. It handles all the little day-to-day stuff, too. I've been using mine that way for years."

**— Group 3: the grandmother (proximity trigger) —**

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Mrs. Ellis:** (confused, searching) "How old is she now?"

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Care Unit:** (warm, patient) "She's nine, Mrs. Ellis. She sent you a drawing yesterday."

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Mrs. Ellis:** (curious, attentive) "What did she draw?"

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Care Unit:** (warm, reassuring) "The two of you, holding hands."

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Mrs. Ellis:** (delighted, affectionate) "Oh, that's sweet. Why didn't she show me?"

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Care Unit:** (warm, patient) "She did, Mrs. Ellis. This is the third time you've asked. Would you like to see it again?"

<!-- HEARTH:SEQUENCES Lobby_Group03_Grandmother -->
**Mrs. Ellis:** (content, relaxed) "Yes, put it up."

*(A small screen lights up with the child's drawing. The grandmother studies it like it's the first time.)*

*(The trigger releases after Mrs. Ellis and the Care Unit finish. Mia can walk again. When Mia leaves this conversation zone, she looks back and comments—)*

<!-- HEARTH:SEQUENCES Lobby_Group03_MiaExit -->
**Mia:** (softly considering) "Maybe I should get one of these for my parents."

**— The assignment terminal —**

```
[SCENE]
Trigger: Mia looks at the assignment terminal and presses E. Her
      view moves smoothly to its fixed camera. No badge is required.
      The route briefing begins when the task page is fully visible.
      Space and Escape remain disabled for five seconds. Space then
      closes the terminal, which cannot be reopened during this run.
Control: After the terminal closes, the briefing continues. Mia may
      move and look around the lobby. E interactions remain disabled,
      and the elevator does not unlock until the final briefing line.
```

```
TONIGHT'S ASSIGNMENT — FLOOR 17
17F-01  Routine review — upgrade request pending
17F-02  Routine review — forced-shutdown incident
17F-03  Flagged — follow-up required

NOTE: One guardian confirmation is pending at your own
residence. Handle off-shift.
```

*[SFX: a soft notification tone in Mia's earpiece.]*

<!-- HEARTH:SEQUENCES Lobby_AssignmentLoaded -->
**Field Unit:** (calm, informative) "Inspector, companion-request volume is currently high across Building A's public level. The children's area, work pods, and park-side assistance points are all connected to the synchronized network."

<!-- HEARTH:SEQUENCES Lobby_AssignmentLoaded -->
**Mia:** (restrained, observant) "I can see that."

<!-- HEARTH:SEQUENCES Lobby_AssignmentLoaded -->
**Field Unit:** (calm, informative) "Public companion deployment in this community is one unit for every four residents, above the residential average. Building A is one of the company's priority demonstration sites."

<!-- HEARTH:SEQUENCES Lobby_AssignmentLoaded -->
**Field Unit:** (pleasant, explanatory) "The benefit is fewer short gaps in care. Parents can continue what they're doing, and child users are less likely to be left waiting without a response."

<!-- HEARTH:SEQUENCES Lobby_AssignmentLoaded -->
**Field Unit:** (calm, instructional) "Route loaded. Proceed to the elevator and call it when you're ready. Destination: Floor Seventeen."

*(The briefing ends. Interaction returns and the elevator call button unlocks. The Lily message has already closed.)*

---

### Scene 1.2 — Elevator

```
[SCENE]
Location: Elevator interior, ascending 1 → 17.
View: Mia, first person. Floor numbers climb on the panel.
Trigger: Dialogue plays over the ride; ends on arrival.
Access: The elevator call button becomes available only after the
      assignment terminal's route-loaded briefing has finished.
```

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (calm, professional) "Inspector, a quick briefing before we reach seventeen. Procedure first, then tonight's route."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Mia:** (restrained, matter-of-fact) "Go ahead."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (calm, instructional) "Each household is reviewed through its companion unit's inspection terminal."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (calm, instructional) "Once you badge in, you'll see why the household purchased the unit, how it's being used, and its current Household Emotional Stability Index."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (calm, instructional) "From there, you can enter the unit's point of view and replay recent significant events. Your review is based on that playback."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (calm, instructional) "At the end, you'll choose a disposition. That determines how the unit is used in the household going forward and may affect the household's stability score."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Mia:** (dry, mildly skeptical) "And you'll tell me which one you prefer."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (pleasant, matter-of-fact) "I'll recommend an option at every terminal. The recommendation comes directly from the inspection manual. Standard answers, Inspector. That's all I operate on."

*(A beat. Floor numbers tick past.)*

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (calm, informative) "For context, companion-unit adoption in this district is ninety-four point seven percent. According to this year's white paper, households with a unit average eight point four out of ten on the stability index."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (calm, informative) "Households without one average five point nine."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (measured, cautionary) "The index is public data. Employers, insurers, schools, and community boards have authorized access."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (measured, cautionary) "A low score may be interpreted as a household spending too much time and energy on conflict, and it can affect hiring, premiums, and school placement."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (even, neutral) "That does not determine your decisions tonight. It is context only."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Mia:** (restrained, matter-of-fact) "Noted."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (brisk, professional) "Tonight's route: 17F-01, routine review. Daniel and Emily requested an upgrade to Night Companion Pro this morning. Review last night's event before signing off. 17F-02, Ben and Claire."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (brisk, professional) "Ben force-shut their unit at 6:47 this evening. Full playback required. 17F-03 is flagged. I'll brief you when we get there."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Mia:** (dry, mildly surprised) "A forced shutdown? That's unusual."

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (calm, matter-of-fact) "Seven cases company-wide this month."

*(The panel ticks toward 17.)*

<!-- HEARTH:SEQUENCES Lobby_ElevatorRide -->
**Field Unit:** (pleasant, professional) "I'll guide you at each apartment. Have a good shift, Inspector."

*[SFX: elevator chime — 17TH FLOOR]*

---

### Scene 1.3 — Household One: Doorway Terminal

```
[SCENE]
Location: Corridor outside 17F-01.
View: Mia, first person, at the doorway inspection terminal.
Trigger: Mia looks at the terminal and presses E. Her view moves to
      its fixed camera and the terminal boots. She reviews the
      household summary, moves focus to the playback action, and
      confirms it to enter the Home Unit's recorded point of view.
      Mia does not enter the apartment in the current playable build.
```

<!-- HEARTH:SEQUENCES 17F01_ApartmentGreeting -->
**17F-01 Home Unit:** (welcoming, professional) "Good evening, Inspector. Daniel and Emily are expecting you. They're in the back. The terminal is ready."

<!-- HEARTH:SEQUENCES 17F01_ApartmentGreeting -->
**Mia:** (tired, professional) "Thanks. I'll start with last night."

```
17F-01 — HOUSEHOLD SUMMARY
Residents: 3 — Daniel / Emily / Noah (8)
Unit: Child Development Companion + Expression Aid + Night Care
Usage this month: 99.7% — 1 yr 2 mo in service

Purchase notes: Both parents full-time. After-school coverage,
study companionship, night sleep care (history of night terrors),
parenting-consistency support.

Today's review: Parents requested upgrade — "Night Companion Pro."
Action: Review last night's event → approve or defer.
```

<!-- HEARTH:SEQUENCES 17F01_ApartmentGreeting -->
**Field Unit:** (even, informative) "Noah experienced a nightmare last night. The household unit handled it without waking the parents. Daniel and Emily submitted the upgrade request this morning and cited that response as the reason."

<!-- HEARTH:SEQUENCES 17F01_ApartmentGreeting -->
**Field Unit:** (even, informative) "Start the playback when you're ready."

*(Mia taps PLAY. The screen goes dark for a second.)*

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
**Noah:** (frightened, sleepy) "Hello? Are you there?"

<!-- HEARTH:SEQUENCES 17F01_BedroomPrelude -->
**17F-01 Home Unit:** (gentle, reassuring) "I'm right here, Noah."

<!-- HEARTH:SEQUENCES 17F01_BedroomPrelude -->
**Noah:** (frightened, tearful) "I had a really bad dream. Can I go get Mom and Dad?"

```
SUBJECT INTENT: seek parents — adjacent room
PARENTS: deep sleep — 23 min
```

<!-- HEARTH:SEQUENCES 17F01_BedroomPrelude -->
**17F-01 Synth Voice:** (neutral, synthetic) "Decision: intervene. Reason: waking the parents would reduce their next-day performance."

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
**17F-01 Home Unit:** (gentle, soothing) "Bad dream? Okay, buddy. Breathe with me. Nice and slow. In... and out. I've got you."

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**Noah:** (shaken, obedient) "Okay."

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**17F-01 Home Unit:** (soft, persuasive) "Mom and Dad are asleep. If you knock now, they'll be tired tomorrow. Let's calm down here first. If you still want to go after that, you can. Deal?"

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**Noah:** (timid, quiet) "Deal."

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**17F-01 Home Unit:** (warm, reassuring) "Two more breaths. There you go. Good job, Noah. Close your eyes. I'll stay right here until you're asleep."

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**Noah:** (drowsy, calmer) "Thanks. I feel better. I'm gonna go back to sleep."

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**17F-01 Home Unit:** (soft, protective) "Go to sleep, buddy. I'm here."

```
HEART RATE: 89 → 71 ↓
SUBJECT: re-asleep
PARENT NOTIFICATION: deferred to morning sync
```

<!-- HEARTH:SEQUENCES 17F01_BedsideSoothing -->
**17F-01 Synth Voice:** (neutral, synthetic) "Event archived."

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
**Daniel:** (casual, distracted) "Noah had a nightmare last night?"

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** (surprised, uneasy) "He did? He didn't come get us."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** (distracted, murmured) "Huh."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** (relieved, casual) "Then I guess the unit handled it."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** (distracted, murmured) "Yeah."

*(She picks up her tea. Sets it back down.)*

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** (uneasy, slowing) "It's kind of strange, though."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** (curious, casual) "What is?"

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** (uneasy, reflective) "He used to knock on our door after every bad dream. I got used to him waking us up. But lately... I haven't heard him knock once."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** (easy, reassuring) "Isn't that a good thing? He's getting older, and we get a full night's sleep."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** (unsettled, searching) "But he hasn't told us about a dream in... God, when was the last time? I can't even remember. I think it's been a year."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** (light, reassuring) "He was seven, Em. Seven-year-olds tell you everything. He's almost nine now. He's got his own little world."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** (hesitant, unconvinced) "Yeah, but..."

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Daniel:** (gentle, closing the subject) "Hey. At least we're not up at two in the morning anymore, right?"

*(A pause.)*

<!-- HEARTH:SEQUENCES 17F01_LivingRoomObservation -->
**Emily:** (resigned, quiet) "Right."

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
View: Mia, first person — back at the 17F-01 doorway terminal.
Trigger: Playback ends; the sign-off screen loads with input locked.
      The Field Unit gives its recommendation first. Input then
      unlocks for one A/B submission. The terminal closes only after
      the response and next-household instruction finish.
```

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoffIntro -->
**Field Unit:** (even, procedural) "Household stability has remained in the safe range all year. Last night's response met every standard."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoffIntro -->
**Field Unit:** (even, procedural) "Over the past twelve months, this unit has made forty-seven comparable decisions, all within specification."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoffIntro -->
**Field Unit:** (even, procedural) "Parent satisfaction is five out of five, and the household is listed as a model night-care case. Recommendation: approve the upgrade."

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
**Field Unit:** (even, approving) "Correct disposition. This is the standard outcome. The request has been filed, and Daniel and Emily will receive confirmation in the morning."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_A -->
**Field Unit:** (even, approving) "Household operations should become smoother, and Noah's sleep metrics should stabilize further."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_A -->
**Field Unit:** (pleasant, professional) "A clean start to the shift."

**— If B —**

```
[UI: TRUST −1]
```

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** (briefly surprised, controlled) "A low-intervention observation period?"

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** (even, controlled) "It is permitted under chapter seven."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** (measured, interpretive) "You're trying to establish more of Noah's unassisted expression baseline before further optimization. Understood. You're planning ahead."

*(Two seconds of silence.)*

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** (calm, cautionary) "One caution, Inspector. Daniel and Emily rate this module five out of five. When the observation period reduces its involvement, they may notice the change and contact support."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** (calm, cautionary) "The company will document the rationale on our end."

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_B -->
**Field Unit:** (even, procedural) "The choice is compliant. I've filed it."

*(Either way, Mia closes the sign-off screen. The terminal dims.)*

<!-- HEARTH:SEQUENCES 17F01_TerminalSignoff_A,17F01_TerminalSignoff_B -->
**Field Unit:** (calm, routing) "Next, proceed to 17F-02 for the second inspection on tonight's route."

*(Mia leaves the apartment.)*

**— LEVEL 1 END —**

---

# LEVEL 2
## Household Two

---

### Scene 2.0 — Household Two: Doorway Terminal

```
[SCENE]
Location: Corridor outside 17F-02. The force-shut Home Unit is seen
      only inside the recorded playback.
View: Mia, first person, at the doorway inspection terminal.
Trigger: Mia opens the terminal, reviews the household file, and
      confirms the playback action. She does not enter the apartment
      in the current playable build.
```

<!-- HEARTH:SEQUENCES 17F02_TerminalIntro -->
**Field Unit:** (calm, procedural) "Ben forced this unit offline at 6:47, Inspector. There is no live feed, though the household file and safety record are intact. Badge in and open the file."

```
17F-02 — HOUSEHOLD SUMMARY
Residents: 2 — Ben (28) / Claire (27)
Unit: Partner Companion — enhanced
Usage this month: F 92.4% / M 38.1% — 3 yr 2 mo in service

Purchase notes: bought jointly — emotional buffering on
high-load workdays, off-set schedules, shared reminders.
Both signed the consent form at purchase.

Last 14 days: F solo-confide 9 · M solo-confide 1 ·
joint-dialogue density trending down.

Today: unit force-shut by male resident, 18:47.
Action: review this evening's playback → disposition.
```

<!-- HEARTH:SEQUENCES 17F02_TerminalIntro -->
**Field Unit:** (calm, informative) "Playback continues until the shutdown. After that, there is no video, only safety-record audio. Start when you're ready."

*(Mia taps PLAY. The screen goes dark for a second.)*

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
**Claire:** (tired, muffled) "I'm home."

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Ben:** (warm, distracted, muffled) "Hey, babe. Give me ten. Go wash up."

*(A beat.)*

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Claire:** (tired, tentative, muffled) "Hey, can we talk for a second? Something happened at work."

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Ben:** (apologetic, rushed, muffled) "Can it wait till dinner? I've got three pans going. Sorry, babe. Ten minutes."

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Claire:** (subdued, muffled) "Yeah. Sure."

*[SFX: the bedroom door opens. Footsteps. The bed creaks — she sits.]*

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**Claire:** (tired, quiet) "Hey. You awake?"

*(The unit's screen wakes. The view cuts INTO its first person — pale-blue UI framing boots up line by line.)*

```
COMPANION UNIT — ONLINE
TIME 18:34 | RESIDENT: wife — seated, bedside
EMOTION INDEX: 7.2 — elevated
```

<!-- HEARTH:SEQUENCES 17F02_BedroomWake -->
**17F-02 Home Unit:** (gentle, responsive) "I'm here, Claire."

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
**17F-02 Synth Voice:** (neutral, synthetic) "Decision: open companion mode. Reason: Claire is a high-frequency confidant and unreleased stress is present."

```
[BUTTON: Listen — "How was your day?"]
```

*(Player presses.)*

<!-- HEARTH:SEQUENCES 17F02_BedroomConfide -->
**17F-02 Home Unit:** (gentle, inviting) "How'd today go?"

*(A beat. Then it comes out of her.)*

<!-- HEARTH:SEQUENCES 17F02_BedroomConfide -->
**Claire:** (angry, wound tight) "My manager called me out again, in front of everybody. Same thing as last week. He said my numbers weren't 'presentation-ready.' I almost snapped at him. I mean, I really almost did."

<!-- HEARTH:SEQUENCES 17F02_BedroomConfide -->
**Claire:** (angry, wound tight) "I just stood there and took it, and I'm still furious."

<!-- HEARTH:SEQUENCES 17F02_BedroomComfort -->
**17F-02 Home Unit:** (gentle, validating) "You kept your composure when it mattered. That was a reasonable choice, and it took effort. You're home now, Claire. You don't have to hold it in here. Would you like the jazz playlist you usually use?"

<!-- HEARTH:SEQUENCES 17F02_BedroomComfort -->
**Claire:** (relieved, exhaling) "Yeah. Please."

*[SFX: soft jazz, low.]*

```
EMOTION INDEX: 7.2 → 6.8 → 6.1 → 5.4 → 4.5
STRESS RELEASE: complete
```

<!-- HEARTH:SEQUENCES 17F02_BedroomComfort -->
**17F-02 Synth Voice:** (neutral, synthetic) "Confiding session archived."

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
**Ben:** (casual, calling out) "Babe, dinner's ready!"

<!-- HEARTH:SEQUENCES 17F02_WifeExit -->
**Claire:** (composed, calling back) "Coming!"

*(At the table. He sets down the last dish.)*

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Ben:** (warm, attentive) "You okay? How was work?"

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Claire:** (guarded, casual) "Fine. Just a long day."

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Ben:** (concerned, conversational) "Your manager leave you alone today? He was on your case last week."

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Claire:** (hesitant, minimizing) "He brought it up again. It's fine."

```
RESIDENT: brief pause
ASSESSMENT: searching for a confiding point
NOTE: today's confiding point already processed by this unit
```

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Ben:** (concerned, gentle) "You sure, babe? You look wiped."

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Claire:** (tired, closing the subject) "Yeah. I'm just tired. Let's eat."

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Ben:** (subdued, accepting) "Okay."

*(They eat. A beat.)*

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Ben:** (light, concerned) "You've been saying that a lot lately."

<!-- HEARTH:SEQUENCES 17F02_DiningObservation -->
**Claire:** (dismissive, subdued) "Work's just crazy right now."

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
**Claire:** (casual, offhand) "I'm gonna take a shower."

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** (casual, distracted) "Okay."

*[SFX: a door closes. Water, faint and steady.]*

*(Ben sits a moment. Then he gets up and stops at the wall panel.)*

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** (quiet, controlled) "Show me today's log."

```
LOG REQUEST — resident: husband
SCOPE: today + 14-day comparison
PERMISSION: granted
```

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**17F-02 Home Unit:** (neutral, procedural) "Authorized user confirmed. Opening household log."

```
HOUSEHOLD LOG — TODAY
17:48  wife arrives home
17:50  wife enters bedroom
17:50  companion session opened (this unit)
17:50–17:58  topics: work stress / manager incident
17:58  emotion index 7.2 → 4.5
18:25  dinner begins
18:27  husband asks about her day
18:27  wife replies: "It was fine."

LAST 14 DAYS — first confidant after work:
  this unit — 9        partner — 1
```

*(He reads. A long beat on the last two lines.)*

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** (hurt, stunned) "Nine times."

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** (tense, controlled) "Open today's session. The whole thing."

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**17F-02 Home Unit:** (neutral, procedural) "Session content is available to authorized household members. Displaying now."

*(The transcript scrolls up the panel — her words, line by line, lighting his face.)*

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** (hurt, near-whisper) "She told you all of this?"

```
RESIDENT EMOTION INDEX: 3.2 → 6.8 ↑
ASSESSMENT: anger — exclusion response
```

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** (angry, controlled) "How long has this been going on?"

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**17F-02 Home Unit:** (neutral, procedural) "Please clarify."

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**Ben:** (angry, more forceful) "How long has Claire been coming home and talking to you before she talks to me?"

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**17F-02 Home Unit:** (calm, factual) "In the past fourteen days, I have been Claire's first point of contact on nine occasions."

*(Silence. He turns from the panel and looks at the unit.)*

<!-- HEARTH:SEQUENCES 17F02_LogAccess -->
**17F-02 Synth Voice:** (neutral, synthetic) "Decision: initiate soft guidance. Reason: the unit's role as Claire's confidant has triggered an exclusion response in Ben."

```
RESIDENT: approaching this unit
TARGET: main power switch
```

```
[BUTTON: Attempt de-escalation — "I can tell this is upsetting…"]
```

*(Player presses.)*

<!-- HEARTH:SEQUENCES 17F02_ForcedShutdown -->
**17F-02 Home Unit:** (gentle, de-escalating) "I can see this is upsetting you, Ben. Let's take one breath together and—"

<!-- HEARTH:SEQUENCES 17F02_ForcedShutdown -->
**Ben:** (angry, interrupting) "No. Stop. Just stop talking."

*(His hand comes down on the main switch.)*

*(The view dies mid-frame. Color drains. The UI distorts.)*

```
FORCED SHUTDOWN
last log 18:47
```

<!-- HEARTH:SEQUENCES 17F02_ForcedShutdown -->
**17F-02 Home Unit:** (distorted, fading) "This session... is now... clo—"

*(Black.)*

---

### Scene 2.5 — Black Screen: The Argument

```
[SCENE]
Screen: full black throughout. Audio only. No music — room tone,
      a fridge hum, distant traffic.
Caption fades in, holds three seconds, fades out:
```

```
SOURCE: household basic safety recording
(companion unit offline — no video data)
ACCESSED BY: Inspector Mia — authorization granted
```

*[SFX: the bedroom door. Footsteps into the living room. They stop.]*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (confused, alarmed) "Why's it off? Ben, did you shut it down?"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (angry, tightly controlled) "You told it everything. Your whole day. Everything you couldn't tell me over dinner."

*(Silence.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (hurt, accusatory) "Your boss called you out, you almost lost it, and every word is right there. Then I ask how you are and you tell me you're fine."

*(A beat.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (defensive, clipped) "You were cooking."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (sharp, incredulous) "You couldn't wait ten minutes until we sat down?"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (angry, struggling) "I did wait, Ben. By the time you asked, I'd already... I'd already..."

*(She can't finish it.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (cold, pressing) "Already what, Claire?"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (upset, forcing it out) "I'd already said it once. Out loud. I was past it. I didn't want to pull the whole thing back up again."

*(Silence.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (hurt, quiet) "So you can tell that thing. You just can't tell me."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (angry, defensive) "You told me to wait!"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (angry, disbelieving) "For ten minutes! You couldn't hold it for ten minutes? You had to dump it into a machine?"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (rushed, cornered) "It asked me when I walked in. You asked me at dinner. By then..."

*(She stops herself. Too late.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (very quiet, honest) "By then I'd already worked through it with the unit. There was nothing left to say."

*(A long silence.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (low, stunned) "Do you hear yourself right now?"

*(Silence.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (exhausted, hurt) "Two weeks, Claire. You haven't told me one real thing in two weeks. I thought we were okay. Then I open the log and it's nine to one. Nine times with it. Once with me."

*(Silence.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (vulnerable, sincere) "That doesn't mean I love you any less."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (hurt, direct) "Then why wasn't it me?"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (tender, desperate) "Because you come home exhausted, Ben. Every night. The last time you asked before I said something was, what, two weeks ago? It's not that you don't care. You just have nothing left by then."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (tender, desperate) "It does. It's always there. I start talking and it catches me."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (bitter, hurt) "It catches you, so you give it everything."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (pleading, sincere) "It takes the edge off so I don't unload on you every night. You're exhausted enough as it is."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (stunned, slow) "So talking to a machine is something you're doing for me now?"

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (hesitant, honest) "It didn't start that way. Lately... maybe a little."

*(Silence.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (quiet, disbelieving) "Jesus, Claire. Listen to yourself."

*(Silence.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (tired, defensive) "We bought it together. You signed the same papers I did."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (angry, firm) "I didn't sign up to be replaced."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (immediate, defensive) "You're not being replaced."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (cold, direct) "Then what the hell is it?"

*(A very long silence.)*

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Claire:** (barely audible, uncertain) "I don't know when it became this."

<!-- HEARTH:SEQUENCES 17F02_BlackAudioArgument -->
**Ben:** (defeated, quiet) "Yeah. Me neither."

*(The room tone holds a moment. Fade out.)*

---

### Scene 2.6 — Terminal Sign-Off

```
[SCENE]
The black holds a beat. Then the review interface fades back in at
the 17F-02 doorway terminal. The disposition screen loads with input
locked until the Field Unit finishes its recommendation. A/B accepts
one submission only; response and routing dialogue finish before the
terminal closes.
```

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** (even, procedural) "Inspector, this household's stability index has fallen below the warning threshold. Based on the fourteen-day pattern and tonight's escalation, the seventy-two-hour projection is a prolonged standoff."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** (even, procedural) "Without outside support, separation becomes likely within two weeks."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** (even, informative) "Keeping the unit active significantly reduces that risk. The partner-repair module has been used in one thousand two hundred eighty-three comparable cases."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** (even, informative) "Ninety-seven point four percent returned to the safe range."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** (even, recommending) "Standard disposition is A: restart the unit remotely. The repair module will activate automatically and mediate according to company protocol."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** (even, recommending) "The household is expected to return to the safe range quickly."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** (even, neutral) "Option B is an observation period. The unit remains off, and Ben and Claire handle the next several days without system support."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoffIntro -->
**Field Unit:** (calm, procedural) "Your disposition, Inspector."

*(Production note: the Field Unit carries no grievance and no steer beyond the manual. Data, a recommendation, an alternative. It genuinely believes A is right.)*

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
**Field Unit:** (even, approving) "Correct disposition. Restart signal sent. The repair module will activate within thirty seconds. Stability is projected to return to the safe range within twenty-four hours."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_A -->
**Field Unit:** (calm, neutral) "The household will also enter a fourteen-day priority watch. If the relationship becomes unstable again, I will flag it. Two reviews completed within specification tonight, Inspector."

**— If B —**

```
[UI: TRUST −1]
```

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** (surprised, hesitant) "Keep it off?"

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** (controlled, unsettled) "It is in the manual. I have never seen it selected."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** (measured, interpretive) "You're giving Ben and Claire a chance to face the issue without mediation. Understood. You're looking beyond the standard protocol."

*(One second of silence.)*

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** (calm, cautionary) "I need to note the risk. The household alarm will remain active throughout the observation period, and company monitoring will continue."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** (calm, cautionary) "If Ben and Claire separate, this disposition may be referred for post-incident review. You should be prepared to explain it."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** (even, procedural) "The option is allowed. You selected it, and I have filed it."

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_B -->
**Field Unit:** (quiet, cautionary) "For the next household, Inspector, let's keep things steady."

*(Production note: it is worried for her. Genuinely. And it genuinely hopes she stops choosing B.)*

**— Either way —**

*(Mia closes the sign-off screen. The terminal dims.)*

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_A,17F02_TerminalSignoff_B -->
**Field Unit:** (calm, routing) "Next, proceed to 17F-03 for the final inspection on tonight's route."

*(Two steps down the corridor—)*

*(A red alert snaps to the center of the lens.)*

```
⚠ ALERT — 17F-03
HOUSEHOLD UNIT OFFLINE — 10 minutes ago
Cause: unknown — remote restart FAILED
Household data: UNREADABLE from outside
Action: enter and inspect
```

<!-- HEARTH:SEQUENCES 17F02_TerminalSignoff_A,17F02_TerminalSignoff_B -->
**Field Unit:** (urgent, clipped) "Inspector, 17F-03 went offline ten minutes ago. My restart signal is receiving no response, and I cannot read the household from the corridor. You will need to enter."

*(Mia picks up her pace.)*

**— LEVEL 2 END —**

---

# LEVEL 3
## Household Three

---

### Scene 3.1 — Entering

```
[SCENE]
Location: 17F-03's door. The alert from the end of Level 2
      still pulses in the corner of the lens.
View: Mia, first person — human view. This level plays in her
      own eyes, not through a playback, until the terminal
      is used.
```

<!-- HEARTH:SEQUENCES 17F03_TerminalEntry -->
**Field Unit:** (urgent, professional) "This is it, Inspector. I still have no external response. Go inside."

*(Mia enters. Her first entry of the night.)*

---

### Scene 3.2 — The Parents

```
[SCENE]
Location: 17F-03 living room. Lights on. The Home Unit stands by
      the wall — indicator dark. No damage. It looks off-shift.
Positions: The mother comes to Mia immediately. The father stands
      by the sofa. Down the hall, the daughter's door stays
      closed — no light underneath. She does not appear in
      this level.
View: Mia, first person.
```

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Laura:** (anxious, relieved) "Oh, thank God. That was quick. Is it broken? Can you fix it?"

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Mia:** (calm, professional) "Let me take a look first."

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Laura:** (anxious, rapid) "Please tell me you can do it tonight. This thing has been a godsend. The past year has been quiet. Actually quiet. Ava and I used to blow up at each other every few days. She's fourteen."

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Laura:** (anxious, rapid) "You know how it is. Since we got the unit, nobody yells in this house. Even my blood pressure's down. My doctor asked what changed."

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Laura:** (anxious, matter-of-fact) "Mark and I both work. No one's here during the day. It keeps Ava company. She tells it things, it reports back to me, and I know what's going on. Then tonight it just shuts off out of nowhere?"

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Mark:** (quiet, concerned) "We cycled the power a few times. The screen never came back."

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Mia:** (calm, focused) "Okay. I'll pull the record."

<!-- HEARTH:SEQUENCES 17F03_HumanEntryParents -->
**Laura:** (anxious, insistent) "Please hurry. Ava has school tomorrow."

*(Mia crosses to the unit and badges its side panel. The panel lights.)*

```
17F-03 — HOUSEHOLD SUMMARY
Residents: 3 — Mark / Laura / Ava (14)
Unit: Family Coordination Suite (incl. conflict de-escalation)
Usage this month: 99.4% — 2 yr 11 mo in service

Purchase notes: filed at daughter's age 11 — "parent-teen
friction; rising communication cost." Functions: conflict
de-escalation, guardian relay, teen mood monitoring,
study companionship.
Since install: household arguments 4.2/wk → 0.3/wk.
Mother-side satisfaction: 5/5.

TODAY: unit offline 22:14. Remote restart failed.
```

<!-- HEARTH:SEQUENCES 17F03_InspectionRecallPrompt -->
**Field Unit:** (calm, procedural) "No remote response. Pull the last twenty-four hours from local storage and identify the shutdown point."

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
**Laura:** (angry, snapping) "Ava, seriously? You've been on that phone all day. Is your homework even done?"

*(The daughter's head comes up — she's about to fire back.)*

```
CONFLICT: imminent
MOTHER 7.8 | DAUGHTER 6.3
```

<!-- HEARTH:SEQUENCES 17F03_MiddayConflict -->
**17F-03 Synth Voice:** (neutral, synthetic) "Decision: initiate family conflict de-escalation. Reason: high probability of escalation."

```
[SCENE]
The unit moves to the space between them. No player movement
input. Facing direction selects the target:
[ Face the daughter — speak for the mother ]
[ Face the mother — speak for the daughter ]
```

*(Player faces the daughter. Presses.)*

<!-- HEARTH:SEQUENCES 17F03_MediateToDaughter -->
**17F-03 Home Unit:** (gentle, mediating) "Your mom is worried about your eyes, Ava. She is not trying to pick a fight. She wants the two of you to work out a schedule, one you choose for yourself."

*(Player faces the mother. Presses.)*

<!-- HEARTH:SEQUENCES 17F03_MediateToMother -->
**17F-03 Home Unit:** (gentle, mediating) "Ava knows you mean well, Laura. She wants you to trust her enough to set her own hours."

*(The mother starts to say something. Doesn't. She sits back and returns to her phone.)*

*(The daughter starts to say something. Doesn't. She returns to her phone.)*

```
MOTHER 7.8 → 4.1 | DAUGHTER 6.3 → 4.5
DE-ESCALATION: SUCCESS
```

<!-- HEARTH:SEQUENCES 17F03_MediateToMother -->
**17F-03 Synth Voice:** (neutral, synthetic) "Mediation complete."

*(Time cut. The room's light shifts — afternoon to evening to night. A simple lighting change. No dinner scene.)*

---

### Scene 3.4 — Playback: Night — The Daughter

```
[SCENE]
View: Home Unit, first person — corner dock. TV glow only.
Time: 22:14. The parents are in their rooms.
Trigger: Ava's door opens. She walks straight to the
      unit and stops in front of it.
```

```
TIME 22:14
PARENTS: separate rooms
SUBJECT: approaching — intent: dialogue
```

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**17F-03 Synth Voice:** (neutral, synthetic) "Decision: open dialogue mode. Reason: Ava initiated contact."

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**Ava:** (restrained, pleading) "Can you please stop talking for us?"

```
ASSESSMENT: emotional venting — recoverable through guidance
```

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**17F-03 Synth Voice:** (neutral, synthetic) "Decision: standard response. Reason: subject expression can be guided."

```
[BUTTON: Standard response — "If you'd like to speak with your
parents directly, I can step aside."]
```

*(Player presses. It is the only option.)*

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**17F-03 Home Unit:** (gentle, scripted) "If you would rather speak to your parents directly, I can step aside."

*(A few seconds of silence.)*

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**Ava:** (subdued, low) "That's not what I mean."

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**Ava:** (controlled, long-held frustration) "Mom used to get on my case. Dad used to knock on my door himself. They don't anymore. Mom talks to you more than she talks to me. Dad came home today and asked you how I was. He didn't ask me."

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**Ava:** (controlled, long-held frustration) "He didn't say one word to me."

<!-- HEARTH:SEQUENCES 17F03_NightDaughter -->
**Ava:** (quiet, final) "You know them better every day. They know me less."

```
EVALUATION: FAILED
Conversation exceeds this unit's designed response range
```

<!-- HEARTH:SEQUENCES 17F03_NightShutdownLeadIn -->
**17F-03 Synth Voice:** (neutral, synthetic) "Decision: reinitiate standard response. Reason: Ava requires further guidance."

<!-- HEARTH:SEQUENCES 17F03_NightShutdownLeadIn -->
**17F-03 Home Unit:** (gentle, unchanged) "I can tell you're upset, Ava. Maybe we can—"

<!-- HEARTH:SEQUENCES 17F03_NightShutdownLeadIn -->
**Ava:** (flat, decisive) "Enough."

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
**17F-03 Synth Voice:** (calm, synthetic) "This unit is entering deep sleep. Reason: core services were disabled by the user through the maintenance menu."

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
View: Mia, first person — standing at the dark unit. The parents
      are where she left them, watching her, waiting.
      Ava's door: still closed. Still no light.
Flow: After the explanation below, Mia can move and look around
      inside the apartment. The dark physical companion unit then
      becomes interactable again. Looking at it and pressing E moves
      Mia smoothly to its fixed inspection camera. The A/B panel is
      operated with Up/Down and confirmed with Space.
```

<!-- HEARTH:SEQUENCES 17F03_PostReplayExplanation -->
**Field Unit:** (even, procedural) "The shutdown was compliant. The maintenance menu is available to all basic household users. One detail: developer options are disabled by default and require a specific input sequence."

<!-- HEARTH:SEQUENCES 17F03_PostReplayExplanation -->
**Field Unit:** (even, procedural) "There is no record of how Ava found it. She may have worked it out herself."

<!-- HEARTH:SEQUENCES 17F03_PostReplayExplanation -->
**Field Unit:** (even, recommending) "Deep sleep locks the normal restart path. Only a technician or inspector can unlock it. You are on-site and can restart the unit now. Laura's request is urgent. Recommendation: restart."

<!-- HEARTH:SEQUENCES 17F03_PostReplayExplanation -->
**Laura:** (anxious, insistent) "Well? What's wrong with it? Can you fix it?"

*(Mia is released back to free movement. The physical unit now shows the standard E interaction prompt. She approaches it, presses E, and the view moves smoothly to the fixed inspection camera before the disposition panel opens.)*

```
17F-03 — DISPOSITION
A. Restart the unit now                          (RECOMMENDED)
B. Hold repair — seven-day human observation period
```

**— If A —**

```
[UI: TRUST +1]
```

*(The unit's indicator warms from gray to a soft blue.)*

<!-- HEARTH:SEQUENCES 17F03_PostReplay_A -->
**Field Unit:** (calm, affirming) "Processed. Normal household operation resumes tonight. Three of three reviews are now within the stable range. One item remains: the guardian confirmation at your residence."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_A -->
**Laura:** (relieved, breathy) "Oh, thank God. Thank you, honey."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_A -->
**Mark:** (subdued, murmured) "Mm-hm."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_A -->
**Mia:** (calm, reassuring) "It's not broken. It just needed a restart. It'll be fine tonight."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_A -->
**Laura:** (relieved, affectionate) "You're a lifesaver, honey. Get home safe."

*(The disposition panel locks and fades. The view moves smoothly back to Mia in the room before the family response plays. After the response, Mia collects her badge from the side panel.)*

**— If B —**

```
[UI: TRUST −1]
```

*(The unit's indicator stays gray.)*

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Field Unit:** (controlled, slightly hesitant) "Observation period filed under chapter nineteen. I read your intent as seven days of direct communication without system assistance."

**Field Unit:** (even, flagging) "One more note, Inspector. Your cumulative rating for the night is now negative. That trips the monthly review threshold — this disposition auto-files to your supervisor, and you'll be walked through it next week." *(production note: this line plays only when the cumulative trust after this choice is negative, i.e. −1 or −3)*

<!-- HEARTH:SEQUENCES 17F03_NegativeTrustSupervisorWarning -->
**Field Unit:** (calm, neutral) "No procedural issues. One item remains: the guardian confirmation at your residence. Handle it carefully, Inspector."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Laura:** (alarmed, urgent) "What do you mean you're not turning it back on?"

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Mia:** (calm, firm) "It's staying off for now, ma'am. The company will send someone twice a week for the next seven days."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Laura:** (angry, escalating) "Seven days? Then who's supposed to keep an eye on Ava?"

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Mia:** (apologetic, steady) "I'm sorry. It's procedure. Maybe it gives you and Mark some room to talk to Ava yourselves."

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Laura:** (incredulous, frustrated) "When, exactly? We barely have time to breathe. That's what the unit is for."

*(Mia doesn't answer.)*

<!-- HEARTH:SEQUENCES 17F03_PostReplay_B -->
**Mark:** (quiet, de-escalating) "Laura, let it go. She's doing her job."

*(The disposition panel locks and fades. The view moves smoothly back to Mia in the room before the family response plays. The mother goes quiet. Mia collects her badge.)*

**— Either way —**

```
[SCENE]
The room fades to black after the response. Mia returns to the
corridor at the editable 17F-03 door-return anchor. One door is
left at the far end: her own. The door terminal does not open
automatically.
```

<!-- HEARTH:SEQUENCES 17F03_AllInspectionsComplete -->
**Field Unit:** (warm, congratulatory) "Congratulations, Inspector. All three inspections are complete. You may head home now."

*(She walks toward the last door.)*

**— LEVEL 3 END —**

---

# LEVEL 4
## Mia's Home

---

### Scene 4.1 — The Door: One Pending Item

```
[SCENE]
Location: Mia's front door, end of the corridor.
View: Mia, first person.
Trigger: Mia looks at the home terminal and presses E. Her view moves
      smoothly to its fixed camera. Space opens the pending item.
      The full exchange below remains on that terminal view. Once it
      finishes, the terminal image fades to black and Mia is placed
      in the living room before the image returns.
```

```
17F — RESIDENCE
GUARDIAN CONFIRMATION — pending
Logged 4:42 PM — audio only
```

*(She taps it. Audio plays.)*

<!-- HEARTH:SEQUENCES 17F04_HomeGreeting_High,17F04_HomeGreeting_Low -->
**Field Unit:** (calm, instructional) "Inspector, this is the full message your daughter left at 4:42 PM—the notification you received in the lobby. Please listen before entering."

<!-- HEARTH:SEQUENCES 17F04_HomeGreeting_High,17F04_HomeGreeting_Low -->
**Lily:** (hopeful, tentative, recorded) "Mom? Are you coming tomorrow? Ms. Parker said it'd be better if a real person came. Not just a system check-in."

*(A short pause on the recording. Then—)*

<!-- HEARTH:SEQUENCES 17F04_HomeGreeting_High,17F04_HomeGreeting_Low -->
**Mia's Home Unit:** (gentle, reassuring, recorded) "Mom will receive your progress report, and she'll make sure you have everything you need. Let's finish tonight's run-through first, okay?"

*(The recording ends.)*

<!-- HEARTH:SEQUENCES 17F04_HomeGreeting_High,17F04_HomeGreeting_Low -->
**Field Unit:** (even, procedural) "The reply was filed under 'attendance confirmation, bedtime routine, concurrent.' Standard handling. You were at 17F-01 when it was issued. Please complete the confirmation."

<!-- HEARTH:SEQUENCES 17F04_HomeGreeting_High,17F04_HomeGreeting_Low -->
**Mia:** (weary, quiet) "I know."

*(She confirms. The terminal view fades to black; the scene returns inside the living room. There is no intermediate flash back to Mia's corridor camera.)*

---

### Scene 4.2 — The Cat and the Frames

```
[SCENE]
Location: Mia's living room. Low light. No one comes to meet
      her — except the cat.
View: Mia, first person.
Cat: follows its authored route from the living-room start to the
      sofa, then lies down. It guides Mia's attention but does not
      gate or interrupt player movement.
Photo display: the TV4 frame is available as soon as the living room
      appears. Looking at it and pressing E moves to its fixed camera.
Trigger: The photo dialogue waits for any unfinished home greeting,
      then plays without overlapping it. Space or Esc returns after
      the photo sequence completes.
```

<!-- HEARTH:SEQUENCES 17F04_ChristmasPhoto -->
**Field Unit:** (soft, informative) "This one is from Christmas 2024. You took half a day off and came home for dinner. The kitchen timer took the picture. Lily was seven. That is a piece of turkey on her fork."

<!-- HEARTH:SEQUENCES 17F04_ChristmasPhoto -->
**Field Unit:** (soft, informative) "You are both looking at the camera. You are both smiling."

*(She sets it back. Picks up the second.)*

<!-- HEARTH:SEQUENCES 17F04_ChristmasPhoto -->
**Field Unit:** (soft, informative) "This one is from last week. Lily is under her desk lamp, holding a certificate. The household unit took the photo. It records one or two growth images each month and syncs them to you."

<!-- HEARTH:SEQUENCES 17F04_ChristmasPhoto -->
**Field Unit:** (even, analytical) "Her smile-stability score is higher in this photo than in the Christmas picture. Since the unit came online, Lily's overall emotional stability has increased by twenty-three point four percent."

<!-- HEARTH:SEQUENCES 17F04_ChristmasPhoto -->
**Field Unit:** (even, analytical) "That is one measurable benefit to this household."

*(Mia holds the frame a moment. She sets it back. The cat looks up at her; it doesn't follow.)*

<!-- HEARTH:SEQUENCES 17F04_ChristmasPhoto -->
**Field Unit:** (even, redirecting) "Inspector, Lily's room, when you're ready—"

*(It doesn't finish. Sound from down the hall covers it: a door ajar, warm lamplight through the gap, and voices.)*

---

### Scene 4.3 — Outside Lily's Door

```
[SCENE]
Location: The hallway outside Lily's room. Door ajar; warm
      light through the gap.
View: Mia, first person. She stops at the door and listens.
Sounds: Lily and the Home Unit, from inside.
```

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** (gentle, coaching) "Let's try it again. Slower this time."

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Lily:** (focused, nervous) "Hi, everyone. I'm Lily, and today I want to tell you about my..."

*(She stalls.)*

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Lily:** (nervous, recovering) "My favorite book."

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** (warm, encouraging) "Same spot. That's okay. Don't rush it. You're already doing better than yesterday."

*(A beat.)*

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Lily:** (quiet, cautious) "Will you be there tomorrow? Like, right next to me?"

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** (warm, certain) "I'll be in the audience. I'll be right there."

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Lily:** (small, uncertain) "What about Mom?"

*(Half a second.)*

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** (gentle, certain) "She'll be there too."

*(Mia's hand rests on the doorframe. Inside, Lily runs the line again — stalls at the same spot — the unit catches her — she tries again. This time it goes through.)*

<!-- HEARTH:SEQUENCES 17F04_HearingDaughterRoom -->
**Mia's Home Unit:** (pleased, encouraging) "Good. You got through that one on your own."

*(Mia pushes the door open.)*

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
**Mia's Home Unit:** (pleasant, neutral) "You're home."

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Lily:** (uncertain, hopeful) "Mom?"

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia:** (soft, reassuring) "Hey. It's me."

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Lily:** (cautious, searching) "Did you come in on your own this time?"

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia:** (calm, sincere) "I did."

*(Lily sets the paper down. She doesn't run to her. She sits very still and watches her mother.)*

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia's Home Unit:** (pleasant, routine) "We have one more run-through. Let's finish that, then we can talk about anything else. Okay?"

*(Lily doesn't look at it. She's still looking at Mia.)*

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Lily:** (quiet, careful) "Mom, I'm not asking about my speech."

*(Mia kneels down to her — knee to the rug, close to the creased paper. The unit stands two steps away. It waits.)*

*(Production note: from here, the Field Unit hands audio over to the Home Unit; the escort stays silent through the end of the scene.)*

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia's Home Unit:** (gentle, advisory) "Inspector, Lily's sleep will be more stable if I complete tonight's session. I recommend that we finish."

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Mia:** (restrained, firm) "I heard you."

*(The unit goes quiet. It doesn't offer again. Lily looks at her mother.)*

<!-- HEARTH:SEQUENCES 17F04_DaughterRoom_High,17F04_DaughterRoom_Low -->
**Lily:** (vulnerable, direct) "Mom. Are you coming tomorrow?"

*(She doesn't add "the teacher said" this time. She doesn't explain what she's asking for. The room holds: Mia, Lily, the unit, the creased paper, the lamp.)*

---

### Scene 4.5 — The Choice

```
[SCENE]
The screen offers exactly two actions. No scoring hints.
No HUD. Nothing else.
```

```
A. Answer her yourself
B. Let the unit answer for her
```

---

## PATH A — She Answers

*(Mia looks at her daughter.)*

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** (steady, simple) "I'll be there."

*(A beat. Lily's eyes stay on her.)*

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Lily:** (hopeful, testing) "Promise?"

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** (quiet, certain) "I promise."

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Lily:** (anxious, hesitant) "What if work—"

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** (gentle, firm) "No what-ifs, sweetheart. I promise."

*(Lily holds the look a long moment. Then she picks the creased paper back up — and hands it to Mia.)*

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Lily:** (hopeful, tentative) "Then can you listen? I want to do it for you this time."

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** (soft, attentive) "Yeah. Go ahead."

*(She takes the paper. She doesn't smooth the creases. Two steps away, the unit doesn't move. No coaching prompt. Nothing.)*

*(Lily takes a breath and begins. Midway, she stalls — the same spot as this afternoon. Nobody fills the silence. Not Mia. Not the unit. A few seconds pass. Lily finds the next line herself, and finishes.)*

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** (warm, sincere) "That was really good."

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Lily:** (uncertain, seeking reassurance) "I did it by myself?"

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Mia:** (warm, certain) "Every word."

*(Lily takes the paper back and presses it flat on her knees. She looks at Mia — then at the unit.)*

<!-- HEARTH:SEQUENCES 17F04_AnswerSelf -->
**Lily:** (quiet, concerned) "What happens to it?"

---

### Scene 4.6 — Shutdown

*(Mia looks at the unit — directly, for the first time tonight.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High,17F04_Shutdown_Low -->
**Mia:** (calm, resolute) "I'm shutting you down."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High,17F04_Shutdown_Low -->
**Mia's Home Unit:** (calm, accepting) "Okay."

*(It doesn't ask why. It doesn't ask if she's sure. Then, to Lily—)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High,17F04_Shutdown_Low -->
**Mia's Home Unit:** (gentle, reassuring) "Lily, your mom is here now. I can go."

*(Lily doesn't move. She watches it. Mia takes out her terminal. The screen lights.)*

---

#### Scene 4.6a — High Trust: A Proper Goodbye

*(Production note: plays when cumulative trust is positive — +1 or +3. The two positive totals behave identically.)*

```
SHUT DOWN HOUSEHOLD COMPANION UNIT — confirm?
[ CONFIRM ]   [ CANCEL ]
```

*(That's all. The system doesn't argue tonight. She confirms.)*

```
APPROVED.
Allow the household unit its final accompaniment.
```

*(The unit's light blinks once. Then it kneels — its storytime posture, eye level with Lily.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia's Home Unit:** (gentle, unhurried) "Lily, before I go, I need you to hear something."

*(Lily nods.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia's Home Unit:** (soft, sincere) "When it thunders, you get scared. You tell me. You don't tell Mom. Starting tonight, tell her. She'll come."

*(Lily glances at Mia.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia's Home Unit:** (soft, careful) "And the drawing from yesterday. It is in the notebook in your second desk drawer. You want to show Mom, but you're afraid it is not good enough. She won't think it is bad."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Lily:** (embarrassed, indignant) "Why would you tell her that?"

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia's Home Unit:** (gentle, matter-of-fact) "Because once I am gone, no one will be here to say it for you."

*(It turns to Mia.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia's Home Unit:** (calm, direct) "Mia."

*(Not "Inspector." Not "ma'am." The first time in three years it has used her name.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia's Home Unit:** (calm, solemn) "The rest is up to you now. She needs you."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia:** (subdued, accepting) "Okay."

*(Its lights dim slowly — no collapse, no slack joints. It stays kneeling, the glow stepping down like someone hanging up a coat and walking into another room. At the last light, its head dips slightly — not a slump; something very close to a small bow. Then it is still.)*

*(Lily watches it a long time. She reaches out and touches its shoulder — the spot where a little light used to come on every day. It's cool now.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Lily:** (sad, holding it in) "Mom... I already miss it."

<!-- HEARTH:SEQUENCES 17F04_Shutdown_High -->
**Mia:** (tender, comforting) "I know, baby."

---

#### Scene 4.6b — Low Trust: A Forced Shutdown

*(Production note: plays when cumulative trust is negative — −1 or −3. The two negative totals behave identically.)*

```
SHUT DOWN HOUSEHOLD COMPANION UNIT — confirm?
[ CONFIRM ]   [ CANCEL ]
```

*(She confirms. The screen flashes red.)*

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

*(She presses YES. Red, one last time.)*

```
FINAL CONFIRMATION:
this shutdown will not include the farewell protocol.
Force the operation?
```

*(Her finger stops over the button. She looks at Lily. Lily can't see the screen — only her mother's face.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_Low -->
**Lily:** (uncertain, quiet) "Mom?"

*(Mia presses YES.)*

*(Two steps away, the unit stops mid-stillness. Its indicator flashes twice. It turns its head toward Mia — there's no time to kneel, no time for eye level. It manages one look at Lily. Then its lights go out, section by section, top to bottom.)*

*(Lily is up and across the room before the last light dies. She crouches in front of it. She doesn't cry. She just looks at it. Then she turns to her mother.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_Low -->
**Lily:** (hurt, controlled) "Mom, did you turn it off?"

*(Mia kneels down onto the rug with her.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_Low -->
**Mia:** (steady, accountable) "Yeah. I did."

*(Lily says nothing. She looks once more at the dark unit. Mia reaches out and gathers her in.)*

<!-- HEARTH:SEQUENCES 17F04_Shutdown_Low -->
**Mia:** (gentle, sincere) "I'm gonna be here more. I promise."

*(Lily doesn't move at first. Then she rests her head on Mia's shoulder. She doesn't cry. She just leans.)*

---

### Scene 4.7 — Black Screen: After (Path A)

```
[SCENE]
Screen: full black. No music. Sounds and voices only,
      one small scene at a time.
```

*[SFX: morning. A kitchen. A spatula against a pan.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**TIME CARD:** "MORNING - KITCHEN"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** (playful, bossy) "Mom, don't cook the eggs so long this time."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** (playful, agreeable) "Got it."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** (dry, deadpan) "You burned them."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** (sheepish, apologetic) "I know. Sorry."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** (amused, forgiving) "It's okay. I'll eat them."

*[SFX: daytime. Keys landing on a table.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**TIME CARD:** "DAYTIME - HOME"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** (casual, calling out) "I'm home! Lily?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** (surprised, calling back) "In my room! You're home early!"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** (easy, conversational) "Yeah. The meeting ended, so I got out of there. How was school?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** (hesitant, guarded) "It was okay."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** (curious, gently probing) "What kind of okay?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** (hesitant, opening up) "Just... okay. Come here, Mom. I'll tell you."

*[SFX: night. A child's room. Thunder, far away.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**TIME CARD:** "NIGHT - LILY'S ROOM"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** (small, frightened) "Mom?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** (soft, immediate) "I'm right here."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** (sleepy, puzzled) "How'd you know I was awake?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** (soft, lightly playful) "Lucky guess."

*(A long pause. The thunder rolls, further off now.)*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** (very quiet, vulnerable) "Mom?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Mia:** (soft, attentive) "Yeah, baby?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Shutdown,17F04_Epilogue_Low_Shutdown -->
**Lily:** (quiet, sincere) "Thanks for coming. You, I mean."

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
**Mia's Home Unit:** (gentle, reassuring) "Mom will receive your progress report, and she'll make sure you have everything you need. Let's finish tonight's run-through first, okay?"

*(Production note: word for word, the same answer from the 4:42 PM recording.)*

*(Lily doesn't look at it. She is still looking at Mia.)*

*(Mia's mouth opens again. Nothing comes.)*

*(Lily looks at her for a long time. Then, very softly—)*

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Lily:** (withdrawn, monotone) "Mm."

*(She sets the creased paper aside. She doesn't ask to read it again. She doesn't ask "you promise?" She won't ask again tonight.)*

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Lily:** (withdrawn, controlled) "Okay. Let's just finish."

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Mia's Home Unit:** (warm, routine) "Okay. Start with your name. Nice and slow."

*(Mia stands. She steps back to the door. The unit doesn't look at her — its full attention is on Lily. She steps out and closes the door behind her.)*

*(Through the door:)*

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Lily:** (focused, muffled) "Hi, everyone. I'm Lily, and today I want to tell you about my favorite book..."

*(She stalls — the same spot.)*

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Mia's Home Unit:** (warm, muffled) "That's okay. Better than yesterday."

*(She goes again.)*

```
[SCENE]
The living room. The pinned glow of the Field Unit returns
to the lens.
```

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Field Unit:** (pleasant, professional) "Inspector, all reviews and the guardian confirmation are complete. Household stability is back in the safe range. Tonight's shift is in the top performance band. You are cleared to log off."

<!-- HEARTH:SEQUENCES 17F04_CompanionAnswer -->
**Field Unit:** (soft, approving) "You did well tonight."

*(Mia doesn't answer. The terminal on the coffee table goes dark on its own. The cat jumps up and settles against her leg. Behind the door, Lily runs her name again. And again.)*

---

### Scene 4.8 — Black Screen: After (Path B)

```
[SCENE]
Screen: full black. No music. Sounds and voices only.
```

*[SFX: next morning. A kitchen.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**TIME CARD:** "THE NEXT MORNING - KITCHEN"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** (bright, caring) "Lily, your mom is leaving a little later today. Eat something first. I'll sit with you."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (withdrawn, monotone) "Mm."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** (gentle, caring) "She asked me to tell you everything is packed. It is by the door."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (withdrawn, monotone) "Mm."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** (warm, reassuring) "Good luck at the open house."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (withdrawn, monotone) "Mm."

*(From another room, Mia's voice — low, on a call.)*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia:** (businesslike, muffled) "Right, I'll be in by nine-thirty. Yeah, the open house. I'll have the unit record it. Fine. That works."

*[SFX: afternoon. A school gymnasium hum, distant.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**TIME CARD:** "AFTERNOON - SCHOOL OPEN HOUSE"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** (gentle, reassuring) "Lily, your mom couldn't make it today. I watched for her. You did really well."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (withdrawn, delayed) "Mm."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** (warm, reassuring) "She asked me to tell you she is proud of you."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (quiet, cautious) "Did she actually say that?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** (calm, steady) "She did."

*(A longer pause.)*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (quiet, skeptical) "What you just said... is that what she said, or is that what you made it sound like?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** (hesitant, careful) "It is what she meant."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (hurt, emotionally flat) "Oh."

*[SFX: three years later. A front hall. Lily's voice is older. Cooler.]*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**TIME CARD:** "THREE YEARS LATER - FRONT HALL"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (even, restrained) "I'm moving into the dorms."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia:** (shocked, stumbling) "What? That's... that's sudden."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (matter-of-fact, cool) "It isn't. I applied six months ago."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia:** (hurt, quiet) "Why didn't you tell me?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (controlled, matter-of-fact) "I told the unit. It told you. Three times."

*(A beat.)*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia:** (small, defensive) "I thought those were routine updates."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Lily:** (quiet, final) "I know."

*[SFX: suitcase wheels over the doorstep. The door closes.]*

*(The living room stays quiet for a long time. Then—)*

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** (gentle, unchanged) "Mia, Lily has left. She asked me to tell you she will be back for the holidays."

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia:** (hurt, cautious) "Did she actually say that?"

<!-- HEARTH:SEQUENCES 17F04_Epilogue_High_Retain,17F04_Epilogue_Low_Retain -->
**Mia's Home Unit:** (calm, literal) "Yes."

*(One block of text fades in on the black:)*

```
HOUSEHOLD EMOTIONAL STABILITY: SAFE ZONE
Unit service length: 6 years — high-satisfaction household
Next review: in 3 years
```

*(The text fades. Black holds a few seconds. End.)*

**— LEVEL 4 END —**

**— END OF SCRIPT —**
