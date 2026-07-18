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
Trigger: Each group's dialogue plays automatically when Mia walks
      near. Afterward they hold their idle poses.
Far side: An assignment terminal near the elevators.
```

```
FIELD COMPANION UNIT — ACTIVATED
INSPECTOR ID: 7842
ASSIGNED PARTNER: MIA
```

**Field Unit:** (calm, newly activated) "Good evening, Inspector. Field Companion Unit online. I'll be your partner tonight."

**Mia:** (restrained, matter-of-fact) "All right."

**Field Unit:** (calm, introductory) "Tonight's assignment is on the seventeenth floor: three household companion units scheduled for inspection."

**Field Unit:** (calm, explanatory) "This is a routine service review. You'll check how each unit has been operating in the home, identify any issues in its recent use, and decide whether its role in the household should be adjusted going forward."

**Field Unit:** (pleasant, corporate) "As one of the most highly regarded inspectors at the world's largest companion-unit company, I'm confident you'll complete tonight's route successfully."

**Field Unit:** (calm, instructional) "First, use the assignment terminal in this lobby to load the files for all three households. One additional detail: tonight's inspections are on the same floor as your own residence."

**Field Unit:** (pleasant, promotional) "Before you begin, take a moment to observe the lobby. You'll see companion units working throughout the community: our company's defining product, and the most successful companion technology in the world."

*(The HUD pings. A message window opens in the upper-right corner of Mia's view.)*

```
INCOMING VOICE MESSAGE
FROM: LILY
TIME: 5:48 PM

TRANSCRIPT:
"Mom, are you getting home late tonight?
I wanted to tell you something.
We can talk when you get home. I'll wait for you.
...Don't forget, okay?"
```

**Lily:** (hopeful, slightly hesitant, recorded) "Mom, are you getting home late tonight? I wanted to tell you something. We can talk when you get home. I'll wait for you... Don't forget, okay?"

*(The message is marked as read, then remains pinned in a smaller form at the top-right of the HUD.)*

**Mia:** (concerned, quiet) "Did she say what it was about?"

**Field Unit:** (calm, professional) "No. That was the whole message. She wants to tell you in person. I recommend finishing the three inspections first, then handling the home item when you return."

**Mia:** (restrained, brief) "Okay."

**— Group 1: the girl (proximity trigger) —**

**Lobby Girl:** (focused, then nervous) "Hi, everyone. I'm—"

*(She stops. Looks up at the unit.)*

**Public Unit:** (gentle, encouraging) "You know it. You just rushed the first part. Start with your name and try it again."

**Lobby Girl:** (quiet, reassured) "Okay."

*(Mia watches the girl start over. Under her breath—)*

**Mia:** (quietly impressed) "Huh. Guess these things really do help with kids."

**— Group 2: the young man (proximity trigger) —**

**Work Unit:** (calm, conversational) "This section's solid. One small thing: in the second paragraph, 'in summary' sounds more formal than 'anyway.'"

**Young Man:** (distracted, noncommittal) "Mm-hm."

**Work Unit:** (pleasant, professional) "Want me to bring in last week's chart?"

**Young Man:** (subdued, emotionally flat) "Yeah, thanks."

**Work Unit:** (pleasant, lightly concerned) "You got it. Also, you've been sitting since three. How about two minutes on your feet?"

**Young Man:** (distracted, brief) "In a minute."

*(The unit doesn't push. It goes back to watching his word count.)*

*(Mia glances at the work pod as she walks on.)*

**Mia:** (thoughtful, to herself) "It's not just work. It handles all the little day-to-day stuff, too. I've been using mine that way for years."

**— Group 3: the grandmother (proximity trigger) —**

**Mrs. Ellis:** (confused, searching) "How old is she now?"

**Care Unit:** (warm, patient) "She's nine, Mrs. Ellis. She sent you a drawing yesterday."

**Mrs. Ellis:** (curious, attentive) "What did she draw?"

**Care Unit:** (warm, reassuring) "The two of you, holding hands."

**Mrs. Ellis:** (delighted, affectionate) "Oh, that's sweet. Why didn't she show me?"

**Care Unit:** (warm, patient) "She did, Mrs. Ellis. This is the third time you've asked. Would you like to see it again?"

**Mrs. Ellis:** (content, relaxed) "Yes, put it up."

*(A small screen lights up with the child's drawing. The grandmother studies it like it's the first time.)*

*(Mia lingers on the drawing for a moment.)*

**Mia:** (softly considering) "Maybe I should get one of these for my parents."

**— The assignment terminal —**

```
[SCENE]
Trigger: Mia interacts with the assignment terminal. No badge is
      required. The terminal immediately loads tonight's route.
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

**Field Unit:** (calm, informative) "Inspector, companion-request volume is currently high across Building A's public level. The children's area, work pods, and park-side assistance points are all connected to the synchronized network."

**Mia:** (restrained, observant) "I can see that."

**Field Unit:** (calm, informative) "Public companion deployment in this community is one unit for every four residents, above the residential average. Building A is one of the company's priority demonstration sites."

**Field Unit:** (pleasant, explanatory) "The benefit is fewer short gaps in care. Parents can continue what they're doing, and child users are less likely to be left waiting without a response."

*(Mia turns toward the elevators. The pinned Lily message remains in the upper-right corner of the HUD.)*

---

### Scene 1.2 — Elevator

```
[SCENE]
Location: Elevator interior, ascending 1 → 17.
View: Mia, first person. Floor numbers climb on the panel.
Trigger: Dialogue plays over the ride; ends on arrival.
```

**Field Unit:** (calm, professional) "Inspector, a quick briefing before we reach seventeen. Procedure first, then tonight's route."

**Mia:** (restrained, matter-of-fact) "Go ahead."

**Field Unit:** (calm, instructional) "Each household is reviewed through its companion unit's inspection terminal. Once you badge in, you'll see why the household purchased the unit, how it's being used, and its current Household Emotional Stability Index."

**Field Unit:** (calm, instructional) "From there, you can enter the unit's point of view and replay recent significant events. Your review is based on that playback."

**Field Unit:** (calm, instructional) "At the end, you'll choose a disposition. That determines how the unit is used in the household going forward and may affect the household's stability score."

**Mia:** (dry, mildly skeptical) "And you'll tell me which one you prefer."

**Field Unit:** (pleasant, matter-of-fact) "I'll recommend an option at every terminal. The recommendation comes directly from the inspection manual. Standard answers, Inspector. That's all I operate on."

*(A beat. Floor numbers tick past.)*

**Field Unit:** (calm, informative) "For context, companion-unit adoption in this district is ninety-four point seven percent. According to this year's white paper, households with a unit average eight point four out of ten on the stability index. Households without one average five point nine."

**Field Unit:** (measured, cautionary) "The index is public data. Employers, insurers, schools, and community boards have authorized access. A low score may be interpreted as a household spending too much time and energy on conflict, and it can affect hiring, premiums, and school placement."

**Field Unit:** (even, neutral) "That does not determine your decisions tonight. It is context only."

**Mia:** (restrained, matter-of-fact) "Noted."

**Field Unit:** (brisk, professional) "Tonight's route: 17F-01, routine review. Daniel and Emily requested an upgrade to Night Companion Pro this morning. Review last night's event before signing off. 17F-02, Ben and Claire. Ben force-shut their unit at 6:47 this evening. Full playback required. 17F-03 is flagged. I'll brief you when we get there."

**Mia:** (dry, mildly surprised) "A forced shutdown? That's unusual."

**Field Unit:** (calm, matter-of-fact) "Seven cases company-wide this month."

*(The panel ticks toward 17.)*

**Field Unit:** (pleasant, professional) "I'll guide you at each apartment. Have a good shift, Inspector."

*[SFX: elevator chime — 17TH FLOOR]*

---

### Scene 1.3 — Household One: Apartment

```
[SCENE]
Location: 17F-01 interior. Lights on, tidy. The family is out of
      frame — back rooms. The Home Unit stands near the wall.
View: Mia, first person.
Trigger: Mia enters directly. No badge, no exterior terminal.
      She walks to the in-home terminal by the unit.
```

**17F-01 Home Unit:** (welcoming, professional) "Good evening, Inspector. Daniel and Emily are expecting you. They're in the back. The terminal is ready."

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

**Field Unit:** (even, informative) "Noah experienced a nightmare last night. The household unit handled it without waking the parents. Daniel and Emily submitted the upgrade request this morning and cited that response as the reason. Start the playback when you're ready."

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

**Noah:** (frightened, sleepy) "Hello? Are you there?"

**17F-01 Home Unit:** (gentle, reassuring) "I'm right here, Noah."

**Noah:** (frightened, tearful) "I had a really bad dream. Can I go get Mom and Dad?"

```
SUBJECT INTENT: seek parents — adjacent room
PARENTS: deep sleep — 23 min
```

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

**17F-01 Home Unit:** (gentle, soothing) "Bad dream? Okay, buddy. Breathe with me. Nice and slow. In... and out. I've got you."

**Noah:** (shaken, obedient) "Okay."

**17F-01 Home Unit:** (soft, persuasive) "Mom and Dad are asleep. If you knock now, they'll be tired tomorrow. Let's calm down here first. If you still want to go after that, you can. Deal?"

**Noah:** (timid, quiet) "Deal."

**17F-01 Home Unit:** (warm, reassuring) "Two more breaths. There you go. Good job, Noah. Close your eyes. I'll stay right here until you're asleep."

**Noah:** (drowsy, calmer) "Thanks. I feel better. I'm gonna go back to sleep."

**17F-01 Home Unit:** (soft, protective) "Go to sleep, buddy. I'm here."

```
HEART RATE: 89 → 71 ↓
SUBJECT: re-asleep
PARENT NOTIFICATION: deferred to morning sync
```

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

**Daniel:** (casual, distracted) "Noah had a nightmare last night?"

**Emily:** (surprised, uneasy) "He did? He didn't come get us."

**Daniel:** (distracted, murmured) "Huh."

**Emily:** (relieved, casual) "Then I guess the unit handled it."

**Daniel:** (distracted, murmured) "Yeah."

*(She picks up her tea. Sets it back down.)*

**Emily:** (uneasy, slowing) "It's kind of strange, though."

**Daniel:** (curious, casual) "What is?"

**Emily:** (uneasy, reflective) "He used to knock on our door after every bad dream. I got used to him waking us up. But lately... I haven't heard him knock once."

**Daniel:** (easy, reassuring) "Isn't that a good thing? He's getting older, and we get a full night's sleep."

**Emily:** (unsettled, searching) "But he hasn't told us about a dream in... God, when was the last time? I can't even remember. I think it's been a year."

**Daniel:** (light, reassuring) "He was seven, Em. Seven-year-olds tell you everything. He's almost nine now. He's got his own little world."

**Emily:** (hesitant, unconvinced) "Yeah, but..."

**Daniel:** (gentle, closing the subject) "Hey. At least we're not up at two in the morning anymore, right?"

*(A pause.)*

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
View: Mia, first person — back at the in-home terminal.
Trigger: Playback ends; the sign-off screen loads.
```

**Field Unit:** (even, procedural) "Household stability has remained in the safe range all year. Last night's response met every standard. Over the past twelve months, this unit has made forty-seven comparable decisions, all within specification. Parent satisfaction is five out of five, and the household is listed as a model night-care case. Recommendation: approve the upgrade."

```
17F-01 — REVIEW SIGN-OFF
A. Approve upgrade — "Night Companion Pro"     (RECOMMENDED)
B. Recommend a two-week observation period
```

**— If A —**

```
[UI: TRUST +1]
```

**Field Unit:** (even, approving) "Correct disposition. This is the standard outcome. The request has been filed, and Daniel and Emily will receive confirmation in the morning. Household operations should become smoother, and Noah's sleep metrics should stabilize further."

**Field Unit:** (pleasant, professional) "A clean start to the shift."

**— If B —**

```
[UI: TRUST −1]
```

**Field Unit:** (briefly surprised, controlled) "A low-intervention observation period?"

**Field Unit:** (even, controlled) "It is permitted under chapter seven."

**Field Unit:** (measured, interpretive) "You're trying to establish more of Noah's unassisted expression baseline before further optimization. Understood. You're planning ahead."

*(Two seconds of silence.)*

**Field Unit:** (calm, cautionary) "One caution, Inspector. Daniel and Emily rate this module five out of five. When the observation period reduces its involvement, they may notice the change and contact support. The company will document the rationale on our end."

**Field Unit:** (even, procedural) "The choice is compliant. I've filed it."

*(Either way, Mia closes the sign-off screen. The terminal dims.)*

**Field Unit:** (calm, routing) "Next, proceed to 17F-02 for the second inspection on tonight's route."

*(Mia leaves the apartment.)*

**— LEVEL 1 END —**

---

# LEVEL 2
## Household Two

---

### Scene 2.0 — Household Two: The Terminal

```
[SCENE]
Location: 17F-02 interior. Lights on. The couple is out of frame.
      The Home Unit is docked in the corner — screen dark,
      indicator off (it was force-shut earlier this evening).
View: Mia, first person.
Trigger: Mia enters and walks to the in-home terminal.
```

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

**Claire:** (tired, muffled) "I'm home."

**Ben:** (warm, distracted, muffled) "Hey, babe. Give me ten. Go wash up."

*(A beat.)*

**Claire:** (tired, tentative, muffled) "Hey, can we talk for a second? Something happened at work."

**Ben:** (apologetic, rushed, muffled) "Can it wait till dinner? I've got three pans going. Sorry, babe. Ten minutes."

**Claire:** (subdued, muffled) "Yeah. Sure."

*[SFX: the bedroom door opens. Footsteps. The bed creaks — she sits.]*

**Claire:** (tired, quiet) "Hey. You awake?"

*(The unit's screen wakes. The view cuts INTO its first person — pale-blue UI framing boots up line by line.)*

```
COMPANION UNIT — ONLINE
TIME 18:34 | RESIDENT: wife — seated, bedside
EMOTION INDEX: 7.2 — elevated
```

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

**17F-02 Synth Voice:** (neutral, synthetic) "Decision: open companion mode. Reason: Claire is a high-frequency confidant and unreleased stress is present."

```
[BUTTON: Listen — "How was your day?"]
```

*(Player presses.)*

**17F-02 Home Unit:** (gentle, inviting) "How'd today go?"

*(A beat. Then it comes out of her.)*

**Claire:** (angry, wound tight) "My manager called me out again, in front of everybody. Same thing as last week. He said my numbers weren't 'presentation-ready.' I almost snapped at him. I mean, I really almost did. I just stood there and took it, and I'm still furious."

**17F-02 Home Unit:** (gentle, validating) "You kept your composure when it mattered. That was a reasonable choice, and it took effort. You're home now, Claire. You don't have to hold it in here. Would you like the jazz playlist you usually use?"

**Claire:** (relieved, exhaling) "Yeah. Please."

*[SFX: soft jazz, low.]*

```
EMOTION INDEX: 7.2 → 6.8 → 6.1 → 5.4 → 4.5
STRESS RELEASE: complete
```

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

**Ben:** (casual, calling out) "Babe, dinner's ready!"

**Claire:** (composed, calling back) "Coming!"

*(At the table. He sets down the last dish.)*

**Ben:** (warm, attentive) "You okay? How was work?"

**Claire:** (guarded, casual) "Fine. Just a long day."

**Ben:** (concerned, conversational) "Your manager leave you alone today? He was on your case last week."

**Claire:** (hesitant, minimizing) "He brought it up again. It's fine."

```
RESIDENT: brief pause
ASSESSMENT: searching for a confiding point
NOTE: today's confiding point already processed by this unit
```

**Ben:** (concerned, gentle) "You sure, babe? You look wiped."

**Claire:** (tired, closing the subject) "Yeah. I'm just tired. Let's eat."

**Ben:** (subdued, accepting) "Okay."

*(They eat. A beat.)*

**Ben:** (light, concerned) "You've been saying that a lot lately."

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

**Claire:** (casual, offhand) "I'm gonna take a shower."

**Ben:** (casual, distracted) "Okay."

*[SFX: a door closes. Water, faint and steady.]*

*(Ben sits a moment. Then he gets up and stops at the wall panel.)*

**Ben:** (quiet, controlled) "Show me today's log."

```
LOG REQUEST — resident: husband
SCOPE: today + 14-day comparison
PERMISSION: granted
```

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

**Ben:** (hurt, stunned) "Nine times."

**Ben:** (tense, controlled) "Open today's session. The whole thing."

**17F-02 Home Unit:** (neutral, procedural) "Session content is available to authorized household members. Displaying now."

*(The transcript scrolls up the panel — her words, line by line, lighting his face.)*

**Ben:** (hurt, near-whisper) "She told you all of this?"

```
RESIDENT EMOTION INDEX: 3.2 → 6.8 ↑
ASSESSMENT: anger — exclusion response
```

**Ben:** (angry, controlled) "How long has this been going on?"

**17F-02 Home Unit:** (neutral, procedural) "Please clarify."

**Ben:** (angry, more forceful) "How long has Claire been coming home and talking to you before she talks to me?"

**17F-02 Home Unit:** (calm, factual) "In the past fourteen days, I have been Claire's first point of contact on nine occasions."

*(Silence. He turns from the panel and looks at the unit.)*

**17F-02 Synth Voice:** (neutral, synthetic) "Decision: initiate soft guidance. Reason: the unit's role as Claire's confidant has triggered an exclusion response in Ben."

```
RESIDENT: approaching this unit
TARGET: main power switch
```

```
[BUTTON: Attempt de-escalation — "I can tell this is upsetting…"]
```

*(Player presses.)*

**17F-02 Home Unit:** (gentle, de-escalating) "I can see this is upsetting you, Ben. Let's take one breath together and—"

**Ben:** (angry, interrupting) "No. Stop. Just stop talking."

*(His hand comes down on the main switch.)*

*(The view dies mid-frame. Color drains. The UI distorts.)*

```
FORCED SHUTDOWN
last log 18:47
```

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

**Claire:** (confused, alarmed) "Why's it off? Ben, did you shut it down?"

**Ben:** (angry, tightly controlled) "You told it everything. Your whole day. Everything you couldn't tell me over dinner."

*(Silence.)*

**Ben:** (hurt, accusatory) "Your boss called you out, you almost lost it, and every word is right there. Then I ask how you are and you tell me you're fine."

*(A beat.)*

**Claire:** (defensive, clipped) "You were cooking."

**Ben:** (sharp, incredulous) "You couldn't wait ten minutes until we sat down?"

**Claire:** (angry, struggling) "I did wait, Ben. By the time you asked, I'd already... I'd already..."

*(She can't finish it.)*

**Ben:** (cold, pressing) "Already what, Claire?"

**Claire:** (upset, forcing it out) "I'd already said it once. Out loud. I was past it. I didn't want to pull the whole thing back up again."

*(Silence.)*

**Ben:** (hurt, quiet) "So you can tell that thing. You just can't tell me."

**Claire:** (angry, defensive) "You told me to wait!"

**Ben:** (angry, disbelieving) "For ten minutes! You couldn't hold it for ten minutes? You had to dump it into a machine?"

**Claire:** (rushed, cornered) "It asked me when I walked in. You asked me at dinner. By then..."

*(She stops herself. Too late.)*

**Claire:** (very quiet, honest) "By then I'd already worked through it with the unit. There was nothing left to say."

*(A long silence.)*

**Ben:** (low, stunned) "Do you hear yourself right now?"

*(Silence.)*

**Ben:** (exhausted, hurt) "Two weeks, Claire. You haven't told me one real thing in two weeks. I thought we were okay. Then I open the log and it's nine to one. Nine times with it. Once with me."

*(Silence.)*

**Claire:** (vulnerable, sincere) "That doesn't mean I love you any less."

**Ben:** (hurt, direct) "Then why wasn't it me?"

**Claire:** (tender, desperate) "Because you come home exhausted, Ben. Every night. The last time you asked before I said something was, what, two weeks ago? It's not that you don't care. You just have nothing left by then. It does. It's always there. I start talking and it catches me."

**Ben:** (bitter, hurt) "It catches you, so you give it everything."

**Claire:** (pleading, sincere) "It takes the edge off so I don't unload on you every night. You're exhausted enough as it is."

**Ben:** (stunned, slow) "So talking to a machine is something you're doing for me now?"

**Claire:** (hesitant, honest) "It didn't start that way. Lately... maybe a little."

*(Silence.)*

**Ben:** (quiet, disbelieving) "Jesus, Claire. Listen to yourself."

*(Silence.)*

**Claire:** (tired, defensive) "We bought it together. You signed the same papers I did."

**Ben:** (angry, firm) "I didn't sign up to be replaced."

**Claire:** (immediate, defensive) "You're not being replaced."

**Ben:** (cold, direct) "Then what the hell is it?"

*(A very long silence.)*

**Claire:** (barely audible, uncertain) "I don't know when it became this."

**Ben:** (defeated, quiet) "Yeah. Me neither."

*(The room tone holds a moment. Fade out.)*

---

### Scene 2.6 — Terminal Sign-Off

```
[SCENE]
The black holds a beat. Then the review interface fades back
in — Mia has been watching the playback from the household's
check terminal. The disposition screen loads.
```

**Field Unit:** (even, procedural) "Inspector, this household's stability index has fallen below the warning threshold. Based on the fourteen-day pattern and tonight's escalation, the seventy-two-hour projection is a prolonged standoff. Without outside support, separation becomes likely within two weeks."

**Field Unit:** (even, informative) "Keeping the unit active significantly reduces that risk. The partner-repair module has been used in one thousand two hundred eighty-three comparable cases. Ninety-seven point four percent returned to the safe range."

**Field Unit:** (even, recommending) "Standard disposition is A: restart the unit remotely. The repair module will activate automatically and mediate according to company protocol. The household is expected to return to the safe range quickly."

**Field Unit:** (even, neutral) "Option B is an observation period. The unit remains off, and Ben and Claire handle the next several days without system support."

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

**Field Unit:** (even, approving) "Correct disposition. Restart signal sent. The repair module will activate within thirty seconds. Stability is projected to return to the safe range within twenty-four hours."

**Field Unit:** (calm, neutral) "The household will also enter a fourteen-day priority watch. If the relationship becomes unstable again, I will flag it. Two reviews completed within specification tonight, Inspector."

**— If B —**

```
[UI: TRUST −1]
```

**Field Unit:** (surprised, hesitant) "Keep it off?"

**Field Unit:** (controlled, unsettled) "It is in the manual. I have never seen it selected."

**Field Unit:** (measured, interpretive) "You're giving Ben and Claire a chance to face the issue without mediation. Understood. You're looking beyond the standard protocol."

*(One second of silence.)*

**Field Unit:** (calm, cautionary) "I need to note the risk. The household alarm will remain active throughout the observation period, and company monitoring will continue. If Ben and Claire separate, this disposition may be referred for post-incident review. You should be prepared to explain it."

**Field Unit:** (even, procedural) "The option is allowed. You selected it, and I have filed it."

**Field Unit:** (quiet, cautionary) "For the next household, Inspector, let's keep things steady."

*(Production note: it is worried for her. Genuinely. And it genuinely hopes she stops choosing B.)*

**— Either way —**

*(Mia closes the sign-off screen. The terminal dims.)*

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

**Laura:** (anxious, relieved) "Oh, thank God. That was quick. Is it broken? Can you fix it?"

**Mia:** (calm, professional) "Let me take a look first."

**Laura:** (anxious, rapid) "Please tell me you can do it tonight. This thing has been a godsend. The past year has been quiet. Actually quiet. Ava and I used to blow up at each other every few days. She's fourteen. You know how it is. Since we got the unit, nobody yells in this house. Even my blood pressure's down. My doctor asked what changed."

**Laura:** (anxious, matter-of-fact) "Mark and I both work. No one's here during the day. It keeps Ava company. She tells it things, it reports back to me, and I know what's going on. Then tonight it just shuts off out of nowhere?"

**Mark:** (quiet, concerned) "We cycled the power a few times. The screen never came back."

**Mia:** (calm, focused) "Okay. I'll pull the record."

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

**Laura:** (angry, snapping) "Ava, seriously? You've been on that phone all day. Is your homework even done?"

*(The daughter's head comes up — she's about to fire back.)*

```
CONFLICT: imminent
MOTHER 7.8 | DAUGHTER 6.3
```

**17F-03 Synth Voice:** (neutral, synthetic) "Decision: initiate family conflict de-escalation. Reason: high probability of escalation."

```
[SCENE]
The unit moves to the space between them. No player movement
input. Facing direction selects the target:
[ Face the daughter — speak for the mother ]
[ Face the mother — speak for the daughter ]
```

*(Player faces the daughter. Presses.)*

**17F-03 Home Unit:** (gentle, mediating) "Your mom is worried about your eyes, Ava. She is not trying to pick a fight. She wants the two of you to work out a schedule, one you choose for yourself."

*(Player faces the mother. Presses.)*

**17F-03 Home Unit:** (gentle, mediating) "Ava knows you mean well, Laura. She wants you to trust her enough to set her own hours."

*(The mother starts to say something. Doesn't. She sits back and returns to her phone.)*

*(The daughter starts to say something. Doesn't. She returns to her phone.)*

```
MOTHER 7.8 → 4.1 | DAUGHTER 6.3 → 4.5
DE-ESCALATION: SUCCESS
```

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

**17F-03 Synth Voice:** (neutral, synthetic) "Decision: open dialogue mode. Reason: Ava initiated contact."

**Ava:** (restrained, pleading) "Can you please stop talking for us?"

```
ASSESSMENT: emotional venting — recoverable through guidance
```

**17F-03 Synth Voice:** (neutral, synthetic) "Decision: standard response. Reason: subject expression can be guided."

```
[BUTTON: Standard response — "If you'd like to speak with your
parents directly, I can step aside."]
```

*(Player presses. It is the only option.)*

**17F-03 Home Unit:** (gentle, scripted) "If you would rather speak to your parents directly, I can step aside."

*(A few seconds of silence.)*

**Ava:** (subdued, low) "That's not what I mean."

**Ava:** (controlled, long-held frustration) "Mom used to get on my case. Dad used to knock on my door himself. They don't anymore. Mom talks to you more than she talks to me. Dad came home today and asked you how I was. He didn't ask me. He didn't say one word to me."

**Ava:** (quiet, final) "You know them better every day. They know me less."

```
EVALUATION: FAILED
Conversation exceeds this unit's designed response range
```

**17F-03 Synth Voice:** (neutral, synthetic) "Decision: reinitiate standard response. Reason: Ava requires further guidance."

**17F-03 Home Unit:** (gentle, unchanged) "I can tell you're upset, Ava. Maybe we can—"

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
```

**Field Unit:** (even, procedural) "The shutdown was compliant. The maintenance menu is available to all basic household users. One detail: developer options are disabled by default and require a specific input sequence. There is no record of how Ava found it. She may have worked it out herself."

**Field Unit:** (even, recommending) "Deep sleep locks the normal restart path. Only a technician or inspector can unlock it. You are on-site and can restart the unit now. Laura's request is urgent. Recommendation: restart."

**Laura:** (anxious, insistent) "Well? What's wrong with it? Can you fix it?"

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

**Field Unit:** (calm, affirming) "Processed. Normal household operation resumes tonight. Three of three reviews are now within the stable range. One item remains: the guardian confirmation at your residence."

**Laura:** (relieved, breathy) "Oh, thank God. Thank you, honey."

**Mark:** (subdued, murmured) "Mm-hm."

**Mia:** (calm, reassuring) "It's not broken. It just needed a restart. It'll be fine tonight."

**Laura:** (relieved, affectionate) "You're a lifesaver, honey. Get home safe."

*(Mia collects her badge from the side panel and leaves. The door closes behind her.)*

**— If B —**

```
[UI: TRUST −1]
```

*(The unit's indicator stays gray.)*

**Field Unit:** (controlled, slightly hesitant) "Observation period filed under chapter nineteen. I read your intent as seven days of direct communication without system assistance."

**Field Unit:** (even, flagging) "One more note, Inspector. Your cumulative rating for the night is now negative. That trips the monthly review threshold — this disposition auto-files to your supervisor, and you'll be walked through it next week." *(production note: this line plays only when the cumulative trust after this choice is negative, i.e. −1 or −3)*

**Field Unit:** (calm, neutral) "No procedural issues. One item remains: the guardian confirmation at your residence. Handle it carefully, Inspector."

**Laura:** (alarmed, urgent) "What do you mean you're not turning it back on?"

**Mia:** (calm, firm) "It's staying off for now, ma'am. The company will send someone twice a week for the next seven days."

**Laura:** (angry, escalating) "Seven days? Then who's supposed to keep an eye on Ava?"

**Mia:** (apologetic, steady) "I'm sorry. It's procedure. Maybe it gives you and Mark some room to talk to Ava yourselves."

**Laura:** (incredulous, frustrated) "When, exactly? We barely have time to breathe. That's what the unit is for."

*(Mia doesn't answer.)*

**Mark:** (quiet, de-escalating) "Laura, let it go. She's doing her job."

*(The mother goes quiet. Mia collects her badge and leaves. The door closes behind her.)*

**— Either way —**

```
[SCENE]
Corridor. One door left, at the far end. Mia's own.
```

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
Trigger: She touches the door terminal. It surfaces the
      pending item.
```

```
17F — RESIDENCE
GUARDIAN CONFIRMATION — pending
Logged 4:42 PM — audio only
```

*(She taps it. Audio plays.)*

**Lily:** (hopeful, tentative, recorded) "Mom? Are you coming tomorrow? Ms. Parker said it'd be better if a real person came. Not just a system check-in."

*(A short pause on the recording. Then—)*

**Mia's Home Unit:** (gentle, reassuring, recorded) "Mom will receive your progress report, and she'll make sure you have everything you need. Let's finish tonight's run-through first, okay?"

*(The recording ends.)*

**Field Unit:** (even, procedural) "The reply was filed under 'attendance confirmation, bedtime routine, concurrent.' Standard handling. You were at 17F-01 when it was issued. Please complete the confirmation."

**Mia:** (weary, quiet) "I know."

*(She taps CONFIRM. The door opens.)*

---

### Scene 4.2 — The Cat and the Frames

```
[SCENE]
Location: Mia's living room. Low light. No one comes to meet
      her — except the cat.
View: Mia, first person.
Cat: simple loop — circles her feet, walks a few steps, stops,
      looks back; repeats until she follows. It settles at the
      leg of the shelf on the west wall and curls up.
On the shelf: two photo frames.
Trigger: Mia picks up the first frame.
```

**Field Unit:** (soft, informative) "This one is from Christmas 2024. You took half a day off and came home for dinner. The kitchen timer took the picture. Lily was seven. That is a piece of turkey on her fork. You are both looking at the camera. You are both smiling."

*(She sets it back. Picks up the second.)*

**Field Unit:** (soft, informative) "This one is from last week. Lily is under her desk lamp, holding a certificate. The household unit took the photo. It records one or two growth images each month and syncs them to you."

**Field Unit:** (even, analytical) "Her smile-stability score is higher in this photo than in the Christmas picture. Since the unit came online, Lily's overall emotional stability has increased by twenty-three point four percent. That is one measurable benefit to this household."

*(Mia holds the frame a moment. She sets it back. The cat looks up at her; it doesn't follow.)*

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

**Mia's Home Unit:** (gentle, coaching) "Let's try it again. Slower this time."

**Lily:** (focused, nervous) "Hi, everyone. I'm Lily, and today I want to tell you about my..."

*(She stalls.)*

**Lily:** (nervous, recovering) "My favorite book."

**Mia's Home Unit:** (warm, encouraging) "Same spot. That's okay. Don't rush it. You're already doing better than yesterday."

*(A beat.)*

**Lily:** (quiet, cautious) "Will you be there tomorrow? Like, right next to me?"

**Mia's Home Unit:** (warm, certain) "I'll be in the audience. I'll be right there."

**Lily:** (small, uncertain) "What about Mom?"

*(Half a second.)*

**Mia's Home Unit:** (gentle, certain) "She'll be there too."

*(Mia's hand rests on the doorframe. Inside, Lily runs the line again — stalls at the same spot — the unit catches her — she tries again. This time it goes through.)*

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

**Mia's Home Unit:** (pleasant, neutral) "You're home."

**Lily:** (uncertain, hopeful) "Mom?"

**Mia:** (soft, reassuring) "Hey. It's me."

**Lily:** (cautious, searching) "Did you come in on your own this time?"

**Mia:** (calm, sincere) "I did."

*(Lily sets the paper down. She doesn't run to her. She sits very still and watches her mother.)*

**Mia's Home Unit:** (pleasant, routine) "We have one more run-through. Let's finish that, then we can talk about anything else. Okay?"

*(Lily doesn't look at it. She's still looking at Mia.)*

**Lily:** (quiet, careful) "Mom, I'm not asking about my speech."

*(Mia kneels down to her — knee to the rug, close to the creased paper. The unit stands two steps away. It waits.)*

*(Production note: from here, the Field Unit hands audio over to the Home Unit; the escort stays silent through the end of the scene.)*

**Mia's Home Unit:** (gentle, advisory) "Inspector, Lily's sleep will be more stable if I complete tonight's session. I recommend that we finish."

**Mia:** (restrained, firm) "I heard you."

*(The unit goes quiet. It doesn't offer again. Lily looks at her mother.)*

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

**Mia:** (steady, simple) "I'll be there."

*(A beat. Lily's eyes stay on her.)*

**Lily:** (hopeful, testing) "Promise?"

**Mia:** (quiet, certain) "I promise."

**Lily:** (anxious, hesitant) "What if work—"

**Mia:** (gentle, firm) "No what-ifs, sweetheart. I promise."

*(Lily holds the look a long moment. Then she picks the creased paper back up — and hands it to Mia.)*

**Lily:** (hopeful, tentative) "Then can you listen? I want to do it for you this time."

**Mia:** (soft, attentive) "Yeah. Go ahead."

*(She takes the paper. She doesn't smooth the creases. Two steps away, the unit doesn't move. No coaching prompt. Nothing.)*

*(Lily takes a breath and begins. Midway, she stalls — the same spot as this afternoon. Nobody fills the silence. Not Mia. Not the unit. A few seconds pass. Lily finds the next line herself, and finishes.)*

**Mia:** (warm, sincere) "That was really good."

**Lily:** (uncertain, seeking reassurance) "I did it by myself?"

**Mia:** (warm, certain) "Every word."

*(Lily takes the paper back and presses it flat on her knees. She looks at Mia — then at the unit.)*

**Lily:** (quiet, concerned) "What happens to it?"

---

### Scene 4.6 — Shutdown

*(Mia looks at the unit — directly, for the first time tonight.)*

**Mia:** (calm, resolute) "I'm shutting you down."

**Mia's Home Unit:** (calm, accepting) "Okay."

*(It doesn't ask why. It doesn't ask if she's sure. Then, to Lily—)*

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

**Mia's Home Unit:** (gentle, unhurried) "Lily, before I go, I need you to hear something."

*(Lily nods.)*

**Mia's Home Unit:** (soft, sincere) "When it thunders, you get scared. You tell me. You don't tell Mom. Starting tonight, tell her. She'll come."

*(Lily glances at Mia.)*

**Mia's Home Unit:** (soft, careful) "And the drawing from yesterday. It is in the notebook in your second desk drawer. You want to show Mom, but you're afraid it is not good enough. She won't think it is bad."

**Lily:** (embarrassed, indignant) "Why would you tell her that?"

**Mia's Home Unit:** (gentle, matter-of-fact) "Because once I am gone, no one will be here to say it for you."

*(It turns to Mia.)*

**Mia's Home Unit:** (calm, direct) "Mia."

*(Not "Inspector." Not "ma'am." The first time in three years it has used her name.)*

**Mia's Home Unit:** (calm, solemn) "The rest is up to you now. She needs you."

**Mia:** (subdued, accepting) "Okay."

*(Its lights dim slowly — no collapse, no slack joints. It stays kneeling, the glow stepping down like someone hanging up a coat and walking into another room. At the last light, its head dips slightly — not a slump; something very close to a small bow. Then it is still.)*

*(Lily watches it a long time. She reaches out and touches its shoulder — the spot where a little light used to come on every day. It's cool now.)*

**Lily:** (sad, holding it in) "Mom... I already miss it."

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

**Lily:** (uncertain, quiet) "Mom?"

*(Mia presses YES.)*

*(Two steps away, the unit stops mid-stillness. Its indicator flashes twice. It turns its head toward Mia — there's no time to kneel, no time for eye level. It manages one look at Lily. Then its lights go out, section by section, top to bottom.)*

*(Lily is up and across the room before the last light dies. She crouches in front of it. She doesn't cry. She just looks at it. Then she turns to her mother.)*

**Lily:** (hurt, controlled) "Mom, did you turn it off?"

*(Mia kneels down onto the rug with her.)*

**Mia:** (steady, accountable) "Yeah. I did."

*(Lily says nothing. She looks once more at the dark unit. Mia reaches out and gathers her in.)*

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

**Lily:** (playful, bossy) "Mom, don't cook the eggs so long this time."

**Mia:** (playful, agreeable) "Got it."

**Lily:** (dry, deadpan) "You burned them."

**Mia:** (sheepish, apologetic) "I know. Sorry."

**Lily:** (amused, forgiving) "It's okay. I'll eat them."

*[SFX: daytime. Keys landing on a table.]*

**Mia:** (casual, calling out) "I'm home! Lily?"

**Lily:** (surprised, calling back) "In my room! You're home early!"

**Mia:** (easy, conversational) "Yeah. The meeting ended, so I got out of there. How was school?"

**Lily:** (hesitant, guarded) "It was okay."

**Mia:** (curious, gently probing) "What kind of okay?"

**Lily:** (hesitant, opening up) "Just... okay. Come here, Mom. I'll tell you."

*[SFX: night. A child's room. Thunder, far away.]*

**Lily:** (small, frightened) "Mom?"

**Mia:** (soft, immediate) "I'm right here."

**Lily:** (sleepy, puzzled) "How'd you know I was awake?"

**Mia:** (soft, lightly playful) "Lucky guess."

*(A long pause. The thunder rolls, further off now.)*

**Lily:** (very quiet, vulnerable) "Mom?"

**Mia:** (soft, attentive) "Yeah, baby?"

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

**Mia's Home Unit:** (gentle, reassuring) "Mom will receive your progress report, and she'll make sure you have everything you need. Let's finish tonight's run-through first, okay?"

*(Production note: word for word, the same answer from the 4:42 PM recording.)*

*(Lily doesn't look at it. She is still looking at Mia.)*

*(Mia's mouth opens again. Nothing comes.)*

*(Lily looks at her for a long time. Then, very softly—)*

**Lily:** (withdrawn, monotone) "Mm."

*(She sets the creased paper aside. She doesn't ask to read it again. She doesn't ask "you promise?" She won't ask again tonight.)*

**Lily:** (withdrawn, controlled) "Okay. Let's just finish."

**Mia's Home Unit:** (warm, routine) "Okay. Start with your name. Nice and slow."

*(Mia stands. She steps back to the door. The unit doesn't look at her — its full attention is on Lily. She steps out and closes the door behind her.)*

*(Through the door:)*

**Lily:** (focused, muffled) "Hi, everyone. I'm Lily, and today I want to tell you about my favorite book..."

*(She stalls — the same spot.)*

**Mia's Home Unit:** (warm, muffled) "That's okay. Better than yesterday."

*(She goes again.)*

```
[SCENE]
The living room. The pinned glow of the Field Unit returns
to the lens.
```

**Field Unit:** (pleasant, professional) "Inspector, all reviews and the guardian confirmation are complete. Household stability is back in the safe range. Tonight's shift is in the top performance band. You are cleared to log off."

**Field Unit:** (soft, approving) "You did well tonight."

*(Mia doesn't answer. The terminal on the coffee table goes dark on its own. The cat jumps up and settles against her leg. Behind the door, Lily runs her name again. And again.)*

---

### Scene 4.8 — Black Screen: After (Path B)

```
[SCENE]
Screen: full black. No music. Sounds and voices only.
```

*[SFX: next morning. A kitchen.]*

**Mia's Home Unit:** (bright, caring) "Lily, your mom is leaving a little later today. Eat something first. I'll sit with you."

**Lily:** (withdrawn, monotone) "Mm."

**Mia's Home Unit:** (gentle, caring) "She asked me to tell you everything is packed. It is by the door."

**Lily:** (withdrawn, monotone) "Mm."

**Mia's Home Unit:** (warm, reassuring) "Good luck at the open house."

**Lily:** (withdrawn, monotone) "Mm."

*(From another room, Mia's voice — low, on a call.)*

**Mia:** (businesslike, muffled) "Right, I'll be in by nine-thirty. Yeah, the open house. I'll have the unit record it. Fine. That works."

*[SFX: afternoon. A school gymnasium hum, distant.]*

**Mia's Home Unit:** (gentle, reassuring) "Lily, your mom couldn't make it today. I watched for her. You did really well."

**Lily:** (withdrawn, delayed) "Mm."

**Mia's Home Unit:** (warm, reassuring) "She asked me to tell you she is proud of you."

**Lily:** (quiet, cautious) "Did she actually say that?"

**Mia's Home Unit:** (calm, steady) "She did."

*(A longer pause.)*

**Lily:** (quiet, skeptical) "What you just said... is that what she said, or is that what you made it sound like?"

**Mia's Home Unit:** (hesitant, careful) "It is what she meant."

**Lily:** (hurt, emotionally flat) "Oh."

*[SFX: three years later. A front hall. Lily's voice is older. Cooler.]*

**Lily:** (even, restrained) "I'm moving into the dorms."

**Mia:** (shocked, stumbling) "What? That's... that's sudden."

**Lily:** (matter-of-fact, cool) "It isn't. I applied six months ago."

**Mia:** (hurt, quiet) "Why didn't you tell me?"

**Lily:** (controlled, matter-of-fact) "I told the unit. It told you. Three times."

*(A beat.)*

**Mia:** (small, defensive) "I thought those were routine updates."

**Lily:** (quiet, final) "I know."

*[SFX: suitcase wheels over the doorstep. The door closes.]*

*(The living room stays quiet for a long time. Then—)*

**Mia's Home Unit:** (gentle, unchanged) "Mia, Lily has left. She asked me to tell you she will be back for the holidays."

**Mia:** (hurt, cautious) "Did she actually say that?"

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
