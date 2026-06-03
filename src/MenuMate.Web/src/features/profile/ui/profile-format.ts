export function formatRole(role: string) {
  if (role === "Admin") {
    return "Администратор"
  }

  if (role === "User") {
    return "Пользователь"
  }

  return role
}

export function formatDateTime(value: string | null) {
  if (!value) {
    return "Не активен"
  }

  return new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(new Date(value))
}
