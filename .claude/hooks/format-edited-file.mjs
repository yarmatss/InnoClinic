// PostToolUse hook: formats the file Claude just edited so it matches CI and the husky pre-commit hook.
import { execFileSync } from "node:child_process";
import { existsSync, readdirSync, readFileSync } from "node:fs";
import path from "node:path";

const input = JSON.parse(readFileSync(0, "utf8"));
const filePath = input.tool_input?.file_path;
if (!filePath || !existsSync(filePath)) process.exit(0);

const root = process.env.CLAUDE_PROJECT_DIR ?? input.cwd;
const backend = path.join(root, "Backend");
const frontend = path.join(root, "Frontend");
const isUnder = (dir, p = filePath) => {
  const rel = path.relative(dir, p);
  return !rel.startsWith("..") && !path.isAbsolute(rel);
};

function run(file, args, cwd) {
  try {
    execFileSync(file, args, { cwd, stdio: ["ignore", "ignore", "pipe"] });
  } catch (err) {
    process.stderr.write(`format hook: ${err.stderr?.toString() || err.message}\n`);
  }
}

function findProject(dir) {
  for (; isUnder(backend, dir); dir = path.dirname(dir)) {
    const csproj = readdirSync(dir).find((f) => f.endsWith(".csproj"));
    if (csproj) return path.join(dir, csproj);
  }
  return null;
}

if (filePath.endsWith(".cs") && isUnder(backend)) {
  run("dotnet", ["format", "whitespace", "--folder", "--include", filePath], backend);
  const project = findProject(path.dirname(filePath));
  if (project) {
    // Sorts usings per Backend/.editorconfig; whitespace mode alone doesn't.
    run("dotnet", ["format", "style", project, "--include", filePath, "--diagnostics", "IDE0055", "--severity", "info"], backend);
  }
} else if (/\.(ts|tsx|css|html)$/.test(filePath) && isUnder(path.join(frontend, "src"))) {
  const prettier = path.join(frontend, "node_modules", "prettier", "bin", "prettier.cjs");
  if (existsSync(prettier)) run("node", [prettier, "--write", "--end-of-line", "lf", filePath], frontend);
}
