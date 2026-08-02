from __future__ import annotations

import html
import json
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parent.parent
SOURCE = ROOT / "HEARTH_音频资源需求与素材来源清单.md"
OUTPUT = ROOT / "HEARTH_SFX_Link_Navigator.html"


def clean_markdown(value: str) -> str:
    value = re.sub(r"`([^`]*)`", r"\1", value)
    value = re.sub(r"<br\s*/?>", " ", value, flags=re.IGNORECASE)
    value = re.sub(r"\[([^\]]+)\]\([^)]+\)", r"\1", value)
    return html.unescape(value).strip()


def read_items() -> list[dict[str, object]]:
    current_section = ""
    items: list[dict[str, object]] = []
    link_pattern = re.compile(r"\[([^\]]+)\]\((https?://[^)]+)\)")

    for line in SOURCE.read_text(encoding="utf-8").splitlines():
        if line.startswith("## "):
            current_section = re.sub(r"^##\s+\d+\.\s*", "", line).strip()
            continue

        if not re.match(r"^\|\s*`[A-Z]+-\d+`", line):
            continue

        cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
        if len(cells) != 4:
            raise ValueError(f"Unexpected table row: {line}")

        id_match = re.match(r"`([^`]+)`\s*(.*)", cells[0])
        if not id_match:
            raise ValueError(f"Missing material ID: {line}")

        links = [
            {"label": clean_markdown(label), "url": url}
            for label, url in link_pattern.findall(cells[3])
        ]
        if len(links) != 5:
            raise ValueError(f"{id_match.group(1)} has {len(links)} links, expected 5")

        items.append(
            {
                "id": id_match.group(1),
                "title": clean_markdown(id_match.group(2)),
                "category": current_section,
                "placement": clean_markdown(cells[1]),
                "target": clean_markdown(cells[2]),
                "links": links,
            }
        )

    if not items:
        raise ValueError("No sound-effect rows were found in the source document")
    return items


def build_html(items: list[dict[str, object]]) -> str:
    data = json.dumps(items, ensure_ascii=False).replace("</", "<\\/")
    categories = list(dict.fromkeys(str(item["category"]) for item in items))
    category_data = json.dumps(categories, ensure_ascii=False).replace("</", "<\\/")

    template = r'''<!doctype html>
<html lang="zh-CN">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>HEARTH 音效链接导航</title>
  <style>
    :root {
      color-scheme: dark;
      --bg: #0e1116;
      --panel: #171c24;
      --panel-2: #1e2530;
      --line: #303948;
      --text: #eef3f8;
      --muted: #9da9b7;
      --accent: #7ed7e6;
      --accent-2: #b7f0b1;
      --warning: #f5cf78;
    }
    * { box-sizing: border-box; }
    body {
      margin: 0;
      background: radial-gradient(circle at top left, #17222d 0, var(--bg) 38rem);
      color: var(--text);
      font-family: "Segoe UI", "Microsoft YaHei", system-ui, sans-serif;
      line-height: 1.55;
    }
    button, input { font: inherit; }
    .shell { width: min(1440px, calc(100% - 32px)); margin: 0 auto; }
    header { padding: 44px 0 24px; }
    .eyebrow { color: var(--accent); font-size: 13px; letter-spacing: .16em; text-transform: uppercase; }
    h1 { margin: 8px 0 8px; font-size: clamp(30px, 5vw, 56px); line-height: 1.05; }
    .intro { max-width: 860px; margin: 0; color: var(--muted); }
    .stats { display: flex; flex-wrap: wrap; gap: 10px; margin-top: 20px; }
    .stat { padding: 7px 11px; border: 1px solid var(--line); border-radius: 999px; background: #111720cc; color: var(--muted); }
    .stat strong { color: var(--text); }
    .toolbar {
      position: sticky;
      top: 0;
      z-index: 5;
      padding: 14px 0;
      background: color-mix(in srgb, var(--bg) 88%, transparent);
      backdrop-filter: blur(18px);
      border-bottom: 1px solid var(--line);
    }
    .toolbar-row { display: flex; gap: 10px; align-items: center; flex-wrap: wrap; }
    .search {
      flex: 1 1 320px;
      min-width: 0;
      padding: 12px 14px;
      color: var(--text);
      background: var(--panel);
      border: 1px solid var(--line);
      border-radius: 10px;
      outline: none;
    }
    .search:focus { border-color: var(--accent); box-shadow: 0 0 0 3px #7ed7e61f; }
    .filters { display: flex; gap: 7px; overflow-x: auto; padding: 10px 0 2px; }
    .chip, .action {
      border: 1px solid var(--line);
      border-radius: 9px;
      background: var(--panel);
      color: var(--muted);
      padding: 8px 11px;
      cursor: pointer;
      white-space: nowrap;
    }
    .chip:hover, .action:hover { color: var(--text); border-color: #526073; }
    .chip.active { color: #081316; background: var(--accent); border-color: var(--accent); }
    main { padding: 26px 0 64px; }
    .section { margin: 0 0 38px; }
    .section-head { display: flex; align-items: baseline; gap: 12px; margin-bottom: 12px; }
    .section h2 { margin: 0; font-size: 22px; }
    .section-count { color: var(--muted); font-size: 13px; }
    .grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(min(100%, 420px), 1fr)); gap: 14px; }
    .card { border: 1px solid var(--line); border-radius: 14px; background: linear-gradient(145deg, var(--panel), #12171e); overflow: hidden; }
    .card-head { padding: 18px 18px 14px; border-bottom: 1px solid var(--line); }
    .card-title { display: flex; gap: 10px; align-items: baseline; }
    .code { color: var(--accent); font: 700 13px/1.2 ui-monospace, Consolas, monospace; }
    .card h3 { margin: 0; font-size: 18px; }
    .meta { display: grid; gap: 8px; margin-top: 13px; color: var(--muted); font-size: 14px; }
    .meta b { color: var(--text); font-weight: 600; }
    .links { display: grid; gap: 1px; background: var(--line); }
    .candidate { display: grid; grid-template-columns: auto 1fr auto; gap: 10px; align-items: center; padding: 11px 14px; background: var(--panel-2); }
    .candidate:hover { background: #242d3a; }
    .candidate input { width: 17px; height: 17px; accent-color: var(--accent-2); }
    .candidate a { color: var(--text); text-decoration: none; min-width: 0; }
    .candidate a:hover { color: var(--accent); }
    .domain { color: var(--muted); font-size: 12px; white-space: nowrap; }
    .card-actions { display: flex; gap: 8px; padding: 12px 14px; }
    .action { padding: 7px 10px; font-size: 13px; }
    .notice { margin: 0 0 22px; padding: 12px 14px; border-left: 3px solid var(--warning); background: #f5cf780d; color: var(--muted); }
    .empty { display: none; padding: 60px 0; text-align: center; color: var(--muted); }
    .toast { position: fixed; right: 18px; bottom: 18px; padding: 10px 14px; border-radius: 9px; color: #071113; background: var(--accent-2); box-shadow: 0 12px 40px #0008; opacity: 0; transform: translateY(8px); pointer-events: none; transition: .2s ease; }
    .toast.show { opacity: 1; transform: translateY(0); }
    @media (max-width: 640px) {
      .shell { width: min(100% - 20px, 1440px); }
      header { padding-top: 30px; }
      .candidate { grid-template-columns: auto 1fr; }
      .domain { grid-column: 2; }
    }
  </style>
</head>
<body>
  <header class="shell">
    <div class="eyebrow">HEARTH · SFX SOURCE NAVIGATOR</div>
    <h1>音效链接导航</h1>
    <p class="intro">双击此 HTML 后，它会在系统默认浏览器中运行。点击候选即可在新标签页打开；勾选框会自动保存在本机，方便你逐项比较并保留中意素材。</p>
    <div class="stats">
      <span class="stat"><strong id="itemCount"></strong> 个音效需求</span>
      <span class="stat"><strong id="linkCount"></strong> 个候选链接</span>
      <span class="stat"><strong id="selectedCount">0</strong> 个已标记</span>
    </div>
  </header>

  <div class="toolbar">
    <div class="shell">
      <div class="toolbar-row">
        <input id="search" class="search" type="search" placeholder="搜索编号、音效、位置、网站……" autofocus>
        <button id="copySelected" class="action" type="button">复制已标记链接</button>
        <button id="clearSelected" class="action" type="button">清除标记</button>
      </div>
      <div id="filters" class="filters"></div>
    </div>
  </div>

  <main class="shell">
    <p class="notice">“打开本项 5 个候选”可能被浏览器的弹窗保护拦截；遇到这种情况，直接逐条点击候选最稳定。</p>
    <div id="content"></div>
    <div id="empty" class="empty">没有匹配结果，请换一个关键词。</div>
  </main>
  <div id="toast" class="toast"></div>

  <script>
    const items = __ITEM_DATA__;
    const categories = __CATEGORY_DATA__;
    const storageKey = 'hearth-sfx-selected-v1';
    let activeCategory = '全部';
    let selected = new Set(JSON.parse(localStorage.getItem(storageKey) || '[]'));

    const escapeHtml = value => String(value)
      .replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;')
      .replaceAll('"', '&quot;').replaceAll("'", '&#039;');
    const domainOf = url => { try { return new URL(url).hostname.replace(/^www\./, ''); } catch { return ''; } };
    const toast = message => {
      const node = document.getElementById('toast');
      node.textContent = message;
      node.classList.add('show');
      clearTimeout(window.toastTimer);
      window.toastTimer = setTimeout(() => node.classList.remove('show'), 1800);
    };
    const saveSelected = () => {
      localStorage.setItem(storageKey, JSON.stringify([...selected]));
      document.getElementById('selectedCount').textContent = selected.size;
    };
    const copyText = async text => {
      try { await navigator.clipboard.writeText(text); }
      catch {
        const area = document.createElement('textarea');
        area.value = text;
        document.body.appendChild(area);
        area.select();
        document.execCommand('copy');
        area.remove();
      }
    };

    function renderFilters() {
      const root = document.getElementById('filters');
      root.innerHTML = ['全部', ...categories].map(category =>
        `<button type="button" class="chip ${category === activeCategory ? 'active' : ''}" data-category="${escapeHtml(category)}">${escapeHtml(category)}</button>`
      ).join('');
      root.querySelectorAll('.chip').forEach(button => button.addEventListener('click', () => {
        activeCategory = button.dataset.category;
        renderFilters();
        renderItems();
      }));
    }

    function renderItems() {
      const query = document.getElementById('search').value.trim().toLowerCase();
      const visible = items.filter(item => {
        const inCategory = activeCategory === '全部' || item.category === activeCategory;
        const haystack = [item.id, item.title, item.placement, item.target, ...item.links.flatMap(link => [link.label, link.url])].join(' ').toLowerCase();
        return inCategory && (!query || haystack.includes(query));
      });

      const grouped = categories.map(category => [category, visible.filter(item => item.category === category)]).filter(([, rows]) => rows.length);
      document.getElementById('content').innerHTML = grouped.map(([category, rows]) => `
        <section class="section">
          <div class="section-head"><h2>${escapeHtml(category)}</h2><span class="section-count">${rows.length} 项</span></div>
          <div class="grid">${rows.map(item => `
            <article class="card" data-id="${escapeHtml(item.id)}">
              <div class="card-head">
                <div class="card-title"><span class="code">${escapeHtml(item.id)}</span><h3>${escapeHtml(item.title)}</h3></div>
                <div class="meta">
                  <div><b>放置：</b>${escapeHtml(item.placement)}</div>
                  <div><b>目标：</b>${escapeHtml(item.target)}</div>
                </div>
              </div>
              <div class="links">${item.links.map((link, index) => `
                <label class="candidate">
                  <input type="checkbox" data-url="${escapeHtml(link.url)}" ${selected.has(link.url) ? 'checked' : ''}>
                  <a href="${escapeHtml(link.url)}" target="_blank" rel="noopener noreferrer">${index + 1}. ${escapeHtml(link.label)} ↗</a>
                  <span class="domain">${escapeHtml(domainOf(link.url))}</span>
                </label>`).join('')}
              </div>
              <div class="card-actions">
                <button type="button" class="action copy-five" data-id="${escapeHtml(item.id)}">复制 5 个链接</button>
                <button type="button" class="action open-five" data-id="${escapeHtml(item.id)}">打开本项 5 个候选</button>
              </div>
            </article>`).join('')}</div>
        </section>`).join('');

      document.getElementById('empty').style.display = visible.length ? 'none' : 'block';
      bindCardActions();
    }

    function bindCardActions() {
      document.querySelectorAll('input[data-url]').forEach(input => input.addEventListener('change', () => {
        input.checked ? selected.add(input.dataset.url) : selected.delete(input.dataset.url);
        saveSelected();
      }));
      document.querySelectorAll('.copy-five').forEach(button => button.addEventListener('click', async () => {
        const item = items.find(row => row.id === button.dataset.id);
        await copyText(item.links.map(link => link.url).join('\n'));
        toast(`已复制 ${item.id} 的 5 个链接`);
      }));
      document.querySelectorAll('.open-five').forEach(button => button.addEventListener('click', () => {
        const item = items.find(row => row.id === button.dataset.id);
        item.links.forEach(link => window.open(link.url, '_blank', 'noopener'));
        toast('已请求打开 5 个标签页');
      }));
    }

    document.getElementById('search').addEventListener('input', renderItems);
    document.getElementById('copySelected').addEventListener('click', async () => {
      if (!selected.size) return toast('还没有标记候选');
      await copyText([...selected].join('\n'));
      toast(`已复制 ${selected.size} 个已标记链接`);
    });
    document.getElementById('clearSelected').addEventListener('click', () => {
      selected.clear();
      saveSelected();
      renderItems();
      toast('已清除全部标记');
    });

    document.getElementById('itemCount').textContent = items.length;
    document.getElementById('linkCount').textContent = items.reduce((sum, item) => sum + item.links.length, 0);
    saveSelected();
    renderFilters();
    renderItems();
  </script>
</body>
</html>
'''
    return template.replace("__ITEM_DATA__", data).replace("__CATEGORY_DATA__", category_data)


def main() -> None:
    items = read_items()
    OUTPUT.write_text(build_html(items), encoding="utf-8", newline="\n")
    print(f"Generated {OUTPUT.name}: {len(items)} items, {sum(len(item['links']) for item in items)} links")


if __name__ == "__main__":
    main()
