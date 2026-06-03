# 0010 Наблюдаемость и Serilog

## Статус

Принято как отложенное решение.

## Контекст

Aspire и `MenuMate.ServiceDefaults` уже дают базовую наблюдаемость: structured logging через стандартный `ILogger`, health checks, service discovery и OpenTelemetry. При этом в production может понадобиться более управляемая настройка логов, enrichers, sinks и удобная отправка в Seq или другой log storage.

## Решение

Serilog пока не добавляем как обязательную зависимость.

Добавить Serilog стоит, когда появятся:

- production-сценарии с централизованным сбором логов;
- требование писать логи в Seq, файлы или отдельный sink;
- необходимость enrichers для request id, user id, module name, tenant/family space id;
- отдельные правила аудита и troubleshooting.

До этого используем `ILogger` и не привязываем прикладной и доменный код к конкретному провайдеру логирования.

## Последствия

- Сейчас меньше инфраструктурной сложности.
- Переход на Serilog позже будет дешевым, если код продолжит зависеть только от `ILogger`.
- При добавлении Serilog нужно сделать это в composition root, не протаскивая Serilog API в модули.
