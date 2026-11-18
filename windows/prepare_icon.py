from pathlib import Path
from PIL import Image

html = Path("app/src/main/assets/index.html")
png = Path("app/src/main/res/drawable-nodpi/html_os_icon.png")
ico = Path("windows/assets/html_os_icon.ico")

if not html.is_file() or html.stat().st_size < 1000:
    raise SystemExit("Main HTML OS source is missing or unexpectedly small")
if not png.is_file():
    raise SystemExit("HTML OS logo PNG is missing")

with Image.open(png) as image:
    image.verify()

with Image.open(png) as image:
    image.convert("RGBA").save(
        ico,
        format="ICO",
        sizes=[(16, 16), (24, 24), (32, 32), (48, 48), (64, 64), (96, 96)],
    )

print(f"Validated HTML ({html.stat().st_size} bytes) and logo ({png.stat().st_size} bytes)")
