#!/usr/bin/env bash
# Publish the Avalonia app as a self-contained Linux tarball.
#
# Usage:
#   ./scripts/publish-linux.sh                        # x64, version 1.0.0
#   VERSION=1.2.0 ./scripts/publish-linux.sh          # custom version
#   RUNTIME=linux-arm64 ./scripts/publish-linux.sh    # ARM64 (Raspberry Pi, M1 under Rosetta)
#
# Output: artifacts/tar/PromptAssistant-<VERSION>-<RUNTIME>.tar.gz
#
# IMPORTANT — system library prerequisites on the target machine:
#   The tarball bundles the .NET runtime but NOT display-system libraries.
#   The target machine must have:
#     libX11, libXrandr, libXi, libXext   (X11 display — or Wayland compositor)
#     libGL / libEGL                       (GPU compositing via Skia)
#     libfontconfig                        (font enumeration)
#     libicu / libssl                      (already present on most distros)
#   Ubuntu/Debian:   sudo apt install libx11-6 libxrandr2 libxi6 libgl1 libfontconfig1
#   Fedora/RHEL:     sudo dnf install libX11 libXrandr libXi mesa-libGL fontconfig
#   Arch Linux:      sudo pacman -S libx11 libxrandr libxi mesa fontconfig
#   These are pre-installed on most desktop distros. Headless servers will need them added.
set -euo pipefail

# ─── Config ─────────────────────────────────────────────────────────────────
APP_NAME="Prompt Assistant"
EXECUTABLE_NAME="PromptAssistant.Desktop"
VERSION="${VERSION:-1.0.0}"
RUNTIME="${RUNTIME:-linux-x64}"

PROJECT="src/PromptAssistant.Desktop/PromptAssistant.Desktop.csproj"

# ─── Paths ──────────────────────────────────────────────────────────────────
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PUBLISH_DIR="$ROOT/artifacts/publish/$RUNTIME"
TAR_OUT="$ROOT/artifacts/tar"
TAR_PATH="$TAR_OUT/PromptAssistant-$VERSION-$RUNTIME.tar.gz"

# The tarball extracts into a named folder (not loose files).
FOLDER_NAME="PromptAssistant-$VERSION-$RUNTIME"

echo "▶ Cleaning previous artifacts"
rm -rf "$ROOT/artifacts"
mkdir -p "$PUBLISH_DIR" "$TAR_OUT"

# ─── 1. Publish self-contained binary ───────────────────────────────────────
echo "▶ Publishing $RUNTIME (self-contained)"
dotnet publish "$ROOT/$PROJECT" \
    -c Release \
    -r "$RUNTIME" \
    --self-contained true \
    -p:PublishSingleFile=false \
    -p:UseAppHost=true \
    -p:Version="$VERSION" \
    -o "$PUBLISH_DIR"

chmod +x "$PUBLISH_DIR/$EXECUTABLE_NAME"

# ─── 2. Create tarball (extract-to-named-folder layout) ─────────────────────
echo "▶ Creating tarball"
# Rename publish dir temporarily so the tar contains a top-level named folder.
STAGING_DIR="$(dirname "$PUBLISH_DIR")/$FOLDER_NAME"
mv "$PUBLISH_DIR" "$STAGING_DIR"

tar -czf "$TAR_PATH" \
    -C "$(dirname "$STAGING_DIR")" \
    "$FOLDER_NAME"

# Restore publish dir name for consistent artifact paths.
mv "$STAGING_DIR" "$PUBLISH_DIR"

TAR_SIZE=$(du -h "$TAR_PATH" | cut -f1)
echo ""
echo "✓ Done"
echo "  Tarball: $TAR_PATH"
echo "  Size:    $TAR_SIZE"
echo "  Runtime: $RUNTIME"
echo "  Version: $VERSION"
echo ""
echo "  Install: tar -xzf $(basename "$TAR_PATH")"
echo "           cd $FOLDER_NAME && ./$EXECUTABLE_NAME"
echo ""
echo "  Reminder: display libraries (libX11, libGL, libfontconfig) must be"
echo "  present on the target machine. See script header for distro commands."
