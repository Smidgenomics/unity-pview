
# 0.3.0

Note: Includes breaking changes to API.

## Changed

* (Menu) Renamed menu item types: `menu`->`cmd`, `create`->`new`\
(Old names still work but are marked obsolete)
* (Icons) Renamed several icon rule properties for internal consistency (`coords` -> `uv` etc).

## Added

* (Icons) Generic regex support for rules.
* (Icons) Small icon variants, used for list view.
* (Icons) Default icon for folders.
* (Icons) Profile include list (mix-in).
* (Icons) Multi-rule syntax for icon settings re-use.
* (Icons) Custom icon background color.
* (Icons) Right-alignment flag (useful for type icons whose assets have previews).

## Removed

* (Icons) Removed `extends` field from icons (supplanted by include list).

## Fixed

* Bug causing wrong window to focus when context clicking with multiple project windows open.
* "Bug" causing custom context menu not showing when right clicking inside empty folder (may become a settings flag in the future if workaround proves unsafe).

## Other

* Misc. stabilizations and refactor.
* Example json profiles and icon atlas.

# 0.2.0

## Changed

* Overhauled plugin project settings.
* Misc. refactor and stabilization.

## Added

* JSON profiles for project context menu and icons.
* Support for alt key to override context menu.


## Removed

* ScriptableObject configs.