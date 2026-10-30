"""Check actual TTF/TTC cmap coverage without requiring a Unity editor process."""
import bisect
import os
from pathlib import Path
import struct
import sys

root = Path(__file__).resolve().parent.parent
font = Path(sys.argv[1]) if len(sys.argv) > 1 else Path(os.environ.get("WINDIR", "C:/Windows")) / "Fonts/msyh.ttc"
data = font.read_bytes()
u16 = lambda offset: struct.unpack_from(">H", data, offset)[0]
u32 = lambda offset: struct.unpack_from(">I", data, offset)[0]
face = u32(12) if data[:4] == b"ttcf" else 0
tables = {data[pos:pos + 4]: u32(pos + 8) for pos in range(face + 12, face + 12 + u16(face + 4) * 16, 16)}
cmap = tables[b"cmap"]
subtables = []
for pos in range(cmap + 4, cmap + 4 + u16(cmap + 2) * 8, 8):
    platform, encoding = u16(pos), u16(pos + 2)
    address = cmap + u32(pos + 4)
    if (platform == 0 or platform == 3 and encoding in (1, 10)) and u16(address) in (4, 12):
        subtables.append(address)


def glyph(code, address):
    if u16(address) == 12:
        groups = [struct.unpack_from(">III", data, pos) for pos in range(address + 16, address + 16 + u32(address + 12) * 12, 12)]
        index = bisect.bisect_right([start for start, end, base in groups], code) - 1
        if index >= 0:
            start, end, base = groups[index]
            return base + code - start if code <= end else 0
        return 0
    segments = u16(address + 6) // 2
    for index in range(segments):
        end = u16(address + 14 + index * 2)
        if code > end:
            continue
        start = u16(address + 16 + segments * 2 + index * 2)
        if code < start:
            return 0
        delta = u16(address + 16 + segments * 4 + index * 2)
        offset_pos = address + 16 + segments * 6 + index * 2
        offset = u16(offset_pos)
        if offset == 0:
            return (code + delta) & 0xffff
        value = u16(offset_pos + offset + (code - start) * 2)
        return (value + delta) & 0xffff if value else 0
    return 0


sources = root / "Unity6-project1/Assets/Scripts/StartMenu"
text = "".join(path.read_text(encoding="utf-8-sig") for path in sources.glob("*.cs"))
required = {ord(char) for char in text if "\u4e00" <= char <= "\u9fff"}
required.update(range(32, 127))
required.update(map(ord, "≤≥−×→…"))
missing = [chr(code) for code in sorted(required) if not any(glyph(code, address) for address in subtables)]
if missing:
    raise SystemExit("Missing glyphs: " + "".join(missing))
print(f"PASS: {font.name} face 0 contains all {len(required)} required Chinese, ASCII and UI symbol glyphs.")
