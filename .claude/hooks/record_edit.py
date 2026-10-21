# PostToolUse: remember which files were edited so format_on_stop.py can format them once per turn,
# and remind Claude once per turn that Backend/Shared changes only reach services as new package versions.
import json
import os
import re
import sys
import tempfile
from pathlib import Path

payload = json.loads(sys.stdin.buffer.read().decode("utf-8"))
file_path = (payload.get("tool_input") or {}).get("file_path")
session_id = payload.get("session_id")
project_dir = os.environ.get("CLAUDE_PROJECT_DIR") or payload.get("cwd")

if not (file_path and session_id and project_dir):
    sys.exit(0)

try:
    rel = Path(file_path).resolve().relative_to(Path(project_dir).resolve()).as_posix()
except ValueError:
    sys.exit(0)

state_dir = Path(tempfile.gettempdir()) / "innoclinic-claude-format"
state_dir.mkdir(parents=True, exist_ok=True)

is_backend = rel.startswith("Backend/") and rel.endswith(".cs")
is_frontend = (
    re.fullmatch(r"Frontend/.+\.(ts|tsx|js|jsx|mjs|cjs|css|html)", rel) is not None
    and not rel.startswith(("Frontend/node_modules/", "Frontend/dist/"))
)

if is_backend or is_frontend:
    with open(state_dir / f"{session_id}.txt", "a", encoding="utf-8", newline="\n") as f:
        f.write(rel + "\n")

shared_marker = state_dir / f"{session_id}.shared-reminded"
if rel.startswith("Backend/Shared/") and not shared_marker.exists():
    shared_marker.touch()
    print(json.dumps({
        "hookSpecificOutput": {
            "hookEventName": "PostToolUse",
            "additionalContext": (
                f"You edited {rel} in Backend/Shared. Services consume Shared as NuGet packages, so this change "
                "has no effect on them until a new package version is restored. To build or test services against it, "
                "use the shared-package-local skill. Before finishing, tell the user which shared csproj <Version> values "
                "to bump and which Directory.Packages.props entries to update after publishing."
            ),
        }
    }))
