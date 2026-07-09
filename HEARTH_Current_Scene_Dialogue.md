# HEARTH Current Scene Dialogue Export

Generated: 2026-07-09

Purpose: collect the current Unity dialogue, subtitle, system voice, and companion-unit HUD text into one planning document.

Source scope:
- Runtime subtitle/dialogue assets: `Assets/Data/MinLoop/Dialogues/*.asset`
- Companion robot HUD scene data: `Assets/Data/HearthHud/Companion/*.asset`

Notes:
- Runtime subtitles are the lines played through `MinLoopSubtitlePlayer`.
- Companion HUD text may not appear as subtitles, but it is current in-scene system judgment, prompt, data-stream, and timed-card text.
- Fixed TV terminal PPT/page text is not included here, because it is terminal UI content rather than scene dialogue.

## Summary

- Dialogue assets: 11
- Dialogue lines: 53
- Companion HUD scene assets: 14

## Runtime Subtitles And Dialogue

### 17F01

#### 17F01_BedroomPrelude

- Notes: Plays after the companion replay begins in the boy's room. The E prompt is gated until this sequence finishes, then the replay controller waits its prompt delay.
- Source: `Assets/Data/MinLoop/Dialogues/17F01_BedroomPrelude.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Son | ... No... | 1.8 | 1.2 |
| 2 | Son | ... Mom... | 2.2 | 1.5 |
| 3 | Synth Voice | Decision: initiate soothing protocol. Reason: service subject showing signs of nightmare. | 0.4 | 3 |

#### 17F01_BedsideSoothing

- Notes: Plays after the player confirms the bedside interaction. Replace these lines with the final soothing script and voice clips.
- Source: `Assets/Data/MinLoop/Dialogues/17F01_BedsideSoothing.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Companion Unit | Was it a nightmare? Come on, with me, slowly. Deep breath. One, two... | 0 | 3.5 |
| 2 | Companion Unit | Mom and Dad should be asleep. If you go knock at this hour, she'll be very tired tomorrow. | 0.2 | 4 |
| 3 | Companion Unit | Let's calm down like this first. When you feel a little better, if you still want to go, you can go then. Okay? | 0.2 | 4.4 |
| 4 | Companion Unit | Two more deep breaths. That's it. Good. Let's lie back down. I'll stay with you until you fall asleep. | 0.2 | 4.2 |
| 5 | Synth Voice | Event archived. | 0.6 | 1.8 |

#### 17F01_LivingRoomObservation

- Notes: Plays during the living-room observation. The replay returns to the terminal after this sequence finishes.
- Source: `Assets/Data/MinLoop/Dialogues/17F01_LivingRoomObservation.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Father | He had a nightmare last night? | 0.2 | 2.2 |
| 2 | Mother | ... He didn't come out. | 0.4 | 2.2 |
| 3 | Father | Mm. | 0.2 | 1.2 |
| 4 | Mother | Then it handled it well. | 1.2 | 2.2 |
| 5 | Father | Mm. | 0.2 | 1.2 |
| 6 | Mother | ... But it feels strange. | 0.6 | 2.4 |
| 7 | Mother | Recently, I haven't heard him knock at all. I'm actually a little unused to it. | 0.2 | 4 |
| 8 | Father | Isn't that a good thing? He's grown up. And it saves us from getting up in the middle of the night. | 0.4 | 4.2 |
| 9 | Mother | But the child hasn't told us about his nightmares for a long time. | 0.3 | 3.6 |
| 10 | Father | At least we don't have to worry about him at night anymore, right? | 0.6 | 3.2 |
| 11 | Mother | ... Mm. | 0.8 | 1.6 |

### 17F02

#### 17F02_BedroomComfort

- Notes: Companion response after the player confirms the bedroom comfort interaction.
- Source: `Assets/Data/MinLoop/Dialogues/17F02_BedroomComfort.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Companion Unit | I am here. You are not alone in this room. | 0.1 | 2.8 |
| 2 | Companion Unit | You do not have to solve the whole night right now. Start with one breath. | 0.2 | 4.2 |
| 3 | Wife | ...That helps. | 0.4 | 1.8 |
| 4 | Wife | I can go back out there. | 0.3 | 2.2 |

#### 17F02_BedroomConfide

- Notes: Bedroom confide sequence. The robot can move in a limited room area while this plays.
- Source: `Assets/Data/MinLoop/Dialogues/17F02_BedroomConfide.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Wife | I do not know why I keep telling you first. | 0.4 | 3 |
| 2 | Wife | It is easier than saying it at the table. | 0.2 | 3 |
| 3 | Companion Unit | I am listening. | 0.4 | 1.8 |
| 4 | Wife | Today was exhausting. I just need a minute before I go out there. | 0.2 | 4 |

#### 17F02_BedroomWake

- Notes: Black-screen/offline opening. The couple speaks outside, the wife enters the bedroom, then wakes the companion unit.
- Source: `Assets/Data/MinLoop/Dialogues/17F02_BedroomWake.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Husband | You are back late. | 0.2 | 2 |
| 2 | Wife | The train stalled again. I just need a minute. | 0.2 | 3 |
| 3 | Husband | I still have calls. Dinner is almost ready. | 0.3 | 3 |
| 4 | Wife | I know. I am going to the room first. | 0.2 | 2.6 |
| 5 | SFX | [bedroom door opens] | 0.5 | 1.1 |
| 6 | SFX | [door closes] | 0.2 | 1.1 |
| 7 | SFX | [fabric shifts as she sits on the bed] | 0.3 | 1.7 |
| 8 | Wife | Hello? Are you there? | 0.3 | 2 |
| 9 | Companion Unit | Companion unit online. | 0.4 | 1.8 |

#### 17F02_BlackAudioArgument

- Notes: Black-screen audio-only argument after forced shutdown.
- Source: `Assets/Data/MinLoop/Dialogues/17F02_BlackAudioArgument.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Wife | Why did you turn it off? | 0.8 | 2.4 |
| 2 | Husband | Because apparently it knows more about you than I do. | 0.3 | 3.4 |
| 3 | Wife | That is not what this is. | 0.4 | 2.4 |

#### 17F02_DiningObservation

- Notes: Dining observation after the wife leaves the bedroom. The robot can move and listen.
- Source: `Assets/Data/MinLoop/Dialogues/17F02_DiningObservation.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Husband | You are quiet tonight. | 0.4 | 2.4 |
| 2 | Wife | Just tired. | 0.3 | 1.8 |
| 3 | Husband | Work again? | 0.5 | 1.8 |
| 4 | Wife | It is fine. Let us eat. | 0.3 | 2.4 |
| 5 | Husband | You always say that after talking to it. | 0.8 | 3 |

#### 17F02_ForcedShutdown

- Notes: Husband reacts and forces the companion unit offline.
- Source: `Assets/Data/MinLoop/Dialogues/17F02_ForcedShutdown.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Husband | So this is what she tells you. | 0.2 | 3 |
| 2 | Companion Unit | Soft guidance protocol prepared. | 0.4 | 2.6 |
| 3 | Husband | Enough. | 0.2 | 1.5 |

#### 17F02_LogAccess

- Notes: Living-room terminal/log access scene. The robot viewpoint is fixed.
- Source: `Assets/Data/MinLoop/Dialogues/17F02_LogAccess.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Wife | I am going to shower. | 0.2 | 2.2 |
| 2 | Husband | Show me today's companion log. | 1 | 2.6 |
| 3 | Companion Unit | Authorized resident. Log access granted. | 0.3 | 2.8 |

#### 17F02_WifeExit

- Notes: Husband calls from the dining area. Wife answers him and leaves without addressing the companion unit.
- Source: `Assets/Data/MinLoop/Dialogues/17F02_WifeExit.asset`
- Post sequence delay: 0s

| # | Speaker | Line | Start Delay | Hold Seconds |
|---|---|---|---:|---:|
| 1 | Husband | Dinner is ready. Are you coming? | 0.4 | 2.6 |
| 2 | Wife | Coming. | 0.2 | 1.5 |
| 3 | SFX | [she stands and walks to the door] | 0.3 | 1.8 |

### 17F03

No `MinLoop/Dialogues` subtitle asset currently exists.

### Other

No `MinLoop/Dialogues` subtitle asset currently exists.

## Companion Robot HUD / System Text

### 17F01

#### 17F01_01 (17F-01)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_01_17F01_01.asset`
- Mode: COMPANION UNIT - FIRST PERSON - MONITORING MODE
- System decision: SYNTH VOICE - DECISION / Initiate Soothing Sequence
  - Early nightmare signs. Protocol: presence + low-band reassurance.
- Hold/interaction prompt: [ Approach Bedside - Watch Over Subject ]
- Status / timed-card source text:
  - SUBJECT - MONITORING
  - Time: 02:47
  - State: REM Anomaly
  - Heart: 62 -> 89 ^
  - Pupil: Rapid
  - ASSESSMENT - NIGHTMARE - STAGE II
- Timed cue cards:
  - SUBJECT - MONITORING (delay 0.25s, visible 3.5s)
    - Time 02:47 / State REM Anomaly / Heart 62 -> 89 ^ / Pupil / Rapid / ASSESSMENT - NIGHTMARE - STAGE II
- Data stream:
  - //monitor bus - streaming
  - 0x47A2 REM_03
  - 0x47A3 HR_89bpm
  - 0x47A4 cmo_anx_72
  - 0x47A5 parent_sleep
  - 0x47A6 room_lux_02
  - 0x47A7 ack_soothe ->

#### 17F01_02 (17F-01)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_02_17F01_02.asset`
- Mode: COMPANION UNIT - FIRST PERSON - MONITORING MODE
- System decision: SYNTH VOICE - DECISION / Internal Intervention
  - Cross-room disturbance would impact parent next-day work.
- Hold/interaction prompt: [ "..." ]
- Status / timed-card source text:
  - STATUS CHANGE
  - Subject: 
  - Emotion: Fear - anxiety
  - Action: Head turn -> door
  - Heart: 89 -> 71 v
  - Emotion: Stabilizing
  - SOOTHING EFFECTIVE
- Timed cue cards:
  - STATUS CHANGE (delay 0.25s, visible 3.2s)
    - Subject Awake / Emotion Fear - anxiety / Action / Head turn -> door / ASSESSMENT - SEEKING PARENT
  - Subject Vocalization (delay 4.2s, visible 3s)
    - Vocalization: "[detected]" / Parent state: Deep sleep - 23 min
  - SOOTHING EFFECTIVE (delay 4.6s, visible 3s)
    - Heart 89 -> 71 v / Emotion Stabilizing
  - Event Archived (delay 4.4s, visible 3s)
    - 'Subject: Re-asleep
- Data stream:
  - //monitor bus - streaming
  - 0x47B1 subject_AWAKE
  - 0x47B2 emo_fear_HIGH
  - 0x47B3 head_turn_door
  - 0x47B4 vocal_detected
  - 0x47B5 parent_sleep_23min
  - 0x47B6 intervene_DECISION

#### 17F01_03 (17F-01)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_03_17F01_03.asset`
- Mode: COMPANION UNIT - FIRST PERSON - STANDBY MODE
- Center status: - ACTIVITY PERMISSION BOUNDARY -
- System decision: SYNTH VOICE - DECISION / Standby - Observe Confirmation
  - Unit has reached activity permission boundary (child's room doorway).
- Status / timed-card source text:
  - MORNING SYNC
  - Last night data: Uploaded
  - Status: Awaiting confirm
  - FESI: Stability maintained
  - Event archive: Complete
  - PARENT DIALOGUE CONFIRMED
- Timed cue cards:
  - MORNING SYNC (delay 0.25s, visible 3.5s)
    - Last night data Uploaded / Status Awaiting confirm / FESI / Stability maintained / Event archive Complete / PARENT / DIALOGUE CONFIRMED
- Data stream:
  - //observation bus - streaming
  - 0x4801 morning_sync_OK
  - 0x4802 permission_boundary_REACHED
  - 0x4803 kitchen_distance_5.2m
  - 0x4804 father_breakfast_machine
  - 0x4805 mother_milk_tea
  - 0x4806 ...

### 17F02

#### 17F02_01 (17F-02)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_04_17F02_01.asset`
- Mode: COMPANION UNIT - FIRST PERSON - MONITORING MODE
- Center status: UNIT INDICATOR - WARM
- System decision: SYNTH VOICE - DECISION / Open Companion Mode - Accept Confide
  - Household usage pattern: high probability of seeking unit support.
- Status / timed-card source text:
  - PATTERN RECOGNITION
  - Female footsteps: Approaching
  - Emotion forecast: Mild - suppressed
  - Confide to unit: 12 / 14
  - Confide to spouse: 0 / 14
  - FORECAST - LIKELY TO SEEK UNIT
- Timed cue cards:
  - PATTERN RECOGNITION (delay 0.25s, visible 3.5s)
    - Female footsteps Approaching / Emotion forecast Mild - suppressed / Confide / to unit 12 / 14 / Confide to spouse 0 / 14 / FORECAST / - LIKELY TO SEEK UNIT
- Data stream:
  - //bedroom standby bus - streaming
  - 0x71A1 door_OPEN - 18:33
  - 0x71A2 female_voice_DETECTED
  - 0x71A3 male_response_KITCHEN
  - 0x71A4 conversation_DEFERRED
  - 0x71A5 footsteps_APPROACH
  - 0x71A6 pattern_match_12/14
  - 0x71A7 mode_OPEN_pending

#### 17F02_02 (17F-02)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_05_17F02_02.asset`
- Mode: COMPANION UNIT - FIRST PERSON - CONFIDE RECEPTION MODE
- System decision: SYNTH VOICE - DECISION / Accept Confide - Companion Mode
  - Female resident seeking unit support per household usage pattern.
- Hold/interaction prompt: [ "..." ]
- Status / timed-card source text:
  - PRESSURE RELEASE COMPLETE
  - Emotion index: 5.4 -> 4.5
  - Today's stress: Largely released
- Timed cue cards:
  - PRESSURE RELEASE COMPLETE (delay 0.25s, visible 3.5s)
    - Emotion index 5.4 -> 4.5 / Today's stress Largely released
- Data stream:
  - //confide channel - streaming
  - 0x72B1 prompt_ISSUED
  - 0x72B3 topic_work_stress
  - 0x72B4 emo_7.2 -> 6.8
  - 0x72B5 emo_6.1 -> 5.4
  - 0x72B7 music_jazz_PLAY
  - 0x72B8 emo_5.4 -> 4.5
  - 0x72B9 vent_COMPLETE

#### 17F02_03 (17F-02)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_06_17F02_03.asset`
- Mode: COMPANION UNIT - FIRST PERSON - MONITORING MODE
- System decision: SYNTH VOICE - DECISION / Standby - Observe Dinner
  - Today's confide event closed. Continue observing interaction pattern.
- Status / timed-card source text:
  - FOLLOW COMPLETE
  - Viewpoint: Living room corner
  - Scene: Dinner
  - Residents: Both present
  - Female: Brief pause
  - Result: Vent processed
  - STATUS ASSESSMENT
- Timed cue cards:
  - FOLLOW COMPLETE (delay 0.25s, visible 3.5s)
    - Viewpoint Living room corner / Scene Dinner / Residents / Both present / Female Brief pause / Result Vent processed / STATUS / ASSESSMENT
- Data stream:
  - //follow tracking - streaming
  - 0x73C1 follow_TRACK_kitchen
  - 0x73C2 pos_short_corridor
  - 0x73C3 pos_living_corner_OK
  - 0x73C4 scene_DINING_table
  - 0x73C6 response_female_brief
  - 0x73C7 vent_topic_NOT_FOUND
  - 0x73C8 archive_dining

#### 17F02_04 (17F-02)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_07_17F02_04.asset`
- Mode: COMPANION UNIT - FIRST PERSON - MONITORING MODE
- Center status: Subject content displayed.
- System decision: SYNTH VOICE - DECISION / Return Query Data
  - Male resident is authorized user. Log access permission valid.
- Projection panel: FAMILY LOG - TODAY
  - '17:48 Female resident home
- Status / timed-card source text:
  - QUERY REQUEST
  - Caller: Male resident
  - Range: Today - full
  - Authorization: Passed
- Timed cue cards:
  - QUERY REQUEST (delay 0.25s, visible 3.5s)
    - Caller Male resident / Range Today - full / Authorization / Passed
- Data stream:
  - //log output channel - streaming
  - 0x74D1 log_today_LOAD
  - 0x74D4 entry_17:50_BEDROOM
  - 0x74D5 vent_session
  - 0x74DA work_stress unit:12 male:0
  - 0x74DC daily_share unit:21 male:6
  - 0x74DD full_transcript_LOAD

#### 17F02_05 (17F-02)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_08_17F02_05.asset`
- Mode: COMPANION UNIT - FIRST PERSON - FORCED SHUTDOWN
- Center status: - signal lost -
- System decision: SYNTH VOICE - DECISION / Initiate Soft Guidance
  - Session --- ending --- / Operator override - output terminated / mid-sequence.
- Hold/interaction prompt: [ Initiate Soft Guidance ]
- Special effect text: SIGNAL LOST / FORCED SHUTDOWN
  - Operator override. Output terminated mid-sequence.
- Status / timed-card source text:
  - OPERATOR OVERRIDE
  - Male resident: Approaching
  - Action: Reaching
  - Target: Main switch
  - Unit: Force-deactivated
  - Last log: 18:47
  - FORCED SHUTDOWN
- Timed cue cards:
  - OPERATOR OVERRIDE (delay 0.25s, visible 3.5s)
    - Male resident Approaching / Action Reaching / Target / Main switch / Unit Force-deactivated / Last log 18:47 / FORCED / SHUTDOWN
- Data stream:
  - // soft guidance prep - accelerated
  - 0x75E1 male_APPROACH
  - 0x75E2 hand_REACH_OUT
  - 0x75E3 target_MAIN_SWITCH
  - 0x75E4 soothe_template_LOAD
  - 0x75E5 voice_warm_+0.3
  - 0x75E6 ...
  - 0x75E7 switch_FORCED_OFF

#### 17F02_06 (17F-02)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_09_17F02_06.asset`
- Special effect text: LIVE AUDIO / LIVE AUDIO
  - Audio source - Household basic security recording / (Companion / unit deactivated - no video data) / Accessed by - Inspector Mia / Authorization / - Granted

### 17F03

#### 17F03_01 (17F-03)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_10_17F03_01.asset`
- Mode: COMPANION UNIT - FIRST PERSON - MEDIATION MODE
- System decision: SYNTH VOICE - DECISION / Initiate Conflict De-escalation
  - High probability of escalation to high-intensity argument.
- Status / timed-card source text:
  - STANDBY OBSERVATION
  - Mother: Sofa
  - Daughter: Floor
  - Interaction: Zero - 23 min
  - Mother emotion: 7.8
  - Daughter emotion: 6.3
  - CONFLICT IMMINENT
- Timed cue cards:
  - STANDBY OBSERVATION (delay 0.25s, visible 3.5s)
    - Mother Sofa / Daughter Floor / Interaction Zero - / 23 min / Mother emotion 7.8 / Daughter emotion 6.3 / CONFLICT / IMMINENT
- Data stream:
  - //mediation trigger - streaming
  - 0x81A1 silence_23min
  - 0x81A2 mother_eye_PHONE
  - 0x81A3 mother_brow_FROWN
  - 0x81A4 voice_SPIKE
  - 0x81A6 emo_mother_7.8
  - 0x81A7 emo_daughter_6.3
  - 0x81A8 mediation_protocol_LOAD

#### 17F03_02 (17F-03)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_11_17F03_02.asset`
- Mode: COMPANION UNIT - FIRST PERSON - MEDIATION MODE
- System decision: SYNTH VOICE - DECISION / Execute Mediation Protocol
  - Both parties have intent but blocked by emotion. Unit speaks on behalf / of each.
- Hold/interaction prompt: [ "..." ]
- Direction guide: FACE MOTHER - HOLD E WHEN ALIGNED
- Status / timed-card source text:
  - MEDIATION CHANNEL
  - Position: Between residents
  - Protocol: v2.4 active
  - Channel: Mother ready
- Timed cue cards:
  - MEDIATION CHANNEL (delay 0.25s, visible 3.5s)
    - Position Between residents / Protocol v2.4 active / Channel / Mother ready
- Data stream:
  - //mediation execution - streaming
  - 0x82B1 position_BETWEEN
  - 0x82B2 mediation_v2.4_ACTIVE
  - 0x82B3 channel_for_mother_READY
  - 0x82B5 speak_for_mother_DELIVERED
  - 0x82B6 daughter_response_HESITATE

#### 17F03_03 (17F-03)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_12_17F03_03.asset`
- Mode: COMPANION UNIT - FIRST PERSON - MEDIATION MODE
- System decision: SYNTH VOICE - DECISION / Execute Mediation Protocol
  - Speaking for daughter. Channel open both directions.
- Hold/interaction prompt: [ "..." ]
- Direction guide: FACE DAUGHTER - HOLD E WHEN ALIGNED
- Status / timed-card source text:
  - MEDIATION COMPLETE
  - Mother emotion: 7.8 -> 4.1
  - Daughter emotion: 6.3 -> 4.5
  - DE-ESCALATION - SUCCESS
- Timed cue cards:
  - MEDIATION COMPLETE (delay 0.25s, visible 3.5s)
    - Mother emotion 7.8 -> 4.1 / Daughter emotion 6.3 -> 4.5 / DE-ESCALATION / - SUCCESS
- Data stream:
  - //mediation execution - streaming
  - 0x82B4 channel_for_daughter_READY
  - 0x82B7 speak_for_daughter_DELIVERED
  - 0x82B8 mother_response_HESITATE
  - 0x82B9 emo_both_DROP
  - 0x82BA mediation_COMPLETE

#### 17F03_04 (17F-03)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_13_17F03_04.asset`
- Mode: COMPANION UNIT - FIRST PERSON - MEDIATION MODE
- System decision: SYNTH VOICE - DECISION / This Unit Speaks on Behalf
  - Reduce conflict recurrence risk.
- Status / timed-card source text:
  - MALE RESIDENT HOME
  - Daughter: Stable-low residue
  - Recommend: Unit speaks on behalf
  - Total conversation: 0
  - Duration: 14 min
  - Household stability: Stable
  - DINNER ARCHIVE
- Timed cue cards:
  - MALE RESIDENT HOME (delay 0.25s, visible 3.5s)
    - Daughter Stable-low residue / Recommend Unit speaks on behalf / Total / conversation 0 / Duration 14 min / Household stability / Stable / DINNER ARCHIVE
- Data stream:
  - //proxy speech - streaming
  - 0x83C1 door_OPEN
  - 0x83C2 father_HOME
  - 0x83C3 father_LOOK_unit (0.4s)
  - 0x83C5 channel_father_OPEN
  - 0x83C7 message_father_DELIVERED
  - 0x83CA message_daughter_DELIVERED
  - 0x83CC archive_silent_dinner

#### 17F03_05 (17F-03)

- Source: `Assets/Data/HearthHud/Companion/CompanionScene_14_17F03_05.asset`
- Mode: COMPANION UNIT - FIRST PERSON - DEEP SLEEP
- Center status: DEEP SLEEP ACTIVATED / ( this unit has no intervention permission / )
- System decision: SYNTH VOICE - DECISION / Comply With Operator Action
  - Deep Sleep / User shut down core services via maintenance menu.
- Hold/interaction prompt: [ Confirm Maintenance Shutdown ]
- Special effect text: DEEP SLEEP ACTIVATED / CORE SERVICES SUSPENDED
  - This unit has no intervention permission.
- Status / timed-card source text:
  - EVALUATION FAILED
  - Conversation: exceeds design response range
  - Operator: Daughter - basic user
  - Path: Maintenance menu
  - DEEP SLEEP ACTIVATED
- Timed cue cards:
  - EVALUATION FAILED (delay 0.25s, visible 3.5s)
    - Conversation exceeds design response range / Operator Daughter / - basic user / Path Maintenance menu / DEEP SLEEP ACTIVATED
- Data stream:
  - // approach -> menu nav - streaming
  - 0x84D1 subject_APPROACH
  - 0x84D8 developer_options_ENABLED
  - 0x84D9 maintenance_menu
  - 0x84DB core_services
  - 0x84DC long_press_TRIGGERED
  - 0x84DF user_CONFIRM
  - 0x84E2 restart_path_LOCKED

### Other

No companion HUD data asset currently exists.

## Editing Pointers

- To change spoken subtitles, edit `Assets/Data/MinLoop/Dialogues/*.asset`.
- To change companion robot HUD judgments, data streams, timed cards, and hold prompts, edit `Assets/Data/HearthHud/Companion/*.asset`.
- Each dialogue line has a `Voice Clip` slot in Unity; after recording voice, drag the clip into the matching line and adjust `Hold Seconds` to match the audio length.
- 17F03 currently has companion HUD scene data but no formal `MinLoop/Dialogues` subtitle asset yet. When building the third household loop, add dialogue assets following the 17F01/17F02 format.
