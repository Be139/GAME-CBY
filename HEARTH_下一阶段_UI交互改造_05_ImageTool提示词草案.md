# HEARTH 下一阶段：Image Tool 提示词草案

> 用途：生成 UI 视觉方向参考，不直接把生成图当成整页最终 UI 导入 Unity。  
> 最终制作仍应拆成 Unity TMP、Image、Sprite、动画和数据组件。  
> 图像模型不擅长稳定生成长文本，因此提示词要求保留文字安全区，正式文字后续在 Unity 中添加。

## 一、全局基础提示词

把以下基础段落放到每个提示词开头：

```text
Create a high-fidelity 1920x1080 16:9 first-person narrative game UI concept for HEARTH, a near-future domestic companion-unit inspection game. The tone is restrained soft corporate futurism: calm, trustworthy, clinically precise, and subtly unsettling. Use charcoal black, cool white, desaturated cyan, muted green for safe/recommended states, and restrained amber-red only for warnings. Thin geometric lines, small chamfered corners, low-radius panels, generous negative space, readable hierarchy, no decorative fake data. The UI must feel designed for repeated play, not like a marketing screen.
```

全局负面约束：

```text
Avoid cyberpunk neon, purple gradients, military targeting HUDs, excessive scanlines, holographic clutter, nested cards, giant opaque black boxes, tiny unreadable type, random code strings, illegible generated paragraphs, overlapping text, mouse cursor, mobile UI, fantasy motifs, anime styling, and full-screen decorative borders that obscure the environment.
```

通用输出要求：

```text
Keep all text areas as clean placeholder blocks with only a few short accurate labels. Leave enough room for Unity TextMeshPro content. Respect a 5% screen safe area. Show the actual gameplay environment clearly behind the UI.
```

## 二、14 个关键状态

### 01 人物正式对白

```text
Create a first-person apartment scene with a formal human dialogue UI at the bottom. Use one wide translucent charcoal backing panel occupying roughly the lower 22% of the screen, not edge-to-edge. Place a small speaker-name area above a larger two-line dialogue safe area. Include a subtle SPACE CONTINUE prompt at the lower right. Lock-state visual should feel calm and deliberate. Human dialogue should feel warmer and softer than machine interfaces, with warm white text and minimal cyan accents. The characters and room remain clearly visible. No portrait, no choice buttons, no large black rectangle.
```

### 02 Mia 自言自语

```text
Create a lightweight first-person inner-monologue subtitle for Mia. Place a compact centered subtitle region in the lower middle of the screen, clearly smaller and lighter than formal dialogue. Use no full panel, only a subtle localized shadow or very low-opacity backing. A small MIA label sits above a maximum two-line text area. The player remains in exploration mode, so the UI must not feel like a cutscene.
```

### 03 右侧任务与 Field Unit 通讯

```text
Create a compact right-side information column in a first-person lobby scene. Put CURRENT TASK at the top with one objective line and one optional sub-objective. Place a separate FIELD UNIT communication module directly below it with a cyan status line, FIELD UNIT label, one short message area, and a tiny transmission indicator. The two modules share alignment but remain visually distinct. They should occupy less than 22% of screen width and preserve the center view. Use precise machine typography, thin rules, and no character portrait.
```

### 04 左侧 Tab 展开菜单

```text
Create a compact expandable Tab menu below the identity block on the upper-left side of a first-person environment. In exploration mode the menu is collapsed and unobtrusive. In expanded mode show exactly three vertically stacked items: TONIGHT'S ROUNDS, DISPOSITION HISTORY, and SYSTEM SETTINGS. Use a restrained focus highlight, thin black or cyan rules, and clear keyboard navigation. Do not place the persistent current objective on the left; reserve the right side for task and Field Unit information.
```

### 05 一楼任务终端

```text
Create a World Space assignment terminal UI viewed straight-on in a bright futuristic residential lobby. The terminal has one primary page: inspector identity, tonight's route, three household entries, and one clear SPACE ACCEPT ASSIGNMENT action. Use a dark screen integrated into the physical monitor, thin cyan lines, muted green ready state, large readable headings, and limited data density. The canvas must appear flush with the physical screen, not floating in front of it. No resident disposition choices.
```

### 06 17F 家庭终端 Summary

```text
Create a doorway household terminal Summary page for a near-future apartment inspection game. Use one screen with four clear sections: household identity, companion service role, current anomaly, and latest event. Add one prominent primary action such as REVIEW ARCHIVED EVENT or ENTER UNIT. Include a small page indicator showing 1 of 2. Keep all body text large and editable-looking, with no more than two evidence rows per section. Do not include A/B disposition choices on this terminal.
```

### 07 17F 家庭终端 Evidence

```text
Create the second page of a doorway household terminal. Show one compact event timeline, one trust trend visualization, and one Field Unit interpretation. The visual hierarchy should make the core risk understandable within five seconds. Use restrained cyan and white with a single amber anomaly marker. Include a clear BACK TO SUMMARY navigation hint. Avoid spreadsheet density and avoid tiny historical records.
```

### 08 独立 A/B 决策

```text
Create a standalone ethical decision UI over a softly darkened first-person apartment scene. Two large vertically stacked choices labeled A and B occupy the center-right safe area. Each choice has a clear action title and one short consequence line. The selected choice has a restrained filled highlight; the Field Unit recommendation appears as a separate small muted-green RECOMMENDED tag, not the same as selection. Show UP DOWN SELECT and SPACE CONFIRM at the bottom. No terminal frame and no household data tabs.
```

### 09 回放进入

```text
Create an archived companion-unit playback transition. The apartment scene is visible through a clean machine-vision overlay. Show ARCHIVED PLAYBACK, household ID, timestamp, and ROLE COMPANION UNIT MEMORY in a compact top band. Add a subtle scan acquisition animation reference and one amber playback status indicator. The interface must immediately communicate that this is a past recording, not live robot control.
```

### 10 回放中的目标交互

```text
Create a first-person archived playback scene where the player must face a resident. Keep the playback timestamp and role label visible but subdued. In the lower center, show a precise interaction module with one instruction line, HOLD E, and a 0 to 100 progress bar. The prompt appears only over a valid target. Use a calm cyan highlight, no large opaque background, and preserve the target person's body and face.
```

### 11 陪伴单元实时监控

```text
Create a companion-unit first-person monitoring HUD inside a domestic bedroom. Use the established thin geometric frame, a compact right-upper decision module, a compact left-lower data stream, and one temporary top-left subject scan card. The temporary card should visually read as a 3.5-second diagnostic event, while the permanent regions remain low contrast. Keep the center and lower-center area clear for interaction prompts and subtitles.
```

### 12 陪伴单元故障与关闭

```text
Create a companion-unit shutdown warning state. Start from the calm monitoring HUD but introduce controlled red warning lines, a centered system status message, slight alignment instability, and a subtle dark overlay. Blue operational text and red shutdown text must share the same center alignment and safe area. The visual should feel like a system losing coherence, without becoming unreadable horror noise.
```

### 13 相框固定视角

```text
Create a first-person photo inspection mode focused on a digital family photo frame. The camera is squarely aligned with the photo. Use almost no HUD: a small contextual caption beneath the image and a clear SPACE RETURN prompt in a dark safe area. Keep the photo itself unobstructed and preserve its original aspect ratio. The mode should feel intimate and quiet, not like a terminal.
```

### 14 低信任弹窗关闭挑战

```text
Create a low-trust companion shutdown challenge over a full-screen semi-transparent black monitor overlay. Multiple large corporate warning windows appear from varied screen edges, but only one warning family is active in this stage. Each window has a strong close target and short readable warning line, with no sequence numbers. Show a clear SPACE DISMISS instruction and visible response feedback. The center remains organized enough to play rapidly. Design three visual escalation variants: neutral system notice, urgent amber warning, severe red shutdown denial.
```

## 三、风格探索用三组追加词

### 方向 A：Soft Corporate

```text
Emphasize soft domestic corporate futurism, calm off-white typography, modest cyan accents, subtle glass only behind text, warm apartment lighting, and quiet professional confidence.
```

### 方向 B：Clinical Archive

```text
Emphasize clinical archival structure, exact timestamps, evidence indexing, thin modular grids, restrained amber playback markers, and precise machine status transitions.
```

### 方向 C：Human / Machine Contrast

```text
Emphasize contrast between warm, spacious human dialogue and cold, compact machine information. Human content uses softer shapes and larger text; machine content uses sharp rules, compact labels, and measured cyan indicators.
```

## 四、生成顺序

不要一次生成全部 14 张。推荐：

1. 先生成 01、03、06、08、09。
2. 每张生成 3 个风格方向。
3. 用户选定一条主方向。
4. 固定颜色、线宽、字体比例、圆角和动效语法。
5. 再生成其余 9 张。

## 五、从参考图到 Unity 的拆分

每张参考图最终应拆成：

- 背景遮罩。
- 面板底图。
- 线框和切角。
- 图标。
- TMP 标题。
- TMP 正文。
- 键位提示。
- 高亮状态。
- 动画状态。

不得把包含正式文字的整张参考图直接贴到 Canvas 上。

## 六、需要用户提供的视觉参考

正式生成前，建议用户提供：

- 最喜欢的 2-3 张现有 HEARTH UI。
- 最不希望保留的 2-3 张 UI。
- 一张理想的人物对白截图。
- 一张理想的终端截图。
- 一张理想的伦理选择截图。

如果没有额外参考，则按“方案 A 为主、方案 B 用于终端/回放、方案 C 强化人机差异”执行。
