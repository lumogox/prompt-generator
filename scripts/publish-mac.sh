#!/usr/bin/env bash
# Publish the Avalonia app as a macOS .app bundle wrapped in a .dmg.
#
# Usage:
#   ./scripts/publish-mac.sh                       # arm64, version 1.0.0
#   VERSION=1.2.0 ./scripts/publish-mac.sh         # custom version
#   RUNTIME=osx-x64 ./scripts/publish-mac.sh       # Intel Macs
#   RUNTIME=osx-x64 VERSION=1.2.0 ./scripts/publish-mac.sh
#
# Output: artifacts/dmg/PromptAssistant-<VERSION>-<RUNTIME>.dmg
#
# Note: this produces an UNSIGNED bundle. macOS Gatekeeper will show a misleading
# "App is damaged" dialog because the .dmg is downloaded with com.apple.quarantine
# set and the binary lacks an Apple Developer ID signature.
#
# End-user workaround:  xattr -cr "/Applications/Prompt Assistant.app"
# (right-click → Open no longer bypasses this on Sonoma+; xattr is the reliable path.)
#
# Proper fix for distribution: codesign with a Developer ID Application certificate +
# notarize via `xcrun notarytool submit` + staple. Requires Apple Developer Program
# enrollment ($99/yr). See README for the workaround documented for end users.
set -euo pipefail

# ─── Config ─────────────────────────────────────────────────────────────────
APP_NAME="Prompt Assistant"
BUNDLE_ID="com.aziwell.promptassistant"
EXECUTABLE_NAME="PromptAssistant.Desktop"   # matches the .csproj output binary
VERSION="${VERSION:-1.0.0}"
RUNTIME="${RUNTIME:-osx-arm64}"             # or osx-x64 for Intel

PROJECT="src/PromptAssistant.Desktop/PromptAssistant.Desktop.csproj"

# ─── Paths ──────────────────────────────────────────────────────────────────
ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PUBLISH_DIR="$ROOT/artifacts/publish/$RUNTIME"
BUNDLE_DIR="$ROOT/artifacts/bundle/$APP_NAME.app"
DMG_STAGING="$ROOT/artifacts/dmg-staging"
DMG_OUT="$ROOT/artifacts/dmg"
DMG_PATH="$DMG_OUT/PromptAssistant-$VERSION-$RUNTIME.dmg"

echo "▶ Cleaning previous artifacts"
rm -rf "$ROOT/artifacts"
mkdir -p "$PUBLISH_DIR" "$BUNDLE_DIR/Contents/MacOS" "$BUNDLE_DIR/Contents/Resources" "$DMG_STAGING" "$DMG_OUT"

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

# ─── 2. Assemble the .app bundle ────────────────────────────────────────────
echo "▶ Assembling .app bundle"
cp -R "$PUBLISH_DIR"/. "$BUNDLE_DIR/Contents/MacOS/"

cat > "$BUNDLE_DIR/Contents/Info.plist" <<EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleName</key>            <string>$APP_NAME</string>
    <key>CFBundleDisplayName</key>     <string>$APP_NAME</string>
    <key>CFBundleIdentifier</key>      <string>$BUNDLE_ID</string>
    <key>CFBundleVersion</key>         <string>$VERSION</string>
    <key>CFBundleShortVersionString</key><string>$VERSION</string>
    <key>CFBundleExecutable</key>      <string>$EXECUTABLE_NAME</string>
    <key>CFBundlePackageType</key>     <string>APPL</string>
    <key>CFBundleInfoDictionaryVersion</key><string>6.0</string>
    <key>LSMinimumSystemVersion</key>  <string>11.0</string>
    <key>NSHighResolutionCapable</key> <true/>
    <key>NSPrincipalClass</key>        <string>NSApplication</string>
    <key>LSApplicationCategoryType</key><string>public.app-category.developer-tools</string>
</dict>
</plist>
EOF

chmod +x "$BUNDLE_DIR/Contents/MacOS/$EXECUTABLE_NAME"
xattr -cr "$BUNDLE_DIR" 2>/dev/null || true

# ─── 3. Stage DMG contents (.app + Applications symlink) ────────────────────
echo "▶ Staging DMG contents"
cp -R "$BUNDLE_DIR" "$DMG_STAGING/"
ln -s /Applications "$DMG_STAGING/Applications"

# ─── 4. Build the DMG ───────────────────────────────────────────────────────
echo "▶ Building DMG"
hdiutil create \
    -volname "$APP_NAME" \
    -srcfolder "$DMG_STAGING" \
    -ov \
    -format UDZO \
    "$DMG_PATH" >/dev/null

DMG_SIZE=$(du -h "$DMG_PATH" | cut -f1)
echo ""
echo "✓ Done"
echo "  DMG:     $DMG_PATH"
echo "  Size:    $DMG_SIZE"
echo "  Runtime: $RUNTIME"
echo "  Version: $VERSION"
echo ""
echo "  Install: open the DMG and drag the .app into Applications."
echo "  First launch on another Mac (unsigned binary) requires stripping quarantine:"
echo "      xattr -cr \"/Applications/$APP_NAME.app\""
echo "  Right-click → Open no longer bypasses Gatekeeper for fully-unsigned apps on"
echo "  macOS Sonoma+; the xattr command is the reliable fix until codesign+notarize"
echo "  is wired up."
