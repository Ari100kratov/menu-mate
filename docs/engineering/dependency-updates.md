# Обновление зависимостей

Зависимости обновляются небольшими согласованными группами: .NET/EF Core, Aspire, телеметрия, тестовый стек и frontend. После обновления обязательны restore/install, полная сборка, unit- и integration-тесты, а для frontend — format, lint, typecheck и production build.

EF CLI закреплен как локальный инструмент репозитория в `.config/dotnet-tools.json`. После клонирования или смены версии выполните `dotnet tool restore`; команды `dotnet ef ...` из корня репозитория затем используют согласованную с runtime версию.

Проверка доступных стабильных версий:

```powershell
dotnet package list --project MenuMate.slnx --outdated
cd src/MenuMate.Web
pnpm outdated
```

Централизованные версии NuGet находятся в `Directory.Packages.props`. Frontend-версии и lockfile находятся в `src/MenuMate.Web/package.json` и `pnpm-lock.yaml`.

Переход на новый major выполняется только когда совместимы его peer dependencies и инструменты. В текущем наборе TypeScript остается на ветке 5.9: объявленная registry версия 7.x пока не совместима с используемой стабильной веткой `typescript-eslint` 8.x. Это осознанное ограничение совместимости, а не пропущенное patch-обновление.

AppHost пока явно использует пакетный DCP/dashboard (`AspireUseCliBundle=false`). Aspire 13.5 уже предлагает отдельный CLI bundle, но его включение меняет требования к рабочим станциям и CI: там должен быть установлен Aspire CLI. Перевод выполняется отдельной инфраструктурной задачей после подготовки CI; предупреждение `ASPIRE010` для принятого совместимого режима подавлено в проекте AppHost.
