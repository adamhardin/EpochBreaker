#!/bin/bash
set -e

# ── Configuration ──────────────────────────────────────────────
UNITY="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"
PROJECT_DIR="$(cd "$(dirname "$0")/EpochBreaker" && pwd)"
BUILD_DIR="$(cd "$(dirname "$0")" && pwd)/EpochBreaker/build/WebGL/WebGL"
BUILD_METHOD="EpochBreaker.Editor.WebGLBuildScript.Build"
REPO_ROOT="$(cd "$(dirname "$0")" && pwd)"

# ── Preflight checks ──────────────────────────────────────────
if [ ! -f "$UNITY" ]; then
    echo "ERROR: Unity not found at $UNITY"
    echo "Update the UNITY path in this script to match your installation."
    exit 1
fi

if [ ! -d "$PROJECT_DIR/Assets" ]; then
    echo "ERROR: Unity project not found at $PROJECT_DIR"
    exit 1
fi

# Parse flags
FORCE=false
for arg in "$@"; do
    case "$arg" in
        --force|-y) FORCE=true ;;
    esac
done

# Check for uncommitted changes on current branch
if ! git -C "$REPO_ROOT" diff-index --quiet HEAD -- 2>/dev/null; then
    echo "WARNING: You have uncommitted changes on the current branch."
    echo "They won't affect the deploy, but you may want to commit them first."
    if [ "$FORCE" = true ]; then
        echo "Continuing (--force)."
    else
        read -p "Continue anyway? [y/N] " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            exit 1
        fi
    fi
fi

# ── Step 1: Build WebGL ───────────────────────────────────────
echo ""
echo "═══════════════════════════════════════════════════"
echo "  Building WebGL (this may take 10-20 minutes)..."
echo "═══════════════════════════════════════════════════"
echo ""

"$UNITY" \
    -batchmode \
    -nographics \
    -quit \
    -projectPath "$PROJECT_DIR" \
    -executeMethod "$BUILD_METHOD" \
    -logFile -

BUILD_EXIT=$?
if [ $BUILD_EXIT -ne 0 ]; then
    echo ""
    echo "ERROR: Unity build failed with exit code $BUILD_EXIT"
    echo "Check the log output above for details."
    exit 1
fi

if [ ! -d "$BUILD_DIR" ]; then
    echo "ERROR: Build output not found at $BUILD_DIR"
    exit 1
fi

echo ""
echo "Build succeeded! Output at: $BUILD_DIR"
echo ""

# ── Step 2: Deploy to gh-pages branch ─────────────────────────
echo "═══════════════════════════════════════════════════"
echo "  Deploying to gh-pages branch..."
echo "═══════════════════════════════════════════════════"
echo ""

CURRENT_BRANCH=$(git -C "$REPO_ROOT" rev-parse --abbrev-ref HEAD)

# Create a temporary directory for the deploy
DEPLOY_TMP=$(mktemp -d)
trap "rm -rf $DEPLOY_TMP" EXIT

# Copy build output to temp dir
cp -R "$BUILD_DIR"/* "$DEPLOY_TMP"/

# Add .nojekyll to prevent GitHub Pages from processing with Jekyll
touch "$DEPLOY_TMP/.nojekyll"

# Initialize git in temp dir and force push to gh-pages
cd "$DEPLOY_TMP"
git init
git checkout -b gh-pages
git add -A
git commit -m "Deploy WebGL build $(date '+%Y-%m-%d %H:%M:%S')"

# Get the remote URL from the main repo
REMOTE_URL=$(git -C "$REPO_ROOT" remote get-url origin)
git remote add origin "$REMOTE_URL"
git config http.postBuffer 52428800
git push -f origin gh-pages

cd "$REPO_ROOT"

echo ""
echo "═══════════════════════════════════════════════════"
echo "  Deploy complete!"
echo "═══════════════════════════════════════════════════"
echo ""
echo "  The GitHub Actions workflow will now publish to"
echo "  GitHub Pages. Check the Actions tab for progress."
echo ""
echo "  Your game will be live at:"
echo "  https://adamhardin.github.io/EpochBreaker/"
echo ""
