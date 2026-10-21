# PreToolUse: block edits to committed EF migrations; they may already be applied to local and Azure databases.
import json
import os
import re
import subprocess
import sys
from pathlib import Path

payload = json.loads(sys.stdin.buffer.read().decode("utf-8"))
file_path = (payload.get("tool_input") or {}).get("file_path")
project_dir = os.environ.get("CLAUDE_PROJECT_DIR") or payload.get("cwd")

if not (file_path and project_dir):
    sys.exit(0)

try:
    rel = Path(file_path).resolve().relative_to(Path(project_dir).resolve()).as_posix()
except ValueError:
    sys.exit(0)

if not re.fullmatch(r"Backend/.+/Migrations/[^/]+\.cs", rel):
    sys.exit(0)

tracked = subprocess.run(
    ["git", "ls-files", "--error-unmatch", rel],
    cwd=project_dir,
    capture_output=True,
).returncode == 0

if tracked:
    print(json.dumps({
        "hookSpecificOutput": {
            "hookEventName": "PreToolUse",
            "permissionDecision": "deny",
            "permissionDecisionReason": (
                f"{rel} is a committed migration. Migrations are applied automatically on startup, "
                "locally and in Azure, so editing one that already ran causes schema drift. "
                "Add a new migration instead (ef-migration skill). If the user explicitly wants this file edited, "
                "explain that .claude/hooks/guard_migrations.py blocks it and let them edit it themselves."
            ),
        }
    }))
