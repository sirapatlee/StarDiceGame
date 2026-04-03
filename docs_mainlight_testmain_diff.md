# MainLight.unity vs TestMain.unity (quick diff)

Date: 2026-04-03

## Key findings

1. **Skybox material is different**
   - `TestMain.unity` uses `fire skybox.mat` (`guid: fe305c3e2b433404cb0ddfaf13f18afe`).
   - `MainLight.unity` uses `light material.mat` (`guid: 108fa53cdc9d0ba47adbab9a763b2b85`).

2. **MainLight has fewer prefab instances in scene root**
   - `TestMain.unity` has 6 `PrefabInstance` roots:
     - ChoiceUIManager
     - BoardManager
     - Canvas
     - UIManager
     - shopmanager
     - GameTurnManager
   - `MainLight.unity` has only 2 `PrefabInstance` roots:
     - UIManager
     - Canvas

3. **MainLight contains Light-specific gimmick scripts that TestMain does not**
   - `MainLightHealGimmickTurnTicker`
   - `MainLightHealGimmickController`

4. **TestMain contains scripts absent from MainLight**
   - `PlayerStatusPanelRefs` appears in TestMain but not MainLight.
   - One script guid (`67db9e8f0e2ae9c40bc1e2b64352a6b4`) is referenced in TestMain and is likely package/external script (meta not found under `Assets/**/*.meta`).

## Why MainLight may behave strangely

Most likely structural cause is that `MainLight.unity` is missing several manager prefabs that are present in `TestMain.unity` (BoardManager / GameTurnManager / ShopManager / ChoiceUIManager). If gameplay flow depends on these managers being scene-level roots, behavior differences are expected.

## Suggested next checks in Unity Editor

1. Open both scenes and compare **Hierarchy root objects**.
2. Verify whether `BoardManager`, `GameTurnManager`, and `ShopManager` are instantiated elsewhere in MainLight (runtime or additive load) or truly missing.
3. Confirm if Light gimmick scripts are intentionally replacing normal board flow in MainLight.
4. If behavior should be identical, start by adding the missing manager prefabs into MainLight and retest.

## RuntimeHub warning analysis (2 EventSystem / 2 AudioListener)

- `RuntimeHub.unity` contains both `EventSystem` and `AudioListener`.
- `MainLight.unity` also contains both `EventSystem` and `AudioListener`.
- If scene flow loads `MainLight` **additively** from `RuntimeHub` and keeps `RuntimeHub` alive, Unity will warn about duplicates.

KISS fix applied in `RuntimeHubController`:
- After additive load + set active scene, keep only one `EventSystem` and one `AudioListener` (prefer objects in the newly loaded scene) and disable the rest.
