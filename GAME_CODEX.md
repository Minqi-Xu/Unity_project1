# Game Codex

Open Start Menu and select **Index**, between Settings and Quit Game. The active codex is English-only and uses the existing TextMeshPro default font, with LiberationSans SDF as fallback. It does not load a Chinese/system font.

## Pages

- Characters: traits, limitations, initial attributes, starting weapons and their related buffs. Six selectable characters and two configured characters marked Not selectable.
- Auxiliary Weapons: the five droppable weapon types, grouped by weapon and then rarity, with base stats and corresponding buffs.
- Buffs: Attack, Survival, Mobility, Growth, Universal & Compound, Weapon Specific; then Common, Rare, Epic, Legendary. Search names and effects within the current page.

## Rarity cards

Each buff's complete description panel has a rarity-tinted background and matching outline: Common green, Rare blue, Epic purple, Legendary gold. Rarity is also written in the card alongside its max level. Text remains light for readability. Character and weapon-stat panels retain their neutral backgrounds.

## Catalog

There are 100 buff families and 196 rarity variants, plus five Rare weapon unlock cards (201 total reward definitions). The codex shares the actual gameplay factories and Resources character/weapon data; browsing does not initialize run managers or modify player selection.

The complete snapshot is in BUFF_CATALOG.md. Regenerate it with `python Tests/export_buff_catalog.py`. The optional Unity command `Tools > Codex > Export Current Catalog to Markdown` exports an English live catalog to GAME_CODEX_EXPORT.md.

## Verification

Full runtime/editor compilation and standalone combat/time/speed checks passed. English-only source/data checks passed. The existing catalog smoke test also checks English descriptions and distinct backgrounds for all four rarities. Editor smoke tests and Play Mode visual/input verification have not been executed.

Exit Play Mode and run StartMenu again after this change to rebuild previously created UI objects.
