# HUD Prototype Review Page

**Date:** 2026-02-09
**Purpose:** Single-page browser review for comparing multiple HUD layout prototypes (Options A-D) on one screen.

---

## 1) Quick Start (Localhost)

1. Create a folder for the prototype page (example: docs/hud-prototypes/)
2. Add two files:
   - index.html (use the HTML below)
   - styles.css (use the CSS below)
3. Serve locally:

```
python3 -m http.server 8000
```

4. Open:

```
http://localhost:8000
```

---

## 2) HTML (index.html)

```html
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>HUD Prototype Review</title>
  <link rel="stylesheet" href="styles.css" />
</head>
<body>
  <header class="page-header">
    <h1>HUD Prototype Review</h1>
    <p>Compare Options A-D. Each block simulates a 16:9 gameplay viewport.</p>
  </header>

  <main class="grid">
    <section class="card">
      <h2>Option A - Three Column Bottom Bar</h2>
      <div class="screen option-a">
        <div class="hud">
          <div class="panel bottom left">
            <div class="primary">Weapons + HP</div>
            <div class="secondary">Abilities</div>
            <div class="secondary">Heat/Special</div>
          </div>
          <div class="panel bottom center">
            <div class="primary">Score 350</div>
            <div class="primary">0:12.9</div>
            <div class="secondary">Quick Draw</div>
          </div>
          <div class="panel bottom right">
            <div class="primary">Era 1 [E-XXXXXX]</div>
            <div class="secondary">Relics 24/25</div>
            <div class="secondary">Lives 2</div>
          </div>
        </div>
      </div>
    </section>

    <section class="card">
      <h2>Option B - Left Rail + Top Bar</h2>
      <div class="screen option-b">
        <div class="hud">
          <div class="panel left-rail">
            <div class="primary">Weapon Dock</div>
            <div class="secondary">Abilities</div>
            <div class="secondary">Special/Heat</div>
          </div>
          <div class="panel top left">
            <div class="primary">HP</div>
            <div class="secondary">Lives 2</div>
          </div>
          <div class="panel top center">
            <div class="primary">Score 350</div>
            <div class="primary">0:12.9</div>
          </div>
          <div class="panel top right">
            <div class="primary">Era 1 [E-XXXXXX]</div>
            <div class="secondary">Relics 24/25</div>
          </div>
        </div>
      </div>
    </section>

    <section class="card">
      <h2>Option C - Top Progression, Bottom Combat</h2>
      <div class="screen option-c">
        <div class="hud">
          <div class="panel top full">
            <div class="primary">Era 1 [E-XXXXXX]</div>
            <div class="secondary">Relics 24/25 | Lives 2</div>
          </div>
          <div class="panel bottom full">
            <div class="primary">Weapon Dock + HP</div>
            <div class="secondary">Abilities | Special/Heat</div>
            <div class="secondary">Score 350 | 0:12.9</div>
          </div>
        </div>
      </div>
    </section>

    <section class="card">
      <h2>Option D - Corner Clusters (Preferred)</h2>
      <div class="screen option-d">
        <div class="hud">
          <div class="panel top left">
            <div class="primary">HP</div>
            <div class="secondary">Lives 2 | Easy 0.5X</div>
          </div>
          <div class="panel top right">
            <div class="primary">Era 1 [E-XXXXXX]</div>
            <div class="secondary">Relics 24/25</div>
            <div class="secondary">Items HP:2 ATK:1</div>
          </div>
          <div class="panel bottom left">
            <div class="primary">Weapon Dock</div>
            <div class="secondary">Abilities</div>
            <div class="secondary">Special/Heat</div>
          </div>
          <div class="panel bottom right">
            <div class="primary">Score 350</div>
            <div class="primary">0:12.9</div>
            <div class="secondary">>>AUTO: Best DPS</div>
          </div>
        </div>
      </div>
    </section>
  </main>
</body>
</html>
```

---

## 3) CSS (styles.css)

```css
:root {
  --bg: #0e0b16;
  --panel: rgba(12, 10, 20, 0.8);
  --border: rgba(200, 160, 90, 0.8);
  --primary: #ffffff;
  --secondary: #cfd2d9;
}

* { box-sizing: border-box; }

body {
  margin: 0;
  background: #0b0a12;
  color: var(--primary);
  font-family: "Courier New", monospace;
}

.page-header {
  padding: 24px;
  border-bottom: 1px solid #222;
}

.grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(360px, 1fr));
  gap: 24px;
  padding: 24px;
}

.card {
  background: #111019;
  border: 1px solid #1f1b2a;
  padding: 16px;
}

.screen {
  position: relative;
  width: 100%;
  aspect-ratio: 16 / 9;
  background: var(--bg);
  border: 1px solid #2b2438;
  margin-top: 12px;
  overflow: hidden;
}

.hud {
  position: absolute;
  inset: 0;
}

.panel {
  position: absolute;
  background: var(--panel);
  border: 2px solid var(--border);
  padding: 10px 12px;
  box-shadow: 0 8px 18px rgba(0, 0, 0, 0.35);
}

.primary { color: var(--primary); font-size: 14px; line-height: 1.4; }
.secondary { color: var(--secondary); font-size: 11px; line-height: 1.6; }

/* Generic panel placements */
.top.left { top: 12px; left: 12px; }
.top.right { top: 12px; right: 12px; text-align: right; }
.bottom.left { bottom: 12px; left: 12px; }
.bottom.right { bottom: 12px; right: 12px; text-align: right; }

/* Option A */
.option-a .bottom.left { left: 12px; bottom: 12px; }
.option-a .bottom.center { left: 50%; bottom: 12px; transform: translateX(-50%); }
.option-a .bottom.right { right: 12px; bottom: 12px; text-align: right; }

/* Option B */
.option-b .left-rail { left: 12px; top: 12px; bottom: 12px; width: 140px; }
.option-b .top.left { left: 170px; }
.option-b .top.center { left: 50%; transform: translateX(-50%); top: 12px; }
.option-b .top.right { right: 12px; }

/* Option C */
.option-c .top.full { left: 12px; right: 12px; top: 12px; }
.option-c .bottom.full { left: 12px; right: 12px; bottom: 12px; }
```

---

## 4) Review Notes

- This page is for spatial comparison only. Do not treat typography, colors, or borders as final.
- If you want to simulate safe-area insets, add extra padding to .top.left and .top.right or create a class like .safe-top { top: 36px; }.

---

**End of document**
