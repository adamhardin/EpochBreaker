#!/usr/bin/env bash
set -euo pipefail

# ── Epoch Breaker: Test & Build Script ──
# Runs Play Mode smoke tests, then builds WebGL if all pass.
# Usage:
#   ./scripts/test-and-build.sh          # Run tests + build
#   ./scripts/test-and-build.sh --test   # Run tests only (no build)

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
UNITY_PROJECT="$PROJECT_ROOT/EpochBreaker"
UNITY="/Applications/Unity/Hub/Editor/6000.3.6f1/Unity.app/Contents/MacOS/Unity"
RESULTS_DIR="$PROJECT_ROOT/test-results"
RESULTS_XML="$RESULTS_DIR/smoke-tests.xml"
LOG_FILE="$RESULTS_DIR/test-log.txt"

# Parse args
TEST_ONLY=false
if [[ "${1:-}" == "--test" ]]; then
    TEST_ONLY=true
fi

# Ensure Unity exists
if [[ ! -x "$UNITY" ]]; then
    echo "ERROR: Unity not found at $UNITY"
    echo "Update the UNITY path in this script to match your installation."
    exit 1
fi

# Create results directory
mkdir -p "$RESULTS_DIR"

echo "══════════════════════════════════════════════"
echo "  Epoch Breaker — Pre-Deploy Test Suite"
echo "══════════════════════════════════════════════"
echo ""
echo "Project: $UNITY_PROJECT"
echo "Unity:   $UNITY"
echo ""

# ── Step 1: Run Smoke Tests ──
echo "▶ Running Play Mode smoke tests..."
echo "  (This opens Unity in batch mode — no GUI window)"
echo ""

# Close Unity if it's already open with this project (batch mode requires exclusive access)
if pgrep -f "Unity.*EpochBreaker" > /dev/null 2>&1; then
    echo "WARNING: Unity appears to be running with this project."
    echo "Batch mode requires exclusive access. Close Unity first, or"
    echo "use the Test Runner inside Unity (Window > General > Test Runner)."
    exit 1
fi

"$UNITY" \
    -runTests \
    -batchmode \
    -nographics \
    -projectPath "$UNITY_PROJECT" \
    -testPlatform PlayMode \
    -testCategory "Smoke" \
    -testResults "$RESULTS_XML" \
    -logFile "$LOG_FILE" \
    2>&1 || true

# Parse results from XML
if [[ ! -f "$RESULTS_XML" ]]; then
    echo ""
    echo "ERROR: Test results file not generated."
    echo "Check the log: $LOG_FILE"
    exit 1
fi

# Extract pass/fail counts from NUnit XML
TOTAL=$(grep -o 'total="[0-9]*"' "$RESULTS_XML" | head -1 | grep -o '[0-9]*')
PASSED=$(grep -o 'passed="[0-9]*"' "$RESULTS_XML" | head -1 | grep -o '[0-9]*')
FAILED=$(grep -o 'failed="[0-9]*"' "$RESULTS_XML" | head -1 | grep -o '[0-9]*')
RESULT=$(grep -o 'result="[A-Za-z]*"' "$RESULTS_XML" | head -1 | grep -o '"[A-Za-z]*"' | tr -d '"')

echo ""
echo "── Test Results ──"
echo "  Total:  ${TOTAL:-?}"
echo "  Passed: ${PASSED:-?}"
echo "  Failed: ${FAILED:-?}"
echo "  Result: ${RESULT:-?}"
echo ""
echo "  Full results: $RESULTS_XML"
echo "  Full log:     $LOG_FILE"
echo ""

if [[ "${FAILED:-0}" != "0" ]] || [[ "${RESULT:-}" == "Failed" ]]; then
    echo "FAILED: ${FAILED} test(s) failed. Fix failures before deploying."
    echo ""
    # Show failed test names
    echo "Failed tests:"
    grep -o 'name="[^"]*"' "$RESULTS_XML" | while read -r line; do
        # This is a rough extraction — check the XML for details
        echo "  - $line"
    done
    exit 1
fi

echo "ALL TESTS PASSED"
echo ""

if $TEST_ONLY; then
    echo "Done (--test mode, skipping build)."
    exit 0
fi

# ── Step 2: Build WebGL ──
echo "▶ Building WebGL..."
echo ""

BUILD_DIR="$PROJECT_ROOT/WebGL-Build"

"$UNITY" \
    -batchmode \
    -nographics \
    -projectPath "$UNITY_PROJECT" \
    -buildTarget WebGL \
    -executeMethod BuildScript.BuildWebGL \
    -logFile "$RESULTS_DIR/build-log.txt" \
    -quit

if [[ $? -eq 0 ]]; then
    echo ""
    echo "BUILD SUCCEEDED"
    echo "Output: $BUILD_DIR"
else
    echo ""
    echo "BUILD FAILED — check $RESULTS_DIR/build-log.txt"
    exit 1
fi

echo ""
echo "══════════════════════════════════════════════"
echo "  Ready to deploy!"
echo "══════════════════════════════════════════════"
