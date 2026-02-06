# Build Log

Tracks each WebGL deployment to GitHub Pages. The build number displayed on the title screen (bottom-left) corresponds to entries here.

**Live URL**: [https://adamhardin.github.io/EpochBreaker/](https://adamhardin.github.io/EpochBreaker/)
**Build ID source**: `EpochBreaker/Assets/Scripts/Gameplay/BuildInfo.cs`

---

| Build | Version | Date | Notes |
|-------|---------|------|-------|
| 001 | 0.1.0 | 2026-02-04 | Initial WebGL deployment — title screen, procedural levels, 10 epochs |
| 002 | 0.1.0 | 2026-02-04 | Fix EventSystem InputModule for new Input System |
| 003 | 0.1.0 | 2026-02-04 | Fix input issues, remove touch controls |
| 004 | 0.1.0 | 2026-02-04 | Migrate to new Input System, add Stomp touch button |
| 005 | 0.1.0 | 2026-02-05 | Boss AI fixes, death effects |
| 006 | 0.1.0 | 2026-02-05 | Game modes (Campaign, Streak, FreePlay), achievements UI, continue system, settings menu |
| 007 | 0.2.0 | 2026-02-06 | Audio volume normalization, build ID on title screen |

---

## How to Update

1. Increment `BUILD_NUMBER` in `BuildInfo.cs`
2. Update `BUILD_DATE` to today's date
3. Update `VERSION` if appropriate
4. Add a row to the table above
5. Build and deploy (see README.md for steps)
