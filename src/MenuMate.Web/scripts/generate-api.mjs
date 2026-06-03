import { mkdir } from "node:fs/promises"
import path from "node:path"
import { fileURLToPath } from "node:url"
import { spawn } from "node:child_process"

const directoryName = path.dirname(fileURLToPath(import.meta.url))
const webRoot = path.resolve(directoryName, "..")
const repoRoot = path.resolve(webRoot, "..", "..")
const apiProject = process.env.MENUMATE_API_PROJECT
  ? path.resolve(process.env.MENUMATE_API_PROJECT)
  : path.resolve(repoRoot, "src", "MenuMate.Api", "MenuMate.Api.csproj")
const apiUrl = process.env.MENUMATE_OPENAPI_URL ?? "http://127.0.0.1:5019"
const openApiUrl = `${apiUrl.replace(/\/+$/, "")}/openapi/v1.json`
const outputPath = path.resolve(webRoot, "src", "shared", "api", "generated", "schema.d.ts")
const dataProtectionKeysPath = path.resolve(webRoot, ".tmp", "data-protection-keys")

await mkdir(path.dirname(outputPath), { recursive: true })

const apiProcess = spawn(
  "dotnet",
  ["run", "--no-build", "--project", apiProject, "--no-launch-profile", "--urls", apiUrl],
  {
    cwd: repoRoot,
    env: {
      ...process.env,
      ASPNETCORE_ENVIRONMENT: process.env.ASPNETCORE_ENVIRONMENT ?? "Development",
      ConnectionStrings__Database:
        process.env.ConnectionStrings__Database ??
        "Host=127.0.0.1;Port=5432;Database=menumate_openapi;Username=postgres;Password=postgres",
      "Logging__LogLevel__Microsoft.AspNetCore.DataProtection": "Critical",
      MENUMATE_DATA_PROTECTION_KEYS_PATH:
        process.env.MENUMATE_DATA_PROTECTION_KEYS_PATH ?? dataProtectionKeysPath,
    },
    stdio: ["ignore", "pipe", "pipe"],
  },
)

apiProcess.stdout.on("data", (chunk) => {
  process.stdout.write(chunk)
})

apiProcess.stderr.on("data", (chunk) => {
  process.stderr.write(chunk)
})

try {
  await waitForOpenApi(openApiUrl)
  await run("pnpm", ["exec", "openapi-typescript", openApiUrl, "-o", outputPath], webRoot)
} finally {
  apiProcess.kill()
}

async function waitForOpenApi(url) {
  const deadline = Date.now() + 30_000

  while (Date.now() < deadline) {
    try {
      const response = await fetch(url)
      if (response.ok) {
        return
      }
    } catch {
      // API is still starting.
    }

    await new Promise((resolve) => setTimeout(resolve, 500))
  }

  throw new Error(`Не удалось получить OpenAPI спецификацию по адресу ${url}`)
}

async function run(command, args, cwd) {
  const isWindows = process.platform === "win32"
  const executable = isWindows ? `${command}.cmd` : command

  await new Promise((resolve, reject) => {
    const child = spawn(executable, args, {
      cwd,
      stdio: "inherit",
      shell: isWindows,
    })

    child.on("error", reject)

    child.on("exit", (code) => {
      if (code === 0) {
        resolve()
        return
      }

      reject(new Error(`${command} ${args.join(" ")} завершился с кодом ${String(code)}`))
    })
  })
}
