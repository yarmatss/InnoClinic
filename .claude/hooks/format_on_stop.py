# Stop: format the files edited this turn (dotnet format, prettier) and report ESLint errors back to Claude.
import json
import os
import shutil
import subprocess
import sys
import tempfile
from pathlib import Path

payload = json.loads(sys.stdin.buffer.read().decode("utf-8"))
project_dir = Path(os.environ.get("CLAUDE_PROJECT_DIR") or payload["cwd"]).resolve()
state_dir = Path(tempfile.gettempdir()) / "innoclinic-claude-format"
list_file = state_dir / f"{payload['session_id']}.txt"

# Re-arm the Backend/Shared reminder from record_edit.py for the next turn.
(state_dir / f"{payload['session_id']}.shared-reminded").unlink(missing_ok=True)

if not list_file.exists():
    sys.exit(0)

files = [f for f in dict.fromkeys(list_file.read_text(encoding="utf-8").splitlines()) if f and (project_dir / f).exists()]


def run(args, cwd):
    exe = shutil.which(args[0]) or args[0]
    return subprocess.run([exe, *args[1:]], cwd=cwd, capture_output=True, text=True, encoding="utf-8", errors="replace")


def short(result):
    return (result.stderr or result.stdout).strip()[:500]


def format_target(csproj, backend_dir, solutions, chosen):
    # One dotnet format run per solution loads the MSBuild workspace once for all its projects.
    rel = csproj.relative_to(backend_dir).as_posix()
    containing = [s for s, text in solutions.items() if rel in text]
    return next((s for s in containing if s in chosen), containing[0] if containing else csproj)


problems = []
eslint_output = ""

backend_dir = project_dir / "Backend"
solutions = {s: s.read_text(encoding="utf-8") for s in sorted(backend_dir.glob("*.slnx"))}
by_target = {}
for file in (f for f in files if f.startswith("Backend/")):
    path = project_dir / file
    csproj = next(
        (p for d in path.parents if d.is_relative_to(backend_dir) for p in sorted(d.glob("*.csproj"))),
        None,
    )
    if csproj:
        target = format_target(csproj, backend_dir, solutions, by_target)
        by_target.setdefault(target, []).append(str(path.relative_to(backend_dir)))

for target, target_files in by_target.items():
    result = run(["dotnet", "format", str(target.relative_to(backend_dir)), "--no-restore", "--include", *target_files], backend_dir)
    if result.returncode != 0:
        problems.append(f"dotnet format failed for {target.name}: {short(result)}")

frontend_dir = project_dir / "Frontend"
frontend_files = [str((project_dir / f).relative_to(frontend_dir)) for f in files if f.startswith("Frontend/")]

if frontend_files:
    prettier = run(["npx", "--no-install", "prettier", "--write", "--end-of-line", "lf", *frontend_files], frontend_dir)
    if prettier.returncode != 0:
        problems.append(f"prettier failed: {short(prettier)}")

    lintable = [f for f in frontend_files if f.endswith((".ts", ".tsx"))]
    if lintable:
        eslint = run(["npx", "--no-install", "eslint", "--no-warn-ignored", *lintable], frontend_dir)
        if eslint.returncode == 1:
            eslint_output = eslint.stdout.strip()
        elif eslint.returncode != 0:
            problems.append(f"eslint failed: {short(eslint)}")

# Delete the list only after formatting, so a crash or timeout leaves it for the next turn.
list_file.unlink(missing_ok=True)

output = {}
if eslint_output and not payload.get("stop_hook_active"):
    output["decision"] = "block"
    output["reason"] = f"ESLint reported problems in files you edited. Fix them in the code (don't use --fix):\n{eslint_output[:4000]}"
elif eslint_output:
    # Already blocked once this turn; don't loop, but don't drop the errors silently either.
    problems.append(f"ESLint errors remain after Claude's fix attempt:\n{eslint_output[:1500]}")
if problems:
    output["systemMessage"] = "\n".join(problems)
if output:
    print(json.dumps(output))
